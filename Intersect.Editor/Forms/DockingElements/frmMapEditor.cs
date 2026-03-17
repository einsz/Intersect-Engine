using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Classes.Maps;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Maps;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Lighting;
using Intersect.Framework.Core.GameObjects.Mapping.Tilesets;
using Intersect.Framework.Core.Config;
using Intersect.Framework.Core.GameObjects.Maps;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;

namespace Intersect.Editor.Forms.DockingElements;

public class FrmMapEditor : Panel
{
    public MapSaveState CurrentMapState;
    public List<MapSaveState> MapRedoStates = new();
    public List<MapSaveState> MapUndoStates = new();

    public Drawable picMap;
    public Scrollable pnlMapContainer;
    private bool mMapChanged;

    public FrmMapEditor()
    {
        picMap = new Drawable { CanFocus = true, Size = new Size(800, 600) };
        picMap.MouseDown += PicMap_MouseDown;
        picMap.MouseMove += PicMap_MouseMove;
        picMap.MouseUp += PicMap_MouseUp;
        picMap.MouseDoubleClick += PicMap_DoubleClick;
        picMap.Paint += PicMap_Paint;

        pnlMapContainer = new Scrollable { Content = picMap, Size = new Size(800, 600) };

        var layout = new DynamicLayout();
        layout.Add(pnlMapContainer, yscale: true);
        Content = layout;

        Text = Strings.Mapping.editortitle;
        Globals.ToolChanged += Globals_ToolChanged;

        // Timer to trigger redraws at ~30fps
        var redrawTimer = new UITimer { Interval = 1.0 / 10.0 }; // 10fps for debugging
        redrawTimer.Elapsed += (s, e) =>
        {
            Core.Graphics.InvalidateMap();
            picMap.Invalidate();
        };
        redrawTimer.Start();

        System.Diagnostics.Debug.WriteLine("FrmMapEditor: Timer started, Drawable size=" + picMap.Size);
    }

    private void Globals_ToolChanged(object sender, EventArgs e)
    {
        SetCursorSpriteInGrid();
    }

    public string Text { get; set; }

    public void InitMapEditor()
    {
        if (Globals.CurrentMap == null) return;

        // Create render targets for map display
        var mapWidth = (Options.Instance.Map.MapWidth + 2) * Options.Instance.Map.TileWidth;
        var mapHeight = (Options.Instance.Map.MapHeight + 2) * Options.Instance.Map.TileHeight;
        Core.Graphics.CreateMapEditorTarget(mapWidth, mapHeight);

        pnlMapContainer.ScrollSize = new Size(mapWidth, mapHeight);

        Core.Graphics.CurrentView = new System.Drawing.Rectangle(
            (pnlMapContainer.Width - Options.Instance.Map.MapWidth * Options.Instance.Map.TileWidth) / 2,
            (pnlMapContainer.Height - Options.Instance.Map.MapHeight * Options.Instance.Map.TileHeight) / 2,
            pnlMapContainer.Width,
            pnlMapContainer.Height
        );

        Globals.MapLayersWindow?.RefreshNpcList();
        Globals.MapPropertiesWindow?.Init(Globals.CurrentMap);
        Globals.CurrentMap.SaveStateAsUnchanged();
    }

    public void UnloadMap()
    {
        ResetUndoRedoStates();
    }

    public void ResetUndoRedoStates()
    {
        MapUndoStates.Clear();
        MapRedoStates.Clear();
        CurrentMapState = null;
    }

    public void PrepUndoState()
    {
        if (CurrentMapState == null && Globals.CurrentMap != null)
        {
            CurrentMapState = Globals.CurrentMap.SaveInternal();
        }
    }

    public void AddUndoState()
    {
        if (CurrentMapState != null && Globals.CurrentMap != null)
        {
            MapUndoStates.Add(CurrentMapState);
            MapRedoStates.Clear();
            CurrentMapState = Globals.CurrentMap.SaveInternal();
        }
        mMapChanged = false;
    }

