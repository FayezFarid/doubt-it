using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AreaControler : MonoBehaviour
{
    [SerializeField] public bool Over8;
    bool Under8 ;
    bool Over12;
    [SerializeField] bool HasBeenMod;
    [SerializeField] GameObject GON;
    void Update()
    {
        if (transform.childCount > 8)
        { if (!Over8)
            {
                foreach (Transform card in transform)
                {
                    card.GetComponent<Image>().enabled = false;
                }
                Over8 = true;
                HasBeenMod = true;
                GameObject go1 = new GameObject();
                go1.name = name + "Visual";
                go1.AddComponent<TextMeshProUGUI>().text = transform.childCount.ToString() + "Cards";
                go1.GetComponent<TextMeshProUGUI>().fontSize = 100;
                go1.transform.SetParent(GameObject.Find("Canvas").transform, false);
                go1.transform.SetSiblingIndex(8);
                if (transform.position.x < 0)
                    go1.transform.localPosition = transform.localPosition;
                else go1.transform.position = transform.position + new Vector3(200, 0, 0);
                go1.GetComponent<RectTransform>().sizeDelta += new Vector2(300, 50);
                GON = go1;
            }
        }
        else
        {
            if (HasBeenMod)
            { 
              foreach (Transform card in transform)
              {
                card.GetComponent<Image>().enabled = true ;
              }
                Destroy(GON);
                HasBeenMod = false;
                Over8 = false;

            }
        }
    }
}
