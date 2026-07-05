namespace FileOperationQueue.App.App;

public static class AppPaths
{
    private const string AppDataFolderName = "file-operation-queue";
    private const string QueueFileName = "queue.json";

    public static string AppDataDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppDataFolderName);

    public static string QueueFilePath =>
        Path.Combine(AppDataDirectory, QueueFileName);
}

