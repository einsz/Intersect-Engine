using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Content;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.GameObjects;
using Intersect.Utilities;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmShop : EditorForm
{
    private List<ShopDescriptor> mChanged = new();
    private string? mCopiedItem;
    private ShopDescriptor? mEditorItem;
    private List<string> mKnownFolders = new();

    protected ListBox? lstGameObjects;
    protected ListBox? lstSoldItems;
    protected ListBox? lstBoughtItems;
    protected TextBox? txtName;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbDefaultCurrency;
    protected DropDown? cmbAddSoldItem;
    protected DropDown? cmbSellFor;
    protected DropDown? cmbAddBoughtItem;
    protected DropDown? cmbBuyFor;
    protected DropDown? cmbBuySound;
    protected DropDown? cmbSellSound;
    protected NumericStepper? nudSellCost;
    protected NumericStepper? nudBuyAmount;
    protected RadioButton? rdoBuyWhitelist;
    protected RadioButton? rdoBuyBlacklist;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnAddSoldItem;
    protected Button? btnDelSoldItem;
    protected Button? btnAddBoughtItem;
    protected Button? btnDelBoughtItem;
    protected Button? btnItemUp;
    protected Button? btnItemDown;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected GroupBox? grpGeneral;
    protected GroupBox? grpItemsSold;
    protected GroupBox? grpItemsBought;
    protected Panel? pnlContainer;
    protected Label? lblName;
    protected Label? lblDefaultCurrency;
    protected Label? lblAddSoldItem;
    protected Label? lblSellFor;
    protected Label? lblSellCost;
    protected Label? lblItemBought;
    protected Label? lblBuyFor;
    protected Label? lblBuyAmount;
    protected Label? lblBuySound;
    protected Label? lblSellSound;
    protected Label? lblFolder;

    public FrmShop()
    {
        ApplyHooks();
        BuildUI();
        InitializeForm();
    }

    private void BuildUI()
    {
        Title = Strings.ShopEditor.title;
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
        txtSearch = new TextBox { PlaceholderText = Strings.ShopEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.ShopEditor.folderlabel };

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
                new Button { Text = Strings.ShopEditor.New },
                new Button { Text = Strings.ShopEditor.delete },
                new Button { Text = Strings.ShopEditor.copy },
                new Button { Text = Strings.ShopEditor.paste },
                new Button { Text = Strings.ShopEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.ShopEditor.save };
        btnCancel = new Button { Text = Strings.ShopEditor.cancel };
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
        cmbDefaultCurrency = new DropDown();
        cmbAddSoldItem = new DropDown();
        cmbSellFor = new DropDown();
        cmbAddBoughtItem = new DropDown();
        cmbBuyFor = new DropDown();
        cmbBuySound = new DropDown();
        cmbSellSound = new DropDown();

        nudSellCost = new NumericStepper { MinValue = 0 };
        nudBuyAmount = new NumericStepper { MinValue = 0 };

        rdoBuyWhitelist = new RadioButton { Text = Strings.ShopEditor.whitelist };
        rdoBuyBlacklist = new RadioButton(rdoBuyWhitelist) { Text = Strings.ShopEditor.blacklist };

        lstSoldItems = new ListBox();
        lstBoughtItems = new ListBox();

        btnAddSoldItem = new Button { Text = Strings.ShopEditor.addsolditem };
        btnDelSoldItem = new Button { Text = Strings.ShopEditor.removesolditem };
        btnAddBoughtItem = new Button { Text = Strings.ShopEditor.addboughtitem };
        btnDelBoughtItem = new Button { Text = Strings.ShopEditor.removeboughtitem };
        btnItemUp = new Button { Text = "^" };
        btnItemDown = new Button { Text = "v" };

        grpGeneral = new GroupBox { Text = Strings.ShopEditor.general };
        grpItemsSold = new GroupBox { Text = Strings.ShopEditor.itemssold };
        grpItemsBought = new GroupBox { Text = Strings.ShopEditor.itemsboughtwhitelist };

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

        rdoBuyWhitelist!.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.BuyingWhitelist = rdoBuyWhitelist.Checked;
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
        cmbAddBoughtItem!.Items.Clear();
        cmbAddSoldItem!.Items.Clear();
        cmbBuyFor!.Items.Clear();
        cmbSellFor!.Items.Clear();
        cmbDefaultCurrency!.Items.Clear();

        foreach (var name in ItemDescriptor.Names)
        {
            cmbAddBoughtItem.Items.Add(name);
            cmbAddSoldItem.Items.Add(name);
            cmbBuyFor.Items.Add(name);
            cmbSellFor.Items.Add(name);
            cmbDefaultCurrency.Items.Add(name);
        }

        cmbBuySound!.Items.Clear();
        cmbBuySound.Items.Add(Strings.General.None);
        foreach (var name in GameContentManager.SmartSortedSoundNames)
        {
            cmbBuySound.Items.Add(name);
        }

        cmbSellSound!.Items.Clear();
        cmbSellSound.Items.Add(Strings.General.None);
        foreach (var name in GameContentManager.SmartSortedSoundNames)
        {
            cmbSellSound.Items.Add(name);
        }

        InitEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Shop)
        {
            InitEditor();
            if (mEditorItem != null && !ShopDescriptor.Lookup.Values.Contains(mEditorItem))
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
            cmbDefaultCurrency!.SelectedIndex = ItemDescriptor.ListIndex(mEditorItem.DefaultCurrencyId);

            if (mEditorItem.BuyingWhitelist)
            {
                rdoBuyWhitelist!.Checked = true;
            }
            else
            {
                rdoBuyBlacklist!.Checked = true;
            }

            cmbBuySound!.SelectedIndex = cmbBuySound.Items.IndexOf(
                new ListItem { Text = TextUtils.NullToNone(mEditorItem.BuySound) }
            );
            cmbSellSound!.SelectedIndex = cmbSellSound.Items.IndexOf(
                new ListItem { Text = TextUtils.NullToNone(mEditorItem.SellSound) }
            );

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
        foreach (var itm in ShopDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((ShopDescriptor)itm.Value).Folder) &&
                !mFolders.Contains(((ShopDescriptor)itm.Value).Folder))
            {
                mFolders.Add(((ShopDescriptor)itm.Value).Folder);
                if (!mKnownFolders.Contains(((ShopDescriptor)itm.Value).Folder))
                {
                    mKnownFolders.Add(((ShopDescriptor)itm.Value).Folder);
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
            var items = ShopDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var shop = (ShopDescriptor?)pair.Value;
                if (shop != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = shop.Name ?? "Deleted" });
                }
            }
        }
    }
}
