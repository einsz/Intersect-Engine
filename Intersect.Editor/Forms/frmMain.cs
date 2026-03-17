using System.ComponentModel;
using System.Diagnostics;
using Eto.Forms;
using Eto.Drawing;
using Intersect.Editor.Content;
using Intersect.Editor.Core;
using Intersect.Editor.Forms.DockingElements;
using Intersect.Editor.Forms.Editors;
using Intersect.Editor.Forms.Editors.Quest;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Maps;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Utilities;

namespace Intersect.Editor.Forms;

public class FrmMain : Form
{
    // Delegate types
    public delegate void HandleDisconnect();
    public delegate void TryOpenEditor(GameObjectType type);
    public delegate void UpdateTimeList();

    public HandleDisconnect DisconnectDelegate;
    public TryOpenEditor EditorDelegate;
    public UpdateTimeList TimeDelegate;

    // Editor references
    private dynamic mAnimationEditor;
    private dynamic mClassEditor;
    private dynamic mCommonEventEditor;
    private dynamic mCraftingTablesEditor;
    private dynamic mCraftsEditor;
    private dynamic mItemEditor;
    private dynamic mNpcEditor;
    private dynamic mProjectileEditor;
    private dynamic mQuestEditor;
    private dynamic mResourceEditor;
    private dynamic mShopEditor;
    private dynamic mSpellEditor;
    private dynamic mSwitchVariableEditor;
    private dynamic mTimeEditor;

    // Menu items that need to be toggled
    private CheckMenuItem hideDarknessMenuItem;
    private CheckMenuItem hideFogMenuItem;
    private CheckMenuItem hideOverlayMenuItem;
    private CheckMenuItem hideTilePreviewMenuItem;
    private CheckMenuItem hideResourcesMenuItem;
    private CheckMenuItem hideEventsMenuItem;
    private CheckMenuItem mapGridMenuItem;
    private ButtonMenuItem undoMenuItem;
    private ButtonMenuItem redoMenuItem;
    private ButtonMenuItem cutMenuItem;
    private ButtonMenuItem copyMenuItem;
    private ButtonMenuItem pasteMenuItem;
    private ButtonMenuItem fillMenuItem;
    private ButtonMenuItem eraseLayerMenuItem;
    private CheckMenuItem allLayersMenuItem;
    private CheckMenuItem currentLayerOnlyMenuItem;

    // Toolbar buttons (as regular Buttons in a toolbar layout)
    private Button btnNewMap;
    private Button btnSaveMap;
    private Button btnCut;
    private Button btnCopy;
    private Button btnPaste;
    private Button btnUndo;
    private Button btnRedo;
    private Button btnBrush;
    private Button btnSelect;
    private Button btnRect;
    private Button btnFlipVertical;
    private Button btnFlipHorizontal;
    private Button btnFill;
    private Button btnErase;
    private Button btnDropper;
    private Button btnScreenshot;
    private Button btnRun;
    private Button btnTime;
    private Button btnBug;
    private Button btnQuestion;

    // Status bar labels
    private Label lblCoords;
    private Label lblRevision;
    private Label lblFPS;
    private Label lblDebug;

    // Content area
    private TabControl contentTabs;
    private Drawable mapEditorDrawable;
    private Drawable mapGridDrawable;
    private Scrollable mapEditorScrollable;

    // Side panels
    private TabControl sideTabControl;

    // Layers tab controls
    private TabPage layersTab;
    private TabPage mapListTab;
    private TabPage mapPropertiesTab;

    public FrmMain()
    {
        Console.WriteLine("FrmMain constructor starting");
        Title = "Intersect Editor";
        MinimumSize = new Size(1024, 768);
        Size = new Size(1280, 800);
        WindowState = WindowState.Maximized;

        InitializeComponents();
        SetupMenus();
        SetupToolbar();
        SetupStatusBar();
        SetupLayout();
        Console.WriteLine("FrmMain constructor completed");

        // Initialize delegates
        DisconnectDelegate = HandleServerDisconnect;
        EditorDelegate = TryOpenEditorMethod;
        TimeDelegate = UpdateTimeSimulationList;

        // Set up global delegate for editor opening
        Globals.OpenEditorDelegate = TryOpenEditorMethod;

        // Save login preferences
        Globals.LoginForm?.TryRemembering();
    }

