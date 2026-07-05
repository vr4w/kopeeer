namespace Kopeeer.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var startupRequest = StartupQueueRequest.TryParse(Environment.GetCommandLineArgs().Skip(1).ToArray());
        using var singleInstanceMutex = new Mutex(
            initiallyOwned: true,
            name: SingleInstanceCoordinator.MutexName,
            createdNew: out var isFirstInstance);

        if (!isFirstInstance)
        {
            if (startupRequest is not null)
            {
                SingleInstanceCoordinator.TrySendToRunningInstance(startupRequest);
            }

            return;
        }

        using var form = new MainForm(startupRequest);
        using var coordinator = SingleInstanceCoordinator.Start(form.ApplyExternalQueueRequestAsync);
        Application.Run(form);
    }
}
