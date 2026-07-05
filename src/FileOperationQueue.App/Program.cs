using FileOperationQueue.App.Tray;

namespace FileOperationQueue.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new QueueApplicationContext());
    }
}

