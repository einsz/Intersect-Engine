using Eto.Forms;
using Eto.Drawing;
using Hjg.Pngcs;
using Intersect.Editor.Core;
using Intersect.Editor.Forms;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Framework.Core.GameObjects.Maps.MapList;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;

namespace Intersect.Editor.Maps;

public partial class MapGrid
{
    public Rectangle ContentRect;

    public MapGridItem[,] Grid;

    public int GridHeight = 50;

    public int GridWidth = 50;

    public bool Loaded;

    private MapGridItem mContextMap;

    private ContextMenu mContextMenu;

    private bool mCreateTextures;

    private int mCurrentCellX = -1;

    private int mCurrentCellY = -1;

    private Button mDropDownLinkItem;

    private Button mDropDownUnlinkItem;

    private List<Texture2D> mFreeTextures = new List<Texture2D>();

    private List<Guid> mGridMaps = null;

    private List<Guid> mLinkMaps = new List<Guid>();

    private float mMaxZoom = 1f;

    private float mMinZoom;

    private Button mRecacheMapItem;

    private bool mSizeChanged = true;

    private object mTexLock = new object();

    private List<Texture2D> mTextures = new List<Texture2D>();

    private List<MapGridItem> mToLoad = new List<MapGridItem>();

    private Thread mWorkerThread;

    public bool ShowLines = true;

    public int TileHeight;

    public int TileWidth;

    public Rectangle ViewRect;

    public float Zoom = 1;

    public MapGrid(
        Button dropDownItem,
        Button dropDownUnlink,
        Button recacheItem,
        ContextMenu contextMenu,
        object icon
    )
    {
        mWorkerThread = new Thread(AsyncLoadingThread);
        mWorkerThread.Start();
        mDropDownLinkItem = dropDownItem;
        mDropDownLinkItem.Click += LinkMapItem_Click;
        mContextMenu = contextMenu;
        mDropDownUnlinkItem = dropDownUnlink;
        mDropDownUnlinkItem.Click += UnlinkMapItem_Click;
        mRecacheMapItem = recacheItem;
        mRecacheMapItem.Click += _recacheMapItem_Click;

        Icon = icon as Icon;
    }

    public Icon? Icon { get; set; }

    private void AsyncLoadingThread()
    {
        while (!Globals.ClosingEditor)
        {
            lock (mTexLock)
            {
                if (mToLoad.Count > 0 && mFreeTextures.Count > 0)
                {
                    var itm = mToLoad[0];
                    mToLoad.RemoveAt(0);
                    var tex = mFreeTextures[0];
                    mFreeTextures.RemoveAt(0);

                    var texData = Database.LoadMapCache(itm.MapId, itm.Revision, TileWidth, TileHeight);
                    try
                    {
                        if (texData != null)
                        {
                            tex.SetData(texData);
                            itm.Tex = tex;
                        }
                        else
                        {
                            mFreeTextures.Add(tex);
                        }
                    }
                    catch (Exception ex)
                    {
                        mFreeTextures.Add(tex);
                        mToLoad.Add(itm);
                    }
                }
            }
        }
    }

    public object GetMapGridLock()
    {
        return mTexLock;
    }

