using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public GameObject levelBarrierPrefab;
    public string[] levelScenes;
    private int currentLevelIndex = 0;
    private int previousLevelIndex = -1; // new variable to track the last level to unload
    private PlayerController player;
    private Transform currentGoal;
    private Transform currentStart;
    private Camera mainCamera;
    private Vector3 levelOffset = Vector3.zero;
    private Vector3 cameraOffset = Vector3.zero;
    private float playerTileDiff = 1.2f;
    // 1.2f because right now, if the player is at (1, 1, 1),
    // then the tile the player is standing on is (1, -0.2, 1)
    // this may be a bad design pattern, so need to be reviewed.
    private bool isLevelCompleted = false;
    private GameObject levelBarrier;


    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;
        if (levelBarrierPrefab != null)
        {
            levelBarrier = Instantiate(levelBarrierPrefab);
        }

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
        // This should be fixed when integrating with refactored PlayerController!
        if (player != null && currentGoal != null && IsPlayerAtGoal() && !isLevelCompleted)
        {
            LevelComplete();
        }
    }

    private bool IsPlayerAtGoal()
    {
        return Mathf.Approximately(player.transform.position.x, currentGoal.position.x) &&
            Mathf.Approximately(player.transform.position.y, currentGoal.position.y + playerTileDiff) &&
            Mathf.Approximately(player.transform.position.z, currentGoal.position.z);
    }

    private void LevelComplete()
    {
        isLevelCompleted = true;
        Debug.Log("Level Clear!");
        StartCoroutine(FinishLevel());
    }

    private IEnumerator FinishLevel()
    {
        // Keep track of the previous level to unload later
        if (currentLevelIndex > 0)
        {
            previousLevelIndex = currentLevelIndex - 1;
        }

        // Load the next level and move the camera
       currentLevelIndex++;
        LoadLevel(currentLevelIndex);

        // Wait until the next level is fully loaded
        yield return new WaitUntil(() => SceneManager.GetSceneByName(levelScenes[currentLevelIndex]).isLoaded);

        // // Now unload the previous level if there is one to unload
        // if (previousLevelIndex >= 0)
        // {
        //     yield return SceneManager.UnloadSceneAsync(levelScenes[previousLevelIndex]);
        //     previousLevelIndex = -1; // Reset previousLevelIndex after unloading
        // }
        isLevelCompleted = false;
    }

    private void LoadLevel(int index)
    {
        if (index < levelScenes.Length)
        {
            SceneManager.LoadSceneAsync(levelScenes[index], LoadSceneMode.Additive);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive)
        {
            // automatically finds currentStart and currentGoal in every level,
            // then calculate offset to match previous goal = current start
            // and move all objects(including player) accordingly to that offset
            GameObject nextGoal = GameObject.FindWithTag("GoalTile");
            GameObject nextStart = GameObject.FindWithTag("StartTile");
            if (nextGoal != null && nextStart != null)
            {   
                Vector3 previousLevelOffset = levelOffset;
                if (currentGoal != null) // passing the first level
                {
                    Vector3 nextStartPosition = nextStart.transform.position;
                    levelOffset = currentGoal.position - nextStartPosition;
                }
                currentGoal = nextGoal.transform;
                currentStart = nextStart.transform;
                nextGoal.tag = "Untagged"; // to prevent multiple "GoalTile"s in one scene
                nextStart.tag = "Untagged";
                ApplyOffsetToScene(scene);
                
                if (levelBarrier != null)
                {
                    levelBarrier.transform.position = currentStart.position - new Vector3(0, 0, 1);
                }

                // for camera, offset is "accumulated", so need to be controlled like this.
                StartCoroutine(MoveCameraWithTransition(levelOffset - previousLevelOffset)); 
            }
        }
    }

    private void ApplyOffsetToScene(Scene scene)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            obj.transform.position += levelOffset;
        }

        player.transform.position = new Vector3(
            currentStart.position.x,
            currentStart.position.y + playerTileDiff,
            currentStart.position.z
        );
    }

    private IEnumerator MoveCameraWithTransition(Vector3 offset)
    {
        Vector3 targetPosition = mainCamera.transform.position + offset;
        
        // Ease-in
        Vector3 velocity = Vector3.zero;  
        while (Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.1f)
        {
            mainCamera.transform.position = Vector3.SmoothDamp(
                mainCamera.transform.position,
                targetPosition,
                ref velocity,
                0.3f  
            );
            yield return null;
        }

        // Linear
        /*while (Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPosition,
                transitionSpeed * Time.deltaTime
            );
            yield return null;
        }*/

        mainCamera.transform.position = targetPosition; 

        // Now unload the previous level if there is one to unload
        if (previousLevelIndex >= 0)
        {
            yield return SceneManager.UnloadSceneAsync(levelScenes[previousLevelIndex]);
            previousLevelIndex = -1; // Reset previousLevelIndex after unloading
        }
    }
}