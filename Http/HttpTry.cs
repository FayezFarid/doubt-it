using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft;

public class HttpTry : MonoBehaviour
{
    public TestJson jsontest;
    public string jsonFile;
    public void Start()
    {
        jsontest = new TestJson() { TestInt = 1, TestString = "wewwwwwwwww" };
        StartCoroutine(SendData());
    }
    IEnumerator SendData()
    {
        Debug.Log("Send Data start");
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:5000/test");
      
        yield return request.SendWebRequest();
        if ( request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
        }
        jsonFile=JsonUtility.ToJson(jsontest);

    }
}
[System.Serializable]
public class TestJson
{
    public int TestInt;
    public string TestString;
}