using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.Framework.Core.GameObjects.NPCs;
using Intersect.Framework.Core.GameObjects.Quests;
using Intersect.GameObjects;
using Microsoft.Extensions.Logging;

namespace Intersect.Editor.Forms.Editors.Quest;

public partial class QuestTaskEditor : Panel
{
    public bool Cancelled;

    private string mEventBackup = null;

    private QuestDescriptor mMyQuest;

    private QuestTaskDescriptor mMyTask;

    protected GroupBox grpEditor;
    protected Label lblType;
    protected DropDown cmbTaskType;
    protected Label lblDesc;
    protected TextArea txtStartDesc;
    protected Button btnEditTaskEvent;
    protected Button btnSave;
    protected Button btnCancel;
    protected GroupBox grpKillNpcs;
    protected NumericStepper nudNpcQuantity;
    protected DropDown cmbNpc;
    protected Label lblNpc;
    protected Label lblNpcQuantity;
    protected GroupBox grpGatherItems;
    protected NumericStepper nudItemAmount;
    protected DropDown cmbItem;
    protected Label lblItem;
    protected Label lblItemQuantity;
    protected Label lblEventDriven;

    public Form ParentForm { get; set; }

    public QuestTaskEditor(QuestDescriptor refQuest, QuestTaskDescriptor refTask)
    {
        if (refQuest == null)
        {
            Intersect.Core.ApplicationContext.Context.Value?.Logger.LogWarning($@"{nameof(refQuest)} is null.");
        }

        if (refTask == null)
        {
            Intersect.Core.ApplicationContext.Context.Value?.Logger.LogWarning($@"{nameof(refTask)} is null.");
        }

        mMyTask = refTask;
        mMyQuest = refQuest;

        if (mMyTask?.EditingEvent == null)
        {
            Intersect.Core.ApplicationContext.Context.Value?.Logger.LogWarning($@"{nameof(mMyTask.EditingEvent)} is null.");
        }

        mEventBackup = mMyTask?.EditingEvent?.JsonData;

        BuildUI();
        InitLocalization();

        cmbTaskType.SelectedIndex = mMyTask == null ? -1 : (int)mMyTask.Objective;
        txtStartDesc.Text = mMyTask?.Description;
        UpdateFormElements();

        switch (cmbTaskType.SelectedIndex)
        {
            case 0: //Event Driven
                break;
            case 1: //Gather Items
                cmbItem.SelectedIndex = ItemDescriptor.ListIndex(mMyTask?.TargetId ?? Guid.Empty);
                nudItemAmount.Value = mMyTask?.Quantity ?? 0;
                break;
            case 2: //Kill NPCS
                cmbNpc.SelectedIndex = NPCDescriptor.ListIndex(mMyTask?.TargetId ?? Guid.Empty);
                nudNpcQuantity.Value = mMyTask?.Quantity ?? 0;
                break;
        }
    }

