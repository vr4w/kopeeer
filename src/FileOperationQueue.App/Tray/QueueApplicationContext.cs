using FileOperationQueue.App.App;
using FileOperationQueue.App.Branding;
using FileOperationQueue.App.Resources;
using FileOperationQueue.App.Ui;
using FileOperationQueue.Core.Queue;

namespace FileOperationQueue.App.Tray;

public sealed class QueueApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly MainForm _mainForm;

    public QueueApplicationContext()
    {
        var queue = new OperationQueue(new JsonFileQueueStore(AppPaths.QueueFilePath));
        _mainForm = new MainForm(queue);
        _mainForm.FormClosed += (_, _) => ExitThread();

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = BuildContextMenu(),
            Icon = SystemIcons.Application,
            Text = ProductBranding.DisplayName,
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => ShowMainForm();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _mainForm.Dispose();
        }

        base.Dispose(disposing);
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(UiText.ShowQueue, null, (_, _) => ShowMainForm());
        menu.Items.Add(UiText.Refresh, null, async (_, _) => await _mainForm.RefreshQueueAsync());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(UiText.Exit, null, (_, _) => ExitThread());
        return menu;
    }

    private void ShowMainForm()
    {
        if (_mainForm.Visible)
        {
            _mainForm.Activate();
            return;
        }

        _mainForm.Show();
        _mainForm.Activate();
    }
}

