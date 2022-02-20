using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Tests
{
    public class NewTestScript
    {
        public List<GameObject> Cards = new List<GameObject>();
        // A Test behaves as an ordinary method
        //[Test]
        //public void NewTestScriptSimplePasses()
        //{
           
            
        //}
        void CreateCards()
        {

            GameObject EmptyCard =
            MonoBehaviour.Instantiate(Resources.Load<GameObject>("EmptyCard"));
            Debug.Log(EmptyCard.name);

            int PlayerNumber = Random.Range(4, 8);
            for (int Fi = 0; Fi < 13; Fi++)
            {

                for (int f = 0; f < PlayerNumber; f++)
                {
                    GameObject carde = MonoBehaviour.Instantiate(EmptyCard);
                    carde.GetComponent<OfflineCardManager>().EqNumber = Random.Range(0, 12);
                    //carde.GetComponent<OfflineCardManager>().EqNumber = f;
                    carde.name = carde.GetComponent<OfflineCardManager>().EqNumber.ToString();
                    Cards.Add(carde);

                }
            }
            GameObject toCheckWith = new GameObject("Check is here");
            IEnumerable<GameObject> query = Cards.OrderBy(Card => Card.GetComponent<OfflineCardManager>().EqNumber);
            Cards = query.ToList();
            //   toCheckWith.AddComponent<RoatCards>();
            //  toCheckWith.GetComponent<RoatCards>().GoCards = Cards;
            foreach (var item in Cards)
            {
                Debug.Log(item.GetComponent<OfflineCardManager>().EqNumber);
            }
        }
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator DestroyingDuplicate()
        {
            CreateCards();
            int i = 0;
            int j;
            int NbDup = 2;
            List<GameObject> ToBeDeleted = new List<GameObject>();
        
                while (Cards.Count - i >= NbDup)
                {
                    bool Same = true;
                    j = 0;
                    while (j != Cards[i].GetComponent<OfflineCardManager>().EqNumber)
                    {
                        j++;
                        if (j > 13)
                            break;
                    }
                    for (int k = i; k < i + NbDup; k++)
                    {
                        if (Cards[k].GetComponent<OfflineCardManager>().EqNumber != j)
                        {
                            Same = false;
                            break;
                        }

                    }
                    Debug.Log("Card is " + j + " 4 duplicates is " + Same + "i =" + i);
                    if (Same)
                    {
                        for (int Hk = i; Hk < i + NbDup; Hk++)
                        {
                            ToBeDeleted.Add(Cards[Hk]);
                        }
                
                        i += NbDup;
                    }
                    else i++;
                    j++;
                }
            foreach (var item in ToBeDeleted)
            {
                Cards.Remove(item);
            }
            foreach (var item in Cards)
            {
                Debug.Log(item.GetComponent<OfflineCardManager>().EqNumber);
            }
            yield return  new WaitForEndOfFrame();
        }
    }
    
}