    public void SetCursorSpriteInGrid() { }

    private void PicMap_MouseDown(object sender, MouseEventArgs e)
    {
        if (Globals.EditingLight != null) return;
        if (Globals.CurrentMap == null) return;

        var mouseX = (int)e.Location.X;
        var mouseY = (int)e.Location.Y;

        if (mouseX < Core.Graphics.CurrentView.Left ||
            mouseY < Core.Graphics.CurrentView.Top ||
            mouseX > Core.Graphics.CurrentView.Left + Options.Instance.Map.MapWidth * Options.Instance.Map.TileWidth ||
            mouseY > Core.Graphics.CurrentView.Top + Options.Instance.Map.MapHeight * Options.Instance.Map.TileHeight)
        {
            if (Globals.Dragging)
            {
                ProcessSelectionMovement(Globals.CurrentMap, true);
                PlaceSelection();
            }
            return;
        }

        if (CurrentMapState == null)
        {
            CurrentMapState = Globals.CurrentMap.SaveInternal();
        }

        if (e.Buttons == MouseButtons.Primary)
        {
            Globals.MouseButton = 0;
            HandlePrimaryMouseDown(mouseX, mouseY);
        }
        else if (e.Buttons == MouseButtons.Alternate)
        {
            Globals.MouseButton = 1;
            HandleAlternateMouseDown(mouseX, mouseY);
        }
    }

    private void HandlePrimaryMouseDown(int mouseX, int mouseY)
    {
        var tmpMap = Globals.CurrentMap;

        if (Globals.CurrentTool == EditingTool.Dropper)
        {
            foreach (var layer in Enumerable.Reverse(Options.Instance.Map.Layers.All))
            {
                if (tmpMap.Layers[layer][Globals.CurTileX, Globals.CurTileY].TilesetId != Guid.Empty)
                {
                    Globals.MapLayersWindow?.SetTileset(TilesetDescriptor.GetName(tmpMap.Layers[layer][Globals.CurTileX, Globals.CurTileY].TilesetId));
                    Globals.CurSelW = 0;
                    Globals.CurSelH = 0;
                    Globals.MapLayersWindow?.SetAutoTile(tmpMap.Layers[layer][Globals.CurTileX, Globals.CurTileY].Autotile);
                    Globals.CurSelX = tmpMap.Layers[layer][Globals.CurTileX, Globals.CurTileY].X;
                    Globals.CurSelY = tmpMap.Layers[layer][Globals.CurTileX, Globals.CurTileY].Y;
                    Globals.CurrentTool = EditingTool.Brush;
                    Globals.MapLayersWindow?.SetLayer(layer);
                    break;
                }
            }
            return;
        }

        if (Globals.CurrentTool == EditingTool.Selection)
        {
            HandleSelectionTool();
            return;
        }

        if (Globals.CurrentTool == EditingTool.Rectangle)
        {
            Globals.CurMapSelX = Globals.CurTileX;
            Globals.CurMapSelY = Globals.CurTileY;
            Globals.CurMapSelW = 0;
            Globals.CurMapSelH = 0;
            return;
        }

        if (Globals.CurrentTool == EditingTool.Fill)
        {
            if (Globals.CurrentLayer == "Attributes")
                SmartFillAttributes(Globals.CurTileX, Globals.CurTileY);
            else if (Options.Instance.Map.Layers.All.Contains(Globals.CurrentLayer))
                SmartFillLayer(Globals.CurTileX, Globals.CurTileY);
            Globals.MouseButton = -1;
            return;
        }

        if (Globals.CurrentTool == EditingTool.Erase)
        {
            if (Globals.CurrentLayer == "Attributes")
                SmartEraseAttributes(Globals.CurTileX, Globals.CurTileY);
            else if (Options.Instance.Map.Layers.All.Contains(Globals.CurrentLayer))
                SmartEraseLayer(Globals.CurTileX, Globals.CurTileY);
            Globals.MouseButton = -1;
            return;
        }

        // Brush tool
        if (Globals.CurrentLayer == "Attributes")
        {
            Globals.MapLayersWindow?.PlaceAttribute(Globals.CurrentMap, Globals.CurTileX, Globals.CurTileY);
            mMapChanged = true;
                    Core.Graphics.InvalidateMap();
        }
        else if (Options.Instance.Map.Layers.All.Contains(Globals.CurrentLayer) && Globals.CurrentTileset != null)
        {
            PlaceTiles(tmpMap);
            mMapChanged = true;
                    Core.Graphics.InvalidateMap();
        }
    }

