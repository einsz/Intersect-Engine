using Intersect.Editor.Forms.Helpers;
using System.ComponentModel;
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
using Intersect.Framework.Core.GameObjects.NPCs;
using Intersect.GameObjects;
using Intersect.Utilities;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmNpc : EditorForm
{
    private List<NPCDescriptor> mChanged = new();
    private string? mCopiedItem;
    private NPCDescriptor? mEditorItem;
    private List<string> mKnownFolders = new();
    private BindingList<NotifiableDrop> _dropList = new();

    // Eto.Forms controls
    protected ListBox? lstGameObjects;
    protected TextBox? txtName;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbSprite;
    protected DropDown? cmbMovement;
    protected DropDown? cmbSpell;
    protected DropDown? cmbFreq;
    protected DropDown? cmbHostileNPC;
    protected DropDown? cmbDropItem;
    protected DropDown? cmbAttackAnimation;
    protected DropDown? cmbOnDeathEventKiller;
    protected DropDown? cmbOnDeathEventParty;
    protected DropDown? cmbDamageType;
    protected DropDown? cmbScalingStat;
    protected DropDown? cmbAttackSpeedModifier;
    protected NumericStepper? nudLevel;
    protected NumericStepper? nudSpawnDuration;
    protected NumericStepper? nudSightRange;
    protected NumericStepper? nudFlee;
    protected NumericStepper? nudResetRadius;
    protected NumericStepper? nudHp;
    protected NumericStepper? nudMana;
    protected NumericStepper? nudStr;
    protected NumericStepper? nudMag;
    protected NumericStepper? nudDef;
    protected NumericStepper? nudMR;
    protected NumericStepper? nudSpd;
    protected NumericStepper? nudExp;
    protected NumericStepper? nudHpRegen;
    protected NumericStepper? nudMpRegen;
    protected NumericStepper? nudDamage;
    protected NumericStepper? nudCritChance;
    protected NumericStepper? nudCritMultiplier;
    protected NumericStepper? nudScaling;
    protected NumericStepper? nudAttackSpeedValue;
    protected NumericStepper? nudDropMaxAmount;
    protected NumericStepper? nudDropMinAmount;
    protected NumericStepper? nudDropChance;
    protected NumericStepper? nudTenacity;
    protected NumericStepper? nudRgbaR;
    protected NumericStepper? nudRgbaG;
    protected NumericStepper? nudRgbaB;
    protected NumericStepper? nudRgbaA;
    protected CheckBox? chkAggressive;
    protected CheckBox? chkSwarm;
    protected CheckBox? chkFocusDamageDealer;
    protected CheckBox? chkEnabled;
    protected CheckBox? chkAttackAllies;
    protected CheckBox? chkIndividualLoot;
    protected CheckBox? chkKnockback;
    protected CheckBox? chkSilence;
    protected CheckBox? chkStun;
    protected CheckBox? chkSnare;
    protected CheckBox? chkBlind;
    protected CheckBox? chkTransform;
    protected CheckBox? chkTaunt;
    protected CheckBox? chkSleep;
    protected ListBox? lstSpells;
    protected ListBox? lstAggro;
    protected ListBox? lstDrops;
    protected Button? btnAdd;
    protected Button? btnRemove;
    protected Button? btnAddAggro;
    protected Button? btnRemoveAggro;
    protected Button? btnDropAdd;
    protected Button? btnDropRemove;
    protected Button? btnPlayerFriendProtectorCond;
    protected Button? btnAttackOnSightCond;
    protected Button? btnPlayerCanAttackCond;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected CheckBox? btnAlphabetical;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected GroupBox? grpNpcs;
    protected GroupBox? grpGeneral;
    protected GroupBox? grpBehavior;
    protected GroupBox? grpStats;
    protected GroupBox? grpCombat;
    protected GroupBox? grpRegen;
    protected GroupBox? grpSpells;
    protected GroupBox? grpNpcVsNpc;
    protected GroupBox? grpDrops;
    protected GroupBox? grpConditions;
    protected GroupBox? grpCommonEvents;
    protected GroupBox? grpAttackSpeed;
    protected GroupBox? grpImmunities;
    protected Panel? pnlContainer;
    protected Label? lblName;
    protected Label? lblPic;
    protected Label? lblLevel;
    protected Label? lblSpawnDuration;
    protected Label? lblSightRange;
    protected Label? lblMovement;
    protected Label? lblResetRadius;
    protected Label? lblFlee;
    protected Label? lblHP;
    protected Label? lblMana;
    protected Label? lblStr;
    protected Label? lblMag;
    protected Label? lblDef;
    protected Label? lblMR;
    protected Label? lblSpd;
    protected Label? lblExp;
    protected Label? lblHpRegen;
    protected Label? lblManaRegen;
    protected Label? lblRegenHint;
    protected Label? lblSpell;
    protected Label? lblFreq;
    protected Label? lblNPC;
    protected Label? lblDropItem;
    protected Label? lblDropMaxAmount;
    protected Label? lblDropMinAmount;
    protected Label? lblDropChance;
    protected Label? lblDamage;
    protected Label? lblCritChance;
    protected Label? lblCritMultiplier;
    protected Label? lblDamageType;
    protected Label? lblScalingStat;
    protected Label? lblScaling;
    protected Label? lblAttackAnimation;
    protected Label? lblAttackSpeedModifier;
    protected Label? lblAttackSpeedValue;
    protected Label? lblOnDeathEventKiller;
    protected Label? lblOnDeathEventParty;
    protected Label? lblTenacity;
    protected Label? lblFolder;
    protected Label? lblRed;
    protected Label? lblGreen;
    protected Label? lblBlue;
    protected Label? lblAlpha;

    public FrmNpc()
    {
        ApplyHooks();
        BuildUI();
        InitializeForm();
    }

    private void BuildUI()
    {
        Title = Strings.NpcEditor.title;
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
        txtSearch = new TextBox { PlaceholderText = Strings.NpcEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.NpcEditor.folderlabel };

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
                new Button { Text = Strings.NpcEditor.New },
                new Button { Text = Strings.NpcEditor.delete },
                new Button { Text = Strings.NpcEditor.copy },
                new Button { Text = Strings.NpcEditor.paste },
                new Button { Text = Strings.NpcEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.NpcEditor.save };
        btnCancel = new Button { Text = Strings.NpcEditor.cancel };
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
        cmbSprite = new DropDown();
        cmbMovement = new DropDown();
        cmbSpell = new DropDown();
        cmbFreq = new DropDown();
        cmbHostileNPC = new DropDown();
        cmbDropItem = new DropDown();
        cmbAttackAnimation = new DropDown();
        cmbOnDeathEventKiller = new DropDown();
        cmbOnDeathEventParty = new DropDown();
        cmbDamageType = new DropDown();
        cmbScalingStat = new DropDown();
        cmbAttackSpeedModifier = new DropDown();

        nudLevel = new NumericStepper { MinValue = 1 };
        nudSpawnDuration = new NumericStepper { MinValue = 0 };
        nudSightRange = new NumericStepper { MinValue = 0 };
        nudFlee = new NumericStepper { MinValue = 0, MaxValue = 100 };
        nudResetRadius = new NumericStepper { MinValue = 0 };
        nudHp = new NumericStepper { MinValue = 0 };
        nudMana = new NumericStepper { MinValue = 0 };
        nudStr = new NumericStepper { MinValue = 0, MaxValue = Options.Instance.Player.MaxStat };
        nudMag = new NumericStepper { MinValue = 0, MaxValue = Options.Instance.Player.MaxStat };
        nudDef = new NumericStepper { MinValue = 0, MaxValue = Options.Instance.Player.MaxStat };
        nudMR = new NumericStepper { MinValue = 0, MaxValue = Options.Instance.Player.MaxStat };
        nudSpd = new NumericStepper { MinValue = 0, MaxValue = Options.Instance.Player.MaxStat };
        nudExp = new NumericStepper { MinValue = 0 };
        nudHpRegen = new NumericStepper();
        nudMpRegen = new NumericStepper();
        nudDamage = new NumericStepper { MinValue = 0 };
        nudCritChance = new NumericStepper { MinValue = 0, MaxValue = 100 };
        nudCritMultiplier = new NumericStepper { MinValue = 0, DecimalPlaces = 2 };
        nudScaling = new NumericStepper { MinValue = 0 };
        nudAttackSpeedValue = new NumericStepper { MinValue = 0 };
        nudDropMaxAmount = new NumericStepper { MinValue = 1 };
        nudDropMinAmount = new NumericStepper { MinValue = 1 };
        nudDropChance = new NumericStepper { MinValue = 0, MaxValue = 100, DecimalPlaces = 2 };
        nudTenacity = new NumericStepper { MinValue = 0, DecimalPlaces = 2 };
        nudRgbaR = new NumericStepper { MinValue = 0, MaxValue = 255 };
        nudRgbaG = new NumericStepper { MinValue = 0, MaxValue = 255 };
        nudRgbaB = new NumericStepper { MinValue = 0, MaxValue = 255 };
        nudRgbaA = new NumericStepper { MinValue = 0, MaxValue = 255 };

        chkAggressive = new CheckBox { Text = Strings.NpcEditor.aggressive };
        chkSwarm = new CheckBox { Text = Strings.NpcEditor.swarm };
        chkFocusDamageDealer = new CheckBox { Text = Strings.NpcEditor.focusdamagedealer };
        chkEnabled = new CheckBox { Text = Strings.NpcEditor.enabled };
        chkAttackAllies = new CheckBox { Text = Strings.NpcEditor.attackallies };
        chkIndividualLoot = new CheckBox { Text = Strings.NpcEditor.individualizedloot };
        chkKnockback = new CheckBox { Text = Strings.NpcEditor.Immunities[SpellEffect.Knockback] };
        chkSilence = new CheckBox { Text = Strings.NpcEditor.Immunities[SpellEffect.Silence] };
        chkStun = new CheckBox { Text = Strings.NpcEditor.Immunities[SpellEffect.Stun] };
        chkSnare = new CheckBox { Text = Strings.NpcEditor.Immunities[SpellEffect.Snare] };
        chkBlind = new CheckBox { Text = Strings.NpcEditor.Immunities[SpellEffect.Blind] };
        chkTransform = new CheckBox { Text = Strings.NpcEditor.Immunities[SpellEffect.Transform] };
        chkTaunt = new CheckBox { Text = Strings.NpcEditor.Immunities[SpellEffect.Taunt] };
        chkSleep = new CheckBox { Text = Strings.NpcEditor.Immunities[SpellEffect.Sleep] };

        lstSpells = new ListBox();
        lstAggro = new ListBox();
        lstDrops = new ListBox();

        btnAdd = new Button { Text = Strings.NpcEditor.addspell };
        btnRemove = new Button { Text = Strings.NpcEditor.removespell };
        btnAddAggro = new Button { Text = Strings.NpcEditor.addhostility };
        btnRemoveAggro = new Button { Text = Strings.NpcEditor.removehostility };
        btnDropAdd = new Button { Text = Strings.NpcEditor.dropadd };
        btnDropRemove = new Button { Text = Strings.NpcEditor.dropremove };
        btnPlayerFriendProtectorCond = new Button { Text = Strings.NpcEditor.playerfriendprotectorconditions };
        btnAttackOnSightCond = new Button { Text = Strings.NpcEditor.attackonsightconditions };
        btnPlayerCanAttackCond = new Button { Text = Strings.NpcEditor.playercanattackconditions };

        grpNpcs = new GroupBox { Text = Strings.NpcEditor.npcs };
        grpGeneral = new GroupBox { Text = Strings.NpcEditor.general };
        grpBehavior = new GroupBox { Text = Strings.NpcEditor.behavior };
        grpStats = new GroupBox { Text = Strings.NpcEditor.stats };
        grpCombat = new GroupBox { Text = Strings.NpcEditor.combat };
        grpRegen = new GroupBox { Text = Strings.NpcEditor.regen };
        grpSpells = new GroupBox { Text = Strings.NpcEditor.spells };
        grpNpcVsNpc = new GroupBox { Text = Strings.NpcEditor.npcvsnpc };
        grpDrops = new GroupBox { Text = Strings.NpcEditor.drops };
        grpConditions = new GroupBox { Text = Strings.NpcEditor.conditions };
        grpCommonEvents = new GroupBox { Text = Strings.NpcEditor.commonevents };
        grpAttackSpeed = new GroupBox { Text = Strings.NpcEditor.attackspeed };
        grpImmunities = new GroupBox { Text = Strings.NpcEditor.ImmunitiesTitle };

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

        if (chkAggressive != null) chkAggressive.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Aggressive = chkAggressive.Checked ?? false;
            }
        };

        if (btnSave != null) btnSave.Click += (s, e) =>
        {
            foreach (var item in mChanged)
            {
                item.Immunities.Sort();
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
        cmbSprite!.Items.Clear();
        cmbSprite.Items.Add(Strings.General.None);
        foreach (var name in GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Entity))
        {
            cmbSprite.Items.Add(name);
        }

        cmbSpell!.Items.Clear();
        foreach (var name in SpellDescriptor.Names)
        {
            cmbSpell.Items.Add(name);
        }

        cmbHostileNPC!.Items.Clear();
        foreach (var name in NPCDescriptor.Names)
        {
            cmbHostileNPC.Items.Add(name);
        }

        cmbDropItem!.Items.Clear();
        cmbDropItem.Items.Add(Strings.General.None);
        foreach (var name in ItemDescriptor.Names)
        {
            cmbDropItem.Items.Add(name);
        }

        cmbAttackAnimation!.Items.Clear();
        cmbAttackAnimation.Items.Add(Strings.General.None);
        foreach (var name in AnimationDescriptor.Names)
        {
            cmbAttackAnimation.Items.Add(name);
        }

        cmbOnDeathEventKiller!.Items.Clear();
        cmbOnDeathEventKiller.Items.Add(Strings.General.None);
        foreach (var name in Intersect.Framework.Core.GameObjects.Events.EventDescriptor.Names)
        {
            cmbOnDeathEventKiller.Items.Add(name);
        }

        cmbOnDeathEventParty!.Items.Clear();
        cmbOnDeathEventParty.Items.Add(Strings.General.None);
        foreach (var name in Intersect.Framework.Core.GameObjects.Events.EventDescriptor.Names)
        {
            cmbOnDeathEventParty.Items.Add(name);
        }

        cmbScalingStat!.Items.Clear();
        for (var x = 0; x < Enum.GetValues<Stat>().Length; x++)
        {
            cmbScalingStat.Items.Add(Globals.GetStatName(x));
        }

        cmbMovement!.Items.Clear();
        foreach (var movement in Strings.NpcEditor.movements)
        {
            cmbMovement.Items.Add(movement.Value.ToString());
        }

        cmbFreq!.Items.Clear();
        foreach (var freq in Strings.NpcEditor.frequencies)
        {
            cmbFreq.Items.Add(freq.Value.ToString());
        }

        cmbDamageType!.Items.Clear();
        foreach (var dmgType in Strings.Combat.damagetypes)
        {
            cmbDamageType.Items.Add(dmgType.Value.ToString());
        }

        cmbAttackSpeedModifier!.Items.Clear();
        foreach (var val in Strings.NpcEditor.attackspeedmodifiers.Values)
        {
            cmbAttackSpeedModifier.Items.Add(val.ToString());
        }

        InitEditor();
    }

    private void AssignEditorItem(Guid id)
    {
        mEditorItem = NPCDescriptor.Get(id);
        UpdateEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Npc)
        {
            InitEditor();
            if (mEditorItem != null && !NPCDescriptor.Lookup.Values.Contains(mEditorItem))
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
            cmbSprite!.SelectedIndex = cmbSprite.Items.IndexOf(
                new ListItem { Text = TextUtils.NullToNone(mEditorItem.Sprite) }
            );

            nudLevel!.Value = mEditorItem.Level;
            nudSpawnDuration!.Value = mEditorItem.SpawnDuration;

            chkAggressive!.Checked = mEditorItem.Aggressive;
            nudSightRange!.Value = mEditorItem.SightRange;
            cmbMovement!.SelectedIndex = Math.Min(mEditorItem.Movement, cmbMovement.Items.Count - 1);
            chkSwarm!.Checked = mEditorItem.Swarm;
            nudFlee!.Value = mEditorItem.FleeHealthPercentage;
            chkFocusDamageDealer!.Checked = mEditorItem.FocusHighestDamageDealer;
            nudResetRadius!.Value = mEditorItem.ResetRadius;

            cmbOnDeathEventKiller!.SelectedIndex = Intersect.Framework.Core.GameObjects.Events.EventDescriptor.ListIndex(mEditorItem.OnDeathEventId) + 1;
            cmbOnDeathEventParty!.SelectedIndex = Intersect.Framework.Core.GameObjects.Events.EventDescriptor.ListIndex(mEditorItem.OnDeathPartyEventId) + 1;

            nudStr!.Value = mEditorItem.Stats[(int)Stat.Attack];
            nudMag!.Value = mEditorItem.Stats[(int)Stat.AbilityPower];
            nudDef!.Value = mEditorItem.Stats[(int)Stat.Defense];
            nudMR!.Value = mEditorItem.Stats[(int)Stat.MagicResist];
            nudSpd!.Value = mEditorItem.Stats[(int)Stat.Speed];
            nudHp!.Value = mEditorItem.MaxVitals[(int)Vital.Health];
            nudMana!.Value = mEditorItem.MaxVitals[(int)Vital.Mana];
            nudExp!.Value = mEditorItem.Experience;
            chkAttackAllies!.Checked = mEditorItem.AttackAllies;
            chkEnabled!.Checked = mEditorItem.NpcVsNpcEnabled;

            nudDamage!.Value = mEditorItem.Damage;
            nudCritChance!.Value = mEditorItem.CritChance;
            nudCritMultiplier!.Value = mEditorItem.CritMultiplier;
            nudScaling!.Value = mEditorItem.Scaling;
            cmbDamageType!.SelectedIndex = mEditorItem.DamageType;
            cmbScalingStat!.SelectedIndex = mEditorItem.ScalingStat;
            cmbAttackAnimation!.SelectedIndex = AnimationDescriptor.ListIndex(mEditorItem.AttackAnimationId) + 1;
            cmbAttackSpeedModifier!.SelectedIndex = mEditorItem.AttackSpeedModifier;
            nudAttackSpeedValue!.Value = mEditorItem.AttackSpeedValue;

            nudHpRegen!.Value = mEditorItem.VitalRegen[(int)Vital.Health];
            nudMpRegen!.Value = mEditorItem.VitalRegen[(int)Vital.Mana];

            chkIndividualLoot!.Checked = mEditorItem.IndividualizedLoot;
            nudTenacity!.Value = mEditorItem.Tenacity;

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
        foreach (var itm in NPCDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((NPCDescriptor)itm.Value).Folder) &&
                !mFolders.Contains(((NPCDescriptor)itm.Value).Folder))
            {
                mFolders.Add(((NPCDescriptor)itm.Value).Folder);
                if (!mKnownFolders.Contains(((NPCDescriptor)itm.Value).Folder))
                {
                    mKnownFolders.Add(((NPCDescriptor)itm.Value).Folder);
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
            var items = NPCDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var npc = (NPCDescriptor?)pair.Value;
                if (npc != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = npc.Name ?? "Deleted" });
                }
            }
        }
    }
}
