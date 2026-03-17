using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.Localization;
using Intersect.Enums;
using Intersect.Extensions;
using Intersect.Framework.Core.GameObjects.Events;

namespace Intersect.Editor.Forms;

public class FrmVariableSelector : Dialog<bool>
{
    private Guid mSelectedVariableId;
    private VariableType mSelectedVariableType;
    private bool mResult;
    private bool mPopulating;
    private VariableDataType mFilterType;
    private VariableSelection mSelection;

    private DropDown cmbVariableType;
    private DropDown cmbVariables;
    private GroupBox grpSelection;
    private GroupBox grpVariableType;
    private GroupBox grpVariable;
    private Button btnOk;
    private Button btnCancel;

    public FrmVariableSelector(VariableType variableType, Guid variableId, VariableDataType filterType)
    {
        mSelectedVariableId = variableId;
        mSelectedVariableType = variableType;
        mFilterType = filterType;
        InitializeControls();
        PopulateForm();
    }

    public FrmVariableSelector()
    {
        InitializeControls();
        PopulateForm();
    }

    private void InitializeControls()
    {
        Title = Strings.VariableSelector.Title;
        MinimumSize = new Size(400, 300);
        Resizable = true;

        cmbVariableType = new DropDown();
        cmbVariableType.SelectedIndexChanged += CmbVariableType_SelectedIndexChanged;

        cmbVariables = new DropDown();
        cmbVariables.SelectedIndexChanged += CmbVariables_SelectedIndexChanged;

        grpVariableType = new GroupBox { Text = Strings.VariableSelector.LabelVariableType, Content = cmbVariableType };
        grpVariable = new GroupBox { Text = Strings.VariableSelector.LabelVariableValue, Content = cmbVariables };
        grpSelection = new GroupBox { Text = Strings.VariableSelector.LabelGroup };

        btnOk = new Button { Text = Strings.General.Okay };
        btnOk.Click += BtnOk_Click;
        btnCancel = new Button { Text = Strings.General.Cancel };
        btnCancel.Click += (s, e) => Close();

        foreach (var varType in Strings.VariableSelector.VariableTypes.Values)
        {
            cmbVariableType.Items.Add(new ListItem { Text = varType });
        }

        var layout = new DynamicLayout { Padding = 10, DefaultSpacing = new Size(5, 5) };
        layout.AddRow(grpVariableType);
        layout.AddRow(grpVariable);
        layout.AddRow(grpSelection);
        layout.AddRow(null, btnOk, btnCancel);

        Content = layout;

        PositiveButtons.Add(btnOk);
        NegativeButtons.Add(btnCancel);
    }

    private void PopulateForm()
    {
        mPopulating = true;
        cmbVariableType.SelectedIndex = (int)mSelectedVariableType;
        ReloadVariablesOf(mSelectedVariableType);
        cmbVariables.SelectedIndex = mSelectedVariableType.GetRelatedTable().ListIndex(mSelectedVariableId, mFilterType);
        mPopulating = false;
    }

    private void ReloadVariablesOf(VariableType type)
    {
        cmbVariables.Items.Clear();
        foreach (var name in type.GetRelatedTable().Names(mFilterType))
        {
            cmbVariables.Items.Add(new ListItem { Text = name });
        }
    }

    private void CmbVariableType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (mPopulating) return;

        mSelectedVariableType = (VariableType)cmbVariableType.SelectedIndex;
        ReloadVariablesOf(mSelectedVariableType);

        if (cmbVariables.Items.Count > 0)
        {
            cmbVariables.SelectedIndex = 0;
            mSelectedVariableId = mSelectedVariableType.GetRelatedTable().IdFromList(0, mFilterType);
        }
        else
        {
            cmbVariables.SelectedIndex = -1;
            mSelectedVariableId = Guid.Empty;
        }
    }

    private void CmbVariables_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (mPopulating) return;
        mSelectedVariableId = mSelectedVariableType.GetRelatedTable().IdFromList(cmbVariables.SelectedIndex, mFilterType);
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        mResult = true;
        mSelection = new VariableSelection(mSelectedVariableType, mSelectedVariableId);
        Close();
    }

    public bool GetResult() => mResult;
    public VariableSelection GetSelection() => mSelection;
}

public class VariableSelection
{
    public VariableSelection(VariableType variableType, Guid variableId)
    {
        VariableType = variableType;
        VariableId = variableId;
    }

    public VariableType VariableType { get; set; }
    public Guid VariableId { get; set; }
}
