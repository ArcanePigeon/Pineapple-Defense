using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using System.Linq;

public enum SpawnLocation
{
    UP = 0, RIGHT, DOWN, LEFT
}
public class Wave
{
    public EnemyType type;
    public SpawnLocation spawnLocation;
    public int enemiesLeft;
    public int difficulty;

    public Wave(EnemyType type, SpawnLocation spawnLocation, int enemiesLeft, int difficulty)
    {
        this.type = type;
        this.spawnLocation = spawnLocation;
        this.enemiesLeft = enemiesLeft;
        this.difficulty = difficulty;
    }
}
public struct PlayerStats
{
    public int health;
    public int money;
    public int score;

    public PlayerStats(int health, int money, int score)
    {
        this.health = health;
        this.money = money;
        this.score = score;
    }
}
public class GameScript : MonoBehaviour {
    private bool isPaused = false;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject boardBack;
    [SerializeField] private Transform waveBoxSpawn;
    [SerializeField] private GameObject upgradeButton;
    public Dictionary<Vector2,Tile> tiles;
    private float tileWidth;
    private const float GRID_SIZE = 16;
    private float boardWidth;
    private List<TickableObject> tickableObjects;
    private List<TickableObject> tickableTowers;
    private int selectedTowerInShop = -1;
    private Tile selectedTile;
    [SerializeField] private Transform[] enemySpawn = new Transform[4];
    [SerializeField] private Transform masterPineapple;
    private int activeProjectiles;
    private Dictionary<GameObject, Projectile> projectilePool;
    private int activeEnemies;
    private Dictionary<Enemy, GameObject> enemyPool;
    private int activeWaveBoxes;
    private Dictionary<GameObject, WaveBox> waveBoxPool;
    private Stack<Node> pathUp;
    private Stack<Node> pathDown;
    private Stack<Node> pathLeft;
    private Stack<Node> pathRight;
    private Stack<Node> flyingPath;
    private Vector2[] startingTile = { new Vector2(7, -2) , new Vector2(17, 7), new Vector2(7, 17), new Vector2(-2, 7) };
    public bool boardHasChanged = true;
    private int level = 0;
    private const float NORMAL_WAVE_SPEED = 0.1f;
    private const float FAST_WAVE_SPEED = 6f;
    private float waveBoxSpeed = 0.1f;
    private string waveBoxPath = "Level/WaveBox";
    private List<Wave> activeWaves;
    private EnemyType[] waveOrder = { EnemyType.NORMAL, EnemyType.IMMUNE, EnemyType.GROUP, EnemyType.FAST, EnemyType.FLYING, EnemyType.SWARM, EnemyType.BOSS};
    private Timer waveTimer;
    private PlayerStats playerStats;
    private bool isTowerLevelVisible = true;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text scoreText;

    [SerializeField] private TMP_Text upgradeCostText;
    [SerializeField] private TMP_Text sellAmountText;

    [SerializeField] private TMP_Text towerNameText;
    [SerializeField] private TMP_Text towerLevelText;
    [SerializeField] private TMP_Text towerDescriptionText;

    [SerializeField] private TMP_Text speedCurrentValue;
    [SerializeField] private TMP_Text speedUpgradeValue;

    [SerializeField] private TMP_Text damageCurrentValue;
    [SerializeField] private TMP_Text damageUpgradeValue;

    [SerializeField] private TMP_Text rangeCurrentValue;
    [SerializeField] private TMP_Text rangeUpgradeValue;

    [SerializeField] private TMP_Text specialCurrentValue;
    [SerializeField] private TMP_Text specialUpgradeValue;

    [SerializeField] private GameObject specialStatus;

    [SerializeField] private GameObject towerStatsCard;

    [SerializeField] private GameObject upgradeSellUI;

    [SerializeField] private GameObject costUI;
    [SerializeField] private TMP_Text towerCostText;

    [SerializeField] private Animator[] shopButtons = new Animator[7];

    [SerializeField] private GameObject menu;

    private const int MAX_WAVES = 35;

    private int sentWaves;
    private int[,] hiddenTileLocations = {
            { 5, -1 }, { 6, -1 }, { 7, -1 }, { 8, -1 }, { 9, -1 }, { 10, -1 }, { 7, -2 },
            { -1, 5 }, { -1, 6 }, { -1, 7 }, { -1, 8 }, { -1, 9 }, { -1, 10 }, { -2, 7 },
            { 5, 16 }, { 6, 16 }, { 7, 16 }, { 8, 16 }, { 9, 16 }, { 10, 16 }, { 7, 17 },
            { 16, 5 }, { 16, 6 }, { 16, 7 }, { 16, 8 }, { 16, 9 }, { 16, 10 }, { 17, 7 }};

