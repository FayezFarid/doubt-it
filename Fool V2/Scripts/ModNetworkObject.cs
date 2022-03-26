using Fusion;
using System.Collections;
using System;
using System.Collections.Generic;

public struct ModNetworkObject:INetworkStruct, IEquatable<ModNetworkObject>
{
    public NetworkObject _obj;
    public int EqNumber;
   
    public static bool operator !=(ModNetworkObject obj1, ModNetworkObject obj2) => !obj2._obj == obj1._obj;
    public static bool operator ==(ModNetworkObject obj1, ModNetworkObject obj2) => obj2._obj == obj1._obj;

    public bool Equals(ModNetworkObject other)
    {
        throw new NotImplementedException();
    }
    public override string ToString()
    {
        return _obj ? "Empty" : _obj.Id.ToString();
    }
}