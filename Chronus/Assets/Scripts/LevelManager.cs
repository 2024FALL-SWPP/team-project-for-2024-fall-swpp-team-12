using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    // There may be an error saying:
    // There are 2 audio listeners in the scene. Please ensure there is always exactly one audio listener in the scene. 
    // This can be handled, but doesn't effect the game, so I'll just pass.
    public Transform goalTile;   
    public string nextLevelSceneName;
    private PlayerController player;   
    private float playerTileDiff = 1.2f;
    // 1.2f because right now, if the player is at (1, 1, 1),
    // then the tile the player is standing on is (1, -0.2, 1)
    // this may be a bad design pattern, so need to be reviewed.
    private bool isLevelCompleted = false;
    
    private void Start()
    {
        player = FindObjectOfType<PlayerController>();  
    }

    private void Update()
    {
        // I think this clear logic should not be "updated" per frame.
        // clear logic should be called once after the player movement, to check if the player is at the goal tile. 
        // -> To avoid this from being called multiple times, I introduced "isLevelCompleted" bool (temporary)
        // This "must be" fixed when integrating with refactored PlayerController!!
        if (player != null && IsPlayerAtGoal() && !isLevelCompleted) 
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
        StartCoroutine(LoadNextLevel());
    }

    private IEnumerator LoadNextLevel()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextLevelSceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.UnloadSceneAsync(currentScene);
    }
}