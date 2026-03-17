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
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.GameObjects;
using Intersect.Localization;
using Intersect.Utilities;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmItem : EditorForm
{
    private List<ItemDescriptor> mChanged = new();
    private string? mCopiedItem;
    private ItemDescriptor? mEditorItem;
    private List<string> mKnownFolders = new();
    private List<string> mKnownCooldownGroups = new();

    // Eto.Forms controls
    protected ListBox? lstGameObjects;
    protected TextBox? txtName;
    protected TextBox? txtDesc;
    protected DropDown? cmbType;
    protected DropDown? cmbPic;
    protected DropDown? cmbFolder;
    protected NumericStepper? nudPrice;
    protected NumericStepper? nudRgbaR;
    protected NumericStepper? nudRgbaG;
    protected NumericStepper? nudRgbaB;
    protected NumericStepper? nudRgbaA;
    protected NumericStepper? nudStr;
    protected NumericStepper? nudMag;
    protected NumericStepper? nudDef;
    protected NumericStepper? nudMR;
    protected NumericStepper? nudSpd;
    protected NumericStepper? nudDamage;
    protected NumericStepper? nudCritChance;
    protected NumericStepper? nudCritMultiplier;
    protected NumericStepper? nudScaling;
    protected NumericStepper? nudCooldown;
    protected NumericStepper? nudHealthBonus;
    protected NumericStepper? nudManaBonus;
    protected NumericStepper? nudHPRegen;
    protected NumericStepper? nudMpRegen;
    protected NumericStepper? nudBag;
    protected NumericStepper? nudInterval;
    protected NumericStepper? nudIntervalPercentage;
    protected NumericStepper? nudInvStackLimit;
    protected NumericStepper? nudBankStackLimit;
    protected NumericStepper? nudDeathDropChance;
    protected NumericStepper? nudItemDespawnTime;
    protected NumericStepper? nudAttackSpeedValue;
    protected NumericStepper? nudBlockChance;
    protected NumericStepper? nudBlockAmount;
    protected NumericStepper? nudBlockDmgAbs;
    protected NumericStepper? nudStatRangeHigh;
    protected NumericStepper? nudHPPercentage;
    protected NumericStepper? nudMPPercentage;
    protected NumericStepper? nudStrPercentage;
    protected NumericStepper? nudMagPercentage;
    protected NumericStepper? nudDefPercentage;
    protected NumericStepper? nudMRPercentage;
    protected NumericStepper? nudSpdPercentage;
    protected NumericStepper? nudEffectPercent;
    protected CheckBox? chkCanDrop;
    protected CheckBox? chkCanBank;
    protected CheckBox? chkCanGuildBank;
    protected CheckBox? chkCanBag;
    protected CheckBox? chkCanTrade;
    protected CheckBox? chkCanSell;
    protected CheckBox? chkStackable;
    protected CheckBox? chk2Hand;
    protected CheckBox? chkQuickCast;
    protected CheckBox? chkSingleUseSpell;
    protected CheckBox? chkSingleUseEvent;
    protected CheckBox? chkIgnoreGlobalCooldown;
    protected CheckBox? chkIgnoreCdr;
    protected DropDown? cmbEquipmentSlot;
    protected DropDown? cmbToolType;
    protected DropDown? cmbProjectile;
    protected DropDown? cmbAttackAnimation;
    protected DropDown? cmbDamageType;
    protected DropDown? cmbScalingStat;
    protected DropDown? cmbAnimation;
    protected DropDown? cmbEquipmentAnimation;
    protected DropDown? cmbTeachSpell;
    protected DropDown? cmbConsume;
    protected DropDown? cmbWeaponSprite;
    protected DropDown? cmbMalePaperdoll;
    protected DropDown? cmbFemalePaperdoll;
    protected DropDown? cmbRarity;
    protected DropDown? cmbCooldownGroup;
    protected DropDown? cmbEvent;
    protected DropDown? cmbEventTriggers;
    protected TextBox? txtCannotUse;
    protected TextBox? txtSearch;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnEditRequirements;
    protected Button? btnAddFolder;
    protected Button? btnAddCooldownGroup;
    protected Button? btnClearSearch;
    protected ListBox? lstEventTriggers;
    protected ListBox? lstBonusEffects;
    protected ListBox? lstStatRanges;
    protected GroupBox? grpItems;
    protected GroupBox? grpGeneral;
    protected GroupBox? grpEquipment;
    protected GroupBox? grpWeaponProperties;
    protected GroupBox? grpShieldProperties;
    protected GroupBox? grpConsumable;
    protected GroupBox? grpSpell;
    protected GroupBox? grpEvent;
    protected GroupBox? grpBags;
    protected GroupBox? grpStack;
    protected GroupBox? grpEvents;
    protected GroupBox? grpCooldown;
    protected GroupBox? grpVitalBonuses;
    protected GroupBox? grpRegen;
    protected GroupBox? grpAttackSpeed;
    protected GroupBox? grpRequirements;
    protected GroupBox? grpEffects;
    protected GroupBox? grpStatBonuses;
    protected GroupBox? grpStatRanges;
    protected Panel? pnlContainer;
    protected Label? lblName;
    protected Label? lblType;
    protected Label? lblDesc;
    protected Label? lblPic;
    protected Label? lblPrice;
    protected Label? lblAnim;
    protected Label? lblRed;
    protected Label? lblGreen;
    protected Label? lblBlue;
    protected Label? lblAlpha;
    protected Label? lblDeathDropChance;
    protected Label? lblDespawnTime;
    protected Label? lblEquipmentSlot;
    protected Label? lblStr;
    protected Label? lblMag;
    protected Label? lblDef;
    protected Label? lblMR;
    protected Label? lblSpd;
    protected Label? lblDamage;
    protected Label? lblCritChance;
    protected Label? lblCritMultiplier;
    protected Label? lblDamageType;
    protected Label? lblScalingStat;
    protected Label? lblScalingAmount;
    protected Label? lblAttackAnimation;
    protected Label? lblSpriteAttack;
    protected Label? lblProjectile;
    protected Label? lblToolType;
    protected Label? lblCooldown;
    protected Label? lblCooldownGroup;
    protected Label? lblHealthBonus;
    protected Label? lblManaBonus;
    protected Label? lblHpRegen;
    protected Label? lblManaRegen;
    protected Label? lblRegenHint;
    protected Label? lblAttackSpeedModifier;
    protected Label? lblAttackSpeedValue;
    protected Label? lblMalePaperdoll;
    protected Label? lblFemalePaperdoll;
    protected Label? lblBag;
    protected Label? lblSpell;
    protected Label? lblVital;
    protected Label? lblInterval;
    protected Label? lblEventForTrigger;
    protected Label? lblCannotUse;
    protected Label? lblFolder;
    protected Label? lblEquipmentAnimation;
    protected Label? lblEffectPercent;
    protected Label? lblStatRangeFrom;
    protected Label? lblStatRangeTo;
    protected Label? lblBlockChance;
    protected Label? lblBlockAmount;
    protected Label? lblBlockDmgAbs;
    protected Label? lblInvStackLimit;
    protected Label? lblBankStackLimit;
    protected DropDown? cmbAttackSpeedModifier;

    public FrmItem()
    {
        ApplyHooks();
        BuildUI();
        InitializeForm();
    }

    private void BuildUI()
    {
        Title = Strings.ItemEditor.title;
        MinimumSize = new Size(1024, 768);

        // Create main layout with splitter
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
        txtSearch = new TextBox { PlaceholderText = Strings.ItemEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.ItemEditor.folderlabel };

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
                Items =
                {
                    searchPanel,
                    folderPanel,
                    lstGameObjects
                }
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
                new Button { Text = Strings.ItemEditor.New },
                new Button { Text = Strings.ItemEditor.delete },
                new Button { Text = Strings.ItemEditor.copy },
                new Button { Text = Strings.ItemEditor.paste },
                new Button { Text = Strings.ItemEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.ItemEditor.save };
        btnCancel = new Button { Text = Strings.ItemEditor.cancel };
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
        cmbType = new DropDown();
        cmbPic = new DropDown();
        nudPrice = new NumericStepper { MinValue = 0, MaxValue = int.MaxValue };
        nudRgbaR = new NumericStepper { MinValue = 0, MaxValue = 255 };
        nudRgbaG = new NumericStepper { MinValue = 0, MaxValue = 255 };
        nudRgbaB = new NumericStepper { MinValue = 0, MaxValue = 255 };
        nudRgbaA = new NumericStepper { MinValue = 0, MaxValue = 255 };
        nudStr = new NumericStepper { MinValue = -Options.Instance.Player.MaxStat, MaxValue = Options.Instance.Player.MaxStat };
        nudMag = new NumericStepper { MinValue = -Options.Instance.Player.MaxStat, MaxValue = Options.Instance.Player.MaxStat };
        nudDef = new NumericStepper { MinValue = -Options.Instance.Player.MaxStat, MaxValue = Options.Instance.Player.MaxStat };
        nudMR = new NumericStepper { MinValue = -Options.Instance.Player.MaxStat, MaxValue = Options.Instance.Player.MaxStat };
        nudSpd = new NumericStepper { MinValue = -Options.Instance.Player.MaxStat, MaxValue = Options.Instance.Player.MaxStat };
        nudDamage = new NumericStepper { MinValue = 0, MaxValue = int.MaxValue };
        nudCritChance = new NumericStepper { MinValue = 0, MaxValue = 100 };
        nudCritMultiplier = new NumericStepper { MinValue = 0, MaxValue = 100, DecimalPlaces = 2 };
        nudScaling = new NumericStepper { MinValue = 0, MaxValue = int.MaxValue };
        nudCooldown = new NumericStepper { MinValue = 0, MaxValue = int.MaxValue };
        nudHealthBonus = new NumericStepper();
        nudManaBonus = new NumericStepper();
        nudHPRegen = new NumericStepper();
        nudMpRegen = new NumericStepper();
        nudBag = new NumericStepper { MinValue = 1 };
        nudInterval = new NumericStepper();
        nudIntervalPercentage = new NumericStepper();
        nudInvStackLimit = new NumericStepper { MinValue = 1 };
        nudBankStackLimit = new NumericStepper { MinValue = 1 };
        nudDeathDropChance = new NumericStepper { MinValue = 0, MaxValue = 100 };
        nudItemDespawnTime = new NumericStepper { MinValue = 0, MaxValue = long.MaxValue };
        nudAttackSpeedValue = new NumericStepper();
        nudBlockChance = new NumericStepper { MinValue = 0, MaxValue = 100 };
        nudBlockAmount = new NumericStepper();
        nudBlockDmgAbs = new NumericStepper();
        nudStatRangeHigh = new NumericStepper();
        nudHPPercentage = new NumericStepper();
        nudMPPercentage = new NumericStepper();
        nudStrPercentage = new NumericStepper();
        nudMagPercentage = new NumericStepper();
        nudDefPercentage = new NumericStepper();
        nudMRPercentage = new NumericStepper();
        nudSpdPercentage = new NumericStepper();
        nudEffectPercent = new NumericStepper();

        chkCanDrop = new CheckBox { Text = Strings.ItemEditor.CanDrop };
        chkCanBank = new CheckBox { Text = Strings.ItemEditor.CanBank };
        chkCanGuildBank = new CheckBox { Text = Strings.ItemEditor.CanGuildBank };
        chkCanBag = new CheckBox { Text = Strings.ItemEditor.CanBag };
        chkCanTrade = new CheckBox { Text = Strings.ItemEditor.CanTrade };
        chkCanSell = new CheckBox { Text = Strings.ItemEditor.CanSell };
        chkStackable = new CheckBox { Text = Strings.ItemEditor.stackable };
        chk2Hand = new CheckBox { Text = Strings.ItemEditor.twohanded };
        chkQuickCast = new CheckBox { Text = Strings.ItemEditor.quickcast };
        chkSingleUseSpell = new CheckBox { Text = Strings.ItemEditor.destroyspell };
        chkSingleUseEvent = new CheckBox { Text = Strings.ItemEditor.SingleUseEvent };
        chkIgnoreGlobalCooldown = new CheckBox { Text = Strings.ItemEditor.IgnoreGlobalCooldown };
        chkIgnoreCdr = new CheckBox { Text = Strings.ItemEditor.IgnoreCooldownReduction };

        cmbEquipmentSlot = new DropDown();
        cmbToolType = new DropDown();
        cmbProjectile = new DropDown();
        cmbAttackAnimation = new DropDown();
        cmbDamageType = new DropDown();
        cmbScalingStat = new DropDown();
        cmbAnimation = new DropDown();
        cmbEquipmentAnimation = new DropDown();
        cmbTeachSpell = new DropDown();
        cmbConsume = new DropDown();
        cmbWeaponSprite = new DropDown();
        cmbMalePaperdoll = new DropDown();
        cmbFemalePaperdoll = new DropDown();
        cmbRarity = new DropDown();
        cmbCooldownGroup = new DropDown();
        cmbEvent = new DropDown();
        cmbEventTriggers = new DropDown();
        cmbAttackSpeedModifier = new DropDown();

        txtCannotUse = new TextBox();
        txtSearch = new TextBox { PlaceholderText = Strings.ItemEditor.searchplaceholder };
        btnEditRequirements = new Button { Text = Strings.ItemEditor.requirements };
        btnAddCooldownGroup = new Button { Text = "+" };

        lstEventTriggers = new ListBox();
        lstBonusEffects = new ListBox();
        lstStatRanges = new ListBox();

        grpItems = new GroupBox { Text = Strings.ItemEditor.items };
        grpGeneral = new GroupBox { Text = Strings.ItemEditor.general };
        grpEquipment = new GroupBox { Text = Strings.ItemEditor.equipment };
        grpWeaponProperties = new GroupBox { Text = Strings.ItemEditor.weaponproperties };
        grpShieldProperties = new GroupBox { Text = Strings.ItemEditor.ShieldProperties };
        grpConsumable = new GroupBox { Text = Strings.ItemEditor.consumeablepanel };
        grpSpell = new GroupBox { Text = Strings.ItemEditor.spellpanel };
        grpEvent = new GroupBox { Text = Strings.ItemEditor.eventpanel };
        grpBags = new GroupBox { Text = Strings.ItemEditor.bagpanel };
        grpStack = new GroupBox { Text = Strings.ItemEditor.StackOptions };
        grpEvents = new GroupBox { Text = Strings.ItemEditor.EventGroup };
        grpCooldown = new GroupBox { Text = Strings.ItemEditor.CooldownOptions };
        grpVitalBonuses = new GroupBox { Text = Strings.ItemEditor.vitalbonuses };
        grpRegen = new GroupBox { Text = Strings.ItemEditor.regen };
        grpAttackSpeed = new GroupBox { Text = Strings.ItemEditor.attackspeed };
        grpRequirements = new GroupBox { Text = Strings.ItemEditor.requirementsgroup };
        grpEffects = new GroupBox { Text = Strings.ItemEditor.BonusEffectGroup };
        grpStatBonuses = new GroupBox { Text = Strings.ItemEditor.bonuses };
        grpStatRanges = new GroupBox { Text = Strings.ItemEditor.StatRangeTitle };

        lblName = new Label { Text = Strings.ItemEditor.name };
        lblType = new Label { Text = Strings.ItemEditor.type };
        lblDesc = new Label { Text = Strings.ItemEditor.description };
        lblPic = new Label { Text = Strings.ItemEditor.picture };
        lblPrice = new Label { Text = Strings.ItemEditor.price };
        lblAnim = new Label { Text = Strings.ItemEditor.animation };
        lblRed = new Label { Text = Strings.ItemEditor.Red };
        lblGreen = new Label { Text = Strings.ItemEditor.Green };
        lblBlue = new Label { Text = Strings.ItemEditor.Blue };
        lblAlpha = new Label { Text = Strings.ItemEditor.Alpha };
        lblDeathDropChance = new Label { Text = Strings.ItemEditor.DeathDropChance };
        lblDespawnTime = new Label { Text = Strings.ItemEditor.DespawnTime };
        lblEquipmentSlot = new Label { Text = Strings.ItemEditor.slot };
        lblStr = new Label { Text = Strings.ItemEditor.attackbonus };
        lblMag = new Label { Text = Strings.ItemEditor.abilitypowerbonus };
        lblDef = new Label { Text = Strings.ItemEditor.defensebonus };
        lblMR = new Label { Text = Strings.ItemEditor.magicresistbonus };
        lblSpd = new Label { Text = Strings.ItemEditor.speedbonus };
        lblDamage = new Label { Text = Strings.ItemEditor.basedamage };
        lblCritChance = new Label { Text = Strings.ItemEditor.critchance };
        lblCritMultiplier = new Label { Text = Strings.ItemEditor.critmultiplier };
        lblDamageType = new Label { Text = Strings.ItemEditor.damagetype };
        lblScalingStat = new Label { Text = Strings.ItemEditor.scalingstat };
        lblScalingAmount = new Label { Text = Strings.ItemEditor.scalingamount };
        lblAttackAnimation = new Label { Text = Strings.ItemEditor.attackanimation };
        lblSpriteAttack = new Label { Text = Strings.ItemEditor.AttackSpriteOverride };
        lblProjectile = new Label { Text = Strings.ItemEditor.projectile };
        lblToolType = new Label { Text = Strings.ItemEditor.tooltype };
        lblCooldown = new Label { Text = Strings.ItemEditor.cooldown };
        lblCooldownGroup = new Label { Text = Strings.ItemEditor.CooldownGroup };
        lblHealthBonus = new Label { Text = Strings.ItemEditor.health };
        lblManaBonus = new Label { Text = Strings.ItemEditor.mana };
        lblHpRegen = new Label { Text = Strings.ItemEditor.hpregen };
        lblManaRegen = new Label { Text = Strings.ItemEditor.mpregen };
        lblRegenHint = new Label { Text = Strings.ItemEditor.regenhint };
        lblAttackSpeedModifier = new Label { Text = Strings.ItemEditor.attackspeedmodifier };
        lblAttackSpeedValue = new Label { Text = Strings.ItemEditor.attackspeedvalue };
        lblMalePaperdoll = new Label { Text = Strings.ItemEditor.malepaperdoll };
        lblFemalePaperdoll = new Label { Text = Strings.ItemEditor.femalepaperdoll };
        lblBag = new Label { Text = Strings.ItemEditor.bagslots };
        lblSpell = new Label { Text = Strings.ItemEditor.spell };
        lblVital = new Label { Text = Strings.ItemEditor.vital };
        lblInterval = new Label { Text = Strings.ItemEditor.consumeamount };
        lblEventForTrigger = new Label { Text = Strings.ItemEditor.EventGroupLabel };
        lblCannotUse = new Label { Text = Strings.ItemEditor.cannotuse };
        lblFolder = new Label { Text = Strings.ItemEditor.folderlabel };
        lblEquipmentAnimation = new Label { Text = Strings.ItemEditor.equipmentanimation };
        lblEffectPercent = new Label { Text = Strings.ItemEditor.bonusamount };
        lblStatRangeFrom = new Label { Text = Strings.ItemEditor.StatRangeFrom };
        lblStatRangeTo = new Label { Text = Strings.ItemEditor.StatRangeTo };
        lblBlockChance = new Label { Text = Strings.ItemEditor.BlockChance };
        lblBlockAmount = new Label { Text = Strings.ItemEditor.BlockAmount };
        lblBlockDmgAbs = new Label { Text = Strings.ItemEditor.BlockAbsorption };
        lblInvStackLimit = new Label { Text = Strings.ItemEditor.InventoryStackLimit };
        lblBankStackLimit = new Label { Text = Strings.ItemEditor.BankStackLimit };

        // Wire up event handlers
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        // Wire up events
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

        if (cmbType != null) cmbType.SelectedIndexChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                RefreshExtendedData();
            }
        };

        nudPrice!.ValueChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Price = (int)nudPrice.Value;
            }
        };

        if (chkCanDrop != null) chkCanDrop.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.CanDrop = chkCanDrop.Checked ?? false;
            }
        };

        if (chkCanBank != null) chkCanBank.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.CanBank = chkCanBank.Checked ?? false;
            }
        };

        if (chkStackable != null) chkStackable.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Stackable = chkStackable.Checked ?? false;
            }
        };

        chk2Hand!.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.TwoHanded = chk2Hand.Checked ?? false;
            }
        };

        if (btnEditRequirements != null) btnEditRequirements.Click += (s, e) =>
        {
            if (mEditorItem != null)
            {
                var frm = new FrmDynamicRequirements(mEditorItem.UsageRequirements, RequirementType.Item);
                frm.ShowModal(this);
            }
        };
    }

    private void InitializeForm()
    {
        // Initialize combo boxes
        cmbEquipmentSlot!.Items.Clear();
        foreach (var slot in Options.Instance.Equipment.Slots)
        {
            cmbEquipmentSlot.Items.Add(slot);
        }

        cmbToolType!.Items.Clear();
        cmbToolType.Items.Add(Strings.General.None);
        foreach (var toolType in Options.Instance.Equipment.ToolTypes)
        {
            cmbToolType.Items.Add(toolType);
        }

        cmbProjectile!.Items.Clear();
        cmbProjectile.Items.Add(Strings.General.None);
        foreach (var name in ProjectileDescriptor.Names)
        {
            cmbProjectile.Items.Add(name);
        }

        // Initialize item list
        InitEditor();
    }

    private void AssignEditorItem(Guid id)
    {
        mEditorItem = ItemDescriptor.Get(id);
        UpdateEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Item)
        {
            InitEditor();
            if (mEditorItem != null && !ItemDescriptor.Lookup.Values.Contains(mEditorItem))
            {
                mEditorItem = null;
                UpdateEditor();
            }
        }
        else if (type == GameObjectType.Class ||
                 type == GameObjectType.Projectile ||
                 type == GameObjectType.Animation ||
                 type == GameObjectType.Spell)
        {
            InitializeForm();
        }
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        foreach (var item in mChanged)
        {
            item.RestoreBackup();
            item.DeleteBackup();
        }

        Close();
        Globals.CurrentEditor = -1;
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        foreach (var item in mChanged)
        {
            PacketSender.SendSaveObject(item);
            item.DeleteBackup();
        }

        Close();
        Globals.CurrentEditor = -1;
    }

    private void UpdateEditor()
    {
        if (mEditorItem != null)
        {
            pnlContainer!.Visible = true;

            txtName!.Text = mEditorItem.Name;
            // cmbFolder!.Text = mEditorItem.Folder; // DropDown doesn't have Text setter in Eto
            txtDesc!.Text = mEditorItem.Description;
            cmbType!.SelectedIndex = (int)mEditorItem.ItemType;
            nudPrice!.Value = mEditorItem.Price;

            if (mEditorItem.Rarity < cmbRarity!.Items.Count)
            {
                cmbRarity.SelectedIndex = mEditorItem.Rarity;
            }

            nudStr!.Value = mEditorItem.StatsGiven[0];
            nudMag!.Value = mEditorItem.StatsGiven[1];
            nudDef!.Value = mEditorItem.StatsGiven[2];
            nudMR!.Value = mEditorItem.StatsGiven[3];
            nudSpd!.Value = mEditorItem.StatsGiven[4];

            nudHealthBonus!.Value = mEditorItem.VitalsGiven[0];
            nudManaBonus!.Value = mEditorItem.VitalsGiven[1];
            nudHPRegen!.Value = mEditorItem.VitalsRegen[0];
            nudMpRegen!.Value = mEditorItem.VitalsRegen[1];

            nudDamage!.Value = mEditorItem.Damage;
            nudCritChance!.Value = mEditorItem.CritChance;
            nudCritMultiplier!.Value = mEditorItem.CritMultiplier;
            nudScaling!.Value = mEditorItem.Scaling;

            chkCanDrop!.Checked = Convert.ToBoolean(mEditorItem.CanDrop);
            chkCanBank!.Checked = Convert.ToBoolean(mEditorItem.CanBank);
            chkCanGuildBank!.Checked = Convert.ToBoolean(mEditorItem.CanGuildBank);
            chkCanBag!.Checked = Convert.ToBoolean(mEditorItem.CanBag);
            chkCanSell!.Checked = Convert.ToBoolean(mEditorItem.CanSell);
            chkCanTrade!.Checked = Convert.ToBoolean(mEditorItem.CanTrade);
            chkStackable!.Checked = Convert.ToBoolean(mEditorItem.Stackable);

            nudInvStackLimit!.Value = mEditorItem.MaxInventoryStack;
            nudBankStackLimit!.Value = mEditorItem.MaxBankStack;
            nudDeathDropChance!.Value = mEditorItem.DropChanceOnDeath;
            nudItemDespawnTime!.Value = mEditorItem.DespawnTime;

            cmbToolType!.SelectedIndex = mEditorItem.Tool + 1;
            chk2Hand!.Checked = mEditorItem.TwoHanded;

            nudBlockChance!.Value = mEditorItem.BlockChance;
            nudBlockAmount!.Value = mEditorItem.BlockAmount;
            nudBlockDmgAbs!.Value = mEditorItem.BlockAbsorption;

            cmbDamageType!.SelectedIndex = mEditorItem.DamageType;
            cmbScalingStat!.SelectedIndex = mEditorItem.ScalingStat;

            cmbProjectile!.SelectedIndex = ProjectileDescriptor.ListIndex(mEditorItem.ProjectileId) + 1;
            cmbAnimation!.SelectedIndex = AnimationDescriptor.ListIndex(mEditorItem.AnimationId) + 1;

            nudCooldown!.Value = mEditorItem.Cooldown;
            // cmbCooldownGroup!.Text = mEditorItem.CooldownGroup; // DropDown doesn't have Text setter in Eto
            chkIgnoreGlobalCooldown!.Checked = mEditorItem.IgnoreGlobalCooldown;
            chkIgnoreCdr!.Checked = mEditorItem.IgnoreCooldownReduction;

            txtCannotUse!.Text = mEditorItem.CannotUseMessage;

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

    private void RefreshExtendedData()
    {
        if (mEditorItem == null) return;

        grpConsumable!.Visible = false;
        grpSpell!.Visible = false;
        grpEquipment!.Visible = false;
        grpEvent!.Visible = false;
        grpBags!.Visible = false;
        chkStackable!.Enabled = true;

        if (cmbType!.SelectedIndex == (int)ItemType.Consumable)
        {
            cmbConsume!.SelectedIndex = (int)mEditorItem.Consumable.Type;
            nudInterval!.Value = mEditorItem.Consumable.Value;
            nudIntervalPercentage!.Value = mEditorItem.Consumable.Percentage;
            grpConsumable.Visible = true;
        }
        else if (cmbType.SelectedIndex == (int)ItemType.Spell)
        {
            cmbTeachSpell!.SelectedIndex = SpellDescriptor.ListIndex(mEditorItem.SpellId) + 1;
            chkQuickCast!.Checked = mEditorItem.QuickCast;
            chkSingleUseSpell!.Checked = mEditorItem.SingleUse;
            grpSpell.Visible = true;
        }
        else if (cmbType.SelectedIndex == (int)ItemType.Event)
        {
            cmbEvent!.SelectedIndex = Intersect.Framework.Core.GameObjects.Events.EventDescriptor.ListIndex(mEditorItem.EventId) + 1;
            chkSingleUseEvent!.Checked = mEditorItem.SingleUse;
            grpEvent.Visible = true;
        }
        else if (cmbType.SelectedIndex == (int)ItemType.Equipment)
        {
            grpEquipment.Visible = true;
            cmbEquipmentSlot!.SelectedIndex = mEditorItem.EquipmentSlot;
            chkStackable.Checked = false;
            chkStackable.Enabled = false;
        }
        else if (cmbType.SelectedIndex == (int)ItemType.Bag)
        {
            mEditorItem.SlotCount = Math.Max(1, mEditorItem.SlotCount);
            grpBags.Visible = true;
            nudBag!.Value = mEditorItem.SlotCount;
            chkStackable.Checked = false;
            chkStackable.Enabled = false;
        }
        else if (cmbType.SelectedIndex == (int)ItemType.Currency)
        {
            chkStackable.Checked = true;
            chkStackable.Enabled = false;
        }

        mEditorItem.ItemType = (ItemType)cmbType.SelectedIndex;
    }

    public void InitEditor()
    {
        var mFolders = new List<string>();
        foreach (var itm in ItemDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((ItemDescriptor)itm.Value).Folder) &&
                !mFolders.Contains(((ItemDescriptor)itm.Value).Folder))
            {
                mFolders.Add(((ItemDescriptor)itm.Value).Folder);
                if (!mKnownFolders.Contains(((ItemDescriptor)itm.Value).Folder))
                {
                    mKnownFolders.Add(((ItemDescriptor)itm.Value).Folder);
                }
            }

            if (!string.IsNullOrWhiteSpace(((ItemDescriptor)itm.Value).CooldownGroup) &&
                !mKnownCooldownGroups.Contains(((ItemDescriptor)itm.Value).CooldownGroup))
            {
                mKnownCooldownGroups.Add(((ItemDescriptor)itm.Value).CooldownGroup);
            }
        }

        if (Options.Instance.Combat.LinkSpellAndItemCooldowns)
        {
            foreach (var itm in SpellDescriptor.Lookup)
            {
                if (!string.IsNullOrWhiteSpace(((SpellDescriptor)itm.Value).CooldownGroup) &&
                    !mKnownCooldownGroups.Contains(((SpellDescriptor)itm.Value).CooldownGroup))
                {
                    mKnownCooldownGroups.Add(((SpellDescriptor)itm.Value).CooldownGroup);
                }
            }
        }

        mKnownCooldownGroups.Sort();
        cmbCooldownGroup!.Items.Clear();
        cmbCooldownGroup.Items.Add(string.Empty);
        foreach (var group in mKnownCooldownGroups)
        {
            cmbCooldownGroup.Items.Add(group);
        }

        mFolders.Sort();
        mKnownFolders.Sort();
        cmbFolder!.Items.Clear();
        cmbFolder.Items.Add("");
        foreach (var folder in mKnownFolders)
        {
            cmbFolder.Items.Add(folder);
        }

        // Populate item list
        if (lstGameObjects != null)
        {
            lstGameObjects.Items.Clear();
            var items = ItemDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var item = (ItemDescriptor?)pair.Value;
                if (item != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = item.Name ?? "Deleted" });
                }
            }
        }
    }
}
