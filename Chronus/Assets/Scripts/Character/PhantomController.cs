using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PhantomController : CharacterBase
{
    public static PhantomController phantomController; 

    public TurnLogIterator<string> commandIterator;
    public TurnLogIterator<(Vector3, Quaternion)> positionIterator;
    private List<string> listCommandOrder = new();
    private List<(Vector3, Quaternion)> listPosLog;  
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
        // this phantom is actually invoked at PlayerController
    }

    public void InitializeLog()
    {
        listCommandOrder = new List<string> { "" };
        commandIterator = new TurnLogIterator<string>(listCommandOrder);
        listPosLog = new List<(Vector3, Quaternion)> { (transform.position, transform.rotation) };
        positionIterator = new TurnLogIterator<(Vector3, Quaternion)>(listPosLog);
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
        isPhantomExisting = false;
    }

    public override void KillCharacter()
    {
        base.KillCharacter();
        KillPhantom();
    }
}