    private Dictionary<TowerType, TowerInfo> towerStatInfo;

    private TowerStats[] pineappleCannonTowerStats = {
        new TowerStats(10,    7,   3,     5,   1,     0.5,  true),
        new TowerStats(30,    28,  2.8,   8,   1.2,   0.8,  true),
        new TowerStats(90,    91,  2.6,   13,  1.6,   1.1,  true),
        new TowerStats(180,   217, 2.4,   20,  2.0,   1.5,  true),
        new TowerStats(350,   462, 2.0,   45,  2.9,   2.0,  true)
    };

    private TowerStats[] pinaColadaTowerStats = {
        new TowerStats(15,    10,   4,     0,   1.4,   2,    true),
        new TowerStats(60,    52,   3.8,   0,   1.8,   2.3,  true),
        new TowerStats(150,   157,  3.3,   0,   2.1,   2.8,  true),
        new TowerStats(400,   437,  3,     0,   2.6,   3.5,  true),
        new TowerStats(900,   1067, 2.6,   0,   3.1,   4.5,  true)
    };

    private TowerStats[] gatlinPineappleTowerStats = {
        new TowerStats(8,    5,     2.5,     1,   1,       3,  true),
        new TowerStats(25,   23,    2.1,     2,   1.4,     4,  true),
        new TowerStats(70,   72,    1.7,     4,   1.9,     5,  true),
        new TowerStats(250,  247,   1.3,     7,   2.6,     7,  true),
        new TowerStats(800,  807,   1.0,     12,  3.0,     10, true)
    };

    private TowerStats[] acidicJuicerTowerStats = {
        new TowerStats(6,    4,    1.3,     1,   0.8,     0,  false),
        new TowerStats(20,   18,   1.2,     2,   1.0,     0,  false),
        new TowerStats(100,  88,   1.1,     4,   1.3,     0,  false),
        new TowerStats(450,  403,  1.0,     6,   1.7,     0,  false),
        new TowerStats(1000, 1103, 0.9,     9,   2.2,     0,  false)
    };

    private TowerStats[] sliceThrowerTowerStats = {
        new TowerStats(10,    7,     3,     3,   1.5,     1,  true),
        new TowerStats(50,    42,    2.8,   6,   1.8,     2,  true),
        new TowerStats(120,   126,   2.6,   9,   2.2,     4,  true),
        new TowerStats(700,   616,   2.4,   12,  2.7,     8,  true),
        new TowerStats(1200,  1456,  2,     25,  3.3,     10, true)
    };

    private TowerStats[] thornTosserTowerStats = {
        new TowerStats(8,    5,     1.8,     2,   1,       1,  true),
        new TowerStats(25,   23,    1.6,     4,   1.1,     2,  true),
        new TowerStats(70,   72,    1.4,     8,   1.2,     3,  true),
        new TowerStats(250,  247,   1.0,     15,  1.3,     4,  true),
        new TowerStats(800,  807,   0.8,     35,  2.0,     8,  true)
    };

    private TowerStats[] pineappleWallTowerStats = {
        new TowerStats(4,    2,   0,     0,   0,     0,  false),
        new TowerStats(0,    0,   0,     0,   0,     0,  false),
        new TowerStats(0,    0,   0,     0,   0,     0,  false),
        new TowerStats(0,    0,   0,     0,   0,     0,  false),
        new TowerStats(0,    0,   0,     0,   0,     0,  false)
    };


    void Start() {
        playerStats = new PlayerStats(100, 100, 0);
        tiles = new Dictionary<Vector2, Tile>();
        tickableObjects = new List<TickableObject>();
        tickableTowers = new List<TickableObject>();
        projectilePool = new Dictionary<GameObject, Projectile>();
        enemyPool = new Dictionary<Enemy, GameObject>();
        waveBoxPool = new Dictionary<GameObject, WaveBox>();
        towerStatInfo = new Dictionary<TowerType, TowerInfo>();
        AddTowerStatInfo();
        activeWaves = new List<Wave>();
        boardWidth = boardBack.transform.localScale.x;
        tileWidth = boardWidth / (float)GRID_SIZE;
        tilePrefab.transform.localScale = new Vector3(tileWidth, tileWidth, 1);
        CreateGrid();
        CreateHiddenTiles();
        InitAStar();
        waveTimer = new Timer(1f, false);
        sentWaves = 0;
        NewWaveBox();
    }

