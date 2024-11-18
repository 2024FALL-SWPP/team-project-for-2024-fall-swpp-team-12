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
        // this phantom is actually invoked at PlayerController
    }

    public void AdvanceTurn()
    {
        if (order >= listCommandOrder.Count) 
        {
            gameObject.SetActive(false);
            isPhantomExisting = false;    
            return;
        }
        HandleMovementInput(listCommandOrder[order]);
        order++;
    }

    protected override void Update() 
    {
        if (!isPhantomExisting) return;
        base.Update();
    }
}
