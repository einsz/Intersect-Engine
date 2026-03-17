using Intersect.Editor.Core;
using Intersect.Editor.Localization;
using Intersect.Editor.Maps;
using Eto.Forms;
using Eto.Drawing;

namespace Intersect.Editor.Forms.DockingElements;

public partial class FrmMapProperties : Panel
{
    public delegate void UpdatePropertiesDelegate();

    public UpdatePropertiesDelegate UpdatePropertiesHandler;

    private PropertyGrid _gridMapProperties;

    public FrmMapProperties()
    {
        _gridMapProperties = new PropertyGrid();
        Content = _gridMapProperties;

        UpdatePropertiesHandler = Update;
        InitLocalization();
    }

    public void Init(MapInstance map)
    {
        Application.Instance.Invoke(() =>
        {
            _gridMapProperties.SelectedObject = new MapProperties(map);
            InitLocalization();
        });
    }

    private void InitLocalization()
    {
        // Panel doesn't have Text property - use Title if needed
    }

    public void Update()
    {
        _gridMapProperties.Refresh();
    }

    public object Selection()
    {
        return _gridMapProperties.SelectedProperty;
    }
}

public class PropertyGrid : Panel
{
    private object _selectedObject;
    private StackLayout _layout;
    private Dictionary<string, Control> _propertyControls;

    public PropertyGrid()
    {
        _propertyControls = new Dictionary<string, Control>();
        _layout = new StackLayout { Orientation = Orientation.Vertical, Spacing = 4 };
        Content = new Scrollable { Content = _layout };
    }

    public object SelectedObject
    {
        get => _selectedObject;
        set
        {
            _selectedObject = value;
            RebuildGrid();
        }
    }

    public string SelectedProperty { get; private set; }

    public void Refresh()
    {
        RebuildGrid();
    }

    private void RebuildGrid()
    {
        _layout.Items.Clear();
        _propertyControls.Clear();

        if (_selectedObject == null)
            return;

        var properties = _selectedObject.GetType().GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
        );

        foreach (var prop in properties)
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            var label = new Label { Text = prop.Name, Width = 120 };
            Control editor;

            var value = prop.GetValue(_selectedObject);
            var propType = prop.PropertyType;

            if (propType == typeof(string))
            {
                var textBox = new TextBox { Text = value?.ToString() ?? string.Empty };
                textBox.TextChanged += (s, e) =>
                {
                    prop.SetValue(_selectedObject, textBox.Text);
                };
                editor = textBox;
            }
            else if (propType == typeof(int) || propType == typeof(int?))
            {
                var stepper = new NumericStepper { Value = value != null ? (int)value : 0 };
                stepper.ValueChanged += (s, e) =>
                {
                    prop.SetValue(_selectedObject, (int)stepper.Value);
                };
                editor = stepper;
            }
            else if (propType == typeof(long) || propType == typeof(long?))
            {
                var stepper = new NumericStepper { Value = value != null ? (double)(long)value : 0 };
                stepper.ValueChanged += (s, e) =>
                {
                    prop.SetValue(_selectedObject, (long)stepper.Value);
                };
                editor = stepper;
            }
            else if (propType == typeof(double) || propType == typeof(float))
            {
                var stepper = new NumericStepper { Value = value != null ? Convert.ToDouble(value) : 0 };
                stepper.ValueChanged += (s, e) =>
                {
                    prop.SetValue(_selectedObject, Convert.ChangeType(stepper.Value, propType));
                };
                editor = stepper;
            }
            else if (propType == typeof(bool))
            {
                var checkBox = new CheckBox { Checked = value != null && (bool)value };
                checkBox.CheckedChanged += (s, e) =>
                {
                    prop.SetValue(_selectedObject, checkBox.Checked ?? false);
                };
                editor = checkBox;
            }
            else if (propType.IsEnum)
            {
                var dropDown = new DropDown();
                foreach (var enumValue in Enum.GetValues(propType))
                {
                    dropDown.Items.Add(new ListItem { Text = enumValue.ToString(), Key = enumValue.ToString() });
                }
                if (value != null)
                    dropDown.SelectedIndex = Array.IndexOf(Enum.GetValues(propType), value);
                dropDown.SelectedIndexChanged += (s, e) =>
                {
                    if (dropDown.SelectedIndex >= 0 && dropDown.SelectedValue is ListItem selectedItem)
                        prop.SetValue(_selectedObject, Enum.Parse(propType, selectedItem.Key));
                };
                editor = dropDown;
            }
            else if (propType == typeof(Guid))
            {
                var textBox = new TextBox { Text = value?.ToString() ?? string.Empty, ReadOnly = true };
                editor = textBox;
            }
            else
            {
                var textBox = new TextBox { Text = value?.ToString() ?? string.Empty };
                textBox.TextChanged += (s, e) =>
                {
                    try
                    {
                        var converted = Convert.ChangeType(textBox.Text, propType);
                        prop.SetValue(_selectedObject, converted);
                    }
                    catch { }
                };
                editor = textBox;
            }

            _propertyControls[prop.Name] = editor;

            var row = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Items = { label, editor }
            };
            _layout.Items.Add(row);
        }
    }
}
