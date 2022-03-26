using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
[Serializable]
public class ModNetworkArray
{
    [Networked, Capacity(100)]
    public NetworkArray<ModNetworkObject> _Array => default;
    private int count;
    public int Count
    {
        get 
        {
            return count;
        }
        
    }
    public void Set(int index, ModNetworkObject toSet)
    {
        _Array.Set(index,toSet);
    }
    /// <summary>
    /// Gets with index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ModNetworkObject Get(int index)
    {
       return  _Array.Get(index);
    }
    public bool Exist(ModNetworkObject ToSearch)
    {
        foreach (var item in _Array)
        {
            if(item != null)
            if (item == ToSearch)
            {
                return true;
            }
        }
        return false;
    }
    public int GetIndexOf(ModNetworkObject ToSearch)
    {
        if (!Exist(ToSearch))
            return -1;
        int i = 0;
        foreach (var item in _Array)
        {
            if (item != null)
            if (item    == ToSearch)
            {
                return i;
            }
            i++;
        }
        return -1;
    }
    public void Remove(ModNetworkObject toRemove)
    {
        int Index = GetIndexOf(toRemove);
        if (Index == -1)
            return;
         _Array.Set(Index, default);
    }
}