    public void Load(string[,] mapGrid)
    {
        lock (GetMapGridLock())
        {
            Loaded = false;
            var gridMaps = new List<Guid>();
            GridWidth = mapGrid.GetLength(0);
            GridHeight = mapGrid.GetLength(1);
            UnloadTextures();
            Grid = new MapGridItem[GridWidth, GridHeight];
            for (var x = -1; x <= GridWidth; x++)
            {
                for (var y = -1; y <= GridHeight; y++)
                {
                    if (y == -1 || y == GridHeight || x == -1 || x == GridWidth)
                    {
                    }
                    else
                    {
                        if (mapGrid[x, y] == null)
                        {
                            Grid[x, y] = new MapGridItem(Guid.Empty);
                        }
                        else
                        {
                            var obj = JObject.Parse(mapGrid[x, y]);
                            Grid[x, y] = new MapGridItem(
                                Guid.Parse(obj["Guid"].ToString()),
                                obj["Name"].ToString(),
                                int.Parse(obj["Revision"].ToString())
                            );

                            gridMaps.Add(Grid[x, y].MapId);
                        }
                    }
                }
            }

            mLinkMaps.Clear();
            for (var i = 0; i < MapList.OrderedMaps.Count; i++)
            {
                if (!gridMaps.Contains(MapList.OrderedMaps[i].MapId))
                {
                    mLinkMaps.Add(MapList.OrderedMaps[i].MapId);
                }
            }

            mMaxZoom = 1f;
            Zoom = mMinZoom;
            TileWidth = (int)(Options.Instance.Map.TileWidth * Options.Instance.Map.MapWidth * Zoom);
            TileHeight = (int)(Options.Instance.Map.TileHeight * Options.Instance.Map.MapHeight * Zoom);
            ContentRect = new Rectangle(
                ViewRect.Width / 2 - TileWidth * (GridWidth + 2) / 2,
                ViewRect.Height / 2 - TileHeight * (GridHeight + 2) / 2,
                TileWidth * (GridWidth + 2),
                TileHeight * (GridHeight + 2)
            );

            mCreateTextures = true;
            Loaded = true;
            mGridMaps = gridMaps;
        }
    }

    public void DoubleClick(int x, int y)
    {
        for (var x1 = 1; x1 < GridWidth + 1; x1++)
        {
            for (var y1 = 1; y1 < GridHeight + 1; y1++)
            {
                var tileRect = new Rectangle(
                    ContentRect.X + x1 * TileWidth,
                    ContentRect.Y + y1 * TileHeight,
                    TileWidth,
                    TileHeight
                );

                if (tileRect.Contains(new Eto.Drawing.Point((int)x, (int)y)))
                {
                    if (Grid[x1 - 1, y1 - 1].MapId != Guid.Empty)
                    {
                        if (Globals.CurrentMap != null &&
                            Globals.CurrentMap.Changed() &&
                            MessageBox.Show(
                                Strings.Mapping.savemapdialogue,
                                Strings.Mapping.savemap,
                                MessageBoxButtons.YesNo
                            ) == DialogResult.Yes)
                        {
                            SaveMap();
                        }

                        Globals.MainForm.EnterMap(Grid[x1 - 1, y1 - 1].MapId, true);
                        Globals.MapEditorWindow.Select();
                    }
                }
            }
        }
    }

    private void SaveMap()
    {
        if (Globals.CurrentTool == EditingTool.Selection)
        {
            if (Globals.Dragging == true)
            {
                Globals.MapEditorWindow.ProcessSelectionMovement(Globals.CurrentMap, true);
                Globals.MapEditorWindow.PlaceSelection();
            }
        }

        PacketSender.SendMap(Globals.CurrentMap);
    }

    public void ScreenshotWorld()
    {
        var fileDialog = new SaveFileDialog
        {
            Title = Strings.MapGrid.savescreenshotdialogue
        };

        fileDialog.Filters.Add(new FileFilter("PNG Image", ".png"));
        fileDialog.Filters.Add(new FileFilter("JPEG Image", ".jpg"));
        fileDialog.Filters.Add(new FileFilter("Bitmap Image", ".bmp"));
        fileDialog.Filters.Add(new FileFilter("GIF Image", ".gif"));

        var result = fileDialog.ShowDialog(null);
        if (result == DialogResult.Ok && !string.IsNullOrEmpty(fileDialog.FileName))
        {
            if (MessageBox.Show(
                    Strings.MapGrid.savescreenshotconfirm,
                    Strings.MapGrid.savescreenshottitle,
                    MessageBoxButtons.YesNo
                ) == DialogResult.Yes)
            {
                FetchMissingPreviews(false);
                Globals.PreviewProgressForm = new FrmProgress();
                Globals.PreviewProgressForm.SetTitle(Strings.MapGrid.savingscreenshot);
                var screenShotThread = new Thread(() => ScreenshotWorld(fileDialog.FileName));
                screenShotThread.Start();
                Globals.PreviewProgressForm.Show();
            }
        }
    }

