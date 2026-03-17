using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Framework.Core.GameObjects.Maps.MapList;
using Eto.Forms;
using Eto.Drawing;

namespace Intersect.Editor.Forms.DockingElements;

public partial class FrmMapList : Panel
{
    private TreeGridView _treeGridView;
    private Button _btnAlphabetical;
    private Button _toolSelectMap;
    private Button _btnNewMap;
    private Button _btnNewFolder;
    private Button _btnRename;
    private Button _btnDelete;
    private Button _btnRefreshList;
    private ContextMenu _contextMenu;
    private ButtonMenuItem _newMapMenuItem;
    private ButtonMenuItem _renameMenuItem;
    private ButtonMenuItem _newFolderMenuItem;
    private ButtonMenuItem _deleteMenuItem;
    private ButtonMenuItem _copyIdMenuItem;

    public TreeGridView TreeGridView => _treeGridView;

    public FrmMapList()
    {
        InitializeComponents();
        InitLocalization();
    }

    private void InitializeComponents()
    {
        _treeGridView = new TreeGridView();
        _treeGridView.ShowHeader = false;
        _treeGridView.Columns.Add(new GridColumn
        {
            DataCell = new TextBoxCell(0),
            AutoSize = true,
            Editable = false
        });
        _treeGridView.CellDoubleClick += TreeGridView_CellDoubleClick;

        _btnAlphabetical = new Button { Text = Strings.MapList.alphabetical };
        _btnAlphabetical.Click += BtnAlphabetical_Click;

        _toolSelectMap = new Button { Text = Strings.MapList.selectcurrent };
        _toolSelectMap.Click += ToolSelectMap_Click;

        _btnNewMap = new Button { Text = Strings.MapList.newmap };
        _btnNewMap.Click += BtnNewMap_Click;

        _btnNewFolder = new Button { Text = Strings.MapList.newfolder };
        _btnNewFolder.Click += BtnNewFolder_Click;

        _btnRename = new Button { Text = Strings.MapList.rename };
        _btnRename.Click += BtnRename_Click;

        _btnDelete = new Button { Text = Strings.MapList.delete };
        _btnDelete.Click += BtnDelete_Click;

        _btnRefreshList = new Button { Text = "Refresh" };
        _btnRefreshList.Click += BtnRefreshList_Click;

        _newMapMenuItem = new ButtonMenuItem { Text = Strings.MapList.newmap };
        _newMapMenuItem.Click += BtnNewMap_Click;

        _renameMenuItem = new ButtonMenuItem { Text = Strings.MapList.rename };
        _renameMenuItem.Click += BtnRename_Click;

        _newFolderMenuItem = new ButtonMenuItem { Text = Strings.MapList.newfolder };
        _newFolderMenuItem.Click += BtnNewFolder_Click;

        _deleteMenuItem = new ButtonMenuItem { Text = Strings.MapList.delete };
        _deleteMenuItem.Click += BtnDelete_Click;

        _copyIdMenuItem = new ButtonMenuItem { Text = Strings.MapList.copyid };
        _copyIdMenuItem.Click += CopyIdMenuItem_Click;

        _contextMenu = new ContextMenu(
            _newMapMenuItem,
            _newFolderMenuItem,
            _renameMenuItem,
            _deleteMenuItem,
            _copyIdMenuItem
        );
        _treeGridView.ContextMenu = _contextMenu;

        var buttonRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Items =
            {
                _toolSelectMap,
                _btnNewMap,
                _btnNewFolder,
                _btnRename,
                _btnDelete,
                _btnAlphabetical,
                _btnRefreshList
            }
        };

        var layout = new DynamicLayout();
        layout.AddRow(buttonRow);
        layout.Add(_treeGridView, yscale: true);

