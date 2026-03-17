using Intersect.Config;
using Intersect.Editor.Content;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Animations;
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.Framework.Core.GameObjects.Mapping.Tilesets;
using Intersect.Framework.Core.GameObjects.Maps;
using Intersect.Framework.Core.GameObjects.Maps.Attributes;
using Intersect.Framework.Core.GameObjects.Maps.MapList;
using Intersect.Framework.Core.GameObjects.NPCs;
using Intersect.Framework.Core.GameObjects.Resources;
using Intersect.GameObjects;
using Intersect.Utilities;
using Eto.Forms;
using Eto.Drawing;

namespace Intersect.Editor.Forms.DockingElements;

public enum LayerTabs
{
    Tiles = 0,
    Attributes,
    Lights,
    Events,
    Npcs
}

public partial class FrmMapLayers : Panel
{
    public LayerTabs CurrentTab = LayerTabs.Tiles;
    public Dictionary<string, bool> LayerVisibility = new Dictionary<string, bool>();

    private bool _tilesetMouseDown;
    private string _lastTileLayer;

    private TabControl _tabControl;

    // Tiles tab
    private TabPage _tabTiles;
    private DropDown _cmbTilesets;
    private DropDown _cmbAutotile;
    private DropDown _cmbMapLayer;
    private Drawable _picTileset;
    private Scrollable _pnlTilesetContainer;
    private List<Button> _mapLayerButtons;

    // Attributes tab
    private TabPage _tabAttributes;
    private DropDown _cmbAttributeType;

    // Lights tab
    private TabPage _tabLights;

    // Events tab
    private TabPage _tabEvents;

    // NPCs tab
    private TabPage _tabNpcs;
    private DropDown _cmbNpc;
    private DropDown _cmbDir;
    private ListBox _lstMapNpcs;
    private Button _btnAddMapNpc;
    private Button _btnRemoveMapNpc;
    private RadioButton _rbRandom;
    private RadioButton _rbDeclared;
    private Label _lblNpcCount;

    public Drawable PicTileset => _picTileset;
    public ListBox LstMapNpcs => _lstMapNpcs;
    public RadioButton RbDeclared => _rbDeclared;
    public RadioButton RbRandom => _rbRandom;

    public FrmMapLayers()
    {
        InitializeComponents();
        _mapLayerButtons = new List<Button>();
    }

    private void InitializeComponents()
    {
        _tabControl = new TabControl();

        // Tiles Tab
        _tabTiles = new TabPage { Text = Strings.MapLayers.tiles };
        InitializeTilesTab();
        _tabControl.Pages.Add(_tabTiles);

        // Attributes Tab
        _tabAttributes = new TabPage { Text = Strings.MapLayers.attributes };
        InitializeAttributesTab();
        _tabControl.Pages.Add(_tabAttributes);

        // Lights Tab
        _tabLights = new TabPage { Text = Strings.MapLayers.lights };
        InitializeLightsTab();
        _tabControl.Pages.Add(_tabLights);

        // Events Tab
        _tabEvents = new TabPage { Text = Strings.MapLayers.events };
        InitializeEventsTab();
        _tabControl.Pages.Add(_tabEvents);

        // NPCs Tab
        _tabNpcs = new TabPage { Text = Strings.MapLayers.npcs };
        InitializeNpcsTab();
        _tabControl.Pages.Add(_tabNpcs);

        _tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

        Content = _tabControl;
    }

