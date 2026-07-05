using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Win32;
using Kopeeer.Core;
using Kopeeer.Worker;

namespace Kopeeer.App;

public sealed class MainForm : Form
{
    private static readonly bool IsDarkMode = DetectWindowsDarkMode();
    private static readonly Color WindowBackground = IsDarkMode ? Color.FromArgb(24, 24, 27) : Color.FromArgb(246, 247, 249);
    private static readonly Color PanelBackground = IsDarkMode ? Color.FromArgb(32, 33, 36) : Color.White;
    private static readonly Color TextPrimary = IsDarkMode ? Color.FromArgb(244, 244, 245) : Color.FromArgb(30, 34, 39);
    private static readonly Color TextMuted = IsDarkMode ? Color.FromArgb(161, 161, 170) : Color.FromArgb(104, 112, 122);
    private static readonly Color Accent = IsDarkMode ? Color.FromArgb(96, 165, 250) : Color.FromArgb(37, 99, 235);
    private static readonly Color Border = IsDarkMode ? Color.FromArgb(63, 63, 70) : Color.FromArgb(224, 228, 235);

    private readonly InMemoryJobQueue _queue = new();
    private readonly BindingList<QueueJob> _jobs = [];
    private readonly Dictionary<Guid, JobRowControl> _jobRows = [];
    private readonly FileJobLogger _logger;
    private readonly SequentialQueueProcessor _queueProcessor;

    private readonly TextBox _sourceTextBox = new();
    private readonly TextBox _targetTextBox = new();
    private readonly RadioButton _copyRadioButton = new();
    private readonly RadioButton _moveRadioButton = new();
    private readonly Button _addButton = new();
    private readonly Button _startButton = new();
    private readonly Button _clearCompletedButton = new();
    private readonly Button _manualButton = new();
    private readonly CheckBox _shutdownWhenDoneCheckBox = new();
    private readonly Label _statusLabel = new();
    private readonly Label _summaryLabel = new();
    private readonly Label _operationLabel = new();
    private readonly Label _currentFileLabel = new();
    private readonly Label _overallPercentLabel = new();
    private readonly Label _speedLabel = new();
    private readonly ThinProgressBar _overallProgressBar = new();
    private readonly FlowLayoutPanel _queueList = new();
    private readonly Panel _emptyState = new();
    private readonly Panel _manualPanel = new();
    private readonly RowStyle _manualRowStyle = new(SizeType.Absolute, 0);

    public MainForm(StartupQueueRequest? startupQueueRequest = null)
    {
        Text = "Kopeeer";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(500, 280);
        Size = new Size(560, 330);
        BackColor = WindowBackground;
        Font = new Font("Segoe UI", 9F);

        _logger = new FileJobLogger(Path.Combine(Directory.GetCurrentDirectory(), "logs", "kopeeer.log"));
        _queueProcessor = new SequentialQueueProcessor(_queue, new FileOperationProcessor(), _logger);
        _logger.AppStarted();

        Controls.Add(BuildLayout());
        WireValidationEvents();
        RefreshActions();
        UpdateStatus("Ready");

        Shown += async (_, _) => await ApplyStartupRequestAsync(startupQueueRequest);
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(14),
            BackColor = WindowBackground
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.RowStyles.Add(_manualRowStyle);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildActions(), 0, 1);
        root.Controls.Add(BuildManualInputArea(), 0, 2);
        root.Controls.Add(BuildTransferPanel(), 0, 3);
        root.Controls.Add(BuildQueueArea(), 0, 4);
        root.Controls.Add(BuildStatusBar(), 0, 5);

