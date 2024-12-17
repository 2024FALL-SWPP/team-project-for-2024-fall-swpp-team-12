using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;

public class LevelManager : MonoBehaviour
{
    public static LevelManager levelManager;
    public GameObject levelBarrierPrefab;
    public string[] levelScenes;
    public bool includeTestField = false;
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
    private string baseSceneName;

    private void Awake()
    {
        if (levelManager == null) { levelManager = this; }

        baseSceneName = includeTestField ? "TestBaseScene" : "LevelBaseScene";

        if (includeTestField)
        {
            levelScenes = new string[]
            {
                "TestField"
            };
        }
        else
        {
            levelScenes = new string[]
            {
                "Prologue",
                "L-001-1",
                "L-001-2",
                "L-001-3",
                "L-001-4",
                "L-001-5",
                "L-002-1",
                "L-002-2",
                "L-002-3",
                "L-002-4",
                "L-003-1",
                "L-003-2",
                "L-003-3",
                "L-003-4",
                "Event",
                "L-004-1",
                "L-004-2",
                "L-004-3",
                "Epilogue"
            };
        }
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;
        if (levelBarrierPrefab != null)
        {
            levelBarrier = Instantiate(levelBarrierPrefab);
            if (includeTestField && levelBarrier != null)
            {
                Destroy(levelBarrier);
                Debug.Log("Level barrier removed because TestField is included.");
            }
        }

        //if (PlayerPrefs.HasKey("SavedLevelIndex")) currentLevelIndex = PlayerPrefs.GetInt("SavedLevelIndex");
        //else currentLevelIndex = 0;
        currentLevelIndex = 0; //need for test.

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != baseSceneName) SceneManager.LoadScene(baseSceneName);

        LoadLevel(currentLevelIndex);
        SceneManager.sceneLoaded += OnSceneLoaded; // sceneLoaded = a event called when a scene is loaded
        InputManager.inputManager.OnReset += ResetLevel;
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
            SoundManager.soundManager.PlaySound2D("ui_levelClear", 0.25f);
            StartCoroutine(FinishLevel());
        }
    }

    private IEnumerator FinishLevel()
    {
        Destroy(GameObject.Find("CameraOffsetAnchor"));

        if (currentLevelIndex > 0) previousLevelIndex = currentLevelIndex - 1;
        currentLevelIndex++;
        PlayerPrefs.SetInt("SavedLevelIndex", currentLevelIndex);
        PlayerPrefs.Save();
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


            // Dynamic tile detection for tutorialTile, lever and box
            GameObject tutorialTile = GameObject.FindWithTag("TutorialTile");
            GameObject lever = GameObject.FindWithTag("Lever");
            GameObject box = GameObject.FindWithTag("Box");

            TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
            if (tutorialManager != null)
            {
                tutorialManager.ShowTutorialForLevel(levelScenes[currentLevelIndex]);

                if (tutorialTile != null)
                {
                    tutorialManager.SetTargetObject(tutorialTile.transform);
                }
                else if (lever != null)
                {
                    tutorialManager.SetTargetObject(lever.transform);
                }
                else if (box != null)
                {
                    tutorialManager.SetTargetObject(box.transform);
                }
                else
                {
                    tutorialManager.ClearTargetObject();
                }
            }

            if (ScenarioManager.scenarioManager != null)
            {
                ScenarioManager.scenarioManager.StartMonologue(currentLevelIndex);
            }

            TurnManager.turnManager.InitializeObjectLists();
            ResetLevel();

            if (currentLevelIndex == 0)
            {
                InputManager.inputManager.isPlayingScript = true; //input lock
                StartCoroutine("PlayPrologueScript");
            }
            else if (currentLevelIndex == 14)
            {
                StartCoroutine("PlayEventScript");
            }
        }
    }
    IEnumerator PlayPrologueScript()
    {
        while (ScenarioManager.scenarioManager.isLockedToRead) yield return null; //wait until monologue read ends.
        //phantom with command order.
        PhantomController.phantomController.transform.position = new Vector3(5,17,21);
        PhantomController.phantomController.transform.rotation = Quaternion.Euler(0,180,0);
        PhantomController.phantomController.playerCurPos = PhantomController.phantomController.transform.position;
        PhantomController.phantomController.playerCurRot = PhantomController.phantomController.transform.rotation;
        PhantomController.phantomController.gameObject.SetActive(true);
        PhantomController.phantomController.isPhantomExisting = true;
        PhantomController.phantomController.positionIterator.SetCurrent((PhantomController.phantomController.playerCurPos, PhantomController.phantomController.playerCurRot, true));
        PhantomController.phantomController.commandIterator.Clear();
        PhantomController.phantomController.commandIterator.Add("");
        PhantomController.phantomController.commandIterator.Add("r");
        PhantomController.phantomController.commandIterator.Add("s");
        PhantomController.phantomController.commandIterator.Add("s");
        PhantomController.phantomController.commandIterator.Add("s");
        PhantomController.phantomController.commandIterator.Add("s");
        PhantomController.phantomController.commandIterator.Add("r");
        PhantomController.phantomController.commandIterator.Add("w");
        PhantomController.phantomController.commandIterator.ResetToStart();
        //player r spamming automatically.
        yield return new WaitForSeconds(1.5f);
        InputManager.inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1.5f);
        InputManager.inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1.0f);
        InputManager.inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1.0f);
        InputManager.inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1.5f);
        InputManager.inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(1.5f);
        InputManager.inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(3.0f);
        InputManager.inputManager.OnMovementControl?.Invoke("r");
        yield return new WaitForSeconds(2.5f);
        GameObject.Find("psuedoTile").gameObject.SetActive(false);
        InputManager.inputManager.OnMovementControl?.Invoke("r");

        InputManager.inputManager.isPlayingScript = false;
    }
    IEnumerator PlayEventScript()
    {
        while (IsPlayerAtNearGoal() && !TurnManager.turnManager.CLOCK && !PlayerController.playerController.isTimeRewinding) yield return null;
        InputManager.inputManager.isPlayingScript = true; //input lock

        GameObject.Find("psuedoTile").gameObject.SetActive(false);
        InputManager.inputManager.isPlayingScript = false;
    }
    private bool IsPlayerAtNearGoal()
    {
        return Mathf.Approximately(player.transform.position.x, currentGoal.position.x) &&
            Mathf.Approximately(player.transform.position.y, currentGoal.position.y + playerTileDiff) &&
            Mathf.Approximately(player.transform.position.z, currentGoal.position.z - 2.0f); //right behind 1 grid
    }

    private void ResetLevel()
    {
        // Place player at the start point
        player.transform.position = new Vector3(
            currentStart.position.x,
            currentStart.position.y + playerTileDiff,
            currentStart.position.z
        );
        player.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        // Reset player log
        player.InitializeLog();
        TurnManager.turnManager.LeaveTimeRewind();
        // Reset level
        TurnManager.turnManager.ResetObjects(); // <- Phantom, Player too
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