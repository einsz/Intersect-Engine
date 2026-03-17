using Intersect.Editor.Forms.Helpers;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Events;

namespace Intersect.Editor.Forms.Editors;

public partial class FrmCommonEvent : EditorForm
{
    private string? mCopiedItem;
    private List<string> mKnownFolders = new();

    protected ListBox? lstGameObjects;
    protected TextBox? txtSearch;
    protected DropDown? cmbFolder;
    protected CheckBox? btnAlphabetical;
    protected Button? btnAddFolder;
    protected Button? btnClearSearch;
    protected Label? lblFolder;
    protected GroupBox? grpCommonEvents;
    protected Button? btnNew;
    protected Button? btnDelete;
    protected Button? btnCopy;
    protected Button? btnPaste;

    public FrmCommonEvent()
    {
        ApplyHooks();
        BuildUI();
        InitLocalization();
        InitEditor();
    }

    private void BuildUI()
    {
        Title = Strings.CommonEventEditor.title;
        MinimumSize = new Size(800, 600);

        lstGameObjects = new ListBox();
        txtSearch = new TextBox { PlaceholderText = Strings.CommonEventEditor.searchplaceholder };
        btnAlphabetical = new CheckBox { Text = "A-Z" };
        cmbFolder = new DropDown();
        btnAddFolder = new Button { Text = "+" };
        btnClearSearch = new Button { Text = "X" };
        lblFolder = new Label { Text = Strings.CommonEventEditor.folderlabel };

        btnNew = new Button { Text = Strings.CommonEventEditor.New };
        btnDelete = new Button { Text = Strings.CommonEventEditor.delete };
        btnCopy = new Button { Text = Strings.CommonEventEditor.copy };
        btnPaste = new Button { Text = Strings.CommonEventEditor.paste };

        grpCommonEvents = new GroupBox { Text = Strings.CommonEventEditor.events };

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

        var toolPanel = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Items = { btnNew, btnDelete, btnCopy, btnPaste }
        };

        var leftPanel = new Panel
        {
            Content = new StackLayout
            {
                Padding = new Padding(5),
                Spacing = 5,
                Items = { searchPanel, folderPanel, lstGameObjects, toolPanel }
            }
        };

        Content = leftPanel;

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        if (lstGameObjects != null) lstGameObjects.SelectedIndexChanged += (s, e) =>
        {
            var evt = GetSelectedEvent();
            btnDelete!.Enabled = evt != null;
            btnCopy!.Enabled = evt != null;
            btnPaste!.Enabled = evt != null && mCopiedItem != null;
            cmbFolder!.Visible = evt != null;
            btnAddFolder!.Visible = evt != null;
            lblFolder!.Visible = evt != null;
            if (evt != null)
            {
                cmbFolder.Text = evt.Folder;
            }
        };

        lstGameObjects.MouseDoubleClick += (s, e) =>
        {
            var evt = GetSelectedEvent();
            if (evt != null)
            {
                // Open event editor - simplified for Eto.Forms
                MessageBox.Show(this, $"Opening event editor for: {evt.Name}", "Event Editor");
                InitEditor();
            }
        };

        if (btnNew != null) btnNew.Click += (s, e) =>
        {
            PacketSender.SendCreateObject(GameObjectType.Event);
        };

        if (btnDelete != null) btnDelete.Click += (s, e) =>
        {
            var evt = GetSelectedEvent();
            if (evt != null)
            {
                var result = MessageBox.Show(this, Strings.CommonEventEditor.deleteprompt,
                    Strings.CommonEventEditor.delete, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    PacketSender.SendDeleteObject(evt);
                }
            }
        };

        if (btnCopy != null) btnCopy.Click += (s, e) =>
        {
            var evt = GetSelectedEvent();
            if (evt != null)
            {
                mCopiedItem = evt.JsonData;
                btnPaste.Enabled = true;
            }
        };

        if (btnPaste != null) btnPaste.Click += (s, e) =>
        {
            var evt = GetSelectedEvent();
            if (evt != null && mCopiedItem != null)
            {
                var result = MessageBox.Show(this, Strings.CommonEventEditor.pasteprompt,
                    Strings.CommonEventEditor.pastetitle, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    evt.Load(mCopiedItem, true);
                    PacketSender.SendSaveObject(evt);
                    InitEditor();
                }
            }
        };
    }

    private void InitLocalization()
    {
//        Text = Strings.CommonEventEditor.title;
        grpCommonEvents!.Text = Strings.CommonEventEditor.events;
        btnNew!.Text = Strings.CommonEventEditor.New;
        btnDelete!.Text = Strings.CommonEventEditor.delete;
        btnCopy!.Text = Strings.CommonEventEditor.copy;
        btnPaste!.Text = Strings.CommonEventEditor.paste;
        btnAlphabetical!.Text = "A-Z";
        txtSearch!.PlaceholderText = Strings.CommonEventEditor.searchplaceholder;
        lblFolder!.Text = Strings.CommonEventEditor.folderlabel;
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Event)
        {
            InitEditor();
        }
    }

    private Intersect.Framework.Core.GameObjects.Events.EventDescriptor? GetSelectedEvent()
    {
        if (lstGameObjects!.SelectedIndex < 0)
            return null;

        var selectedItem = lstGameObjects.Items[lstGameObjects.SelectedIndex];
        if (Guid.TryParse(selectedItem.Key, out var id))
        {
            return Intersect.Framework.Core.GameObjects.Events.EventDescriptor.Get(id);
        }
        return null;
    }

    public void InitEditor()
    {
        var mFolders = new List<string>();
        foreach (var itm in Intersect.Framework.Core.GameObjects.Events.EventDescriptor.Lookup)
        {
            if (((Intersect.Framework.Core.GameObjects.Events.EventDescriptor)itm.Value).CommonEvent)
            {
                if (!string.IsNullOrEmpty(((Intersect.Framework.Core.GameObjects.Events.EventDescriptor)itm.Value).Folder) &&
                    !mFolders.Contains(((Intersect.Framework.Core.GameObjects.Events.EventDescriptor)itm.Value).Folder))
                {
                    mFolders.Add(((Intersect.Framework.Core.GameObjects.Events.EventDescriptor)itm.Value).Folder);
                    if (!mKnownFolders.Contains(((Intersect.Framework.Core.GameObjects.Events.EventDescriptor)itm.Value).Folder))
                    {
                        mKnownFolders.Add(((Intersect.Framework.Core.GameObjects.Events.EventDescriptor)itm.Value).Folder);
                    }
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
            var items = Intersect.Framework.Core.GameObjects.Events.EventDescriptor.Lookup
                .Where(pair => ((Intersect.Framework.Core.GameObjects.Events.EventDescriptor)pair.Value)?.CommonEvent ?? false)
                .OrderBy(p => p.Value?.Name);
            foreach (var pair in items)
            {
                var evt = (Intersect.Framework.Core.GameObjects.Events.EventDescriptor?)pair.Value;
                if (evt != null)
                {
                    lstGameObjects.Items.Add(new ListItem { Key = pair.Key.ToString(), Text = evt.Name ?? "Deleted" });
                }
            }
        }
    }
}
