using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBox : MonoBehaviour
{
    public float moveDistance = 2.0f;
    private Rigidbody rb;
    public float checkDistance = 0.5f;

    public Vector3 boxCurPos;
    // Time rewind logs
    public List<Vector3> listBoxPosLog; // position
    private int maxTurn = 0; //Time Rewinding Mode
    private int curTurn = 0; //for simultaneous(parallel) Time Rewinding Mode managing, local(private) turn variable is need for each object.
    private bool isTimeRewinding = false; //for simultaneous(parallel) Time Rewinding Mode managing, local(private) turn variable is need for each object.

    private void Start()
    {
        InputManager.inputManager.OnTimeRewindModeToggle += ToggleTimeRewindModeForBox;
        InputManager.inputManager.OnTimeRewindControl += HandleTimeRewindInputForBox;

        rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        // Initialize log
        listBoxPosLog = new List<Vector3>() { transform.position };
    }
    /*
    private void Update()
    {
        if (TurnManager.turnManager.turnClock)
        {
            if (!TurnManager.turnManager.dicTurnCheck["Box"])
            {
                SaveCurrentPos();
                TurnManager.turnManager.dicTurnCheck["Box"] = true;
            }
        }

        // Demo falling code. need fix.
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, checkDistance))
        {
            if (hit.collider.CompareTag("GroundFloor") || hit.collider.CompareTag("FirstFloor"))
            {
                // Tile detected, keep Y position constraint to keep the box stable
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            }
        }
        else
        {
            // If no tile is detected, allow the box to fall
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    private void SaveCurrentPos()
    {
        listBoxPosLog.Add(this.transform.position);
    }

    public void RestorePos(int turnIndex)
    {
        transform.position = listBoxPosLog[turnIndex];
    }
    public void RemoveLog(int startIndex)
    {
        listBoxPosLog.RemoveRange(startIndex + 1, listBoxPosLog.Count - startIndex - 1); // turn 0: one initial element
    }



    // Functions for Time Rewind //
    private void ToggleTimeRewindModeForBox()
    {
        if (TurnManager.turnManager.CLOCK) return; //***** active when Not executing Actions.

        if (!isTimeRewinding) //when OFF
        {
            maxTurn = TurnManager.turnManager.turn;
            curTurn = maxTurn;
            isTimeRewinding = true; //toggle ON
        }
        else //when ON
        {
            if (curTurn < maxTurn)
            {
                RemoveLog(curTurn);
                boxCurPos = transform.position;
            }
            isTimeRewinding = false; //toggle OFF
        }
    }



    private void HandleTimeRewindInputForBox(string command)
    {
        if (TurnManager.turnManager.CLOCK || !isTimeRewinding) return; //***** active when Time Rewinding Mode.
        switch (command)
        {
            case "q": // go to the Past (turn -1)
                if (curTurn >= 1)
                {
                    GoToThePastOrFuture(-1);
                }
                break;

            case "e": // go to the Future (turn +1)
                if (curTurn <= maxTurn - 1)
                {
                    GoToThePastOrFuture(1);
                }
                break;
        }
    }
    private void GoToThePastOrFuture(int turnDelta)
    {
        curTurn += turnDelta;
        //position tracking log -> preview the current position(and rotation)
        RestorePos(curTurn);
    }
}
