using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Content;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Animations;
using Intersect.Framework.Core.GameObjects.Lighting;
using Intersect.GameObjects;
using Intersect.Utilities;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmAnimation : EditorForm
{
    private List<AnimationDescriptor> mChanged = new();
    private string? mCopiedItem;
    private AnimationDescriptor? mEditorItem;
    private List<string> mKnownFolders = new();

    protected ListBox? lstGameObjects;
    protected TextBox? txtName;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbSound;
    protected DropDown? cmbLowerGraphic;
    protected DropDown? cmbUpperGraphic;
    protected NumericStepper? nudLowerHorizontalFrames;
    protected NumericStepper? nudLowerVerticalFrames;
    protected NumericStepper? nudLowerFrameCount;
    protected NumericStepper? nudLowerFrameDuration;
    protected NumericStepper? nudLowerLoopCount;
    protected NumericStepper? nudUpperHorizontalFrames;
    protected NumericStepper? nudUpperVerticalFrames;
    protected NumericStepper? nudUpperFrameCount;
    protected NumericStepper? nudUpperFrameDuration;
    protected NumericStepper? nudUpperLoopCount;
    protected Slider? scrlLowerFrame;
    protected Slider? scrlUpperFrame;
    protected Slider? scrlDarkness;
    protected CheckBox? chkCompleteSoundPlayback;
    protected CheckBox? chkLoopSoundDuringPreview;
    protected CheckBox? chkDisableLowerRotations;
    protected CheckBox? chkDisableUpperRotations;
    protected CheckBox? chkRenderAbovePlayer;
    protected CheckBox? chkRenderBelowFringe;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnSwap;
    protected Button? btnPlayLower;
    protected Button? btnPlayUpper;
    protected Button? btnLowerClone;
    protected Button? btnUpperClone;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected GroupBox? grpAnimations;
    protected GroupBox? grpGeneral;
    protected GroupBox? grpLower;
    protected GroupBox? grpUpper;
    protected GroupBox? grpLowerPlayback;
    protected GroupBox? grpUpperPlayback;
    protected GroupBox? grpLowerFrameOpts;
    protected GroupBox? grpUpperFrameOpts;
    protected GroupBox? grpLowerExtraOptions;
    protected GroupBox? grpUpperExtraOptions;
    protected Panel? pnlContainer;
    protected Label? lblName;
    protected Label? lblSound;
    protected Label? lblLowerGraphic;
    protected Label? lblLowerHorizontalFrames;
    protected Label? lblLowerVerticalFrames;
    protected Label? lblLowerFrameCount;
    protected Label? lblLowerFrameDuration;
    protected Label? lblLowerLoopCount;
    protected Label? lblLowerFrame;
    protected Label? lblUpperGraphic;
    protected Label? lblUpperHorizontalFrames;
    protected Label? lblUpperVerticalFrames;
    protected Label? lblUpperFrameCount;
    protected Label? lblUpperFrameDuration;
    protected Label? lblUpperLoopCount;
    protected Label? lblUpperFrame;
    protected Label? labelDarkness;
    protected Label? lblFolder;

    public FrmAnimation()
    {
        try
        {
            Console.WriteLine("  FrmAnimation: ApplyHooks...");
            ApplyHooks();
            Console.WriteLine("  FrmAnimation: BuildUI...");
            BuildUI();
            Console.WriteLine("  FrmAnimation: InitializeForm...");
            InitializeForm();
            Console.WriteLine("  FrmAnimation: Done");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FrmAnimation constructor error: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    private void BuildUI()
    {
        Title = Strings.AnimationEditor.title;
        MinimumSize = new Size(1024, 768);

        var mainSplitter = new Splitter
        {
            Orientation = Orientation.Horizontal,
            Position = 250,
            Panel1 = BuildLeftPanel(),
            Panel2 = BuildRightPanel()
        };

        Content = mainSplitter;
    }

    private Panel BuildLeftPanel()
    {
        lstGameObjects = new ListBox();
        txtSearch = new TextBox { PlaceholderText = Strings.AnimationEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.AnimationEditor.folderlabel };

        var searchPanel = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Items = { txtSearch, btnClearSearch, btnAlphabetical }
        };

        var folderPanel = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Items = { lblFolder, cmbFolder, btnAddFolder }
        };

        return new Panel
        {
            Content = new StackLayout
            {
                Padding = new Padding(5),
                Spacing = 5,
                Items = { searchPanel, folderPanel, lstGameObjects }
            }
        };
    }

    private Panel BuildRightPanel()
    {
        pnlContainer = new Panel();
        BuildEditorControls();

        var topButtons = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Items =
            {
                new Button { Text = Strings.AnimationEditor.New },
                new Button { Text = Strings.AnimationEditor.delete },
                new Button { Text = Strings.AnimationEditor.copy },
                new Button { Text = Strings.AnimationEditor.paste },
                new Button { Text = Strings.AnimationEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.AnimationEditor.save };
        btnCancel = new Button { Text = Strings.AnimationEditor.cancel };
        _btnSave = btnSave;
        _btnCancel = btnCancel;

        var bottomButtons = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Items = { btnSave, btnCancel }
        };

        return new Panel
        {
            Content = new StackLayout
            {
                Padding = new Padding(5),
                Spacing = 5,
                Items = { topButtons, pnlContainer, bottomButtons }
            }
        };
    }

    private void BuildEditorControls()
    {
        txtName = new TextBox();
        cmbSound = new DropDown();
        cmbLowerGraphic = new DropDown();
        cmbUpperGraphic = new DropDown();

        nudLowerHorizontalFrames = new NumericStepper { MinValue = 1 };
        nudLowerVerticalFrames = new NumericStepper { MinValue = 1 };
        nudLowerFrameCount = new NumericStepper { MinValue = 1 };
        nudLowerFrameDuration = new NumericStepper { MinValue = 1 };
        nudLowerLoopCount = new NumericStepper { MinValue = 0 };
        nudUpperHorizontalFrames = new NumericStepper { MinValue = 1 };
        nudUpperVerticalFrames = new NumericStepper { MinValue = 1 };
        nudUpperFrameCount = new NumericStepper { MinValue = 1 };
        nudUpperFrameDuration = new NumericStepper { MinValue = 1 };
        nudUpperLoopCount = new NumericStepper { MinValue = 0 };

        scrlLowerFrame = new Slider { MinValue = 1 };
        scrlUpperFrame = new Slider { MinValue = 1 };
        scrlDarkness = new Slider { MinValue = 0, MaxValue = 100 };

        chkCompleteSoundPlayback = new CheckBox { Text = Strings.AnimationEditor.soundcomplete };
        chkLoopSoundDuringPreview = new CheckBox { Text = Strings.AnimationEditor.LoopSoundDuringPreview };
        chkDisableLowerRotations = new CheckBox { Text = Strings.AnimationEditor.DisableRotations };
        chkDisableUpperRotations = new CheckBox { Text = Strings.AnimationEditor.DisableRotations };
        chkRenderAbovePlayer = new CheckBox { Text = Strings.AnimationEditor.renderaboveplayer };
        chkRenderBelowFringe = new CheckBox { Text = Strings.AnimationEditor.renderbelowfringe };

        btnSwap = new Button { Text = Strings.AnimationEditor.swap };
        btnPlayLower = new Button { Text = Strings.AnimationEditor.Play };
        btnPlayUpper = new Button { Text = Strings.AnimationEditor.Play };
        btnLowerClone = new Button { Text = Strings.AnimationEditor.CloneFromPrevious };
        btnUpperClone = new Button { Text = Strings.AnimationEditor.CloneFromPrevious };

        grpAnimations = new GroupBox { Text = Strings.AnimationEditor.animations };
        grpGeneral = new GroupBox { Text = Strings.AnimationEditor.general };
        grpLower = new GroupBox { Text = Strings.AnimationEditor.lowergroup };
        grpUpper = new GroupBox { Text = Strings.AnimationEditor.uppergroup };
        grpLowerPlayback = new GroupBox { Text = Strings.AnimationEditor.Playback };
        grpUpperPlayback = new GroupBox { Text = Strings.AnimationEditor.Playback };
        grpLowerFrameOpts = new GroupBox { Text = Strings.AnimationEditor.FrameOptions };
        grpUpperFrameOpts = new GroupBox { Text = Strings.AnimationEditor.FrameOptions };
        grpLowerExtraOptions = new GroupBox { Text = Strings.AnimationEditor.extraoptions };
        grpUpperExtraOptions = new GroupBox { Text = Strings.AnimationEditor.extraoptions };

        pnlContainer = new Panel();

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        txtName!.TextChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Name = txtName.Text;
            }
        };

        if (btnSave != null) btnSave.Click += (s, e) =>
        {
            if (mChanged != null)
            {
                foreach (var item in mChanged)
                {
                    PacketSender.SendSaveObject(item);
                    item.DeleteBackup();
                }
            }
            Close();
            Globals.CurrentEditor = -1;
        };

        if (btnCancel != null) btnCancel.Click += (s, e) =>
        {
            if (mChanged != null)
            {
                foreach (var item in mChanged)
                {
                    item.RestoreBackup();
                    item.DeleteBackup();
                }
            }
            Close();
            Globals.CurrentEditor = -1;
        };
    }

    private void InitializeForm()
    {
        cmbSound?.Items.Clear();
        cmbSound?.Items.Add(Strings.General.None);
        if (GameContentManager.SmartSortedSoundNames != null)
        {
            foreach (var name in GameContentManager.SmartSortedSoundNames)
            {
                cmbSound?.Items.Add(name);
            }
        }

        var animTextures = GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Animation);

        cmbLowerGraphic?.Items.Clear();
        cmbLowerGraphic?.Items.Add(Strings.General.None);
        if (animTextures != null)
        {
            foreach (var name in animTextures)
            {
                cmbLowerGraphic?.Items.Add(name);
            }
        }

        cmbUpperGraphic?.Items.Clear();
        cmbUpperGraphic?.Items.Add(Strings.General.None);
        if (animTextures != null)
        {
            foreach (var name in animTextures)
            {
                cmbUpperGraphic?.Items.Add(name);
            }
        }

        InitEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Animation)
        {
            InitEditor();
            if (mEditorItem != null && !AnimationDescriptor.Lookup.Values.Contains(mEditorItem))
            {
                mEditorItem = null;
                UpdateEditor();
            }
        }
    }

    private void UpdateEditor()
    {
        if (mEditorItem != null)
        {
            pnlContainer!.Visible = true;

            txtName!.Text = mEditorItem.Name;
            // cmbFolder!.Text = mEditorItem.Folder; // DropDown doesn't have Text setter in Eto
            cmbSound!.SelectedIndex = cmbSound.Items.IndexOf(
                new ListItem { Text = TextUtils.NullToNone(mEditorItem.Sound) }
            );
            chkCompleteSoundPlayback!.Checked = mEditorItem.CompleteSound;

            cmbLowerGraphic!.SelectedIndex = cmbLowerGraphic.Items.IndexOf(
                new ListItem { Text = TextUtils.NullToNone(mEditorItem.Lower.Sprite) }
            );
            nudLowerHorizontalFrames!.Value = mEditorItem.Lower.XFrames;
            nudLowerVerticalFrames!.Value = mEditorItem.Lower.YFrames;
            nudLowerFrameCount!.Value = mEditorItem.Lower.FrameCount;
            nudLowerFrameDuration!.Value = mEditorItem.Lower.FrameSpeed;
            nudLowerLoopCount!.Value = mEditorItem.Lower.LoopCount;

            cmbUpperGraphic!.SelectedIndex = cmbUpperGraphic.Items.IndexOf(
                new ListItem { Text = TextUtils.NullToNone(mEditorItem.Upper.Sprite) }
            );
            nudUpperHorizontalFrames!.Value = mEditorItem.Upper.XFrames;
            nudUpperVerticalFrames!.Value = mEditorItem.Upper.YFrames;
            nudUpperFrameCount!.Value = mEditorItem.Upper.FrameCount;
            nudUpperFrameDuration!.Value = mEditorItem.Upper.FrameSpeed;
            nudUpperLoopCount!.Value = mEditorItem.Upper.LoopCount;

            chkDisableLowerRotations!.Checked = mEditorItem.Lower.DisableRotations;
            chkDisableUpperRotations!.Checked = mEditorItem.Upper.DisableRotations;
            chkRenderAbovePlayer!.Checked = mEditorItem.Lower.AlternateRenderLayer;
            chkRenderBelowFringe!.Checked = mEditorItem.Upper.AlternateRenderLayer;

            if (mChanged.IndexOf(mEditorItem) == -1)
            {
                mChanged.Add(mEditorItem);
                mEditorItem.MakeBackup();
            }
        }
        else
        {
            pnlContainer!.Visible = false;
        }

        UpdateEditorButtons(mEditorItem != null);
    }

    public void InitEditor()
    {
        var mFolders = new List<string>();
        foreach (var anim in AnimationDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((AnimationDescriptor)anim.Value).Folder) &&
                !mFolders.Contains(((AnimationDescriptor)anim.Value).Folder))
            {
                mFolders.Add(((AnimationDescriptor)anim.Value).Folder);
                if (!mKnownFolders.Contains(((AnimationDescriptor)anim.Value).Folder))
                {
                    mKnownFolders.Add(((AnimationDescriptor)anim.Value).Folder);
                }
            }
        }

        mFolders.Sort();
        mKnownFolders.Sort();
        cmbFolder!.Items.Clear();
        cmbFolder.Items.Add("");
        foreach (var folder in mKnownFolders)
        {
            cmbFolder.Items.Add(folder);
        }

        if (lstGameObjects != null)
        {
            lstGameObjects.Items.Clear();
            var items = AnimationDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var anim = (AnimationDescriptor?)pair.Value;
                if (anim != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = anim.Name ?? "Deleted" });
                }
            }
        }
    }
}