    private void HandleSelectionTool()
    {
        if (Globals.Dragging)
        {
            if (Globals.CurTileX >= Globals.CurMapSelX + Globals.TotalTileDragX &&
                Globals.CurTileX <= Globals.CurMapSelX + Globals.TotalTileDragX + Globals.CurMapSelW &&
                Globals.CurTileY >= Globals.CurMapSelY + Globals.TotalTileDragY &&
                Globals.CurTileY <= Globals.CurMapSelY + Globals.TotalTileDragY + Globals.CurMapSelH)
            {
                Globals.TileDragX = Globals.CurTileX;
                Globals.TileDragY = Globals.CurTileY;
                return;
            }
            ProcessSelectionMovement(Globals.CurrentMap, true);
            PlaceSelection();
        }
        else
        {
            if (Globals.CurTileX >= Globals.CurMapSelX &&
                Globals.CurTileX <= Globals.CurMapSelX + Globals.CurMapSelW &&
                Globals.CurTileY >= Globals.CurMapSelY &&
                Globals.CurTileY <= Globals.CurMapSelY + Globals.CurMapSelH)
            {
                Globals.Dragging = true;
                Globals.TileDragX = Globals.CurTileX;
                Globals.TileDragY = Globals.CurTileY;
                Globals.TotalTileDragX = 0;
                Globals.TotalTileDragY = 0;
                Globals.SelectionSource = Globals.CurrentMap;
                return;
            }
            Globals.CurMapSelX = Globals.CurTileX;
            Globals.CurMapSelY = Globals.CurTileY;
            Globals.CurMapSelW = 0;
            Globals.CurMapSelH = 0;
        }
    }

    private void HandleAlternateMouseDown(int mouseX, int mouseY)
    {
        if (Globals.CurrentTool == EditingTool.Selection && Globals.Dragging)
        {
            ProcessSelectionMovement(Globals.CurrentMap, true);
            PlaceSelection();
        }

        if (Globals.CurrentLayer == "Attributes" && Globals.CurrentTool == EditingTool.Brush)
        {
            Globals.MapLayersWindow?.RemoveAttribute(Globals.CurrentMap, Globals.CurTileX, Globals.CurTileY);
            mMapChanged = true;
                    Core.Graphics.InvalidateMap();
        }
        else if (Options.Instance.Map.Layers.All.Contains(Globals.CurrentLayer) && Globals.CurrentTool == EditingTool.Brush)
        {
            Globals.CurrentMap.Layers[Globals.CurrentLayer][Globals.CurTileX, Globals.CurTileY].TilesetId = Guid.Empty;
            Globals.CurrentMap.Layers[Globals.CurrentLayer][Globals.CurTileX, Globals.CurTileY].Autotile = 0;
            Globals.CurrentMap.InitAutotiles();
            mMapChanged = true;
                    Core.Graphics.InvalidateMap();
        }
    }