    private void InitAStar()
    {
        AStar.Init(this);
        AStar.CreateNodes(tiles);
        GenerateSpawnPaths();
        flyingPath = new Stack<Node>();
        flyingPath.Push(AStar.nodes[new Vector2(7, 7)]);
    }

    private void GenerateSpawnPaths()
    {
        pathUp = AStar.GetPath(startingTile[(int)SpawnLocation.UP]);
        pathDown = AStar.GetPath(startingTile[(int)SpawnLocation.DOWN]);
        pathLeft = AStar.GetPath(startingTile[(int)SpawnLocation.LEFT]);
        pathRight = AStar.GetPath(startingTile[(int)SpawnLocation.RIGHT]);
    }

    public void RestartGame()
    {
        playerStats = new PlayerStats(100, 100, 0);
        foreach(var obj in tickableObjects)
        {
            obj.Disable();
        }
        foreach(Tower tower in tickableTowers)
        {
            tower.Destroy();
        }
        foreach(var box in waveBoxPool)
        {
            box.Value.Disable();
        }
        waveBoxSpeed = NORMAL_WAVE_SPEED;
        tickableTowers = new List<TickableObject>();
        activeWaves = new List<Wave>();
        activeEnemies = 0;
        activeProjectiles = 0;
        activeWaveBoxes = 0;
        level = 0;
        GenerateSpawnPaths();
        sentWaves = 0;
        NewWaveBox();
        UpdateText();
        SetTowerStatsCardInfo();
    }
    public void AddTowerStatInfo()
    {
        towerStatInfo.Add(TowerType.PINEAPPLE_CANNON, new TowerInfo("Pineapple Cannon", "Fires explosive pineapples that do lots of damage to enemies in the blast radius. Special: Radius of blast.",pineappleCannonTowerStats[0]));
        towerStatInfo.Add(TowerType.PINA_COLADA, new TowerInfo("Pina Colada", "Fires ice cubes that slow down enemies. Special: Enemy slow down duration.",pinaColadaTowerStats[0]));
        towerStatInfo.Add(TowerType.GATLIN_PINEAPPLE, new TowerInfo("Gatlin Pineapple", "Fires bursts of sharp leaves that do damage to enemies. Special: Number of leaves per burst.",gatlinPineappleTowerStats[0]));
        towerStatInfo.Add(TowerType.ACIDIC_JUICER, new TowerInfo("Acidic Juicer", "Spreads a radius of acidic juce around it dealing lower damage quickly to enemies that pass over it.",acidicJuicerTowerStats[0]));
        towerStatInfo.Add(TowerType.SLICE_THROWER, new TowerInfo("Slice Thrower", "Lanuches slices of pineapple that can pierce through enemies. Special: Number of enemies the slice can hit before breaking.",sliceThrowerTowerStats[0]));
        towerStatInfo.Add(TowerType.THORN_TOSSER, new TowerInfo("Thorn Tosser", "Rapidly fires thorns in all directions that can pierece through enemies. Special: Number of enemies the slice can hit before breaking.",thornTosserTowerStats[0]));
        towerStatInfo.Add(TowerType.PINEAPPLE_WALL, new TowerInfo("Pineapple Wall", "Just a cheap wall.",pineappleWallTowerStats[0]));
    }
    public float GetWaveBoxSpeed()
    {
        return waveBoxSpeed;
    }

    public void SendNextWave()
    {
        sentWaves += 1;
        waveBoxSpeed = FAST_WAVE_SPEED;
    }

