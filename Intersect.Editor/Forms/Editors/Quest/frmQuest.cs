using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Events;
using Intersect.GameObjects;
using Microsoft.Extensions.Logging;

namespace Intersect.Editor.Forms.Editors.Quest;

public partial class FrmQuest : EditorForm
{
    private List<QuestDescriptor> mChanged = new();
    private string? mCopiedItem;
    private QuestDescriptor? mEditorItem;
    private List<string> mKnownFolders = new();

    protected ListBox? lstGameObjects;
    protected ListBox? lstTasks;
    protected TextBox? txtName;
    protected TextArea? txtBeforeDesc;
    protected TextArea? txtStartDesc;
    protected TextArea? txtInProgressDesc;
    protected TextArea? txtEndDesc;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected DropDown? cmbUnstartedCategory;
    protected DropDown? cmbInProgressCategory;
    protected DropDown? cmbCompletedCategory;
    protected NumericStepper? nudOrderValue;
    protected CheckBox? chkRepeatable;
    protected CheckBox? chkQuittable;
    protected CheckBox? chkLogBeforeOffer;
    protected CheckBox? chkLogAfterComplete;
    protected CheckBox? chkDoNotShowUnlessReqsMet;
    protected CheckBox? btnAlphabetical;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Button? btnEditRequirements;
    protected Button? btnEditStartEvent;
    protected Button? btnEditCompletionEvent;
    protected Button? btnAddTask;
    protected Button? btnRemoveTask;
    protected Button? btnShiftTaskUp;
    protected Button? btnShiftTaskDown;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected GroupBox? grpQuests;
    protected GroupBox? grpGeneral;
    protected GroupBox? grpLogOptions;
    protected GroupBox? grpProgessionOptions;
    protected GroupBox? grpQuestReqs;
    protected GroupBox? grpQuestTasks;
    protected GroupBox? grpActions;
    protected Panel? pnlContainer;
    protected Label? lblName;
    protected Label? lblBeforeOffer;
    protected Label? lblOffer;
    protected Label? lblInProgress;
    protected Label? lblCompleted;
    protected Label? lblOnStart;
    protected Label? lblOnEnd;
    protected Label? lblUnstartedCategory;
    protected Label? lblInProgressCategory;
    protected Label? lblCompletedCategory;
    protected Label? lblSortOrder;
    protected Label? lblFolder;

    public FrmQuest()
    {
        ApplyHooks();
        BuildUI();
        InitLocalization();
    }