        return root;
    }

    private Control BuildHeader()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = WindowBackground
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));

        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font(Font.FontFamily, 13F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Text = "Kopeeer",
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        _summaryLabel.Dock = DockStyle.Fill;
        _summaryLabel.ForeColor = TextMuted;
        _summaryLabel.TextAlign = ContentAlignment.MiddleRight;
        panel.Controls.Add(_summaryLabel, 1, 0);

        return panel;
    }

    private Control BuildActions()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = WindowBackground
        };

        StyleButton(_startButton, "Start");
        StyleButton(_clearCompletedButton, "Clear");
        StyleGhostButton(_manualButton, "Manual");

        _shutdownWhenDoneCheckBox.Text = "Shut down when done";
        _shutdownWhenDoneCheckBox.AutoSize = true;
        _shutdownWhenDoneCheckBox.ForeColor = TextMuted;
        _shutdownWhenDoneCheckBox.Margin = new Padding(8, 7, 0, 0);
        _shutdownWhenDoneCheckBox.BackColor = WindowBackground;

        _startButton.Click += async (_, _) => await StartQueueAsync();
        _clearCompletedButton.Click += (_, _) => ClearCompleted();
        _manualButton.Click += (_, _) => ToggleManualPanel();

        panel.Controls.Add(_startButton);
        panel.Controls.Add(_clearCompletedButton);
        panel.Controls.Add(_manualButton);
        panel.Controls.Add(_shutdownWhenDoneCheckBox);

        return panel;
    }

    private Control BuildManualInputArea()
    {
        _manualPanel.Dock = DockStyle.Fill;
        _manualPanel.Visible = false;
        _manualPanel.Padding = new Padding(12, 8, 12, 8);
        _manualPanel.BackColor = PanelBackground;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4,
            BackColor = PanelBackground
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        _sourceTextBox.Dock = DockStyle.Fill;
        _targetTextBox.Dock = DockStyle.Fill;
        _sourceTextBox.BorderStyle = BorderStyle.FixedSingle;
        _targetTextBox.BorderStyle = BorderStyle.FixedSingle;

        var sourceFileButton = MakeSmallButton("File");
        var sourceFolderButton = MakeSmallButton("Folder");
        var targetButton = MakeSmallButton("Browse");

        sourceFileButton.Click += (_, _) => SelectSourceFile();
        sourceFolderButton.Click += (_, _) => SelectSourceFolder();
        targetButton.Click += (_, _) => SelectTargetFolder();

        _copyRadioButton.Text = "Copy";
        _copyRadioButton.Checked = true;
        _moveRadioButton.Text = "Move";
        _copyRadioButton.ForeColor = TextPrimary;
        _moveRadioButton.ForeColor = TextPrimary;

        StyleButton(_addButton, "Add");
        _addButton.Dock = DockStyle.Right;
        _addButton.Click += (_, _) => AddManualJob();

        layout.Controls.Add(MakeInputLabel("From"), 0, 0);
        layout.Controls.Add(_sourceTextBox, 1, 0);
        layout.Controls.Add(sourceFileButton, 2, 0);
        layout.Controls.Add(sourceFolderButton, 3, 0);
        layout.Controls.Add(MakeInputLabel("To"), 0, 1);
        layout.Controls.Add(_targetTextBox, 1, 1);
        layout.Controls.Add(targetButton, 2, 1);
        layout.Controls.Add(MakeInputLabel("Mode"), 0, 2);
        layout.Controls.Add(_copyRadioButton, 1, 2);
        layout.Controls.Add(_moveRadioButton, 2, 2);
        layout.Controls.Add(_addButton, 3, 3);

        _manualPanel.Controls.Add(layout);
        return _manualPanel;
    }

    private Control BuildQueueArea()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = WindowBackground,
            Padding = new Padding(0, 8, 0, 0)
        };

        _queueList.Dock = DockStyle.Fill;
        _queueList.FlowDirection = FlowDirection.TopDown;
        _queueList.WrapContents = false;
        _queueList.AutoScroll = true;
        _queueList.BackColor = WindowBackground;

        _emptyState.Dock = DockStyle.Fill;
        _emptyState.BackColor = WindowBackground;
        _emptyState.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = TextMuted,
            Text = "Right-drag files onto a folder and choose Kopeeer.",
            TextAlign = ContentAlignment.MiddleCenter
        });

        panel.Controls.Add(_queueList);
        panel.Controls.Add(_emptyState);
        return panel;
    }

    private Control BuildTransferPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = PanelBackground,
            Padding = new Padding(12, 8, 12, 9),
            Margin = new Padding(0, 4, 0, 6)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 4,
            BackColor = PanelBackground
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 21));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 14));

        _operationLabel.Dock = DockStyle.Fill;
        _operationLabel.ForeColor = TextMuted;
        _operationLabel.Font = new Font(Font.FontFamily, 8.25F, FontStyle.Bold);
        _operationLabel.Text = "READY";

        _currentFileLabel.Dock = DockStyle.Fill;
        _currentFileLabel.Font = new Font(Font.FontFamily, 10.5F, FontStyle.Bold);
        _currentFileLabel.ForeColor = TextPrimary;
        _currentFileLabel.AutoEllipsis = true;
        _currentFileLabel.Text = "No active transfer";

        _overallPercentLabel.Dock = DockStyle.Fill;
        _overallPercentLabel.Font = new Font(Font.FontFamily, 15F, FontStyle.Bold);
        _overallPercentLabel.ForeColor = Accent;
        _overallPercentLabel.TextAlign = ContentAlignment.MiddleRight;
        _overallPercentLabel.Text = "0%";

        _speedLabel.Dock = DockStyle.Fill;
        _speedLabel.ForeColor = TextMuted;
        _speedLabel.TextAlign = ContentAlignment.MiddleRight;
        _speedLabel.Text = "";

        _overallProgressBar.Dock = DockStyle.Fill;
        _overallProgressBar.BackColor = PanelBackground;
        _overallProgressBar.TrackColor = IsDarkMode ? Color.FromArgb(49, 50, 54) : Color.FromArgb(232, 235, 240);
        _overallProgressBar.BarColor = Accent;

        layout.Controls.Add(_operationLabel, 0, 0);
        layout.SetColumnSpan(_operationLabel, 3);
        layout.Controls.Add(_currentFileLabel, 0, 1);
        layout.Controls.Add(_speedLabel, 1, 1);
        layout.Controls.Add(_overallPercentLabel, 2, 1);
        layout.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = TextMuted,
            Text = "Now",
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 2);
        layout.Controls.Add(_overallProgressBar, 0, 3);
        layout.SetColumnSpan(_overallProgressBar, 3);

        panel.Controls.Add(layout);
        panel.Paint += (_, args) =>
        {
            using var pen = new Pen(Border);
            args.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };

        return panel;
    }

    private Control BuildStatusBar()
    {
        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.ForeColor = TextMuted;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        return _statusLabel;
    }

    private static Label MakeInputLabel(string text) =>
        new()
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = TextMuted,
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft
        };

    private static Button MakeSmallButton(string text)
    {
        var button = new Button { Text = text, Dock = DockStyle.Fill };
        StyleGhostButton(button, text);
        return button;
    }

    private static void StyleButton(Button button, string text)
    {
        button.Text = text;
        button.AutoSize = false;
        button.Size = new Size(76, 28);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Accent;
        button.ForeColor = Color.White;
        button.Margin = new Padding(0, 2, 8, 2);
        button.Cursor = Cursors.Hand;
    }

    private static void StyleGhostButton(Button button, string text)
    {
        button.Text = text;
        button.AutoSize = false;
        button.Size = new Size(76, 28);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Border;
        button.BackColor = PanelBackground;
        button.ForeColor = TextPrimary;
        button.Margin = new Padding(0, 2, 8, 2);
        button.Cursor = Cursors.Hand;
    }

    private void WireValidationEvents()
    {
        _sourceTextBox.TextChanged += (_, _) => RefreshActions();
        _targetTextBox.TextChanged += (_, _) => RefreshActions();
    }

    private void ToggleManualPanel()
    {
        _manualPanel.Visible = !_manualPanel.Visible;
        _manualRowStyle.Height = _manualPanel.Visible ? 132 : 0;
        _manualButton.Text = _manualPanel.Visible ? "Hide" : "Manual";
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

            RefreshQueueList();
            UpdateStatus($"Added {added} {operationType.ToString().ToLowerInvariant()} job(s)");
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
        UpdateStatus("Processing...");

        try
        {
            await _queueProcessor.ProcessAllAsync(RefreshQueueList, _ => RefreshQueueList());
            UpdateStatus(BuildCompletionMessage());
            if (_shutdownWhenDoneCheckBox.Checked && _jobs.Count > 0 && _jobs.All(job => job.Status is JobStatus.Completed or JobStatus.Failed))
            {
                UpdateStatus("Done. Windows will shut down in 30 seconds.");
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown.exe",
                    Arguments = "/s /t 30 /c \"Kopeeer queue finished.\"",
                    UseShellExecute = false
                });
            }
        }
        finally
        {
            RefreshActions();
            RefreshQueueList();
        }
    }

    private void ClearCompleted()
    {
        foreach (var job in _jobs.Where(job => job.Status == JobStatus.Completed).ToArray())
        {
            _jobs.Remove(job);
            if (_jobRows.Remove(job.Id, out var row))
            {
                _queueList.Controls.Remove(row);
                row.Dispose();
            }
        }

        RefreshQueueList();
        UpdateStatus("Cleared completed jobs");
    }

    private void RefreshQueueList()
    {
        if (InvokeRequired)
        {
            BeginInvoke((MethodInvoker)RefreshQueueList);
            return;
        }

        foreach (var job in _jobs)
        {
            if (!_jobRows.TryGetValue(job.Id, out var row))
            {
                row = new JobRowControl();
                row.Width = Math.Max(420, _queueList.ClientSize.Width - 24);
                row.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                _jobRows[job.Id] = row;
                _queueList.Controls.Add(row);
            }

            row.UpdateJob(job);
        }

        foreach (Control control in _queueList.Controls)
        {
            control.Width = Math.Max(420, _queueList.ClientSize.Width - 24);
        }

        _emptyState.Visible = _jobs.Count == 0;
        _queueList.Visible = _jobs.Count > 0;
        UpdateTransferPanel();
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
            return "Idle";
        }

        var running = _jobs.Count(job => job.Status == JobStatus.Running);
        var pending = _jobs.Count(job => job.Status == JobStatus.Pending);
        var completed = _jobs.Count(job => job.Status == JobStatus.Completed);
        var failed = _jobs.Count(job => job.Status == JobStatus.Failed);
        return running > 0
            ? $"{running} running, {pending} queued"
            : failed > 0
                ? $"{completed} done, {failed} failed"
                : $"{completed} done, {pending} queued";
    }

    private string BuildCompletionMessage()
    {
        var failed = _jobs.Count(job => job.Status == JobStatus.Failed);
        return failed == 0 ? "Done" : $"Done with {failed} failed";
    }

    private void UpdateTransferPanel()
    {
        var running = _jobs.FirstOrDefault(job => job.Status == JobStatus.Running);
        var totalBytes = _jobs.Sum(job => job.TotalBytes);
        var transferredBytes = _jobs.Sum(job => job.TransferredBytes);
        var percent = totalBytes <= 0 ? 0 : (int)Math.Clamp(transferredBytes * 100 / totalBytes, 0, 100);

        _overallProgressBar.Value = percent;
        _overallPercentLabel.Text = $"{percent}%";

        if (running is null)
        {
            var pending = _jobs.FirstOrDefault(job => job.Status == JobStatus.Pending);
            var failed = _jobs.Any(job => job.Status == JobStatus.Failed);
            var completed = _jobs.Count > 0 && _jobs.All(job => job.Status is JobStatus.Completed or JobStatus.Failed);

            _operationLabel.Text = _jobs.Count == 0
                ? "READY"
                : completed
                    ? failed ? "DONE WITH ERRORS" : "DONE"
                    : pending is not null
                        ? pending.OperationType.ToString().ToUpperInvariant()
                        : "READY";
            _currentFileLabel.Text = _jobs.Count == 0
                ? "No active transfer"
                : completed
                    ? "Queue finished"
                    : pending is not null
                        ? DisplayName(pending.SourcePath)
                        : "No active transfer";
            _speedLabel.Text = "";
            return;
        }

        var currentName = !string.IsNullOrWhiteSpace(running.CurrentItem)
            ? running.CurrentItem
            : DisplayName(running.SourcePath);
        _operationLabel.Text = running.OperationType.ToString().ToUpperInvariant();
        _currentFileLabel.Text = currentName;
        _speedLabel.Text = FormatSpeed(running.BytesPerSecond);
    }

    private static string DisplayName(string path)
    {
        var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var name = Path.GetFileName(trimmed);
        return string.IsNullOrWhiteSpace(name) ? path : name;
    }

    private static string FormatSpeed(double bytesPerSecond)
    {
        if (bytesPerSecond <= 0)
        {
            return "";
        }

        string[] units = ["B/s", "KB/s", "MB/s", "GB/s"];
        var value = bytesPerSecond;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.0} {units[unit]}";
    }

    private void UpdateStatus(string message)
    {
        _statusLabel.Text = message;
        _summaryLabel.Text = BuildSummary();
    }

    private static bool DetectWindowsDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
        catch
        {
            return false;
        }
    }

    public Task ApplyExternalQueueRequestAsync(StartupQueueRequest request)
    {
        if (InvokeRequired)
        {
            BeginInvoke((MethodInvoker)(async () => await ApplyStartupRequestAsync(request, autoStart: true)));
            return Task.CompletedTask;
        }

        return ApplyStartupRequestAsync(request, autoStart: true);
    }

    private async Task ApplyStartupRequestAsync(StartupQueueRequest? request, bool autoStart = true)
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
                UpdateStatus("Canceled");
                return;
            }

            targetFolder = dialog.SelectedPath;
        }

        if (string.IsNullOrWhiteSpace(targetFolder))
        {
            UpdateStatus("No target folder selected");
            return;
        }

        AddJobs(request.OperationType, request.SourcePaths, targetFolder);
        WindowState = FormWindowState.Normal;
        Show();
        Activate();

        if (autoStart && !_queueProcessor.IsRunning && _jobs.Any(job => job.Status == JobStatus.Pending))
        {
            await StartQueueAsync();
        }
    }

    private sealed class JobRowControl : Panel
    {
        private readonly Label _nameLabel = new();
        private readonly Label _detailLabel = new();
        private readonly Label _statusLabel = new();
        private readonly ThinProgressBar _progressBar = new();

        public JobRowControl()
        {
            Height = 52;
            Margin = new Padding(0, 0, 0, 6);
            Padding = new Padding(10, 7, 10, 7);
            BackColor = PanelBackground;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = PanelBackground
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 19));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 17));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));

            _nameLabel.Dock = DockStyle.Fill;
            _nameLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _nameLabel.ForeColor = TextPrimary;
            _nameLabel.AutoEllipsis = true;

            _detailLabel.Dock = DockStyle.Fill;
            _detailLabel.ForeColor = TextMuted;
            _detailLabel.AutoEllipsis = true;

            _statusLabel.Dock = DockStyle.Fill;
            _statusLabel.ForeColor = TextMuted;
            _statusLabel.TextAlign = ContentAlignment.TopRight;

            _progressBar.Dock = DockStyle.Fill;
            _progressBar.TrackColor = IsDarkMode ? Color.FromArgb(49, 50, 54) : Color.FromArgb(232, 235, 240);
            _progressBar.BarColor = Accent;

            layout.Controls.Add(_nameLabel, 0, 0);
            layout.Controls.Add(_statusLabel, 1, 0);
            layout.Controls.Add(_detailLabel, 0, 1);
            layout.SetColumnSpan(_detailLabel, 2);
            layout.Controls.Add(_progressBar, 0, 2);
            layout.SetColumnSpan(_progressBar, 2);

            Controls.Add(layout);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(Border);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        public void UpdateJob(QueueJob job)
        {
            _nameLabel.Text = DisplayName(job.SourcePath);
            _detailLabel.Text = $"{job.OperationType} -> {DisplayName(job.TargetFolder)}";
            _progressBar.Value = Math.Clamp(job.ProgressPercent, 0, 100);

            _statusLabel.Text = job.Status switch
            {
                JobStatus.Running => $"{job.ProgressPercent}%",
                JobStatus.Completed => "Done",
                JobStatus.Failed => "Failed",
                _ => "Queued"
            };

            BackColor = job.Status switch
            {
                JobStatus.Failed => IsDarkMode ? Color.FromArgb(52, 34, 36) : Color.FromArgb(255, 245, 245),
                JobStatus.Completed => IsDarkMode ? Color.FromArgb(31, 46, 38) : Color.FromArgb(247, 252, 249),
                _ => PanelBackground
            };
        }
    }

    private sealed class ThinProgressBar : Control
    {
        private int _value;

        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Clamp(value, 0, 100);
                Invalidate();
            }
        }

        public Color TrackColor { get; set; }

        public Color BarColor { get; set; }

        public ThinProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Height = 8;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var trackBrush = new SolidBrush(TrackColor);
            using var barBrush = new SolidBrush(BarColor);
            e.Graphics.FillRectangle(trackBrush, ClientRectangle);
            var width = (int)Math.Round(ClientSize.Width * (Value / 100d));
            if (width > 0)
            {
                e.Graphics.FillRectangle(barBrush, 0, 0, width, ClientSize.Height);
            }
        }
    }
}
