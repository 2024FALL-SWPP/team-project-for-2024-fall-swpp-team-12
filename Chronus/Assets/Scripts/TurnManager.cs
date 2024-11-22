using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class TurnManager : MonoBehaviour
{
    public static TurnManager turnManager; // Singleton
    public int rewindTurnCount = 0;
    public bool CLOCK = false;
    public PlayerController player;
    public PhantomController phantom;
    // maybe we should integrate all objects into one interface? (considering design pattern)
    public List<Box> boxList = new();
    public List<Button> buttonList = new();
    public List<Lever> leverList = new();
    public List<MovingObstacle> obstacleList = new();
    private void Awake() 
    {
        if (turnManager == null) { turnManager = this; }
    }

    void Start()
    {
        InitializeObjectLists();
    }

    void Update()
    {
        // Starting a turn is done at PlayerController, by StartTurn()
        if (CheckAllMoveComplete())
        {
            EndTurn();
            ResetMoveComplete();
        }
    }

    public void StartTurn() 
    {
        CLOCK = true;
        if (phantom.isPhantomExisting) phantom.AdvanceTurn();
        boxList.ForEach(box => box.AdvanceTurn());
        leverList.ForEach(lever => lever.AdvanceTurn());
        buttonList.ForEach(button => button.AdvanceTurn());
        obstacleList.ForEach(obstacle => obstacle.AdvanceTurn());
    }

    private void EndTurn()
    {
        CLOCK = false;
        // turn++;
        Debug.Log("turn: " + rewindTurnCount);
        player.positionIterator.Add((player.playerCurPos, player.playerCurRot));
        boxList.ForEach(box => box.SaveCurrentPos());
        obstacleList.ForEach(obstacle => obstacle.SaveCurrentPos());
        // lever & button is done at its script, saving its states. 

        // Check if player has cleared the level -> if cleared, go to next level
        LevelManager.levelManager.CheckAndCompleteLevel();
    }

    private bool CheckAllMoveComplete() 
    // Check if all movements of the current turn on the scene are complete.
    {
        return (
            player.isMoveComplete &&
            (!phantom.isPhantomExisting || phantom.isMoveComplete) &&
            boxList.All(box => box.isMoveComplete) &&
            leverList.All(lever => lever.isMoveComplete) &&
            buttonList.All(button => button.isMoveComplete) &&
            obstacleList.All(obstacle => obstacle.isMoveComplete)
        );
    }

    private void ResetMoveComplete()
    {
        player.isMoveComplete = false;
        phantom.isMoveComplete = false;
        boxList.ForEach(box => box.isMoveComplete = false);
        leverList.ForEach(lever => lever.isMoveComplete = false);
        buttonList.ForEach(button => button.isMoveComplete = false);
        obstacleList.ForEach(obstacle => obstacle.isMoveComplete = false);
    }

    // maybe we need a separate ObjectManager to control all these lists & time rewind?
    public void InitializeObjectLists()
    {

        GameObject[] boxObjects = GameObject.FindGameObjectsWithTag("Box");
        boxList.Clear();
        foreach (GameObject boxObject in boxObjects)
        {
            Box boxScript = boxObject.GetComponent<Box>();
            boxList.Add(boxScript); 
        }

        GameObject[] leverObjects = GameObject.FindGameObjectsWithTag("Lever");
        leverList.Clear();
        foreach (GameObject leverObject in leverObjects)
        {
            Lever leverScript = leverObject.GetComponent<Lever>();
            leverList.Add(leverScript);
        }

        GameObject[] buttonObjects = GameObject.FindGameObjectsWithTag("Button");
        buttonList.Clear();
        foreach (GameObject buttonObject in buttonObjects)
        {
            Button buttonScript = buttonObject.GetComponent<Button>();
            buttonList.Add(buttonScript);
        }

        GameObject[] obstacleObjects = GameObject.FindGameObjectsWithTag("MovingObstacle");
        obstacleList.Clear();
        foreach (GameObject obstacleObject in obstacleObjects)
        {
            MovingObstacle obstacleScript = obstacleObject.GetComponent<MovingObstacle>();
            obstacleList.Add(obstacleScript);
        }
    }

    // Functions for time rewind 
    public void EnterTimeRewind()
    // entering time rewind mode: make an empty phantom(actually, the player) in current position 
    {
        player.isTimeRewinding = true;

        // kill already existing phantom, if not ended
        phantom.isPhantomExisting = false;
        phantom.gameObject.SetActive(false);
    }

    public void LeaveTimeRewind()
    // leaving time rewind mode: make a phantom based on the input
    {
        if (player.DidRewind)
        {
            var currentTransform = player.positionIterator.Current;
            phantom.gameObject.transform.position = currentTransform.Item1;
            phantom.gameObject.transform.rotation = currentTransform.Item2;
            phantom.playerCurPos = currentTransform.Item1;
            phantom.playerCurRot = currentTransform.Item2;

            // actually initializing the phantom
            phantom.gameObject.SetActive(true);
            phantom.isPhantomExisting = true;
            phantom.listCommandOrder.Clear();
            while (player.commandIterator.HasNext())
            {
                phantom.listCommandOrder.Add(player.commandIterator.Next());
            }
            phantom.order = 0;

            player.RemoveLastKEntriesFromLogs(rewindTurnCount);

            boxList.ForEach(box => box.RemoveLog(rewindTurnCount));
            leverList.ForEach(lever => lever.RemoveLog(rewindTurnCount));
            buttonList.ForEach(button => button.RemoveLog(rewindTurnCount));
            obstacleList.ForEach(obstacle => obstacle.RemoveLog(rewindTurnCount));
        }
        player.isTimeRewinding = false;
    }

    public void GoToThePast()
    {
        GoToThePastOrFuture(-1);
    }

    public void GoToTheFuture()
    {
        GoToThePastOrFuture(1);
    }

    private void GoToThePastOrFuture(int turnDelta)
    {
        if (turnDelta != -1 && turnDelta != 1)
        {
            throw new ArgumentException("turnDelta must be -1 (Past) or 1 (Future).", nameof(turnDelta));
        }

        rewindTurnCount -= turnDelta;

        // Use iterator to navigate through position logs
        (Vector3 position, Quaternion rotation) newTransform;

        if (turnDelta == -1 && player.positionIterator.HasPrevious())
        {
            newTransform = player.positionIterator.Previous();
            player.commandIterator.Previous();

            boxList.ForEach(box => box.positionIterator.Previous());
            leverList.ForEach(lever => lever.stateIterator.Previous());
            leverList.ForEach(lever => lever.commandIterator.Previous());
            buttonList.ForEach(button => button.stateIterator.Previous());
            buttonList.ForEach(button => button.commandIterator.Previous());
            obstacleList.ForEach(obstacle => obstacle.positionIterator.Previous());
        }
        else if (turnDelta == 1 && player.positionIterator.HasNext())
        {
            newTransform = player.positionIterator.Next();
            player.commandIterator.Next();
            
            boxList.ForEach(box => box.positionIterator.Next());
            leverList.ForEach(lever => lever.stateIterator.Next());
            leverList.ForEach(lever => lever.commandIterator.Next());
            buttonList.ForEach(button => button.stateIterator.Next());
            buttonList.ForEach(button => button.commandIterator.Next());
            obstacleList.ForEach(obstacle => obstacle.positionIterator.Next());
        }
        else
        {
            Debug.Log("Cannot go further in the specified direction!");
            return;
        }

        // Apply new position and rotation to the player
        player.gameObject.transform.position = newTransform.Item1;
        player.gameObject.transform.rotation = newTransform.Item2;
        player.playerCurPos = newTransform.Item1;
        player.playerCurRot = newTransform.Item2;

        // // Restore object states based on the iterator's current position
        boxList.ForEach(box => box.RestoreState());
        leverList.ForEach(lever => lever.RestoreState());
        buttonList.ForEach(button => button.RestoreState());
        obstacleList.ForEach(obstacle => obstacle.RestoreState());
    }
}