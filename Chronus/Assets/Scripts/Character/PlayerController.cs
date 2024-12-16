
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

    public void SetIndexNext() => currentIndex++;
    public void SetIndexPrevious() => currentIndex--;

    public void ResetToStart() => currentIndex = 0;

    public void ResetToEnd() => currentIndex = log.Count - 1;

    public int GetCurrentIndex() => currentIndex;

    public T GetNext() => log[currentIndex + 1];

    public void SetCurrent(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        log[currentIndex] = item;
    }

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

    public void RemoveLastK(int k)
    {
        if (k < 0)
        {
            throw new ArgumentException("k must be non-negative.", nameof(k));
        }
        if (k > log.Count)
        {
            throw new InvalidOperationException("Cannot remove more elements than exist in the log.");
        }

        log.RemoveRange(log.Count - k, k);

        if (currentIndex >= log.Count)
        {
            currentIndex = log.Count - 1;
        }
    }

    public void SetPreviousRange(T item)
    {
        for (int i = currentIndex - 1; i >= 0; i--)
        {
            log[i] = item;
        }
    }

    public void Clear()
    {
        log.Clear();
        currentIndex = 0;
    }
}

public class PlayerController : CharacterBase
{
    public static PlayerController playerController;
    public string curKey = "r"; // command log(wasdr)
    public bool isTimeRewinding = false;
    // Iterators for logs
    public TurnLogIterator<string> commandIterator;
    public TurnLogIterator<(Vector3, Quaternion)> positionIterator;
    public bool DidRewind => positionIterator != null && positionIterator.HasNext();

    public List<string> listCommandLog; // command log for time rewind (objects) and phantom action (copy commands)
    public List<(Vector3, Quaternion)> listPosLog; // position tracking log for time rewind (reset position)

    // Blinking effect
    public Renderer[] playerRenderers;
    public Coroutine blinkCoroutine;
    public int blinkCount = 3;
    public float blinkInterval = 0.2f;
    public bool isBlinking = false;

    public Vector3 tempTagetPositionOfBox { get; set; }
    public bool checkOverlappingBox = false;

    protected override void Awake() // singleton
    {
        base.Awake();
        if (playerController == null) { playerController = this; }
    }

    protected override void Start()
    {
        base.Start();

        InitializeLog();

        InputManager.inputManager.OnMovementControl += HandleMovementInput;
        InputManager.inputManager.OnTimeRewindModeToggle += ToggleTimeRewindMode;
        InputManager.inputManager.OnTimeRewindControl += HandleTimeRewindInput;

        // Initialize Renderers
        var meshRenderers = GetComponentsInChildren<MeshRenderer>();
        var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        // Combine both types of renderers into one array
        playerRenderers = new Renderer[meshRenderers.Length + skinnedMeshRenderers.Length];
        meshRenderers.CopyTo(playerRenderers, 0);
        skinnedMeshRenderers.CopyTo(playerRenderers, meshRenderers.Length);

        tempTagetPositionOfBox = Vector3.zero;
    }

    public void InitializeLog()
    {
        playerCurPos = transform.position;
        playerCurRot = transform.rotation;

        listCommandLog = new List<string> { "" };
        listPosLog = new List<(Vector3, Quaternion)> { (playerCurPos, playerCurRot) };

        commandIterator = new TurnLogIterator<string>(listCommandLog);
        positionIterator = new TurnLogIterator<(Vector3, Quaternion)>(listPosLog);
    }

    public void ResetToStart()
    {
        commandIterator.ResetToStart();
        positionIterator.ResetToStart();
        RestoreState();
    }

    private void GameOverAndReset()
    {
        //freeze y pos and bool variables of fall condition reset
        if (!PhantomController.phantomController.isFallComplete) PhantomController.phantomController.KillCharacter(); //intercept during fall
        else PhantomController.phantomController.KillPhantom(); //just normal kill
        TurnManager.turnManager.boxList.ForEach(box => box.DropKillBox());

        TurnManager.turnManager.CLOCK = false; //CLOCK off
        //isMoveComplete reset
        TurnManager.turnManager.ResetMoveComplete();
        //TurnManager.turnManager.ResetFallComplete();

        //reset position or state & reset iterator
        //ResetToStart();
        //InitializeLog();
        TurnManager.turnManager.ResetObjects();

    }

    public override void KillCharacter()
    {
        if (isBlinking) return;

        base.KillCharacter();
        isBlinking = true; //switch on before coroutine.
        StartCoroutine(HandleBlinkAndReset());
    }
    /*
    public void StartBlinking()
    {
        if (playerRenderers == null || playerRenderers.Length == 0) return;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(PlayBlinkEffect());
    }*/

    // Coroutine to handle blinking effect
    private IEnumerator PlayBlinkEffect(int blinkCount)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            foreach (var renderer in playerRenderers)
            {
                renderer.enabled = false;
            }
            yield return new WaitForSeconds(blinkInterval);

            foreach (var renderer in playerRenderers)
            {
                renderer.enabled = true;
            }
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // Coroutine to handle blinking at the current position and reset
    private IEnumerator HandleBlinkAndReset()
    {
        // Blink at the current position
        yield return PlayBlinkEffect(blinkCount);
        // Reset to the starting position and blink again
        GameOverAndReset();
        isBlinking = false; //can move now.
        yield return PlayBlinkEffect(blinkCount - 1);
    }

    protected override void StartAction() // when this is called, the global cycle(the turn) starts
    {
        base.StartAction();
        commandIterator.Add(curKey); // command log update for the phantom
        TurnManager.turnManager.StartTurn();
    }

    protected override void HandleMovementInput(string command)
    {
        if (isBlinking || TurnManager.turnManager.CLOCK || willDropDeath || isTimeRewinding) return; // active when the turn is entirely ended, and not in time rewind mode
        curKey = command;
        base.HandleMovementInput(command);
    }

    // Functions for Time Rewind mode //
    private void ToggleTimeRewindMode()
    {
        if (!isTimeRewinding && (isBlinking || TurnManager.turnManager.CLOCK || willDropDeath)) return; // assuring that every action should be ended (during the turn)

        if (!isTimeRewinding) TurnManager.turnManager.EnterTimeRewind();
        else TurnManager.turnManager.LeaveTimeRewind();
    }

    private void HandleTimeRewindInput(string command)
    {
        if (!isTimeRewinding) return; // active only in time rewind mode
        switch (command)
        {
            case "q": // go to the 1 turn past
                if (positionIterator.HasPrevious()) TurnManager.turnManager.GoToThePast();
                break;

            case "e": // go to the 1 turn future
                if (positionIterator.HasNext()) TurnManager.turnManager.GoToTheFuture();
                break;
        }
    }
    public void SaveCurPosAndRot()
    {
        positionIterator.Add((playerCurPos, playerCurRot));
    }

    public void RemoveLog(int k)
    {
        if (k < 0) throw new ArgumentException("k must be non-negative.", nameof(k));

        commandIterator.RemoveLastK(k);
        positionIterator.RemoveLastK(k);
    }

    public void RestoreState()
    {
        (Vector3 position, Quaternion rotation) newTransform = positionIterator.Current;
        transform.position = newTransform.position;
        transform.rotation = newTransform.rotation;
        playerCurPos = newTransform.position;
        playerCurRot = newTransform.rotation;
    }
}


