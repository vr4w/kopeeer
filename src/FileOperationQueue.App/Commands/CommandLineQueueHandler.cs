using FileOperationQueue.App.App;
using FileOperationQueue.App.Branding;
using FileOperationQueue.App.Resources;
using FileOperationQueue.Core.Queue;

namespace FileOperationQueue.App.Commands;

public static class CommandLineQueueHandler
{
    public static async Task<bool> TryHandleAsync(IReadOnlyList<string> args)
    {
        var request = QueueCommandRequest.TryParse(args);
        if (request is null)
        {
            return false;
        }

        using var folderDialog = new FolderBrowserDialog
        {
            Description = UiText.SelectDestination,
            UseDescriptionForTitle = true
        };

        if (folderDialog.ShowDialog() != DialogResult.OK)
        {
            return true;
        }

        var queue = new OperationQueue(new JsonFileQueueStore(AppPaths.QueueFilePath));
        var job = await queue.EnqueueAsync(
            request.Kind,
            request.Sources,
            folderDialog.SelectedPath);

        MessageBox.Show(
            string.Format(UiText.JobQueuedFormat, job.Sources.Count, job.DestinationDirectory),
            ProductBranding.DisplayName,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        return true;
    }
}

