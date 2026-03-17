using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.Localization;
using Intersect.Framework.Core.GameObjects.Conditions;
using Intersect.Framework.Core.GameObjects.Conditions.ConditionMetadata;

namespace Intersect.Editor.Forms.Editors;

public enum RequirementType
{
    Item,
    Resource,
    Spell,
    Event,
    Quest,
    NpcFriend,
    NpcAttackOnSight,
    NpcDontAttackOnSight,
    NpcCanBeAttacked,
    Craft
}

public partial class FrmDynamicRequirements : Dialog<DialogResult>
{
    private ConditionList? mEdittingList;
    private ConditionLists mEdittingLists;
    private ConditionList? mSourceList;
    private ConditionLists mSourceLists;

    protected ListBox? lstConditionLists;
    protected ListBox? lstConditions;
    protected TextBox? txtListName;
    protected Button? btnAddList;
    protected Button? btnRemoveList;
    protected Button? btnAddCondition;
    protected Button? btnRemoveCondition;
    protected Button? btnSave;
    protected Button? btnCancel;
    protected Label? lblInstructions;
    protected Label? lblListName;
    protected GroupBox? grpConditionLists;
    protected GroupBox? grpConditionList;

    public FrmDynamicRequirements(ConditionLists lists, RequirementType type)
    {
        Title = Strings.DynamicRequirements.title;
        MinimumSize = new Size(600, 500);
        Size = new Size(700, 600);
        Resizable = true;

        mSourceLists = lists;
        mEdittingLists = new ConditionLists(lists.Data());

        BuildUI();
        InitLocalization(type);
        UpdateLists();
    }

