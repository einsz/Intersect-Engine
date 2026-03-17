using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.GameObjects;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmTime : Form
{
    private DaylightCycleDescriptor? mBackupTime;
    private DaylightCycleDescriptor? mYTime;

    protected ListBox? lstTimes;
    protected DropDown? cmbIntervals;
    protected CheckBox? chkSync;
    protected TextBox? txtTimeRate;
    protected Slider? scrlAlpha;
    protected ColorPicker? clrSelector;
    protected Panel? pnlColor;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected GroupBox? grpSettings;
    protected GroupBox? grpRangeOptions;
    protected Label? lblTimes;
    protected Label? lblIntervals;
    protected Label? lblRate;
    protected Label? lblRateSuffix;
    protected Label? lblRateDesc;
    protected Label? lblColorDesc;
    protected Label? lblBrightness;

    public FrmTime()
    {
        Title = Strings.TimeEditor.title;
        MinimumSize = new Size(600, 500);
        Size = new Size(700, 600);
        BuildUI();
        InitLocalization();
    }

    private void BuildUI()
    {
        // Time intervals
        lstTimes = new ListBox();
        lblTimes = new Label { Text = Strings.TimeEditor.times };

        // Settings
        cmbIntervals = new DropDown();
        for (var i = 0; i < Strings.TimeEditor.intervals.Count; i++)
        {
            cmbIntervals.Items.Add(Strings.TimeEditor.intervals[i]);
        }

        chkSync = new CheckBox { Text = Strings.TimeEditor.sync };
        txtTimeRate = new TextBox();
        lblRate = new Label { Text = Strings.TimeEditor.rate };
        lblRateSuffix = new Label { Text = Strings.TimeEditor.ratesuffix };
        lblRateDesc = new Label { Text = Strings.TimeEditor.ratedesc };

        grpSettings = new GroupBox
        {
            Text = Strings.TimeEditor.settings,
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Padding = new Padding(5),
                Items =
                {
                    new StackLayout { Orientation = Orientation.Horizontal, Spacing = 5, Items = { new Label { Text = Strings.TimeEditor.interval }, cmbIntervals } },
                    chkSync,
                    new StackLayout { Orientation = Orientation.Horizontal, Spacing = 5, Items = { lblRate, txtTimeRate, lblRateSuffix } },
                    lblRateDesc
                }
            }
        };

        // Color overlay
        scrlAlpha = new Slider { MinValue = 0, MaxValue = 255 };
        clrSelector = new ColorPicker();
        pnlColor = new Panel { BackgroundColor = Colors.Black, Size = new Size(100, 100) };
        lblColorDesc = new Label { Text = Strings.TimeEditor.colorpaneldesc };
        lblBrightness = new Label();

        grpRangeOptions = new GroupBox
        {
            Text = Strings.TimeEditor.overlay,
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Padding = new Padding(5),
                Items =
                {
                    lblColorDesc,
                    pnlColor,
                    new StackLayout { Orientation = Orientation.Horizontal, Spacing = 5, Items = { new Label { Text = "Alpha" }, scrlAlpha } },
                    lblBrightness,
                    clrSelector
                }
            }
        };

        // Buttons
        btnSave = new Button { Text = Strings.TimeEditor.save };
        btnCancel = new Button { Text = Strings.TimeEditor.cancel };

        // Main layout
        var leftPanel = new Panel
        {
            Content = new StackLayout
            {
                Padding = new Padding(5),
                Spacing = 5,
                Items = { lblTimes, lstTimes }
            }
        };

        var rightPanel = new Panel
        {
            Content = new StackLayout
            {
                Padding = new Padding(5),
                Spacing = 10,
                Items =
                {
                    grpSettings,
                    grpRangeOptions,
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items = { btnSave, btnCancel }
                    }
                }
            }
        };

        var mainSplitter = new Splitter
        {
            Orientation = Orientation.Horizontal,
            Position = 200,
            Panel1 = leftPanel,
            Panel2 = rightPanel
        };

        Content = mainSplitter;

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        if (cmbIntervals != null) cmbIntervals.SelectedIndexChanged += (s, e) =>
        {
            if (mYTime != null && cmbIntervals.SelectedIndex >= 0)
            {
                var newInterval = DaylightCycleDescriptor.GetTimeInterval(cmbIntervals.SelectedIndex);
                if (mYTime.RangeInterval != newInterval)
                {
                    mYTime.RangeInterval = newInterval;
                    UpdateList(newInterval);
                    mYTime.ResetColors();
                    grpRangeOptions!.Visible = false;
                }
            }
        };

        if (lstTimes != null) lstTimes.SelectedIndexChanged += (s, e) =>
        {
            if (mYTime != null && lstTimes.SelectedIndex >= 0 && lstTimes.SelectedIndex < mYTime.DaylightHues.Length)
            {
                grpRangeOptions!.Visible = true;
                var hue = mYTime.DaylightHues[lstTimes.SelectedIndex];
                pnlColor!.BackgroundColor = Eto.Drawing.Color.FromArgb(255, hue.R, hue.G, hue.B);
                scrlAlpha!.Value = hue.A;
                var brightness = (int)((255 - scrlAlpha.Value) / 255f * 100);
                // Update brightness label
            }
            else
            {
                grpRangeOptions!.Visible = false;
            }
        };

        if (chkSync != null) chkSync.CheckedChanged += (s, e) =>
        {
            if (mYTime != null)
            {
                mYTime.SyncTime = chkSync.Checked ?? false;
                txtTimeRate!.Enabled = !(chkSync.Checked ?? false);
            }
        };

        txtTimeRate!.TextChanged += (s, e) =>
        {
            if (mYTime != null && float.TryParse(txtTimeRate.Text, out var val))
            {
                mYTime.Rate = val;
            }
        };

        if (scrlAlpha != null) scrlAlpha.ValueChanged += (s, e) =>
        {
            if (mYTime != null && lstTimes!.SelectedIndex >= 0 && lstTimes.SelectedIndex < mYTime.DaylightHues.Length)
            {
                var brightness = (int)((255 - scrlAlpha.Value) / 255f * 100);
                // Text = brightness removed - update label directly
                mYTime.DaylightHues[lstTimes.SelectedIndex].A = (byte)scrlAlpha.Value;
                pnlColor!.Invalidate();
            }
        };

        clrSelector!.ValueChanged += (s, e) =>
        {
            if (mYTime != null && lstTimes!.SelectedIndex >= 0 && lstTimes.SelectedIndex < mYTime.DaylightHues.Length)
            {
                var color = clrSelector.Value;
                pnlColor!.BackgroundColor = color;
                mYTime.DaylightHues[lstTimes.SelectedIndex].R = (byte)(color.R * 255);
                mYTime.DaylightHues[lstTimes.SelectedIndex].G = (byte)(color.G * 255);
                mYTime.DaylightHues[lstTimes.SelectedIndex].B = (byte)(color.B * 255);
            }
        };

        pnlColor!.MouseDoubleClick += (s, e) =>
        {
            if (mYTime != null && lstTimes!.SelectedIndex >= 0 && lstTimes.SelectedIndex < mYTime.DaylightHues.Length)
            {
                var color = pnlColor.BackgroundColor;
                mYTime.DaylightHues[lstTimes.SelectedIndex].R = (byte)(color.R * 255);
                mYTime.DaylightHues[lstTimes.SelectedIndex].G = (byte)(color.G * 255);
                mYTime.DaylightHues[lstTimes.SelectedIndex].B = (byte)(color.B * 255);
            }
        };

        if (btnSave != null) btnSave.Click += (s, e) =>
        {
            if (mYTime != null)
            {
                PacketSender.SendSaveTime(mYTime.GetInstanceJson());
            }
            Close();
            Globals.CurrentEditor = -1;
        };

        if (btnCancel != null) btnCancel.Click += (s, e) =>
        {
            if (mYTime != null && mBackupTime != null)
            {
                mYTime.LoadFromJson(mBackupTime.GetInstanceJson());
            }
            Close();
            Globals.CurrentEditor = -1;
        };
    }

    private void InitLocalization()
    {
//        Text = Strings.TimeEditor.title;
        lblTimes!.Text = Strings.TimeEditor.times;
        grpSettings!.Text = Strings.TimeEditor.settings;
        lblIntervals!.Text = Strings.TimeEditor.interval;
        chkSync!.Text = Strings.TimeEditor.sync;
        lblRate!.Text = Strings.TimeEditor.rate;
        lblRateSuffix!.Text = Strings.TimeEditor.ratesuffix;
        lblRateDesc!.Text = Strings.TimeEditor.ratedesc;
        grpRangeOptions!.Text = Strings.TimeEditor.overlay;
        lblColorDesc!.Text = Strings.TimeEditor.colorpaneldesc;
        btnSave!.Text = Strings.TimeEditor.save;
        btnCancel!.Text = Strings.TimeEditor.cancel;
    }

    public void InitEditor(DaylightCycleDescriptor time)
    {
        mYTime = time;
        mBackupTime = new DaylightCycleDescriptor();
        mBackupTime.LoadFromJson(time.GetInstanceJson());

        chkSync!.Checked = mYTime.SyncTime;
        txtTimeRate!.Text = mYTime.Rate.ToString();
        txtTimeRate.Enabled = !mYTime.SyncTime;
        cmbIntervals!.SelectedIndex = DaylightCycleDescriptor.GetIntervalIndex(mYTime.RangeInterval);
        UpdateList(mYTime.RangeInterval);
    }

    private void UpdateList(int duration)
    {
        if (lstTimes == null) return;

        lstTimes.Items.Clear();
        var time = new DateTime(2000, 1, 1, 0, 0, 0);
        for (var i = 0; i < 1440; i += duration)
        {
            var addRange = time.ToString("h:mm:ss tt") + " " + Strings.TimeEditor.to + " ";
            time = time.AddMinutes(duration);
            addRange += time.ToString("h:mm:ss tt");
            lstTimes.Items.Add(addRange);
        }
    }
}
