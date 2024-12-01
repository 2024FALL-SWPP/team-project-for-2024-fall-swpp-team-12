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

    private RewindSliderUI rewindUI;

    private void Awake() 
    {
        if (turnManager == null) { turnManager = this; }
    }

    void Start()
    {
        InitializeObjectLists();
        InputManager.inputManager.OnUndo += HandleUndo;
        rewindUI = FindObjectOfType<RewindSliderUI>();
    }

    void Update()
    {
        // Starting a turn is done at PlayerController, by StartTurn()
        if (CheckAllMoveComplete())
        {
            EndTurn();
        }
    }

    public void StartTurn() //by player
    {
        CLOCK = true;
        phantom.AdvanceTurn();
        boxList.ForEach(box => box.AdvanceTurn()); //problem here: box move -> button press -> ????
        leverList.ForEach(lever => lever.AdvanceTurn());
        buttonList.ForEach(button => button.AdvanceTurn());
        //after switch change
        obstacleList.ForEach(obstacle => obstacle.AdvanceTurn());
    }

    private void EndTurn()
    {
        //Debug.Log("turn: " + rewindTurnCount);

        obstacleList.ForEach(obstacle => obstacle.SaveCurrentPos());
        player.SaveCurPosAndRot();
        phantom.SaveCurrentPosAndRot();
        boxList.ForEach(box => box.SaveCurrentPos());
        leverList.ForEach(lever => lever.SaveCurrentState());
        buttonList.ForEach(button => button.SaveCurrentState());

        ResetMoveComplete();

        // Check if player has cleared the level -> if cleared, go to next level
        LevelManager.levelManager?.CheckAndCompleteLevel();    
    }

    private bool CheckAllMoveComplete() 
    // Check if all movements of the current turn on the scene are complete.
    {
        return (
            buttonList.All(button => button.isMoveComplete) &&
            CheckMoveCompleteExceptButton()
        );
    }

    public bool CheckMoveCompleteExceptButton()
    {
        return (
            player.isMoveComplete &&
            (!phantom.isPhantomExisting || phantom.isMoveComplete) &&
            boxList.All(box => box.isMoveComplete) &&
            leverList.All(lever => lever.isMoveComplete) &&
            obstacleList.All(obstacle => obstacle.isMoveComplete)
        );
    }

    public bool CheckMovingObjectsMoveComplete()
    // Check if moving obstacles and boxes of the current turn on the scene are complete.
    //need for preventing player and phantom from not falling, staying on the air.
    {
        return (
            boxList.All(box => box.isMoveComplete || !box.isFallComplete) &&
            CheckMovingObstaclesMoveComplete()
        );
    }

    public bool CheckMovingObstaclesMoveComplete()
    // Check if moving obstacles of the current turn on the scene are complete.
    //need for preventing boxes from not falling, staying on the air.
    {
        return obstacleList.All(obstacle => obstacle.isMoveComplete);
    }

    public void ResetMoveComplete()
    {
        player.isMoveComplete = false;
        phantom.isMoveComplete = false;
        boxList.ForEach(box => box.isMoveComplete = false);
        leverList.ForEach(lever => lever.isMoveComplete = false);
        buttonList.ForEach(button => button.isMoveComplete = false);
        obstacleList.ForEach(obstacle => obstacle.isMoveComplete = false);

        CLOCK = false;
    }
    /*
    public void ResetFallComplete()
    {
        phantom.isFallComplete = true;
        boxList.ForEach(box => box.isFallComplete = true);
    }*/

    // maybe we need a separate ObjectManager to control all these lists & time rewind?
    public void InitializeObjectLists()
    {
        rewindTurnCount = 0;

        GameObject[] boxObjects = GameObject.FindGameObjectsWithTag("Box");
        boxList.Clear();
        foreach (GameObject boxObject in boxObjects)
        {
            Box boxScript = boxObject.GetComponent<Box>();
            boxScript.InitializeLog();
            boxList.Add(boxScript); 
        }

        GameObject[] leverObjects = GameObject.FindGameObjectsWithTag("Lever");
        leverList.Clear();
        foreach (GameObject leverObject in leverObjects)
        {
            Lever leverScript = leverObject.GetComponent<Lever>();
            leverScript.InitializeLog();
            leverList.Add(leverScript);
        }

        GameObject[] buttonObjects = GameObject.FindGameObjectsWithTag("Button");
        buttonList.Clear();
        foreach (GameObject buttonObject in buttonObjects)
        {
            Button buttonScript = buttonObject.GetComponent<Button>();
            buttonScript.InitializeLog();
            buttonList.Add(buttonScript);
        }

        GameObject[] obstacleObjects = GameObject.FindGameObjectsWithTag("MovingObstacle");
        obstacleList.Clear();
        foreach (GameObject obstacleObject in obstacleObjects)
        {
            MovingObstacle obstacleScript = obstacleObject.GetComponent<MovingObstacle>();
            obstacleScript.InitializeLog();
            obstacleList.Add(obstacleScript);
        }
    }

    public void ResetObjects()
    {
        player.ResetToStart();
        phantom.ResetToStart();
        boxList.ForEach(box => box.ResetToStart());
        leverList.ForEach(lever => lever.ResetToStart());
        buttonList.ForEach(button => button.ResetToStart());
        obstacleList.ForEach(obstacle => obstacle.ResetToStart());

        player.InitializeLog();
        phantom.InitializeLog();
        boxList.ForEach(box => box.InitializeLog());
        leverList.ForEach(lever => lever.InitializeLog());
        buttonList.ForEach(button => button.InitializeLog());
        obstacleList.ForEach(obstacle => obstacle.InitializeLog());
    }

    // Functions for time rewind 
    public void EnterTimeRewind()
    // entering time rewind mode: make an empty phantom(actually, the player) in current position 
    {
        player.isTimeRewinding = true;

        rewindTurnCount = 0; //always starts at 0.
        // kill already existing phantom, if not ended
        if (phantom.willDropDeath) phantom.KillCharacter(); //intercept during death fall
        else phantom.KillPhantom(); //just normal kill
        rewindTurnCount = 0;
        rewindUI?.EnterRewindMode(player.listCommandLog);
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

            phantom.positionIterator.SetCurrent((phantom.playerCurPos, phantom.playerCurRot, true));

            //Command Order List
            phantom.commandIterator.Clear();
            phantom.commandIterator.Add("");
            while (player.commandIterator.HasNext())
            {
                phantom.commandIterator.Add(player.commandIterator.Next());
            }
            phantom.commandIterator.ResetToStart();

            player.RemoveLog(rewindTurnCount);
            phantom.RemoveLog(rewindTurnCount);
            boxList.ForEach(box => box.RemoveLog(rewindTurnCount));
            leverList.ForEach(lever => lever.RemoveLog(rewindTurnCount));
            buttonList.ForEach(button => button.RemoveLog(rewindTurnCount));
            obstacleList.ForEach(obstacle => obstacle.RemoveLog(rewindTurnCount));
        }
        player.isTimeRewinding = false;

        rewindUI?.LeaveRewindMode();
    }

    public void GoToThePast() //previous - restore
    {
        GoToThePastOrFuture(-1);
    }

    public void GoToTheFuture() //next - restore
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
        //(Vector3 position, Quaternion rotation) newTransform;

        if (turnDelta == -1 && player.positionIterator.HasPrevious())
        {
            player.positionIterator.Previous();
            player.commandIterator.Previous();

            phantom.positionIterator.Previous();
            boxList.ForEach(box => box.positionIterator.Previous());
            leverList.ForEach(lever => lever.stateIterator.Previous());
            buttonList.ForEach(button => button.stateIterator.Previous());
            obstacleList.ForEach(obstacle => obstacle.positionIterator.Previous());
        }
        else if (turnDelta == 1 && player.positionIterator.HasNext())
        {
            player.positionIterator.Next();
            player.commandIterator.Next();

            phantom.positionIterator.Next();
            boxList.ForEach(box => box.positionIterator.Next());
            leverList.ForEach(lever => lever.stateIterator.Next());
            buttonList.ForEach(button => button.stateIterator.Next());
            obstacleList.ForEach(obstacle => obstacle.positionIterator.Next());
        }
        else
        {
            Debug.Log("Cannot go further in the specified direction!");
            return;
        }

        // Apply new position and rotation to the player
        player.RestoreState();

        // Restore object states based on the iterator's current position
        boxList.ForEach(box => box.RestoreState());
        leverList.ForEach(lever => lever.RestoreState());
        buttonList.ForEach(button => button.RestoreState());
        obstacleList.ForEach(obstacle => obstacle.RestoreState());

        rewindUI?.MoveSlider(turnDelta);
    }

    private void HandleUndo() //previous(1) - restore - removelog
    {
        // If player was time rewinding, just leave at the entered tile, with no phantom
        // ^^ need to implement this!!
        if (!PlayerController.playerController.isBlinking && !PlayerController.playerController.willDropDeath && !CLOCK && !player.isTimeRewinding && player.positionIterator.HasPrevious())
        {
            GoToThePast();
            phantom.RestoreState(); //phantom restores state also.

            int deltaTurn = 1;
            player.RemoveLog(deltaTurn);
            phantom.RemoveLog(deltaTurn);
            boxList.ForEach(box => box.RemoveLog(deltaTurn));
            leverList.ForEach(lever => lever.RemoveLog(deltaTurn));
            buttonList.ForEach(button => button.RemoveLog(deltaTurn));
            obstacleList.ForEach(obstacle => obstacle.RemoveLog(deltaTurn));

            phantom.commandIterator.SetIndexPrevious(); // no remove, just checkout previous
        }
    }
}