    void ScreenshotWorld(string filename)
    {
        var rowSize = Options.Instance.Map.TileHeight * Options.Instance.Map.MapHeight;
        var colSize = Options.Instance.Map.MapWidth * Options.Instance.Map.TileWidth;
        var cols = colSize * GridWidth;
        var rows = rowSize * GridHeight;
        var tmpBitmap = new System.Drawing.Bitmap(colSize, rowSize);
        var g = System.Drawing.Graphics.FromImage(tmpBitmap);
        var png = new PngWriter(
            new FileStream(filename, FileMode.OpenOrCreate),
            new ImageInfo(cols, rows, 16, true)
        );

        var pngReaderDict = new Dictionary<Guid, System.Drawing.Bitmap>();
        var cacheRow = 0;

        for (var y = 0; y < rows; y++)
        {
            var gridRow = (int)Math.Floor(y / (double)rowSize);
            if (gridRow != cacheRow)
            {
                foreach (var cache in pngReaderDict)
                {
                    cache.Value.Dispose();
                }

                pngReaderDict.Clear();
                cacheRow = gridRow;
            }

            var row = new byte[png.ImgInfo.Cols * 4];
            for (var x = 0; x < GridWidth; x++)
            {
                var gridCol = x;

                var item = Grid[gridCol, gridRow];
                if (item.MapId != Guid.Empty)
                {
                    System.Drawing.Bitmap reader = null;
                    if (pngReaderDict.ContainsKey(item.MapId))
                    {
                        reader = pngReaderDict[item.MapId];
                    }
                    else
                    {
                        var data = Database.LoadMapCacheRaw(item.MapId, item.Revision);
                        if (data != null)
                        {
                            reader = new System.Drawing.Bitmap(new MemoryStream(data));
                            pngReaderDict.Add(item.MapId, reader);
                        }
                    }

                    if (reader != null)
                    {
                        var rowNum = y - gridRow * rowSize;

                        for (var x1 = x * colSize; x1 < x * colSize + colSize; x1++)
                        {
                            var clr = reader.GetPixel(x1 - x * colSize, rowNum);
                            row[x1 * 4] = clr.R;
                            row[x1 * 4 + 1] = clr.G;
                            row[x1 * 4 + 2] = clr.B;
                            row[x1 * 4 + 3] = clr.A;
                        }
                    }
                    else
                    {
                        for (var x1 = x * colSize; x1 < x * colSize + colSize; x1++)
                        {
                            row[x1 * 4] = 0;
                            row[x1 * 4 + 1] = 255;
                            row[x1 * 4 + 2] = 0;
                            row[x1 * 4 + 3] = 255;
                        }
                    }
                }
                else
                {
                    for (var x1 = x * colSize; x1 < x * colSize + colSize; x1++)
                    {
                        row[x1 * 4] = System.Drawing.Color.Gray.R;
                        row[x1 * 4 + 1] = System.Drawing.Color.Gray.G;
                        row[x1 * 4 + 2] = System.Drawing.Color.Gray.B;
                        row[x1 * 4 + 3] = System.Drawing.Color.Gray.A;
                    }
                }
            }

            png.WriteRowByte(row, y);
            Globals.PreviewProgressForm.SetProgress(
                Strings.MapGrid.savingrow.ToString(y, rows),
                (int)(y / (float)rows * 100),
                false
            );

            Application.Instance.Invoke(() => { });
        }

        png.End();
        Globals.PreviewProgressForm.NotifyClose();
    }

    public bool Contains(Guid mapId)
    {
        if (Grid != null && Loaded)
        {
            return mGridMaps.Contains(mapId);
        }

        return false;
    }

    public void ResetForm()
    {
        lock (mTexLock)
        {
            UnloadTextures();
            mCreateTextures = true;
        }
    }