    private void BuildUI()
    {
        lblType = new Label { Text = "Task Type:" };
        cmbTaskType = new DropDown();
        lblDesc = new Label { Text = "Desc:" };
        txtStartDesc = new TextArea { Height = 80 };
        btnEditTaskEvent = new Button { Text = "Edit Task Completion Event" };
        btnSave = new Button { Text = "Ok" };
        btnCancel = new Button { Text = "Cancel" };

        lblEventDriven = new Label
        {
            Text = "Event Driven: The description should lead the player to an event. The event will then complete the task using the complete quest task command.",
            Wrap = WrapMode.Word
        };

        // Kill NPCs group
        lblNpc = new Label { Text = "NPC" };
        cmbNpc = new DropDown();
        lblNpcQuantity = new Label { Text = "Amount:" };
        nudNpcQuantity = new NumericStepper { MinValue = 1, MaxValue = 100000, Value = 1 };

        grpKillNpcs = new GroupBox
        {
            Text = "Kill NPC(s)",
            Visible = false,
            Content = new TableLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(
                        new TableCell(lblNpc, true),
                        new TableCell(cmbNpc, true)
                    ),
                    new TableRow(
                        new TableCell(lblNpcQuantity, true),
                        new TableCell(nudNpcQuantity, true)
                    )
                }
            }
        };

        // Gather Items group
        lblItem = new Label { Text = "Item:" };
        cmbItem = new DropDown();
        lblItemQuantity = new Label { Text = "Amount:" };
        nudItemAmount = new NumericStepper { MinValue = 1, MaxValue = 100000, Value = 1 };

        grpGatherItems = new GroupBox
        {
            Text = "Gather Item(s)",
            Visible = false,
            Content = new TableLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(
                        new TableCell(lblItem, true),
                        new TableCell(cmbItem, true)
                    ),
                    new TableRow(
                        new TableCell(lblItemQuantity, true),
                        new TableCell(nudItemAmount, true)
                    )
                }
            }
        };

        // Editor group
        var editorLayout = new DynamicLayout
        {
            Padding = new Padding(10),
            DefaultSpacing = new Size(5, 5)
        };

        editorLayout.BeginVertical();
        editorLayout.AddRow(lblType, cmbTaskType);
        editorLayout.AddRow(lblDesc, txtStartDesc);
        editorLayout.EndVertical();

        editorLayout.Add(lblEventDriven);
        editorLayout.Add(grpGatherItems);
        editorLayout.Add(grpKillNpcs);
        editorLayout.Add(btnEditTaskEvent);

        var buttonRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Items = { btnSave, btnCancel }
        };

        editorLayout.Add(buttonRow);

        grpEditor = new GroupBox
        {
            Text = "Task Editor",
            Content = editorLayout
        };

        Content = new StackLayout
        {
            Padding = new Padding(5),
            Items = { grpEditor }
        };

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        btnSave.Click += btnSave_Click;
        btnCancel.Click += btnCancel_Click;
        cmbTaskType.SelectedIndexChanged += cmbConditionType_SelectedIndexChanged;
        btnEditTaskEvent.Click += btnEditTaskEvent_Click;
    }

    private void InitLocalization()
    {
        grpEditor.Text = Strings.TaskEditor.editor;

        lblType.Text = Strings.TaskEditor.type;
        cmbTaskType.Items.Clear();
        for (var i = 0; i < Strings.TaskEditor.types.Count; i++)
        {
            cmbTaskType.Items.Add(Strings.TaskEditor.types[i]);
        }

        lblDesc.Text = Strings.TaskEditor.desc;

        grpKillNpcs.Text = Strings.TaskEditor.killnpcs;
        lblNpc.Text = Strings.TaskEditor.npc;
        lblNpcQuantity.Text = Strings.TaskEditor.npcamount;

        grpGatherItems.Text = Strings.TaskEditor.gatheritems;
        lblItem.Text = Strings.TaskEditor.item;
        lblItemQuantity.Text = Strings.TaskEditor.gatheramount;

        lblEventDriven.Text = Strings.TaskEditor.eventdriven;

        btnEditTaskEvent.Text = Strings.TaskEditor.editcompletionevent;
        btnSave.Text = Strings.TaskEditor.ok;
        btnCancel.Text = Strings.TaskEditor.cancel;
    }

    private void UpdateFormElements()
    {
        grpGatherItems.Visible = false;
        grpKillNpcs.Visible = false;
        lblEventDriven.Visible = false;

        switch (cmbTaskType.SelectedIndex)
        {
            case 0: //Event Driven
                lblEventDriven.Visible = true;
                break;
            case 1: //Gather Items
                grpGatherItems.Visible = true;
                cmbItem.Items.Clear();
                var itemNames = ItemDescriptor.Names;
                if (itemNames != null)
                {
                    foreach (var name in itemNames)
                    {
                        cmbItem.Items.Add(name);
                    }
                }

                if (cmbItem.Items.Count > 0)
                {
                    cmbItem.SelectedIndex = 0;
                }

                nudItemAmount.Value = 1;
                break;
            case 2: //Kill Npcs
                grpKillNpcs.Visible = true;
                cmbNpc.Items.Clear();
                var npcNames = NPCDescriptor.Names;
                if (npcNames != null)
                {
                    foreach (var name in npcNames)
                    {
                        cmbNpc.Items.Add(name);
                    }
                }

                if (cmbNpc.Items.Count > 0)
                {
                    cmbNpc.SelectedIndex = 0;
                }

                nudNpcQuantity.Value = 1;
                break;
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        mMyTask.Objective = (QuestObjective)cmbTaskType.SelectedIndex;
        mMyTask.Description = txtStartDesc.Text;
        switch (mMyTask.Objective)
        {
            case QuestObjective.EventDriven:
                mMyTask.TargetId = Guid.Empty;
                mMyTask.Quantity = 1;
                break;
            case QuestObjective.GatherItems:
                mMyTask.TargetId = ItemDescriptor.IdFromList(cmbItem.SelectedIndex);
                mMyTask.Quantity = (int)nudItemAmount.Value;
                break;
            case QuestObjective.KillNpcs:
                mMyTask.TargetId = NPCDescriptor.IdFromList(cmbNpc.SelectedIndex);
                mMyTask.Quantity = (int)nudNpcQuantity.Value;
                break;
        }

        ParentForm?.Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Cancelled = true;
        mMyTask.EditingEvent.Load(mEventBackup);
        ParentForm?.Close();
    }

    private void cmbConditionType_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateFormElements();
    }

    private void btnEditTaskEvent_Click(object sender, EventArgs e)
    {
        mMyTask.EditingEvent.Name = Strings.TaskEditor.completionevent.ToString(mMyQuest.Name);
        var editor = new Intersect.Editor.Forms.Editors.Events.FrmEvent(null)
        {
            MyEvent = mMyTask.EditingEvent
        };

        editor.InitEditor(true, true, true);
        editor.ShowModal(this);
        Globals.MainForm.BringToFront();
        BringToFront();
    }
}
