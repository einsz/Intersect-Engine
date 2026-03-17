using Intersect.Editor.Localization;
using Intersect.Enums;
using Intersect.Extensions;
using Intersect.Framework.Core.GameObjects.Events;

namespace Intersect.Editor.Utilities;
public static class VariableSelectorUtils
{
    public static void OpenVariableSelector(Action<object> onSelectionComplete,
        Guid selectedVariableId,
        VariableType selectedVariableType,
        VariableDataType dataTypeFilter = 0)
    {
        // TODO: Implement VariableSelector when FrmVariableSelector is ported to Eto.Forms
        throw new NotImplementedException("VariableSelector not yet ported to Eto.Forms");
    }

    public static string GetSelectedVarText(VariableType variableType, Guid selectedVariableId)
    {
        Strings.VariableSelector.VariableTypes.TryGetValue((int)variableType, out var type);
        var varName = variableType.GetRelatedTable().GetLookup().Get(selectedVariableId)?.Name;

        if (varName == default || selectedVariableId == Guid.Empty)
        {
            return Strings.VariableSelector.ValueNoneSelected.ToString();
        }

        return Strings.VariableSelector.ValueCurrentSelection.ToString(varName, type);
    }
}
