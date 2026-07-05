using System.ComponentModel;
using System.Diagnostics;
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
    private readonly Button _clearCompletedButton = new();
    private readonly Button _openLogsButton = new();
    private readonly Button _manualButton = new();
    private readonly Label _statusLabel = new();
    private readonly Label _summaryLabel = new();
    private readonly DataGridView _queueGrid = new();
    private readonly GroupBox _manualGroup = new() { Text = "Manual test job" };
    private readonly RowStyle _manualRowStyle = new(SizeType.Absolute, 0);

    public MainForm(StartupQueueRequest? startupQueueRequest = null)
    {
        Text = "Kopeeer 0.2.0-alpha";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 540);
        Size = new Size(1080, 660);

        _logger = new FileJobLogger(Path.Combine(Directory.GetCurrentDirectory(), "logs", "kopeeer.log"));
        _queueProcessor = new SequentialQueueProcessor(_queue, new FileOperationProcessor(), _logger);
        _logger.AppStarted();

        _bindingSource.DataSource = _jobs;

        Controls.Add(BuildLayout());
        ConfigureGrid();
        WireValidationEvents();
        RefreshActions();
        UpdateStatus($"Ready. Log: {_logger.LogFilePath}");

        Shown += async (_, _) => await ApplyStartupRequestAsync(startupQueueRequest);
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(14)
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(_manualRowStyle);
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildActions(), 0, 1);
        root.Controls.Add(BuildManualInputArea(), 0, 2);
        root.Controls.Add(BuildQueueArea(), 0, 3);
        root.Controls.Add(BuildStatusBar(), 0, 4);

        return root;
    }

    private static Control BuildHeader()
    {
        var baseFont = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
        var titleFont = new Font(baseFont.FontFamily, 16, FontStyle.Bold);

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };

        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Font = titleFont,
            Text = "Kopeeer",
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "A calm queue for Windows file operations.",
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 1);

        return panel;
    }

    private Control BuildActions()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        _startButton.Text = "Start queue";
        _clearCompletedButton.Text = "Clear completed";
        _openLogsButton.Text = "Open logs";
        _manualButton.Text = "Add job manually...";

        _startButton.AutoSize = true;
        _clearCompletedButton.AutoSize = true;
        _openLogsButton.AutoSize = true;
        _manualButton.AutoSize = true;

        _startButton.Click += async (_, _) => await StartQueueAsync();
        _clearCompletedButton.Click += (_, _) => ClearCompleted();
        _openLogsButton.Click += (_, _) => OpenLogs();
        _manualButton.Click += (_, _) => ToggleManualPanel();

        panel.Controls.Add(_startButton);
        panel.Controls.Add(_clearCompletedButton);
        panel.Controls.Add(_openLogsButton);
        panel.Controls.Add(_manualButton);

        return panel;
    }

    private Control BuildManualInputArea()
    {
        _manualGroup.Dock = DockStyle.Fill;
        _manualGroup.Visible = false;

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
        _addButton.Dock = DockStyle.Fill;
        _addButton.Click += (_, _) => AddManualJob();

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
        layout.Controls.Add(_addButton, 3, 3);

        _manualGroup.Controls.Add(layout);
        return _manualGroup;
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
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Source", DataPropertyName = nameof(QueueJob.SourcePath), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 38 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Target", DataPropertyName = nameof(QueueJob.TargetFolder), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 28 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Created", DataPropertyName = nameof(QueueJob.CreatedAt), Width = 145 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Started", DataPropertyName = nameof(QueueJob.StartedAt), Width = 145 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Completed", DataPropertyName = nameof(QueueJob.CompletedAt), Width = 145 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Error", DataPropertyName = nameof(QueueJob.ErrorMessage), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 24 });
    }

    private void WireValidationEvents()
    {
        _sourceTextBox.TextChanged += (_, _) => RefreshActions();
        _targetTextBox.TextChanged += (_, _) => RefreshActions();
    }

    private void ToggleManualPanel()
    {
        _manualGroup.Visible = !_manualGroup.Visible;
        _manualRowStyle.Height = _manualGroup.Visible ? 190 : 0;
        _manualButton.Text = _manualGroup.Visible ? "Hide manual add" : "Add job manually...";
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

    private void AddManualJob()
    {
        var operationType = _copyRadioButton.Checked ? FileOperationType.Copy : FileOperationType.Move;
        AddJobs(operationType, [_sourceTextBox.Text], _targetTextBox.Text);
    }

    private void AddJobs(FileOperationType operationType, IEnumerable<string> sourcePaths, string targetFolder)
    {
        try
        {
            var added = 0;
            foreach (var sourcePath in sourcePaths)
            {
                var addedJob = _queue.Add(sourcePath, targetFolder, operationType);
                _jobs.Add(addedJob);
                _logger.JobAdded(addedJob);
                added++;
            }

            RefreshQueueGrid();
            UpdateStatus($"Added {added} {operationType} job(s). {_jobs.Count(queueJob => queueJob.Status == JobStatus.Pending)} pending.");
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

    private void ClearCompleted()
    {
        foreach (var job in _jobs.Where(job => job.Status == JobStatus.Completed).ToArray())
        {
            _jobs.Remove(job);
        }

        RefreshQueueGrid();
        UpdateStatus("Completed jobs cleared.");
    }

    private void OpenLogs()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_logger.LogFilePath) ?? ".");
        if (!File.Exists(_logger.LogFilePath))
        {
            File.WriteAllText(_logger.LogFilePath, string.Empty);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _logger.LogFilePath,
            UseShellExecute = true
        });
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
        var hasCompleted = _jobs.Any(job => job.Status == JobStatus.Completed);
        var isRunning = _queueProcessor.IsRunning;

        _addButton.Enabled = hasSource && hasTarget && !isRunning;
        _startButton.Enabled = hasPending && !isRunning;
        _clearCompletedButton.Enabled = hasCompleted && !isRunning;
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

    private async Task ApplyStartupRequestAsync(StartupQueueRequest? request)
    {
        if (request is null)
        {
            return;
        }

        var targetFolder = request.TargetFolder;
        if (request.PickTarget)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select target folder",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                UpdateStatus("Context menu request canceled.");
                return;
            }

            targetFolder = dialog.SelectedPath;
        }

        if (string.IsNullOrWhiteSpace(targetFolder))
        {
            UpdateStatus("No target folder selected.");
            return;
        }

        AddJobs(request.OperationType, request.SourcePaths, targetFolder);
        Activate();
        await Task.CompletedTask;
    }
}
