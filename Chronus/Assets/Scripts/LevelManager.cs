using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager levelManager;
    public GameObject levelBarrierPrefab;
    public string[] levelScenes;
    private int currentLevelIndex;
    private int previousLevelIndex = -1;
    private PlayerController player;
    private Transform currentGoal;
    private Transform currentStart;
    private Camera mainCamera;
    private Vector3 levelOffset = Vector3.zero;
    private GameObject levelBarrier;
    private readonly float playerTileDiff = 1.2f;
    // 1.2f: because right now, if the player is at (1, 1, 1),
    // then the tile the player is standing on is (1, -0.2, 1)
    private readonly string baseSceneName = "LevelBaseScene";

    private void Awake()
    {
        if (levelManager == null) { levelManager = this; }
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;
        if (levelBarrierPrefab != null) levelBarrier = Instantiate(levelBarrierPrefab);

        if (PlayerPrefs.HasKey("SavedLevelIndex")) currentLevelIndex = PlayerPrefs.GetInt("SavedLevelIndex");
        else currentLevelIndex = 0;

        // I don't know how it's going to work in real application
        // but in test environment: this will work for now.
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != baseSceneName) SceneManager.LoadScene(baseSceneName);

        LoadLevel(currentLevelIndex);
        SceneManager.sceneLoaded += OnSceneLoaded; // sceneLoaded = a event called when a scene is loaded

        InputManager.inputManager.OnReset += ResetLevel;
    }

    private void OnApplicationQuit()
    // Save is done automatically when quitting the game; Load is done at the Start()
    {
        PlayerPrefs.SetInt("SavedLevelIndex", currentLevelIndex);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private bool IsPlayerAtGoal()
    {
        return Mathf.Approximately(player.transform.position.x, currentGoal.position.x) &&
            Mathf.Approximately(player.transform.position.y, currentGoal.position.y + playerTileDiff) &&
            Mathf.Approximately(player.transform.position.z, currentGoal.position.z);
    }

    public void CheckAndCompleteLevel()
    // This is called at TurnManager.EndTurn()
    {
        if (player != null && currentGoal != null && IsPlayerAtGoal())
        {
            Debug.Log("Level Clear!");
            StartCoroutine(FinishLevel());
        }
    }

    private IEnumerator FinishLevel()
    {
        Destroy(GameObject.Find("CameraOffsetAnchor"));

        // Keep track of the previous level to unload later
        if (currentLevelIndex > 0) previousLevelIndex = currentLevelIndex - 1;

        // Load the next level and move the camera
        currentLevelIndex++;
        LoadLevel(currentLevelIndex);

        // Wait until the next level is fully loaded
        yield return new WaitUntil(() => SceneManager.GetSceneByName(levelScenes[currentLevelIndex]).isLoaded);
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

                // Apply offset to the scene.
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject obj in rootObjects) obj.transform.position += levelOffset;

                // Add a transparent barrier to prevent going to the previous level
                if (levelBarrier != null) levelBarrier.transform.position = currentStart.position - new Vector3(0, 0, 1);

                StartCoroutine(MoveCameraWithTransition());
            }

            ResetLevel();
        }
    }

    private void ResetLevel()
    {
        // Place player at the start point
        player.transform.position = new Vector3(
            currentStart.position.x,
            currentStart.position.y + playerTileDiff,
            currentStart.position.z
        );
        // Reset player log
        player.InitializeLog();
        player.isTimeRewinding = false;
        // Get information from the level
        TurnManager.turnManager.InitializeObjectLists();
        // Kill the phantom
        PhantomController.phantomController.InitializeLog();
        PhantomController.phantomController.isPhantomExisting = false;
        PhantomController.phantomController.gameObject.SetActive(false);
    }

    private IEnumerator MoveCameraWithTransition()
    {
        // Assure that every scene has CameraOffsetAnchor (skipping null checking)
        GameObject anchor = GameObject.Find("CameraOffsetAnchor");
        Vector3 targetPosition = anchor.transform.position;
        Quaternion targetRotation = anchor.transform.rotation;

        // Ease-in transition
        Vector3 velocity = Vector3.zero;
        while (Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.1f ||
            Quaternion.Angle(mainCamera.transform.rotation, targetRotation) > 0.1f)
        {
            mainCamera.transform.position = Vector3.SmoothDamp(
                mainCamera.transform.position,
                targetPosition,
                ref velocity,
                0.3f
            );

            mainCamera.transform.rotation = Quaternion.Lerp(
                mainCamera.transform.rotation,
                targetRotation,
                Time.deltaTime / 0.3f
            );
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

        // After the transition, unload the previous level if there is one to unload
        if (previousLevelIndex >= 0)
        {
            Scene sceneToUnload = SceneManager.GetSceneByName(levelScenes[previousLevelIndex]);
            if (sceneToUnload.isLoaded) yield return SceneManager.UnloadSceneAsync(sceneToUnload);
            previousLevelIndex = -1; // Reset previousLevelIndex after unloading
        }
    }
}