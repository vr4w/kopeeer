using System.ComponentModel;
using Kopeeer.Core;
using Kopeeer.Worker;

namespace Kopeeer.App;

public sealed class MainForm : Form
{
    private readonly InMemoryJobQueue _queue = new();
    private readonly BindingList<QueueJob> _jobs = [];
    private readonly BindingSource _bindingSource = new();
    private readonly FileJobLogger _logger;
    private readonly SequentialQueueProcessor _queueProcessor;

    private readonly TextBox _sourceTextBox = new();
    private readonly TextBox _targetTextBox = new();
    private readonly RadioButton _copyRadioButton = new();
    private readonly RadioButton _moveRadioButton = new();
    private readonly Button _addButton = new();
    private readonly Button _startButton = new();
    private readonly Label _statusLabel = new();
    private readonly Label _summaryLabel = new();
    private readonly DataGridView _queueGrid = new();

    public MainForm(StartupQueueRequest? startupQueueRequest = null)
    {
        Text = "Kopeeer 0.1.0-alpha";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1040, 660);
        Size = new Size(1180, 760);

        _logger = new FileJobLogger(Path.Combine(Directory.GetCurrentDirectory(), "logs", "kopeeer.log"));
        _queueProcessor = new SequentialQueueProcessor(_queue, new FileOperationProcessor(), _logger);
        _logger.AppStarted();

        _bindingSource.DataSource = _jobs;