    public void FetchMissingPreviews(bool clearAllFirst)
    {
        var maps = new List<Guid>();
        if (clearAllFirst)
        {
            if (MessageBox.Show(
                    Strings.MapGrid.clearandfetch,
                    Strings.MapGrid.fetchcaption,
                    MessageBoxButtons.YesNo
                ) != DialogResult.Yes)
            {
                return;
            }

            if (MessageBox.Show(
                    Strings.MapGrid.keepmapcache,
                    Strings.MapGrid.mapcachecaption,
                    MessageBoxButtons.YesNo
                ) == DialogResult.Yes)
            {
                Database.GridHideOverlay = Core.Graphics.HideOverlay;
                Database.GridHideDarkness = Core.Graphics.HideDarkness;
                Database.GridHideFog = Core.Graphics.HideFog;
                Database.GridHideResources = Core.Graphics.HideResources;
                if (Core.Graphics.LightColor != null)
                {
                    Database.GridLightColor = System.Drawing.Color.FromArgb(
                            Core.Graphics.LightColor.A,
                            Core.Graphics.LightColor.R,
                            Core.Graphics.LightColor.G,
                            Core.Graphics.LightColor.B
                        )
                        .ToArgb();
                }
                else
                {
                    Database.GridLightColor = System.Drawing.Color.FromArgb(255, 255, 255, 255).ToArgb();
                }
            }
            else
            {
                Database.GridHideOverlay = true;
                Database.GridHideDarkness = true;
                Database.GridHideFog = true;
                Database.GridHideResources = false;
                Database.GridLightColor = System.Drawing.Color.White.ToArgb();
            }

            Database.SaveGridOptions();
            Database.ClearAllMapCache();
        }

        for (var x = 0; x < GridWidth; x++)
        {
            for (var y = 0; y < GridHeight; y++)
            {
                if (Grid[x, y].MapId != Guid.Empty)
                {
                    var img = Database.LoadMapCacheLegacy(Grid[x, y].MapId, Grid[x, y].Revision);
                    if (img == null)
                    {
                        maps.Add(Grid[x, y].MapId);
                    }
                    else
                    {
                        img.Dispose();
                    }
                }
            }
        }

        if (maps.Count > 0)
        {
            if (clearAllFirst ||
                MessageBox.Show(
                    Strings.MapGrid.justfetch,
                    Strings.MapGrid.fetchcaption,
                    MessageBoxButtons.YesNo
                ) == DialogResult.Yes)
            {
                Globals.FetchingMapPreviews = true;
                Globals.PreviewProgressForm = new FrmProgress();
                Globals.PreviewProgressForm.SetTitle(Strings.MapGrid.fetchingmaps);
                Globals.PreviewProgressForm.SetProgress(
                    Strings.MapGrid.fetchingprogress.ToString(0, maps.Count),
                    0,
                    false
                );

                Globals.FetchCount = maps.Count;
                Globals.MapsToFetch = maps;
                for (var i = 0; i < maps.Count; i++)
                {
                    PacketSender.SendNeedMap(maps[i]);
                }

                Globals.PreviewProgressForm.Show();
            }
        }
    }

