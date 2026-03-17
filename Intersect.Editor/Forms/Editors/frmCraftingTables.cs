using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Crafting;
using Intersect.GameObjects;
using Intersect.Models;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmCraftingTables : EditorForm
{
    private List<CraftingTableDescriptor> mChanged = new();
    private string? mCopiedItem;
    private CraftingTableDescriptor? mEditorItem;
    private List<string> mKnownFolders = new();

    protected ListBox? lstGameObjects;
    protected ListBox? lstCrafts;
    protected TextBox? txtName;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbCrafts;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnAddCraftedItem;
    protected Button? btnRemoveCraftedItem;
    protected Button? btnCraftUp;
    protected Button? btnCraftDown;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected GroupBox? grpTables;
    protected GroupBox? grpCrafts;
    protected GroupBox? grpGeneral;
    protected Panel? pnlContainer;
    protected Label? lblName;
    protected Label? lblAddCraftedItem;
    protected Label? lblFolder;

    public FrmCraftingTables()
    {
        ApplyHooks();
        BuildUI();
        InitializeForm();
    }

    private void BuildUI()
    {
        Title = Strings.CraftingTableEditor.title;
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
        txtSearch = new TextBox { PlaceholderText = Strings.CraftingTableEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.CraftingTableEditor.folderlabel };

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
                new Button { Text = Strings.CraftingTableEditor.New },
                new Button { Text = Strings.CraftingTableEditor.delete },
                new Button { Text = Strings.CraftingTableEditor.copy },
                new Button { Text = Strings.CraftingTableEditor.paste },
                new Button { Text = Strings.CraftingTableEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.CraftingTableEditor.save };
        btnCancel = new Button { Text = Strings.CraftingTableEditor.cancel };
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
        cmbCrafts = new DropDown();
        lstCrafts = new ListBox();

        btnAddCraftedItem = new Button { Text = Strings.CraftingTableEditor.add };
        btnRemoveCraftedItem = new Button { Text = Strings.CraftingTableEditor.remove };
        btnCraftUp = new Button { Text = "^" };
        btnCraftDown = new Button { Text = "v" };

        grpTables = new GroupBox { Text = Strings.CraftingTableEditor.tables };
        grpCrafts = new GroupBox { Text = Strings.CraftingTableEditor.crafts };
        grpGeneral = new GroupBox { Text = Strings.CraftingTableEditor.general };

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

        if (btnAddCraftedItem != null) btnAddCraftedItem.Click += (s, e) =>
        {
            if (mEditorItem != null && cmbCrafts!.SelectedIndex >= 0)
            {
                var id = CraftingRecipeDescriptor.IdFromList(cmbCrafts.SelectedIndex);
                var craft = CraftingRecipeDescriptor.Get(id);
                if (craft != null && !mEditorItem.Crafts.Contains(id))
                {
                    mEditorItem.Crafts.Add(id);
                    UpdateList();
                }
            }
        };

        if (btnRemoveCraftedItem != null) btnRemoveCraftedItem.Click += (s, e) =>
        {
            if (mEditorItem != null && lstCrafts!.SelectedIndex > -1)
            {
                mEditorItem.Crafts.RemoveAt(lstCrafts.SelectedIndex);
                UpdateList();
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
        cmbCrafts!.Items.Clear();
        foreach (var name in CraftingRecipeDescriptor.Names)
        {
            cmbCrafts.Items.Add(name);
        }

        InitEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.CraftTables)
        {
            InitEditor();
            if (mEditorItem != null && !DatabaseObject<CraftingTableDescriptor>.Lookup.Values.Contains(mEditorItem))
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
            UpdateList();

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

    public void UpdateList()
    {
        if (lstCrafts == null || mEditorItem == null) return;

        lstCrafts.Items.Clear();
        foreach (var id in mEditorItem.Crafts)
        {
            lstCrafts.Items.Add(CraftingRecipeDescriptor.GetName(id));
        }
    }

    public void InitEditor()
    {
        var mFolders = new List<string>();
        foreach (var itm in CraftingTableDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((CraftingTableDescriptor)itm.Value).Folder) &&
                !mFolders.Contains(((CraftingTableDescriptor)itm.Value).Folder))
            {
                mFolders.Add(((CraftingTableDescriptor)itm.Value).Folder);
                if (!mKnownFolders.Contains(((CraftingTableDescriptor)itm.Value).Folder))
                {
                    mKnownFolders.Add(((CraftingTableDescriptor)itm.Value).Folder);
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
            var items = CraftingTableDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var table = (CraftingTableDescriptor?)pair.Value;
                if (table != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = table.Name ?? "Deleted" });
                }
            }
        }
    }
}
