using FileOperationQueue.Core.Queue;

namespace FileOperationQueue.App.Ui;

public static class JobListViewItemFactory
{
    public static ListViewItem Create(FileOperationJob job)
    {
        var item = new ListViewItem(job.CreatedAt.ToLocalTime().ToString("g"))
        {
            Tag = job
        };

        item.SubItems.Add(job.Kind.ToString());
        item.SubItems.Add(job.Status.ToString());
        item.SubItems.Add(string.Join("; ", job.Sources));
        item.SubItems.Add(job.DestinationDirectory);
        item.SubItems.Add(job.ErrorMessage ?? string.Empty);

        return item;
    }
}
