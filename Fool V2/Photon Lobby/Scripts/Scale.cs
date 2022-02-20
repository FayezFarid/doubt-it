using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class Scale : MonoBehaviour //,  ILayoutGroup, ILayoutSelfController
{
    public RectTransform BlueArea;
  
    void Start()
    {
        Debug.Log(Screen.width);
        Change();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Change()
    {
        Vector2 size;
        double x=(GetComponent<RectTransform>().sizeDelta.x/2);
        double y = GetComponent<RectTransform>().sizeDelta.y / 2;
        Debug.Log("X= " + x + "Y= " + y+" Default x= "+ GetComponent<RectTransform>().rect.width+" y= "+ GetComponent<RectTransform>().rect.height);
        size = new Vector2((float)x, (float)y);
    }
}