    private void BuildUI()
    {
        // Instructions
        lblInstructions = new Label();

        // Condition lists panel
        lstConditionLists = new ListBox();
        btnAddList = new Button { Text = Strings.DynamicRequirements.addlist };
        btnRemoveList = new Button { Text = Strings.DynamicRequirements.removelist };

        grpConditionLists = new GroupBox
        {
            Text = Strings.DynamicRequirements.conditionlists,
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Padding = new Padding(5),
                Items =
                {
                    lstConditionLists,
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items = { btnAddList, btnRemoveList }
                    }
                }
            }
        };

        // Condition list editor panel
        lblListName = new Label { Text = Strings.DynamicRequirements.listname };
        txtListName = new TextBox();
        lstConditions = new ListBox();
        btnAddCondition = new Button { Text = Strings.DynamicRequirements.addcondition };
        btnRemoveCondition = new Button { Text = Strings.DynamicRequirements.removecondition };

        grpConditionList = new GroupBox
        {
            Text = Strings.DynamicRequirements.conditionlist,
            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Padding = new Padding(5),
                Items =
                {
                    new StackLayout { Orientation = Orientation.Horizontal, Spacing = 5, Items = { lblListName, txtListName } },
                    lstConditions,
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items = { btnAddCondition, btnRemoveCondition }
                    }
                }
            }
        };

        // Buttons
        btnSave = new Button { Text = Strings.DynamicRequirements.save };
        btnCancel = new Button { Text = Strings.DynamicRequirements.cancel };

        // Layout
        var splitter = new Splitter
        {
            Orientation = Orientation.Horizontal,
            Position = 250,
            Panel1 = new Panel
            {
                Content = new StackLayout
                {
                    Padding = new Padding(5),
                    Spacing = 10,
                    Items = { lblInstructions, grpConditionLists }
                }
            },
            Panel2 = new Panel
            {
                Content = new StackLayout
                {
                    Padding = new Padding(5),
                    Spacing = 10,
                    Items =
                    {
                        grpConditionList,
                        new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Items = { btnSave, btnCancel }
                        }
                    }
                }
            }
        };

        Content = splitter;

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        if (lstConditionLists != null) lstConditionLists.SelectedIndexChanged += (s, e) =>
        {
            if (lstConditionLists.SelectedIndex > -1 && lstConditionLists.SelectedIndex < mEdittingLists.Lists.Count)
            {
                UpdateConditions(mEdittingLists.Lists[lstConditionLists.SelectedIndex]);
            }
        };

        lstConditionLists.KeyDown += (s, e) =>
        {
            if (e.Key == Keys.Delete)
            {
                btnRemoveList_Click();
            }
        };

        lstConditions!.MouseDoubleClick += (s, e) =>
        {
            if (lstConditions.SelectedIndex > -1 && mEdittingList != null)
            {
                var condition = OpenConditionEditor(mEdittingList.Conditions[lstConditions.SelectedIndex]);
                if (condition != null)
                {
                    mEdittingList.Conditions[lstConditions.SelectedIndex] = condition;
                    UpdateConditions(mEdittingList);
                }
            }
        };

        lstConditions.KeyDown += (s, e) =>
        {
            if (e.Key == Keys.Delete)
            {
                btnRemoveCondition_Click();
            }
        };

        txtListName!.TextChanged += (s, e) =>
        {
            if (mEdittingList != null && !string.IsNullOrWhiteSpace(txtListName.Text))
            {
                mEdittingList.Name = txtListName.Text;
                var index = mEdittingLists.Lists.IndexOf(mEdittingList);
                if (index > -1 && index < lstConditionLists.Items.Count)
                {
                    lstConditionLists.Items[index].Text = txtListName.Text;
                }
            }
        };

        if (btnAddList != null) btnAddList.Click += (s, e) => btnAddList_Click();
        if (btnRemoveList != null) btnRemoveList.Click += (s, e) => btnRemoveList_Click();
        if (btnAddCondition != null) btnAddCondition.Click += (s, e) => btnAddCondition_Click();
        if (btnRemoveCondition != null) btnRemoveCondition.Click += (s, e) => btnRemoveCondition_Click();
        if (btnSave != null) btnSave.Click += (s, e) => btnSave_Click();
        if (btnCancel != null) btnCancel.Click += (s, e) => btnCancel_Click();
    }

    private void InitLocalization(RequirementType type)
    {
//        Text = Strings.DynamicRequirements.title;
        grpConditionLists!.Text = Strings.DynamicRequirements.conditionlists;

        switch (type)
        {
            case RequirementType.Item:
                lblInstructions!.Text = Strings.DynamicRequirements.instructionsitem;
                break;
            case RequirementType.Resource:
                lblInstructions!.Text = Strings.DynamicRequirements.instructionsresource;
                break;
            case RequirementType.Spell:
                lblInstructions!.Text = Strings.DynamicRequirements.instructionsspell;
                break;
            case RequirementType.Event:
                lblInstructions!.Text = Strings.DynamicRequirements.instructionsevent;
                break;
            case RequirementType.Quest:
                lblInstructions!.Text = Strings.DynamicRequirements.instructionsquest;
                break;
            case RequirementType.NpcFriend:
                lblInstructions!.Text = Strings.DynamicRequirements.instructionsnpcfriend;
                break;
            case RequirementType.NpcAttackOnSight:
                lblInstructions!.Text = Strings.DynamicRequirements.instructionsnpcattackonsight;
                break;
            case RequirementType.NpcDontAttackOnSight:
                lblInstructions!.Text = Strings.DynamicRequirements.instructionsnpcdontattackonsight;
                break;
            case RequirementType.NpcCanBeAttacked:
                lblInstructions!.Text = Strings.DynamicRequirements.instructionsnpccanbeattacked;
                break;
            case RequirementType.Craft:
                lblInstructions!.Text = Strings.DynamicRequirements.instructionscraft;
                break;
        }

        btnAddList!.Text = Strings.DynamicRequirements.addlist;
        btnRemoveList!.Text = Strings.DynamicRequirements.removelist;
        btnSave!.Text = Strings.DynamicRequirements.save;
        btnCancel!.Text = Strings.DynamicRequirements.cancel;
        grpConditionList!.Text = Strings.DynamicRequirements.conditionlist;
        lblListName!.Text = Strings.DynamicRequirements.listname;
        btnAddCondition!.Text = Strings.DynamicRequirements.addcondition;
        btnRemoveCondition!.Text = Strings.DynamicRequirements.removecondition;
    }

    private void UpdateLists()
    {
        grpConditionLists!.Visible = true;
        grpConditionList!.Visible = false;
        lstConditionLists!.Items.Clear();
        for (var i = 0; i < mEdittingLists.Lists.Count; i++)
        {
            lstConditionLists.Items.Add(mEdittingLists.Lists[i].Name);
        }
    }

    private void UpdateConditions(ConditionList list)
    {
        grpConditionList!.Visible = true;
        lstConditions!.Items.Clear();
        if (list != mEdittingList)
        {
            mSourceList = list;
            mEdittingList = mSourceList;
        }

        txtListName!.Text = list.Name;
        for (var i = 0; i < list.Conditions.Count; i++)
        {
            if (list.Conditions[i].Negated)
            {
                lstConditions.Items.Add(
                    Strings.EventConditionDesc.negated.ToString(
                        Strings.GetEventConditionalDesc((dynamic)list.Conditions[i])
                    )
                );
            }
            else
            {
                lstConditions.Items.Add(Strings.GetEventConditionalDesc((dynamic)list.Conditions[i]));
            }
        }
    }

    private void btnAddList_Click()
    {
        var newList = new ConditionList();
        mEdittingLists.Lists.Add(newList);
        UpdateLists();
        lstConditionLists!.SelectedIndex = lstConditionLists.Items.Count - 1;
        UpdateConditions(newList);
    }

    private void btnRemoveList_Click()
    {
        if (lstConditionLists!.SelectedIndex > -1)
        {
            mEdittingLists.Lists.RemoveAt(lstConditionLists.SelectedIndex);
            UpdateLists();
        }
    }

    private void btnAddCondition_Click()
    {
        var condition = OpenConditionEditor(new VariableIsCondition());
        if (condition != null && mEdittingList != null)
        {
            mEdittingList.Conditions.Add(condition);
            UpdateConditions(mEdittingList);
        }
    }

    private void btnRemoveCondition_Click()
    {
        if (lstConditions!.SelectedIndex > -1 && mEdittingList != null)
        {
            mEdittingList.Conditions.RemoveAt(lstConditions.SelectedIndex);
            UpdateConditions(mEdittingList);
        }
    }

    private Condition? OpenConditionEditor(Condition condition)
    {
        // Simplified condition editor - in a full implementation, this would open a proper condition editor dialog
        var result = MessageBox.Show(this,
            "Condition editor not fully implemented in Eto.Forms version. Use original WinForms version for full editing.",
            "Condition Editor",
            MessageBoxButtons.OK);
        return null;
    }

    private void btnSave_Click()
    {
        mSourceLists.Load(mEdittingLists.Data());
        Close(DialogResult.Ok);
    }

    private void btnCancel_Click()
    {
        Close(DialogResult.Cancel);
    }
}
