using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Networking;
using Intersect.Framework.Core.GameObjects.Maps.MapList;
using Microsoft.Extensions.Logging;

namespace Intersect.Editor.Forms.Controls;

public partial class MapTreeList : Panel
{
    private readonly Dictionary<MapListItem, TreeGridItem> _nodeLookup = new();

    public delegate void TryUpdateMapList(Guid selectMap, List<Guid> restrictMaps = null);

    public bool Chronological = false;

    public TryUpdateMapList MapListDelegate;

    private bool mCanEdit;

    private List<Guid> mOpenFolders = new List<Guid>();

    private List<Guid>? mRestrictMapIds;

    private Guid mSelectedMap = Guid.Empty;

    private int mSelectionType = -1;

    protected TreeGridView list;
    protected ContextMenu mContextMenu;
    protected ButtonMenuItem mDropDownLinkItem;
    protected ButtonMenuItem mDropDownUnlinkItem;
    protected ButtonMenuItem mRecacheMapItem;

    public MapTreeList()
    {
        list = new TreeGridView
        {
            ShowHeader = false,
            AllowMultipleSelection = false,
            AllowDrop = false
        };

        var nameColumn = new GridColumn
        {
            HeaderText = "Name",
            DataCell = new TextBoxCell(0),
            AutoSize = true,
            Editable = false,
            Expand = true
        };

        list.Columns.Add(nameColumn);
        list.DataStore = new TreeGridItemCollection();

        mDropDownLinkItem = new ButtonMenuItem { Text = "Link Map" };
        mDropDownUnlinkItem = new ButtonMenuItem { Text = "Unlink Map" };
        mRecacheMapItem = new ButtonMenuItem { Text = "Recache Map" };

        mContextMenu = new ContextMenu(mDropDownLinkItem, mDropDownUnlinkItem, mRecacheMapItem);

        list.SelectionChanged += treeMapList_AfterSelect;
        list.CellDoubleClick += treeMapList_CellDoubleClick;
        list.MouseDown += treeMapList_MouseDown;
        list.AfterExpand += list_AfterExpand;
        list.AfterCollapse += list_AfterCollapse;

        MapListDelegate = UpdateMapList;

        Content = new StackLayout
        {
            Padding = new Padding(5),
            Items = { new StackLayoutItem(list, expand: true) }
        };
    }

    private void treeMapList_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Buttons == MouseButtons.Alternate)
        {
            var hitTest = list.GetCellAt(e.Location);
            if (hitTest != null && hitTest.Row >= 0)
            {
                var dataStore = list.DataStore as TreeGridItemCollection;
                if (dataStore != null && hitTest.Row < dataStore.Count)
                {
                    list.SelectedItem = dataStore[hitTest.Row];
                }
            }
        }
    }

    private void treeMapList_CellDoubleClick(object sender, GridCellMouseEventArgs e)
    {
    }

    private void treeMapList_AfterSelect(object sender, EventArgs e)
    {
        var selected = list.SelectedItem as TreeGridItem;
        if (selected?.Tag != null)
        {
            switch (selected.Tag)
            {
                case MapListMap mapListMap:
                    mSelectionType = 0;
                    mSelectedMap = mapListMap.MapId;
                    break;

                case MapListFolder mapListFolder:
                    mSelectionType = 1;
                    mSelectedMap = mapListFolder.FolderId;
                    break;

                default:
                    mSelectionType = -1;
                    mSelectedMap = Guid.Empty;
                    break;
            }
        }
    }

    public void UpdateMapList(Guid selectMapId = default, List<Guid>? restrictMaps = null)
    {
        Intersect.Core.ApplicationContext.Context.Value?.Logger.LogInformation("Updating list");
        var selectedMapListMap = selectMapId == default ? default : MapList.List.FindMap(selectMapId);
        if (selectedMapListMap != default && _nodeLookup.TryGetValue(selectedMapListMap, out var treeItem))
        {
            list.SelectedItem = treeItem;
        }
        else
        {
            var collection = new TreeGridItemCollection();
            _nodeLookup.Clear();
            mRestrictMapIds = restrictMaps;
            AddMapListToTree(MapList.List, collection, selectMapId, mRestrictMapIds);
            list.DataStore = collection;
        }
    }

    private void AddMapListToTree(
        MapList mapList,
        TreeGridItemCollection parentCollection,
        Guid selectMapId = default,
        List<Guid>? restrictMaps = null)
    {
        TreeGridItem tmpItem;
        if (Chronological)
        {
            foreach (var map in MapList.OrderedMaps)
            {
                if (restrictMaps != null && !restrictMaps.Contains(map.MapId))
                {
                    continue;
                }

                tmpItem = new TreeGridItem(map.Name);
                tmpItem.Tag = map;
                _nodeLookup[map] = tmpItem;
                parentCollection.Add(tmpItem);

                var selectedId = selectMapId;
                if (selectedId == default && mSelectionType == 0)
                {
                    selectedId = mSelectedMap;
                }

                if (map.MapId == selectMapId)
                {
                    list.SelectedItem = tmpItem;
                }
            }
        }
        else
        {
            foreach (var item in mapList.Items)
            {
                switch (item)
                {
                    case MapListFolder folder:
                        tmpItem = new TreeGridItem(item.Name);
                        tmpItem.Tag = item;
                        _nodeLookup[item] = tmpItem;
                        parentCollection.Add(tmpItem);
                        AddMapListToTree(folder.Children, tmpItem.Children, selectMapId, restrictMaps);

                        if (mOpenFolders.Contains(folder.FolderId))
                        {
                            tmpItem.Expanded = true;
                        }

                        if (mSelectionType == 1 && mSelectedMap == folder.FolderId)
                        {
                            list.SelectedItem = tmpItem;
                        }

                        break;

                    case MapListMap map:
                        if (restrictMaps?.Contains(map.MapId) ?? true)
                        {
                            tmpItem = new TreeGridItem(item.Name);
                            tmpItem.Tag = map;
                            _nodeLookup[item] = tmpItem;
                            parentCollection.Add(tmpItem);

                            var selectedId = selectMapId;
                            if (selectedId == default && mSelectionType == 0)
                            {
                                selectedId = mSelectedMap;
                            }

                            if (map.MapId == selectMapId)
                            {
                                list.SelectedItem = tmpItem;
                            }

                            break;
                        }

                        break;
                }
            }
        }
    }

    public void EnableEditing(ContextMenu menu)
    {
        if (menu != null)
        {
            list.ContextMenu = menu;
        }

        mCanEdit = true;
    }

    public void SetDoubleClick(EventHandler<GridCellMouseEventArgs> handler)
    {
        list.CellDoubleClick += handler;
    }

    public void SetSelect(EventHandler handler)
    {
        list.SelectionChanged += handler;
    }

    private void list_AfterExpand(object sender, TreeGridViewItemEventArgs e)
    {
        if (e.Item?.Tag is MapListFolder folder)
        {
            if (!mOpenFolders.Contains(folder.FolderId))
            {
                mOpenFolders.Add(folder.FolderId);
            }
        }
    }

    private void list_AfterCollapse(object sender, TreeGridViewItemEventArgs e)
    {
        if (e.Item?.Tag is MapListFolder folder)
        {
            mOpenFolders.Remove(folder.FolderId);
        }
    }
}
