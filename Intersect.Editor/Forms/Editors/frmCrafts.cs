using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Crafting;
using Intersect.Framework.Core.GameObjects.Events;
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.GameObjects;
using Intersect.Models;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmCrafts : EditorForm
{
    private List<CraftingRecipeDescriptor> mChanged = new();
    private string? mCopiedItem;
    private CraftingRecipeDescriptor? mEditorItem;
    private List<string> mKnownFolders = new();

    protected ListBox? lstGameObjects;
    protected ListBox? lstIngredients;
    protected TextBox? txtName;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbResult;
    protected DropDown? cmbIngredient;
    protected DropDown? cmbEvent;
    protected NumericStepper? nudSpeed;
    protected NumericStepper? nudFailureChance;
    protected NumericStepper? nudItemLossChance;
    protected NumericStepper? nudCraftQuantity;
    protected NumericStepper? nudQuantity;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnAdd;
    protected Button? btnRemove;
    protected Button? btnDupIngredient;
    protected Button? btnCraftRequirements;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected GroupBox? grpCrafts;
    protected GroupBox? grpGeneral;
    protected GroupBox? grpIngredients;
    protected Panel? pnlContainer;
    protected Label? lblName;
    protected Label? lblItem;
    protected Label? lblCraftQuantity;
    protected Label? lblSpeed;
    protected Label? lblFailureChance;
    protected Label? lblItemLossChance;
    protected Label? lblIngredient;
    protected Label? lblQuantity;
    protected Label? lblCommonEvent;
    protected Label? lblFolder;

    public FrmCrafts()
    {
        ApplyHooks();
        BuildUI();
        InitializeForm();
    }

    private void BuildUI()
    {
        Title = Strings.CraftsEditor.title;
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
        txtSearch = new TextBox { PlaceholderText = Strings.CraftsEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.CraftsEditor.folderlabel };

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
                new Button { Text = Strings.CraftsEditor.New },
                new Button { Text = Strings.CraftsEditor.delete },
                new Button { Text = Strings.CraftsEditor.copy },
                new Button { Text = Strings.CraftsEditor.paste },
                new Button { Text = Strings.CraftsEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.CraftsEditor.save };
        btnCancel = new Button { Text = Strings.CraftsEditor.cancel };
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
        cmbResult = new DropDown();
        cmbIngredient = new DropDown();
        cmbEvent = new DropDown();

        nudSpeed = new NumericStepper { MinValue = 0 };
        nudFailureChance = new NumericStepper { MinValue = 0, MaxValue = 100 };
        nudItemLossChance = new NumericStepper { MinValue = 0, MaxValue = 100 };
        nudCraftQuantity = new NumericStepper { MinValue = 1 };
        nudQuantity = new NumericStepper { MinValue = 1 };

        lstIngredients = new ListBox();

        btnAdd = new Button { Text = Strings.CraftsEditor.newingredient };
        btnRemove = new Button { Text = Strings.CraftsEditor.deleteingredient };
        btnDupIngredient = new Button { Text = Strings.CraftsEditor.duplicateingredient };
        btnCraftRequirements = new Button { Text = Strings.CraftsEditor.Requirements };

        grpCrafts = new GroupBox { Text = Strings.CraftsEditor.crafts };
        grpGeneral = new GroupBox { Text = Strings.CraftsEditor.general };
        grpIngredients = new GroupBox { Text = Strings.CraftsEditor.ingredients };

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
                mEditorItem.Time = (int)nudSpeed.Value;
            }
        };

        nudFailureChance!.ValueChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.FailureChance = (int)nudFailureChance.Value;
            }
        };

        nudItemLossChance!.ValueChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.ItemLossChance = (int)nudItemLossChance.Value;
            }
        };

        if (btnCraftRequirements != null) btnCraftRequirements.Click += (s, e) =>
        {
            if (mEditorItem != null)
            {
                var frm = new FrmDynamicRequirements(mEditorItem.CraftingRequirements, RequirementType.Craft);
                frm.ShowModal(this);
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
        cmbResult!.Items.Clear();
        cmbResult.Items.Add(Strings.General.None);
        foreach (var name in ItemDescriptor.Names)
        {
            cmbResult.Items.Add(name);
        }

        cmbIngredient!.Items.Clear();
        cmbIngredient.Items.Add(Strings.General.None);
        foreach (var name in ItemDescriptor.Names)
        {
            cmbIngredient.Items.Add(name);
        }

        cmbEvent!.Items.Clear();
        cmbEvent.Items.Add(Strings.General.None);
        foreach (var name in Intersect.Framework.Core.GameObjects.Events.EventDescriptor.Names)
        {
            cmbEvent.Items.Add(name);
        }

        InitEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Crafts)
        {
            InitEditor();
            if (mEditorItem != null && !DatabaseObject<CraftingRecipeDescriptor>.Lookup.Values.Contains(mEditorItem))
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
            nudSpeed!.Value = mEditorItem.Time;
            nudFailureChance!.Value = mEditorItem.FailureChance;
            nudItemLossChance!.Value = mEditorItem.ItemLossChance;
            cmbResult!.SelectedIndex = ItemDescriptor.ListIndex(mEditorItem.ItemId) + 1;
            nudCraftQuantity!.Value = mEditorItem.Quantity;
            cmbEvent!.SelectedIndex = Intersect.Framework.Core.GameObjects.Events.EventDescriptor.ListIndex(mEditorItem.EventId) + 1;

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
        foreach (var itm in CraftingRecipeDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((CraftingRecipeDescriptor)itm.Value).Folder) &&
                !mFolders.Contains(((CraftingRecipeDescriptor)itm.Value).Folder))
            {
                mFolders.Add(((CraftingRecipeDescriptor)itm.Value).Folder);
                if (!mKnownFolders.Contains(((CraftingRecipeDescriptor)itm.Value).Folder))
                {
                    mKnownFolders.Add(((CraftingRecipeDescriptor)itm.Value).Folder);
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
            var items = CraftingRecipeDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var craft = (CraftingRecipeDescriptor?)pair.Value;
                if (craft != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = craft.Name ?? "Deleted" });
                }
            }
        }
    }
}
