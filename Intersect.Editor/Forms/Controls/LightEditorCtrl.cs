using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Framework.Core.GameObjects.Lighting;
using Intersect.GameObjects;
using Graphics = Intersect.Editor.Core.Graphics;

namespace Intersect.Editor.Forms.Controls;

public partial class LightEditorCtrl : Panel
{
    public bool CanClose = true;

    private LightDescriptor mBackupLight;
    private LightDescriptor mEditingLight;

    protected GroupBox grpLightEditor;
    protected Label lblOffsetX;
    protected Label lblOffsetY;
    protected Label lblColor;
    protected Label lblIntensity;
    protected Label lblSize;
    protected Label lblExpandAmt;
    protected NumericStepper nudOffsetX;
    protected NumericStepper nudOffsetY;
    protected NumericStepper nudSize;
    protected NumericStepper nudIntensity;
    protected NumericStepper nudExpand;
    protected Panel pnlLightColor;
    protected Button btnSelectLightColor;
    protected Button btnOkay;
    protected Button btnCancel;

    public LightEditorCtrl()
    {
        BuildUI();
        InitLocalization();
    }

    private void BuildUI()
    {
        lblOffsetX = new Label { Text = "X Offset:" };
        lblOffsetY = new Label { Text = "Y Offset:" };
        lblColor = new Label { Text = "Color:" };
        lblIntensity = new Label { Text = "Intensity:" };
        lblSize = new Label { Text = "Size:" };
        lblExpandAmt = new Label { Text = "Expand:" };

        nudOffsetX = new NumericStepper { MinValue = -1000, MaxValue = 1000 };
        nudOffsetY = new NumericStepper { MinValue = -1000, MaxValue = 1000 };
        nudSize = new NumericStepper { MinValue = 0, MaxValue = 1000 };
        nudIntensity = new NumericStepper { MinValue = 0, MaxValue = 255 };
        nudExpand = new NumericStepper { MinValue = 0, MaxValue = 1000 };

        pnlLightColor = new Panel
        {
            BackgroundColor = Colors.White,
            Size = new Size(50, 25)
        };

        btnSelectLightColor = new Button { Text = "..." };
        btnOkay = new Button { Text = "Save" };
        btnCancel = new Button { Text = "Revert" };

        var colorRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Items = { pnlLightColor, btnSelectLightColor }
        };

        var formLayout = new DynamicLayout
        {
            Padding = new Padding(10),
            DefaultSpacing = new Size(5, 5)
        };

        formLayout.BeginVertical();
        formLayout.AddRow(lblOffsetX, nudOffsetX);
        formLayout.AddRow(lblOffsetY, nudOffsetY);
        formLayout.AddRow(lblSize, nudSize);
        formLayout.AddRow(lblIntensity, nudIntensity);
        formLayout.AddRow(lblExpandAmt, nudExpand);
        formLayout.AddRow(lblColor, colorRow);
        formLayout.EndVertical();

        formLayout.AddRow(null, btnOkay, btnCancel);

        grpLightEditor = new GroupBox
        {
            Text = "Light Editor",
            Content = formLayout
        };

        Content = new StackLayout
        {
            Padding = new Padding(5),
            Items = { grpLightEditor }
        };

        if (!CanClose)
        {
            btnOkay.Visible = false;
        }

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        btnOkay.Click += btnLightEditorClose_Click;
        btnCancel.Click += btnLightEditorRevert_Click;
        btnSelectLightColor.Click += btnSelectLightColor_Click;