        Controls.Add(BuildLayout());
        ConfigureGrid();
        WireValidationEvents();
        ApplyStartupRequest(startupQueueRequest);
        RefreshActions();
        UpdateStatus($"Ready. Log: {_logger.LogFilePath}");
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(14)
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildInputArea(), 0, 1);
        root.Controls.Add(BuildQueueArea(), 0, 2);
        root.Controls.Add(BuildStatusBar(), 0, 3);

        return root;
    }

    private static Control BuildHeader()
    {
        var baseFont = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;

        return new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font(baseFont, FontStyle.Bold),
            Text = "A calm queue for Windows file operations.",
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private Control BuildInputArea()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Create queue job"
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4,
            Padding = new Padding(12)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        _sourceTextBox.Dock = DockStyle.Fill;
        _targetTextBox.Dock = DockStyle.Fill;
        _sourceTextBox.PlaceholderText = "Choose a file or folder to copy/move";
        _targetTextBox.PlaceholderText = "Choose the destination folder";

        var sourceFileButton = new Button { Text = "File...", Dock = DockStyle.Fill };
        var sourceFolderButton = new Button { Text = "Folder...", Dock = DockStyle.Fill };
        var targetButton = new Button { Text = "Browse...", Dock = DockStyle.Fill };

        sourceFileButton.Click += (_, _) => SelectSourceFile();
        sourceFolderButton.Click += (_, _) => SelectSourceFolder();
        targetButton.Click += (_, _) => SelectTargetFolder();

        _copyRadioButton.Text = "Copy";
        _copyRadioButton.Checked = true;
        _moveRadioButton.Text = "Move";

        _addButton.Text = "Add to queue";
        _startButton.Text = "Start queue";
        _addButton.Dock = DockStyle.Fill;
        _startButton.Dock = DockStyle.Fill;
        _addButton.Click += (_, _) => AddJob();
        _startButton.Click += async (_, _) => await StartQueueAsync();

        layout.Controls.Add(MakeInputLabel("Source"), 0, 0);
        layout.Controls.Add(_sourceTextBox, 1, 0);
        layout.Controls.Add(sourceFileButton, 2, 0);
        layout.Controls.Add(sourceFolderButton, 3, 0);

        layout.Controls.Add(MakeInputLabel("Target"), 0, 1);
        layout.Controls.Add(_targetTextBox, 1, 1);
        layout.Controls.Add(targetButton, 2, 1);

        layout.Controls.Add(MakeInputLabel("Operation"), 0, 2);
        layout.Controls.Add(_copyRadioButton, 1, 2);
        layout.Controls.Add(_moveRadioButton, 2, 2);

        layout.Controls.Add(_addButton, 2, 3);
        layout.Controls.Add(_startButton, 3, 3);

        group.Controls.Add(layout);
        return group;
    }

    private Control BuildQueueArea()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Queue"
        };

        group.Controls.Add(_queueGrid);
        return group;
    }

    private Control BuildStatusBar()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _summaryLabel.Dock = DockStyle.Fill;
        _summaryLabel.TextAlign = ContentAlignment.MiddleRight;

        panel.Controls.Add(_statusLabel, 0, 0);
        panel.Controls.Add(_summaryLabel, 1, 0);

        return panel;
    }

    private static Label MakeInputLabel(string text) =>
        new()
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft
        };

    private void ConfigureGrid()
    {
        _queueGrid.Dock = DockStyle.Fill;
        _queueGrid.AutoGenerateColumns = false;
        _queueGrid.AllowUserToAddRows = false;
        _queueGrid.AllowUserToDeleteRows = false;
        _queueGrid.ReadOnly = true;
        _queueGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _queueGrid.MultiSelect = false;
        _queueGrid.RowHeadersVisible = false;
        _queueGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
        _queueGrid.BackgroundColor = SystemColors.Window;
        _queueGrid.BorderStyle = BorderStyle.FixedSingle;
        _queueGrid.DataSource = _bindingSource;

        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Operation", DataPropertyName = nameof(QueueJob.OperationType), Width = 90 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(QueueJob.Status), Width = 90 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Source", DataPropertyName = nameof(QueueJob.SourcePath), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 34 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Target", DataPropertyName = nameof(QueueJob.TargetFolder), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 26 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Created", DataPropertyName = nameof(QueueJob.CreatedAt), Width = 145 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Started", DataPropertyName = nameof(QueueJob.StartedAt), Width = 145 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Completed", DataPropertyName = nameof(QueueJob.CompletedAt), Width = 145 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Error", DataPropertyName = nameof(QueueJob.ErrorMessage), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 20 });
    }

    private void WireValidationEvents()
    {
        _sourceTextBox.TextChanged += (_, _) => RefreshActions();
        _targetTextBox.TextChanged += (_, _) => RefreshActions();
    }

    private void SelectSourceFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select source file",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _sourceTextBox.Text = dialog.FileName;
        }
    }

    private void SelectSourceFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select source folder",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _sourceTextBox.Text = dialog.SelectedPath;
        }
    }

    private void SelectTargetFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select target folder",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _targetTextBox.Text = dialog.SelectedPath;
        }
    }

    private void AddJob()
    {
        try
        {
            var operationType = _copyRadioButton.Checked ? FileOperationType.Copy : FileOperationType.Move;
            var addedJob = _queue.Add(_sourceTextBox.Text, _targetTextBox.Text, operationType);
            _jobs.Add(addedJob);
            _logger.JobAdded(addedJob);
            RefreshQueueGrid();
            UpdateStatus($"Added {operationType} job. {_jobs.Count(queueJob => queueJob.Status == JobStatus.Pending)} pending.");
        }
        catch (Exception exception)
        {
            UpdateStatus(exception.Message);
            MessageBox.Show(this, exception.Message, "Kopeeer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task StartQueueAsync()
    {
        _addButton.Enabled = false;
        _startButton.Enabled = false;
        UpdateStatus("Processing queue...");

        try
        {
            await _queueProcessor.ProcessAllAsync(RefreshQueueGrid);
            UpdateStatus(BuildCompletionMessage());
        }
        finally
        {
            RefreshActions();
            RefreshQueueGrid();
        }
    }

    private void RefreshQueueGrid()
    {
        if (InvokeRequired)
        {
            BeginInvoke((MethodInvoker)RefreshQueueGrid);
            return;
        }

        _bindingSource.ResetBindings(false);
        RefreshActions();
    }

    private void RefreshActions()
    {
        var hasSource = File.Exists(_sourceTextBox.Text) || Directory.Exists(_sourceTextBox.Text);
        var hasTarget = Directory.Exists(_targetTextBox.Text);
        var hasPending = _jobs.Any(job => job.Status == JobStatus.Pending);
        var isRunning = _queueProcessor.IsRunning;

        _addButton.Enabled = hasSource && hasTarget && !isRunning;
        _startButton.Enabled = hasPending && !isRunning;
        _summaryLabel.Text = BuildSummary();
    }

    private string BuildSummary()
    {
        if (_jobs.Count == 0)
        {
            return "No jobs";
        }

        var pending = _jobs.Count(job => job.Status == JobStatus.Pending);
        var running = _jobs.Count(job => job.Status == JobStatus.Running);
        var completed = _jobs.Count(job => job.Status == JobStatus.Completed);
        var failed = _jobs.Count(job => job.Status == JobStatus.Failed);
        return $"Pending {pending} | Running {running} | Completed {completed} | Failed {failed}";
    }

    private string BuildCompletionMessage()
    {
        var failed = _jobs.Count(job => job.Status == JobStatus.Failed);
        return failed == 0
            ? "Queue finished successfully."
            : $"Queue finished with {failed} failed job(s).";
    }

    private void UpdateStatus(string message)
    {
        _statusLabel.Text = message;
        _summaryLabel.Text = BuildSummary();
    }

    private void ApplyStartupRequest(StartupQueueRequest? request)
    {
        if (request is null)
        {
            return;
        }

        _sourceTextBox.Text = request.SourcePath;
        _copyRadioButton.Checked = request.OperationType == FileOperationType.Copy;
        _moveRadioButton.Checked = request.OperationType == FileOperationType.Move;
        UpdateStatus("Source loaded from command line. Select a target folder and add the job.");
    }
}
