#include <windows.h>
#include <shlobj.h>
#include <shlwapi.h>
#include <shellapi.h>
#include <strsafe.h>

#include <string>
#include <vector>

namespace
{
constexpr wchar_t DropHandlerClassIdText[] = L"{6A8E31AF-39B4-4B10-88AD-8F2C1CFB95D4}";
constexpr wchar_t CopyHookClassIdText[] = L"{E9C3BBA3-CC7C-49E3-9A6D-0D76F0956602}";
constexpr wchar_t DragDropMenuClassIdText[] = L"{A9D60874-04A4-4962-8798-69D186A6E5E6}";
constexpr wchar_t AppPathValueName[] = L"AppPath";
constexpr DWORD MkAlt = 0x0020;

const CLSID DropHandlerClassId = { 0x6a8e31af, 0x39b4, 0x4b10, { 0x88, 0xad, 0x8f, 0x2c, 0x1c, 0xfb, 0x95, 0xd4 } };
const CLSID CopyHookClassId = { 0xe9c3bba3, 0xcc7c, 0x49e3, { 0x9a, 0x6d, 0x0d, 0x76, 0xf0, 0x95, 0x66, 0x02 } };
const CLSID DragDropMenuClassId = { 0xa9d60874, 0x04a4, 0x4962, { 0x87, 0x98, 0x69, 0xd1, 0x86, 0xa6, 0xe5, 0xe6 } };

HINSTANCE ModuleInstance = nullptr;
long ModuleReferences = 0;

void IncrementModuleReferences()
{
    InterlockedIncrement(&ModuleReferences);
}

void DecrementModuleReferences()
{
    InterlockedDecrement(&ModuleReferences);
}

std::wstring Quote(const std::wstring& value)
{
    std::wstring quoted = L"\"";
    auto backslashCount = 0;

    for (const auto ch : value)
    {
        if (ch == L'\\')
        {
            backslashCount++;
            continue;
        }

        if (ch == L'"')
        {
            quoted.append(backslashCount * 2 + 1, L'\\');
            quoted += ch;
        }
        else
        {
            quoted.append(backslashCount, L'\\');
            quoted += ch;
        }

        backslashCount = 0;
    }

    quoted.append(backslashCount * 2, L'\\');
    quoted += L"\"";
    return quoted;
}

std::wstring GetLogPath()
{
    wchar_t localAppData[MAX_PATH]{};
    if (SUCCEEDED(SHGetFolderPathW(nullptr, CSIDL_LOCAL_APPDATA, nullptr, SHGFP_TYPE_CURRENT, localAppData)))
    {
        std::wstring folder = localAppData;
        folder += L"\\Kopeeer";
        CreateDirectoryW(folder.c_str(), nullptr);
        return folder + L"\\shell-extension.log";
    }

    return L"kopeeer-shell-extension.log";
}

void Log(const std::wstring& message)
{
    const auto logPath = GetLogPath();
    HANDLE file = CreateFileW(
        logPath.c_str(),
        FILE_APPEND_DATA,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        nullptr,
        OPEN_ALWAYS,
        FILE_ATTRIBUTE_NORMAL,
        nullptr);

    if (file == INVALID_HANDLE_VALUE)
    {
        return;
    }

    SYSTEMTIME now{};
    GetLocalTime(&now);

    wchar_t prefix[96]{};
    StringCchPrintfW(
        prefix,
        ARRAYSIZE(prefix),
        L"%04u-%02u-%02u %02u:%02u:%02u ",
        now.wYear,
        now.wMonth,
        now.wDay,
        now.wHour,
        now.wMinute,
        now.wSecond);

    std::wstring line = prefix;
    line += message;
    line += L"\r\n";

    DWORD written = 0;
    WriteFile(file, line.c_str(), static_cast<DWORD>(line.size() * sizeof(wchar_t)), &written, nullptr);
    CloseHandle(file);
}

bool IsAltShiftPressed(DWORD keyState)
{
    const bool fromDropState = (keyState & MK_SHIFT) == MK_SHIFT && (keyState & MkAlt) == MkAlt;
    const bool fromKeyboard = (GetKeyState(VK_SHIFT) & 0x8000) != 0 && (GetKeyState(VK_MENU) & 0x8000) != 0;
    return fromDropState || fromKeyboard;
}

std::wstring ReadRegisteredAppPath()
{
    wchar_t value[MAX_PATH * 4]{};
    DWORD valueSize = sizeof(value);

    std::wstring key = L"Software\\Classes\\CLSID\\";
    key += DropHandlerClassIdText;

    const auto result = RegGetValueW(
        HKEY_CURRENT_USER,
        key.c_str(),
        AppPathValueName,
        RRF_RT_REG_SZ,
        nullptr,
        value,
        &valueSize);

    if (result == ERROR_SUCCESS && value[0] != L'\0')
    {
        return value;
    }

    wchar_t envValue[MAX_PATH * 4]{};
    const auto envLength = GetEnvironmentVariableW(L"KOPEEER_APP_EXE", envValue, ARRAYSIZE(envValue));
    if (envLength > 0 && envLength < ARRAYSIZE(envValue))
    {
        return envValue;
    }

    return {};
}

std::vector<std::wstring> ExtractHDropPaths(IDataObject* dataObject)
{
    std::vector<std::wstring> paths;

    FORMATETC format{};
    format.cfFormat = CF_HDROP;
    format.dwAspect = DVASPECT_CONTENT;
    format.lindex = -1;
    format.tymed = TYMED_HGLOBAL;

    STGMEDIUM medium{};
    if (FAILED(dataObject->GetData(&format, &medium)))
    {
        Log(L"Drop did not contain CF_HDROP data.");
        return paths;
    }

    auto drop = static_cast<HDROP>(GlobalLock(medium.hGlobal));
    if (drop == nullptr)
    {
        ReleaseStgMedium(&medium);
        Log(L"Failed to lock CF_HDROP data.");
        return paths;
    }

    const auto count = DragQueryFileW(drop, 0xFFFFFFFF, nullptr, 0);
    for (UINT index = 0; index < count; index++)
    {
        const auto length = DragQueryFileW(drop, index, nullptr, 0);
        std::wstring path(length, L'\0');
        DragQueryFileW(drop, index, path.data(), length + 1);
        paths.push_back(path);
    }

    GlobalUnlock(medium.hGlobal);
    ReleaseStgMedium(&medium);
    return paths;
}

HRESULT LaunchKopeeer(const std::wstring& operation, const std::wstring& targetFolder, const std::vector<std::wstring>& sources)
{
    const auto appPath = ReadRegisteredAppPath();
    if (appPath.empty())
    {
        Log(L"No Kopeeer app path is registered.");
        return HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND);
    }

