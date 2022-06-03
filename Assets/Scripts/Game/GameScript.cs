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
public enum GameState
{
    NONE, WIN, LOSE, PLAYING
}
public class GameScript : MonoBehaviour
{
    // Game state
    private bool isPaused = false;
    private int selectedTowerInShop = -1;
    private Tile selectedTile;
    public bool boardHasChanged = true;
    private int level = 0;
    private PlayerStats playerStats;
    private bool isTowerLevelVisible = true;
    private GameState gameState;
    [SerializeField] public bool ENABLE_CHEAT_CODE = false;

    // Object Pools
    private int activeProjectiles;
    private Dictionary<GameObject, Projectile> projectilePool;
    private int activeEnemies;
    private Dictionary<Enemy, GameObject> enemyPool;
    private int activeWaveBoxes;
    private Dictionary<GameObject, WaveBox> waveBoxPool;

    // Board / Enemy Spawning
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject boardBack;
    public Dictionary<Vector2, Tile> tiles;
    private float tileWidth;
    private const float GRID_SIZE = 16;
    private float boardWidth;
    [SerializeField] private Transform[] enemySpawn = new Transform[4];
    [SerializeField] private Transform masterPineapple;
    [SerializeField] private Animator masterPineappleAnimator;
    private Vector2[] startingTile = { new Vector2(7, -2), new Vector2(17, 7), new Vector2(7, 17), new Vector2(-2, 7) };
    // Tiles outside of the placeable field used for pathing
    private int[,] hiddenTileLocations = {
            { 5, -1 }, { 6, -1 }, { 7, -1 }, { 8, -1 }, { 9, -1 }, { 10, -1 }, { 7, -2 },
            { -1, 5 }, { -1, 6 }, { -1, 7 }, { -1, 8 }, { -1, 9 }, { -1, 10 }, { -2, 7 },
            { 5, 16 }, { 6, 16 }, { 7, 16 }, { 8, 16 }, { 9, 16 }, { 10, 16 }, { 7, 17 },
            { 16, 5 }, { 16, 6 }, { 16, 7 }, { 16, 8 }, { 16, 9 }, { 16, 10 }, { 17, 7 }};

    // Pathing
    private Stack<Node> pathUp;
    private Stack<Node> pathDown;
    private Stack<Node> pathLeft;
    private Stack<Node> pathRight;
    private Stack<Node> flyingPath;

    // Wave info
    private const int MAX_WAVES = 35;
    [SerializeField] private Transform waveBoxSpawn;
    private const float NORMAL_WAVE_SPEED = 0.1f;
    private const float FAST_WAVE_SPEED = 6f;
    private float waveBoxSpeed = 0.1f;
    private string waveBoxPath = "Level/WaveBox";
    private List<Wave> activeWaves;
    private EnemyType[] waveOrder = { EnemyType.NORMAL, EnemyType.IMMUNE, EnemyType.GROUP, EnemyType.FAST, EnemyType.FLYING, EnemyType.SWARM, EnemyType.BOSS };
    private Timer waveTimer;
    private int sentWaves;

    // Tickable Objects
    private List<TickableObject> tickableObjects;
    private List<TickableObject> tickableTowers;

    // Game State Text
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text scoreText;

    // Tower Info
    [SerializeField] private GameObject towerStatsCard;

    // Tower Info Text
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

    // Display when tower has a special effect
    // Displays special current and upgrade values
    [SerializeField] private GameObject specialStatus;

    // Display when a tower in the field is selected
    [SerializeField] private GameObject upgradeSellUI;

    // Display when selecting a tower from the shop
    [SerializeField] private GameObject costUI;
    [SerializeField] private TMP_Text towerCostText;

    // Disable when the selected tower is at max upgrade
    [SerializeField] private GameObject upgradeButton;

    // Shop buttons
    [SerializeField] private Animator[] shopButtons = new Animator[7];

    // Main Menu
    [SerializeField] private GameObject menu;
    [SerializeField] private TMP_Text gameStatusText;
    [SerializeField] private TMP_Text playButtonText;
    [SerializeField] private TMP_Text winningScoreText;
    [SerializeField] private GameObject resumeButton;


    // Tower info for shop
    private Dictionary<TowerType, TowerInfo> towerStatInfo;

    // Tower Stats for each level 1 - 5
    // Cost, Speed, Damage, Range, Special, hasSpecial, Upgrade Time
    private TowerStats[] pineappleCannonTowerStats = {
        new TowerStats(10,    3,     5,   1,     0.5,  true, 0f),
        new TowerStats(80,    2.8,   8,   1.2,   0.8,  true, 1f),
        new TowerStats(350,   2.6,   13,  1.4,   1.1,  true, 3f),
        new TowerStats(850,   2.4,   20,  1.6,   1.5,  true, 9f),
        new TowerStats(1500,  2.0,   45,  2.9,   2.0,  true, 20f)
    };