        nudOffsetX.ValueChanged += nudOffsetX_ValueChanged;
        nudOffsetY.ValueChanged += nudOffsetY_ValueChanged;
        nudSize.ValueChanged += nudSize_ValueChanged;
        nudIntensity.ValueChanged += nudIntensity_ValueChanged;
        nudExpand.ValueChanged += nudExpand_ValueChanged;
    }

    public void LoadEditor(LightDescriptor tmpLight)
    {
        mEditingLight = tmpLight;
        mBackupLight = new LightDescriptor(tmpLight);
        nudIntensity.Value = tmpLight.Intensity;
        nudSize.Value = tmpLight.Size;
        nudOffsetX.Value = tmpLight.OffsetX;
        nudOffsetY.Value = tmpLight.OffsetY;
        nudExpand.Value = (int)tmpLight.Expand;

        pnlLightColor.BackgroundColor = new Color(
            tmpLight.Color.R / 255f,
            tmpLight.Color.G / 255f,
            tmpLight.Color.B / 255f,
            tmpLight.Color.A / 255f
        );

        if (!CanClose)
        {
            btnOkay.Visible = false;
        }

        InitLocalization();
    }

    private void InitLocalization()
    {
        grpLightEditor.Text = Strings.LightEditor.title;
        lblOffsetX.Text = Strings.LightEditor.xoffset;
        lblOffsetY.Text = Strings.LightEditor.yoffset;
        lblColor.Text = Strings.LightEditor.color;
        lblIntensity.Text = Strings.LightEditor.intensity;
        lblSize.Text = Strings.LightEditor.size;
        lblExpandAmt.Text = Strings.LightEditor.expandamt;

        new ToolTip { Text = Strings.LightEditor.revert }.Attach(btnCancel);
        new ToolTip { Text = Strings.LightEditor.save }.Attach(btnOkay);
        new ToolTip { Text = Strings.LightEditor.SelectColor }.Attach(btnSelectLightColor);
    }

    private void btnLightEditorClose_Click(object sender, EventArgs e)
    {
        if (CanClose)
        {
            Visible = false;
        }

        if (mEditingLight == Globals.EditingLight)
        {
            Globals.EditingLight = null;
        }
    }

    private void btnLightEditorRevert_Click(object sender, EventArgs e)
    {
        if (mEditingLight != null)
        {
            mEditingLight.Intensity = mBackupLight.Intensity;
            mEditingLight.Size = mBackupLight.Size;
            mEditingLight.OffsetX = mBackupLight.OffsetX;
            mEditingLight.OffsetY = mBackupLight.OffsetY;
            mEditingLight.Expand = mBackupLight.Expand;
            LoadEditor(mEditingLight);
            if (mEditingLight == Globals.EditingLight)
            {
                Globals.EditingLight = null;
            }
        }

        Graphics.TilePreviewUpdated = true;
        if (CanClose)
        {
            Visible = false;
        }
    }

    private void btnSelectLightColor_Click(object sender, EventArgs e)
    {
        var colorDialog = new ColorDialog
        {
            Color = Colors.White
        };

        var result = colorDialog.ShowDialog(this);
        if (result == DialogResult.Ok)
        {
            var selectedColor = colorDialog.Color;
            pnlLightColor.BackgroundColor = selectedColor;
            mEditingLight.Color = Color.FromArgb(
                (int)(selectedColor.A * 255),
                (int)(selectedColor.R * 255),
                (int)(selectedColor.G * 255),
                (int)(selectedColor.B * 255)
            );

            Graphics.TilePreviewUpdated = true;
        }
    }

    public void Cancel()
    {
        btnLightEditorClose_Click(null, null);
    }

    private void nudOffsetX_ValueChanged(object sender, EventArgs e)
    {
        if (mEditingLight == null)
        {
            return;
        }

        mEditingLight.OffsetX = (int)nudOffsetX.Value;
        Graphics.TilePreviewUpdated = true;
    }

    private void nudOffsetY_ValueChanged(object sender, EventArgs e)
    {
        if (mEditingLight == null)
        {
            return;
        }

        mEditingLight.OffsetY = (int)nudOffsetY.Value;
        Graphics.TilePreviewUpdated = true;
    }

    private void nudSize_ValueChanged(object sender, EventArgs e)
    {
        if (mEditingLight == null)
        {
            return;
        }

        mEditingLight.Size = (int)nudSize.Value;
        Graphics.TilePreviewUpdated = true;
    }

    private void nudIntensity_ValueChanged(object sender, EventArgs e)
    {
        if (mEditingLight == null)
        {
            return;
        }

        mEditingLight.Intensity = (byte)nudIntensity.Value;
        Graphics.TilePreviewUpdated = true;
    }

    private void nudExpand_ValueChanged(object sender, EventArgs e)
    {
        if (mEditingLight == null)
        {
            return;
        }

        mEditingLight.Expand = (int)nudExpand.Value;
        Graphics.TilePreviewUpdated = true;
    }
}