    public void RightClickGrid(int x, int y, Panel mapGridView)
    {
        for (var x1 = 0; x1 < GridWidth + 2; x1++)
        {
            for (var y1 = 0; y1 < GridHeight + 2; y1++)
            {
                var tileRect = new Rectangle(
                    ContentRect.X + x1 * TileWidth,
                    ContentRect.Y + y1 * TileHeight,
                    TileWidth,
                    TileHeight
                );

                if (tileRect.Contains(new Eto.Drawing.Point((int)x, (int)y)))
                {
                    mCurrentCellX = x1;
                    mCurrentCellY = y1;
                    if (mCurrentCellX >= 0 && mCurrentCellY >= 0)
                    {
                        if (mCurrentCellX >= 0 &&
                            mCurrentCellY >= 0 &&
                            mCurrentCellX - 1 <= GridWidth &&
                            mCurrentCellY - 1 <= GridHeight)
                        {
                            if (mCurrentCellX == 0 ||
                                mCurrentCellY == 0 ||
                                mCurrentCellX - 1 == GridWidth ||
                                mCurrentCellY - 1 == GridHeight ||
                                Grid[mCurrentCellX - 1, mCurrentCellY - 1].MapId == Guid.Empty)
                            {
                                var adjacentMap = Guid.Empty;

                                if (mCurrentCellX > 1 && mCurrentCellY != 0 && mCurrentCellY - 1 < GridHeight)
                                {
                                    if (Grid[mCurrentCellX - 2, mCurrentCellY - 1].MapId != Guid.Empty)
                                    {
                                        adjacentMap = Grid[mCurrentCellX - 2, mCurrentCellY - 1].MapId;
                                    }
                                }

                                if (mCurrentCellX < GridWidth &&
                                    mCurrentCellY != 0 &&
                                    mCurrentCellY - 1 < GridHeight)
                                {
                                    if (Grid[mCurrentCellX, mCurrentCellY - 1].MapId != Guid.Empty)
                                    {
                                        adjacentMap = Grid[mCurrentCellX, mCurrentCellY - 1].MapId;
                                    }
                                }

                                if (mCurrentCellX != 0 && mCurrentCellY > 1 && mCurrentCellX - 1 < GridWidth)
                                {
                                    if (Grid[mCurrentCellX - 1, mCurrentCellY - 2].MapId != Guid.Empty)
                                    {
                                        adjacentMap = Grid[mCurrentCellX - 1, mCurrentCellY - 2].MapId;
                                    }
                                }

                                if (mCurrentCellX != 0 &&
                                    mCurrentCellY < GridHeight &&
                                    mCurrentCellX - 1 < GridWidth)
                                {
                                    if (Grid[mCurrentCellX - 1, mCurrentCellY].MapId != Guid.Empty)
                                    {
                                        adjacentMap = Grid[mCurrentCellX - 1, mCurrentCellY].MapId;
                                    }
                                }

                                if (adjacentMap != Guid.Empty)
                                {
                                    mContextMenu.Show();
                                    mDropDownUnlinkItem.Visible = false;
                                    mDropDownLinkItem.Visible = true;
                                    mRecacheMapItem.Visible = false;
                                }
                            }
                            else
                            {
                                mContextMap = Grid[mCurrentCellX - 1, mCurrentCellY - 1];
                                mContextMenu.Show();
                                mDropDownUnlinkItem.Visible = true;
                                mRecacheMapItem.Visible = true;
                                mDropDownLinkItem.Visible = false;
                            }
                        }
                    }

                    return;
                }
            }
        }

        mCurrentCellX = -1;
        mCurrentCellY = -1;
    }

    private void UnlinkMapItem_Click(object sender, EventArgs e)
    {
        if (mContextMap != null && mContextMap.MapId != Guid.Empty)
        {
            if (MessageBox.Show(
                    Strings.MapGrid.unlinkprompt.ToString(mContextMap.Name),
                    Strings.MapGrid.unlinkcaption,
                    MessageBoxButtons.YesNo
                ) == DialogResult.Yes)
            {
                PacketSender.SendUnlinkMap(mContextMap.MapId);
            }
        }
    }

    private void _recacheMapItem_Click(object sender, EventArgs e)
    {
        if (mContextMap != null && mContextMap.MapId != Guid.Empty)
        {
            Database.SaveMapCache(mContextMap.MapId, mContextMap.Revision, null);
            if (MapInstance.Get(mContextMap.MapId) != null)
            {
                MapInstance.Get(mContextMap.MapId).Delete();
            }

            Globals.MapsToFetch = new List<Guid>() { mContextMap.MapId };
            PacketSender.SendNeedMap(mContextMap.MapId);
        }
    }

