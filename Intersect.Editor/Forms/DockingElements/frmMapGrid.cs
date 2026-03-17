using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Maps;
using Eto.Forms;
using Eto.Drawing;
using Graphics = Intersect.Editor.Core.Graphics;

namespace Intersect.Editor.Forms.DockingElements;

public partial class FrmMapGrid : Panel
{
    private bool _dragging;
    private int _dragX;
    private int _dragY;
    private int _posX;
    private int _posY;
    private MapGridItem _toolTipItem;

    private Drawable _pnlMapGrid;
    private Button _btnScreenshotWorld;
    private Button _btnGridView;
    private Button _btnFetchPreview;
    private Button _btnDownloadMissing;
    private Button _btnReDownloadAll;
    private Button _btnLinkMap;
    private Button _btnUnlinkMap;
    private Button _btnRecacheMap;
    private ContextMenu _contextMenu;

    public Drawable PnlMapGrid => _pnlMapGrid;

    public FrmMapGrid()
    {
        InitializeComponents();
        InitLocalization();
        InitGridWindow();
    }

    private void InitializeComponents()
    {
        _pnlMapGrid = new Drawable();
        _pnlMapGrid.CanFocus = true;

        _btnScreenshotWorld = new Button { Text = Strings.MapGrid.screenshotworld };
        _btnScreenshotWorld.Click += BtnScreenshotWorld_Click;

        _btnGridView = new Button { Text = Strings.MapGrid.gridlines };
        _btnGridView.Click += BtnGridView_Click;

        _btnFetchPreview = new Button { Text = Strings.MapGrid.preview };
        _btnFetchPreview.Click += (s, e) => Globals.MapGrid?.FetchMissingPreviews(false);

        _btnDownloadMissing = new Button { Text = Strings.MapGrid.downloadmissing };
        _btnDownloadMissing.Click += (s, e) => Globals.MapGrid?.FetchMissingPreviews(false);

        _btnReDownloadAll = new Button { Text = Strings.MapGrid.downloadall };
        _btnReDownloadAll.Click += (s, e) => Globals.MapGrid?.FetchMissingPreviews(true);

        _btnLinkMap = new Button { Text = Strings.MapGrid.link };
        _btnUnlinkMap = new Button { Text = Strings.MapGrid.unlink };
        _btnRecacheMap = new Button { Text = Strings.MapGrid.recache };

        var contextLinkMap = new ButtonMenuItem { Text = Strings.MapGrid.link };
        var contextUnlinkMap = new ButtonMenuItem { Text = Strings.MapGrid.unlink };
        var contextRecacheMap = new ButtonMenuItem { Text = Strings.MapGrid.recache };
        var contextDownloadMissing = new ButtonMenuItem { Text = Strings.MapGrid.downloadmissing };
        contextDownloadMissing.Click += (s, e) => Globals.MapGrid?.FetchMissingPreviews(false);
        var contextReDownloadAll = new ButtonMenuItem { Text = Strings.MapGrid.downloadall };
        contextReDownloadAll.Click += (s, e) => Globals.MapGrid?.FetchMissingPreviews(true);

        _contextMenu = new ContextMenu(
            contextLinkMap,
            contextUnlinkMap,
            contextRecacheMap,
            new SeparatorMenuItem(),
            contextDownloadMissing,
            contextReDownloadAll
        );

        var buttonRow = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Items =
            {
                _btnScreenshotWorld,
                _btnGridView,
                _btnFetchPreview,
                _btnDownloadMissing,
                _btnReDownloadAll
            }
        };

        var layout = new DynamicLayout();
        layout.AddRow(buttonRow);
        layout.Add(_pnlMapGrid, yscale: true);

        Content = layout;

