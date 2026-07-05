using FileOperationQueue.App.Tray;
using FileOperationQueue.App.Commands;

namespace FileOperationQueue.App;

internal static class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        if (await CommandLineQueueHandler.TryHandleAsync(args))
        {
            return;
        }

        Application.Run(new QueueApplicationContext());
    }
}
