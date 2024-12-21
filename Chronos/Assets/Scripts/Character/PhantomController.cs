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

    public bool willBoxKillPhantom = false;

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
        positionIterator.SetCurrent((Vector3.zero, Quaternion.identity, false));
        RestoreState();
    }

    public void AdvanceTurn()
    {
        if (isPhantomExisting && !commandIterator.HasNext()) //finished lifetime
        {
            SoundManager.soundManager.PlaySound3D("phantom_terminate", this.transform, 0.07f);
            KillPhantom();
        }

        if (commandIterator.GetCurrentIndex() >= 0)
        {
            if (commandIterator.HasNext())
            {
                if (isPhantomExisting)
                {
                    string nextCommand = commandIterator.Next();  // Get the next command
                    HandleMovementInput(nextCommand);  // Execute the command
                }
                else commandIterator.Next();
            }
            else
            {
                commandIterator.SetIndexNext();
            }
        }
    }

    protected override void Update()
    {
        if (!isPhantomExisting) return;
        //intercept by timerewinding: at TurnManager - EnterTimeRewind
        //intercept by gameover: at PlayerController - KillCharacter
        base.Update();
    }

    public override void AdvanceFall()
    {
        if (willBoxKillPhantom) //check before fall (after all obstacles move)
        {
            willBoxKillPhantom = false;
            SoundManager.soundManager.PlaySound3D("phantom_terminate", this.transform, 0.1f);
            this.KillCharacter();
            return;
        }
        base.AdvanceFall();
    }

    public void KillPhantom()
    {
        if (animator != null && isPhantomExisting)
        {
            animator.Rebind(); // Reset Animator to default pose
            animator.Update(0); // Apply the reset immediately
            gameObject.SetActive(false);
            isPhantomExisting = false; //deactivate. -> can update turn instead of isMoveComplete.
        }
    }

    public override void KillCharacter()
    {
        base.KillCharacter();
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