        _pnlMapGrid.MouseWheel += PnlMapGrid_MouseWheel;
        _pnlMapGrid.MouseDown += PnlMapGrid_MouseDown;
        _pnlMapGrid.MouseMove += PnlMapGrid_MouseMove;
        _pnlMapGrid.MouseUp += PnlMapGrid_MouseUp;
        _pnlMapGrid.MouseLeave += PnlMapGrid_MouseLeave;
        _pnlMapGrid.MouseDoubleClick += PnlMapGrid_MouseDoubleClick;
        _pnlMapGrid.KeyDown += PnlMapGrid_KeyDown;
        _pnlMapGrid.Paint += PnlMapGrid_Paint;
    }

    private void InitLocalization()
    {
        // Panel doesn't have Text property in Eto.Forms
        _btnGridView.Text = Strings.MapGrid.gridlines;
        _btnFetchPreview.Text = Strings.MapGrid.preview;
        _btnDownloadMissing.Text = Strings.MapGrid.downloadmissing;
        _btnReDownloadAll.Text = Strings.MapGrid.downloadall;
        _btnUnlinkMap.Text = Strings.MapGrid.unlink;
        _btnLinkMap.Text = Strings.MapGrid.link;
        _btnRecacheMap.Text = Strings.MapGrid.recache;
    }

    public void InitGridWindow()
    {
        if (Globals.MapGrid == null)
        {
            Globals.MapGrid = new MapGrid(
                _btnLinkMap,
                _btnUnlinkMap,
                _btnRecacheMap,
                _contextMenu,
                null
            );
        }
    }

    public void ResetForm()
    {
        _dragging = false;
        _toolTipItem = null;
    }

    private void PnlMapGrid_Paint(object sender, PaintEventArgs e)
    {
        Globals.MapGrid?.Draw(e.Graphics, _pnlMapGrid.Width, _pnlMapGrid.Height);
    }

    private void PnlMapGrid_MouseWheel(object sender, MouseEventArgs e)
    {
        Globals.MapGrid?.ZoomIn((int)(e.Delta.Height * 120), (int)e.Location.X, (int)e.Location.Y);
        _pnlMapGrid.Invalidate();
    }

    private void PnlMapGrid_MouseMove(object sender, MouseEventArgs e)
    {
        _posX = (int)e.Location.X;
        _posY = (int)e.Location.Y;

        if (_dragging)
        {
            Globals.MapGrid?.Move(_dragX - _posX, _dragY - _posY);
            _dragX = _posX;
            _dragY = _posY;
            _pnlMapGrid.Invalidate();
        }

        var currentItem = Globals.MapGrid?.GetItemAt(_posX, _posY);
        if (_toolTipItem != null && currentItem != _toolTipItem)
        {
            _toolTipItem = null;
        }
        else if (currentItem != null)
        {
            _toolTipItem = currentItem;
        }
    }

    private void PnlMapGrid_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Buttons == MouseButtons.Primary || e.Buttons == MouseButtons.Middle)
        {
            _dragging = true;
            _dragX = (int)e.Location.X;
            _dragY = (int)e.Location.Y;
        }
        else if (e.Buttons == MouseButtons.Alternate)
        {
            Globals.MapGrid?.RightClickGrid((int)e.Location.X, (int)e.Location.Y, _pnlMapGrid);
        }
    }

    private void PnlMapGrid_MouseUp(object sender, MouseEventArgs e)
    {
        _dragging = false;
    }

    private void PnlMapGrid_MouseLeave(object sender, MouseEventArgs e)
    {
        _toolTipItem = null;
    }

    private void PnlMapGrid_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        Globals.MapGrid?.DoubleClick((int)e.Location.X, (int)e.Location.Y);
    }

    private void BtnGridView_Click(object sender, EventArgs e)
    {
        if (Globals.MapGrid != null)
        {
            Globals.MapGrid.ShowLines = !Globals.MapGrid.ShowLines;
            _pnlMapGrid.Invalidate();
        }
    }

    private void BtnScreenshotWorld_Click(object sender, EventArgs e)
    {
        Globals.MapGrid?.ScreenshotWorld();
    }

    private void PnlMapGrid_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Keys.Add || (e.Modifiers == Keys.Shift && e.Key == Keys.Equal))
        {
            Globals.MapGrid?.ZoomIn(120, _posX, _posY);
        }
        else if (e.Key == Keys.Subtract || e.Key == Keys.Minus)
        {
            Globals.MapGrid?.ZoomIn(-120, _posX, _posY);
        }

        var xDiff = 0;
        var yDiff = 0;
        if (e.Key == Keys.W || e.Key == Keys.Up)
        {
            yDiff -= 20;
        }
        if (e.Key == Keys.S || e.Key == Keys.Down)
        {
            yDiff += 20;
        }
        if (e.Key == Keys.A || e.Key == Keys.Left)
        {
            xDiff -= 20;
        }
        if (e.Key == Keys.D || e.Key == Keys.Right)
        {
            xDiff += 20;
        }

        if (xDiff != 0 || yDiff != 0)
        {
            Globals.MapGrid?.Move(xDiff, yDiff);
            _pnlMapGrid.Invalidate();
        }
    }
}