    private void LinkMapItem_Click(object sender, EventArgs e)
    {
        var frmWarpSelection = new FrmWarpSelection();
        frmWarpSelection.InitForm(false, mLinkMaps);
        frmWarpSelection.ShowModal(Application.Instance.MainForm);
        if (frmWarpSelection.GetResult())
        {
            var linkMapId = frmWarpSelection.GetMap();
            var adjacentMapId = Guid.Empty;

            if (mCurrentCellX > 1 && mCurrentCellY != 0 && mCurrentCellY - 1 < GridHeight)
            {
                if (Grid[mCurrentCellX - 2, mCurrentCellY - 1].MapId != Guid.Empty)
                {
                    adjacentMapId = Grid[mCurrentCellX - 2, mCurrentCellY - 1].MapId;
                }
            }

            if (mCurrentCellX < GridWidth && mCurrentCellY != 0 && mCurrentCellY - 1 < GridHeight)
            {
                if (Grid[mCurrentCellX, mCurrentCellY - 1].MapId != Guid.Empty)
                {
                    adjacentMapId = Grid[mCurrentCellX, mCurrentCellY - 1].MapId;
                }
            }

            if (mCurrentCellX != 0 && mCurrentCellY > 1 && mCurrentCellX - 1 < GridWidth)
            {
                if (Grid[mCurrentCellX - 1, mCurrentCellY - 2].MapId != Guid.Empty)
                {
                    adjacentMapId = Grid[mCurrentCellX - 1, mCurrentCellY - 2].MapId;
                }
            }

            if (mCurrentCellX != 0 && mCurrentCellY < GridHeight && mCurrentCellX - 1 < GridWidth)
            {
                if (Grid[mCurrentCellX - 1, mCurrentCellY].MapId != Guid.Empty)
                {
                    adjacentMapId = Grid[mCurrentCellX - 1, mCurrentCellY].MapId;
                }
            }

            if (adjacentMapId != Guid.Empty)
            {
                PacketSender.SendLinkMap(adjacentMapId, linkMapId, mCurrentCellX - 1, mCurrentCellY - 1);
            }
        }
    }