    if (targetFolder.empty())
    {
        Log(L"Target folder is empty.");
        return E_INVALIDARG;
    }

    if (sources.empty())
    {
        Log(L"No source paths were extracted.");
        return E_INVALIDARG;
    }

    std::wstring command = Quote(appPath);
    command += L" --enqueue --operation ";
    command += operation;
    command += L" --target ";
    command += Quote(targetFolder);
    command += L" --sources";

    for (const auto& source : sources)
    {
        command += L" ";
        command += Quote(source);
    }

    STARTUPINFOW startupInfo{};
    startupInfo.cb = sizeof(startupInfo);
    PROCESS_INFORMATION processInfo{};

    std::vector<wchar_t> mutableCommand(command.begin(), command.end());
    mutableCommand.push_back(L'\0');

    if (!CreateProcessW(
        nullptr,
        mutableCommand.data(),
        nullptr,
        nullptr,
        FALSE,
        0,
        nullptr,
        nullptr,
        &startupInfo,
        &processInfo))
    {
        const auto error = GetLastError();
        Log(L"CreateProcessW failed for Kopeeer.App.exe.");
        return HRESULT_FROM_WIN32(error);
    }

    CloseHandle(processInfo.hThread);
    CloseHandle(processInfo.hProcess);

    Log(L"Queued " + operation + L" into Kopeeer. Target: " + targetFolder + L", sources: " + std::to_wstring(sources.size()));
    return S_OK;
}

