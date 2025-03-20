using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// Mostly just holds data. Does what people tell it to
public class LevelsManager : MonoBehaviour
{
    private readonly static Dictionary<int, string> combatAreas = new Dictionary<int, string>() { { 1, "Level1JustCockpit" } };
    
    //Singleton functionality
    private static LevelsManager instance;
    public static LevelsManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new GameObject().AddComponent<LevelsManager>();
            }
            return instance;
        }
    }
    [SerializeField] private bool overrideOther = false;


    // Set in editor
    public int currentLevel; // Here for debugging ;)
    public GameObject gameVictoryUI;

    private GameManager gameManager;
    private ShipBuilderManager builder;
    private bool wonLevel = false;
    public bool inCombat = false;

    public class LevelData
    {
        public readonly int shipWidth;
        public readonly int shipHeight;
        public readonly int[] cockpits;
        public readonly int[] thrusters;
        public readonly int[] armors;
        public readonly int[] machineGuns;
        public readonly int[] cannons;
        public readonly int[] shields;
        public readonly int[] missiles;

        public LevelData(int shipWidth, int shipHeight, int[] cockpits, int[] thrusters, int[] armors, int[] machineGuns, int[] cannons, int[] shields, int[] missiles) {
            this.shipWidth = shipWidth;
            this.shipHeight = shipHeight;
            this.cockpits = cockpits;
            this.thrusters = thrusters;
            this.armors = armors;
            this.machineGuns = machineGuns;
            this.cannons = cannons;
            this.shields = shields;
            this.missiles = missiles;
        }
    }

    // Start is called before the first frame update
    void Start() {
        builder = FindObjectOfType<ShipBuilderManager>();
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update() {
        if (inCombat) {
            if (HumanShipInput.Active == null) {
                Debug.Log("Detected loss on level " + currentLevel);
                inCombat = false;
                //ActivateLevelDoneScreen("Your failure was expected. Grab some scraps and return to base.");
            } else if (EnemyShip.ActiveEnemies.Count == 0) {
                Debug.Log("Detected win on level " + currentLevel);
                currentLevel++;
                wonLevel = true;
                if (currentLevel == transform.childCount) {
                    gameVictoryUI.SetActive(true);

                } else {
                    SceneManager.LoadScene("VictoryScene");
                    inCombat = false;
                }
            }
        }
    }

    public void StartNextLevelBuilder() {
        HumanShipInput player = FindObjectOfType<HumanShipInput>();
        if (player != null) {
            Destroy(player.gameObject);
        }
        foreach (Projectile proj in FindObjectsOfType<Projectile>()) {
            Destroy(proj.gameObject);
        }
        gameManager.shipBuilderPanel.SetActive(true); // HACK!
        builder.Initialize(transform.GetChild(currentLevel).GetComponent<LevelSetup>().ToLevelData(), !wonLevel);
        wonLevel = false;
        inCombat = false;
    }

    public void StartNextLevelCombat() {
        SceneManager.LoadScene(combatAreas[currentLevel]);
    }

    public void OnGiveUpButton() {
        //if (!levelVictoryUI.activeSelf) {
        //    ActivateLevelDoneScreen("Your lack of courage is unwavering. Return to base for motivational disciplinary action.");
        //}
    }

    public void OnContinueButton() {
        StartNextLevelBuilder();
    }

    //Ensure only one LevelsManager exists and that instance sets itself to this object if it should
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if(instance == null)
        {
            instance = this;
        }
        else if(overrideOther)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
