using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;

    // Player Data
    public int maxHealth;
    public PlayerData playerData;
    public GameObject player;
    public int healingCost = 10;

    // Level Data
    public bool respawning;
    public float respawnDelay = 0.5f;
    public LevelData currentLevel;
    public GameObject levelInterfaceCanvas;
    private LevelInterfaceController levelInterfaceController;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// Game Cycle Manipulation
    public void RestartGame()
    {
        Cursor.visible = false;
        Time.timeScale = 1;
        LoadLevel(1);
    }

    public void FinishGame(bool victory)
    {
        // Scene interfaceScene = SceneManager.GetSceneByName("Interface");
        // SceneManager.SetActiveScene(interfaceScene);
        // PlayerInterfaceController interfaceController = FindFirstObjectByType<PlayerInterfaceController>();
        // interfaceController.ShowFinishGamePanel(victory);
        Cursor.visible = true;
        // Time.timeScale = 0;
        Debug.Log(victory ? "Victory!" : "Game Over");
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    /// Level Manipulation 
    IEnumerator LoadAsyncLevel(string sceneName)
    {
        // Load level scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
                asyncLoad.allowSceneActivation = true;
            yield return null;
        }
        this.player = GameObject.FindGameObjectWithTag("Player");
        this.currentLevel = new LevelData(this.player.GetComponent<CharacterController>());
        this.playerData = new PlayerData(this.maxHealth);
        this.currentLevel.InitializeCheckPoints(GameObject.FindGameObjectsWithTag("CheckPoint"));

        // Load interface scene
        asyncLoad = SceneManager.LoadSceneAsync("Interface", LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
                asyncLoad.allowSceneActivation = true;
            yield return null;
        }
        this.levelInterfaceCanvas = GameObject.FindGameObjectWithTag("LevelInterfaceCanvas");
        this.levelInterfaceController = this.levelInterfaceCanvas.GetComponent<LevelInterfaceController>();
    }

    public void LoadLevel(int levelNumber)
    {
        switch (levelNumber)
        {
            case 1:
                StartCoroutine(LoadAsyncLevel("TestArea"));
                break;
            default:
                StartCoroutine(LoadAsyncLevel("TestArea"));
                break;
        }
    }

    public void Respawn()
    {
        if (!respawning)
        {
            respawning = true;
            StartCoroutine(RespawnCoroutine());
        }
    }

    public IEnumerator RespawnCoroutine()
    {
        // Disable player
        currentLevel.GetPlayer().gameObject.SetActive(false);
        this.levelInterfaceController.FadeOut();
        yield return new WaitForSeconds(respawnDelay);

        // Move player to last checkpoint
        currentLevel.GetPlayer().transform.position = currentLevel.GetLastCheckPoint().transform.position;

        // Heal player
        int coins = playerData.GetCoinsQuantity();
        int healedHealth = Math.Min(coins / this.healingCost, playerData.GetMaxHealthValue());
        if (healedHealth == 0)
        {
            // Check game end
            FinishGame(false);
            yield break;
        }
        int spentCoins = healedHealth * this.healingCost;
        playerData.IncreaseHealth(healedHealth);
        playerData.IncreaseCoins(-spentCoins);

        currentLevel.GetPlayer().gameObject.SetActive(true);
        this.levelInterfaceController.FadeIn();
        respawning = false;
    }

}