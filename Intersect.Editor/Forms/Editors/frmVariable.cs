using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Variables;
using Intersect.Models;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmSwitchVariable : EditorForm
{
    private List<IDatabaseObject> mChanged = new();
    private IDatabaseObject? mEditorItem;
    private List<string> mKnownFolders = new();
    private List<string> mGlobalKnownFolders = new();
    private List<string> mGuildKnownFolders = new();
    private List<string> mUserKnownFolders = new();

    protected TreeGridView? lstGameObjects;
    protected TextBox? txtObjectName;
    protected TextBox? txtId;
    protected TextBox? txtStringValue;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbVariableType;
    protected DropDown? cmbBooleanValue;
    protected NumericStepper? nudVariableValue;
    protected RadioButton? rdoPlayerVariables;
    protected RadioButton? rdoGlobalVariables;
    protected RadioButton? rdoGuildVariables;
    protected RadioButton? rdoUserVariables;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected Button? btnNew;
    protected Button? btnDelete;
    protected Button? btnUndo;
    protected GroupBox? grpTypes;
    protected GroupBox? grpList;
    protected GroupBox? grpEditor;
    protected GroupBox? grpValue;
    protected GroupBox? grpVariables;
    protected Label? lblObject;
    protected Label? lblName;
    protected Label? lblId;
    protected Label? lblFolder;

    public FrmSwitchVariable()
    {
        ApplyHooks();
        BuildUI();
        InitLocalization();
    }

    private void BuildUI()
    {
        Title = Strings.VariableEditor.title;
        MinimumSize = new Size(1024, 768);

        // Type selection panel
        rdoPlayerVariables = new RadioButton { Text = Strings.VariableEditor.playervariables };
        rdoGlobalVariables = new RadioButton(rdoPlayerVariables) { Text = Strings.VariableEditor.globalvariables };
        rdoGuildVariables = new RadioButton(rdoPlayerVariables) { Text = Strings.VariableEditor.guildvariables };
        rdoUserVariables = new RadioButton(rdoPlayerVariables) { Text = Strings.GameObjectStrings.UserVariables };
        rdoPlayerVariables.Checked = true;

        grpTypes = new GroupBox
        {
            Text = Strings.VariableEditor.type,
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Padding = new Padding(5),
                Items = { rdoPlayerVariables, rdoGlobalVariables, rdoGuildVariables, rdoUserVariables }
            }
        };

        // List panel
        lstGameObjects = new TreeGridView();
        txtSearch = new TextBox { PlaceholderText = Strings.VariableEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.VariableEditor.folderlabel };

        btnNew = new Button { Text = Strings.VariableEditor.New };
        btnDelete = new Button { Text = Strings.VariableEditor.delete };
        btnUndo = new Button { Text = Strings.VariableEditor.undo };

        grpList = new GroupBox
        {
            Text = Strings.VariableEditor.list,
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Padding = new Padding(5),
                Items =
                {
                    new StackLayout { Orientation = Orientation.Horizontal, Items = { txtSearch, btnClearSearch, btnAlphabetical } },
                    new StackLayout { Orientation = Orientation.Horizontal, Items = { lblFolder, cmbFolder, btnAddFolder } },
                    lstGameObjects,
                    new StackLayout { Orientation = Orientation.Horizontal, Spacing = 5, Items = { btnNew, btnDelete, btnUndo } }
                }
            }
        };

        // Editor panel
        txtObjectName = new TextBox();
        txtId = new TextBox();
        cmbVariableType = new DropDown();
        cmbBooleanValue = new DropDown();
        nudVariableValue = new NumericStepper { MinValue = long.MinValue, MaxValue = long.MaxValue };
        txtStringValue = new TextBox();

        cmbBooleanValue.Items.Clear();
        cmbBooleanValue.Items.Add(Strings.VariableEditor.False);
        cmbBooleanValue.Items.Add(Strings.VariableEditor.True);

        cmbVariableType.Items.Clear();
        foreach (var itm in Strings.VariableEditor.types)
        {
            cmbVariableType.Items.Add(itm.Value);
        }

        lblObject = new Label();
        lblName = new Label { Text = Strings.VariableEditor.name };
        lblId = new Label();

        grpEditor = new GroupBox
        {
            Text = Strings.VariableEditor.editor,
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Padding = new Padding(5),
                Items =
                {
                    new StackLayout { Orientation = Orientation.Horizontal, Spacing = 5, Items = { lblName, txtObjectName } },
                    new StackLayout { Orientation = Orientation.Horizontal, Spacing = 5, Items = { lblId, txtId } },
                    new StackLayout { Orientation = Orientation.Horizontal, Spacing = 5, Items = { new Label { Text = Strings.VariableEditor.type }, cmbVariableType } }
                }
            }
        };

        grpValue = new GroupBox
        {
            Text = Strings.VariableEditor.value,
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Padding = new Padding(5),
                Items = { cmbBooleanValue, nudVariableValue, txtStringValue }
            }
        };

        // Save/Cancel
        btnSave = new Button { Text = Strings.VariableEditor.save };
        btnCancel = new Button { Text = Strings.VariableEditor.cancel };
        _btnSave = btnSave;
        _btnCancel = btnCancel;

        grpVariables = new GroupBox { Text = Strings.VariableEditor.list };

        // Main layout
        var leftPanel = new Panel
        {
            Content = new StackLayout
            {
                Padding = new Padding(5),
                Spacing = 10,
                Items = { grpTypes, grpList }
            }
        };

        var rightPanel = new Panel
        {
            Content = new StackLayout
            {
                Padding = new Padding(5),
                Spacing = 10,
                Items =
                {
                    grpEditor,
                    grpValue,
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items = { btnSave, btnCancel }
                    }
                }
            }
        };

        var mainSplitter = new Splitter
        {
            Orientation = Orientation.Horizontal,
            Position = 300,
            Panel1 = leftPanel,
            Panel2 = rightPanel
        };

        Content = mainSplitter;

        SetupEventHandlers();
        InitEditor();
    }

    private void SetupEventHandlers()
    {
        rdoPlayerVariables!.CheckedChanged += (s, e) => { if (rdoPlayerVariables.Checked == true) VariableRadioChanged(); };
        rdoGlobalVariables!.CheckedChanged += (s, e) => { if (rdoGlobalVariables.Checked == true) VariableRadioChanged(); };
        rdoGuildVariables!.CheckedChanged += (s, e) => { if (rdoGuildVariables.Checked == true) VariableRadioChanged(); };
        rdoUserVariables!.CheckedChanged += (s, e) => { if (rdoUserVariables.Checked == true) VariableRadioChanged(); };

        txtObjectName!.TextChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                if (rdoPlayerVariables.Checked == true && mEditorItem is PlayerVariableDescriptor pv)
                    pv.Name = txtObjectName.Text;
                else if (rdoGlobalVariables.Checked == true && mEditorItem is ServerVariableDescriptor sv)
                    sv.Name = txtObjectName.Text;
                else if (rdoGuildVariables.Checked == true && mEditorItem is GuildVariableDescriptor gv)
                    gv.Name = txtObjectName.Text;
                else if (rdoUserVariables.Checked == true && mEditorItem is UserVariableDescriptor uv)
                    uv.Name = txtObjectName.Text;
            }
        };

        txtId!.TextChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                if (rdoPlayerVariables.Checked == true && mEditorItem is PlayerVariableDescriptor pv)
                    pv.TextId = txtId.Text;
                else if (rdoGlobalVariables.Checked == true && mEditorItem is ServerVariableDescriptor sv)
                    sv.TextId = txtId.Text;
                else if (rdoGuildVariables.Checked == true && mEditorItem is GuildVariableDescriptor gv)
                    gv.TextId = txtId.Text;
                else if (rdoUserVariables.Checked == true && mEditorItem is UserVariableDescriptor uv)
                    uv.TextId = txtId.Text;
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

    private void InitLocalization()
    {
//        Text = Strings.VariableEditor.title;
        grpTypes!.Text = Strings.VariableEditor.type;
        grpList!.Text = Strings.VariableEditor.list;
        rdoPlayerVariables!.Text = Strings.VariableEditor.playervariables;
        rdoGlobalVariables!.Text = Strings.VariableEditor.globalvariables;
        rdoGuildVariables!.Text = Strings.VariableEditor.guildvariables;
        rdoUserVariables!.Text = Strings.GameObjectStrings.UserVariables;
        grpEditor!.Text = Strings.VariableEditor.editor;
        lblName!.Text = Strings.VariableEditor.name;
        grpValue!.Text = Strings.VariableEditor.value;
        btnNew!.ToolTip = Strings.VariableEditor.New;
        btnDelete!.ToolTip = Strings.VariableEditor.delete;
        btnUndo!.ToolTip = Strings.VariableEditor.undo;
        btnAlphabetical!.ToolTip = Strings.VariableEditor.sortalphabetically;
        txtSearch!.PlaceholderText = Strings.VariableEditor.searchplaceholder;
        lblFolder!.Text = Strings.VariableEditor.folderlabel;
        btnSave!.Text = Strings.VariableEditor.save;
        btnCancel!.Text = Strings.VariableEditor.cancel;
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.PlayerVariable ||
            type == GameObjectType.ServerVariable ||
            type == GameObjectType.GuildVariable ||
            type == GameObjectType.UserVariable)
        {
            InitEditor();
            if (mEditorItem != null)
            {
                bool exists = false;
                if (rdoPlayerVariables!.Checked == true)
                    exists = PlayerVariableDescriptor.Lookup.Values.Contains(mEditorItem);
                else if (rdoGlobalVariables!.Checked == true)
                    exists = ServerVariableDescriptor.Lookup.Values.Contains(mEditorItem);
                else if (rdoGuildVariables!.Checked == true)
                    exists = GuildVariableDescriptor.Lookup.Values.Contains(mEditorItem);
                else if (rdoUserVariables!.Checked == true)
                    exists = UserVariableDescriptor.Lookup.Values.Contains(mEditorItem);

                if (!exists)
                {
                    mEditorItem = null;
                    UpdateEditor();
                }
            }
        }
    }

    private void VariableRadioChanged()
    {
        mEditorItem = null;
        InitEditor();
    }

    private void UpdateEditor()
    {
        if (mEditorItem != null)
        {
            grpEditor!.Visible = true;
            grpValue!.Visible = false;

            if (rdoPlayerVariables!.Checked == true && mEditorItem is PlayerVariableDescriptor pv)
            {
                lblObject!.Text = Strings.VariableEditor.playervariable;
                txtObjectName!.Text = pv.Name;
                txtId!.Text = pv.TextId;
                // cmbFolder!.Text = pv.Folder; // DropDown doesn't have Text setter in Eto
                cmbVariableType!.SelectedIndex = (int)(pv.DataType - 1);
            }
            else if (rdoGlobalVariables!.Checked == true && mEditorItem is ServerVariableDescriptor sv)
            {
                lblObject!.Text = Strings.VariableEditor.globalvariable;
                txtObjectName!.Text = sv.Name;
                txtId!.Text = sv.TextId;
                // cmbFolder!.Text = sv.Folder; // DropDown doesn't have Text setter in Eto
                cmbVariableType!.SelectedIndex = (int)(sv.DataType - 1);
                grpValue.Visible = true;

                // Show appropriate value control based on data type
                cmbBooleanValue!.Visible = sv.DataType == VariableDataType.Boolean;
                nudVariableValue!.Visible = sv.DataType == VariableDataType.Integer;
                txtStringValue!.Visible = sv.DataType == VariableDataType.String;

                if (sv.DataType == VariableDataType.Boolean)
                    cmbBooleanValue.SelectedIndex = Convert.ToInt32(sv.Value.Boolean);
                else if (sv.DataType == VariableDataType.Integer)
                    nudVariableValue.Value = sv.Value.Integer;
                else if (sv.DataType == VariableDataType.String)
                    txtStringValue.Text = sv.Value.String;
            }
            else if (rdoGuildVariables!.Checked == true && mEditorItem is GuildVariableDescriptor gv)
            {
                lblObject!.Text = Strings.VariableEditor.guildvariable;
                txtObjectName!.Text = gv.Name;
                txtId!.Text = gv.TextId;
                // cmbFolder!.Text = gv.Folder; // DropDown doesn't have Text setter in Eto
                cmbVariableType!.SelectedIndex = (int)(gv.DataType - 1);
            }
            else if (rdoUserVariables!.Checked == true && mEditorItem is UserVariableDescriptor uv)
            {
                lblObject!.Text = Strings.GameObjectStrings.UserVariable;
                txtObjectName!.Text = uv.Name;
                txtId!.Text = uv.TextId;
                // cmbFolder!.Text = uv.Folder; // DropDown doesn't have Text setter in Eto
                cmbVariableType!.SelectedIndex = (int)(uv.DataType - 1);
            }
        }
        else
        {
            grpEditor!.Visible = false;
        }

        UpdateEditorButtons(mEditorItem != null);
    }

    public void InitEditor()
    {
        // Set title based on selected type
        if (rdoPlayerVariables!.Checked == true)
            grpVariables!.Text = rdoPlayerVariables.Text ?? "";
        else if (rdoGlobalVariables!.Checked == true)
            grpVariables!.Text = rdoGlobalVariables.Text ?? "";
        else if (rdoGuildVariables!.Checked == true)
            grpVariables!.Text = rdoGuildVariables.Text ?? "";
        else if (rdoUserVariables!.Checked == true)
            grpVariables!.Text = rdoUserVariables.Text ?? "";

        grpEditor!.Visible = false;
        cmbBooleanValue!.Visible = false;
        nudVariableValue!.Visible = false;
        txtStringValue!.Visible = false;

        // Collect folders based on selected type
        var mFolders = new List<string>();
        cmbFolder!.Items.Clear();
        cmbFolder.Items.Add("");

        if (rdoPlayerVariables.Checked == true)
        {
            foreach (var itm in PlayerVariableDescriptor.Lookup)
            {
                var pv = (PlayerVariableDescriptor)itm.Value;
                if (!string.IsNullOrEmpty(pv.Folder) && !mFolders.Contains(pv.Folder))
                {
                    mFolders.Add(pv.Folder);
                    if (!mKnownFolders.Contains(pv.Folder))
                        mKnownFolders.Add(pv.Folder);
                }
            }
            mKnownFolders.Sort();
            foreach (var folder in mKnownFolders)
                cmbFolder.Items.Add(folder);
            lblId!.Text = Strings.VariableEditor.textidpv;
        }
        else if (rdoGlobalVariables.Checked == true)
        {
            foreach (var itm in ServerVariableDescriptor.Lookup)
            {
                var sv = (ServerVariableDescriptor)itm.Value;
                if (!string.IsNullOrEmpty(sv.Folder) && !mFolders.Contains(sv.Folder))
                {
                    mFolders.Add(sv.Folder);
                    if (!mGlobalKnownFolders.Contains(sv.Folder))
                        mGlobalKnownFolders.Add(sv.Folder);
                }
            }
            mGlobalKnownFolders.Sort();
            foreach (var folder in mGlobalKnownFolders)
                cmbFolder.Items.Add(folder);
            lblId!.Text = Strings.VariableEditor.textidgv;
        }
        else if (rdoGuildVariables.Checked == true)
        {
            foreach (var itm in GuildVariableDescriptor.Lookup)
            {
                var gv = (GuildVariableDescriptor)itm.Value;
                if (!string.IsNullOrEmpty(gv.Folder) && !mFolders.Contains(gv.Folder))
                {
                    mFolders.Add(gv.Folder);
                    if (!mGuildKnownFolders.Contains(gv.Folder))
                        mGuildKnownFolders.Add(gv.Folder);
                }
            }
            mGuildKnownFolders.Sort();
            foreach (var folder in mGuildKnownFolders)
                cmbFolder.Items.Add(folder);
            lblId!.Text = Strings.VariableEditor.textidguildvar;
        }
        else if (rdoUserVariables.Checked == true)
        {
            foreach (var itm in UserVariableDescriptor.Lookup)
            {
                var uv = (UserVariableDescriptor)itm.Value;
                if (!string.IsNullOrEmpty(uv.Folder) && !mFolders.Contains(uv.Folder))
                {
                    mFolders.Add(uv.Folder);
                    if (!mUserKnownFolders.Contains(uv.Folder))
                        mUserKnownFolders.Add(uv.Folder);
                }
            }
            mUserKnownFolders.Sort();
            foreach (var folder in mUserKnownFolders)
                cmbFolder.Items.Add(folder);
            lblId!.Text = Strings.VariableEditor.UserVariableId;
        }

        UpdateEditor();
    }
}
