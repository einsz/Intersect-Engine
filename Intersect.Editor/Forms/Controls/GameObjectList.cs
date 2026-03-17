using Eto.Forms;
using Eto.Drawing;

namespace Intersect.Editor.Forms.Controls;

public partial class GameObjectList : TreeGridView
{
    private bool mChangingName = false;

    public delegate void UpdateItemDelegate(Guid id);

    public delegate void UpdateToolstripDelegate();

    public delegate void ToolstripButtonClickDelegate(object sender, EventArgs e);

    public UpdateItemDelegate UpdateItemHandler;

    public UpdateToolstripDelegate FocusChangedHandler;

    public ToolstripButtonClickDelegate ToolStripItemNew_Click;

    public ToolstripButtonClickDelegate ToolStripItemCopy_Click;

    public ToolstripButtonClickDelegate ToolStripItemPaste_Click;

    public ToolstripButtonClickDelegate ToolStripItemUndo_Click;

    public ToolstripButtonClickDelegate ToolStripItemDelete_Click;

    private List<string> mExpandedFolders = new List<string>();

    public GameObjectList()
    {
        ShowHeader = false;
        AllowMultipleSelection = false;

        var nameColumn = new GridColumn
        {
            HeaderText = "Name",
            DataCell = new TextBoxCell(0),
            AutoSize = true,
            Editable = false,
            Expand = true
        };

        Columns.Add(nameColumn);
        DataStore = new TreeGridItemCollection();

        SelectionChanged += GameObjectList_AfterSelect;
        MouseDown += GameObjectList_MouseDown;
        GotFocus += GameObjectList_GotFocus;
        LostFocus += GameObjectList_LostFocus;
        KeyDown += GameObjectList_KeyDown;
    }

    public new TreeGridItem SelectedItem => base.SelectedItem as TreeGridItem;

    public void Init(
        UpdateToolstripDelegate updateToolStripHandler,
        UpdateItemDelegate updateItemHandler,
        ToolstripButtonClickDelegate newDelegate,
        ToolstripButtonClickDelegate copyDelegate,
        ToolstripButtonClickDelegate undoDelegate,
        ToolstripButtonClickDelegate pasteDelegate,
        ToolstripButtonClickDelegate deleteDelegate)
    {
        FocusChangedHandler = updateToolStripHandler;
        UpdateItemHandler = updateItemHandler;
        ToolStripItemNew_Click = newDelegate;
        ToolStripItemCopy_Click = copyDelegate;
        ToolStripItemPaste_Click = pasteDelegate;
        ToolStripItemUndo_Click = undoDelegate;
        ToolStripItemDelete_Click = deleteDelegate;
    }

    private void GameObjectList_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Buttons == MouseButtons.Alternate)
        {
            var node = SelectedItem;
            if (node?.Tag is Guid guid)
            {
                var clipboard = new Clipboard();
                clipboard.Text = guid.ToString();
            }
        }

        var selected = SelectedItem;
        if (selected != null)
        {
            if (selected.Expanded)
            {
                if (!mExpandedFolders.Contains(selected.Values?.FirstOrDefault()?.ToString()))
                {
                    var text = selected.Values?.FirstOrDefault()?.ToString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        mExpandedFolders.Add(text);
                    }
                }
            }
            else
            {
                var text = selected.Values?.FirstOrDefault()?.ToString();
                if (!string.IsNullOrEmpty(text) && mExpandedFolders.Contains(text))
                {
                    mExpandedFolders.Remove(text);
                }
            }
        }
    }

    private void GameObjectList_AfterSelect(object sender, EventArgs e)
    {
        if (mChangingName)
        {
            return;
        }

        var selected = SelectedItem;
        if (selected == null || selected.Tag == null)
        {
            return;
        }

        UpdateItemHandler?.Invoke((Guid)selected.Tag);
    }

    private void GameObjectList_LostFocus(object sender, EventArgs e)
    {
        FocusChangedHandler?.Invoke();
    }

    private void GameObjectList_GotFocus(object sender, EventArgs e)
    {
        FocusChangedHandler?.Invoke();
    }

    private void GameObjectList_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Modifiers == Keys.Control)
        {
            if (e.Key == Keys.Z)
            {
                ToolStripItemUndo_Click?.Invoke(null, null);
            }
            else if (e.Key == Keys.V)
            {
                ToolStripItemPaste_Click?.Invoke(null, null);
            }
            else if (e.Key == Keys.C)
            {
                ToolStripItemCopy_Click?.Invoke(null, null);
            }
        }
        else
        {
            if (e.Key == Keys.Delete)
            {
                ToolStripItemDelete_Click?.Invoke(null, null);
            }
        }
    }

    public void UpdateText(string text)
    {
        mChangingName = true;
        var selected = SelectedItem;
        if (selected != null)
        {
            if (selected.Values != null && selected.Values.Length > 0)
            {
                selected.Values[0] = text;
            }
        }

        mChangingName = false;
    }

    public void ExpandFolder(string name)
    {
        if (!mExpandedFolders.Contains(name))
        {
            mExpandedFolders.Add(name);
        }
    }

    public void ClearExpandedFolders()
    {
        mExpandedFolders.Clear();
    }

    public void Repopulate(
        KeyValuePair<Guid, KeyValuePair<string, string>>[] items,
        List<string> folders,
        bool chronological,
        bool customSearch,
        string search)
    {
        var selectedId = Guid.Empty;
        var folderNodes = new Dictionary<string, TreeGridItem>();

        var selected = SelectedItem;
        if (selected?.Tag is Guid tag)
        {
            selectedId = tag;
        }

        var collection = new TreeGridItemCollection();

        var nodes = new List<TreeGridItem>();
        TreeGridItem selectNode = null;

        if (!chronological && !customSearch)
        {
            foreach (var folder in folders)
            {
                var node = new TreeGridItem(folder);
                folderNodes[folder] = node;
                nodes.Add(node);
            }
        }

        foreach (var itm in items)
        {
            var node = new TreeGridItem(itm.Value.Key);
            node.Tag = itm.Key;

            var folder = itm.Value.Value;
            if (!string.IsNullOrEmpty(folder) && !chronological && !customSearch)
            {
                if (folderNodes.TryGetValue(folder, out var folderNode))
                {
                    folderNode.Children.Add(node);
                    if (itm.Key == selectedId)
                    {
                        folderNode.Expanded = true;
                    }
                }
            }
            else
            {
                nodes.Add(node);
            }

            if (customSearch)
            {
                var nodeText = node.Values?.FirstOrDefault()?.ToString() ?? "";
                if (!nodeText.ToLower().Contains(search?.ToLower() ?? ""))
                {
                    nodes.Remove(node);
                }
            }

            if (itm.Key == selectedId)
            {
                selectNode = node;
            }
        }

        foreach (var folderName in mExpandedFolders)
        {
            if (folderNodes.ContainsKey(folderName))
            {
                folderNodes[folderName].Expanded = true;
            }
        }

        foreach (var node in nodes)
        {
            collection.Add(node);
        }

        if (chronological)
        {
            DataStore = new TreeGridItemCollection(
                collection.OrderBy(n => n.Values?.FirstOrDefault()?.ToString() ?? "").Cast<TreeGridItem>()
            );
        }
        else
        {
            DataStore = collection;
        }

        if (selectNode != null)
        {
            SelectedItem = selectNode;
        }
    }
}
