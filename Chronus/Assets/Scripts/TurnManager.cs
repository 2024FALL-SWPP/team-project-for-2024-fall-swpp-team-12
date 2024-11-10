using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager turnManager; //public accessibility using Singleton

    public int turn; //The turn, starting from savePoint!!! //also index of "player command list" 

    /* List of Objects for Turn Check (dunno how to get public enum by singleton so let's just handwrite all lol)
        Player,
        Phantom,
        MirrorPhantom,
        Button,
        Lever,
        Platform,
        Box
    */

    public Dictionary<string, bool> dicTurnCheck;

    public Vector3 savePoint_pos;
    public Quaternion savePoint_rot;

    private void Awake() //Singleton
    {
        if(TurnManager.turnManager == null)
        {
            TurnManager.turnManager = this;
        }
        else
        {
            Destroy(gameObject);   // Prevent duplicate TurnManager
        }
    }

    void Start()
    {

        turn = 0; //0 means player is at the savePoint

        dicTurnCheck = new Dictionary<string, bool>();
        dicTurnCheck.Add("Player", false);
        dicTurnCheck.Add("Phantom", true);
        dicTurnCheck.Add("MirrorPhantom", true);
        dicTurnCheck.Add("Button", true);
        dicTurnCheck.Add("Lever", true);
        dicTurnCheck.Add("Platform", true);
        dicTurnCheck.Add("Box", true);

        savePoint_pos = new Vector3(1, 1, 1);
        savePoint_rot = Quaternion.Euler(new Vector3(0,0,0));

        TickManager.OnTick += OnTickEvent;
    }

    private void OnTickEvent() { }

    // Update is called once per frame
    void Update()
    {
        //print(turn);
        if (dicTurnCheck["Player"]) //Player Leads Turn Control.
        {
            bool ph = dicTurnCheck["Phantom"];
            bool mph = dicTurnCheck["MirrorPhantom"];
            bool bt = dicTurnCheck["Button"];
            bool l = dicTurnCheck["Lever"];
            bool pf = dicTurnCheck["Platform"];
            bool bx = dicTurnCheck["Box"];

            if (ph && mph && bt && l && pf && bx)
            {
                AdvanceTurn();
            }
        }
    }

    public void AdvanceTurn()
    {
        turn += 1; // Increment the turn count
        //Debug.Log("Turn advanced to: " + turn);

        // Block input at the start of each new turn
        PlayerController.playerController.BlockInput();
        //Debug.Log("Input has been blocked at the start of turn " + turn);

        PlayerController.playerController.turnClock = false;
        dicTurnCheck["Player"] = false;

        dicTurnCheck["Phantom"] = true;
        dicTurnCheck["MirrorPhantom"] = true;
        dicTurnCheck["Button"] = true;
        dicTurnCheck["Lever"] = true;
        dicTurnCheck["Platform"] = true;
        dicTurnCheck["Box"] = true;

        ClockOff();
    }

    public void ClockOff()
    {
        TriggerFallingObjects();
        CheckCollisions();
        StartCoroutine(ProceedToNextCycle());
    }

    private void TriggerFallingObjects()
    {
        foreach (PushBox box in FindObjectsOfType<PushBox>())
        {
            box.CheckIfFloating();
        }
    }

    private void CheckCollisions()
    {
        if (Physics.Raycast(PlayerController.playerController.transform.position, Vector3.down, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Input blocked");
        PlayerController.playerController.BlockInput();
    }

    private IEnumerator ProceedToNextCycle()
    {
        yield return new WaitForSeconds(0.1f);

        PlayerController.playerController.UnblockInput();
        StartNewCycle();
    }

    private void StartNewCycle()
    {
        // Reset turn check for next cycle
        dicTurnCheck["Player"] = false;
        dicTurnCheck["Phantom"] = true;
        dicTurnCheck["MirrorPhantom"] = true;
        dicTurnCheck["Button"] = true;
        dicTurnCheck["Lever"] = true;
        dicTurnCheck["Platform"] = true;
        dicTurnCheck["Box"] = true;
    }
}