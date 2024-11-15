using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public static TurnManager turnManager;
    public int turn = 0;
    public bool CLOCK = false;
    public PlayerController player;
    public PhantomController phantom;
    public Dictionary<string, bool> dicTurnCheck;
    // maybe we should integrate all objects into one interface? (considering design pattern)
    public List<Box> boxList = new();
    public List<Button> buttonList = new();
    public List<Lever> leverList = new();
    public List<MovingObstacle> obstacleList = new();
    private void Awake() // Singleton
    {
        if (TurnManager.turnManager == null) { TurnManager.turnManager = this; }
    }

    void Start()
    {
        InitializeObjectLists();
    }

    void Update()
    {
        if (CheckAllMoveComplete())
        {
            EndTurn();
            ResetMoveComplete();
        }
    }

    private bool CheckAllMoveComplete()
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

    public void StartTurn() // this is called at PlayerController
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
        turn++;
        player.listPosLog.Add((player.playerCurPos, player.playerCurRot));
        boxList.ForEach(box => box.SaveCurrentPos());
        //lever & button is done at its script. (state, no move ...) 
    }

    private void InitializeObjectLists()
    {
        GameObject[] boxObjects = GameObject.FindGameObjectsWithTag("Box");
        boxList.Clear();
        foreach (GameObject boxObject in boxObjects)
        {
            Box boxScript = boxObject.GetComponent<Box>();
            boxList.Add(boxScript); // 100% sure there's a script. so not adding if (!= null)
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
    }

    // below this, methods for time rewind 
    public void EnterTimeRewind()
    // entering time rewind mode: make an empty phantom(actually, the player) in current position 
    {
        player.maxTurn = turn;
        player.isTimeRewinding = true;

        // delete already existing phantom, if not ended
        phantom.isPhantomExisting = false;
        phantom.gameObject.SetActive(false);
    }

    public void LeaveTimeRewind()
    // leaving time rewind mode: make a phantom based on the input
    {
        if (turn < player.maxTurn)
        {
            // modifying the location
            phantom.gameObject.transform.position = player.listPosLog[turn].Item1;
            phantom.gameObject.transform.rotation = player.listPosLog[turn].Item2;
            phantom.playerCurPos = player.listPosLog[turn].Item1;
            phantom.playerCurRot = player.listPosLog[turn].Item2;

            // actually initializing the phantom
            phantom.gameObject.SetActive(true);
            phantom.isPhantomExisting = true;
            phantom.listCommandOrder.Clear();
            phantom.listCommandOrder.AddRange(player.listCommandLog.GetRange(turn, player.listCommandLog.Count - turn)); 
            phantom.order = 0;

            // deleting list commands from the player, which has been moved to the phantom
            player.listCommandLog.RemoveRange(turn, player.listCommandLog.Count - turn); // turn 0: no element in listCommandLog
            player.listPosLog.RemoveRange(turn + 1, player.listPosLog.Count - turn - 1); // turn 0: one initial element in listPosLog

            boxList.ForEach(box => box.RemoveLog(turn));
            leverList.ForEach(lever => lever.RemoveLog(turn));
            buttonList.ForEach(button => button.RemoveLog(turn));
        }
        player.isTimeRewinding = false;
    }

    public void GoToThePastOrFuture(int turnDelta) 
    // this is for time rewind, to show changes across all the scene
    {
        turn += turnDelta;
        // position tracking log -> preview the current position(and rotation)
        Vector3 tempPos = player.listPosLog[turn].Item1;
        Quaternion tempRot = player.listPosLog[turn].Item2;
        player.gameObject.transform.position = tempPos;
        player.gameObject.transform.rotation = tempRot;
        player.playerCurPos = tempPos;
        player.playerCurRot = tempRot;

        boxList.ForEach(box => box.RestorePos(turn));
        leverList.ForEach(lever => lever.RestoreState(turn));
        buttonList.ForEach(button => button.RestoreState(turn));
    }

}