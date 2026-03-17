using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Resources;
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.Framework.Core.GameObjects.Animations;
using Intersect.Utilities;
using EventDescriptor = Intersect.Framework.Core.GameObjects.Events.EventDescriptor;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmResource : EditorForm
{
    private readonly List<string> _knownFolders = new();
    private readonly Dictionary<Guid, ResourceDescriptor> _changed = new();
    private string? _copiedItem;
    private ResourceDescriptor? _editorItem;

    protected ListBox? lstGameObjects;
    protected ListBox? lstDrops;
    protected ListBox? lstStates;
    protected TextBox? txtName;
    protected TextBox? txtCannotHarvest;
    protected TextBox? txtStateName;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbToolType;
    protected DropDown? cmbDeathAnimation;
    protected DropDown? cmbDropItem;
    protected DropDown? cmbEvent;
    protected DropDown? cmbTextureType;
    protected DropDown? cmbTextureSource;
    protected DropDown? cmbAnimation;
    protected NumericStepper? nudMinHp;
    protected NumericStepper? nudMaxHp;
    protected NumericStepper? nudSpawnDuration;
    protected NumericStepper? nudHpRegen;
    protected NumericStepper? nudDropMaxAmount;
    protected NumericStepper? nudDropMinAmount;
    protected NumericStepper? nudDropChance;
    protected NumericStepper? nudStateRangeMin;
    protected NumericStepper? nudStateRangeMax;
    protected CheckBox? chkWalkableBefore;
    protected CheckBox? chkWalkableAfter;
    protected CheckBox? chkUseExplicitMaxHealthForResourceStates;
    protected CheckBox? chkRenderBelowEntity;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnRequirements;
    protected Button? btnDropAdd;
    protected Button? btnDropRemove;
    protected Button? btnAddState;
    protected Button? btnRemoveState;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected GroupBox? grpResources;
    protected GroupBox? grpGeneral;
    protected GroupBox? grpDrops;
    protected GroupBox? grpGraphics;
    protected GroupBox? grpGraphicData;
    protected GroupBox? grpCommonEvent;
    protected GroupBox? grpRequirements;
    protected Panel? pnlContainer;
    protected Panel? picResource;
    protected Label? lblName;
    protected Label? lblToolType;
    protected Label? lblHP;
    protected Label? lblMaxHp;
    protected Label? lblSpawnDuration;
    protected Label? lblDeathAnimation;
    protected Label? lblDropItem;
    protected Label? lblDropMaxAmount;
    protected Label? lblDropMinAmount;
    protected Label? lblDropChance;
    protected Label? lblHpRegen;
    protected Label? lblEvent;
    protected Label? lblCannotHarvest;
    protected Label? lblStates;
    protected Label? lblStateName;
    protected Label? lblTextureType;
    protected Label? lblTerxtureSource;
    protected Label? lblAnimation;
    protected Label? lblFolder;

    public FrmResource()
    {
        ApplyHooks();
        BuildUI();
        InitializeForm();
    }

    private void BuildUI()
    {
        Title = Strings.ResourceEditor.title;
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
        txtSearch = new TextBox { PlaceholderText = Strings.ResourceEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.ResourceEditor.folderlabel };

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
                new Button { Text = Strings.ResourceEditor.New },
                new Button { Text = Strings.ResourceEditor.delete },
                new Button { Text = Strings.ResourceEditor.copy },
                new Button { Text = Strings.ResourceEditor.paste },
                new Button { Text = Strings.ResourceEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.ResourceEditor.save };
        btnCancel = new Button { Text = Strings.ResourceEditor.cancel };
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
        txtCannotHarvest = new TextBox();
        txtStateName = new TextBox();
        cmbToolType = new DropDown();
        cmbDeathAnimation = new DropDown();
        cmbDropItem = new DropDown();
        cmbEvent = new DropDown();
        cmbTextureType = new DropDown();
        cmbTextureSource = new DropDown();
        cmbAnimation = new DropDown();

        nudMinHp = new NumericStepper { MinValue = 0 };
        nudMaxHp = new NumericStepper { MinValue = 0 };
        nudSpawnDuration = new NumericStepper { MinValue = 0 };
        nudHpRegen = new NumericStepper { MinValue = 0, MaxValue = 100 };
        nudDropMaxAmount = new NumericStepper { MinValue = 1 };
        nudDropMinAmount = new NumericStepper { MinValue = 1 };
        nudDropChance = new NumericStepper { MinValue = 0, MaxValue = 100, DecimalPlaces = 2 };
        nudStateRangeMin = new NumericStepper { MinValue = 0 };
        nudStateRangeMax = new NumericStepper { MinValue = 0 };

        chkWalkableBefore = new CheckBox { Text = Strings.ResourceEditor.walkablebefore };
        chkWalkableAfter = new CheckBox { Text = Strings.ResourceEditor.walkableafter };
        chkUseExplicitMaxHealthForResourceStates = new CheckBox { Text = Strings.ResourceEditor.UseExplicitMaxHealthForResourceStates };
        chkRenderBelowEntity = new CheckBox { Text = Strings.ResourceEditor.BelowEntities };

        lstDrops = new ListBox();
        lstStates = new ListBox();
        picResource = new Panel();

        btnRequirements = new Button { Text = Strings.ResourceEditor.requirements };
        btnDropAdd = new Button { Text = Strings.ResourceEditor.dropadd };
        btnDropRemove = new Button { Text = Strings.ResourceEditor.dropremove };
        btnAddState = new Button { Text = Strings.ResourceEditor.AddState };
        btnRemoveState = new Button { Text = Strings.ResourceEditor.RemoveState };

        grpResources = new GroupBox { Text = Strings.ResourceEditor.resources };
        grpGeneral = new GroupBox { Text = Strings.ResourceEditor.general };
        grpDrops = new GroupBox { Text = Strings.ResourceEditor.drops };
        grpGraphics = new GroupBox { Text = Strings.ResourceEditor.Appearance };
        grpGraphicData = new GroupBox { Text = Strings.ResourceEditor.StateProperties };
        grpCommonEvent = new GroupBox { Text = Strings.ResourceEditor.commonevent };
        grpRequirements = new GroupBox { Text = Strings.ResourceEditor.requirementsgroup };

        pnlContainer = new Panel();

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        txtName!.TextChanged += (s, e) =>
        {
            if (_editorItem != null)
            {
                _editorItem.Name = txtName.Text;
            }
        };

        if (chkWalkableBefore != null) chkWalkableBefore.CheckedChanged += (s, e) =>
        {
            if (_editorItem != null)
            {
                _editorItem.WalkableBefore = chkWalkableBefore.Checked ?? false;
            }
        };

        if (chkWalkableAfter != null) chkWalkableAfter.CheckedChanged += (s, e) =>
        {
            if (_editorItem != null)
            {
                _editorItem.WalkableAfter = chkWalkableAfter.Checked ?? false;
            }
        };

        if (btnRequirements != null) btnRequirements.Click += (s, e) =>
        {
            if (_editorItem != null)
            {
                var frm = new FrmDynamicRequirements(_editorItem.HarvestingRequirements, RequirementType.Resource);
                frm.ShowModal(this);
            }
        };

        if (btnSave != null) btnSave.Click += (s, e) =>
        {
            foreach (var item in _changed.Values)
            {
                PacketSender.SendSaveObject(item);
                item.DeleteBackup();
            }
            Close();
            Globals.CurrentEditor = -1;
        };

        if (btnCancel != null) btnCancel.Click += (s, e) =>
        {
            foreach (var item in _changed.Values)
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
        cmbToolType!.Items.Clear();
        cmbToolType.Items.Add(Strings.General.None);
        foreach (var toolType in Options.Instance.Equipment.ToolTypes)
        {
            cmbToolType.Items.Add(toolType);
        }

        cmbDeathAnimation!.Items.Clear();
        cmbDeathAnimation.Items.Add(Strings.General.None);
        foreach (var name in AnimationDescriptor.Names)
        {
            cmbDeathAnimation.Items.Add(name);
        }

        cmbDropItem!.Items.Clear();
        cmbDropItem.Items.Add(Strings.General.None);
        foreach (var name in ItemDescriptor.Names)
        {
            cmbDropItem.Items.Add(name);
        }

        cmbEvent!.Items.Clear();
        cmbEvent.Items.Add(Strings.General.None);
        foreach (var name in Intersect.Framework.Core.GameObjects.Events.EventDescriptor.Names)
        {
            cmbEvent.Items.Add(name);
        }

        cmbTextureType!.Items.Clear();
        foreach (var source in Strings.ResourceEditor.TextureSources.Values)
        {
            cmbTextureType.Items.Add(source);
        }

        InitEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Resource)
        {
            InitEditor();
            if (_editorItem != null && !ResourceDescriptor.Lookup.Values.Contains(_editorItem))
            {
                _editorItem = null;
                UpdateEditor();
            }
        }
    }

    private void UpdateEditor()
    {
        if (_editorItem != null)
        {
            pnlContainer!.Visible = true;

            txtName!.Text = _editorItem.Name;
            // cmbFolder!.Text = _editorItem.Folder; // DropDown doesn't have Text setter in Eto
            cmbToolType!.SelectedIndex = _editorItem.Tool + 1;
            nudSpawnDuration!.Value = _editorItem.SpawnDuration;
            cmbDeathAnimation!.SelectedIndex = AnimationDescriptor.ListIndex(_editorItem.DeathAnimationId) + 1;
            nudMinHp!.Value = _editorItem.MinHp;
            nudMaxHp!.Value = _editorItem.MaxHp;
            chkWalkableBefore!.Checked = _editorItem.WalkableBefore;
            chkWalkableAfter!.Checked = _editorItem.WalkableAfter;
            chkUseExplicitMaxHealthForResourceStates!.Checked = _editorItem.UseExplicitMaxHealthForResourceStates;
            cmbEvent!.SelectedIndex = Intersect.Framework.Core.GameObjects.Events.EventDescriptor.ListIndex(_editorItem.EventId) + 1;
            txtCannotHarvest!.Text = _editorItem.CannotHarvestMessage;
            nudHpRegen!.Value = _editorItem.VitalRegen;

            if (!_changed.TryGetValue(_editorItem.Id, out _))
            {
                _changed.Add(_editorItem.Id, _editorItem);
                _editorItem.MakeBackup();
            }
        }
        else
        {
            pnlContainer!.Visible = false;
        }

        UpdateEditorButtons(_editorItem != null);
    }

    public void InitEditor()
    {
        var mFolders = new List<string>();
        foreach (var itm in ResourceDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((ResourceDescriptor)itm.Value).Folder) &&
                !mFolders.Contains(((ResourceDescriptor)itm.Value).Folder))
            {
                mFolders.Add(((ResourceDescriptor)itm.Value).Folder);
                if (!_knownFolders.Contains(((ResourceDescriptor)itm.Value).Folder))
                {
                    _knownFolders.Add(((ResourceDescriptor)itm.Value).Folder);
                }
            }
        }

        mFolders.Sort();
        _knownFolders.Sort();
        cmbFolder!.Items.Clear();
        cmbFolder.Items.Add("");
        foreach (var folder in _knownFolders)
        {
            cmbFolder.Items.Add(folder);
        }

        if (lstGameObjects != null)
        {
            lstGameObjects.Items.Clear();
            var items = ResourceDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var resource = (ResourceDescriptor?)pair.Value;
                if (resource != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = resource.Name ?? "Deleted" });
                }
            }
        }
    }
}