        Content = layout;
    }

    private void InitLocalization()
    {
        // Panel doesn't have Text property in Eto.Forms
        _btnAlphabetical.Text = Strings.MapList.alphabetical;
        _toolSelectMap.Text = Strings.MapList.selectcurrent;
        _btnNewMap.Text = Strings.MapList.newmap;
        _btnNewFolder.Text = Strings.MapList.newfolder;
        _btnRename.Text = Strings.MapList.rename;
        _btnDelete.Text = Strings.MapList.delete;
        _newMapMenuItem.Text = Strings.MapList.newmap;
        _renameMenuItem.Text = Strings.MapList.rename;
        _newFolderMenuItem.Text = Strings.MapList.newfolder;
        _deleteMenuItem.Text = Strings.MapList.delete;
        _copyIdMenuItem.Text = Strings.MapList.copyid;
    }

    private void TreeGridView_CellDoubleClick(object sender, GridCellMouseEventArgs e)
    {
        if (_treeGridView.SelectedItem is TreeGridItem item && item.Tag is MapListMap map)
        {
            if (Globals.CurrentMap != null &&
                Globals.CurrentMap.Changed())
            {
                var result = MessageBox.Show(
                    this,
                    Strings.Mapping.savemapdialogue.ToString(),
                    Strings.Mapping.savemap.ToString(),
                    MessageBoxButtons.YesNo,
                    MessageBoxType.Information
                );
                if (result == DialogResult.Yes)
                {
                    SaveMap();
                }
            }

            Globals.MainForm?.EnterMap(map.MapId, true);
        }
    }

    private void SaveMap()
    {
        if (Globals.CurrentTool == EditingTool.Selection)
        {
            if (Globals.Dragging == true)
            {
                Globals.MapEditorWindow?.ProcessSelectionMovement(Globals.CurrentMap, true);
                Globals.MapEditorWindow?.PlaceSelection();
            }
        }

        PacketSender.SendMap(Globals.CurrentMap);
    }

    public void UpdateMapList()
    {
        _treeGridView.DataStore = BuildTreeData();
    }

    private TreeGridItemCollection BuildTreeData()
    {
        var collection = new TreeGridItemCollection();
        PopulateTreeItems(collection, MapList.List);
        return collection;
    }

    private void PopulateTreeItems(TreeGridItemCollection collection, MapList mapList)
    {
        if (mapList?.Items == null)
            return;

        foreach (var item in mapList.Items)
        {
            var treeItem = new TreeGridItem(item.Name) { Tag = item };

            if (item is MapListFolder folder && folder.Children?.Items != null)
            {
                PopulateTreeItems(treeItem.Children, folder.Children);
            }

            collection.Add(treeItem);
        }
    }

    private void BtnRefreshList_Click(object sender, EventArgs e)
    {
        UpdateMapList();
    }

    private void BtnNewFolder_Click(object sender, EventArgs e)
    {
        var selectedItem = _treeGridView.SelectedItem as TreeGridItem;
        PacketSender.SendAddFolder(selectedItem?.Tag as MapListItem);
    }

    private void BtnRename_Click(object sender, EventArgs e)
    {
        var selectedItem = _treeGridView.SelectedItem as TreeGridItem;
        if (selectedItem == null)
        {
            MessageBox.Show(
                this,
                Strings.MapList.selecttorename.ToString(),
                Strings.MapList.rename.ToString(),
                MessageBoxButtons.OK,
                MessageBoxType.Warning
            );
        }
        else
        {
            var inputBox = new TextInputDialog
            {
                Title = Strings.MapList.rename.ToString(),
                Text = selectedItem.Values[0]?.ToString() ?? string.Empty
            };
            if (inputBox.ShowModal(this))
            {
                var newName = inputBox.Text;
                if (!string.IsNullOrEmpty(newName) && selectedItem.Tag is MapListItem mapItem)
                {
                    PacketSender.SendRename(mapItem, newName);
                    selectedItem.Values[0] = newName;
                }
            }
        }
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        var selectedItem = _treeGridView.SelectedItem as TreeGridItem;
        if (selectedItem == null)
        {
            MessageBox.Show(
                this,
                Strings.MapList.selecttodelete.ToString(),
                Strings.MapList.delete.ToString(),
                MessageBoxButtons.OK,
                MessageBoxType.Warning
            );
        }
        else
        {
            var result = MessageBox.Show(
                this,
                Strings.MapList.deleteconfirm.ToString(selectedItem.Values[0]?.ToString() ?? string.Empty),
                Strings.MapList.delete.ToString(),
                MessageBoxButtons.YesNo,
                MessageBoxType.Warning
            );
            if (result == DialogResult.Yes && selectedItem.Tag is MapListItem mapItem)
            {
                PacketSender.SendDelete(mapItem);
            }
        }
    }

    private bool _chronological = false;

    private void BtnAlphabetical_Click(object sender, EventArgs e)
    {
        _chronological = !_chronological;
        UpdateMapList();
    }

    private void BtnNewMap_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show(
            this,
            Strings.Mapping.newmap.ToString(),
            Strings.Mapping.newmapcaption.ToString(),
            MessageBoxButtons.YesNo,
            MessageBoxType.Warning
        );
        if (result != DialogResult.Yes)
            return;

        if (Globals.CurrentMap?.Changed() == true)
        {
            var saveResult = MessageBox.Show(
                this,
                Strings.Mapping.savemapdialogue.ToString(),
                Strings.Mapping.savemap.ToString(),
                MessageBoxButtons.YesNo,
                MessageBoxType.Information
            );
            if (saveResult == DialogResult.Yes)
            {
                SaveMap();
            }
        }

        var selectedItem = _treeGridView.SelectedItem as TreeGridItem;
        if (selectedItem == null)
        {
            PacketSender.SendCreateMap(-1, Globals.CurrentMap.Id, null);
        }
        else
        {
            PacketSender.SendCreateMap(-1, Globals.CurrentMap.Id, selectedItem.Tag as MapListItem);
        }
    }

    private void ToolSelectMap_Click(object sender, EventArgs e)
    {
        if (Globals.CurrentMap != null)
        {
            UpdateMapList();
        }
    }

    private void CopyIdMenuItem_Click(object sender, EventArgs e)
    {
        var selectedItem = _treeGridView.SelectedItem as TreeGridItem;
        if (selectedItem?.Tag is MapListMap map)
        {
            var clipboard = new Clipboard();
            clipboard.Text = map.MapId.ToString();
        }
    }
}

public class TextInputDialog : Dialog<bool>
{
    private TextBox _textBox;

    public TextInputDialog()
    {
        _textBox = new TextBox();
        var okButton = new Button { Text = "OK" };
        okButton.Click += (s, e) =>
        {
            Result = true;
            Close();
        };
        var cancelButton = new Button { Text = "Cancel" };
        cancelButton.Click += (s, e) =>
        {
            Result = false;
            Close();
        };

        Content = new StackLayout
        {
            Padding = 10,
            Spacing = 10,
            Items =
            {
                _textBox,
                new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Items = { okButton, cancelButton }
                }
            }
        };

        DefaultButton = okButton;
        AbortButton = cancelButton;
    }

    public new string Text
    {
        get => _textBox.Text;
        set => _textBox.Text = value;
    }
}
