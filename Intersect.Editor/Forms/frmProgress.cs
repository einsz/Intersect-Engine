using Eto.Drawing;
using Eto.Forms;
using Intersect.Editor.Core;
using Intersect.Editor.Localization;

namespace Intersect.Editor.Forms;

public partial class FrmProgress : Form
{
    private int mProgressVal;

    private bool mShouldClose;

    private bool mShowCancelBtn;

    private string mStatusText;

    private readonly Label lblStatus;

    private readonly ProgressBar progressBar;

    private readonly Button btnCancel;

    private readonly UITimer tmrUpdater;

    public FrmProgress()
    {
        lblStatus = new Label
        {
            Text = "Doing Work: 0%",
            TextColor = Colors.Gainsboro,
        };

        progressBar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = 100,
        };

        btnCancel = new Button
        {
            Text = "Cancel",
            Visible = false,
        };

        var layout = new DynamicLayout
        {
            Padding = new Padding(13, 13, 13, 13),
            DefaultSpacing = new Size(3, 3),
        };

        layout.AddRow(lblStatus);
        layout.AddRow(progressBar);
        layout.AddRow(null, btnCancel);

        Content = layout;

        WindowStyle = WindowStyle.Utility;
        Resizable = false;
        MinimumSize = new Size(284, 100);
        Size = new Size(300, 110);
        Title = "Doing Work";

        tmrUpdater = new UITimer
        {
            Interval = 0.05,
        };
        tmrUpdater.Elapsed += TmrUpdater_Tick;
        tmrUpdater.Start();

        InitLocalization();
    }

    private void InitLocalization()
    {
        btnCancel.Text = Strings.ProgressForm.cancel;
    }

    public void SetTitle(string title)
    {
        Title = title;
    }

    public void SetProgress(string label, int progress, bool showCancel)
    {
        mStatusText = label;
        if (progress < 0)
        {
            mProgressVal = 0;
        }
        else
        {
            mProgressVal = progress;
        }

        mShowCancelBtn = showCancel;
        UpdateUI();
    }

    public void NotifyClose()
    {
        mShouldClose = true;
    }

    private void TmrUpdater_Tick(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (!Visible)
        {
            return;
        }

        lblStatus.Text = mStatusText ?? string.Empty;
        progressBar.Value = Math.Min(100, Math.Max(0, mProgressVal));
        btnCancel.Visible = mShowCancelBtn;

        if (mShouldClose)
        {
            Close();
        }
    }
}
