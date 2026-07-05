namespace Kopeeer.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm(StartupQueueRequest.TryParse(Environment.GetCommandLineArgs().Skip(1).ToArray())));
    }
}
