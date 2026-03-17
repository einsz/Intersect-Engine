using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Microsoft.Extensions.Logging;

namespace Intersect.Editor.Forms.Editors;

public partial class EditorForm : Form
{
    private bool mClosing = false;

    protected Button? _btnSave;
    protected Button? _btnCancel;

    protected EditorForm()
    {
        Title = "Editor";
        MinimumSize = new Size(800, 600);
        Size = new Size(1024, 768);

        ApplyHooks();
    }

    protected void ApplyHooks()
    {
        PacketHandler.GameObjectUpdatedDelegate = type =>
        {
            if (mClosing)
            {
                return;
            }

            try
            {
                Application.Instance.Invoke(() => GameObjectUpdatedDelegate(type));
            }
            catch (Exception e)
            {
                Intersect.Core.ApplicationContext.Context.Value?.Logger.LogDebug(e, "Error updating game object");
            }
        };

        this.Closed += EditorForm_Closed;
    }

    private void EditorForm_Closed(object? sender, EventArgs e)
    {
        mClosing = true;
    }

    private void FireGameObjectUpdatedDelegate(GameObjectType type)
    {
        if (mClosing)
        {
            return;
        }

        GameObjectUpdatedDelegate(type);
    }

    protected virtual void GameObjectUpdatedDelegate(GameObjectType type)
    {
    }

    protected void UpdateEditorButtons(bool isItemSelected)
    {
        if (_btnSave != null)
        {
            _btnSave.Visible = isItemSelected;
            _btnSave.Enabled = isItemSelected;
        }

        if (_btnCancel != null)
        {
            _btnCancel.Visible = isItemSelected;
            _btnCancel.Enabled = isItemSelected;
        }
    }
}
