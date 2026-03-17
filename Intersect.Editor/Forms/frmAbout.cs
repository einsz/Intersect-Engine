using Eto.Drawing;
using Eto.Forms;
using Intersect.Editor.Core;
using Intersect.Editor.Localization;
using Intersect.Utilities;

namespace Intersect.Editor.Forms;

public partial class FrmAbout : Form
{
    private readonly ImageView picLogo;

    private readonly Label label1;

    private readonly LinkButton lblWebsite;

    private readonly Label lblVersion;

    public FrmAbout()
    {
        picLogo = new ImageView
        {
            Size = new Size(597, 210),
        };

        label1 = new Label
        {
            Text = "",
            TextColor = Colors.White,
            Wrap = WrapMode.Word,
            Size = new Size(597, 155),
        };

        lblWebsite = new LinkButton
        {
            Text = "",
            TextColor = Colors.White,
            Font = new Font(SystemFont.Bold),
        };
        lblWebsite.Click += LblWebsite_Click;

        lblVersion = new Label
        {
            Text = "",
            TextColor = Colors.White,
            TextAlignment = TextAlignment.Right,
        };

        var layout = new DynamicLayout
        {
            Padding = new Padding(13),
            DefaultSpacing = new Size(3, 3),
        };

        layout.AddRow(picLogo);
        layout.AddRow(label1);
        layout.AddRow(lblWebsite);
        layout.AddRow(lblVersion);

        Content = layout;

        Title = "About";
        WindowStyle = WindowStyle.Default;
        Resizable = false;
        MinimumSize = new Size(622, 430);
        Size = new Size(622, 430);
        Maximizable = false;

        Load += FrmAbout_Load;
    }

    private void FrmAbout_Load(object sender, EventArgs e)
    {
        InitLocalization();
    }

    private void InitLocalization()
    {
        this.Title = Strings.About.title;
        lblVersion.Text = Strings.About.version.ToString(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0");
        lblWebsite.Text = Strings.About.site;
    }

    private void LblWebsite_Click(object sender, EventArgs e)
    {
        BrowserUtils.Open("https://ascensiongamedev.com");
    }
}