    public void Update(Microsoft.Xna.Framework.Rectangle panelBounds)
    {
        mMinZoom = Math.Min(
                       panelBounds.Width / (float)(Options.Instance.Map.TileWidth * Options.Instance.Map.MapWidth * (GridWidth + 2)),
                       panelBounds.Height / (float)(Options.Instance.Map.TileHeight * Options.Instance.Map.MapHeight * (GridHeight + 2))
                   ) /
                   2f;

        if (Zoom < mMinZoom)
        {
            Zoom = mMinZoom * 2;
            TileWidth = (int)(Options.Instance.Map.TileWidth * Options.Instance.Map.MapWidth * Zoom);
            TileHeight = (int)(Options.Instance.Map.TileHeight * Options.Instance.Map.MapHeight * Zoom);
            ContentRect = new Rectangle(0, 0, TileWidth * (GridWidth + 2), TileHeight * (GridHeight + 2));
            lock (mTexLock)
            {
                UnloadTextures();
            }

            mCreateTextures = true;
        }

        ViewRect = new Rectangle(panelBounds.Left, panelBounds.Top, panelBounds.Width, panelBounds.Height);
        if (ContentRect.X + TileWidth > ViewRect.Width)
        {
            ContentRect.X = ViewRect.Width - TileWidth;
        }

        if (ContentRect.X + ContentRect.Width < TileWidth)
        {
            ContentRect.X = -ContentRect.Width + TileWidth;
        }

        if (ContentRect.Y + TileHeight > ViewRect.Height)
        {
            ContentRect.Y = ViewRect.Height - TileHeight;
        }

        if (ContentRect.Y + ContentRect.Height < TileHeight)
        {
            ContentRect.Y = -ContentRect.Height + TileHeight;
        }

        if (mCreateTextures)
        {
            CreateTextures(panelBounds);
            mCreateTextures = false;
        }

        lock (GetMapGridLock())
        {
            if (Grid != null)
            {
                lock (mTexLock)
                {
                    for (var x = 0; x < GridWidth; x++)
                    {
                        for (var y = 0; y < GridHeight; y++)
                        {
                            var tileRect = new Rectangle(
                                ContentRect.X + (x + 1) * TileWidth,
                                ContentRect.Y + (y + 1) * TileHeight,
                                TileWidth,
                                TileHeight
                            );

                            if (tileRect.Intersects(ViewRect) && Grid[x, y] != null)
                            {
                                if ((Grid[x, y].Tex == null || Grid[x, y].Tex.IsDisposed) &&
                                    Grid[x, y].MapId != Guid.Empty &&
                                    !mToLoad.Contains(Grid[x, y]))
                                {
                                    mToLoad.Add(Grid[x, y]);
                                }
                            }
                            else
                            {
                                if (mToLoad.Contains(Grid[x, y]))
                                {
                                    mToLoad.Remove(Grid[x, y]);
                                }

                                if (Grid[x, y] != null && Grid[x, y].Tex != null)
                                {
                                    mFreeTextures.Add(Grid[x, y].Tex);
                                    Grid[x, y].Tex = null;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public MapGridItem GetItemAt(int mouseX, int mouseY)
    {
        if (Loaded)
        {
            lock (mTexLock)
            {
                for (var x = 0; x < GridWidth; x++)
                {
                    for (var y = 0; y < GridHeight; y++)
                    {
                        var tileRect = new Rectangle(
                            ContentRect.X + (x + 1) * TileWidth,
                            ContentRect.Y + (y + 1) * TileHeight,
                            TileWidth,
                            TileHeight
                        );

                        if (tileRect.Contains(new Eto.Drawing.Point((int)mouseX, (int)mouseY)) && Grid[x, y] != null)
                        {
                            return Grid[x, y];
                        }
                    }
                }
            }
        }

        return null;
    }

    private void CreateTextures(Microsoft.Xna.Framework.Rectangle panelBounds)
    {
        lock (mTexLock)
        {
            var hCount = (int)Math.Ceiling((float)panelBounds.Width / TileWidth) + 2;
            var wCount = (int)Math.Ceiling((float)panelBounds.Height / TileHeight) + 2;
            for (var i = 0; i < hCount * wCount && i < GridWidth * GridHeight; i++)
            {
                mTextures.Add(new Texture2D(Core.Graphics.GetGraphicsDevice(), TileWidth, TileHeight));
            }

            mFreeTextures.AddRange(mTextures.ToArray());
        }
    }

    private void UnloadTextures()
    {
        lock (mTexLock)
        {
            for (var i = 0; i < mTextures.Count; i++)
            {
                mTextures[i].Dispose();
            }

            mTextures.Clear();
            mFreeTextures.Clear();
            if (Grid != null && Loaded)
            {
                for (var x = 0; x < GridWidth; x++)
                {
                    for (var y = 0; y < GridHeight; y++)
                    {
                        Grid[x, y].Tex = null;
                    }
                }
            }
        }
    }

    public void Move(int x, int y)
    {
        ContentRect.X -= x;
        ContentRect.Y -= y;
        if (ContentRect.X + TileWidth > ViewRect.Width)
        {
            ContentRect.X = ViewRect.Width - TileWidth;
        }

        if (ContentRect.X + ContentRect.Width < TileWidth)
        {
            ContentRect.X = -ContentRect.Width + TileWidth;
        }

        if (ContentRect.Y + TileHeight > ViewRect.Height)
        {
            ContentRect.Y = ViewRect.Height - TileHeight;
        }

        if (ContentRect.Y + ContentRect.Height < TileHeight)
        {
            ContentRect.Y = -ContentRect.Height + TileHeight;
        }
    }

    public void ZoomIn(int val, int mouseX, int mouseY)
    {
        var amt = val / 120;

        var x1 = (double)Math.Min(ContentRect.Width, Math.Max(0, mouseX - ContentRect.X)) / (float)TileWidth;
        var y1 = (double)Math.Min(ContentRect.Height, Math.Max(0, mouseY - ContentRect.Y)) / (float)TileHeight;
        var prevZoom = Zoom;
        Zoom += .05f * amt;
        if (prevZoom != Zoom)
        {
            lock (mTexLock)
            {
                UnloadTextures();
            }

            mCreateTextures = true;
        }

        if (Zoom < mMinZoom)
        {
            Zoom = mMinZoom;
        }

        if (Zoom > mMaxZoom)
        {
            Zoom = mMaxZoom;
        }

        TileWidth = (int)(Options.Instance.Map.TileWidth * Options.Instance.Map.MapWidth * Zoom);
        TileHeight = (int)(Options.Instance.Map.TileHeight * Options.Instance.Map.MapHeight * Zoom);

        var x2 = (int)(x1 * TileWidth);
        var y2 = (int)(y1 * TileHeight);

        ContentRect = new Rectangle(
            -x2 + mouseX,
            -y2 + mouseY,
            TileWidth * (GridWidth + 2),
            TileHeight * (GridHeight + 2)
        );
    }
}
