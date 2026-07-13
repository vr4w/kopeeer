using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Kopeeer.Core;
using Kopeeer.Worker;

namespace Kopeeer.App;

public sealed class MainForm : Form
{
    private const int BorderRadius = 8;
    private const int WmNclButtonDown = 0x00A1;
    private const int HtCaption = 0x02;
    private const int CsDropShadow = 0x00020000;

    private static readonly bool IsDarkMode = DetectWindowsDarkMode();
    private static readonly Color WindowBackground = IsDarkMode ? Color.FromArgb(24, 24, 27) : Color.FromArgb(246, 247, 249);
    private static readonly Color PanelBackground = IsDarkMode ? Color.FromArgb(32, 33, 36) : Color.White;
    private static readonly Color TextPrimary = IsDarkMode ? Color.FromArgb(244, 244, 245) : Color.FromArgb(30, 34, 39);
    private static readonly Color TextMuted = IsDarkMode ? Color.FromArgb(161, 161, 170) : Color.FromArgb(104, 112, 122);
    private static readonly Color Accent = IsDarkMode ? Color.FromArgb(96, 165, 250) : Color.FromArgb(37, 99, 235);
    private static readonly Color Border = IsDarkMode ? Color.FromArgb(63, 63, 70) : Color.FromArgb(224, 228, 235);

    private readonly InMemoryJobQueue _queue = new();
    private readonly BindingList<QueueJob> _jobs = [];
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
    private readonly Button _cancelButton = new();
    private readonly Button _windowCloseButton = new();
    private readonly CheckBox _shutdownWhenDoneCheckBox = new();
    private readonly Label _statusLabel = new();
    private readonly Label _summaryLabel = new();
    private readonly Label _operationLabel = new();
    private readonly Label _currentFileLabel = new();
    private readonly Label _overallPercentLabel = new();
    private readonly Label _speedLabel = new();
    private readonly Label _bytesLabel = new();
    private readonly ThinProgressBar _overallProgressBar = new();
    private readonly FlowLayoutPanel _queueList = new();
    private readonly Panel _emptyState = new();
    private readonly Panel _manualPanel = new();
    private readonly RowStyle _queueRowStyle = new(SizeType.Absolute, 26);
    private CancellationTokenSource? _queueCancellationSource;
    private bool _closeAfterCancel;
    private bool _forceCloseRequested;

    public MainForm(StartupQueueRequest? startupQueueRequest = null)
    {
        Text = "Kopeeer";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        ClientSize = new Size(444, 198);
        BackColor = WindowBackground;
        Font = new Font("Segoe UI", 9F);
        Padding = new Padding(1);

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
            RowCount = 4,
            Padding = new Padding(10, 6, 10, 10),
            BackColor = WindowBackground
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(_queueRowStyle);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        root.Controls.Add(BuildWindowChrome(), 0, 0);
        root.Controls.Add(BuildTransferPanel(), 0, 1);
        root.Controls.Add(BuildQueueArea(), 0, 2);
        root.Controls.Add(BuildActions(), 0, 3);

        return root;
    }