    private void PlaceTiles(MapInstance tmpMap)
    {
        if (Globals.Autotilemode == 0)
        {
            for (var x = 0; x <= Globals.CurSelW; x++)
            {
                for (var y = 0; y <= Globals.CurSelH; y++)
                {
                    var tx = Globals.CurTileX + x;
                    var ty = Globals.CurTileY + y;
                    if (tx >= 0 && tx < Options.Instance.Map.MapWidth &&
                        ty >= 0 && ty < Options.Instance.Map.MapHeight)
                    {
                        tmpMap.Layers[Globals.CurrentLayer][tx, ty].TilesetId = Globals.CurrentTileset.Id;
                        tmpMap.Layers[Globals.CurrentLayer][tx, ty].X = Globals.CurSelX + x;
                        tmpMap.Layers[Globals.CurrentLayer][tx, ty].Y = Globals.CurSelY + y;
                        tmpMap.Layers[Globals.CurrentLayer][tx, ty].Autotile = 0;
                        tmpMap.InitAutotiles();
                    }
                }
            }
        }
        else
        {
            tmpMap.Layers[Globals.CurrentLayer][Globals.CurTileX, Globals.CurTileY].TilesetId = Globals.CurrentTileset.Id;
            tmpMap.Layers[Globals.CurrentLayer][Globals.CurTileX, Globals.CurTileY].X = Globals.CurSelX;
            tmpMap.Layers[Globals.CurrentLayer][Globals.CurTileX, Globals.CurTileY].Y = Globals.CurSelY;
            tmpMap.Layers[Globals.CurrentLayer][Globals.CurTileX, Globals.CurTileY].Autotile = (byte)Globals.Autotilemode;
            tmpMap.InitAutotiles();
        }
    }

    private void PicMap_MouseMove(object sender, MouseEventArgs e)
    {
        if (Globals.EditingLight != null || Globals.CurrentMap == null) return;

        var mouseX = (int)e.Location.X;
        var mouseY = (int)e.Location.Y;

        if (e.Buttons == MouseButtons.Middle)
        {
            Core.Graphics.CurrentView.X -= Globals.MouseX - mouseX;
            Core.Graphics.CurrentView.Y -= Globals.MouseY - mouseY;
            return;
        }

        Globals.MouseX = mouseX;
        Globals.MouseY = mouseY;

        if (mouseX < Core.Graphics.CurrentView.Left ||
            mouseY < Core.Graphics.CurrentView.Top ||
            mouseX > Core.Graphics.CurrentView.Left + Options.Instance.Map.MapWidth * Options.Instance.Map.TileWidth ||
            mouseY > Core.Graphics.CurrentView.Top + Options.Instance.Map.MapHeight * Options.Instance.Map.TileHeight)
            return;

        var oldx = Globals.CurTileX;
        var oldy = Globals.CurTileY;
        Globals.CurTileX = Math.Max(0, Math.Min(
            (int)Math.Floor((double)(mouseX - Core.Graphics.CurrentView.Left) / Options.Instance.Map.TileWidth),
            Options.Instance.Map.MapWidth - 1));
        Globals.CurTileY = Math.Max(0, Math.Min(
            (int)Math.Floor((double)(mouseY - Core.Graphics.CurrentView.Top) / Options.Instance.Map.TileHeight),
            Options.Instance.Map.MapHeight - 1));

        if (oldx != Globals.CurTileX || oldy != Globals.CurTileY)
        {
            Core.Graphics.TilePreviewUpdated = true;
        }

        if (Globals.CurrentTool == EditingTool.Erase || Globals.CurrentTool == EditingTool.Fill || Globals.CurrentTool == EditingTool.Dropper)
            return;

        if (Globals.MouseButton > -1)
        {
            HandleDragPainting();
        }
    }

