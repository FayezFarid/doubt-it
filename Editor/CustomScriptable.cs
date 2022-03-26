using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Stringcontainer))]
public class CustomStringContainer : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Stringcontainer _this = (Stringcontainer)target;
        if (GUILayout.Button("Add list"))
        {
            TextHolder _txt = new TextHolder();
            _txt.ContainedString = _this.ToAdd;
            _this.LangPack.Add(Lang.AR, _txt);
            Debug.Log("added");
        }
        if (GUILayout.Button("Remove Eng list"))
        {
            _this.LangPack.Remove(Lang.AR);
            Debug.Log("added");
        }
        EditorGUILayout.LabelField("Lang pack ");
        foreach (var item in _this.LangPack)
        {
            EditorGUILayout.LabelField(item.Key.ToString());
            foreach (var itemm in item.Value.ContainedString)
            {
                EditorGUILayout.TextArea(itemm);
            }
        }
    }

}
[CustomEditor(typeof(GameHandlerv2))]
public class CustomGameHandlerV2 : Editor
{
    public override void OnInspectorGUI()
    {
        try {
        base.OnInspectorGUI();

        GameHandlerv2 _this = (GameHandlerv2)target;
            EditorGUILayout.LabelField("StartArray test");
            EditorGUILayout.LabelField($"Count {_this._Array.Length}");
            foreach (var item in _this._Array)
            {
                
               EditorGUILayout.LabelField(item.ToString());
            }
            EditorGUILayout.LabelField("End Array");
            EditorGUILayout.LabelField("StartArray");
            foreach (var item in _this.all._Array)
            {  if(item !=null)
                EditorGUILayout.LabelField(item.ToString());
            }
            EditorGUILayout.LabelField("End Array");
        EditorGUILayout.LabelField(_this.CheckWith4.ToString());
        EditorGUILayout.LabelField("Game State");
        if (_this.gameState != null)
        {
            EditorGUILayout.LabelField(_this.gameState.ToString());
        }
        }
        catch
        {

        }

    }
}