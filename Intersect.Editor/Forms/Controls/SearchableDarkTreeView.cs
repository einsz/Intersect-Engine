using Eto.Forms;
using Eto.Drawing;
using Intersect.Collections;
using Intersect.Models;

namespace Intersect.Editor.Forms.Controls;

public partial class SearchableDarkTreeView : Panel
{
    private readonly Dictionary<Guid, TreeGridItem> mIdNodeLookup;
    private IGameObjectLookup<IDatabaseObject> mItemProvider;
    private string mPreviousSearchText;

    protected TextBox txtSearch;
    protected TreeGridView treeViewItems;

    public SearchableDarkTreeView()
    {
        mIdNodeLookup = new Dictionary<Guid, TreeGridItem>();

        txtSearch = new TextBox
        {
            PlaceholderText = "Search..."
        };

        treeViewItems = new TreeGridView
        {
            ShowHeader = false,
            AllowMultipleSelection = false
        };

        var nameColumn = new GridColumn
        {
            HeaderText = "Name",
            DataCell = new TextBoxCell(0),
            AutoSize = true,
            Editable = false,
            Expand = true
        };

        treeViewItems.Columns.Add(nameColumn);

        treeViewItems.DataStore = new TreeGridItemCollection();

        txtSearch.TextChanged += TxtSearch_TextChanged;

        Content = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Padding = new Padding(5),
            Items =
            {
                txtSearch,
                new StackLayoutItem(treeViewItems, expand: true)
            }
        };
    }

    public TreeGridItem SelectedNode
    {
        get
        {
            var selected = treeViewItems?.SelectedItem;
            return selected as TreeGridItem;
        }
    }

    public IObject SelectedObject => SelectedNode?.Tag as IObject;

    public Guid SelectedId
    {
        get => SelectedObject?.Id ?? Guid.Empty;
        set
        {
            if (mIdNodeLookup.TryGetValue(value, out var node))
            {
                treeViewItems.SelectedItem = node;
            }
        }
    }

    public IGameObjectLookup<IDatabaseObject> ItemProvider
    {
        get => mItemProvider;
        set
        {
            mItemProvider = value;
            UpdateNodes();
        }
    }

    public string SearchText
    {
        get => txtSearch?.Text ?? string.Empty;
        set
        {
            if (txtSearch == null)
            {
                throw new ArgumentNullException(nameof(txtSearch));
            }

            txtSearch.Text = value;
        }
    }

    public virtual new void Refresh()
    {
        UpdateNodes();
    }

    public bool FilterBySearchText(IDatabaseObject databaseObject)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var name = databaseObject?.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var searchText = SearchText.Trim();
        if (searchText.Length > name.Length)
        {
            return false;
        }

        return -1 < name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    protected virtual bool FilterBySearchText(KeyValuePair<Guid, IDatabaseObject> pair)
    {
        return FilterBySearchText(pair.Value);
    }

    protected virtual TreeGridItem ObjectAsNode(IDatabaseObject databaseObject)
    {
        if (!mIdNodeLookup.TryGetValue(databaseObject.Id, out var node))
        {
            node = new TreeGridItem(databaseObject.Name ?? "NULL");
            node.Tag = databaseObject;
        }

        return node;
    }

    protected virtual TreeGridItem PairAsNode(KeyValuePair<Guid, IDatabaseObject> pair)
    {
        return ObjectAsNode(
            pair.Value ??
            throw new ArgumentNullException(nameof(pair.Value), $@"{pair.Key} has a null object associated.")
        );
    }

    protected void UpdateNodes()
    {
        var collection = new TreeGridItemCollection();
        mIdNodeLookup.Clear();

        if (ItemProvider == null)
        {
            treeViewItems.DataStore = collection;
            return;
        }

        mPreviousSearchText = SearchText;

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? ItemProvider
            : ItemProvider.Where(FilterBySearchText);

        foreach (var pair in filtered)
        {
            var node = PairAsNode(pair);
            mIdNodeLookup[pair.Key] = node;
            collection.Add(node);
        }

        treeViewItems.DataStore = collection;
    }

    private void TxtSearch_TextChanged(object sender, EventArgs e)
    {
        var searchText = SearchText?.Trim() ?? "";
        var previousSearchText = mPreviousSearchText?.Trim() ?? "";

        if (string.Equals(searchText, previousSearchText, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        UpdateNodes();
    }
}
