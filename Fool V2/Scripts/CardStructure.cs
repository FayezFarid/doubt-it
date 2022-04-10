using Fusion;
using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;
[Serializable]
public class CardStructure
{
    public NetworkObject _Netobj;
   // public GameObject _obj;
    public OfflineCardManager CardManager;
    public int EqNumber;
    [Header("Unique Id for object my logic related")]
    public int  Netid;
    public Transform transform;
    public CardStructure(NetworkObject netobj, OfflineCardManager cardManager, int eqNumber, int networkCard)
    {
        _Netobj = netobj;
        CardManager = cardManager;
        EqNumber = eqNumber;
        this.Netid = networkCard;
        transform = netobj.transform;
    }

    public override string ToString()
    {
        return _Netobj ? "Empty" : _Netobj.name;
    }


}
public struct NetworkCard : INetworkStruct
{
    public int EqNumber;
    [Header("Unique Id for object my logic related")]
    public int ObjID;

    public NetworkCard(int eqNumber, int objID)
    {
        EqNumber = eqNumber;
        ObjID = objID;
    }
    public override string ToString()
    {
        return $"Eqnumber {EqNumber} Obj ID {ObjID}";
    }
}