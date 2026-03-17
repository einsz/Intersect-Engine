using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using Intersect.Editor.Core;
using Intersect.Editor.Localization;

namespace Intersect.Editor.Forms;

public partial class FrmOptions : Form
{
    private readonly Button btnGeneralOptions;

    private readonly Button btnUpdateOptions;

    private readonly Panel pnlGeneral;

    private readonly Panel pnlUpdate;

    private readonly CheckBox chkSuppressTilesetWarning;

    private readonly CheckBox chkCursorSprites;

    private readonly GroupBox grpClientPath;

    private readonly Button btnBrowseClient;

    private readonly TextBox txtGamePath;

    private readonly CheckBox chkPackageAssets;

    private readonly GroupBox grpAssetPackingOptions;

    private readonly Label lblTextureSize;

    private readonly DropDown cmbTextureSize;

    private readonly Label lblMusicBatch;

    private readonly NumericStepper nudMusicBatch;

    private readonly Label lblSoundBatch;

    private readonly NumericStepper nudSoundBatch;

    private readonly Panel contentPanel;

    public FrmOptions()
    {
        // General tab controls
        chkSuppressTilesetWarning = new CheckBox
        {
            Text = "Suppress large tileset size warning.",
        };

        chkCursorSprites = new CheckBox
        {
            Text = "Enable cursor sprites for map tools",
        };

        btnBrowseClient = new Button
        {
            Text = "Browse",
        };
        btnBrowseClient.Click += BtnBrowseClient_Click;

        txtGamePath = new TextBox
        {
            ReadOnly = true,
            Enabled = false,
        };

        grpClientPath = new GroupBox
        {
            Text = "Client Path",
            Padding = new Padding(8),
        };

        var clientPathLayout = new DynamicLayout { DefaultSpacing = new Size(5, 5) };
        clientPathLayout.AddRow(btnBrowseClient, txtGamePath);
        grpClientPath.Content = clientPathLayout;

        // Update tab controls
        chkPackageAssets = new CheckBox
        {
            Text = "Package assets when generating updates",
        };

        lblTextureSize = new Label
        {
            Text = "Max Texture Pack Size (Resolution):",
            TextColor = Colors.White,
        };

        cmbTextureSize = new DropDown();
        cmbTextureSize.Items.Add(new ListItem { Text = "1" });
        cmbTextureSize.Items.Add(new ListItem { Text = "256" });
        cmbTextureSize.Items.Add(new ListItem { Text = "512" });
        cmbTextureSize.Items.Add(new ListItem { Text = "1024" });
        cmbTextureSize.Items.Add(new ListItem { Text = "2048" });
        cmbTextureSize.Items.Add(new ListItem { Text = "4096" });
        cmbTextureSize.Items.Add(new ListItem { Text = "8192" });

        lblMusicBatch = new Label
        {
            Text = "Max Music Pack Size (MB):",
            TextColor = Colors.White,
        };

        nudMusicBatch = new NumericStepper
        {
            MinValue = 1,
            MaxValue = 999999999,
            Value = 8,
        };

        lblSoundBatch = new Label
        {
            Text = "Max Sound Pack Size (MB):",
            TextColor = Colors.White,
        };

        nudSoundBatch = new NumericStepper
        {
            MinValue = 1,
            MaxValue = 999999999,
            Value = 8,
        };

        grpAssetPackingOptions = new GroupBox
        {
            Text = "Asset Packing Options",
            Padding = new Padding(8),
        };

        var assetPackingLayout = new DynamicLayout { DefaultSpacing = new Size(5, 5) };
        assetPackingLayout.AddRow(lblTextureSize, cmbTextureSize);
        assetPackingLayout.AddRow(lblMusicBatch, nudMusicBatch);
        assetPackingLayout.AddRow(lblSoundBatch, nudSoundBatch);
        grpAssetPackingOptions.Content = assetPackingLayout;

        // Tab buttons
        btnGeneralOptions = new Button
        {
            Text = "General",
        };
        btnGeneralOptions.Click += BtnGeneralOptions_Click;

        btnUpdateOptions = new Button
        {
            Text = "Update",
        };
        btnUpdateOptions.Click += BtnUpdateOptions_Click;

        // Panels
        pnlGeneral = new Panel();

        var generalLayout = new DynamicLayout { DefaultSpacing = new Size(5, 10) };
        generalLayout.AddRow(chkSuppressTilesetWarning);
        generalLayout.AddRow(chkCursorSprites);
        generalLayout.AddRow(grpClientPath);
        pnlGeneral.Content = generalLayout;

        pnlUpdate = new Panel();
        pnlUpdate.Visible = false;

        var updateLayout = new DynamicLayout { DefaultSpacing = new Size(5, 10) };
        updateLayout.AddRow(chkPackageAssets);
        updateLayout.AddRow(grpAssetPackingOptions);
        pnlUpdate.Content = updateLayout;

        // Content panel that holds the active panel
        contentPanel = new Panel();

        // Main layout
        var tabButtonRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Items =
            {
                btnGeneralOptions,
                btnUpdateOptions,
            },
        };