    private TowerStats[] pinaColadaTowerStats = {
        new TowerStats(20,    4,     0,   1.4,   2,    true, 0f),
        new TowerStats(120,   3.8,   0,   1.6,   2.3,  true, 1f),
        new TowerStats(450,   3.3,   0,   1.8,   2.8,  true, 3f),
        new TowerStats(1000,  3,     0,   2.6,   3.5,  true, 9f),
        new TowerStats(2100,  1.6,   0,   3.1,   4.5,  true, 20f)
    };

    private TowerStats[] gatlinPineappleTowerStats = {
        new TowerStats(15,   2.5,     1,   1,       3,  true, 0f),
        new TowerStats(85,   2.1,     2,   1.2,     4,  true, 1f),
        new TowerStats(225,  1.7,     4,   1.6,     5,  true, 3f),
        new TowerStats(750,  1.3,     7,   2.0,     7,  true, 9f),
        new TowerStats(1600, 1.0,     12,  3.0,     10, true, 20f)
    };

    private TowerStats[] acidicJuicerTowerStats = {
        new TowerStats(20,   1.3,     1,   0.8,     0,  false, 0f),
        new TowerStats(50,   1.2,     2,   0.9,     0,  false, 1f),
        new TowerStats(300,  1.1,     4,   1.2,     0,  false, 3f),
        new TowerStats(850,  1.0,     6,   1.4,     0,  false, 9f),
        new TowerStats(2000, 0.9,     9,   2.2,     0,  false, 20f)
    };

    private TowerStats[] sliceThrowerTowerStats = {
        new TowerStats(18,    3,     3,   1.5,     1,  true, 0f),
        new TowerStats(50,    2.8,   6,   1.7,     2,  true, 1f),
        new TowerStats(120,   2.6,   9,   2.0,     4,  true, 3f),
        new TowerStats(700,   2.4,   12,  2.3,     8,  true, 9f),
        new TowerStats(1200,  2,     25,  3.3,     10, true, 20f)
    };

    private TowerStats[] thornTosserTowerStats = {
        new TowerStats(12,   1.8,     2,   1,       1,  true, 0f),
        new TowerStats(40,   1.6,     4,   1.1,     2,  true, 1f),
        new TowerStats(110,  1.4,     8,   1.2,     3,  true, 3f),
        new TowerStats(800,  1.0,     15,  1.3,     4,  true, 9f),
        new TowerStats(1900, 0.8,     35,  2.0,     8,  true, 20f)
    };

    private TowerStats[] pineappleWallTowerStats = {
        new TowerStats(2,    0,     0,   0,     0,  false, 0f),
        new TowerStats(0,    0,     0,   0,     0,  false, 0f),
        new TowerStats(0,    0,     0,   0,     0,  false, 0f),
        new TowerStats(0,    0,     0,   0,     0,  false, 0f),
        new TowerStats(0,    0,     0,   0,     0,  false, 0f)
    };


    void Start()
    {
        gameState = GameState.NONE;
        UpdateGameStatus();
        Menu(true);
        playerStats = new PlayerStats(100, 200, 0);
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
        UpdateStatsText();
        UpdateTowerStatsCardInfo();
    }

    public void Menu(bool open)
    {
        if (open)
        {
            isPaused = true;
            menu.SetActive(true);
        }
        else
        {
            isPaused = false;
            menu.SetActive(false);
        }
    }

