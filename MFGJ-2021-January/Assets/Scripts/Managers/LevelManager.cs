﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour, ISaveable
{
    #region Variables
    //the game manager class will manage the scene changes, pause, restart and etc, and will be the intermediate between UI and the player.
    public static LevelManager levelManager;
    GameManager gameManager;

    private bool isGameOver;
    public bool IsGameOver { get => isGameOver; }

    [SerializeField]
    PlayerController player = null;
    Gunning gunning;

    public GameObject PlayerPrefab;
    public GameObject PauseCanvas;
    public GameObject ContinueCanvas;
    public GameObject MissionFailedCanvas;
    public GameObject bossLvl1;
    [SerializeField] GameObject MapCanvas;
    public Transform Checkpoint;

    public GameObject rocketsUI;
    public GameObject javelinUI;
    public GameObject scoreUI;
    public GameObject livesUI;
    public GameObject ammoUI;

    public int score = 0;
    private bool hScore1 = false;
    private bool hScore2 = false;
    private bool hScore3 = false;

    HintsManager hintsManager;
    AudioManager audioManager;
    [HideInInspector]
    public int lastLives;
    [HideInInspector]
    public int lastJavelinAmmo;
    [HideInInspector]
    public int lastRocketsAmmo;
    [HideInInspector]
    public string lastSelectedSpecial;

    [Header("Save and Load Data")]
    //Enemies
    Enemy enemy;
    public int e_idSetter;
    public List<Enemy> _enemies = new List<Enemy>();
    public List<int> _destroyedEnemies = new List<int>();
    //Items
    public int r_idSetter;
    public List<Healing> _recollectable = new List<Healing>();
    public List<int> _grabbedRecollectables = new List<int>();

    #endregion

    #region MonoBehaviour Methods
    private void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        gunning = FindObjectOfType<Gunning>();
        enemy = FindObjectOfType<Enemy>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        hintsManager = FindObjectOfType<HintsManager>();
        gameManager = FindObjectOfType<GameManager>();

        score = 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        isGameOver = false;
        Time.timeScale = 1;
        lastLives = player.lives;
    }

    private void FixedUpdate()
    {
        if (player.healthPoints <= 0)
        {
            MapCanvas.SetActive(false);
            if (player.lives > 0)
            {
                ContinueCanvas.GetComponentInChildren<UnityEngine.UI.Text>().text = lastLives.ToString();
                ContinueCanvas.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                GameOver();
            }
        }
        scoreUI.GetComponentInChildren<UnityEngine.UI.Text>().text = score.ToString();
        livesUI.GetComponentInChildren<UnityEngine.UI.Text>().text = player.lives.ToString();
        ammoUI.GetComponentInChildren<UnityEngine.UI.Text>().text = player.gunning.initial_Ammo.ToString();
        if (ammoUI.GetComponentInChildren<UnityEngine.UI.Text>().text == "0")
        {
            ammoUI.GetComponentInChildren<UnityEngine.UI.Text>().text = "- - -";
        }
    }

    private void Update()
    {
        TogglePause();
        CheckScore();
        ToggleMap();
    }
    #endregion

    #region Other Methods
    private void CheckScore()
    {
        if (score >= 4000 && hScore1 == false) //placeholder ammount to gain 1up
        {
            hintsManager.ShowHintPanel("score", 3);

            player.lives++;
            hScore1 = true;
            //Play 1up SFX
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
        }
        else if (score >= 12500 && hScore2 == false)
        {
            hintsManager.ShowHintPanel("score", 3);

            player.lives++;
            hScore2 = true;
            //Play 1up SFX
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
        }
        else if (score >= 22000 && hScore3 == false)
        {
            hintsManager.ShowHintPanel("score", 3);

            player.lives++;
            hScore3 = true;
            //Play 1up SFX
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder.
            audioManager.PlaySound("PickUpWeapon");//placeholder. More sounds at the same time makes it hear stronger. its only a placeholder.
        }
    }
    private void TogglePause()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && !MapCanvas.activeSelf)
        {
            if (Time.timeScale == 1)
            {
                Pause();
            }
            else Resume();
        }
    }
    private void EndOfProtectedTime()
    {
        PlayerPrefab.tag = "Player";
    }
    private void ToggleMap()
    {
        if (Input.GetKeyDown(KeyCode.M) && !PauseCanvas.activeSelf)
        {
            MapCanvas.SetActive(!MapCanvas.activeSelf);
            if (Time.timeScale != 0)
            {
                Time.timeScale = 0;
            }
            else Time.timeScale = 1;
        }
    }
    #endregion

    #region Game States
    IEnumerator LoadAsyncScene(string scene_name)//from unity docs
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene_name);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    public void Pause()
    {
        PauseCanvas.GetComponentInChildren<UnityEngine.UI.Text>().text = lastLives.ToString();
        PauseCanvas.SetActive(true);
        Time.timeScale = 0;
    }
    public void Resume()
    {
        PauseCanvas.SetActive(false);
        Time.timeScale = 1;
    }
    public void GameOver()
    {
        //Game Over State
        gameManager.GameOver();

        //Delete Data
        string fileName = gameManager.dataFileName;
        var fullPath = Path.Combine(Application.persistentDataPath, fileName);
        File.Delete(fullPath);

        isGameOver = true;
        MissionFailedCanvas.SetActive(true);
        Time.timeScale = 0;
    }
    public void Restart()
    {
        isGameOver = false;
        Time.timeScale = 1;
        MissionFailedCanvas.SetActive(false);

        //Start Game State
        gameManager.StartGame();
        audioManager.MusicChangerLevels("Level One");
    }
    public void ToMainMenu()
    {
        GameOver();
        SceneManager.LoadScene("Main_Menu");
    }
    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    void OnApplicationQuit()
    {
        //Save Data
        DataManager.SaveJsonData(FindObjectOfType<DataManager>());
        Debug.Log("Level Saved");

        //Encrypt Data if you quit the game without save. (for multiple saves during the game)
        if (!gameManager.isNewGame && DataManager.timesSaved < 1) 
        {
            string path = Path.Combine(Application.persistentDataPath, gameManager.dataFileName);
            string json = File.ReadAllText(path);
            FileManager.EncryptOnQuit(gameManager.dataFileName, out json);
        }

        Debug.Log("Application ends. Encrypting Data...");

    }
    public void Continue()
    {
        ContinueCanvas.SetActive(false);

        //Update Transform
        PlayerPrefab.transform.position = Checkpoint.position;
        PlayerPrefab.transform.rotation = Checkpoint.rotation;
        PlayerPrefab.tag = "Untagged";
        PlayerPrefab.SetActive(true);

        //Update Values
        var p = PlayerPrefab.GetComponent<PlayerController>();
        p.lives = lastLives;
        p.healthPoints = p.maxHealthPoints; //Reset health points 

        p.gunning.rocketsAmmo = lastRocketsAmmo;
        p.gunning.javelinAmmo = lastJavelinAmmo;
        p.gunning.selectedSpecial = lastSelectedSpecial;

        Time.timeScale = 1;

        if (bossLvl1.activeSelf == false)
        {
            audioManager.MusicChangerLevels("Level One");
        }
        else
        {
            audioManager.MusicChangerLevels("BossFight");
        }


        //To keep enemies from damaging us when we spawn again.
        Invoke(nameof(EndOfProtectedTime), 2.0f);
    }
    #endregion

    #region Saving and Loading Data
    //Save
    public void PopulateSaveData(SaveData a_SaveData)
    {
        //Player Data
        SaveData.ScoreData scoreData = new SaveData.ScoreData();
        scoreData.s_score = score;
        scoreData.s_hgScore1 = hScore1;
        scoreData.s_hgScore2 = hScore2;
        scoreData.s_hgScore3 = hScore3;
        a_SaveData.m_ScoreData = scoreData;
    }

    //Load
    public void LoadFromSaveData(SaveData a_SaveData)
    {
        //Player Data        
        score = a_SaveData.m_ScoreData.s_score;
        hScore1 = a_SaveData.m_ScoreData.s_hgScore1;
        hScore2 = a_SaveData.m_ScoreData.s_hgScore2;
        hScore3 = a_SaveData.m_ScoreData.s_hgScore3;
    }
    #endregion
}