        var mainLayout = new DynamicLayout
        {
            Padding = new Padding(8),
            DefaultSpacing = new Size(5, 5),
        };

        mainLayout.AddRow(tabButtonRow);
        mainLayout.Add(contentPanel);

        Content = mainLayout;

        Title = "Options";
        WindowStyle = WindowStyle.Default;
        Resizable = false;
        MinimumSize = new Size(373, 220);
        Size = new Size(373, 220);
        Maximizable = false;
        Minimizable = false;

        Closing += FrmOptions_FormClosing;

        InitForm();
        InitLocalization();

        UpdateVisiblePanel();
    }

    private void InitForm()
    {
        var suppressTilesetWarning = Preferences.LoadPreference("SuppressTextureWarning");
        chkSuppressTilesetWarning.Checked = suppressTilesetWarning != "" && Convert.ToBoolean(suppressTilesetWarning);

        chkCursorSprites.Checked = Preferences.EnableCursorSprites;

        txtGamePath.Text = Preferences.LoadPreference("ClientPath");

        var packageUpdateAssets = Preferences.LoadPreference("PackageUpdateAssets");
        chkPackageAssets.Checked = packageUpdateAssets != "" && Convert.ToBoolean(packageUpdateAssets);

        var soundBatchSize = Preferences.LoadPreference("SoundPackSize");
        if (soundBatchSize != "")
        {
            nudSoundBatch.Value = Convert.ToInt32(soundBatchSize);
        }

        var musicBatchSize = Preferences.LoadPreference("MusicPackSize");
        if (musicBatchSize != "")
        {
            nudMusicBatch.Value = Convert.ToInt32(musicBatchSize);
        }

        var texturePackSize = Preferences.LoadPreference("TexturePackSize");
        if (texturePackSize != "")
        {
            foreach (var item in cmbTextureSize.Items)
            {
                if (item.Text == texturePackSize)
                {
                    cmbTextureSize.SelectedIndex = cmbTextureSize.Items.IndexOf(item);
                    break;
                }
            }
        }
        else
        {
            foreach (var item in cmbTextureSize.Items)
            {
                if (item.Text == "2048")
                {
                    cmbTextureSize.SelectedIndex = cmbTextureSize.Items.IndexOf(item);
                    break;
                }
            }
        }
    }

    private void InitLocalization()
    {
        Title = Strings.Options.title;
        btnGeneralOptions.Text = Strings.Options.generaltab.ToString(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0");
        chkSuppressTilesetWarning.Text = Strings.Options.tilesetwarning;
        chkCursorSprites.Text = Strings.Options.CursorSprites;
        grpClientPath.Text = Strings.Options.pathgroup;
        btnBrowseClient.Text = Strings.Options.browsebtn;
        btnUpdateOptions.Text = Strings.Options.UpdateTab;
        grpAssetPackingOptions.Text = Strings.Options.PackageOptions;
        lblMusicBatch.Text = Strings.Options.MusicPackSize;
        lblSoundBatch.Text = Strings.Options.SoundPackSize;
        lblTextureSize.Text = Strings.Options.TextureSize;
    }

    private void FrmOptions_FormClosing(object sender, EventArgs e)
    {
        Preferences.SavePreference("SuppressTextureWarning", chkSuppressTilesetWarning.Checked?.ToString() ?? "False");
        Preferences.EnableCursorSprites = chkCursorSprites.Checked ?? false;
        Preferences.SavePreference("ClientPath", txtGamePath.Text);
        Preferences.SavePreference("PackageUpdateAssets", chkPackageAssets.Checked?.ToString() ?? "False");
        Preferences.SavePreference("SoundPackSize", nudSoundBatch.Value.ToString(CultureInfo.InvariantCulture));
        Preferences.SavePreference("MusicPackSize", nudMusicBatch.Value.ToString(CultureInfo.InvariantCulture));
        Preferences.SavePreference("TexturePackSize", cmbTextureSize.SelectedKey ?? "2048");
    }

    private void BtnBrowseClient_Click(object sender, EventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = Strings.Options.dialogueheader,
            Filters =
            {
                new FileFilter("Executables", ".exe"),
                new FileFilter(Strings.Options.dialogueallfiles + " (*.*)", ".*"),
            },
        };

        var result = dialog.ShowDialog(this);
        if (result == DialogResult.Ok)
        {
            txtGamePath.Text = dialog.FileName;
        }
    }

    private void HidePanels()
    {
        pnlGeneral.Visible = false;
        pnlUpdate.Visible = false;
    }

    private void UpdateVisiblePanel()
    {
        contentPanel.Content = pnlGeneral.Visible ? pnlGeneral : pnlUpdate;
    }

    private void BtnGeneralOptions_Click(object sender, EventArgs e)
    {
        HidePanels();
        pnlGeneral.Visible = true;
        UpdateVisiblePanel();
    }

    private void BtnUpdateOptions_Click(object sender, EventArgs e)
    {
        HidePanels();
        pnlUpdate.Visible = true;
        UpdateVisiblePanel();
    }
}
