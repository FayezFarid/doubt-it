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
    private bool ShowAllCardArray = false;
    private bool ShowCardOnFieldArray = false;
    
    public override void OnInspectorGUI()
    {
        try 
        {
            base.OnInspectorGUI();

            GameHandlerv2 _this = (GameHandlerv2)target;
            EditorGUILayout.SelectableLabel($"Array sum{_this.ArraySum}");

            #region allcards
            GUIContent _netarray = new GUIContent("All Cards NetworkArray");
            if (EditorGUILayout.DropdownButton(_netarray,FocusType.Passive))
            {
                if (ShowAllCardArray)
                    ShowAllCardArray = false;
                else ShowAllCardArray = true;
            }
            if (ShowAllCardArray)
            {
                string ArrayContent = string.Empty;
                foreach (var item in _this.AllNetCards)
                {
                    if (item.EqNumber == -1)
                        break;
                    ArrayContent += item.ToString() + "\n";
                    //EditorGUILayout.PrefixLabel(item.ToString());
                }
                EditorGUILayout.TextArea(ArrayContent); 
            }
            #endregion
            #region CardsOnField
            if (EditorGUILayout.DropdownButton(new GUIContent("CardsOnField"), FocusType.Passive))
            {
                if (ShowCardOnFieldArray)
                    ShowCardOnFieldArray = false;
                else ShowCardOnFieldArray = true;
            }
            if (ShowCardOnFieldArray)
            {
                string ArrayContent = string.Empty;
                foreach (var item in _this.CardsOnField)
                {
                    if (item.EqNumber == -1)
                        break;
                    ArrayContent += item.ToString() + "\n";
                }
                EditorGUILayout.TextArea(ArrayContent);
            }
            #endregion
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
[CustomEditor(typeof(OnlinePlayer))]
public class CustomOnlinePlayer : Editor
{
    private bool ShownetCards = false;
    public override void OnInspectorGUI()
    {
        try
        {   
            base.OnInspectorGUI();
            GUIStyle Headers = new GUIStyle()
            {
                fontSize = 15,
                richText = true,
            };
            OnlinePlayer _this = (OnlinePlayer)target;
            EditorGUILayout.LabelField("<color=blue>NetworkPlayerCards Count:</color> ", Headers);
            EditorGUILayout.LabelField(_this.NetCardCount.ToString());
            if (EditorGUILayout.DropdownButton(new GUIContent("CardsOnField"), FocusType.Passive))
            {
                if (ShownetCards)
                    ShownetCards = false;
                else ShownetCards = true;
            }
            if (ShownetCards)
            {
                string ArrayContent = string.Empty;
                foreach (var item in _this.NetworkPlayerCards)
                {
                    if (item.EqNumber == -1)
                        break;
                    ArrayContent += item.ToString() + "\n";
                }
                EditorGUILayout.TextArea(ArrayContent);
            }
        }
        catch
        {

        }

    }
}