    private void HandleDragPainting()
    {
        var tmpMap = Globals.CurrentMap;
        if (Globals.MouseButton == 0)
        {
            if (Globals.CurrentTool == EditingTool.Selection && !Globals.Dragging)
            {
                Globals.CurMapSelW = Globals.CurTileX - Globals.CurMapSelX;
                Globals.CurMapSelH = Globals.CurTileY - Globals.CurMapSelY;
            }
            else if (Globals.CurrentTool == EditingTool.Rectangle)
            {
                Globals.CurMapSelW = Globals.CurTileX - Globals.CurMapSelX;
                Globals.CurMapSelH = Globals.CurTileY - Globals.CurMapSelY;
            }
            else if (Options.Instance.Map.Layers.All.Contains(Globals.CurrentLayer) && Globals.CurrentTileset != null)
            {
                PlaceTiles(tmpMap);
            }
            else if (Globals.CurrentLayer == "Attributes")
            {
                Globals.MapLayersWindow?.PlaceAttribute(tmpMap, Globals.CurTileX, Globals.CurTileY);
            }
        }
        else if (Globals.MouseButton == 1)
        {
            if (Globals.CurrentLayer == "Attributes")
            {
                Globals.MapLayersWindow?.RemoveAttribute(tmpMap, Globals.CurTileX, Globals.CurTileY);
            }
            else if (Options.Instance.Map.Layers.All.Contains(Globals.CurrentLayer) && Globals.CurrentTool == EditingTool.Brush)
            {
                tmpMap.Layers[Globals.CurrentLayer][Globals.CurTileX, Globals.CurTileY].TilesetId = Guid.Empty;
                tmpMap.Layers[Globals.CurrentLayer][Globals.CurTileX, Globals.CurTileY].Autotile = 0;
                tmpMap.InitAutotiles();
            }
        }
    }

    private void PicMap_MouseUp(object sender, MouseEventArgs e)
    {
        if (Globals.EditingLight != null || e.Buttons == MouseButtons.Middle) return;

        if (Globals.CurrentTool == EditingTool.Rectangle)
        {
            ApplyRectangleTool();
        }

        Globals.MouseButton = -1;
        if (mMapChanged && CurrentMapState != null)
        {
            MapUndoStates.Add(CurrentMapState);
            MapRedoStates.Clear();
            CurrentMapState = Globals.CurrentMap.SaveInternal();
            mMapChanged = false;
        }

        if (Globals.CurrentTool != EditingTool.Selection)
        {
            Globals.CurMapSelX = Globals.CurTileX;
            Globals.CurMapSelY = Globals.CurTileY;
            Globals.CurMapSelW = 0;
            Globals.CurMapSelH = 0;
        }

        if (Globals.Dragging)
        {
            Globals.TotalTileDragX -= Globals.TileDragX - Globals.CurTileX;
            Globals.TotalTileDragY -= Globals.TileDragY - Globals.CurTileY;
            Globals.TileDragX = 0;
            Globals.TileDragY = 0;
        }

        Core.Graphics.TilePreviewUpdated = true;
    }