    public void PlayGame()
    {

        if (gameState != GameState.NONE)
        {
            RestartGame();
        }
        gameState = GameState.PLAYING;
        UpdateGameStatus();
        Menu(false);
    }
    public void Quit()
    {
        Application.Quit();
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
        Deselect();
        playerStats = new PlayerStats(100, 200, 0);
        foreach (var obj in tickableObjects)
        {
            obj.Disable();
        }
        foreach (KeyValuePair<Vector2, Tile> entry in tiles)
        {
            entry.Value.ClearTile();
        }
        foreach (var box in waveBoxPool)
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
        UpdateStatsText();
        UpdateTowerStatsCardInfo();
    }
    public void UpdateGameStatus()
    {
        resumeButton.SetActive(false);
        winningScoreText.text = "";
        switch (gameState)
        {
            case GameState.NONE:
                gameStatusText.color = Color.white;
                gameStatusText.text = "Pineapple Defense";
                playButtonText.text = "Play";
                break;
            case GameState.WIN:
                gameStatusText.color = Color.green;
                gameStatusText.text = "YOU WIN";
                playButtonText.text = "Play Again";
                winningScoreText.text = "Score: " + playerStats.score;
                Menu(true);
                break;
            case GameState.LOSE:
                gameStatusText.color = Color.red;
                gameStatusText.text = "GAME OVER";
                playButtonText.text = "Retry";
                winningScoreText.text = "Score: " + playerStats.score;
                Menu(true);
                break;
            case GameState.PLAYING:
                gameStatusText.color = Color.white;
                gameStatusText.text = "PAUSED";
                playButtonText.text = "Restart";
                resumeButton.SetActive(true);
                break;
            default:
                break;
        }
    }
    public void AddTowerStatInfo()
    {
        towerStatInfo.Add(TowerType.PINEAPPLE_CANNON, new TowerInfo("Pineapple Cannon", "Fires explosive pineapples that do lots of damage to enemies in the blast radius. Special: Radius of blast.", pineappleCannonTowerStats[0]));
        towerStatInfo.Add(TowerType.PINA_COLADA, new TowerInfo("Pina Colada", "Fires ice cubes that slow down enemies. Special: Enemy slow down duration.", pinaColadaTowerStats[0]));
        towerStatInfo.Add(TowerType.GATLIN_PINEAPPLE, new TowerInfo("Gatlin Pineapple", "Fires bursts of sharp leaves that do damage to enemies. Special: Number of leaves per burst.", gatlinPineappleTowerStats[0]));
        towerStatInfo.Add(TowerType.ACIDIC_JUICER, new TowerInfo("Acidic Juicer", "Spreads a radius of acidic juce around it dealing lower damage quickly to enemies that pass over it.", acidicJuicerTowerStats[0]));
        towerStatInfo.Add(TowerType.SLICE_THROWER, new TowerInfo("Slice Thrower", "Lanuches slices of pineapple that can pierce through enemies. Special: Number of enemies the slice can hit before breaking.", sliceThrowerTowerStats[0]));
        towerStatInfo.Add(TowerType.THORN_TOSSER, new TowerInfo("Thorn Tosser", "Rapidly fires thorns in all directions that can pierece through enemies. Special: Number of enemies the slice can hit before breaking.", thornTosserTowerStats[0]));
        towerStatInfo.Add(TowerType.PINEAPPLE_WALL, new TowerInfo("Pineapple Wall", "Just a cheap wall.", pineappleWallTowerStats[0]));
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

    private Tile CreateTile(int i, int j)
    {
        var x = boardBack.transform.position.x - (boardWidth / 2f) + (tileWidth / 2f);
        var y = boardBack.transform.position.y + (boardWidth / 2f) - (tileWidth / 2f);
        var pos = new Vector2(i, j);
        var tile = Instantiate(tilePrefab, new Vector3(x + (tileWidth * i), y - (tileWidth * j)), Quaternion.identity);
        var isOffset = (i % 2 == 0 && j % 2 != 0) || (i % 2 != 0 && j % 2 == 0);
        tile.Init(this, isOffset, boardBack.transform, pos, false);
        tile.name = "Tile " + i + "," + j;
        tiles.Add(pos, tile);
        return tile;
    }

    private void CreateGrid()
    {

        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                var tile = CreateTile(i, j);
                if ((i == 7 || i == 8) && (j == 7 || j == 8))
                {
                    tile.SetSpecialTile(false, false);
                }
            }
        }
    }

    private void CreateHiddenTiles()
    {
        for (int k = 0; k < hiddenTileLocations.GetLength(0); k++)
        {
            var i = hiddenTileLocations[k, 0];
            var j = hiddenTileLocations[k, 1];
            var tile = CreateTile(i, j);
            tile.SetSpecialTile(false, true);
            if (j == 5 || j == 10 || i == 5 || i == 10)
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
        var obj = Instantiate(prefab, new Vector3(x, y, 1), Quaternion.identity);
        obj.name = name;

        return obj;
    }
    private void PlaceTower(Tile tile)
    {
        int cost = towerStatInfo[(TowerType)selectedTowerInShop].towerStats.cost;
        if (playerStats.money < cost)
        {
            return;
        }
        playerStats.money -= cost;
        UpdateStatsText();
        Tower tower;
        switch ((TowerType)selectedTowerInShop)
        {
            case TowerType.PINEAPPLE_CANNON:
                var towerObject = InstantiateWithParent(PineappleCannonTower.towerPath, "PineappleCannon", tile.transform);
                tower = new PineappleCannonTower(this, towerObject, pineappleCannonTowerStats);
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
        tower.totalValue += cost;
        tickableTowers.Add(tower);
        tile.tower = tower;
        tile.isOccupied = true;
        // Inform enemies to update their paths.
        boardHasChanged = true;
    }

    public void InteractTile(Tile tile)
    {
        if (selectedTowerInShop == -1) // If no tower is selected in the shop
        {
            if (tile.isOccupied) // If tile has a tower select it othwerise deselect the current tile.
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
            if (tile.isOccupied) // If tile has a tower select it othwerise place the currently selected tower.
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

    private bool CanPlaceTile(Tile tile)
    {
        if (!tile.isInteractable)
        {
            return false;
        }
        // If any path from the entrance is blocked by placing a tower at this position then return false.
        tile.isOccupied = true;
        var notBlockingEntrance = CheckEntrancePaths();
        tile.isOccupied = false;
        return notBlockingEntrance;
    }

    private bool CheckEntrancePaths()
    {
        // Check each path from the entrance to the goal and return false if the path is blocked.
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

    private void SelectTile(Tile tile)
    {
        if (selectedTowerInShop != -1) // If a tower in the shop is selected deselect that button.
        {
            shopButtons[selectedTowerInShop].SetBool("Active", false);
            selectedTowerInShop = -1;
        }
        DeselectTile();
        tile.SelectTile(true);
        selectedTile = tile;
        tile.tower.ToggleTowerRadiusDisplay(true);
        UpdateTowerStatsCardInfo();
    }

    private void DeselectTile()
    {
        if (selectedTile != null) // If a tile is selected then disable the tower radius display of that tile.
        {
            selectedTile.tower.ToggleTowerRadiusDisplay(false);
            selectedTile.SelectTile(false);
        }
        selectedTile = null;
        UpdateTowerStatsCardInfo();
    }

    // Deselect current tile and shop button.
    public void Deselect()
    {
        if (selectedTowerInShop != -1)
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

    public void UpdateTowerStatsCardInfo()
    {
        if (selectedTowerInShop != -1) // If a shop button is selected then display tower info and cost.
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
        else if (selectedTile != null) // If a tile is selected the display that towers stats and upgrade info.
        {
            towerStatsCard.SetActive(true);
            Tower tower = selectedTile.tower;
            var currentStatus = tower.currentStats;
            var upgradeStatus = tower.upgradeStats;
            var info = towerStatInfo[tower.type];
            towerNameText.text = info.name;
            towerDescriptionText.text = info.description;
            upgradeCostText.text = "" + upgradeStatus.cost;
            sellAmountText.text = "" + Mathf.FloorToInt(0.7f * tower.totalValue);
            costUI.SetActive(false);
            upgradeSellUI.SetActive(true);
            towerLevelText.text = "" + (tower.level + 1);

            upgradeButton.SetActive(true);

            speedCurrentValue.text = "" + currentStatus.speed;
            // fixes rounding errors for doubles
            double deltaSpeed = System.Math.Round((upgradeStatus.speed - currentStatus.speed) * 100) / 100;
            speedUpgradeValue.text = "" + deltaSpeed;

            damageCurrentValue.text = "" + currentStatus.damage;
            damageUpgradeValue.text = "+" + (upgradeStatus.damage - currentStatus.damage);

            rangeCurrentValue.text = "" + currentStatus.range;
            double deltaRange = System.Math.Round((upgradeStatus.range - currentStatus.range) * 100) / 100;
            rangeUpgradeValue.text = "+" + deltaRange;

            if (info.towerStats.hasSpecial) // If the tower has a special stat then display that info.
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
            if (tower.maxUpgrade || tower.upgrading) // If the tower is fully upgraded then do not display upgrade text.
            {
                upgradeButton.SetActive(false);
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
        projectilePool.Add(projectileObject, projectile);
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
            masterPineappleAnimator.ResetTrigger("Hurt");
            masterPineappleAnimator.SetTrigger("Hurt");
            if (playerStats.health <= 0)
            {
                playerStats.health = 0;
                gameState = GameState.LOSE;
                UpdateGameStatus();
            }
        }
        UpdateStatsText();
        enemyPool[enemy].SetActive(false);
        activeEnemies -= 1;
    }

    private void UpdateStatsText()
    {
        healthText.text = "" + playerStats.health;
        moneyText.text = "" + playerStats.money;
        scoreText.text = "SCORE: " + playerStats.score;
    }

    private void SpawnEnemy(EnemyType type, SpawnLocation spawnLocation, int difficulty)
    {
        var spawnPosition = enemySpawn[(int)spawnLocation].position;
        Enemy enemy = GetEnemyFromPool(type, spawnPosition, difficulty);
        var randomOffset = new Vector3(Random.value - 0.5f, Random.value - 0.5f, 0);
        enemy.enemy.transform.position += randomOffset;

        if (type == EnemyType.FLYING) // If enemy is of type flying then it just paths directly to the goal.
        {
            enemy.SetEnemyPath(flyingPath);
            return;
        }

        Stack<Node> path;
        switch (spawnLocation) // Get last generated path from entrance.
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

        enemy.SetEnemyPath(new Stack<Node>(path.Reverse()));
    }

    // Spawns big and small group enemies when a normal group enemy is broken apart.
    // TODO: add iframes when groups spawn to prevent taking damage from existing projectiles that have not been destroyed yet.
    public void SpawnGroup(EnemyType type, Vector3 spawnPosition, Quaternion spawnRotation, int difficulty, Stack<Node> path)
    {
        Enemy enemy = GetEnemyFromPool(type, spawnPosition, difficulty);
        enemy.enemy.transform.rotation = spawnRotation;
        enemy.SetEnemyPath(new Stack<Node>(path.Reverse()));
    }

    private Enemy GetEnemyFromPool(EnemyType type, Vector3 spawnPosition, int difficulty)
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
        int waveDifficulty = Mathf.FloorToInt(waveLevel / 7); // after every boss the difficulty increases
        int enemyAmount = 10 + (waveDifficulty);
        if (type == EnemyType.SWARM) // If enemy of type swarm then generate a duplicate wave on the opposite side of the map and double the default enemy count.
        {
            enemyAmount *= 2;
            var secondSpawnLocation = (SpawnLocation)((waveLevel + 2) % 4);
            Wave wave2 = new Wave(type, secondSpawnLocation, enemyAmount, waveDifficulty);
            activeWaves.Add(wave2);
        }
        Wave wave = new Wave(type, spawnLocation, enemyAmount, waveDifficulty);
        activeWaves.Add(wave);

        if (sentWaves > 0) // If there are still waves that have been requested then decrease the sent waves count.
        {
            sentWaves -= 1;
        }
        if (sentWaves == 0)
        {
            waveBoxSpeed = NORMAL_WAVE_SPEED;
        }
    }
    public void NewWaveBox()
    {
        if (level == MAX_WAVES)
        {
            return;
        }
        if (activeWaveBoxes == waveBoxPool.Count)
        {
            var waveBoxObj = InstantiateWithParent(waveBoxPath, "WaveBox", waveBoxSpawn);
            var waveBox = waveBoxObj.GetComponent<WaveBox>();
            waveBox.Init(this, waveOrder[(int)(level % 7)], level);
            waveBoxPool.Add(waveBoxObj, waveBox);
            activeWaveBoxes += 1;
            level += 1;
        }
        else
        {
            foreach (KeyValuePair<GameObject, WaveBox> entry in waveBoxPool)
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

    private void ActivateWave()
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
        if (selectedTile == null)
        {
            return;
        }
        var money = Mathf.FloorToInt(0.7f * selectedTile.tower.totalValue);
        playerStats.money += money;
        selectedTile.ClearTile();
        selectedTile.SelectTile(false);
        selectedTile = null;
        UpdateStatsText();
        UpdateTowerStatsCardInfo();
        boardHasChanged = true;
    }

    public void UpgradeTower()
    {
        if (selectedTile == null)
        {
            return;
        }
        var cost = selectedTile.tower.GetUpgradeStats().cost;
        if (playerStats.money < cost)
        {
            return;
        }
        if (selectedTile.tower.Upgrade())
        {
            playerStats.money -= cost;
            UpdateStatsText();
            UpdateTowerStatsCardInfo();
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


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Menu(!isPaused);
        }
        // Cheat Codes

        if (ENABLE_CHEAT_CODE && Input.GetKeyDown(KeyCode.C))
        {
            playerStats = new PlayerStats(999, 99999, 0);
        }

        if (gameState == GameState.PLAYING && level == MAX_WAVES && activeWaves.Count == 0 && activeEnemies == 0)
        {
            gameState = GameState.WIN;
            UpdateGameStatus();
        }
        if (!isPaused)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                isTowerLevelVisible = !isTowerLevelVisible;
                foreach (Tower tower in tickableTowers)
                {
                    tower.ToggleTowerLevel(isTowerLevelVisible);
                }
            }
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
        foreach (KeyValuePair<GameObject, WaveBox> entry in waveBoxPool)
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
