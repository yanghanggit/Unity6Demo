using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
public class RequestAction : MonoBehaviour
{
    private bool _success = false;
    private string _downloadHandlerResponseText = "";

    void Start()
    {

    }

    void Update()
    {

    }

    public bool Success
    {
        get { return _success; }
    }

    public void Reset()
    {
        _success = false;
        _downloadHandlerResponseText = "";
    }

    public void OnSuccess()
    {
        _success = true;
    }

    public string DownloadHandlerResponseText
    {
        get { return _downloadHandlerResponseText; }
    }

    private UnityWebRequest CreatePOSTRequest(string uri, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(uri, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;

    }

    public IEnumerator PostRequest(string url, string jsonData)
    {
        UnityWebRequest request = CreatePOSTRequest(url, jsonData);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
            yield break;
        }

        _downloadHandlerResponseText = request.downloadHandler.text;
        Debug.Log("Response: " + _downloadHandlerResponseText);
    }
}