    private Control BuildWindowChrome()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = WindowBackground,
            Cursor = Cursors.Default,
            Margin = new Padding(0, 0, 0, 2)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));

        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font(Font.FontFamily, 8.5F, FontStyle.Regular),
            ForeColor = TextPrimary,
            Text = "Kopeeer",
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Default
        }, 0, 0);

        _summaryLabel.Dock = DockStyle.Fill;
        _summaryLabel.ForeColor = TextMuted;
        _summaryLabel.TextAlign = ContentAlignment.MiddleRight;
        panel.Controls.Add(_summaryLabel, 1, 0);

        StyleWindowCloseButton();
        panel.Controls.Add(_windowCloseButton, 2, 0);

        panel.MouseDown += BeginWindowDrag;
        foreach (Control control in panel.Controls)
        {
            if (control != _windowCloseButton)
            {
                control.MouseDown += BeginWindowDrag;
            }
        }

        return panel;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var createParams = base.CreateParams;
            createParams.ClassStyle |= CsDropShadow;
            return createParams;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyRoundedRegion();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var borderPen = new Pen(Border);
        var borderBounds = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        using var path = CreateRoundedRectanglePath(borderBounds, BorderRadius);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.DrawPath(borderPen, path);
    }

    private void ApplyRoundedRegion()
    {
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        Region?.Dispose();
        using var path = CreateRoundedRectanglePath(new Rectangle(Point.Empty, ClientSize), BorderRadius);
        Region = new Region(path);
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void BeginWindowDrag(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        ReleaseCapture();
        SendMessage(Handle, WmNclButtonDown, HtCaption, 0);
    }

    private void StyleWindowCloseButton()
    {
        _windowCloseButton.Text = "×";
        _windowCloseButton.Dock = DockStyle.Fill;
        _windowCloseButton.Font = new Font(Font.FontFamily, 10F, FontStyle.Regular);
        _windowCloseButton.FlatStyle = FlatStyle.Flat;
        _windowCloseButton.FlatAppearance.BorderSize = 0;
        _windowCloseButton.BackColor = WindowBackground;
        _windowCloseButton.ForeColor = TextMuted;
        _windowCloseButton.Margin = new Padding(5, 0, 0, 2);
        _windowCloseButton.Cursor = Cursors.Hand;
        _windowCloseButton.MouseEnter += (_, _) => _windowCloseButton.ForeColor = TextPrimary;
        _windowCloseButton.MouseLeave += (_, _) => _windowCloseButton.ForeColor = TextMuted;
        _windowCloseButton.Click += (_, _) =>
        {
            if (_queueProcessor.IsRunning)
            {
                CancelQueue();
                return;
            }

            Close();
        };
    }

    private Control BuildActions()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = WindowBackground
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));

        _startButton.Visible = false;
        _clearCompletedButton.Visible = false;
        _manualButton.Visible = false;

        _shutdownWhenDoneCheckBox.Text = "Shut down when done";
        _shutdownWhenDoneCheckBox.AutoSize = true;
        _shutdownWhenDoneCheckBox.ForeColor = TextMuted;
        _shutdownWhenDoneCheckBox.Margin = new Padding(0, 7, 0, 0);
        _shutdownWhenDoneCheckBox.BackColor = WindowBackground;

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.ForeColor = TextMuted;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.AutoEllipsis = true;

        _startButton.Click += async (_, _) => await StartQueueAsync();
        _clearCompletedButton.Click += (_, _) => ClearCompleted();

        panel.Controls.Add(_statusLabel, 0, 0);
        panel.Controls.Add(_shutdownWhenDoneCheckBox, 1, 0);

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
        _addButton.Click += async (_, _) =>
        {
            AddManualJob();
            if (!_queueProcessor.IsRunning && _jobs.Any(job => job.Status == JobStatus.Pending))
            {
                await StartQueueAsync();
            }
        };

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
            Padding = new Padding(0, 2, 0, 2)
        };

        _queueList.Dock = DockStyle.Fill;
        _queueList.FlowDirection = FlowDirection.TopDown;
        _queueList.WrapContents = false;
        _queueList.AutoScroll = false;
        _queueList.BackColor = WindowBackground;

        _emptyState.Dock = DockStyle.Fill;
        _emptyState.BackColor = WindowBackground;
        _emptyState.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = TextMuted,
            Text = "",
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
            BackColor = WindowBackground,
            Padding = new Padding(0, 0, 0, 2),
            Margin = new Padding(0)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4,
            BackColor = WindowBackground
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));

        _operationLabel.Dock = DockStyle.Fill;
        _operationLabel.ForeColor = TextMuted;
        _operationLabel.Font = new Font(Font.FontFamily, 9F, FontStyle.Regular);
        _operationLabel.Text = "Ready";

        _currentFileLabel.Dock = DockStyle.Fill;
        _currentFileLabel.Font = new Font(Font.FontFamily, 10.5F, FontStyle.Regular);
        _currentFileLabel.ForeColor = TextPrimary;
        _currentFileLabel.AutoEllipsis = true;
        _currentFileLabel.Text = "No active transfer";

        _overallPercentLabel.Dock = DockStyle.Fill;
        _overallPercentLabel.Font = new Font(Font.FontFamily, 14F, FontStyle.Regular);
        _overallPercentLabel.ForeColor = Accent;
        _overallPercentLabel.TextAlign = ContentAlignment.MiddleRight;
        _overallPercentLabel.Text = "0%";

        _speedLabel.Dock = DockStyle.Fill;
        _speedLabel.ForeColor = TextMuted;
        _speedLabel.TextAlign = ContentAlignment.MiddleRight;
        _speedLabel.Text = "";

        _bytesLabel.Dock = DockStyle.Fill;
        _bytesLabel.ForeColor = TextMuted;
        _bytesLabel.TextAlign = ContentAlignment.MiddleLeft;
        _bytesLabel.AutoEllipsis = true;
        _bytesLabel.Text = "";

        _overallProgressBar.Dock = DockStyle.Fill;
        _overallProgressBar.BackColor = WindowBackground;
        _overallProgressBar.TrackColor = IsDarkMode ? Color.FromArgb(49, 50, 54) : Color.FromArgb(232, 235, 240);
        _overallProgressBar.BarColor = Accent;

        StyleDangerButton(_cancelButton, "Cancel");
        _cancelButton.Dock = DockStyle.Fill;
        _cancelButton.Margin = new Padding(8, 28, 0, 12);
        _cancelButton.Click += (_, _) =>
        {
            if (_queueProcessor.IsRunning)
            {
                CancelQueue();
                return;
            }

            Close();
        };

        layout.Controls.Add(_operationLabel, 0, 0);
        layout.SetColumnSpan(_operationLabel, 3);
        layout.Controls.Add(_currentFileLabel, 0, 1);
        layout.Controls.Add(_speedLabel, 1, 1);
        layout.Controls.Add(_overallPercentLabel, 2, 1);
        layout.Controls.Add(_cancelButton, 3, 0);
        layout.SetRowSpan(_cancelButton, 4);
        layout.Controls.Add(_overallProgressBar, 0, 2);
        layout.SetColumnSpan(_overallProgressBar, 3);
        layout.Controls.Add(_bytesLabel, 0, 3);
        layout.SetColumnSpan(_bytesLabel, 3);

        panel.Controls.Add(layout);

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

    private static void StyleDangerButton(Button button, string text)
    {
        button.Text = text;
        button.AutoSize = false;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = IsDarkMode ? Color.FromArgb(185, 54, 54) : Color.FromArgb(220, 38, 38);
        button.ForeColor = Color.White;
        button.Cursor = Cursors.Hand;
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
        _queueCancellationSource?.Dispose();
        _queueCancellationSource = new CancellationTokenSource();
        _closeAfterCancel = false;
        UpdateStatus("");

        try
        {
            await _queueProcessor.ProcessAllAsync(RefreshQueueList, _ => RefreshQueueList(), ResolveTargetConflict, _queueCancellationSource.Token);
            UpdateStatus(BuildCompletionMessage());
            if (_shutdownWhenDoneCheckBox.Checked && _jobs.Count > 0 && _jobs.All(IsFinished))
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
        catch (OperationCanceledException)
        {
            foreach (var job in _jobs.Where(job => job.Status is JobStatus.Running or JobStatus.Pending))
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = "Canceled";
                job.CompletedAt = DateTimeOffset.Now;
                job.BytesPerSecond = 0;
            }

            UpdateStatus("Canceled");
        }
        finally
        {
            _queueCancellationSource?.Dispose();
            _queueCancellationSource = null;
            RefreshActions();
            RefreshQueueList();

            if (_closeAfterCancel && !IsDisposed && !_forceCloseRequested)
            {
                BeginInvoke((MethodInvoker)CloseWindowAfterCancel);
            }
        }
    }

    private void CancelQueue()
    {
        _forceCloseRequested = true;
        _closeAfterCancel = true;
        _cancelButton.Enabled = false;
        UpdateStatus("Canceling...");
        _queueCancellationSource?.Cancel();
        CloseWindowAfterCancel();
    }

    private void CloseWindowAfterCancel()
    {
        if (IsDisposed)
        {
            return;
        }

        Hide();
        BeginInvoke((MethodInvoker)(() =>
        {
            Close();
            Application.ExitThread();
        }));
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _queueCancellationSource?.Cancel();
        base.OnFormClosing(e);
    }

    private void ClearCompleted()
    {
        foreach (var job in _jobs.Where(job => job.Status is JobStatus.Completed or JobStatus.Skipped).ToArray())
        {
            _jobs.Remove(job);
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

        var pendingJobs = _jobs.Where(job => job.Status == JobStatus.Pending).ToArray();
        _queueList.SuspendLayout();
        _queueList.Controls.Clear();
        foreach (var job in pendingJobs)
        {
            _queueList.Controls.Add(BuildQueueRow(job));
        }
        _queueList.ResumeLayout();

        _emptyState.Visible = pendingJobs.Length == 0;
        _queueList.Visible = pendingJobs.Length > 0;
        ResizeQueueArea(pendingJobs.Length);
        UpdateTransferPanel();
        RefreshActions();
    }

    private Control BuildQueueRow(QueueJob job)
    {
        var row = new TableLayoutPanel
        {
            Width = Math.Max(360, _queueList.ClientSize.Width - 20),
            Height = 22,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 1),
            BackColor = WindowBackground
        };

        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46));
        row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        row.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = TextPrimary,
            Font = new Font("Segoe UI", 8.25F),
            Text = DisplayName(job.SourcePath),
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        row.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = TextMuted,
            Font = new Font("Segoe UI", 8.25F),
            Text = FormatSourceSize(job.SourcePath),
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleRight
        }, 1, 0);

        row.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = TextPrimary,
            Font = new Font("Segoe UI", 8.25F),
            Text = job.OperationType.ToString(),
            TextAlign = ContentAlignment.MiddleRight
        }, 2, 0);

        return row;
    }

    private void ResizeQueueArea(int pendingCount)
    {
        const int baseClientHeight = 164;
        const int rowHeight = 23;
        const int maxVisibleRows = 5;

        var visibleRows = Math.Min(pendingCount, maxVisibleRows);
        var queueHeight = pendingCount == 0 ? 26 : 6 + visibleRows * rowHeight;
        _queueRowStyle.Height = queueHeight;
        _queueList.AutoScroll = pendingCount > maxVisibleRows;

        var targetHeight = baseClientHeight + queueHeight;
        if (ClientSize.Height != targetHeight)
        {
            ClientSize = new Size(ClientSize.Width, targetHeight);
        }
    }

    private void RefreshActions()
    {
        var hasSource = File.Exists(_sourceTextBox.Text) || Directory.Exists(_sourceTextBox.Text);
        var hasTarget = Directory.Exists(_targetTextBox.Text);
        var hasPending = _jobs.Any(job => job.Status == JobStatus.Pending);
        var isRunning = _queueProcessor.IsRunning;

        _addButton.Enabled = hasSource && hasTarget && !isRunning;
        _startButton.Enabled = hasPending && !isRunning;
        _clearCompletedButton.Enabled = _jobs.Any(job => job.Status is JobStatus.Completed or JobStatus.Skipped) && !isRunning;
        _cancelButton.Enabled = true;
        if (isRunning)
        {
            StyleDangerButton(_cancelButton, "Cancel");
        }
        else
        {
            StyleGhostButton(_cancelButton, "Close");
        }

        _summaryLabel.Text = BuildSummary();
    }

    private string BuildSummary()
    {
        if (_jobs.Count == 0)
        {
            return "Idle";
        }

        var activeIndex = GetActiveJobIndex();
        if (activeIndex >= 0)
        {
            return $"{activeIndex + 1} of {_jobs.Count}";
        }

        var completed = _jobs.Count(IsFinished);
        var failed = _jobs.Count(job => job.Status == JobStatus.Failed);
        var skipped = _jobs.Count(job => job.Status == JobStatus.Skipped);
        return failed > 0 || skipped > 0
            ? $"{completed} of {_jobs.Count}, {failed} failed, {skipped} skipped"
            : $"{completed} of {_jobs.Count}";
    }

    private string BuildCompletionMessage()
    {
        var failed = _jobs.Count(job => job.Status == JobStatus.Failed);
        var skipped = _jobs.Count(job => job.Status == JobStatus.Skipped);
        if (failed == 0 && skipped == 0)
        {
            return "Done";
        }

        if (failed == 0)
        {
            return $"{skipped} skipped";
        }

        if (skipped == 0)
        {
            return $"{failed} failed";
        }

        return $"{failed} failed, {skipped} skipped";
    }

    private void UpdateTransferPanel()
    {
        var running = _jobs.FirstOrDefault(job => job.Status == JobStatus.Running);
        var active = running ?? _jobs.FirstOrDefault(job => job.Status == JobStatus.Pending) ?? _jobs.LastOrDefault();
        var totalBytes = active?.TotalBytes ?? 0;
        var transferredBytes = active?.TransferredBytes ?? 0;
        var percent = totalBytes <= 0 ? 0 : (int)Math.Clamp(transferredBytes * 100 / totalBytes, 0, 100);
        var finished = _jobs.Count > 0 && _jobs.All(IsFinished);

        if (running is null && finished)
        {
            percent = 100;
        }

        _overallProgressBar.Value = percent;
        _overallPercentLabel.Text = $"{percent}%";
        _bytesLabel.Text = FormatTransferSize(transferredBytes, totalBytes);

        if (running is null)
        {
            var pending = _jobs.FirstOrDefault(job => job.Status == JobStatus.Pending);
            var failed = _jobs.Any(job => job.Status == JobStatus.Failed);
            var skipped = _jobs.Any(job => job.Status == JobStatus.Skipped);
            var lastFailed = _jobs.LastOrDefault(job => job.Status == JobStatus.Failed);
            var lastSkipped = _jobs.LastOrDefault(job => job.Status == JobStatus.Skipped);
            var completed = finished;

            _operationLabel.Text = _jobs.Count == 0
                ? "Ready"
                : completed
                    ? failed ? "Could not finish" : skipped ? "Finished with skipped files" : "Done"
                    : pending is not null
                        ? BuildTransferLabel(pending)
                        : "Ready";
            _currentFileLabel.Text = _jobs.Count == 0
                ? "No active transfer"
                : completed
                    ? failed && lastFailed is not null
                        ? BuildFailureMessage(lastFailed)
                        : skipped && lastSkipped is not null
                            ? BuildFailureMessage(lastSkipped)
                        : "Queue finished"
                    : pending is not null
                        ? DisplayName(pending.SourcePath)
                        : "No active transfer";
            _speedLabel.Text = "";
            _bytesLabel.Text = completed && totalBytes > 0
                ? FormatTransferSize(totalBytes, totalBytes)
                : FormatTransferSize(transferredBytes, totalBytes);
            return;
        }

        var currentName = !string.IsNullOrWhiteSpace(running.CurrentItem)
            ? running.CurrentItem
            : DisplayName(running.SourcePath);
        _operationLabel.Text = BuildTransferLabel(running);
        _currentFileLabel.Text = currentName;
        _speedLabel.Text = FormatSpeed(running.BytesPerSecond);
        _bytesLabel.Text = FormatTransferSize(running.TransferredBytes, running.TotalBytes);
    }

    private string BuildTransferLabel(QueueJob job)
    {
        var index = _jobs.IndexOf(job);
        var count = _jobs.Count;
        var position = index >= 0 && count > 0 ? $"{index + 1} of {count}" : "";
        var operation = job.OperationType.ToString();
        return string.IsNullOrWhiteSpace(position) ? operation : $"{operation} - {position}";
    }

    private int GetActiveJobIndex()
    {
        var running = _jobs.FirstOrDefault(job => job.Status == JobStatus.Running);
        if (running is not null)
        {
            return _jobs.IndexOf(running);
        }

        var pending = _jobs.FirstOrDefault(job => job.Status == JobStatus.Pending);
        return pending is null ? -1 : _jobs.IndexOf(pending);
    }

    private static string DisplayName(string path)
    {
        var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var name = Path.GetFileName(trimmed);
        return string.IsNullOrWhiteSpace(name) ? path : name;
    }

    private static string BuildFailureMessage(QueueJob job)
    {
        var fileName = DisplayName(job.SourcePath);
        if (job.Status == JobStatus.Skipped)
        {
            return $"{fileName} skipped";
        }

        if (job.ErrorMessage?.Contains("already exists", StringComparison.OrdinalIgnoreCase) == true)
        {
            return $"{fileName} already exists";
        }

        return string.IsNullOrWhiteSpace(job.ErrorMessage)
            ? $"{fileName} failed"
            : $"{fileName}: {job.ErrorMessage}";
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

    private static string FormatTransferSize(long transferredBytes, long totalBytes)
    {
        if (totalBytes <= 0)
        {
            return "";
        }

        return $"{FormatMegabytes(transferredBytes)} of {FormatMegabytes(totalBytes)}";
    }

    private static string FormatSourceSize(string sourcePath)
    {
        try
        {
            if (File.Exists(sourcePath))
            {
                return FormatMegabytes(new FileInfo(sourcePath).Length);
            }

            if (Directory.Exists(sourcePath))
            {
                long total = 0;
                foreach (var file in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
                {
                    total += new FileInfo(file).Length;
                }

                return FormatMegabytes(total);
            }
        }
        catch
        {
            return "";
        }

        return "";
    }

    private static string FormatMegabytes(long bytes)
    {
        var megabytes = bytes / 1024d / 1024d;
        return megabytes < 10
            ? $"{megabytes:0.0} MB"
            : $"{megabytes:0} MB";
    }

    private void UpdateStatus(string message)
    {
        _statusLabel.Text = message;
        _summaryLabel.Text = BuildSummary();
    }

    private TargetConflictResolution ResolveTargetConflict(QueueJob job, string targetPath)
    {
        if (InvokeRequired)
        {
            return (TargetConflictResolution)Invoke(() => ResolveTargetConflict(job, targetPath));
        }

        using var dialog = new TargetConflictDialog(DisplayName(job.SourcePath), targetPath);
        return dialog.ShowDialog(this) == DialogResult.OK
            ? dialog.Resolution
            : TargetConflictResolution.Cancel;
    }

    private static bool IsFinished(QueueJob job) =>
        job.Status is JobStatus.Completed or JobStatus.Failed or JobStatus.Skipped;

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

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int message, int wParam, int lParam);

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

    private sealed class TargetConflictDialog : Form
    {
        public TargetConflictResolution Resolution { get; private set; } = TargetConflictResolution.Cancel;

        public TargetConflictDialog(string sourceName, string targetPath)
        {
            Text = "Kopeeer";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(380, 142);
            BackColor = WindowBackground;
            Font = new Font("Segoe UI", 9F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(14),
                BackColor = WindowBackground
            };

            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            root.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Text = $"{sourceName} already exists.",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            root.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = TextMuted,
                Text = $"Target: {targetPath}",
                AutoEllipsis = true,
                TextAlign = ContentAlignment.TopLeft
            }, 0, 1);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = WindowBackground
            };

            var cancelButton = MakeDialogButton("Cancel queue", TargetConflictResolution.Cancel);
            var skipButton = MakeDialogButton("Skip", TargetConflictResolution.Skip);
            var renameButton = MakeDialogButton("Rename", TargetConflictResolution.Rename);

            buttons.Controls.Add(cancelButton);
            buttons.Controls.Add(skipButton);
            buttons.Controls.Add(renameButton);
            root.Controls.Add(buttons, 0, 2);
            Controls.Add(root);

            AcceptButton = renameButton;
            CancelButton = cancelButton;
        }

        private Button MakeDialogButton(string text, TargetConflictResolution resolution)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(text.Length > 8 ? 92 : 72, 26),
                Margin = new Padding(6, 4, 0, 0),
                FlatStyle = FlatStyle.System
            };

            button.Click += (_, _) =>
            {
                Resolution = resolution;
                DialogResult = DialogResult.OK;
                Close();
            };

            return button;
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
            Height = 12;
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