std::wstring ResolveTargetFolderFromCopyHookDestination(const wchar_t* destination)
{
    if (destination == nullptr || destination[0] == L'\0')
    {
        return {};
    }

    if (PathIsDirectoryW(destination))
    {
        return destination;
    }

    wchar_t folder[MAX_PATH * 4]{};
    StringCchCopyW(folder, ARRAYSIZE(folder), destination);
    if (PathRemoveFileSpecW(folder))
    {
        return folder;
    }

    return destination;
}

std::wstring PathFromPidl(LPCITEMIDLIST pidl)
{
    if (pidl == nullptr)
    {
        return {};
    }

    wchar_t path[MAX_PATH * 4]{};
    if (SHGetPathFromIDListW(pidl, path))
    {
        return path;
    }

    return {};
}

class DropHandler final : public IDropTarget, public IPersistFile
{
public:
    DropHandler()
    {
        IncrementModuleReferences();
        Log(L"DropHandler created.");
    }

    ~DropHandler()
    {
        Log(L"DropHandler destroyed.");
        DecrementModuleReferences();
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** object) override
    {
        if (object == nullptr)
        {
            return E_POINTER;
        }

        *object = nullptr;
        if (iid == IID_IUnknown || iid == IID_IDropTarget)
        {
            *object = static_cast<IDropTarget*>(this);
        }
        else if (iid == IID_IPersist || iid == IID_IPersistFile)
        {
            *object = static_cast<IPersistFile*>(this);
        }

        if (*object == nullptr)
        {
            return E_NOINTERFACE;
        }

        AddRef();
        return S_OK;
    }

    ULONG STDMETHODCALLTYPE AddRef() override
    {
        return static_cast<ULONG>(InterlockedIncrement(&references_));
    }

    ULONG STDMETHODCALLTYPE Release() override
    {
        const auto references = InterlockedDecrement(&references_);
        if (references == 0)
        {
            delete this;
        }

        return static_cast<ULONG>(references);
    }

