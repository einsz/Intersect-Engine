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
using Intersect.Framework.Core.GameObjects.Events;
using Intersect.Framework.Core.GameObjects.Maps.MapList;
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.GameObjects;
using Intersect.Utilities;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmSpell : EditorForm
{
    private List<SpellDescriptor> mChanged = new();
    private string? mCopiedItem;
    private SpellDescriptor? mEditorItem;
    private List<string> mKnownFolders = new();
    private List<string> mKnownCooldownGroups = new();

    protected ListBox? lstGameObjects;
    protected TextBox? txtName;
    protected TextBox? txtDesc;
    protected TextBox? txtCannotCast;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbType;
    protected DropDown? cmbSprite;
    protected DropDown? cmbCastAnimation;
    protected DropDown? cmbHitAnimation;
    protected DropDown? cmbTickAnimation;
    protected DropDown? cmbCastSprite;
    protected DropDown? cmbTargetType;
    protected DropDown? cmbDamageType;
    protected DropDown? cmbScalingStat;
    protected DropDown? cmbExtraEffect;
    protected DropDown? cmbTransform;
    protected DropDown? cmbProjectile;
    protected DropDown? cmbEvent;
    protected DropDown? cmbWarpMap;
    protected DropDown? cmbDirection;
    protected DropDown? cmbCooldownGroup;
    protected NumericStepper? nudHPCost;
    protected NumericStepper? nudMpCost;
    protected NumericStepper? nudCastDuration;
    protected NumericStepper? nudCooldownDuration;
    protected NumericStepper? nudHPDamage;
    protected NumericStepper? nudMPDamage;
    protected NumericStepper? nudStr;
    protected NumericStepper? nudMag;
    protected NumericStepper? nudDef;
    protected NumericStepper? nudMR;
    protected NumericStepper? nudSpd;
    protected NumericStepper? nudStrPercentage;
    protected NumericStepper? nudMagPercentage;
    protected NumericStepper? nudDefPercentage;
    protected NumericStepper? nudMRPercentage;
    protected NumericStepper? nudSpdPercentage;
    protected NumericStepper? nudScaling;
    protected NumericStepper? nudCritChance;
    protected NumericStepper? nudCritMultiplier;
    protected NumericStepper? nudCastRange;
    protected NumericStepper? nudHitRadius;
    protected NumericStepper? nudDuration;
    protected NumericStepper? nudBuffDuration;
    protected NumericStepper? nudTick;
    protected NumericStepper? nudWarpX;
    protected NumericStepper? nudWarpY;
    protected Slider? scrlRange;
    protected CheckBox? chkBound;
    protected CheckBox? chkFriendly;
    protected CheckBox? chkHOTDOT;
    protected CheckBox? chkIgnoreMapBlocks;
    protected CheckBox? chkIgnoreActiveResources;
    protected CheckBox? chkIgnoreInactiveResources;
    protected CheckBox? chkIgnoreZDimensionBlocks;
    protected CheckBox? chkIgnoreGlobalCooldown;
    protected CheckBox? chkIgnoreCdr;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnDynamicRequirements;
    protected Button? btnVisualMapSelector;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected Button? btnAddCooldownGroup;
    protected GroupBox? grpSpells;
    protected GroupBox? grpGeneral;
    protected GroupBox? grpSpellCost;
    protected GroupBox? grpTargetInfo;
    protected GroupBox? grpCombat;
    protected GroupBox? grpDamage;
    protected GroupBox? grpHotDot;
    protected GroupBox? grpStats;
    protected GroupBox? grpEffectDuration;
    protected GroupBox? grpEffect;
    protected GroupBox? grpDash;
    protected GroupBox? grpDashCollisions;
    protected GroupBox? grpWarp;
    protected GroupBox? grpEvent;
    protected GroupBox? grpRequirements;
    protected Panel? pnlContainer;
    protected Label? lblName;
    protected Label? lblType;
    protected Label? lblIcon;
    protected Label? lblDesc;
    protected Label? lblCastAnimation;
    protected Label? lblSpriteCastAnimation;
    protected Label? lblHitAnimation;
    protected Label? lblHPCost;
    protected Label? lblMPCost;
    protected Label? lblCastDuration;
    protected Label? lblCooldownDuration;
    protected Label? lblCooldownGroup;
    protected Label? lblTargetType;
    protected Label? lblCastRange;
    protected Label? lblProjectile;
    protected Label? lblHitRadius;
    protected Label? lblDuration;
    protected Label? lblCritChance;
    protected Label? lblCritMultiplier;
    protected Label? lblDamageType;
    protected Label? lblHPDamage;
    protected Label? lblManaDamage;
    protected Label? lblScalingStat;
    protected Label? lblScaling;
    protected Label? lblTick;
    protected Label? lblTickAnimation;
    protected Label? lblStr;
    protected Label? lblDef;
    protected Label? lblSpd;
    protected Label? lblMag;
    protected Label? lblMR;
    protected Label? lblBuffDuration;
    protected Label? lblEffect;
    protected Label? lblSprite;
    protected Label? lblRange;
    protected Label? lblMap;
    protected Label? lblX;
    protected Label? lblY;
    protected Label? lblWarpDir;
    protected Label? lblCannotCast;
    protected Label? lblFolder;
    protected Label? lblAnimation;

    public FrmSpell()
    {
        ApplyHooks();
        BuildUI();
        InitializeForm();
    }

    private void BuildUI()
    {
        Title = Strings.SpellEditor.title;
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
        txtSearch = new TextBox { PlaceholderText = Strings.SpellEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.SpellEditor.folderlabel };

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
                new Button { Text = Strings.SpellEditor.New },
                new Button { Text = Strings.SpellEditor.delete },
                new Button { Text = Strings.SpellEditor.copy },
                new Button { Text = Strings.SpellEditor.paste },
                new Button { Text = Strings.SpellEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.SpellEditor.save };
        btnCancel = new Button { Text = Strings.SpellEditor.cancel };
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
        txtDesc = new TextBox();
        txtCannotCast = new TextBox();
        cmbType = new DropDown();
        cmbSprite = new DropDown();
        cmbCastAnimation = new DropDown();
        cmbHitAnimation = new DropDown();
        cmbTickAnimation = new DropDown();
        cmbCastSprite = new DropDown();
        cmbTargetType = new DropDown();
        cmbDamageType = new DropDown();
        cmbScalingStat = new DropDown();
        cmbExtraEffect = new DropDown();
        cmbTransform = new DropDown();
        cmbProjectile = new DropDown();
        cmbEvent = new DropDown();
        cmbWarpMap = new DropDown();
        cmbDirection = new DropDown();
        cmbCooldownGroup = new DropDown();

        nudHPCost = new NumericStepper();
        nudMpCost = new NumericStepper();
        nudCastDuration = new NumericStepper { MinValue = 0, MaxValue = int.MaxValue };
        nudCooldownDuration = new NumericStepper { MinValue = 0, MaxValue = int.MaxValue };
        nudHPDamage = new NumericStepper();
        nudMPDamage = new NumericStepper();
        nudStr = new NumericStepper { MinValue = -Options.Instance.Player.MaxStat, MaxValue = Options.Instance.Player.MaxStat };
        nudMag = new NumericStepper { MinValue = -Options.Instance.Player.MaxStat, MaxValue = Options.Instance.Player.MaxStat };
        nudDef = new NumericStepper { MinValue = -Options.Instance.Player.MaxStat, MaxValue = Options.Instance.Player.MaxStat };
        nudMR = new NumericStepper { MinValue = -Options.Instance.Player.MaxStat, MaxValue = Options.Instance.Player.MaxStat };
        nudSpd = new NumericStepper { MinValue = -Options.Instance.Player.MaxStat, MaxValue = Options.Instance.Player.MaxStat };
        nudScaling = new NumericStepper();
        nudCritChance = new NumericStepper { MinValue = 0, MaxValue = 100 };
        nudCritMultiplier = new NumericStepper { MinValue = 0, DecimalPlaces = 2 };
        nudCastRange = new NumericStepper();
        nudHitRadius = new NumericStepper();
        nudDuration = new NumericStepper();
        nudBuffDuration = new NumericStepper();
        nudTick = new NumericStepper();
        nudWarpX = new NumericStepper { MinValue = 0, MaxValue = (int)Options.Instance.Map.MapWidth };
        nudWarpY = new NumericStepper { MinValue = 0, MaxValue = (int)Options.Instance.Map.MapHeight };

        scrlRange = new Slider { MinValue = 0, MaxValue = 10 };

        chkBound = new CheckBox { Text = Strings.SpellEditor.bound };
        chkFriendly = new CheckBox { Text = Strings.SpellEditor.friendly };
        chkHOTDOT = new CheckBox { Text = Strings.SpellEditor.ishotdot };
        chkIgnoreMapBlocks = new CheckBox { Text = Strings.SpellEditor.ignoreblocks };
        chkIgnoreActiveResources = new CheckBox { Text = Strings.SpellEditor.ignoreactiveresources };
        chkIgnoreInactiveResources = new CheckBox { Text = Strings.SpellEditor.ignoreinactiveresources };
        chkIgnoreZDimensionBlocks = new CheckBox { Text = Strings.SpellEditor.ignorezdimension };
        chkIgnoreGlobalCooldown = new CheckBox { Text = Strings.SpellEditor.IgnoreGlobalCooldown };
        chkIgnoreCdr = new CheckBox { Text = Strings.SpellEditor.IgnoreCooldownReduction };

        btnDynamicRequirements = new Button { Text = Strings.SpellEditor.requirementsbutton };
        btnVisualMapSelector = new Button { Text = Strings.Warping.visual };
        btnAddCooldownGroup = new Button { Text = "+" };

        grpSpells = new GroupBox { Text = Strings.SpellEditor.spells };
        grpGeneral = new GroupBox { Text = Strings.SpellEditor.general };
        grpSpellCost = new GroupBox { Text = Strings.SpellEditor.cost };
        grpTargetInfo = new GroupBox { Text = Strings.SpellEditor.targetting };
        grpCombat = new GroupBox { Text = Strings.SpellEditor.combatspell };
        grpDamage = new GroupBox { Text = Strings.SpellEditor.damagegroup };
        grpHotDot = new GroupBox { Text = Strings.SpellEditor.hotdot };
        grpStats = new GroupBox { Text = Strings.SpellEditor.stats };
        grpEffectDuration = new GroupBox { Text = Strings.SpellEditor.boostduration };
        grpEffect = new GroupBox { Text = Strings.SpellEditor.effectgroup };
        grpDash = new GroupBox { Text = Strings.SpellEditor.dash };
        grpDashCollisions = new GroupBox { Text = Strings.SpellEditor.dashcollisions };
        grpWarp = new GroupBox { Text = Strings.SpellEditor.warptomap };
        grpEvent = new GroupBox { Text = Strings.SpellEditor.Event };
        grpRequirements = new GroupBox { Text = Strings.SpellEditor.requirements };

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

        txtDesc!.TextChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Description = txtDesc.Text;
            }
        };

        if (btnSave != null) btnSave.Click += (s, e) =>
        {
            foreach (var item in mChanged)
            {
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
                item.RestoreBackup();
                item.DeleteBackup();
            }
            Close();
            Globals.CurrentEditor = -1;
        };

        if (btnDynamicRequirements != null) btnDynamicRequirements.Click += (s, e) =>
        {
            if (mEditorItem != null)
            {
                var frm = new FrmDynamicRequirements(mEditorItem.CastingRequirements, RequirementType.Spell);
                frm.ShowModal(this);
            }
        };
    }

    private void InitializeForm()
    {
        cmbScalingStat!.Items.Clear();
        for (var i = 0; i < Enum.GetValues<Stat>().Length; i++)
        {
            cmbScalingStat.Items.Add(Globals.GetStatName(i));
        }

        cmbProjectile!.Items.Clear();
        foreach (var name in ProjectileDescriptor.Names)
        {
            cmbProjectile.Items.Add(name);
        }

        cmbCastAnimation!.Items.Clear();
        cmbCastAnimation.Items.Add(Strings.General.None);
        foreach (var name in AnimationDescriptor.Names)
        {
            cmbCastAnimation.Items.Add(name);
        }

        cmbHitAnimation!.Items.Clear();
        cmbHitAnimation.Items.Add(Strings.General.None);
        foreach (var name in AnimationDescriptor.Names)
        {
            cmbHitAnimation.Items.Add(name);
        }

        cmbTickAnimation!.Items.Clear();
        cmbTickAnimation.Items.Add(Strings.General.None);
        foreach (var name in AnimationDescriptor.Names)
        {
            cmbTickAnimation.Items.Add(name);
        }

        cmbEvent!.Items.Clear();
        cmbEvent.Items.Add(Strings.General.None);
        foreach (var name in Intersect.Framework.Core.GameObjects.Events.EventDescriptor.Names)
        {
            cmbEvent.Items.Add(name);
        }

        cmbSprite!.Items.Clear();
        cmbSprite.Items.Add(Strings.General.None);
        foreach (var name in GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Spell))
        {
            cmbSprite.Items.Add(name);
        }

        cmbTransform!.Items.Clear();
        cmbTransform.Items.Add(Strings.General.None);
        foreach (var name in GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Entity))
        {
            cmbTransform.Items.Add(name);
        }

        cmbCastSprite!.Items.Clear();
        cmbCastSprite.Items.Add(Strings.General.None);
        foreach (var name in GameContentManager.GetOverridesFor(GameContentManager.TextureType.Entity, "cast"))
        {
            cmbCastSprite.Items.Add(name);
        }

        cmbWarpMap!.Items.Clear();
        foreach (var map in MapList.OrderedMaps)
        {
            cmbWarpMap.Items.Add(map?.Name ?? "");
        }

        cmbType!.Items.Clear();
        foreach (var type in Strings.SpellEditor.types)
        {
            cmbType.Items.Add(type.Value.ToString());
        }

        cmbTargetType!.Items.Clear();
        foreach (var targetType in Strings.SpellEditor.targettypes)
        {
            cmbTargetType.Items.Add(targetType.Value.ToString());
        }

        cmbDamageType!.Items.Clear();
        foreach (var dmgType in Strings.Combat.damagetypes)
        {
            cmbDamageType.Items.Add(dmgType.Value.ToString());
        }

        cmbExtraEffect!.Items.Clear();
        foreach (var effect in Strings.SpellEditor.effects)
        {
            cmbExtraEffect.Items.Add(effect.Value.ToString());
        }

        cmbDirection!.Items.Clear();
        for (var i = -1; i < 4; i++)
        {
            cmbDirection.Items.Add(Strings.Direction.dir[(Direction)i]);
        }

        InitEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Spell)
        {
            InitEditor();
            if (mEditorItem != null && !SpellDescriptor.Lookup.Values.Contains(mEditorItem))
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
            txtDesc!.Text = mEditorItem.Description;
            cmbType!.SelectedIndex = (int)mEditorItem.SpellType;

            nudCastDuration!.Value = mEditorItem.CastDuration;
            nudCooldownDuration!.Value = mEditorItem.CooldownDuration;
            cmbCooldownGroup!.SelectedIndex = cmbCooldownGroup.Items.IndexOf(
                new ListItem { Text = mEditorItem.CooldownGroup ?? "" }
            );
            chkIgnoreGlobalCooldown!.Checked = mEditorItem.IgnoreGlobalCooldown;
            chkIgnoreCdr!.Checked = mEditorItem.IgnoreCooldownReduction;

            cmbCastAnimation!.SelectedIndex = AnimationDescriptor.ListIndex(mEditorItem.CastAnimationId) + 1;
            cmbHitAnimation!.SelectedIndex = AnimationDescriptor.ListIndex(mEditorItem.HitAnimationId) + 1;
            cmbTickAnimation!.SelectedIndex = AnimationDescriptor.ListIndex(mEditorItem.TickAnimationId) + 1;

            chkBound!.Checked = mEditorItem.Bound;

            nudHPCost!.Value = mEditorItem.VitalCost[(int)Vital.Health];
            nudMpCost!.Value = mEditorItem.VitalCost[(int)Vital.Mana];

            txtCannotCast!.Text = mEditorItem.CannotCastMessage;

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
        foreach (var itm in SpellDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((SpellDescriptor)itm.Value).Folder) &&
                !mFolders.Contains(((SpellDescriptor)itm.Value).Folder))
            {
                mFolders.Add(((SpellDescriptor)itm.Value).Folder);
                if (!mKnownFolders.Contains(((SpellDescriptor)itm.Value).Folder))
                {
                    mKnownFolders.Add(((SpellDescriptor)itm.Value).Folder);
                }
            }

            if (!string.IsNullOrWhiteSpace(((SpellDescriptor)itm.Value).CooldownGroup) &&
                !mKnownCooldownGroups.Contains(((SpellDescriptor)itm.Value).CooldownGroup))
            {
                mKnownCooldownGroups.Add(((SpellDescriptor)itm.Value).CooldownGroup);
            }
        }

        if (Options.Instance.Combat.LinkSpellAndItemCooldowns)
        {
            foreach (var itm in ItemDescriptor.Lookup)
            {
                if (!string.IsNullOrWhiteSpace(((ItemDescriptor)itm.Value).CooldownGroup) &&
                    !mKnownCooldownGroups.Contains(((ItemDescriptor)itm.Value).CooldownGroup))
                {
                    mKnownCooldownGroups.Add(((ItemDescriptor)itm.Value).CooldownGroup);
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

        mKnownCooldownGroups.Sort();
        cmbCooldownGroup!.Items.Clear();
        cmbCooldownGroup.Items.Add(string.Empty);
        foreach (var group in mKnownCooldownGroups)
        {
            cmbCooldownGroup.Items.Add(group);
        }

        if (lstGameObjects != null)
        {
            lstGameObjects.Items.Clear();
            var items = SpellDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var spell = (SpellDescriptor?)pair.Value;
                if (spell != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = spell.Name ?? "Deleted" });
                }
            }
        }
    }
}
