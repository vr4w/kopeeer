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
    private readonly DataGridView _queueGrid = new();

    public MainForm(StartupQueueRequest? startupQueueRequest = null)
    {
        Text = "Kopeeer 0.1.0-alpha";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 620);
        Size = new Size(1120, 720);

        _logger = new FileJobLogger(Path.Combine(Directory.GetCurrentDirectory(), "logs", "kopeeer.log"));
        _queueProcessor = new SequentialQueueProcessor(_queue, new FileOperationProcessor(), _logger);
        _logger.AppStarted();

        _bindingSource.DataSource = _jobs;

        Controls.Add(BuildLayout());
        ConfigureGrid();
        ApplyStartupRequest(startupQueueRequest);
        UpdateStatus($"Ready. Log: {_logger.LogFilePath}");
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildInputArea(), 0, 1);
        root.Controls.Add(_queueGrid, 0, 2);
        root.Controls.Add(_statusLabel, 0, 3);

        return root;
    }

    private static Control BuildHeader() =>
        new Label
        {
            AutoSize = true,
            Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold),
            Text = "A calm queue for Windows file operations."
        };

    private Control BuildInputArea()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

        _sourceTextBox.Dock = DockStyle.Fill;
        _targetTextBox.Dock = DockStyle.Fill;

        var sourceFileButton = new Button { Text = "Source file", Dock = DockStyle.Fill };
        var sourceFolderButton = new Button { Text = "Source folder", Dock = DockStyle.Fill };
        var targetButton = new Button { Text = "Target folder", Dock = DockStyle.Fill };

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

        layout.Controls.Add(new Label { Text = "Source", AutoSize = true }, 0, 0);
        layout.Controls.Add(_sourceTextBox, 1, 0);
        layout.Controls.Add(sourceFileButton, 2, 0);
        layout.Controls.Add(sourceFolderButton, 3, 0);

        layout.Controls.Add(new Label { Text = "Target", AutoSize = true }, 0, 1);
        layout.Controls.Add(_targetTextBox, 1, 1);
        layout.Controls.Add(targetButton, 2, 1);

        layout.Controls.Add(new Label { Text = "Operation", AutoSize = true }, 0, 2);
        layout.Controls.Add(_copyRadioButton, 1, 2);
        layout.Controls.Add(_moveRadioButton, 2, 2);

        layout.Controls.Add(_addButton, 2, 3);
        layout.Controls.Add(_startButton, 3, 3);

        return layout;
    }

    private void ConfigureGrid()
    {
        _queueGrid.Dock = DockStyle.Fill;
        _queueGrid.AutoGenerateColumns = false;
        _queueGrid.AllowUserToAddRows = false;
        _queueGrid.AllowUserToDeleteRows = false;
        _queueGrid.ReadOnly = true;
        _queueGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _queueGrid.DataSource = _bindingSource;

        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = nameof(QueueJob.Id), Width = 220 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Operation", DataPropertyName = nameof(QueueJob.OperationType), Width = 90 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(QueueJob.Status), Width = 90 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Source", DataPropertyName = nameof(QueueJob.SourcePath), Width = 260 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Target", DataPropertyName = nameof(QueueJob.TargetFolder), Width = 220 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Created", DataPropertyName = nameof(QueueJob.CreatedAt), Width = 150 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Started", DataPropertyName = nameof(QueueJob.StartedAt), Width = 150 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Completed", DataPropertyName = nameof(QueueJob.CompletedAt), Width = 150 });
        _queueGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Error", DataPropertyName = nameof(QueueJob.ErrorMessage), Width = 260 });
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
            var job = _queue.Add(_sourceTextBox.Text, _targetTextBox.Text, operationType);
            _jobs.Add(job);
            _logger.JobAdded(job);
            UpdateStatus("Job added.");
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
            UpdateStatus("Queue processing finished.");
        }
        finally
        {
            _addButton.Enabled = true;
            _startButton.Enabled = true;
            RefreshQueueGrid();
        }
    }

    private void RefreshQueueGrid()
    {
        if (InvokeRequired)
        {
            BeginInvoke(RefreshQueueGrid);
            return;
        }

        _bindingSource.ResetBindings(false);
    }

    private void UpdateStatus(string message) =>
        _statusLabel.Text = message;

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
