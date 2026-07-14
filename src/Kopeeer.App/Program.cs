namespace Kopeeer.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var startupRequest = StartupQueueRequest.TryParse(Environment.GetCommandLineArgs().Skip(1).ToArray());
        AppDiagnostics.Write(startupRequest is null
            ? "app process started without queue request"
            : $"app process started with queue request operation={startupRequest.OperationType} sources={startupRequest.SourcePaths.Length} target=\"{startupRequest.TargetFolder}\" pickTarget={startupRequest.PickTarget}");

        using var singleInstanceMutex = new Mutex(
            initiallyOwned: true,
            name: SingleInstanceCoordinator.MutexName,
            createdNew: out var isFirstInstance);

        if (!isFirstInstance)
        {
            AppDiagnostics.Write("running instance detected");
            if (startupRequest is not null)
            {
                var sent = SingleInstanceCoordinator.TrySendToRunningInstance(startupRequest);
                AppDiagnostics.Write(sent
                    ? "queue request forwarded to running instance"
                    : "queue request forwarding failed");
            }

            return;
        }

        AppDiagnostics.Write("starting primary app instance");
        using var form = new MainForm(startupRequest);
        using var coordinator = SingleInstanceCoordinator.Start(form.ApplyExternalQueueRequestAsync);
        Application.Run(form);
    }
}
