using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Animations;
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.GameObjects;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmProjectile : EditorForm
{
    private List<ProjectileDescriptor> mChanged = new();
    private string? mCopiedItem;
    private ProjectileDescriptor? mEditorItem;
    private List<string> mKnownFolders = new();

    protected ListBox? lstGameObjects;
    protected ListBox? lstAnimations;
    protected TextBox? txtName;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbAnimation;
    protected DropDown? cmbSpell;
    protected DropDown? cmbItem;
    protected NumericStepper? nudSpeed;
    protected NumericStepper? nudSpawn;
    protected NumericStepper? nudAmount;
    protected NumericStepper? nudRange;
    protected NumericStepper? nudKnockback;
    protected NumericStepper? nudConsume;
    protected Slider? scrlSpawnRange;
    protected CheckBox? chkRotation;
    protected CheckBox? chkIgnoreMapBlocks;
    protected CheckBox? chkIgnoreActiveResources;
    protected CheckBox? chkIgnoreInactiveResources;
    protected CheckBox? chkIgnoreZDimensionBlocks;
    protected CheckBox? chkPierce;
    protected CheckBox? chkGrappleOnMap;
    protected CheckBox? chkGrappleOnPlayer;
    protected CheckBox? chkGrappleOnNpc;
    protected CheckBox? chkGrappleOnResource;
    protected RadioButton? rdoBehaviorDefault;
    protected RadioButton? rdoBehaviorDirectShot;
    protected RadioButton? rdoBehaviorHoming;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnAdd;
    protected Button? btnRemove;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected GroupBox? grpProjectiles;
    protected GroupBox? grpProperties;
    protected GroupBox? grpSpawns;
    protected GroupBox? grpAnimations;
    protected GroupBox? grpCollisions;
    protected GroupBox? grpAmmo;
    protected GroupBox? grpGrappleOptions;
    protected GroupBox? grpTargettingOptions;
    protected Panel? pnlContainer;
    protected Panel? picSpawns;
    protected Label? lblName;
    protected Label? lblSpeed;
    protected Label? lblSpawn;
    protected Label? lblAmount;
    protected Label? lblRange;
    protected Label? lblKnockback;
    protected Label? lblSpell;
    protected Label? lblAnimation;
    protected Label? lblSpawnRange;
    protected Label? lblAmmoItem;
    protected Label? lblAmmoAmount;
    protected Label? lblFolder;

    public FrmProjectile()
    {
        ApplyHooks();
        BuildUI();
        InitializeForm();
    }

    private void BuildUI()
    {
        Title = Strings.ProjectileEditor.title;
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
        txtSearch = new TextBox { PlaceholderText = Strings.ProjectileEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.ProjectileEditor.folderlabel };

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
                new Button { Text = Strings.ProjectileEditor.New },
                new Button { Text = Strings.ProjectileEditor.delete },
                new Button { Text = Strings.ProjectileEditor.copy },
                new Button { Text = Strings.ProjectileEditor.paste },
                new Button { Text = Strings.ProjectileEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.ProjectileEditor.save };
        btnCancel = new Button { Text = Strings.ProjectileEditor.cancel };
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
        cmbAnimation = new DropDown();
        cmbSpell = new DropDown();
        cmbItem = new DropDown();

        nudSpeed = new NumericStepper { MinValue = 1 };
        nudSpawn = new NumericStepper { MinValue = 0 };
        nudAmount = new NumericStepper { MinValue = 1 };
        nudRange = new NumericStepper { MinValue = 1 };
        nudKnockback = new NumericStepper { MinValue = 0 };
        nudConsume = new NumericStepper { MinValue = 0 };

        scrlSpawnRange = new Slider { MinValue = 1 };

        chkRotation = new CheckBox { Text = Strings.ProjectileEditor.autorotate };
        chkIgnoreMapBlocks = new CheckBox { Text = Strings.ProjectileEditor.ignoreblocks };
        chkIgnoreActiveResources = new CheckBox { Text = Strings.ProjectileEditor.ignoreactiveresources };
        chkIgnoreInactiveResources = new CheckBox { Text = Strings.ProjectileEditor.ignoreinactiveresources };
        chkIgnoreZDimensionBlocks = new CheckBox { Text = Strings.ProjectileEditor.ignorezdimension };
        chkPierce = new CheckBox { Text = Strings.ProjectileEditor.piercetarget };
        chkGrappleOnMap = new CheckBox { Text = Strings.ProjectileEditor.GrappleOpts[GrappleOption.MapAttribute] };
        chkGrappleOnPlayer = new CheckBox { Text = Strings.ProjectileEditor.GrappleOpts[GrappleOption.Player] };
        chkGrappleOnNpc = new CheckBox { Text = Strings.ProjectileEditor.GrappleOpts[GrappleOption.NPC] };
        chkGrappleOnResource = new CheckBox { Text = Strings.ProjectileEditor.GrappleOpts[GrappleOption.Resource] };

        rdoBehaviorDefault = new RadioButton { Text = Strings.ProjectileEditor.BehaviorDefault };
        rdoBehaviorDirectShot = new RadioButton(rdoBehaviorDefault) { Text = Strings.ProjectileEditor.BehaviorDirectShot };
        rdoBehaviorHoming = new RadioButton(rdoBehaviorDefault) { Text = Strings.ProjectileEditor.BehaviorHoming };

        lstAnimations = new ListBox();
        picSpawns = new Panel();

        btnAdd = new Button { Text = Strings.ProjectileEditor.addanimation };
        btnRemove = new Button { Text = Strings.ProjectileEditor.removeanimation };

        grpProjectiles = new GroupBox { Text = Strings.ProjectileEditor.projectiles };
        grpProperties = new GroupBox { Text = Strings.ProjectileEditor.properties };
        grpSpawns = new GroupBox { Text = Strings.ProjectileEditor.spawns };
        grpAnimations = new GroupBox { Text = Strings.ProjectileEditor.animations };
        grpCollisions = new GroupBox { Text = Strings.ProjectileEditor.collisions };
        grpAmmo = new GroupBox { Text = Strings.ProjectileEditor.ammo };
        grpGrappleOptions = new GroupBox { Text = Strings.ProjectileEditor.GrappleOptionsTitle };
        grpTargettingOptions = new GroupBox { Text = Strings.ProjectileEditor.TargettingOptions };

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

        nudSpeed!.ValueChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Speed = (int)nudSpeed.Value;
            }
        };

        nudSpawn!.ValueChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Delay = (int)nudSpawn.Value;
            }
        };

        nudAmount!.ValueChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Quantity = (int)nudAmount.Value;
            }
        };

        nudRange!.ValueChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Range = (int)nudRange.Value;
            }
        };

        nudKnockback!.ValueChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Knockback = (int)nudKnockback.Value;
            }
        };

        if (chkIgnoreMapBlocks != null) chkIgnoreMapBlocks.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.IgnoreMapBlocks = chkIgnoreMapBlocks.Checked ?? false;
            }
        };

        if (chkPierce != null) chkPierce.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.PierceTarget = chkPierce.Checked ?? false;
            }
        };

        if (btnSave != null) btnSave.Click += (s, e) =>
        {
            foreach (var item in mChanged)
            {
                item.GrappleHookOptions.Sort();
                PacketSender.SendSaveObject(item);
                item.DeleteBackup();
            }
            Close();
            Globals.CurrentEditor = -1;
        };

        if (btnCancel != null) btnCancel.Click += (s, e) =>
        {
            foreach (var item in mChanged)
            {
                item.GrappleHookOptions.Sort();
                item.RestoreBackup();
                item.DeleteBackup();
            }
            Close();
            Globals.CurrentEditor = -1;
        };
    }

    private void InitializeForm()
    {
        cmbAnimation!.Items.Clear();
        cmbAnimation.Items.Add(Strings.General.None);
        foreach (var name in AnimationDescriptor.Names)
        {
            cmbAnimation.Items.Add(name);
        }

        cmbItem!.Items.Clear();
        cmbItem.Items.Add(Strings.General.None);
        foreach (var name in ItemDescriptor.Names)
        {
            cmbItem.Items.Add(name);
        }

        cmbSpell!.Items.Clear();
        cmbSpell.Items.Add(Strings.General.None);
        foreach (var name in SpellDescriptor.Names)
        {
            cmbSpell.Items.Add(name);
        }

        InitEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Projectile)
        {
            InitEditor();
            if (mEditorItem != null && !ProjectileDescriptor.Lookup.Values.Contains(mEditorItem))
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
            nudSpeed!.Value = mEditorItem.Speed;
            nudSpawn!.Value = mEditorItem.Delay;
            nudAmount!.Value = mEditorItem.Quantity;
            nudRange!.Value = mEditorItem.Range;
            cmbSpell!.SelectedIndex = SpellDescriptor.ListIndex(mEditorItem.SpellId) + 1;
            nudKnockback!.Value = mEditorItem.Knockback;
            chkIgnoreMapBlocks!.Checked = mEditorItem.IgnoreMapBlocks;
            chkIgnoreActiveResources!.Checked = mEditorItem.IgnoreActiveResources;
            chkIgnoreInactiveResources!.Checked = mEditorItem.IgnoreExhaustedResources;
            chkIgnoreZDimensionBlocks!.Checked = mEditorItem.IgnoreZDimension;
            chkPierce!.Checked = mEditorItem.PierceTarget;
            cmbItem!.SelectedIndex = ItemDescriptor.ListIndex(mEditorItem.AmmoItemId) + 1;
            nudConsume!.Value = mEditorItem.AmmoRequired;

            chkGrappleOnMap!.Checked = mEditorItem.GrappleHookOptions.Contains(GrappleOption.MapAttribute);
            chkGrappleOnPlayer!.Checked = mEditorItem.GrappleHookOptions.Contains(GrappleOption.Player);
            chkGrappleOnNpc!.Checked = mEditorItem.GrappleHookOptions.Contains(GrappleOption.NPC);
            chkGrappleOnResource!.Checked = mEditorItem.GrappleHookOptions.Contains(GrappleOption.Resource);

            if (!mEditorItem.HomingBehavior && !mEditorItem.DirectShotBehavior)
            {
                rdoBehaviorDefault!.Checked = true;
            }
            else if (mEditorItem.HomingBehavior)
            {
                rdoBehaviorHoming!.Checked = true;
            }
            else if (mEditorItem.DirectShotBehavior)
            {
                rdoBehaviorDirectShot!.Checked = true;
            }

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

        var hasItem = mEditorItem != null;
        UpdateEditorButtons(hasItem);
    }

    public void InitEditor()
    {
        var mFolders = new List<string>();
        foreach (var itm in ProjectileDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((ProjectileDescriptor)itm.Value).Folder) &&
                !mFolders.Contains(((ProjectileDescriptor)itm.Value).Folder))
            {
                mFolders.Add(((ProjectileDescriptor)itm.Value).Folder);
                if (!mKnownFolders.Contains(((ProjectileDescriptor)itm.Value).Folder))
                {
                    mKnownFolders.Add(((ProjectileDescriptor)itm.Value).Folder);
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
            var items = ProjectileDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var proj = (ProjectileDescriptor?)pair.Value;
                if (proj != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = proj.Name ?? "Deleted" });
                }
            }
        }
    }
}