    private void ApplyRectangleTool()
    {
        int selX = Globals.CurMapSelX, selY = Globals.CurMapSelY;
        int selW = Globals.CurMapSelW, selH = Globals.CurMapSelH;
        if (selW < 0) { selX -= Math.Abs(selW); selW = Math.Abs(selW); }
        if (selH < 0) { selY -= Math.Abs(selH); selH = Math.Abs(selH); }

        Globals.CurMapSelX = selX; Globals.CurMapSelY = selY;
        Globals.CurMapSelW = selW; Globals.CurMapSelH = selH;

        if (Globals.CurrentLayer == "Attributes")
        {
            for (var x = selX; x < selX + selW + 1; x++)
                for (var y = selY; y < selY + selH + 1; y++)
                {
                    if (Globals.MouseButton == 0)
                        Globals.MapLayersWindow?.PlaceAttribute(Globals.CurrentMap, x, y);
                    else if (Globals.MouseButton == 1)
                        Globals.MapLayersWindow?.RemoveAttribute(Globals.CurrentMap, x, y);
                }
            mMapChanged = true;
                    Core.Graphics.InvalidateMap();
        }
        else if (Options.Instance.Map.Layers.All.Contains(Globals.CurrentLayer) && Globals.CurrentTileset != null)
        {
            for (var x0 = selX; x0 < selX + selW + 1; x0++)
            {
                for (var y0 = selY; y0 < selY + selH + 1; y0++)
                {
                    if (Globals.MouseButton == 0)
                    {
                        var tileX = (x0 - selX) % (Globals.CurSelW + 1);
                        var tileY = (y0 - selY) % (Globals.CurSelH + 1);
                        Globals.CurrentMap.Layers[Globals.CurrentLayer][x0, y0].TilesetId = Globals.CurrentTileset.Id;
                        Globals.CurrentMap.Layers[Globals.CurrentLayer][x0, y0].X = Globals.CurSelX + tileX;
                        Globals.CurrentMap.Layers[Globals.CurrentLayer][x0, y0].Y = Globals.CurSelY + tileY;
                        Globals.CurrentMap.Layers[Globals.CurrentLayer][x0, y0].Autotile = (byte)Globals.Autotilemode;
                    }
                    else if (Globals.MouseButton == 1)
                    {
                        Globals.CurrentMap.Layers[Globals.CurrentLayer][x0, y0].TilesetId = Guid.Empty;
                        Globals.CurrentMap.Layers[Globals.CurrentLayer][x0, y0].X = 0;
                        Globals.CurrentMap.Layers[Globals.CurrentLayer][x0, y0].Y = 0;
                        Globals.CurrentMap.Layers[Globals.CurrentLayer][x0, y0].Autotile = 0;
                    }
                    Globals.CurrentMap.Autotiles.UpdateAutoTiles(x0, y0, Globals.CurrentLayer, Globals.CurrentMap.GenerateAutotileGrid());
                }
            }
            mMapChanged = true;
                    Core.Graphics.InvalidateMap();
        }
    }

    private void PicMap_DoubleClick(object sender, MouseEventArgs e)
    {
        // Handle double-click for editing events, lights, etc.
        if (Globals.CurrentLayer == "Lights")
        {
            var tmpLight = Globals.CurrentMap?.FindLightAt(Globals.CurTileX, Globals.CurTileY);
            if (tmpLight == null)
            {
                tmpLight = new LightDescriptor(Globals.CurTileX, Globals.CurTileY) { Size = 50 };
                Globals.CurrentMap?.Lights.Add(tmpLight);
            }
            Globals.MapLayersWindow?.ShowLightEditor(tmpLight);
            Globals.EditingLight = tmpLight;
            mMapChanged = true;
                    Core.Graphics.InvalidateMap();
        }
        else if (Globals.CurrentLayer == "Events")
        {
            // Event editing would open FrmEvent
        }
    }

    public void FillLayer() { }
    public void EraseLayer() { }
    public void Copy() { }
    public void Paste() { }
    public void Cut() { }
    public void Delete() { }
    public void FlipVertical() { }
    public void FlipHorizontal() { }
    public void ProcessSelectionMovement(MapInstance map, bool finalize, bool preview = false) { }
    public void PlaceSelection() { }
    public void SmartFillLayer(int x, int y) { }
    public void SmartEraseLayer(int x, int y) { }
    public void SmartFillAttributes(int x, int y) { }
    public void SmartEraseAttributes(int x, int y) { }

    private static int sPaintCount = 0;
    private void PicMap_Paint(object sender, PaintEventArgs e)
    {
        sPaintCount++;
        if (sPaintCount <= 5)
        {
            Console.WriteLine($"PicMap_Paint called #{sPaintCount}, clip={e.ClipRectangle}");
        }

        // Use Eto.Forms bitmap rendering (works on Linux without MonoGame headless device)
        var mapBitmap = Core.Graphics.RenderMapToBitmap();
        if (mapBitmap != null)
        {
            e.Graphics.DrawImage(mapBitmap, 0, 0);
        }
        else
        {
            // Draw a red X to show the paint handler is working
            e.Graphics.DrawLine(Colors.Red, 0, 0, e.ClipRectangle.Width, e.ClipRectangle.Height);
            e.Graphics.DrawLine(Colors.Red, e.ClipRectangle.Width, 0, 0, e.ClipRectangle.Height);
        }
    }
}
