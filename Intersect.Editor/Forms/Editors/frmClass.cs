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
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.Framework.Core.GameObjects.Maps.MapList;
using Intersect.Framework.Core.GameObjects.PlayerClass;
using Intersect.GameObjects;
using Intersect.Utilities;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmClass : EditorForm
{
    private List<ClassDescriptor> mChanged = new();
    private string? mCopiedItem;
    private ClassDescriptor? mEditorItem;
    private List<string> mKnownFolders = new();

    protected ListBox? lstGameObjects;
    protected ListBox? lstSpells;
    protected ListBox? lstSprites;
    protected ListBox? lstSpawnItems;
    protected TextBox? txtName;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbWarpMap;
    protected DropDown? cmbSprite;
    protected DropDown? cmbFace;
    protected DropDown? cmbSpell;
    protected DropDown? cmbSpawnItem;
    protected DropDown? cmbAttackAnimation;
    protected DropDown? cmbAttackSprite;
    protected DropDown? cmbDamageType;
    protected DropDown? cmbScalingStat;
    protected DropDown? cmbAttackSpeedModifier;
    protected DropDown? cmbDirection;
    protected NumericStepper? nudBaseHP;
    protected NumericStepper? nudBaseMana;
    protected NumericStepper? nudAttack;
    protected NumericStepper? nudMag;
    protected NumericStepper? nudDef;
    protected NumericStepper? nudMR;
    protected NumericStepper? nudSpd;
    protected NumericStepper? nudPoints;
    protected NumericStepper? nudDamage;
    protected NumericStepper? nudCritChance;
    protected NumericStepper? nudCritMultiplier;
    protected NumericStepper? nudScaling;
    protected NumericStepper? nudAttackSpeedValue;
    protected NumericStepper? nudHPRegen;
    protected NumericStepper? nudMpRegen;
    protected NumericStepper? nudBaseExp;
    protected NumericStepper? nudExpIncrease;
    protected NumericStepper? nudLevel;
    protected NumericStepper? nudX;
    protected NumericStepper? nudY;
    protected NumericStepper? nudHpIncrease;
    protected NumericStepper? nudMpIncrease;
    protected NumericStepper? nudStrengthIncrease;
    protected NumericStepper? nudArmorIncrease;
    protected NumericStepper? nudSpeedIncrease;
    protected NumericStepper? nudMagicIncrease;
    protected NumericStepper? nudMagicResistIncrease;
    protected NumericStepper? nudPointsIncrease;
    protected NumericStepper? nudSpawnItemAmount;
    protected CheckBox? chkLocked;
    protected RadioButton? rdoStaticIncrease;
    protected RadioButton? rdoPercentageIncrease;
    protected RadioButton? rbMale;
    protected RadioButton? rbFemale;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnVisualMapSelector;
    protected Button? btnAdd;
    protected Button? btnRemove;
    protected Button? btnAddSpell;
    protected Button? btnRemoveSpell;
    protected Button? btnSpawnItemAdd;
    protected Button? btnSpawnItemRemove;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected Button? btnExpGrid;
    protected GroupBox? grpClasses;
    protected GroupBox? grpGeneral;
    protected GroupBox? grpSpawnPoint;
    protected GroupBox? grpSprite;
    protected GroupBox? grpSpriteOptions;
    protected GroupBox? grpGender;
    protected GroupBox? grpSpawnItems;
    protected GroupBox? grpBaseStats;
    protected GroupBox? grpSpells;
    protected GroupBox? grpRegen;
    protected GroupBox? grpCombat;
    protected GroupBox? grpAttackSpeed;
    protected GroupBox? grpLeveling;
    protected GroupBox? grpLevelBoosts;
    protected Panel? pnlContainer;
    protected Label? lblName;
    protected Label? lblMap;
    protected Label? lblX;
    protected Label? lblY;
    protected Label? lblDir;
    protected Label? lblSprite;
    protected Label? lblFace;
    protected Label? lblSpawnItem;
    protected Label? lblSpawnItemAmount;
    protected Label? lblHP;
    protected Label? lblMana;
    protected Label? lblAttack;
    protected Label? lblDef;
    protected Label? lblSpd;
    protected Label? lblMag;
    protected Label? lblMR;
    protected Label? lblPoints;
    protected Label? lblSpellNum;
    protected Label? lblLevel;
    protected Label? lblDamage;
    protected Label? lblCritChance;
    protected Label? lblCritMultiplier;
    protected Label? lblDamageType;
    protected Label? lblScalingStat;
    protected Label? lblScalingAmount;
    protected Label? lblAttackAnimation;
    protected Label? lblSpriteAttack;
    protected Label? lblHpRegen;
    protected Label? lblManaRegen;
    protected Label? lblRegenHint;
    protected Label? lblBaseExp;
    protected Label? lblExpIncrease;
    protected Label? lblHpIncrease;
    protected Label? lblMpIncrease;
    protected Label? lblStrengthIncrease;
    protected Label? lblArmorIncrease;
    protected Label? lblSpeedIncrease;
    protected Label? lblMagicIncrease;
    protected Label? lblMagicResistIncrease;
    protected Label? lblPointsIncrease;
    protected Label? lblFolder;
    protected Label? lblAttackSpeedModifier;
    protected Label? lblAttackSpeedValue;

    public FrmClass()
    {
        ApplyHooks();
        BuildUI();
        InitializeForm();
    }

    private void BuildUI()
    {
        Title = Strings.ClassEditor.title;
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
        txtSearch = new TextBox { PlaceholderText = Strings.ClassEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.ClassEditor.folderlabel };

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
                new Button { Text = Strings.ClassEditor.New },
                new Button { Text = Strings.ClassEditor.delete },
                new Button { Text = Strings.ClassEditor.copy },
                new Button { Text = Strings.ClassEditor.paste },
                new Button { Text = Strings.ClassEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.ClassEditor.save };
        btnCancel = new Button { Text = Strings.ClassEditor.cancel };
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
        cmbWarpMap = new DropDown();
        cmbSprite = new DropDown();
        cmbFace = new DropDown();
        cmbSpell = new DropDown();
        cmbSpawnItem = new DropDown();
        cmbAttackAnimation = new DropDown();
        cmbAttackSprite = new DropDown();
        cmbDamageType = new DropDown();
        cmbScalingStat = new DropDown();
        cmbAttackSpeedModifier = new DropDown();
        cmbDirection = new DropDown();

        nudBaseHP = new NumericStepper { MinValue = 0 };
        nudBaseMana = new NumericStepper { MinValue = 0 };
        nudAttack = new NumericStepper { MinValue = 0, MaxValue = Options.Instance.Player.MaxStat };
        nudMag = new NumericStepper { MinValue = 0, MaxValue = Options.Instance.Player.MaxStat };
        nudDef = new NumericStepper { MinValue = 0, MaxValue = Options.Instance.Player.MaxStat };
        nudMR = new NumericStepper { MinValue = 0, MaxValue = Options.Instance.Player.MaxStat };
        nudSpd = new NumericStepper { MinValue = 0, MaxValue = Options.Instance.Player.MaxStat };
        nudPoints = new NumericStepper { MinValue = 0 };
        nudDamage = new NumericStepper { MinValue = 0 };
        nudCritChance = new NumericStepper { MinValue = 0, MaxValue = 100 };
        nudCritMultiplier = new NumericStepper { MinValue = 0, DecimalPlaces = 2 };
        nudScaling = new NumericStepper { MinValue = 0 };
        nudAttackSpeedValue = new NumericStepper { MinValue = 0 };
        nudHPRegen = new NumericStepper();
        nudMpRegen = new NumericStepper();
        nudBaseExp = new NumericStepper { MinValue = 0 };
        nudExpIncrease = new NumericStepper { MinValue = 0 };
        nudLevel = new NumericStepper { MinValue = 1, MaxValue = Options.Instance.Player.MaxLevel };
        nudX = new NumericStepper { MinValue = 0 };
        nudY = new NumericStepper { MinValue = 0 };
        nudHpIncrease = new NumericStepper();
        nudMpIncrease = new NumericStepper();
        nudStrengthIncrease = new NumericStepper();
        nudArmorIncrease = new NumericStepper();
        nudSpeedIncrease = new NumericStepper();
        nudMagicIncrease = new NumericStepper();
        nudMagicResistIncrease = new NumericStepper();
        nudPointsIncrease = new NumericStepper();
        nudSpawnItemAmount = new NumericStepper { MinValue = 1 };

        chkLocked = new CheckBox { Text = Strings.ClassEditor.locked };
        rdoStaticIncrease = new RadioButton { Text = Strings.ClassEditor.staticboost };
        rdoPercentageIncrease = new RadioButton(rdoStaticIncrease) { Text = Strings.ClassEditor.percentageboost };
        rbMale = new RadioButton { Text = Strings.ClassEditor.male };
        rbFemale = new RadioButton(rbMale) { Text = Strings.ClassEditor.female };

        lstSpells = new ListBox();
        lstSprites = new ListBox();
        lstSpawnItems = new ListBox();

        btnVisualMapSelector = new Button { Text = Strings.Warping.visual };
        btnAdd = new Button { Text = Strings.ClassEditor.addsprite };
        btnRemove = new Button { Text = Strings.ClassEditor.removeicon };
        btnAddSpell = new Button { Text = Strings.ClassEditor.addspell };
        btnRemoveSpell = new Button { Text = Strings.ClassEditor.removespell };
        btnSpawnItemAdd = new Button { Text = Strings.ClassEditor.spawnitemadd };
        btnSpawnItemRemove = new Button { Text = Strings.ClassEditor.spawnitemremove };
        btnExpGrid = new Button { Text = Strings.ClassEditor.expgrid };

        grpClasses = new GroupBox { Text = Strings.ClassEditor.classes };
        grpGeneral = new GroupBox { Text = Strings.ClassEditor.general };
        grpSpawnPoint = new GroupBox { Text = Strings.ClassEditor.spawnpoint };
        grpSprite = new GroupBox { Text = Strings.ClassEditor.spriteface };
        grpSpriteOptions = new GroupBox { Text = Strings.ClassEditor.spriteoptions };
        grpGender = new GroupBox { Text = Strings.ClassEditor.gender };
        grpSpawnItems = new GroupBox { Text = Strings.ClassEditor.spawnitems };
        grpBaseStats = new GroupBox { Text = Strings.ClassEditor.basestats };
        grpSpells = new GroupBox { Text = Strings.ClassEditor.learntspells };
        grpRegen = new GroupBox { Text = Strings.ClassEditor.regen };
        grpCombat = new GroupBox { Text = Strings.ClassEditor.combat };
        grpAttackSpeed = new GroupBox { Text = Strings.NpcEditor.attackspeed };
        grpLeveling = new GroupBox { Text = Strings.ClassEditor.leveling };
        grpLevelBoosts = new GroupBox { Text = Strings.ClassEditor.levelboosts };

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

        if (chkLocked != null) chkLocked.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Locked = chkLocked.Checked ?? false;
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
    }

    private void InitializeForm()
    {
        cmbWarpMap!.Items.Clear();
        foreach (var map in MapList.OrderedMaps)
        {
            cmbWarpMap.Items.Add(map.Name);
        }

        cmbSprite!.Items.Clear();
        cmbSprite.Items.Add(Strings.General.None);
        foreach (var name in GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Entity))
        {
            cmbSprite.Items.Add(name);
        }

        cmbFace!.Items.Clear();
        cmbFace.Items.Add(Strings.General.None);
        foreach (var name in GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Face))
        {
            cmbFace.Items.Add(name);
        }

        cmbSpawnItem!.Items.Clear();
        cmbSpawnItem.Items.Add(Strings.General.None);
        foreach (var name in ItemDescriptor.Names)
        {
            cmbSpawnItem.Items.Add(name);
        }

        cmbSpell!.Items.Clear();
        foreach (var name in SpellDescriptor.Names)
        {
            cmbSpell.Items.Add(name);
        }

        cmbAttackAnimation!.Items.Clear();
        cmbAttackAnimation.Items.Add(Strings.General.None);
        foreach (var name in AnimationDescriptor.Names)
        {
            cmbAttackAnimation.Items.Add(name);
        }

        cmbAttackSprite!.Items.Clear();
        cmbAttackSprite.Items.Add(Strings.General.None);
        foreach (var name in GameContentManager.GetOverridesFor(GameContentManager.TextureType.Entity, "attack"))
        {
            cmbAttackSprite.Items.Add(name);
        }

        cmbScalingStat!.Items.Clear();
        for (var x = 0; x < ((int)Stat.Speed) + 1; x++)
        {
            cmbScalingStat.Items.Add(Globals.GetStatName(x));
        }

        cmbDirection!.Items.Clear();
        for (var i = 0; i < 4; i++)
        {
            cmbDirection.Items.Add(Strings.Direction.dir[(Direction)i]);
        }

        cmbDamageType!.Items.Clear();
        foreach (var dmgType in Strings.Combat.damagetypes)
        {
                    // Removed KeyValuePair to string conversion
        }

        cmbAttackSpeedModifier!.Items.Clear();
        foreach (var val in Strings.NpcEditor.attackspeedmodifiers.Values)
        {
            cmbAttackSpeedModifier.Items.Add(val.ToString());
        }

        InitEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Class)
        {
            InitEditor();
            if (mEditorItem != null && !ClassDescriptor.Lookup.Values.Contains(mEditorItem))
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
            chkLocked!.Checked = Convert.ToBoolean(mEditorItem.Locked);

            nudAttack!.Value = mEditorItem.BaseStat[(int)Stat.Attack];
            nudMag!.Value = mEditorItem.BaseStat[(int)Stat.AbilityPower];
            nudDef!.Value = mEditorItem.BaseStat[(int)Stat.Defense];
            nudMR!.Value = mEditorItem.BaseStat[(int)Stat.MagicResist];
            nudSpd!.Value = mEditorItem.BaseStat[(int)Stat.Speed];
            nudBaseHP!.Value = mEditorItem.BaseVital[(int)Vital.Health];
            nudBaseMana!.Value = mEditorItem.BaseVital[(int)Vital.Mana];
            nudPoints!.Value = mEditorItem.BasePoints;

            nudDamage!.Value = mEditorItem.Damage;
            nudCritChance!.Value = mEditorItem.CritChance;
            nudCritMultiplier!.Value = mEditorItem.CritMultiplier;
            nudScaling!.Value = mEditorItem.Scaling;
            cmbDamageType!.SelectedIndex = mEditorItem.DamageType;
            cmbScalingStat!.SelectedIndex = mEditorItem.ScalingStat;
            cmbAttackAnimation!.SelectedIndex = AnimationDescriptor.ListIndex(mEditorItem.AttackAnimationId) + 1;
            cmbAttackSpeedModifier!.SelectedIndex = mEditorItem.AttackSpeedModifier;
            nudAttackSpeedValue!.Value = mEditorItem.AttackSpeedValue;

            nudHPRegen!.Value = mEditorItem.VitalRegen[(int)Vital.Health];
            nudMpRegen!.Value = mEditorItem.VitalRegen[(int)Vital.Mana];

            nudBaseExp!.Value = mEditorItem.BaseExp;
            nudExpIncrease!.Value = mEditorItem.ExpIncrease;

            if (!mEditorItem.IncreasePercentage)
            {
                rdoStaticIncrease!.Checked = true;
            }
            else
            {
                rdoPercentageIncrease!.Checked = true;
            }

            nudX!.Value = mEditorItem.SpawnX;
            nudY!.Value = mEditorItem.SpawnY;
            cmbDirection!.SelectedIndex = mEditorItem.SpawnDir;

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
        foreach (var itm in ClassDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((ClassDescriptor)itm.Value).Folder) &&
                !mFolders.Contains(((ClassDescriptor)itm.Value).Folder))
            {
                mFolders.Add(((ClassDescriptor)itm.Value).Folder);
                if (!mKnownFolders.Contains(((ClassDescriptor)itm.Value).Folder))
                {
                    mKnownFolders.Add(((ClassDescriptor)itm.Value).Folder);
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
            var items = ClassDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var cls = (ClassDescriptor?)pair.Value;
                if (cls != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = cls.Name ?? "Deleted" });
                }
            }
        }
    }
}
