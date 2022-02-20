using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    public float Height;
    public float width;
    public GameHandlerv2 gameHandler;
    GUIStyle gUIStyle;
    GUIStyle UnderStyle;
    private void Awake()
    {
        gUIStyle = new GUIStyle() { fontSize = 20 };
        UnderStyle = new GUIStyle() { fontSize = 15, richText = true };
    }
    void OnGUI()
    {
        try { 
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
           new Vector3(Screen.width / Height, Screen.height / width, 1.0f));
        //GUI.TextArea(new Rect(700,600, 540, 370), "3asba");
        GUILayout.BeginVertical(gUIStyle);
        GUILayout.Label("Turn int= "+gameHandler.TurnInt.ToString(), gUIStyle) ;
        GUILayout.Label("Selected Card Number = " + gameHandler.SelectedCardNumber.ToString(), gUIStyle);
        GUILayout.Label("IsCardOnField = " + gameHandler.isCardsOnField.ToString(), gUIStyle);
            if (gameHandler.gameState != null) 
        GUILayout.Label("Game state= "+gameHandler.gameState._state.ToString());
        GUILayout.Label("Online players Count=" + gameHandler.onlinePlayers.Count.ToString(), gUIStyle);
        foreach (OnlinePlayer item in gameHandler.onlinePlayers)
        {
            GUILayout.Label("<color=red>"+item.Name+"</color>", UnderStyle);
            if(item.isLocalPlayer)
                GUILayout.Label("<color=green> LocalPlayer </color>", UnderStyle);
            GUILayout.Label("<color=red> Players Cards count =" + item.PlayerCards.Count + "</color>", UnderStyle);
        }
        GUILayout.EndVertical();
        }
        catch
        {

        }

    }
}
