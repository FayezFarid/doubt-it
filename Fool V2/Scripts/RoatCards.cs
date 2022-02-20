using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
public class RoatCards : MonoBehaviour
{
    public List<int> cards = new List<int>();
    public List<int> CheckList = new List<int>();
    public List<GameObject> GoCards = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 13; i++)
        {
            CheckList.Add(i);
            for (int f = 0; f < 4; f++)
            {
                cards.Add(Random.Range(0, 12));
                // cards.Add(i);
            }
        }
        for (int i = 0; i < 12; i++)
        {
            cards.Add(Random.Range(0, 12));
        }
        IEnumerable<int> query = cards.OrderBy(pet => pet);
        cards = query.ToList();
        Testduplicates4();
       // TestallDone();
        //TestDuplicates();
    }
    void TestallDone()
    {
        int Checker;
        for (int i = 0; i < CheckList.Count; i++)
        {
            Checker = 0;
            for (int j = 0; j < cards.Count; j++)
            {

                if (cards[j] == CheckList[i])
                {
                    Checker++;
                }
                if (Checker == 4)
                    break;
            }
            if (Checker <= 3)
                Debug.LogError("Missing card Number= " + cards[i]);

        }

    }
    void Testduplicates4()
    {
        int i = 0;
        int j;
        while (cards.Count-i>=4)
        {
            bool Same = true;
            j = 0;
            while (j!=cards[i])
            {
                j++;
                if (j > 13)
                    break;
            }
            for (int k = i; k < i+4; k++)
            {
                if(cards[k]!=j)
                {
                    Same = false;
                    break;
                }
                
            }
            Debug.Log("Card is " + j + " 4 duplicates is " + Same +"i ="+i);
            if (Same)
                i += 4;
            else i++;
            j++;
        }
    }
    void TestDuplicates()
    {
        List<int> allDup = new List<int>();
        for (int i = 0; i < 13; i++)
        {
            List<int> Duplicates = new List<int>(4);
                for (int j = 0; j < cards.Count; j++)
                {
                   if (i == cards[j])
                   {
                            Duplicates.Add(cards[j]);
                            if (Duplicates.Count == 4)
                            {
                                foreach (int  item in Duplicates)
                                {
                                    allDup.Add(item);
                                }
                              //  removeNull();
                                Debug.Log("Stack of " + i);
                                break;

                            }

                   }
                    
                }
            
        }
        foreach (var item in allDup)
        {
            cards.Remove(item);
        }
    }
    void removeNull()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (cards[i] == null)
                cards.Remove(cards[i]);
        }
    }
    IEnumerator SendData()
    {
        Debug.Log("Send Data start");
        UnityWebRequest request = UnityWebRequest.Post("http://localhost:5000/test", "{\"username\":\"Ayhaja\"}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        if (request.responseCode != 200)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler);
        }
    }
    IEnumerator RotatedDelayed()
    {

        yield return new  WaitForEndOfFrame();
        GetComponent<GridLayoutGroup>().enabled = false;
        int j = transform.childCount * 3;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i == (transform.childCount / 2))
                transform.GetChild(i).eulerAngles = new Vector3(0, 0,0);
            if (i > (transform.childCount / 2))

                transform.GetChild(i).localPosition = new Vector3(transform.GetChild(i).localPosition.x, transform.GetChild(i).localPosition.y+j + i,0);
            else
                transform.GetChild(i).localPosition = new Vector3(transform.GetChild(i).localPosition.x, transform.GetChild(i).localPosition.y + j -i, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
