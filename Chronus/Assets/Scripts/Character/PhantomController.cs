using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PhantomController : CharacterBase
{
    public static PhantomController phantomController; 

    public TurnLogIterator<string> commandIterator;
    public TurnLogIterator<(Vector3, Quaternion, bool)> positionIterator;
    private List<string> listCommandOrder = new();
    private List<(Vector3, Quaternion, bool)> listPosLog;
    public bool isPhantomExisting = false;
    protected override void Awake() 
    {
        base.Awake();
        if (phantomController == null) { phantomController = this; }
    }

    protected override void Start()
    {
        base.Start();
        gameObject.SetActive(false);

        InitializeLog();
        // this phantom is actually invoked at PlayerController
    }

    public void InitializeLog()
    {
        listCommandOrder = new List<string> { "" };
        commandIterator = new TurnLogIterator<string>(listCommandOrder);
        listPosLog = new List<(Vector3, Quaternion, bool)> { (transform.position, transform.rotation, false) };
        positionIterator = new TurnLogIterator<(Vector3, Quaternion, bool)>(listPosLog);
    }
    public void ResetToStart()
    {
        positionIterator.ResetToStart();
        RestoreState();
    }

    public void AdvanceTurn()
    {
        if (!commandIterator.HasNext()) 
        {
            KillPhantom();
            return;
        }
        string nextCommand = commandIterator.Next();  // Get the next command
        HandleMovementInput(nextCommand);  // Execute the command
    }

    protected override void Update() 
    {
        if (!isPhantomExisting) return;
        //intercept by timerewinding: at TurnManager - EnterTimeRewind
        //intercept by gameover: at PlayerController - KillCharacter
        base.Update();
    }

    public void KillPhantom()
    {
        gameObject.SetActive(false);
        isPhantomExisting = false; //deactivate. -> can update turn instead of isMoveComplete.
    }

    public override void KillCharacter()
    {
        base.KillCharacter();
        //willDropDeath = false;
        KillPhantom();
    }

    public void SaveCurrentPosAndRot()
    {
        if (willDropDeath || !isPhantomExisting) //save before kill.
        {
            positionIterator.Add((Vector3.zero, Quaternion.identity, false));
        }
        else
        {
            positionIterator.Add((playerCurPos, playerCurRot, true));
        }
    }
    public void RemoveLog(int k)
    {
        positionIterator.RemoveLastK(k);
    }

    public void RestoreState() // updating for time rewind
    {
        var current = positionIterator.Current;
        if (current.Item3)
        {
            transform.position = current.Item1;
            transform.rotation = current.Item2;
            playerCurPos = current.Item1;
            playerCurRot = current.Item2;
            isPhantomExisting = true;
        }
        else isPhantomExisting = false;

        gameObject.SetActive(current.Item3);
    }

}
