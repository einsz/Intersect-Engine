using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Maps;
using Intersect.Editor.Networking;
using Intersect.Framework.Core.GameObjects.Maps.MapList;

namespace Intersect.Editor.Forms;

public class FrmWarpSelection : Dialog<bool>
{
    private Guid mCurrentMapId = Guid.Empty;
    private int mCurrentX;
    private int mCurrentY;
    private Guid mDrawnMap = Guid.Empty;
    private List<Guid>? mRestrictMaps;
    private bool mTileSelection = true;
    private System.Drawing.Image? mMapImage;

    private CheckBox chkAlphabetical;
    private Button btnOk;
    private Button btnCancel;
    private Button btnRefreshPreview;
    private GroupBox grpMapList;
    private GroupBox grpMapPreview;
    private Drawable pnlMap;
    private TreeGridView mapTreeList;
    private UITimer tmrMapCheck;

    public FrmWarpSelection()
    {
        Title = Strings.WarpSelection.title;
        MinimumSize = new Size(800, 600);
        Resizable = true;

        chkAlphabetical = new CheckBox { Text = Strings.WarpSelection.alphabetical };
        chkAlphabetical.CheckedChanged += ChkAlphabetical_CheckedChanged;

        btnOk = new Button { Text = Strings.WarpSelection.okay };
        btnOk.Click += BtnOk_Click;

        btnCancel = new Button { Text = Strings.WarpSelection.cancel };
        btnCancel.Click += (s, e) => Close();

        btnRefreshPreview = new Button { Text = "Refresh", Enabled = false };
        btnRefreshPreview.Click += BtnRefreshPreview_Click;

        mapTreeList = new TreeGridView();
        mapTreeList.Columns.Add(new GridColumn { HeaderText = "Map", DataCell = new TextBoxCell(0) });
        mapTreeList.CellDoubleClick += MapTreeList_CellDoubleClick;

        pnlMap = new Drawable { BackgroundColor = Colors.Black };
        pnlMap.Paint += PnlMap_Paint;
        pnlMap.MouseDown += PnlMap_MouseDown;

        grpMapList = new GroupBox { Text = Strings.WarpSelection.maplist, Content = new StackLayout { Items = { chkAlphabetical, mapTreeList }, Spacing = 5 } };
        grpMapPreview = new GroupBox { Text = Strings.WarpSelection.mappreview, Content = new StackLayout { Items = { pnlMap, btnRefreshPreview }, Spacing = 5 } };

        var mainSplitter = new Splitter
        {
            Position = 250,
            Panel1 = grpMapList,
            Panel2 = grpMapPreview
        };

        var buttonLayout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Items = { null, btnOk, btnCancel }
        };

        var layout = new DynamicLayout { Padding = 10, DefaultSpacing = new Size(5, 5) };
        layout.Add(mainSplitter, yscale: true);
        layout.Add(buttonLayout);

        Content = layout;

        PositiveButtons.Add(btnOk);
        NegativeButtons.Add(btnCancel);

        tmrMapCheck = new UITimer { Interval = 0.5 };
        tmrMapCheck.Elapsed += TmrMapCheck_Tick;

        mapTreeList.Size = new Size(200, 400);
        pnlMap.Size = new Size(Options.Instance.Map.TileWidth * Options.Instance.Map.MapWidth, Options.Instance.Map.TileHeight * Options.Instance.Map.MapHeight);
    }

    public void InitForm(bool tileSelection = true, List<Guid>? restrictMaps = null)
    {
        mRestrictMaps = restrictMaps;
        mTileSelection = tileSelection;
        if (!tileSelection)
        {
            Title = Strings.WarpSelection.mapselectiontitle;
        }
    }

    private void PnlMap_Paint(object sender, PaintEventArgs e)
    {
        // Map preview rendering
    }

    private void MapTreeList_CellDoubleClick(object sender, GridCellMouseEventArgs e)
    {
        var row = mapTreeList.SelectedItem as TreeGridItem;
        if (row?.Tag is MapListMap mapListMap)
        {
            SelectTile(mapListMap.MapId, mCurrentX, mCurrentY);
        }
    }

    public void SelectTile(Guid mapId, int x, int y)
    {
        if (mCurrentMapId != mapId || x != mCurrentX || y != mCurrentY)
        {
            mCurrentMapId = mapId;
            mCurrentX = x;
            mCurrentY = y;
            UpdatePreview();
        }

        btnRefreshPreview.Enabled = mCurrentMapId != Guid.Empty;
    }

    private void UpdatePreview()
    {
        if (mCurrentMapId != Guid.Empty)
        {
            if (mCurrentMapId != mDrawnMap)
            {
                var img = Database.LoadMapCacheLegacy(mCurrentMapId, -1);
                if (img != null)
                {
                    mMapImage = img;
                }
                else
                {
                    if (MapInstance.Get(mCurrentMapId) != null)
                    {
                        MapInstance.Get(mCurrentMapId).Delete();
                    }

                    Globals.MapsToFetch = new List<Guid>() { mCurrentMapId };
                    if (!Globals.MapsToScreenshot.Contains(mCurrentMapId))
                    {
                        Globals.MapsToScreenshot.Add(mCurrentMapId);
                    }

                    PacketSender.SendNeedMap(mCurrentMapId);
                    tmrMapCheck.Start();
                    return;
                }
            }

            mDrawnMap = mCurrentMapId;
            tmrMapCheck.Stop();
            pnlMap.Invalidate();
        }
        else
        {
            mMapImage = null;
            pnlMap.Invalidate();
        }
    }

    private void ChkAlphabetical_CheckedChanged(object? sender, EventArgs e)
    {
        // Update tree list
    }

    private void TmrMapCheck_Tick(object? sender, EventArgs e)
    {
        if (mCurrentMapId != Guid.Empty)
        {
            var img = Database.LoadMapCacheLegacy(mCurrentMapId, -1);
            if (img != null)
            {
                UpdatePreview();
                tmrMapCheck.Stop();
                img.Dispose();
            }
        }
        else
        {
            tmrMapCheck.Stop();
        }
    }

    private void PnlMap_MouseDown(object sender, MouseEventArgs e)
    {
        var x = (int)e.Location.X;
        var y = (int)e.Location.Y;

        if (x >= pnlMap.Width || y >= pnlMap.Height || x < 0 || y < 0)
            return;

        mCurrentX = (int)Math.Floor((double)x / Options.Instance.Map.TileWidth);
        mCurrentY = (int)Math.Floor((double)y / Options.Instance.Map.TileHeight);
        UpdatePreview();
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (mCurrentMapId != Guid.Empty)
        {
            Result = true;
        }
        Close();
    }

    private void BtnRefreshPreview_Click(object? sender, EventArgs e)
    {
        if (mCurrentMapId != Guid.Empty)
        {
            mDrawnMap = Guid.Empty;
            Database.ClearMapCache(mCurrentMapId);
            UpdatePreview();
        }
    }

    public bool Result { get; private set; }

    public bool GetResult() => Result;
    public Guid GetMap() => mCurrentMapId;
    public int GetX() => mCurrentX;
    public int GetY() => mCurrentY;
}