    private void BuildUI()
    {
        Title = Strings.QuestEditor.title;
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
        txtSearch = new TextBox { PlaceholderText = Strings.QuestEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.QuestEditor.folderlabel };

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
                new Button { Text = Strings.QuestEditor.New },
                new Button { Text = Strings.QuestEditor.delete },
                new Button { Text = Strings.QuestEditor.copy },
                new Button { Text = Strings.QuestEditor.paste },
                new Button { Text = Strings.QuestEditor.undo }
            }
        };

        btnSave = new Button { Text = Strings.QuestEditor.save };
        btnCancel = new Button { Text = Strings.QuestEditor.cancel };
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
        txtBeforeDesc = new TextArea();
        txtStartDesc = new TextArea();
        txtInProgressDesc = new TextArea();
        txtEndDesc = new TextArea();

        cmbUnstartedCategory = new DropDown();
        cmbInProgressCategory = new DropDown();
        cmbCompletedCategory = new DropDown();

        nudOrderValue = new NumericStepper { MinValue = int.MinValue, MaxValue = int.MaxValue };

        chkRepeatable = new CheckBox { Text = Strings.QuestEditor.repeatable };
        chkQuittable = new CheckBox { Text = Strings.QuestEditor.quit };
        chkLogBeforeOffer = new CheckBox { Text = Strings.QuestEditor.showbefore };
        chkLogAfterComplete = new CheckBox { Text = Strings.QuestEditor.showafter };
        chkDoNotShowUnlessReqsMet = new CheckBox { Text = Strings.QuestEditor.donotshowunlessreqsmet };

        lstTasks = new ListBox();

        btnEditRequirements = new Button { Text = Strings.QuestEditor.editrequirements };
        btnEditStartEvent = new Button { Text = Strings.QuestEditor.editstartevent };
        btnEditCompletionEvent = new Button { Text = Strings.QuestEditor.editendevent };
        btnAddTask = new Button { Text = Strings.QuestEditor.addtask };
        btnRemoveTask = new Button { Text = Strings.QuestEditor.removetask };
        btnShiftTaskUp = new Button { Text = "^" };
        btnShiftTaskDown = new Button { Text = "v" };

        grpQuests = new GroupBox { Text = Strings.QuestEditor.quests };
        grpGeneral = new GroupBox { Text = Strings.QuestEditor.general };
        grpLogOptions = new GroupBox { Text = Strings.QuestEditor.logoptions };
        grpProgessionOptions = new GroupBox { Text = Strings.QuestEditor.options };
        grpQuestReqs = new GroupBox { Text = Strings.QuestEditor.requirements };
        grpQuestTasks = new GroupBox { Text = Strings.QuestEditor.tasks };
        grpActions = new GroupBox { Text = Strings.QuestEditor.actions };

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

                if (mEditorItem.StartEvent != null)
                {
                    mEditorItem.StartEvent.Name = Strings.QuestEditor.startevent.ToString(mEditorItem.Name);
                }

                if (mEditorItem.EndEvent != null)
                {
                    mEditorItem.EndEvent.Name = Strings.QuestEditor.endevent.ToString(mEditorItem.Name);
                }
            }
        };

        if (chkRepeatable != null) chkRepeatable.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Repeatable = chkRepeatable.Checked ?? false;
            }
        };

        if (chkQuittable != null) chkQuittable.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.Quitable = chkQuittable.Checked ?? false;
            }
        };

        if (chkLogBeforeOffer != null) chkLogBeforeOffer.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.LogBeforeOffer = chkLogBeforeOffer.Checked ?? false;
            }
        };

        if (chkLogAfterComplete != null) chkLogAfterComplete.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.LogAfterComplete = chkLogAfterComplete.Checked ?? false;
            }
        };

        if (chkDoNotShowUnlessReqsMet != null) chkDoNotShowUnlessReqsMet.CheckedChanged += (s, e) =>
        {
            if (mEditorItem != null)
            {
                mEditorItem.DoNotShowUnlessRequirementsMet = chkDoNotShowUnlessReqsMet.Checked ?? false;
            }
        };

        if (btnEditRequirements != null) btnEditRequirements.Click += (s, e) =>
        {
            if (mEditorItem != null)
            {
                var frm = new FrmDynamicRequirements(mEditorItem.Requirements, RequirementType.Quest);
                frm.ShowModal(this);
            }
        };

        if (btnSave != null) btnSave.Click += (s, e) =>
        {
            foreach (var item in mChanged)
            {
                item?.StartEvent?.RestoreBackup();
                item?.StartEvent?.DeleteBackup();
                item?.EndEvent?.RestoreBackup();
                item?.EndEvent?.DeleteBackup();
                item?.RestoreBackup();
                item?.DeleteBackup();

                if (item != null)
                {
                    PacketSender.SendSaveObject(item);
                    if (item.StartEvent != null) PacketSender.SendSaveObject(item.StartEvent);
                    if (item.EndEvent != null) PacketSender.SendSaveObject(item.EndEvent);
                }
            }
            Close();
            Globals.CurrentEditor = -1;
        };

        if (btnCancel != null) btnCancel.Click += (s, e) =>
        {
            foreach (var item in mChanged)
            {
                item?.StartEvent?.RestoreBackup();
                item?.StartEvent?.DeleteBackup();
                item?.EndEvent?.RestoreBackup();
                item?.EndEvent?.DeleteBackup();
                item?.RestoreBackup();
                item?.DeleteBackup();
            }
            mEditorItem = null;
            Close();
            Globals.CurrentEditor = -1;
        };
    }

    private void InitLocalization()
    {
//        Text = Strings.QuestEditor.title;
        grpQuests!.Text = Strings.QuestEditor.quests;
        grpGeneral!.Text = Strings.QuestEditor.general;
        lblName!.Text = Strings.QuestEditor.name;
        grpLogOptions!.Text = Strings.QuestEditor.logoptions;
        chkLogAfterComplete!.Text = Strings.QuestEditor.showafter;
        chkLogBeforeOffer!.Text = Strings.QuestEditor.showbefore;
        grpProgessionOptions!.Text = Strings.QuestEditor.options;
        chkRepeatable!.Text = Strings.QuestEditor.repeatable;
        chkQuittable!.Text = Strings.QuestEditor.quit;
        lblBeforeOffer!.Text = Strings.QuestEditor.beforeofferdesc;
        lblOffer!.Text = Strings.QuestEditor.offerdesc;
        lblInProgress!.Text = Strings.QuestEditor.inprogressdesc;
        lblCompleted!.Text = Strings.QuestEditor.completeddesc;
        grpQuestReqs!.Text = Strings.QuestEditor.requirements;
        btnEditRequirements!.Text = Strings.QuestEditor.editrequirements;
        grpQuestTasks!.Text = Strings.QuestEditor.tasks;
        btnAddTask!.Text = Strings.QuestEditor.addtask;
        btnRemoveTask!.Text = Strings.QuestEditor.removetask;
        grpActions!.Text = Strings.QuestEditor.actions;
        lblOnStart!.Text = Strings.QuestEditor.onstart;
        btnEditStartEvent!.Text = Strings.QuestEditor.editstartevent;
        lblOnEnd!.Text = Strings.QuestEditor.onend;
        btnEditCompletionEvent!.Text = Strings.QuestEditor.editendevent;
        btnAlphabetical!.Text = "A-Z";
        txtSearch!.PlaceholderText = Strings.QuestEditor.searchplaceholder;
        lblFolder!.Text = Strings.QuestEditor.folderlabel;
        chkDoNotShowUnlessReqsMet!.Text = Strings.QuestEditor.donotshowunlessreqsmet;
        lblUnstartedCategory!.Text = Strings.QuestEditor.unstartedcategory;
        lblInProgressCategory!.Text = Strings.QuestEditor.inprogressgategory;
        lblCompletedCategory!.Text = Strings.QuestEditor.completedcategory;
        lblSortOrder!.Text = Strings.QuestEditor.order;
        btnSave!.Text = Strings.QuestEditor.save;
        btnCancel!.Text = Strings.QuestEditor.cancel;

        // Initialize category dropdowns
        cmbUnstartedCategory!.Items.Clear();
        cmbInProgressCategory!.Items.Clear();
        cmbCompletedCategory!.Items.Clear();

        cmbUnstartedCategory.Items.Add(Strings.General.None);
        cmbInProgressCategory.Items.Add(Strings.General.None);
        cmbCompletedCategory.Items.Add(Strings.General.None);

        foreach (var questCategory in Options.Instance.Quest.Categories)
        {
            cmbUnstartedCategory.Items.Add(questCategory);
            cmbInProgressCategory.Items.Add(questCategory);
            cmbCompletedCategory.Items.Add(questCategory);
        }
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Quest)
        {
            InitEditor();
            if (mEditorItem != null && !QuestDescriptor.Lookup.Values.Contains(mEditorItem))
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
            txtBeforeDesc!.Text = mEditorItem.BeforeDescription;
            txtStartDesc!.Text = mEditorItem.StartDescription;
            txtInProgressDesc!.Text = mEditorItem.InProgressDescription;
            txtEndDesc!.Text = mEditorItem.EndDescription;

            chkRepeatable!.Checked = Convert.ToBoolean(mEditorItem.Repeatable);
            chkQuittable!.Checked = Convert.ToBoolean(mEditorItem.Quitable);
            chkLogBeforeOffer!.Checked = Convert.ToBoolean(mEditorItem.LogBeforeOffer);
            chkLogAfterComplete!.Checked = Convert.ToBoolean(mEditorItem.LogAfterComplete);
            chkDoNotShowUnlessReqsMet!.Checked = Convert.ToBoolean(mEditorItem.DoNotShowUnlessRequirementsMet);

            var unstartedIdx = cmbUnstartedCategory!.Items.IndexOf(
                new ListItem { Text = mEditorItem.UnstartedCategory ?? "" }
            );
            cmbUnstartedCategory.SelectedIndex = unstartedIdx >= 0 ? unstartedIdx : 0;

            var inProgressIdx = cmbInProgressCategory!.Items.IndexOf(
                new ListItem { Text = mEditorItem.InProgressCategory ?? "" }
            );
            cmbInProgressCategory.SelectedIndex = inProgressIdx >= 0 ? inProgressIdx : 0;

            var completedIdx = cmbCompletedCategory!.Items.IndexOf(
                new ListItem { Text = mEditorItem.CompletedCategory ?? "" }
            );
            cmbCompletedCategory.SelectedIndex = completedIdx >= 0 ? completedIdx : 0;

            nudOrderValue!.Value = mEditorItem.OrderValue;

            // Update tasks list
            lstTasks!.Items.Clear();
            foreach (var task in mEditorItem.Tasks)
            {
                lstTasks.Items.Add(task.GetTaskString(Strings.TaskEditor.descriptions));
            }

            if (mChanged.IndexOf(mEditorItem) == -1)
            {
                mChanged.Add(mEditorItem);
                mEditorItem.StartEvent?.MakeBackup();
                mEditorItem.EndEvent?.MakeBackup();
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
        foreach (var itm in QuestDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((QuestDescriptor)itm.Value).Folder) &&
                !mFolders.Contains(((QuestDescriptor)itm.Value).Folder))
            {
                mFolders.Add(((QuestDescriptor)itm.Value).Folder);
                if (!mKnownFolders.Contains(((QuestDescriptor)itm.Value).Folder))
                {
                    mKnownFolders.Add(((QuestDescriptor)itm.Value).Folder);
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
            var items = QuestDescriptor.Lookup.OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var quest = (QuestDescriptor?)pair.Value;
                if (quest != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = quest.Name ?? "Deleted" });
                }
            }
        }
    }
}