    private void InitializeTilesTab()
    {
        var lblLayer = new Label { Text = Strings.Tiles.layer };
        _cmbMapLayer = new DropDown();

        var lblTileset = new Label { Text = Strings.Tiles.tileset };
        _cmbTilesets = new DropDown();
        _cmbTilesets.SelectedIndexChanged += CmbTilesets_SelectedIndexChanged;

        var lblTileType = new Label { Text = Strings.Tiles.tiletype };
        _cmbAutotile = new DropDown();
        _cmbAutotile.Items.Add(Strings.Tiles.normal);
        _cmbAutotile.Items.Add(Strings.Tiles.autotile);
        _cmbAutotile.Items.Add(Strings.Tiles.fake);
        _cmbAutotile.Items.Add(Strings.Tiles.animated);
        _cmbAutotile.Items.Add(Strings.Tiles.cliff);
        _cmbAutotile.Items.Add(Strings.Tiles.waterfall);
        _cmbAutotile.Items.Add(Strings.Tiles.autotilexp);
        _cmbAutotile.Items.Add(Strings.Tiles.animatedxp);
        _cmbAutotile.SelectedIndex = 0;
        _cmbAutotile.SelectedIndexChanged += CmbAutotile_SelectedIndexChanged;

        _picTileset = new Drawable { Size = new Size(300, 400) };
        _picTileset.CanFocus = true;
        _picTileset.MouseDown += PicTileset_MouseDown;
        _picTileset.MouseMove += PicTileset_MouseMove;
        _picTileset.MouseUp += PicTileset_MouseUp;
        _picTileset.Paint += PicTileset_Paint;

        _pnlTilesetContainer = new Scrollable { Content = _picTileset };

        var layerButtonsPanel = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4
        };

        var layout = new DynamicLayout { Padding = new Padding(4), Spacing = new Size(4, 4) };
        layout.AddRow(lblLayer, _cmbMapLayer);
        layout.AddRow(lblTileset, _cmbTilesets);
        layout.AddRow(lblTileType, _cmbAutotile);
        layout.AddRow(layerButtonsPanel);
        layout.Add(_pnlTilesetContainer);

