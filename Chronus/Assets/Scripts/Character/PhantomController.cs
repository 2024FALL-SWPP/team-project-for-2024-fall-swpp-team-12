using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PhantomController : CharacterBase
{
    public static PhantomController phantomController; 
    public List<string> listCommandOrder = new();
    public TurnLogIterator<(Vector3, Quaternion)> positionIterator;
    public List<(Vector3, Quaternion)> listPosLog;  
    public int order = 0;
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
        listPosLog = new List<(Vector3, Quaternion)> { (transform.position, transform.rotation) };
        positionIterator = new TurnLogIterator<(Vector3, Quaternion)>(listPosLog);
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