    private void CreateGrid() {
        var x = boardBack.transform.position.x - (boardWidth / 2f) + (tileWidth / 2f);
        var y = boardBack.transform.position.y + (boardWidth / 2f) - (tileWidth / 2f);
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for(int j = 0; j < GRID_SIZE; j++)
            {
                var pos = new Vector2(i,j);
                var tile = Instantiate(tilePrefab, new Vector3(x+(tileWidth*i),y-(tileWidth*j)), Quaternion.identity);
                var isOffset = (i % 2 == 0 && j % 2 != 0) || (i % 2 != 0 && j % 2 == 0);
                tile.Init(this, isOffset, boardBack.transform, pos, false);
                tile.name = "Tile " + i + "," + j;
                tiles.Add(pos, tile);
                if((x == 7 || x == 8) && (y == 7 || y == 8))
                {
                    tile.SetSpecialTile(false, false);
                }
            }
        }
    }

    private void CreateHiddenTiles()
    {
        var x = boardBack.transform.position.x - (boardWidth / 2f) + (tileWidth / 2f);
        var y = boardBack.transform.position.y + (boardWidth / 2f) - (tileWidth / 2f);
        for (int k = 0; k < hiddenTileLocations.GetLength(0); k++)
        {
            var i = hiddenTileLocations[k, 0];
            var j = hiddenTileLocations[k, 1];
            var pos = new Vector2(i, j);
            var tile = Instantiate(tilePrefab, new Vector3(x + (tileWidth * i), y - (tileWidth * j)), Quaternion.identity);
            var isOffset = (i % 2 == 0 && j % 2 != 0) || (i % 2 != 0 && j % 2 == 0);
            tile.Init(this, isOffset, boardBack.transform, pos, false);
            tile.name = "Tile " + i + "," + j;
            tiles.Add(pos, tile);
            tile.SetSpecialTile(false, true);
            if(j == 5 || j == 10 || i == 5 || i == 10)
            {
                tile.isOccupied = true;
            }
        }
    }

    public GameObject InstantiateWithParent(string path, string name, Transform parent)
    {
        var prefab = Resources.Load(path) as GameObject;
        var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        return obj;
    }
    public GameObject InstantiatePrefab(string path, float x, float y, string name)
    {
        var prefab = Resources.Load(path) as GameObject;
        var obj = Instantiate(prefab, new Vector3(x,y,1), Quaternion.identity);
        obj.name = name;

        return obj;
    }
    public void PlaceTower(Tile tile)
    {
        int cost = towerStatInfo[(TowerType)selectedTowerInShop].towerStats.cost;
        if ( playerStats.money < cost)
        {
            return;
        }
        playerStats.money -= cost;
        UpdateText();
        Tower tower;
        switch ((TowerType)selectedTowerInShop)
        {
            case TowerType.PINEAPPLE_CANNON:
                var towerObject = InstantiateWithParent(PineappleCannonTower.towerPath, "PineappleCannon", tile.transform);
                tower = new PineappleCannonTower(this, towerObject,pineappleCannonTowerStats);
                break;
            case TowerType.PINA_COLADA:
                towerObject = InstantiateWithParent(PinaColadaTower.towerPath, "PinaColada", tile.transform);
                tower = new PinaColadaTower(this, towerObject, pinaColadaTowerStats);
                break;
            case TowerType.GATLIN_PINEAPPLE:
                towerObject = InstantiateWithParent(GatlinPineappleTower.towerPath, "GatlinPineapple", tile.transform);
                tower = new GatlinPineappleTower(this, towerObject, gatlinPineappleTowerStats);
                break;
            case TowerType.ACIDIC_JUICER:
                towerObject = InstantiateWithParent(AcidicJuicerTower.towerPath, "AcidicJuicer", tile.transform);
                tower = new AcidicJuicerTower(this, towerObject, acidicJuicerTowerStats);
                break;
            case TowerType.SLICE_THROWER:
                towerObject = InstantiateWithParent(SliceThrowerTower.towerPath, "SliceThrower", tile.transform);
                tower = new SliceThrowerTower(this, towerObject, sliceThrowerTowerStats);
                break;
            case TowerType.THORN_TOSSER:
                towerObject = InstantiateWithParent(ThornTosserTower.towerPath, "ThornTosser", tile.transform);
                tower = new ThornTosserTower(this, towerObject, sliceThrowerTowerStats);
                break;
            case TowerType.PINEAPPLE_WALL:
                towerObject = InstantiateWithParent(PineappleWallTower.towerPath, "PineappleWall", tile.transform);
                tower = new PineappleWallTower(this, towerObject, pineappleWallTowerStats);
                break;
            default:
                return;
        }
        tower.ToggleTowerLevel(isTowerLevelVisible);
        tickableTowers.Add(tower);
        tile.tower = tower;
        tile.isOccupied = true;
        boardHasChanged = true;
    }

    public void InteractTile(Tile tile)
    {
        if(selectedTowerInShop == -1)
        {
            if (tile.isOccupied)
            {
                SelectTile(tile);
            }
            else
            {
                DeselectTile();
            }
        }
        else
        {
            if (tile.isOccupied)
            {
                SelectTile(tile);
                return;
            }
            if (CanPlaceTile(tile))
            {
                PlaceTower(tile);
                DeselectTile();
            }
        }
    }

    public bool CanPlaceTile(Tile tile)
    {
        tile.isOccupied = true;
        var notBlockingEntrance = CheckEntrancePaths();
        tile.isOccupied = false;
        return notBlockingEntrance;
    }

    public bool CheckEntrancePaths()
    {
        var tmpPathUp = AStar.GetPath(startingTile[(int)SpawnLocation.UP]);
        if (tmpPathUp.Count == 0)
        {
            return false;
        }
        var tmpPathDown = AStar.GetPath(startingTile[(int)SpawnLocation.DOWN]);
        if (tmpPathDown.Count == 0)
        {
            return false;
        }
        var tmpPathLeft = AStar.GetPath(startingTile[(int)SpawnLocation.LEFT]);
        if (tmpPathLeft.Count == 0)
        {
            return false;
        }
        var tmpPathRight = AStar.GetPath(startingTile[(int)SpawnLocation.RIGHT]);
        if (tmpPathRight.Count == 0)
        {
            return false;
        }
        return true;
    }

    public void SelectTile(Tile tile)
    {
        if (selectedTowerInShop != -1)
        {
            shopButtons[selectedTowerInShop].SetBool("Active", false);
            selectedTowerInShop = -1;
        }
        DeselectTile();
        tile.SelectTile(true);
        selectedTile = tile;
        tile.tower.ToggleTowerRadiusDisplay(true);
        SetTowerStatsCardInfo();
    }

    private void DeselectTile()
    {
        if (selectedTile != null)
        {
            selectedTile.tower.ToggleTowerRadiusDisplay(false);
            selectedTile.SelectTile(false);
        }
        selectedTile = null;
        SetTowerStatsCardInfo();
    }

    public void Deselect()
    {
        if(selectedTowerInShop != -1)
        {
            shopButtons[selectedTowerInShop].SetBool("Active", false);
        }
        selectedTowerInShop = -1;
        DeselectTile();
    }
        
    public void SelectShopTower(int towerIndex)
    {
        if (selectedTowerInShop != -1)
        {
            shopButtons[selectedTowerInShop].SetBool("Active", false);
        }
        selectedTowerInShop = towerIndex;
        if (selectedTowerInShop != -1)
        {
            shopButtons[selectedTowerInShop].SetBool("Active", true);
        }
        DeselectTile();
    }

    private void SetTowerStatsCardInfo()
    {
        if(selectedTowerInShop != -1)
        {
            towerStatsCard.SetActive(true);
            var info = towerStatInfo[(TowerType)selectedTowerInShop];
            towerNameText.text = info.name;
            towerDescriptionText.text = info.description;
            towerCostText.text = "" + info.towerStats.cost;
            costUI.SetActive(true);
            upgradeSellUI.SetActive(false);
            towerLevelText.text = "1";

            speedCurrentValue.text = "" + info.towerStats.speed;
            speedUpgradeValue.text = "";

            damageCurrentValue.text = "" + info.towerStats.damage;
            damageUpgradeValue.text = "";

            rangeCurrentValue.text = "" + info.towerStats.range;
            rangeUpgradeValue.text = "";

            if (info.towerStats.hasSpecial)
            {
                specialStatus.SetActive(true);
                specialCurrentValue.text = "" + info.towerStats.special;
                specialUpgradeValue.text = "";
            }
            else
            {
                specialStatus.SetActive(false);
            }
        }
        else if(selectedTile != null)
        {
            towerStatsCard.SetActive(true);
            Tower tower = selectedTile.tower;
            var currentStatus = tower.currentStats;
            var upgradeStatus = tower.upgradeStats;
            var info = towerStatInfo[tower.type];
            towerNameText.text = info.name;
            towerDescriptionText.text = info.description;
            upgradeCostText.text = "" + upgradeStatus.cost;
            sellAmountText.text = "" + currentStatus.sellAmount;
            costUI.SetActive(false);
            upgradeSellUI.SetActive(true);
            towerLevelText.text = "" + (tower.level+1);

            upgradeButton.SetActive(!tower.maxUpgrade);

            speedCurrentValue.text = "" + currentStatus.speed;
            double deltaSpeed = System.Math.Round((upgradeStatus.speed - currentStatus.speed)*100) / 100;
            speedUpgradeValue.text = "" + deltaSpeed;

            damageCurrentValue.text = "" + currentStatus.damage;
            damageUpgradeValue.text = "+" + (upgradeStatus.damage - currentStatus.damage);

            rangeCurrentValue.text = "" + currentStatus.range;
            double deltaRange = System.Math.Round((upgradeStatus.range - currentStatus.range) * 100) / 100;
            rangeUpgradeValue.text = "+" + deltaRange;

            if (info.towerStats.hasSpecial)
            {
                specialStatus.SetActive(true);
                specialCurrentValue.text = "" + currentStatus.special;
                double deltaSpecial = System.Math.Round((upgradeStatus.special - currentStatus.special) * 100) / 100;
                specialUpgradeValue.text = (deltaSpecial > 0 ? "+" : "") + deltaSpecial;
            }
            else
            {
                specialStatus.SetActive(false);
            }
            if (tower.level + 1 == 5)
            {
                speedUpgradeValue.text = "";
                damageUpgradeValue.text = "";
                rangeUpgradeValue.text = "";
                specialUpgradeValue.text = "";
            }
        }
        else
        {
            towerStatsCard.SetActive(false);
        }

    }

    public ProjectileDamageReturn CollideProjectile(GameObject projectile)
    {
        var proj = projectilePool[projectile];
        var projectileDamage = proj.projectileDamageReturn;
        proj.OnCollision();
        return projectileDamage;
    }

    public void KillProjectile(Projectile projectile)
    {
        projectile.projectile.SetActive(false);
        activeProjectiles -= 1;
    }

    public void SpawnProjctileFromPool(ProjectileType type, Vector3 spawnLocation, Quaternion rotation, ProjectileDamageReturn projectileDamageReturn)
    {
        float x = spawnLocation.x;
        float y = spawnLocation.y;
        if (activeProjectiles == projectilePool.Count)
        {
            foreach (KeyValuePair<GameObject, Projectile> entry in projectilePool)
            {
                if (entry.Value.type == type && !entry.Key.activeInHierarchy)
                {
                    var foundProjectile = entry.Value;
                    foundProjectile.projectile.transform.position = new Vector3(x, y, 1);
                    foundProjectile.projectile.transform.rotation = rotation;
                    foundProjectile.SetProjectileValues(projectileDamageReturn);
                    foundProjectile.UniqueReset();
                    foundProjectile.projectile.SetActive(true);
                    activeProjectiles += 1;
                    return;
                }
            }
        }
        Projectile projectile;
        GameObject projectileObject;
        switch (type)
        {
            case ProjectileType.EXPLOSIVE_PINEAPPLE:
                projectileObject = InstantiatePrefab(ExplosivePineappleProjectile.projectilePath, x, y, "ExplosivePineapple");
                projectileObject.transform.rotation = rotation;
                projectile = new ExplosivePineappleProjectile(this, projectileObject);
                break;
            case ProjectileType.EXPLOSION:
                projectileObject = InstantiatePrefab(Explosion.projectilePath, x, y, "Explosion");
                projectileObject.transform.rotation = rotation;
                projectile = new Explosion(this, projectileObject);
                break;
            case ProjectileType.ICE:
                projectileObject = InstantiatePrefab(IceProjectile.projectilePath, x, y, "Ice");
                projectileObject.transform.rotation = rotation;
                projectile = new IceProjectile(this, projectileObject);
                break;
            case ProjectileType.LEAF:
                projectileObject = InstantiatePrefab(LeafProjectile.projectilePath, x, y, "Leaf");
                projectileObject.transform.rotation = rotation;
                projectile = new LeafProjectile(this, projectileObject);
                break;
            case ProjectileType.SLICE:
                projectileObject = InstantiatePrefab(SliceProjectile.projectilePath, x, y, "Slice");
                projectileObject.transform.rotation = rotation;
                projectile = new SliceProjectile(this, projectileObject);
                break;
            case ProjectileType.THORN:
                projectileObject = InstantiatePrefab(ThornProjectile.projectilePath, x, y, "Thorn");
                projectileObject.transform.rotation = rotation;
                projectile = new ThornProjectile(this, projectileObject);
                break;
            default:
                throw new MissingComponentException("Projectile type " + type + " not recognized");
        }
        projectile.SetProjectileValues(projectileDamageReturn);
        projectile.UniqueReset();
        projectilePool.Add(projectileObject,projectile);
        tickableObjects.Add(projectile);
        activeProjectiles += 1;
    }

    public void KillEnemy(Enemy enemy, bool killedByPlayer)
    {
        if (killedByPlayer)
        {
            playerStats.score += enemy.score;
            playerStats.money += enemy.money;
        }
        else
        {
            playerStats.health -= enemy.GetDamage();
            if (playerStats.health <= 0)
            {
                playerStats.health = 0;
                // GAME OVER
            }
        }
        UpdateText();
        enemyPool[enemy].SetActive(false);
        activeEnemies -= 1;
    }

    public void UpdateText()
    {
        healthText.text = "" + playerStats.health;
        moneyText.text = "" + playerStats.money;
        scoreText.text = "SCORE: " + playerStats.score;
    }

    public void SpawnEnemy(EnemyType type, SpawnLocation spawnLocation, int difficulty)
    {

        var spawnPosition = enemySpawn[(int)spawnLocation].position;
        Enemy enemy = GetEnemyFromPool(type, spawnPosition, difficulty);
        var randomOffset = new Vector3(Random.value - 0.5f, Random.value - 0.5f, 0);
        enemy.enemy.transform.position += randomOffset;

        Stack<Node> path;
        switch (spawnLocation)
        {
            case SpawnLocation.UP:
                path = pathUp;
                break;
            case SpawnLocation.DOWN:
                path = pathDown;
                break;
            case SpawnLocation.LEFT:
                path = pathLeft;
                break;
            case SpawnLocation.RIGHT:
                path = pathRight;
                break;
            default:
                path = pathUp;
                break;
        }
        if(type == EnemyType.FLYING)
        {
            enemy.SetEnemyPath(flyingPath);
            return;
        }
        enemy.SetEnemyPath(new Stack<Node>(path.Reverse()));
    }

    public void SpawnGroup(EnemyType type, Vector3 spawnPosition, Quaternion spawnRotation, int difficulty, Stack<Node> path)
    {
        Enemy enemy = GetEnemyFromPool(type, spawnPosition, difficulty);
        enemy.enemy.transform.rotation = spawnRotation;
        enemy.SetEnemyPath(new Stack<Node>(path.Reverse()));
    }

    public Enemy GetEnemyFromPool(EnemyType type, Vector3 spawnPosition, int difficulty)
    {
        float x = spawnPosition.x;
        float y = spawnPosition.y;
        
        if (activeEnemies == enemyPool.Count)
        {
            foreach (KeyValuePair<Enemy, GameObject> entry in enemyPool)
            {
                if (entry.Key.type == type && !entry.Value.activeInHierarchy)
                {
                    var foundEnemy = entry.Key;
                    foundEnemy.enemy.transform.position = new Vector3(x, y, 1);
                    foundEnemy.SetEnemyStats(difficulty);
                    activeEnemies += 1;
                    foundEnemy.enemy.SetActive(true);
                    return foundEnemy;
                }
            }
        }
        Enemy enemy;
        GameObject enemyObject;
        switch (type)
        {
            case EnemyType.NORMAL:
                enemyObject = InstantiatePrefab(NormalEnemy.enemyPath, 0, 0, "NormalEnemy");
                enemy = new NormalEnemy(this, enemyObject);
                break;
            case EnemyType.IMMUNE:
                enemyObject = InstantiatePrefab(ImmuneEnemy.enemyPath, 0, 0, "ImmuneEnemy");
                enemy = new ImmuneEnemy(this, enemyObject);
                break;
            case EnemyType.GROUP:
                enemyObject = InstantiatePrefab(GroupEnemy.enemyPath, 0, 0, "GroupEnemy");
                enemy = new GroupEnemy(this, enemyObject);
                break;
            case EnemyType.GROUP_BIG:
                enemyObject = InstantiatePrefab(GroupEnemyBig.enemyPath, 0, 0, "GroupEnemyBig");
                enemy = new GroupEnemyBig(this, enemyObject);
                break;
            case EnemyType.GROUP_SMALL:
                enemyObject = InstantiatePrefab(GroupEnemySmall.enemyPath, 0, 0, "GroupEnemySmall");
                enemy = new GroupEnemySmall(this, enemyObject);
                break;
            case EnemyType.FAST:
                enemyObject = InstantiatePrefab(FastEnemy.enemyPath, 0, 0, "FastEnemy");
                enemy = new FastEnemy(this, enemyObject);
                break;
            case EnemyType.FLYING:
                enemyObject = InstantiatePrefab(FlyingEnemy.enemyPath, 0, 0, "FlyingEnemy");
                enemy = new FlyingEnemy(this, enemyObject);
                break;
            case EnemyType.SWARM:
                enemyObject = InstantiatePrefab(SwarmEnemy.enemyPath, 0, 0, "SwarmEnemy");
                enemy = new SwarmEnemy(this, enemyObject);
                break;
            case EnemyType.BOSS:
                enemyObject = InstantiatePrefab(BossEnemy.enemyPath, 0, 0, "BossEnemy");
                enemy = new BossEnemy(this, enemyObject);
                break;
            default:
                throw new MissingComponentException("Enemy type " + type + " not recognized");
        }
        enemy.enemy.transform.position = new Vector3(x, y, 1);
        enemy.SetEnemyStats(difficulty);
        activeEnemies += 1;
        enemy.enemy.SetActive(true);
        enemyPool.Add(enemy, enemy.enemy);
        tickableObjects.Add(enemy);
        return enemy;

    }

    public void NewWave(EnemyType type, int waveLevel)
    {
        SpawnLocation spawnLocation = (SpawnLocation)(waveLevel % 4);
        int waveDifficulty = Mathf.CeilToInt(waveLevel / MAX_WAVES);
        int enemyAmount = 10 + (waveDifficulty);
        Wave wave = new Wave(type, spawnLocation, enemyAmount, waveDifficulty);
        activeWaves.Add(wave);
        if(sentWaves > 0)
        {
            sentWaves -= 1;
        }
        if(sentWaves == 0)
        {
            waveBoxSpeed = NORMAL_WAVE_SPEED;
        }
    }
    public void NewWaveBox()
    {
        if(level == MAX_WAVES)
        {
            return;
        }
        if(activeWaveBoxes == waveBoxPool.Count)
        {
            var waveBoxObj = InstantiateWithParent(waveBoxPath, "WaveBox", waveBoxSpawn);
            var waveBox = waveBoxObj.GetComponent<WaveBox>();
            waveBox.Init(this, waveOrder[(int)(level % 7)], level);
            waveBoxPool.Add(waveBoxObj,waveBox);
            activeWaveBoxes += 1;
            level += 1;
        }
        else
        {
            foreach(KeyValuePair<GameObject, WaveBox> entry in waveBoxPool)
            {
                if (!entry.Key.activeInHierarchy)
                {
                    entry.Key.transform.position = waveBoxSpawn.position;
                    entry.Value.Init(this, waveOrder[(int)(level % 7)], level);
                    entry.Key.SetActive(true);
                    activeWaveBoxes += 1;
                    level += 1;
                    return;
                }
            }
        }
    }
    public void KillWaveBox(GameObject box)
    {
        box.SetActive(false);
        activeWaveBoxes -= 1;
    }

    public void ActivateWave()
    {
        for (int i = 0; i < activeWaves.Count; i++)
        {
            var wave = activeWaves[i];
            if (wave.enemiesLeft == 0)
            {
                activeWaves.RemoveAt(i);
                i--;
                continue;
            }
            SpawnEnemy(wave.type, wave.spawnLocation, wave.difficulty);
            wave.enemiesLeft -= 1;
        }
    }

    public void SellTower()
    {
        if(selectedTile == null)
        {
            return;
        }
        var money = selectedTile.tower.GetTowerStats().sellAmount;
        playerStats.money += money;
        selectedTile.tower.Sell();
        selectedTile.isOccupied = false;
        selectedTile.SelectTile(false);
        selectedTile = null;
        UpdateText();
        SetTowerStatsCardInfo();
        boardHasChanged = true;
    }

    public void UpgradeTower()
    {
        if (selectedTile == null)
        {
            return;
        }
        var cost = selectedTile.tower.GetUpgradeStats().cost;
        if(playerStats.money < cost)
        {
            return;
        }
        if (selectedTile.tower.Upgrade())
        {
            playerStats.money -= cost;
            UpdateText();
            SetTowerStatsCardInfo();
        }
        
    }

    public void RemoveGameObject(GameObject obj)
    {
        Destroy(obj);
    }

    public void RemoveTowerFromList(Tower tower)
    {
        tickableTowers.Remove(tower);
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isTowerLevelVisible = !isTowerLevelVisible;
            foreach(Tower tower in tickableTowers)
            {
                tower.ToggleTowerLevel(isTowerLevelVisible);
            }
        }
        if (!isPaused)
        {
            TickObjects();
            GoTimers();
            CheckTimers();
            CheckBoardState();
        }
    }

    private void TickObjects()
    {
        foreach (TickableObject tower in tickableTowers)
        {
            tower.Tick();
        }
        foreach (TickableObject obj in tickableObjects)
        {
            obj.Tick();
        }
        foreach(KeyValuePair<GameObject, WaveBox> entry in waveBoxPool)
        {
            entry.Value.Tick();
        }
    }

    private void RegenerateEnemyPaths()
    {
        AStar.ResetEnemyPaths();
        foreach (KeyValuePair<Enemy, GameObject> entry in enemyPool)
        {
            if (entry.Value.activeInHierarchy)
            {
                entry.Key.GenerateNewPath();
            }
        }
    }

    private void CheckBoardState()
    {
        if (boardHasChanged)
        {
            GenerateSpawnPaths();
            RegenerateEnemyPaths();
        }
    }

    private void GoTimers()
    {
        waveTimer.Tick();
    }
    private void CheckTimers()
    {
        if (waveTimer.Status())
        {
            ActivateWave();
            waveTimer.ResetTimer();
        }
    }
}
