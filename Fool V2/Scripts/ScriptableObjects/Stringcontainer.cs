using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
[CreateAssetMenu(menuName = "String container ")]
public class Stringcontainer : ScriptableObject , ISerializationCallbackReceiver
{
  
    [SerializeField]public List<string> ToAdd = new List<string>();
    public Dictionary<Lang, TextHolder> LangPack;
    [SerializeField] List<Lang> _keys = new List<Lang>();
    [SerializeField] List<TextHolder> _values = new List<TextHolder>();
  
    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();

        foreach (var kvp in LangPack)
        {
            //Debug.Log(kvp.Key);
            //Debug.Log(kvp.Value);
            _keys.Add(kvp.Key);
            _values.Add(kvp.Value);
        }
     //   Debug.Log("On B4 Deseerliazie values count " + _values.Count + "  " + _keys.Count);
    }

    public void OnAfterDeserialize()
    {
        LangPack = new Dictionary<Lang, TextHolder>();
        //Debug.Log("On AFter Deseerliazie values count "+_values.Count+"  "+_keys.Count );
        foreach (var item in _values)
        {
            Debug.Log("Outter item");
            foreach (var itemm in item.ContainedString)
            {
               // Debug.Log(itemm);
            }
        }
        for (int i = 0; i<_keys.Count; i++)
        {  
            LangPack.Add(_keys[i], _values[i]);
            //Debug.Log("Adding to lang pack deserliziace");

        }
           
    }
    
}
public enum Lang
{
    AR,
    ENG,
}
[System.Serializable]
public struct TextHolder
{
    public List<string> ContainedString;
}
