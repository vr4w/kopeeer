using FileOperationQueue.App.Branding;
using FileOperationQueue.App.Resources;
using FileOperationQueue.Core.Queue;

namespace FileOperationQueue.App.Ui;

public sealed class MainForm : Form
{
    private readonly OperationQueue _queue;
    private readonly ListView _jobList;
    private readonly Label _statusLabel;
    private readonly Button _refreshButton;
    private readonly Button _closeButton;

    public MainForm(OperationQueue queue)
    {
        _queue = queue;

        Text = ProductBranding.DisplayName;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 420);
        Size = new Size(1040, 520);

        _jobList = BuildJobList();
        _statusLabel = new Label
        {
            AutoSize = true,
            Text = UiText.WorkerNotConnected
        };
        _refreshButton = new Button
        {
            Text = UiText.Refresh,
            AutoSize = true
        };
        _closeButton = new Button
        {
            Text = UiText.CloseToTray,
            AutoSize = true
        };

        _refreshButton.Click += async (_, _) => await RefreshQueueAsync();
        _closeButton.Click += (_, _) => Hide();

        Controls.Add(BuildLayout());
        Shown += async (_, _) => await RefreshQueueAsync();
    }

    public async Task RefreshQueueAsync()
    {
        var snapshot = await _queue.SnapshotAsync();

        _jobList.BeginUpdate();
        _jobList.Items.Clear();
        _jobList.Groups.Clear();
        var activeGroup = new ListViewGroup(UiText.CurrentJob);
        var pendingGroup = new ListViewGroup(UiText.PendingJobs);
        _jobList.Groups.Add(activeGroup);
        _jobList.Groups.Add(pendingGroup);

        foreach (var job in snapshot.Jobs)
        {
            var item = JobListViewItemFactory.Create(job);
            item.Group = job.Status == FileOperationJobStatus.Active ? activeGroup : pendingGroup;
            _jobList.Items.Add(item);
        }

        _jobList.EndUpdate();
        _statusLabel.Text = snapshot.Jobs.Count == 0 ? UiText.QueueEmpty : UiText.QueueLoaded;
    }

    private TableLayoutPanel BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        var title = new Label
        {
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Text = ProductBranding.Tagline
        };

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };

        actions.Controls.Add(_closeButton);
        actions.Controls.Add(_refreshButton);
        actions.Controls.Add(_statusLabel);

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(_jobList, 0, 1);
        layout.Controls.Add(actions, 0, 2);

        return layout;
    }

    private static ListView BuildJobList()
    {
        var list = new ListView
        {
            Dock = DockStyle.Fill,
            FullRowSelect = true,
            GridLines = true,
            HideSelection = false,
            MultiSelect = false,
            ShowGroups = true,
            View = View.Details
        };

        list.Columns.Add(UiText.Created, 150);
        list.Columns.Add(UiText.Kind, 90);
        list.Columns.Add(UiText.Status, 100);
        list.Columns.Add(UiText.Sources, 320);
        list.Columns.Add(UiText.Destination, 260);
        list.Columns.Add(UiText.Error, 260);

        return list;
    }
}
