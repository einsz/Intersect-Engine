using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Localization;
using Intersect.Framework.Core.GameObjects.Maps;

namespace Intersect.Editor.Forms.Controls;

public partial class MapAttributeTooltip : Panel
{
    private readonly object _mapAttributeLock = new object();
    private MapAttribute _mapAttribute;
    private DynamicLayout pnlContents;

    public MapAttributeTooltip()
    {
        pnlContents = new DynamicLayout
        {
            Padding = new Padding(5),
            DefaultSpacing = new Size(5, 2)
        };

        Content = pnlContents;
        BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        Border = BorderType.Line;
        Visible = false;
    }

    public MapAttribute MapAttribute
    {
        get => _mapAttribute;
        set
        {
            lock (_mapAttributeLock)
            {
                if (_mapAttribute == value)
                {
                    return;
                }

                var oldType = _mapAttribute?.GetType();
                _mapAttribute = value;
                OnAttributeChanged(oldType);
            }
        }
    }

    private static Label CreateLabel(bool bold = false)
    {
        var font = bold ? new Font(SystemFont.Bold) : new Font(SystemFont.Default);
        return new Label
        {
            TextColor = Colors.White,
            Font = font
        };
    }

    protected virtual void OnAttributeChanged(Type oldType)
    {
        Visible = false;

        if (_mapAttribute == null)
        {
            return;
        }

        var localizedProperties = Strings.Localizer.Localize(typeof(Strings), _mapAttribute);
        if (localizedProperties.Count < 1)
        {
            return;
        }

        pnlContents = new DynamicLayout
        {
            Padding = new Padding(5),
            DefaultSpacing = new Size(5, 2)
        };

        for (var rowIndex = 0; rowIndex < localizedProperties.Count; rowIndex++)
        {
            var labelControl = CreateLabel(rowIndex == 0);
            var displayValueControl = CreateLabel(rowIndex == 0);

            var localizedProperty = localizedProperties[rowIndex];
            labelControl.Text = localizedProperty.Key;
            displayValueControl.Text = localizedProperty.Value;

            pnlContents.AddRow(labelControl, displayValueControl);
        }

        Content = pnlContents;
        Visible = true;
    }
}
