using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public Transform goalTile;   
    public string[] levelScenes;
    private int currentLevelIndex = 0;
    private PlayerController player;   
    private float playerTileDiff = 1.2f;
    // 1.2f because right now, if the player is at (1, 1, 1),
    // then the tile the player is standing on is (1, -0.2, 1)
    // this may be a bad design pattern, so need to be reviewed.
    private bool isLevelCompleted = false;
    
    private void Start()
    {
        player = FindObjectOfType<PlayerController>();  
        LoadLevel(currentLevelIndex);
        SceneManager.sceneLoaded += OnSceneLoaded;
        // sceneLoaded = a event called when a scene is loaded
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // I think this clear logic should not be "updated" per frame.
        // clear logic should be called once after the player movement, to check if the player is at the goal tile. 
        // -> To avoid this from being called multiple times, I introduced "isLevelCompleted" bool (temporary)
        // This "must be" fixed when integrating with refactored PlayerController!!
        if (player != null && goalTile != null && IsPlayerAtGoal() && !isLevelCompleted) 
        {
            LevelComplete();
        }
    }

    private bool IsPlayerAtGoal()
    {
        return Mathf.Approximately(player.transform.position.x, goalTile.position.x) &&
            Mathf.Approximately(player.transform.position.y, goalTile.position.y + playerTileDiff) && 
            Mathf.Approximately(player.transform.position.z, goalTile.position.z);
    }

    private void LevelComplete()
    {
        isLevelCompleted = true;
        Debug.Log("Level Clear!");
        StartCoroutine(FinishLevel());
    }

    private IEnumerator FinishLevel() 
    {
        string currentLevelScene = levelScenes[currentLevelIndex];
        yield return SceneManager.UnloadSceneAsync(currentLevelScene);

        currentLevelIndex++;
        LoadLevel(currentLevelIndex);
    }

    private void LoadLevel(int index)
    {
        if (index < levelScenes.Length)
        {
            SceneManager.LoadSceneAsync(levelScenes[index], LoadSceneMode.Additive);
            isLevelCompleted = false;  
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive)
        {
            // automatically finds GoalTile in every level
            GameObject goalObject = GameObject.FindWithTag("GoalTile"); 
            if (goalObject != null)
            {
                goalTile = goalObject.transform;
            }
        }
    }
}