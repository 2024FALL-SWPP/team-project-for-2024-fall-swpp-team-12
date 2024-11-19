using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TurnLogIterator<T>
{
    private readonly List<T> log;
    private int currentIndex;

    public TurnLogIterator(List<T> log)
    {
        this.log = log ?? throw new ArgumentNullException(nameof(log));
        this.currentIndex = log.Count - 1; // Start at the latest log
    }

    public bool HasNext() => currentIndex < log.Count - 1;

    public bool HasPrevious() => currentIndex > 0;

    public T Next()
    {
        if (HasNext())
        {
            currentIndex++;
            return log[currentIndex];
        }
        throw new InvalidOperationException("No next turn available.");
    }
    public T Current
    {
        get
        {
            if (log.Count == 0)
            {
                throw new InvalidOperationException("The log is empty.");
            }
            return log[currentIndex];
        }
    }

    public T Previous()
    {
        if (HasPrevious())
        {
            currentIndex--;
            return log[currentIndex];
        }
        throw new InvalidOperationException("No previous turn available.");
    }

    public void ResetToStart() => currentIndex = 0;

    public void ResetToEnd() => currentIndex = log.Count - 1;

    public int GetCurrentIndex() => currentIndex;

    public void Add(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        log.Add(item);
        // Optionally move the iterator to the newly added item
        currentIndex = log.Count - 1;
    }

    public void RemoveLast()
    {
        if (log.Count == 0)
        {
            throw new InvalidOperationException("Cannot remove from an empty log.");
        }

        log.RemoveAt(log.Count - 1);

        // Adjust the currentIndex if necessary
        if (currentIndex >= log.Count)
        {
            currentIndex = log.Count - 1;
        }
    }
}


public class PlayerController : CharacterBase
{
    public static PlayerController playerController; 
    private string curKey = "r"; // command log(wasdr)
    public bool isTimeRewinding = false;     
    // Iterators for logs
    public TurnLogIterator<string> commandIterator;
    public TurnLogIterator<(Vector3, Quaternion)> positionIterator;
    public bool DidRewind => positionIterator != null && positionIterator.HasNext();

    public List<string> listCommandLog; // command log for time rewind (objects) and phantom action (copy commands)
    public List<(Vector3, Quaternion)> listPosLog; // position tracking log for time rewind (reset position)
    protected override void Awake() // singleton
    {
        base.Awake();
        if (PlayerController.playerController == null) { PlayerController.playerController = this; }
    }
    
    protected override void Start()
    {
        base.Start();

        listCommandLog = new List<string>();
        listPosLog = new List<(Vector3, Quaternion)> { (playerCurPos, playerCurRot) };

        commandIterator = new TurnLogIterator<string>(listCommandLog);
        positionIterator = new TurnLogIterator<(Vector3, Quaternion)>(listPosLog);

        InputManager.inputManager.OnCommand += HandleMovementInput;
        InputManager.inputManager.OnTimeRewindModeToggle += ToggleTimeRewindMode;
        InputManager.inputManager.OnTimeRewindControl += HandleTimeRewindInput;
    }

    protected override void Update() 
    {
        base.Update();
    }

    protected override void HandleMovementInput(string command) 
    {
        if (TurnManager.turnManager.CLOCK || isTimeRewinding) return; // active when the turn is entirely ended, and not in time rewind mode
        curKey = command;
        base.HandleMovementInput(command);
    }

    protected override void StartAction() // when this is called, the global cycle(the turn) starts
    {
        base.StartAction();
        commandIterator.Add(curKey); // command log update for the phantom
        TurnManager.turnManager.StartTurn();
    }

    // Functions for Time Rewind mode //
    private void ToggleTimeRewindMode()
    {
        if (TurnManager.turnManager.CLOCK) return; // assuring that every action should be ended (during the turn)

        if (!isTimeRewinding) 
        {
            TurnManager.turnManager.EnterTimeRewind();
        }
        else 
        {
            TurnManager.turnManager.LeaveTimeRewind();
        }
    }

    private void HandleTimeRewindInput(string command)
    {
        if (TurnManager.turnManager.CLOCK || !isTimeRewinding) return; // active only in time rewind mode
        switch (command)
        {
            case "q": // go to the 1 turn past
                if (positionIterator.HasPrevious())
                {
                    TurnManager.turnManager.GoToThePast();
                }
                else
                {
                    print("Cannot go further to the Past!!!");
                }
                break;

            case "e": // go to the 1 turn future
                if (positionIterator.HasNext())
                {
                    TurnManager.turnManager.GoToTheFuture();
                }
                else
                {
                    print("Cannot go further to the Future!!!");
                }
                break;
        }
    }

    public void RemoveLastKEntriesFromLogs(int k)
    {
        if (k < 0)
        {
            throw new ArgumentException("k must be non-negative.", nameof(k));
        }

        // Remove last k entries from listCommandLog
        for (int i = 0; i < k; i++)
        {
            commandIterator.RemoveLast();
            positionIterator.RemoveLast();
        }
    }
}