    HRESULT STDMETHODCALLTYPE DragEnter(IDataObject*, DWORD keyState, POINTL, DWORD* effect) override
    {
        if (!IsAltShiftPressed(keyState))
        {
            if (effect != nullptr)
            {
                *effect = DROPEFFECT_NONE;
            }

            Log(L"DragEnter ignored because ALT+SHIFT is not pressed.");
            return E_NOTIMPL;
        }

        if (effect != nullptr)
        {
            *effect = DROPEFFECT_COPY;
        }

        Log(L"DragEnter accepted with ALT+SHIFT.");
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE DragOver(DWORD keyState, POINTL, DWORD* effect) override
    {
        if (!IsAltShiftPressed(keyState))
        {
            if (effect != nullptr)
            {
                *effect = DROPEFFECT_NONE;
            }

            return E_NOTIMPL;
        }

        if (effect != nullptr)
        {
            *effect = DROPEFFECT_COPY;
        }

        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE DragLeave() override
    {
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Drop(IDataObject* dataObject, DWORD keyState, POINTL, DWORD* effect) override
    {
        if (!IsAltShiftPressed(keyState))
        {
            if (effect != nullptr)
            {
                *effect = DROPEFFECT_NONE;
            }

            Log(L"Drop ignored because ALT+SHIFT is not pressed.");
            return E_NOTIMPL;
        }

        if (dataObject == nullptr)
        {
            return E_INVALIDARG;
        }

        const auto sources = ExtractHDropPaths(dataObject);
        const auto result = LaunchKopeeer(L"copy", targetPath_, sources);
        if (SUCCEEDED(result) && effect != nullptr)
        {
            *effect = DROPEFFECT_COPY;
        }

        return result;
    }

    HRESULT STDMETHODCALLTYPE GetClassID(CLSID* classId) override
    {
        if (classId == nullptr)
        {
            return E_POINTER;
        }

        *classId = DropHandlerClassId;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE IsDirty() override
    {
        return S_FALSE;
    }

    HRESULT STDMETHODCALLTYPE Load(LPCOLESTR fileName, DWORD) override
    {
        targetPath_ = fileName == nullptr ? L"" : fileName;
        Log(L"Target loaded: " + targetPath_);
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Save(LPCOLESTR, BOOL) override
    {
        return E_NOTIMPL;
    }

    HRESULT STDMETHODCALLTYPE SaveCompleted(LPCOLESTR) override
    {
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE GetCurFile(LPOLESTR* fileName) override
    {
        if (fileName == nullptr)
        {
            return E_POINTER;
        }

        const auto bytes = (targetPath_.size() + 1) * sizeof(wchar_t);
        *fileName = static_cast<LPOLESTR>(CoTaskMemAlloc(bytes));
        if (*fileName == nullptr)
        {
            return E_OUTOFMEMORY;
        }

        CopyMemory(*fileName, targetPath_.c_str(), bytes);
        return S_OK;
    }

private:
    long references_ = 1;
    std::wstring targetPath_;
};

class CopyHook final : public ICopyHookW
{
public:
    CopyHook()
    {
        IncrementModuleReferences();
        Log(L"CopyHook created.");
    }

    ~CopyHook()
    {
        Log(L"CopyHook destroyed.");
        DecrementModuleReferences();
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** object) override
    {
        if (object == nullptr)
        {
            return E_POINTER;
        }

        *object = nullptr;
        if (iid == IID_IUnknown || iid == IID_IShellCopyHookW)
        {
            *object = static_cast<ICopyHookW*>(this);
        }

        if (*object == nullptr)
        {
            return E_NOINTERFACE;
        }

        AddRef();
        return S_OK;
    }

    ULONG STDMETHODCALLTYPE AddRef() override
    {
        return static_cast<ULONG>(InterlockedIncrement(&references_));
    }

    ULONG STDMETHODCALLTYPE Release() override
    {
        const auto references = InterlockedDecrement(&references_);
        if (references == 0)
        {
            delete this;
        }

        return static_cast<ULONG>(references);
    }

    UINT STDMETHODCALLTYPE CopyCallback(
        HWND,
        UINT function,
        UINT,
        LPCWSTR sourceFile,
        DWORD,
        LPCWSTR destinationFile,
        DWORD) override
    {
        const bool supportedFunction = function == FO_COPY || function == FO_MOVE;
        const bool modifierPressed = IsAltShiftPressed(0);

        std::wstring message = L"CopyCallback function=" + std::to_wstring(function);
        message += L", modifier=";
        message += modifierPressed ? L"yes" : L"no";
        message += L", source=";
        message += sourceFile == nullptr ? L"" : sourceFile;
        message += L", destination=";
        message += destinationFile == nullptr ? L"" : destinationFile;
        Log(message);

        if (!supportedFunction || !modifierPressed)
        {
            return IDYES;
        }

        const std::wstring operation = function == FO_MOVE ? L"move" : L"copy";
        const std::wstring targetFolder = ResolveTargetFolderFromCopyHookDestination(destinationFile);
        std::vector<std::wstring> sources;
        if (sourceFile != nullptr && sourceFile[0] != L'\0')
        {
            sources.push_back(sourceFile);
        }

        const auto result = LaunchKopeeer(operation, targetFolder, sources);
        return SUCCEEDED(result) ? IDNO : IDYES;
    }

private:
    long references_ = 1;
};

class DragDropMenuHandler final : public IShellExtInit, public IContextMenu
{
public:
    DragDropMenuHandler()
    {
        IncrementModuleReferences();
        Log(L"DragDropMenuHandler created.");
    }

    ~DragDropMenuHandler()
    {
        Log(L"DragDropMenuHandler destroyed.");
        DecrementModuleReferences();
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** object) override
    {
        if (object == nullptr)
        {
            return E_POINTER;
        }

        *object = nullptr;
        if (iid == IID_IUnknown || iid == IID_IShellExtInit)
        {
            *object = static_cast<IShellExtInit*>(this);
        }
        else if (iid == IID_IContextMenu)
        {
            *object = static_cast<IContextMenu*>(this);
        }

        if (*object == nullptr)
        {
            return E_NOINTERFACE;
        }

        AddRef();
        return S_OK;
    }

    ULONG STDMETHODCALLTYPE AddRef() override
    {
        return static_cast<ULONG>(InterlockedIncrement(&references_));
    }

    ULONG STDMETHODCALLTYPE Release() override
    {
        const auto references = InterlockedDecrement(&references_);
        if (references == 0)
        {
            delete this;
        }

        return static_cast<ULONG>(references);
    }

    HRESULT STDMETHODCALLTYPE Initialize(LPCITEMIDLIST folderPidl, IDataObject* dataObject, HKEY) override
    {
        targetFolder_ = PathFromPidl(folderPidl);
        sourcePaths_.clear();

        if (dataObject != nullptr)
        {
            sourcePaths_ = ExtractHDropPaths(dataObject);
        }

        Log(L"DragDropMenu Initialize target=" + targetFolder_ + L", sources=" + std::to_wstring(sourcePaths_.size()));
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE QueryContextMenu(
        HMENU menu,
        UINT indexMenu,
        UINT idCommandFirst,
        UINT,
        UINT flags) override
    {
        if ((flags & CMF_DEFAULTONLY) == CMF_DEFAULTONLY)
        {
            return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 0);
        }

        if (targetFolder_.empty() || sourcePaths_.empty())
        {
            Log(L"DragDropMenu skipped because target or sources are empty.");
            return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 0);
        }

        MENUITEMINFOW copyItem{};
        copyItem.cbSize = sizeof(copyItem);
        copyItem.fMask = MIIM_ID | MIIM_STRING;
        copyItem.wID = idCommandFirst;
        copyItem.dwTypeData = const_cast<PWSTR>(L"Copy with Kopeeer");

        if (!InsertMenuItemW(menu, indexMenu, TRUE, &copyItem))
        {
            Log(L"InsertMenuItemW failed for Copy with Kopeeer.");
            return HRESULT_FROM_WIN32(GetLastError());
        }

        MENUITEMINFOW moveItem{};
        moveItem.cbSize = sizeof(moveItem);
        moveItem.fMask = MIIM_ID | MIIM_STRING;
        moveItem.wID = idCommandFirst + 1;
        moveItem.dwTypeData = const_cast<PWSTR>(L"Move with Kopeeer");

        if (!InsertMenuItemW(menu, indexMenu + 1, TRUE, &moveItem))
        {
            Log(L"InsertMenuItemW failed for Move with Kopeeer.");
            return HRESULT_FROM_WIN32(GetLastError());
        }

        Log(L"DragDropMenu inserted Copy/Move with Kopeeer.");
        return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 2);
    }

    HRESULT STDMETHODCALLTYPE InvokeCommand(LPCMINVOKECOMMANDINFO commandInfo) override
    {
        if (commandInfo == nullptr)
        {
            return E_INVALIDARG;
        }

        if (HIWORD(commandInfo->lpVerb) != 0 || LOWORD(commandInfo->lpVerb) > 1)
        {
            return E_FAIL;
        }

        const auto operation = LOWORD(commandInfo->lpVerb) == 1 ? L"move" : L"copy";
        Log(std::wstring(L"DragDropMenu command invoked: ") + operation);
        return LaunchKopeeer(operation, targetFolder_, sourcePaths_);
    }

    HRESULT STDMETHODCALLTYPE GetCommandString(UINT_PTR idCommand, UINT flags, UINT*, LPSTR name, UINT characterCount) override
    {
        if (idCommand > 1 || name == nullptr)
        {
            return E_INVALIDARG;
        }

        const auto isMove = idCommand == 1;

        if (flags == GCS_HELPTEXTW)
        {
            return StringCchCopyW(
                reinterpret_cast<PWSTR>(name),
                characterCount,
                isMove ? L"Queue this move operation in Kopeeer." : L"Queue this copy operation in Kopeeer.");
        }

        if (flags == GCS_VERBW)
        {
            return StringCchCopyW(reinterpret_cast<PWSTR>(name), characterCount, isMove ? L"KopeeerMove" : L"KopeeerCopy");
        }

        if (flags == GCS_HELPTEXTA)
        {
            return StringCchCopyA(name, characterCount, isMove ? "Queue this move operation in Kopeeer." : "Queue this copy operation in Kopeeer.");
        }

        if (flags == GCS_VERBA)
        {
            return StringCchCopyA(name, characterCount, isMove ? "KopeeerMove" : "KopeeerCopy");
        }

        return E_NOTIMPL;
    }

private:
    long references_ = 1;
    std::wstring targetFolder_;
    std::vector<std::wstring> sourcePaths_;
};

class ClassFactory final : public IClassFactory
{
public:
    explicit ClassFactory(CLSID classId) : classId_(classId)
    {
        IncrementModuleReferences();
    }

    ~ClassFactory()
    {
        DecrementModuleReferences();
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** object) override
    {
        if (object == nullptr)
        {
            return E_POINTER;
        }

        *object = nullptr;
        if (iid == IID_IUnknown || iid == IID_IClassFactory)
        {
            *object = static_cast<IClassFactory*>(this);
            AddRef();
            return S_OK;
        }

        return E_NOINTERFACE;
    }

    ULONG STDMETHODCALLTYPE AddRef() override
    {
        return static_cast<ULONG>(InterlockedIncrement(&references_));
    }

    ULONG STDMETHODCALLTYPE Release() override
    {
        const auto references = InterlockedDecrement(&references_);
        if (references == 0)
        {
            delete this;
        }

        return static_cast<ULONG>(references);
    }

    HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* outer, REFIID iid, void** object) override
    {
        if (outer != nullptr)
        {
            return CLASS_E_NOAGGREGATION;
        }

        IUnknown* instance = nullptr;
        if (classId_ == DropHandlerClassId)
        {
            instance = static_cast<IDropTarget*>(new (std::nothrow) DropHandler());
        }
        else if (classId_ == CopyHookClassId)
        {
            instance = static_cast<ICopyHookW*>(new (std::nothrow) CopyHook());
        }
        else if (classId_ == DragDropMenuClassId)
        {
            instance = static_cast<IShellExtInit*>(new (std::nothrow) DragDropMenuHandler());
        }

        if (instance == nullptr)
        {
            return E_OUTOFMEMORY;
        }

        const auto result = instance->QueryInterface(iid, object);
        instance->Release();
        return result;
    }

    HRESULT STDMETHODCALLTYPE LockServer(BOOL lock) override
    {
        if (lock)
        {
            IncrementModuleReferences();
        }
        else
        {
            DecrementModuleReferences();
        }

        return S_OK;
    }

private:
    long references_ = 1;
    CLSID classId_;
};
}

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        ModuleInstance = module;
        DisableThreadLibraryCalls(module);
    }

    return TRUE;
}

STDAPI DllGetClassObject(REFCLSID classId, REFIID iid, void** object)
{
    if (classId != DropHandlerClassId && classId != CopyHookClassId && classId != DragDropMenuClassId)
    {
        return CLASS_E_CLASSNOTAVAILABLE;
    }

    auto factory = new (std::nothrow) ClassFactory(classId);
    if (factory == nullptr)
    {
        return E_OUTOFMEMORY;
    }

    const auto result = factory->QueryInterface(iid, object);
    factory->Release();
    return result;
}

STDAPI DllCanUnloadNow()
{
    return ModuleReferences == 0 ? S_OK : S_FALSE;
}