    private void InitializeComponents()
    {
        Console.WriteLine("FrmMain.InitializeComponents() starting");
        // Initialize docking windows
        Globals.MapListWindow = new FrmMapList();
        Globals.MapLayersWindow = new FrmMapLayers();
        Globals.MapGridWindowNew = new FrmMapGrid();
        Globals.MapEditorWindow = new FrmMapEditor();
        Console.WriteLine("FrmMain.InitializeComponents() completed");
    }

    private void SetupMenus()
    {
        var menuBar = new MenuBar();

        // File menu
        var fileMenu = new SubMenuItem { Text = Strings.MainForm.file };
        var saveMapItem = new ButtonMenuItem { Text = Strings.MainForm.SaveMap };
        saveMapItem.Click += (s, e) => SaveMapMenuItem_Click();
        var newMapItem = new ButtonMenuItem { Text = Strings.MainForm.newmap };
        newMapItem.Click += (s, e) => NewMapMenuItem_Click();
        var optionsItem = new ButtonMenuItem { Text = Strings.MainForm.options };
        optionsItem.Click += (s, e) => OptionsMenuItem_Click();
        var exitItem = new ButtonMenuItem { Text = Strings.MainForm.exit };
        exitItem.Click += (s, e) => Application.Instance.Quit();
        fileMenu.Items.Add(saveMapItem);
        fileMenu.Items.Add(newMapItem);
        fileMenu.Items.Add(new SeparatorMenuItem());
        fileMenu.Items.Add(optionsItem);
        fileMenu.Items.Add(new SeparatorMenuItem());
        fileMenu.Items.Add(exitItem);

        // Edit menu
        var editMenu = new SubMenuItem { Text = Strings.MainForm.edit };
        undoMenuItem = new ButtonMenuItem { Text = Strings.MainForm.Undo, Enabled = false };
        undoMenuItem.Click += (s, e) => Undo();
        redoMenuItem = new ButtonMenuItem { Text = Strings.MainForm.Redo, Enabled = false };
        redoMenuItem.Click += (s, e) => Redo();
        cutMenuItem = new ButtonMenuItem { Text = Strings.MainForm.Cut, Enabled = false };
        cutMenuItem.Click += (s, e) => Cut();
        copyMenuItem = new ButtonMenuItem { Text = Strings.MainForm.Copy, Enabled = false };
        copyMenuItem.Click += (s, e) => Copy();
        pasteMenuItem = new ButtonMenuItem { Text = Strings.MainForm.Paste, Enabled = false };
        pasteMenuItem.Click += (s, e) => Paste();
        fillMenuItem = new ButtonMenuItem { Text = Strings.MainForm.Fill };
        fillMenuItem.Click += (s, e) => Fill();
        eraseLayerMenuItem = new ButtonMenuItem { Text = Strings.MainForm.Erase };
        eraseLayerMenuItem.Click += (s, e) => EraseLayer();

        var selectMenu = new SubMenuItem { Text = Strings.MainForm.selectlayers };
        allLayersMenuItem = new CheckMenuItem { Text = Strings.MainForm.alllayers, Checked = true };
        allLayersMenuItem.Click += (s, e) => { Globals.SelectionType = (int)SelectionTypes.AllLayers; allLayersMenuItem.Checked = true; currentLayerOnlyMenuItem.Checked = false; };
        currentLayerOnlyMenuItem = new CheckMenuItem { Text = Strings.MainForm.currentonly };
        currentLayerOnlyMenuItem.Click += (s, e) => { Globals.SelectionType = (int)SelectionTypes.CurrentLayer; allLayersMenuItem.Checked = false; currentLayerOnlyMenuItem.Checked = true; };
        selectMenu.Items.Add(allLayersMenuItem);
        selectMenu.Items.Add(currentLayerOnlyMenuItem);

        editMenu.Items.Add(undoMenuItem);
        editMenu.Items.Add(redoMenuItem);
        editMenu.Items.Add(new SeparatorMenuItem());
        editMenu.Items.Add(cutMenuItem);
        editMenu.Items.Add(copyMenuItem);
        editMenu.Items.Add(pasteMenuItem);
        editMenu.Items.Add(new SeparatorMenuItem());
        editMenu.Items.Add(fillMenuItem);
        editMenu.Items.Add(eraseLayerMenuItem);
        editMenu.Items.Add(selectMenu);

        // View menu
        var viewMenu = new SubMenuItem { Text = Strings.MainForm.view };
        hideDarknessMenuItem = new CheckMenuItem { Text = Strings.MainForm.darkness, Checked = true };
        hideDarknessMenuItem.Click += (s, e) => { Core.Graphics.HideDarkness = !Core.Graphics.HideDarkness; hideDarknessMenuItem.Checked = !Core.Graphics.HideDarkness; };
        hideFogMenuItem = new CheckMenuItem { Text = Strings.MainForm.fog, Checked = true };
        hideFogMenuItem.Click += (s, e) => { Core.Graphics.HideFog = !Core.Graphics.HideFog; hideFogMenuItem.Checked = !Core.Graphics.HideFog; };
        hideOverlayMenuItem = new CheckMenuItem { Text = Strings.MainForm.overlay, Checked = true };
        hideOverlayMenuItem.Click += (s, e) => { Core.Graphics.HideOverlay = !Core.Graphics.HideOverlay; hideOverlayMenuItem.Checked = !Core.Graphics.HideOverlay; };
        hideTilePreviewMenuItem = new CheckMenuItem { Text = Strings.MainForm.tilepreview, Checked = true };
        hideTilePreviewMenuItem.Click += (s, e) => { Core.Graphics.HideTilePreview = !Core.Graphics.HideTilePreview; hideTilePreviewMenuItem.Checked = !Core.Graphics.HideTilePreview; };
        hideResourcesMenuItem = new CheckMenuItem { Text = Strings.MainForm.resources, Checked = true };
        hideResourcesMenuItem.Click += (s, e) => { Core.Graphics.HideResources = !Core.Graphics.HideResources; hideResourcesMenuItem.Checked = !Core.Graphics.HideResources; };
        hideEventsMenuItem = new CheckMenuItem { Text = Strings.MainForm.Events, Checked = true };
        hideEventsMenuItem.Click += (s, e) => { Core.Graphics.HideEvents = !Core.Graphics.HideEvents; hideEventsMenuItem.Checked = !Core.Graphics.HideEvents; };
        mapGridMenuItem = new CheckMenuItem { Text = Strings.MainForm.grid };
        mapGridMenuItem.Click += (s, e) => { Core.Graphics.HideGrid = !Core.Graphics.HideGrid; mapGridMenuItem.Checked = !Core.Graphics.HideGrid; };

        viewMenu.Items.Add(hideDarknessMenuItem);
        viewMenu.Items.Add(hideFogMenuItem);
        viewMenu.Items.Add(hideOverlayMenuItem);
        viewMenu.Items.Add(hideTilePreviewMenuItem);
        viewMenu.Items.Add(hideResourcesMenuItem);
        viewMenu.Items.Add(hideEventsMenuItem);
        viewMenu.Items.Add(mapGridMenuItem);

        // Content Editors menu
        var editorsMenu = new SubMenuItem { Text = Strings.MainForm.editors };
        var animItem = new ButtonMenuItem { Text = Strings.MainForm.animationeditor };
        animItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Animation);
        var classItem = new ButtonMenuItem { Text = Strings.MainForm.classeditor };
        classItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Class);
        var commonEventItem = new ButtonMenuItem { Text = Strings.MainForm.commoneventeditor };
        commonEventItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Event);
        var craftTableItem = new ButtonMenuItem { Text = Strings.MainForm.craftingtableeditor };
        craftTableItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.CraftTables);
        var craftItem = new ButtonMenuItem { Text = Strings.MainForm.craftingeditor };
        craftItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Crafts);
        var itemItem = new ButtonMenuItem { Text = Strings.MainForm.itemeditor };
        itemItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Item);
        var npcItem = new ButtonMenuItem { Text = Strings.MainForm.npceditor };
        npcItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Npc);
        var projItem = new ButtonMenuItem { Text = Strings.MainForm.projectileeditor };
        projItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Projectile);
        var questItem = new ButtonMenuItem { Text = Strings.MainForm.questeditor };
        questItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Quest);
        var resourceItem = new ButtonMenuItem { Text = Strings.MainForm.resourceeditor };
        resourceItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Resource);
        var shopItem = new ButtonMenuItem { Text = Strings.MainForm.shopeditor };
        shopItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Shop);
        var spellItem = new ButtonMenuItem { Text = Strings.MainForm.spelleditor };
        spellItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Spell);
        var varItem = new ButtonMenuItem { Text = Strings.MainForm.variableeditor };
        varItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.PlayerVariable);
        var timeItem = new ButtonMenuItem { Text = Strings.MainForm.timeeditor };
        timeItem.Click += (s, e) => PacketSender.SendOpenEditor(GameObjectType.Time);

        editorsMenu.Items.Add(animItem);
        editorsMenu.Items.Add(classItem);
        editorsMenu.Items.Add(commonEventItem);
        editorsMenu.Items.Add(craftTableItem);
        editorsMenu.Items.Add(craftItem);
        editorsMenu.Items.Add(itemItem);
        editorsMenu.Items.Add(npcItem);
        editorsMenu.Items.Add(projItem);
        editorsMenu.Items.Add(questItem);
        editorsMenu.Items.Add(resourceItem);
        editorsMenu.Items.Add(shopItem);
        editorsMenu.Items.Add(spellItem);
        editorsMenu.Items.Add(varItem);
        editorsMenu.Items.Add(timeItem);

        // Help menu
        var helpMenu = new SubMenuItem { Text = Strings.MainForm.help };
        var questionItem = new ButtonMenuItem { Text = Strings.MainForm.postquestion };
        questionItem.Click += (s, e) => BrowserUtils.Open("https://www.ascensiongamedev.com/community/forum/53-questions-and-answers/");
        var bugItem = new ButtonMenuItem { Text = Strings.MainForm.reportbug };
        bugItem.Click += (s, e) => BrowserUtils.Open("https://github.com/AscensionGameDev/Intersect-Engine/issues/new/choose");
        var aboutItem = new ButtonMenuItem { Text = Strings.MainForm.about };
        aboutItem.Click += (s, e) => { var about = new FrmAbout(); about.Show(); };
        helpMenu.Items.Add(questionItem);
        helpMenu.Items.Add(bugItem);
        helpMenu.Items.Add(new SeparatorMenuItem());
        helpMenu.Items.Add(aboutItem);

        menuBar.Items.Add(fileMenu);
        menuBar.Items.Add(editMenu);
        menuBar.Items.Add(viewMenu);
        menuBar.Items.Add(editorsMenu);
        menuBar.Items.Add(helpMenu);

        Menu = menuBar;
    }

    private void SetupToolbar()
    {
        // Toolbar will be created as buttons in the layout
        btnNewMap = new Button { Text = "New" };
        btnNewMap.Click += (s, e) => NewMapMenuItem_Click();
        btnSaveMap = new Button { Text = "Save" };
        btnSaveMap.Click += (s, e) => SaveMapMenuItem_Click();
        btnCut = new Button { Text = "Cut", Enabled = false };
        btnCut.Click += (s, e) => Cut();
        btnCopy = new Button { Text = "Copy", Enabled = false };
        btnCopy.Click += (s, e) => Copy();
        btnPaste = new Button { Text = "Paste", Enabled = false };
        btnPaste.Click += (s, e) => Paste();
        btnUndo = new Button { Text = "Undo", Enabled = false };
        btnUndo.Click += (s, e) => Undo();
        btnRedo = new Button { Text = "Redo", Enabled = false };
        btnRedo.Click += (s, e) => Redo();
        btnBrush = new Button { Text = "Brush" };
        btnBrush.Click += (s, e) => Globals.CurrentTool = EditingTool.Brush;
        btnSelect = new Button { Text = "Select" };
        btnSelect.Click += (s, e) => { Globals.CurrentTool = EditingTool.Selection; Globals.CurMapSelX = 0; Globals.CurMapSelY = 0; Globals.CurMapSelW = 0; Globals.CurMapSelH = 0; };
        btnRect = new Button { Text = "Rect" };
        btnRect.Click += (s, e) => { Globals.CurrentTool = EditingTool.Rectangle; Globals.CurMapSelX = 0; Globals.CurMapSelY = 0; Globals.CurMapSelW = 0; Globals.CurMapSelH = 0; };
        btnFlipVertical = new Button { Text = "Flip V" };
        btnFlipVertical.Click += (s, e) => Globals.MapEditorWindow?.FlipVertical();
        btnFlipHorizontal = new Button { Text = "Flip H" };
        btnFlipHorizontal.Click += (s, e) => Globals.MapEditorWindow?.FlipHorizontal();
        btnFill = new Button { Text = "Fill" };
        btnFill.Click += (s, e) => Globals.CurrentTool = EditingTool.Fill;
        btnErase = new Button { Text = "Erase" };
        btnErase.Click += (s, e) => Globals.CurrentTool = EditingTool.Erase;
        btnDropper = new Button { Text = "Pick", Enabled = false };
        btnDropper.Click += (s, e) => Globals.CurrentTool = EditingTool.Dropper;
        btnScreenshot = new Button { Text = "Screenshot" };
        btnScreenshot.Click += (s, e) => TakeScreenshot();
        btnRun = new Button { Text = "Run", Enabled = false };
        btnRun.Click += (s, e) => RunClient();
        btnBug = new Button { Text = "Bug" };
        btnBug.Click += (s, e) => BrowserUtils.Open("https://github.com/AscensionGameDev/Intersect-Engine/issues/new/choose");
        btnQuestion = new Button { Text = "Help" };
        btnQuestion.Click += (s, e) => BrowserUtils.Open("https://www.ascensiongamedev.com/community/forum/53-questions-and-answers/");
    }

    private void SetupStatusBar()
    {
        lblCoords = new Label { Text = "" };
        lblRevision = new Label { Text = "" };
        lblFPS = new Label { Text = "FPS: 0" };
        lblDebug = new Label { Text = "" };
    }

    private void SetupLayout()
    {
        // Map editor drawable
        mapEditorDrawable = new Drawable { CanFocus = true, Size = new Size(1600, 1200), BackgroundColor = Colors.Blue };
        mapEditorDrawable.Paint += MapEditorDrawable_Paint;
        mapEditorScrollable = new Scrollable { Content = mapEditorDrawable, Size = new Size(800, 600) };

        // Map grid drawable
        mapGridDrawable = new Drawable { CanFocus = true };
        mapGridDrawable.Paint += MapGridDrawable_Paint;

        // Content tabs
        contentTabs = new TabControl();
        contentTabs.Pages.Add(new TabPage { Text = "Map Editor", Content = mapEditorScrollable });
        contentTabs.Pages.Add(new TabPage { Text = "Map Grid", Content = mapGridDrawable });

        // Side panel tabs
        sideTabControl = new TabControl();
        layersTab = new TabPage { Text = "Layers" };
        mapListTab = new TabPage { Text = "Map List" };
        mapPropertiesTab = new TabPage { Text = "Properties" };

        // Create placeholder panels for side tabs
        var layersPanel = new Panel();
        var mapListPanel = new Panel();
        var propsPanel = new Panel();

        if (Globals.MapLayersWindow is Panel layersWin)
            layersPanel = layersWin;
        if (Globals.MapListWindow is Panel listWin)
            mapListPanel = listWin;
        if (Globals.MapPropertiesWindow is Panel propsWin)
            propsPanel = propsWin;

        layersTab.Content = layersPanel;
        mapListTab.Content = mapListPanel;
        mapPropertiesTab.Content = propsPanel;
        sideTabControl.Pages.Add(layersTab);
        sideTabControl.Pages.Add(mapListTab);
        sideTabControl.Pages.Add(mapPropertiesTab);

        // Toolbar layout
        var toolbarLayout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Items =
            {
                btnNewMap, btnSaveMap, new StackLayoutItem(null, true),
                btnCut, btnCopy, btnPaste, new StackLayoutItem(null, true),
                btnUndo, btnRedo, new StackLayoutItem(null, true),
                btnBrush, btnSelect, btnRect, new StackLayoutItem(null, true),
                btnFlipVertical, btnFlipHorizontal, new StackLayoutItem(null, true),
                btnFill, btnErase, btnDropper, new StackLayoutItem(null, true),
                btnScreenshot, btnRun, new StackLayoutItem(null, true),
                btnBug, btnQuestion
            }
        };

        // Status bar
        var statusLayout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Padding = new Padding(5),
            Items = { lblCoords, lblRevision, lblFPS, lblDebug }
        };

        // Main splitter: side panel | content
        var mainSplitter = new Splitter
        {
            Position = 250,
            Panel1 = sideTabControl,
            Panel2 = contentTabs
        };

        // Main layout
        var mainLayout = new DynamicLayout();
        mainLayout.BeginVertical();
        mainLayout.Add(toolbarLayout);
        mainLayout.Add(mainSplitter, yscale: true);
        mainLayout.Add(statusLayout);
        mainLayout.EndVertical();

        Content = mainLayout;

        // Timer to invalidate the drawable for repainting
        var redrawTimer = new UITimer { Interval = 1.0 / 10.0 };
        redrawTimer.Elapsed += (s, ev) =>
        {
            mapEditorDrawable.Invalidate(true);
        };
        redrawTimer.Start();
    }

    private static int sPaintCount = 0;
    private void MapEditorDrawable_Paint(object sender, PaintEventArgs e)
    {
        sPaintCount++;
        Console.WriteLine($"MapEditorDrawable_Paint called #{sPaintCount}, clip={e.ClipRectangle}");

        // Use Eto.Forms bitmap rendering (works on Linux without MonoGame headless device)
        var mapBitmap = Core.Graphics.RenderMapToBitmap();
        if (mapBitmap != null)
        {
            e.Graphics.DrawImage(mapBitmap, 0, 0);
        }
        else
        {
            // Draw debug indicator
            var rect = e.ClipRectangle;
            e.Graphics.DrawLine(Colors.Red, 0, 0, rect.Width, rect.Height);
            e.Graphics.DrawLine(Colors.Red, rect.Width, 0, 0, rect.Height);

            var status = Globals.CurrentMap != null ? "Map loaded, rendering..." : "No map loaded";
            e.Graphics.DrawText(new Font(SystemFont.Default), Colors.White, 10, 10, status);
        }
    }

    private void MapGridDrawable_Paint(object sender, PaintEventArgs e)
    {
        // Map grid rendering placeholder
    }

    // Public methods for external access
    public void UpdateFpsLabel(int fps)
    {
        Application.Instance.Invoke(() =>
        {
            lblFPS.Text = $"FPS: {fps}";
        });
    }

    public void Update()
    {
        if (Globals.CurrentMap != null)
        {
            lblCoords.Text = Strings.MainForm.loc.ToString(Globals.CurTileX, Globals.CurTileY);
            lblRevision.Text = Strings.MainForm.revision.ToString(Globals.CurrentMap.Revision);
            Title = Strings.MainForm.title.ToString(Globals.CurrentMap.Name);
        }

        // Update button states
        bool hasUndo = Globals.MapEditorWindow != null && Globals.MapEditorWindow.MapUndoStates?.Count > 0;
        bool hasRedo = Globals.MapEditorWindow != null && Globals.MapEditorWindow.MapRedoStates?.Count > 0;
        btnUndo.Enabled = hasUndo;
        btnRedo.Enabled = hasRedo;
        undoMenuItem.Enabled = hasUndo;
        redoMenuItem.Enabled = hasRedo;

        bool canFillErase = Options.Instance.Map.Layers.All.Contains(Globals.CurrentLayer) ||
                            Globals.CurrentLayer == "Attributes";
        btnFill.Enabled = canFillErase;
        btnErase.Enabled = canFillErase;
        fillMenuItem.Enabled = canFillErase;
        eraseLayerMenuItem.Enabled = canFillErase;

        // Tool button states
        btnBrush.Enabled = false;
        btnSelect.Enabled = true;
        btnRect.Enabled = false;
        btnDropper.Enabled = false;

        if (Globals.CurrentLayer == "Attributes")
        {
            btnBrush.Enabled = true;
            btnRect.Enabled = true;
        }
        else if (Globals.CurrentLayer == "Lights" ||
                 Globals.CurrentLayer == "Events" ||
                 Globals.CurrentLayer == "NPCs")
        {
            Globals.CurrentTool = EditingTool.Selection;
        }
        else
        {
            btnBrush.Enabled = true;
            btnRect.Enabled = true;
            btnDropper.Enabled = true;
        }

        // Paste state
        btnPaste.Enabled = Globals.HasCopy;
        pasteMenuItem.Enabled = Globals.HasCopy;
    }

    public void EnterMap(Guid mapId, bool userEntered = false)
    {
        Globals.CurrentMap = MapInstance.Get(mapId);
        Globals.LoadingMap = mapId;

        if (Globals.CurrentMap != null && Globals.MapPropertiesWindow != null)
        {
            Globals.MapPropertiesWindow.Init(Globals.CurrentMap);
        }

        Globals.MapEditorWindow?.UnloadMap();
        PacketSender.SendEnterMap(mapId);
        PacketSender.SendNeedMap(mapId);
        PacketSender.SendNeedGrid(mapId);
        Core.Graphics.TilePreviewUpdated = true;

        // Resize the map drawable to fit the map
        var mapWidth = (Options.Instance.Map.MapWidth + 2) * Options.Instance.Map.TileWidth;
        var mapHeight = (Options.Instance.Map.MapHeight + 2) * Options.Instance.Map.TileHeight;
        mapEditorDrawable.Size = new Eto.Drawing.Size(mapWidth, mapHeight);
        mapEditorScrollable.ScrollSize = new Eto.Drawing.Size(mapWidth, mapHeight);
        Core.Graphics.InvalidateMap();

        if (userEntered)
        {
            Preferences.SavePreference("LastMapOpened", mapId.ToString());
        }
    }

    public void ShowDialogForm(Form form)
    {
        form.Show();
    }

    // Menu event handlers
    private void SaveMapMenuItem_Click()
    {
        if (Globals.CurrentMap?.Changed() == true)
        {
            var result = MessageBox.Show(
                Strings.Mapping.savemapdialoguesure,
                Strings.Mapping.savemap,
                MessageBoxButtons.YesNo,
                MessageBoxType.Question
            );
            if (result == DialogResult.Yes)
            {
                SaveMap();
            }
        }
    }

    private static void SaveMap()
    {
        if (Globals.CurrentTool == EditingTool.Selection && Globals.Dragging)
        {
            Globals.MapEditorWindow?.ProcessSelectionMovement(Globals.CurrentMap, true);
            Globals.MapEditorWindow?.PlaceSelection();
        }
        PacketSender.SendMap(Globals.CurrentMap);
    }

    private void NewMapMenuItem_Click()
    {
        var result = MessageBox.Show(
            Strings.Mapping.newmap,
            Strings.Mapping.newmapcaption,
            MessageBoxButtons.YesNo,
            MessageBoxType.Warning
        );
        if (result != DialogResult.Yes) return;

        if (Globals.CurrentMap?.Changed() == true)
        {
            var saveResult = MessageBox.Show(
                Strings.Mapping.savemapdialogue,
                Strings.Mapping.savemap,
                MessageBoxButtons.YesNo,
                MessageBoxType.Question
            );
            if (saveResult == DialogResult.Yes)
            {
                SaveMap();
            }
        }
        PacketSender.SendCreateMap(-1, Globals.CurrentMap.Id, null);
    }

    private void OptionsMenuItem_Click()
    {
        var optionsForm = new FrmOptions();
        optionsForm.Show();
        UpdateRunState();
    }

    private void Fill()
    {
        if (Options.Instance.Map.Layers.All.Contains(Globals.CurrentLayer))
        {
            Globals.MapEditorWindow?.FillLayer();
        }
    }

    private void EraseLayer()
    {
        if (Options.Instance.Map.Layers.All.Contains(Globals.CurrentLayer))
        {
            Globals.MapEditorWindow?.EraseLayer();
        }
    }

    private void Undo()
    {
        if (Globals.MapEditorWindow?.MapUndoStates?.Count > 0)
        {
            var states = Globals.MapEditorWindow.MapUndoStates;
            Globals.CurrentMap.LoadInternal(states[states.Count - 1]);
            Globals.MapEditorWindow.MapRedoStates.Add(Globals.MapEditorWindow.CurrentMapState);
            Globals.MapEditorWindow.CurrentMapState = states[states.Count - 1];
            states.RemoveAt(states.Count - 1);
            Globals.MapPropertiesWindow?.Update();
            Core.Graphics.TilePreviewUpdated = true;
        }
    }

    private void Redo()
    {
        if (Globals.MapEditorWindow?.MapRedoStates?.Count > 0)
        {
            var states = Globals.MapEditorWindow.MapRedoStates;
            Globals.CurrentMap.LoadInternal(states[states.Count - 1]);
            Globals.MapEditorWindow.MapUndoStates.Add(Globals.MapEditorWindow.CurrentMapState);
            Globals.MapEditorWindow.CurrentMapState = states[states.Count - 1];
            states.RemoveAt(states.Count - 1);
            Globals.MapPropertiesWindow?.Update();
            Core.Graphics.TilePreviewUpdated = true;
        }
    }

    private void Cut()
    {
        if (Globals.CurrentTool == EditingTool.Selection)
        {
            Globals.MapEditorWindow?.Cut();
        }
    }

    private void Copy()
    {
        if (Globals.CurrentTool == EditingTool.Selection)
        {
            Globals.MapEditorWindow?.Copy();
        }
    }

    private void Paste()
    {
        if (Globals.HasCopy)
        {
            Globals.MapEditorWindow?.Paste();
        }
    }

    private void TakeScreenshot()
    {
        var fileDialog = new SaveFileDialog
        {
            Title = Strings.MainForm.screenshot,
            Filters = { new FileFilter("PNG Image", ".png"), new FileFilter("JPEG Image", ".jpg"), new FileFilter("Bitmap Image", ".bmp") }
        };
        if (fileDialog.ShowDialog(this) == DialogResult.Ok)
        {
            using var fs = new FileStream(fileDialog.FileName, FileMode.OpenOrCreate);
            var screenshotTexture = Core.Graphics.ScreenShotMap();
            if (screenshotTexture != null)
            {
                screenshotTexture.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }

    private void RunClient()
    {
        var path = Preferences.LoadPreference("ClientPath");
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            var psi = new ProcessStartInfo(path)
            {
                WorkingDirectory = Directory.GetParent(path)?.FullName ?? ""
            };
            Process.Start(psi);
        }
    }

    private void UpdateRunState()
    {
        btnRun.Enabled = false;
        var path = Preferences.LoadPreference("ClientPath");
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            btnRun.Enabled = true;
        }
    }

    private void HandleServerDisconnect()
    {
        if (!Globals.ClosingEditor)
        {
            Globals.ClosingEditor = true;
            if (Globals.CurrentMap != null)
            {
                var result = MessageBox.Show(
                    Strings.Errors.disconnectedsave,
                    Strings.Errors.disconnectedsavecaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxType.Error
                );
                if (result == DialogResult.Yes)
                {
                    TakeScreenshot();
                }
            }
            else
            {
                MessageBox.Show(
                    Strings.Errors.disconnectedclosing,
                    Strings.Errors.disconnected,
                    MessageBoxButtons.OK,
                    MessageBoxType.Error
                );
            }
            Application.Instance.Quit();
        }
    }

    private void TryOpenEditorMethod(GameObjectType type)
    {
        if (Globals.CurrentEditor != -1) return;

        Console.WriteLine($"Opening editor for: {type}");
        Globals.CurrentEditor = (int)type;

        try
        {
            switch (type)
            {
                case GameObjectType.Animation:
                    Console.WriteLine("Creating FrmAnimation...");
                    var animEditor = new FrmAnimation();
                    Console.WriteLine("FrmAnimation created, calling InitEditor...");
                    try { animEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex.Message}\n{ex.StackTrace}"); }
                    Console.WriteLine("Showing FrmAnimation...");
                    animEditor.Show();
                    break;
                case GameObjectType.Item:
                    var itemEditor = new FrmItem();
                    try { itemEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    itemEditor.Show();
                    break;
                case GameObjectType.Npc:
                    var npcEditor = new FrmNpc();
                    try { npcEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    npcEditor.Show();
                    break;
                case GameObjectType.Resource:
                    var resEditor = new FrmResource();
                    try { resEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    resEditor.Show();
                    break;
                case GameObjectType.Spell:
                    var spellEditor = new FrmSpell();
                    try { spellEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    spellEditor.Show();
                    break;
                case GameObjectType.CraftTables:
                    var craftTableEditor = new FrmCraftingTables();
                    try { craftTableEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    craftTableEditor.Show();
                    break;
                case GameObjectType.Crafts:
                    var craftsEditor = new FrmCrafts();
                    try { craftsEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    craftsEditor.Show();
                    break;
                case GameObjectType.Class:
                    var classEditor = new FrmClass();
                    try { classEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    classEditor.Show();
                    break;
                case GameObjectType.Quest:
                    var questEditor = new FrmQuest();
                    try { questEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    questEditor.Show();
                    break;
                case GameObjectType.Projectile:
                    var projEditor = new FrmProjectile();
                    try { projEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    projEditor.Show();
                    break;
                case GameObjectType.Event:
                    Console.WriteLine("Common event editor not yet available");
                    Globals.CurrentEditor = -1;
                    break;
                case GameObjectType.PlayerVariable:
                    var varEditor = new FrmSwitchVariable();
                    try { varEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    varEditor.Show();
                    break;
                case GameObjectType.Shop:
                    var shopEditor = new FrmShop();
                    try { shopEditor.InitEditor(); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    shopEditor.Show();
                    break;
                case GameObjectType.Time:
                    var timeEditor = new FrmTime();
                    try { timeEditor.InitEditor(DaylightCycleDescriptor.Instance); } catch (Exception ex) { Console.WriteLine($"InitEditor error: {ex}"); }
                    timeEditor.Show();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening editor: {ex.Message}");
            Globals.CurrentEditor = -1;
        }
    }

    private void UpdateTimeSimulationList()
    {
        // Update time simulation dropdown
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!Globals.ClosingEditor && Globals.CurrentMap?.Changed() == true)
        {
            var result = MessageBox.Show(
                Strings.Mapping.maphaschangesdialog,
                Strings.Mapping.mapnotsaved,
                MessageBoxButtons.YesNo,
                MessageBoxType.Warning
            );
            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }
        Globals.ClosingEditor = true;
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        Networking.Network.EditorLidgrenNetwork?.Disconnect("Quitting");
        base.OnClosed(e);
        Application.Instance.Quit();
    }
}

// Stub for FrmSwitchVariable if not defined elsewhere
public class FrmSwitchVariable : Form
{
    public void InitEditor() { }
}
