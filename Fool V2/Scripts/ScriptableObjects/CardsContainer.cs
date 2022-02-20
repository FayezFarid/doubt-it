using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(menuName = "CardContainer ")]
public class CardsContainer : ScriptableObject, ISerializationCallbackReceiver
{
    public Dictionary<string, List<Sprite>> SpriteContainer;
    public List<Sprite> TempoSprite = new List<Sprite>();
    public List<Sprite> LSprites = new List<Sprite>();
    public List<Sprite> KSprites = new List<Sprite>();
    public List<Sprite> PSprites = new List<Sprite>();
    public void OnBeforeSerialize()
    {
        //Debug.Log("Before seralize");
       
        
    }
    public void OnAfterDeserialize()
    {
        SpriteContainer = new Dictionary<string, List<Sprite>>();
        SpriteContainer.Add("K", KSprites);
        SpriteContainer.Add("L", LSprites);
        SpriteContainer.Add("S", TempoSprite);
        SpriteContainer.Add("p", PSprites);

    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(CardsContainer))]
public class CustomCardContainer :Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CardsContainer _this = (CardsContainer)target;
        //if(GUILayout.Button("Add list"))
        //{
        //    _this.SpriteContainer.Add(_this.TempoChar, _this.TempoSprite);
        //}
        //EditorGUILayout.LabelField("Sprite container content");
        foreach (var item in _this.SpriteContainer)
        {
            EditorGUILayout.LabelField(item.Key.ToString());
            EditorGUILayout.LabelField(item.Value.Count.ToString());
        }
    }
}
#endif