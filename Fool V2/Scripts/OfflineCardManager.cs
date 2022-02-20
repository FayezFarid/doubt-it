using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Fusion;

public class OfflineCardManager : NetworkBehaviour
{
    [Networked] public int EqNumber { get; set; }
    [Networked] public string Type { get; set; }
    public NetworkBool selectable;
    public Sprite CardBack;
    public Sprite CurrentSprite;
    public override void Spawned()
    {
        selectable = false;
    }
    public void Select()
    {
        if (selectable)
            GameHandlerv2.Instance.AddCardsOrReturn(gameObject);
    }
    public void ChangeSelectable(bool decision)
    {    
        
        selectable = decision;
        //Debug.Log("Selectable Changed in " + gameObject.name + " To " + selectable + " decision is " + decision);
    }
    public void FlipBurgers(bool FlipTo)
    {  if(!FlipTo)
            GetComponent<Image>().sprite = CardBack;
        else GetComponent<Image>().sprite = CurrentSprite;
    }
}