        _tabTiles.Content = layout;
    }

    private void InitializeAttributesTab()
    {
        var lblAttributeType = new Label { Text = "Attribute Type" };
        _cmbAttributeType = new DropDown();
        _cmbAttributeType.SelectedIndexChanged += CmbAttributeType_SelectedIndexChanged;

        var layout = new DynamicLayout { Padding = new Padding(4), Spacing = new Size(4, 4) };
        layout.AddRow(lblAttributeType, _cmbAttributeType);
        layout.Add(new Panel());

        _tabAttributes.Content = layout;
    }

    private void InitializeLightsTab()
    {
        var lblInstructions = new Label { Text = Strings.MapLayers.lightinstructions };
        var layout = new DynamicLayout { Padding = new Padding(4) };
        layout.AddRow(lblInstructions);
        layout.Add(new Panel());
        _tabLights.Content = layout;
    }

    private void InitializeEventsTab()
    {
        var lblInstructions = new Label { Text = Strings.MapLayers.eventinstructions };
        var layout = new DynamicLayout { Padding = new Padding(4) };
        layout.AddRow(lblInstructions);
        layout.Add(new Panel());
        _tabEvents.Content = layout;
    }

    private void InitializeNpcsTab()
    {
        _cmbNpc = new DropDown();
        _cmbNpc.SelectedIndexChanged += CmbNpc_SelectedIndexChanged;

        _cmbDir = new DropDown();
        _cmbDir.Items.Add(Strings.NpcSpawns.randomdirection);
        for (var i = 0; i < 4; i++)
        {
            _cmbDir.Items.Add(Strings.Direction.dir[(Direction)i]);
        }
        _cmbDir.SelectedIndex = 0;
        _cmbDir.SelectedIndexChanged += CmbDir_SelectedIndexChanged;

        _lstMapNpcs = new ListBox();
        _lstMapNpcs.SelectedIndexChanged += LstMapNpcs_SelectedIndexChanged;

        _btnAddMapNpc = new Button { Text = Strings.NpcSpawns.add };
        _btnAddMapNpc.Click += BtnAddMapNpc_Click;

        _btnRemoveMapNpc = new Button { Text = Strings.NpcSpawns.remove };
        _btnRemoveMapNpc.Click += BtnRemoveMapNpc_Click;

        _rbRandom = new RadioButton { Text = Strings.NpcSpawns.randomlocation };
        _rbRandom.CheckedChanged += RbRandom_CheckedChanged;

        _rbDeclared = new RadioButton(_rbRandom) { Text = Strings.NpcSpawns.declaredlocation };
        _rbRandom.Checked = true;

        _lblNpcCount = new Label { Text = Strings.NpcSpawns.SpawnCount.ToString(0) };

        var npcSelectionGroup = new GroupBox { Text = Strings.NpcSpawns.AddRemove };
        npcSelectionGroup.Content = new StackLayout
        {
            Padding = new Padding(4),
            Spacing = 4,
            Items =
            {
                new StackLayout { Orientation = Orientation.Horizontal, Spacing = 4, Items = { new Label { Text = "NPC:" }, _cmbNpc } },
                new StackLayout { Orientation = Orientation.Horizontal, Spacing = 4, Items = { new Label { Text = "Direction:" }, _cmbDir } },
                new StackLayout { Orientation = Orientation.Horizontal, Spacing = 4, Items = { _btnAddMapNpc, _btnRemoveMapNpc } }
            }
        };

        var spawnLocGroup = new GroupBox { Text = Strings.NpcSpawns.spawndeclared };
        spawnLocGroup.Content = new StackLayout
        {
            Padding = new Padding(4),
            Spacing = 4,
            Items = { _rbRandom, _rbDeclared }
        };

        var layout = new DynamicLayout { Padding = new Padding(4), Spacing = new Size(4, 4) };
        layout.AddRow(_lstMapNpcs);
        layout.AddRow(_lblNpcCount);
        layout.AddRow(npcSelectionGroup);
        layout.AddRow(spawnLocGroup);

        _tabNpcs.Content = layout;
    }

    public void Init()
    {
        _cmbAutotile.SelectedIndex = 0;

        if (Options.Instance.Map.Layers.All.Count <= 5)
        {
            _cmbMapLayer.Visible = false;
        }
        else
        {
            _cmbMapLayer.Visible = true;
            _cmbMapLayer.Items.Clear();
            foreach (var layer in Options.Instance.Map.Layers.All)
            {
                _cmbMapLayer.Items.Add(layer);
            }
            if (_cmbMapLayer.Items.Count > 0)
                _cmbMapLayer.SelectedIndex = 0;
        }

        foreach (var layer in Options.Instance.Map.Layers.All)
        {
            LayerVisibility[layer] = true;
        }

        SetLayer(Options.Instance.Map.Layers.All[0]);
        if (_cmbTilesets.Items.Count > 0)
        {
            SetTileset(_cmbTilesets.Items[0].Text);
        }

        PopulateAttributeTypes();
        InitLocalization();
    }

    public void InitTilesets()
    {
        _cmbTilesets.Items.Clear();
        var tilesetList = new List<string>();
        tilesetList.AddRange(TilesetDescriptor.Names);
        tilesetList.Sort(new AlphanumComparatorFast());
        foreach (var filename in tilesetList)
        {
            if (File.Exists("resources/tilesets/" + filename))
            {
                _cmbTilesets.Items.Add(filename);
            }
        }

        if (TilesetDescriptor.Lookup.Count > 0)
        {
            if (_cmbTilesets.Items.Count > 0)
            {
                _cmbTilesets.SelectedIndex = 0;
            }
            Globals.CurrentTileset = (TilesetDescriptor)TilesetDescriptor.Lookup.Values.ToArray()[0];
        }
    }

    public void InitMapLayers()
    {
        _picTileset?.Invalidate();
    }

    public void SetLayer(string name)
    {
        Globals.CurrentLayer = name;
        _lastTileLayer = name;
        Core.Graphics.TilePreviewUpdated = true;
    }

    public void SetTileset(string name)
    {
        TilesetDescriptor tSet = null;
        var tilesets = TilesetDescriptor.Lookup;
        var id = Guid.Empty;
        foreach (var tileset in tilesets.Pairs)
        {
            if (tileset.Value.Name.ToLower() == name.ToLower())
            {
                id = tileset.Key;
                break;
            }
        }

        if (id != Guid.Empty)
        {
            tSet = TilesetDescriptor.Get(id);
        }

        if (tSet != null)
        {
            if (File.Exists("resources/tilesets/" + tSet.Name))
            {
                _picTileset.Visible = true;
                Globals.CurrentTileset = tSet;
                Globals.CurSelX = 0;
                Globals.CurSelY = 0;
                var tilesetTex = GameContentManager.GetTexture(GameContentManager.TextureType.Tileset, tSet.Name);
                if (tilesetTex != null)
                {
                    _picTileset.Width = tilesetTex.Width;
                    _picTileset.Height = tilesetTex.Height;
                }

                if (_cmbTilesets.Items.Any(item => item.Text == name))
                    _cmbTilesets.SelectedValue = _cmbTilesets.Items.First(item => item.Text == name);

                _picTileset.Invalidate();
            }
        }
    }

    public void SetAutoTile(int index)
    {
        Globals.Autotilemode = index;
        _cmbAutotile.SelectedIndex = index;
        switch (Globals.Autotilemode)
        {
            case 1:
            case 5:
                Globals.CurSelW = 1;
                Globals.CurSelH = 2;
                break;
            case 2:
                Globals.CurSelW = 0;
                Globals.CurSelH = 0;
                break;
            case 3:
                Globals.CurSelW = 5;
                Globals.CurSelH = 2;
                break;
            case 4:
                Globals.CurSelW = 1;
                Globals.CurSelH = 1;
                break;
            case 6:
                Globals.CurSelW = 2;
                Globals.CurSelH = 3;
                break;
            case 7:
                Globals.CurSelW = 8;
                Globals.CurSelH = 3;
                break;
        }
    }

    private void PopulateAttributeTypes()
    {
        var attributeTypes = Enum.GetValues(typeof(MapAttributeType))
            .Cast<MapAttributeType>()
            .Where(type => type != MapAttributeType.Walkable);

        _cmbAttributeType.Items.Clear();
        foreach (var type in attributeTypes)
        {
            if (type == MapAttributeType.ZDimension && !Options.Instance.Map.ZDimensionVisible)
                continue;

            if (AttributeTypeStrings.TryGetValue(type, out var str))
                _cmbAttributeType.Items.Add(str);
            else
                _cmbAttributeType.Items.Add(type.ToString());
        }

        if (_cmbAttributeType.Items.Count > 0)
            _cmbAttributeType.SelectedIndex = 0;
    }

    private static readonly Dictionary<MapAttributeType, string> AttributeTypeStrings = new()
    {
        { MapAttributeType.Animation, Strings.Attributes.MapAnimation },
        { MapAttributeType.Blocked, Strings.Attributes.Blocked },
        { MapAttributeType.Critter, Strings.Attributes.Critter },
        { MapAttributeType.GrappleStone, Strings.Attributes.Grapple },
        { MapAttributeType.Item, Strings.Attributes.ItemSpawn },
        { MapAttributeType.NpcAvoid, Strings.Attributes.NpcAvoid },
        { MapAttributeType.Resource, Strings.Attributes.ResourceSpawn },
        { MapAttributeType.Sound, Strings.Attributes.MapSound },
        { MapAttributeType.Slide, Strings.Attributes.Slide },
        { MapAttributeType.Warp, Strings.Attributes.Warp },
        { MapAttributeType.ZDimension, Strings.Attributes.ZDimension },
    };

    private static readonly Dictionary<string, MapAttributeType> AttributeTypeStringToEnum =
        AttributeTypeStrings.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public MapAttribute CreateAttribute()
    {
        return CreateAttribute(SelectedMapAttributeType);
    }

    public MapAttribute CreateAttribute(MapAttributeType attributeType)
    {
        switch (attributeType)
        {
            case MapAttributeType.Walkable:
            case MapAttributeType.Blocked:
            case MapAttributeType.GrappleStone:
            case MapAttributeType.NpcAvoid:
                return MapAttribute.CreateAttribute(attributeType);
            case MapAttributeType.Item:
                return CreateItemAttribute();
            case MapAttributeType.ZDimension:
                return CreateZDimensionAttribute();
            case MapAttributeType.Warp:
                return CreateWarpAttribute();
            case MapAttributeType.Sound:
                return CreateSoundAttribute();
            case MapAttributeType.Resource:
                return CreateResourceAttribute();
            case MapAttributeType.Animation:
                return CreateAnimationAttribute();
            case MapAttributeType.Slide:
                return CreateSlideAttribute();
            case MapAttributeType.Critter:
                return CreateCritterAttribute();
            default:
                throw new ArgumentOutOfRangeException(nameof(attributeType));
        }
    }

    private MapItemAttribute CreateItemAttribute()
    {
        var itemAttribute = (MapItemAttribute)MapAttribute.CreateAttribute(MapAttributeType.Item);
        return itemAttribute;
    }

    private MapZDimensionAttribute CreateZDimensionAttribute()
    {
        var zDimensionAttribute = (MapZDimensionAttribute)MapAttribute.CreateAttribute(MapAttributeType.ZDimension);
        return zDimensionAttribute;
    }

    private MapWarpAttribute CreateWarpAttribute()
    {
        var warpAttribute = (MapWarpAttribute)MapAttribute.CreateAttribute(MapAttributeType.Warp);
        return warpAttribute;
    }

    private MapSoundAttribute CreateSoundAttribute()
    {
        var soundAttribute = (MapSoundAttribute)MapAttribute.CreateAttribute(MapAttributeType.Sound);
        return soundAttribute;
    }

    private MapResourceAttribute CreateResourceAttribute()
    {
        var resourceAttribute = (MapResourceAttribute)MapAttribute.CreateAttribute(MapAttributeType.Resource);
        return resourceAttribute;
    }

    private MapAnimationAttribute CreateAnimationAttribute()
    {
        var animationAttribute = (MapAnimationAttribute)MapAttribute.CreateAttribute(MapAttributeType.Animation);
        return animationAttribute;
    }

    private MapSlideAttribute CreateSlideAttribute()
    {
        var slideAttribute = (MapSlideAttribute)MapAttribute.CreateAttribute(MapAttributeType.Slide);
        return slideAttribute;
    }

    private MapCritterAttribute CreateCritterAttribute()
    {
        var critterAttribute = (MapCritterAttribute)MapAttribute.CreateAttribute(MapAttributeType.Critter);
        return critterAttribute;
    }

    private MapAttributeType SelectedMapAttributeType
    {
        get
        {
            if (_cmbAttributeType.SelectedValue is string selectedString &&
                AttributeTypeStringToEnum.TryGetValue(selectedString, out var type))
            {
                return type;
            }
            return MapAttributeType.Blocked;
        }
    }

    public MapAttribute PlaceAttribute(MapDescriptor mapDescriptor, int x, int y, MapAttribute attribute = null)
    {
        if (attribute == null)
        {
            attribute = CreateAttribute();
        }
        mapDescriptor.Attributes[x, y] = attribute;
        return attribute;
    }

    public bool RemoveAttribute(MapDescriptor tmpMap, int x, int y)
    {
        if (tmpMap.Attributes[x, y] != null && tmpMap.Attributes[x, y].Type != MapAttributeType.Walkable)
        {
            tmpMap.Attributes[x, y] = null;
            return true;
        }
        return false;
    }

    public void RefreshNpcList()
    {
        _cmbNpc.Items.Clear();
        foreach (var name in NPCDescriptor.Names)
        {
            _cmbNpc.Items.Add(name);
        }

        _lstMapNpcs.Items.Clear();
        for (var i = 0; i < Globals.CurrentMap.Spawns.Count; i++)
        {
            _lstMapNpcs.Items.Add(NPCDescriptor.GetName(Globals.CurrentMap.Spawns[i].NpcId));
        }

        if (_cmbNpc.Items.Count > 0)
        {
            _cmbNpc.SelectedIndex = 0;
        }

        _cmbDir.SelectedIndex = 0;
        _rbRandom.Checked = true;
        if (_lstMapNpcs.Items.Count > 0)
        {
            _lstMapNpcs.SelectedIndex = 0;
            if (_lstMapNpcs.SelectedIndex < Globals.CurrentMap.Spawns.Count)
            {
                _cmbDir.SelectedIndex = (int)Globals.CurrentMap.Spawns[_lstMapNpcs.SelectedIndex].Direction;
                _cmbNpc.SelectedIndex = NPCDescriptor.ListIndex(Globals.CurrentMap.Spawns[_lstMapNpcs.SelectedIndex].NpcId);
                if (Globals.CurrentMap.Spawns[_lstMapNpcs.SelectedIndex].X >= 0)
                {
                    _rbDeclared.Checked = true;
                }
            }
        }

        UpdateNpcCountLabel();
    }

    private void UpdateNpcCountLabel()
    {
        if (_lblNpcCount != null)
            _lblNpcCount.Text = Strings.NpcSpawns.SpawnCount.ToString(_lstMapNpcs.Items.Count);
    }

    private void InitLocalization()
    {
        //Title removed - Panel has no Text
    }

    private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedTab = _tabControl.SelectedPage;
        if (selectedTab == _tabTiles)
        {
            Globals.CurrentTool = Globals.SavedTool;
            SetLayer(_lastTileLayer ?? Options.Instance.Map.Layers.All[0]);
            CurrentTab = LayerTabs.Tiles;
        }
        else if (selectedTab == _tabAttributes)
        {
            Globals.CurrentTool = Globals.SavedTool;
            Globals.CurrentLayer = LayerOptions.Attributes;
            CurrentTab = LayerTabs.Attributes;
        }
        else if (selectedTab == _tabLights)
        {
            if (Globals.CurrentLayer != LayerOptions.Lights && Globals.CurrentLayer != LayerOptions.Events && Globals.CurrentLayer != LayerOptions.Npcs)
            {
                Globals.SavedTool = Globals.CurrentTool;
            }
            Globals.CurrentLayer = LayerOptions.Lights;
            CurrentTab = LayerTabs.Lights;
        }
        else if (selectedTab == _tabEvents)
        {
            if (Globals.CurrentLayer != LayerOptions.Lights && Globals.CurrentLayer != LayerOptions.Events && Globals.CurrentLayer != LayerOptions.Npcs)
            {
                Globals.SavedTool = Globals.CurrentTool;
            }
            Globals.CurrentLayer = LayerOptions.Events;
            CurrentTab = LayerTabs.Events;
        }
        else if (selectedTab == _tabNpcs)
        {
            if (Globals.CurrentLayer != LayerOptions.Lights && Globals.CurrentLayer != LayerOptions.Events && Globals.CurrentLayer != LayerOptions.Npcs)
            {
                Globals.SavedTool = Globals.CurrentTool;
            }
            Globals.CurrentLayer = LayerOptions.Npcs;
            RefreshNpcList();
            CurrentTab = LayerTabs.Npcs;
        }
        Core.Graphics.TilePreviewUpdated = true;
    }

    private void CmbTilesets_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_cmbTilesets.SelectedIndex >= 0)
        {
            SetTileset(_cmbTilesets.Items[_cmbTilesets.SelectedIndex].Text);
        }
    }

    private void CmbAutotile_SelectedIndexChanged(object sender, EventArgs e)
    {
        SetAutoTile(_cmbAutotile.SelectedIndex);
    }

    private void CmbAttributeType_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    private void PicTileset_MouseDown(object sender, MouseEventArgs e)
    {
        var x = (int)e.Location.X;
        var y = (int)e.Location.Y;

        if (x > _picTileset.Width || y > _picTileset.Height)
            return;

        _tilesetMouseDown = true;
        Globals.CurSelX = (int)Math.Floor((double)x / Options.Instance.Map.TileWidth);
        Globals.CurSelY = (int)Math.Floor((double)y / Options.Instance.Map.TileHeight);
        Globals.CurSelW = 0;
        Globals.CurSelH = 0;
        if (Globals.CurSelX < 0) Globals.CurSelX = 0;
        if (Globals.CurSelY < 0) Globals.CurSelY = 0;

        switch (Globals.Autotilemode)
        {
            case 1:
            case 5:
                Globals.CurSelW = 1;
                Globals.CurSelH = 2;
                break;
            case 2:
                Globals.CurSelW = 0;
                Globals.CurSelH = 0;
                break;
            case 3:
                Globals.CurSelW = 5;
                Globals.CurSelH = 2;
                break;
            case 4:
                Globals.CurSelW = 1;
                Globals.CurSelH = 1;
                break;
            case 6:
                Globals.CurSelW = 2;
                Globals.CurSelH = 3;
                break;
            case 7:
                Globals.CurSelW = 8;
                Globals.CurSelH = 3;
                break;
        }
    }

    private void PicTileset_MouseMove(object sender, MouseEventArgs e)
    {
        var x = (int)e.Location.X;
        var y = (int)e.Location.Y;

        if (x > _picTileset.Width || y > _picTileset.Height)
            return;

        if (_tilesetMouseDown && Globals.Autotilemode == 0)
        {
            var tmpX = (int)Math.Floor((double)x / Options.Instance.Map.TileWidth);
            var tmpY = (int)Math.Floor((double)y / Options.Instance.Map.TileHeight);
            Globals.CurSelW = tmpX - Globals.CurSelX;
            Globals.CurSelH = tmpY - Globals.CurSelY;
        }
    }

    private void PicTileset_MouseUp(object sender, MouseEventArgs e)
    {
        var selX = Globals.CurSelX;
        var selY = Globals.CurSelY;
        var selW = Globals.CurSelW;
        var selH = Globals.CurSelH;
        if (selW < 0)
        {
            selX -= Math.Abs(selW);
            selW = Math.Abs(selW);
        }
        if (selH < 0)
        {
            selY -= Math.Abs(selH);
            selH = Math.Abs(selH);
        }
        Globals.CurSelX = selX;
        Globals.CurSelY = selY;
        Globals.CurSelW = selW;
        Globals.CurSelH = selH;
        _tilesetMouseDown = false;
    }

    private void PicTileset_Paint(object sender, PaintEventArgs e)
    {
        if (Globals.CurrentTileset != null)
        {
            var tilesetTex = GameContentManager.GetTexture(GameContentManager.TextureType.Tileset, Globals.CurrentTileset.Name);
            if (tilesetTex != null)
            {
                // Load the tileset as an Eto bitmap for display
                var tilesetBmp = Core.Graphics.LoadTilesetBitmap(Globals.CurrentTileset.Name);
                if (tilesetBmp != null)
                {
                    e.Graphics.DrawImage(tilesetBmp, 0, 0);

                    // Draw selection rectangle if tileset is selected
                    if (Globals.CurSelW != 0 || Globals.CurSelH != 0)
                    {
                        var selX = Math.Min(Globals.CurSelX, Globals.CurSelX + Globals.CurSelW) * Options.Instance.Map.TileWidth;
                        var selY = Math.Min(Globals.CurSelY, Globals.CurSelY + Globals.CurSelH) * Options.Instance.Map.TileHeight;
                        var selW = (Math.Abs(Globals.CurSelW) + 1) * Options.Instance.Map.TileWidth;
                        var selH = (Math.Abs(Globals.CurSelH) + 1) * Options.Instance.Map.TileHeight;
                        e.Graphics.DrawRectangle(Colors.Yellow, selX, selY, selW, selH);
                    }
                }
            }
        }
    }

    private void BtnAddMapNpc_Click(object sender, EventArgs e)
    {
        var n = new NpcSpawn();
        if (_cmbNpc.SelectedIndex > -1)
        {
            n.NpcId = NPCDescriptor.IdFromList(_cmbNpc.SelectedIndex);
            n.X = -1;
            n.Y = -1;
            n.Direction = NpcSpawnDirection.Random;

            Globals.CurrentMap.Spawns.Add(n);
            _lstMapNpcs.Items.Add(NPCDescriptor.GetName(n.NpcId));
            _lstMapNpcs.SelectedIndex = _lstMapNpcs.Items.Count - 1;
        }
        UpdateNpcCountLabel();
    }

    private void BtnRemoveMapNpc_Click(object sender, EventArgs e)
    {
        if (_lstMapNpcs.SelectedIndex > -1)
        {
            Globals.CurrentMap.Spawns.RemoveAt(_lstMapNpcs.SelectedIndex);
            _lstMapNpcs.Items.RemoveAt(_lstMapNpcs.SelectedIndex);

            _lstMapNpcs.Items.Clear();
            for (var i = 0; i < Globals.CurrentMap.Spawns.Count; i++)
            {
                _lstMapNpcs.Items.Add(NPCDescriptor.GetName(Globals.CurrentMap.Spawns[i].NpcId));
            }

            if (_lstMapNpcs.Items.Count > 0)
            {
                _lstMapNpcs.SelectedIndex = 0;
            }

            Core.Graphics.TilePreviewUpdated = true;
        }
        UpdateNpcCountLabel();
    }

    private void LstMapNpcs_SelectedIndexChanged(object sender, EventArgs e)
    {
        LstMapNpcs_Update();
    }

    private void LstMapNpcs_Update()
    {
        if (_lstMapNpcs.Items.Count <= 0 || _lstMapNpcs.SelectedIndex <= -1)
            return;

        var selectedSpawn = Globals.CurrentMap.Spawns[_lstMapNpcs.SelectedIndex];
        _cmbNpc.SelectedIndex = NPCDescriptor.ListIndex(selectedSpawn.NpcId);
        _cmbDir.SelectedIndex = (int)selectedSpawn.Direction;
        _rbDeclared.Checked = selectedSpawn.X >= 0;
        _rbRandom.Checked = !_rbDeclared.Checked;
    }

    private void RbRandom_CheckedChanged(object sender, EventArgs e)
    {
        if (_lstMapNpcs.SelectedIndex > -1)
        {
            if (_rbRandom.Checked == true)
            {
                Globals.CurrentMap.Spawns[_lstMapNpcs.SelectedIndex].X = -1;
                Globals.CurrentMap.Spawns[_lstMapNpcs.SelectedIndex].Y = -1;
                Globals.CurrentMap.Spawns[_lstMapNpcs.SelectedIndex].Direction = NpcSpawnDirection.Random;
                Core.Graphics.TilePreviewUpdated = true;
            }
        }
    }

    private void CmbDir_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_lstMapNpcs.SelectedIndex >= 0 && _cmbDir.SelectedIndex >= 0)
        {
            Globals.CurrentMap.Spawns[_lstMapNpcs.SelectedIndex].Direction = (NpcSpawnDirection)_cmbDir.SelectedIndex;
        }
    }

    private void CmbNpc_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_lstMapNpcs.SelectedIndex >= 0 && _cmbNpc.SelectedIndex >= 0)
        {
            Globals.CurrentMap.Spawns[_lstMapNpcs.SelectedIndex].NpcId = NPCDescriptor.IdFromList(_cmbNpc.SelectedIndex);

            var n = _lstMapNpcs.SelectedIndex;
            _lstMapNpcs.Items.Clear();
            for (var i = 0; i < Globals.CurrentMap.Spawns.Count; i++)
            {
                _lstMapNpcs.Items.Add(NPCDescriptor.GetName(Globals.CurrentMap.Spawns[i].NpcId));
            }
            _lstMapNpcs.SelectedIndex = n;
        }
    }
}
