using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PhantomController : CharacterBase
{
    public static PhantomController phantomController; // singleton
    public List<string> listCommandOrder = new(); //command order list for phantom input -> condition check -> action (Copy Commands)
    public int order = 0;
    public bool isPhantomExisting = false;
    protected override void Awake() // Singleton
    {
        base.Awake();
        if (PhantomController.phantomController == null) { PhantomController.phantomController = this; }
    }

    protected override void Start()
    {
        base.Start();
        gameObject.SetActive(false);
    }

    public void AdvanceTurn()
    {
        if (order >= listCommandOrder.Count) 
        {
            gameObject.SetActive(false);
            isPhantomExisting = false;    
        }
        HandleMovementInput(listCommandOrder[order]);
        order++;
    }

    void Update() 
    {
        if (isPhantomExisting && TurnManager.turnManager.turnClock) 
        {
            sm.IsDoneAction(); 

            if (doneAction) 
            {
                seq++; 
                doneAction = false;
                if (seq < listCurTurn.Count)
                {
                    sm.SetState(listCurTurn[seq]);
                }
                else
                {
                    sm.SetState(idle);
                    TurnManager.turnManager.dicTurnCheck["Phantom"] = true; // This will be handled at F-023
                }
            }
        }
        sm.DoOperateUpdate();
    }
}
