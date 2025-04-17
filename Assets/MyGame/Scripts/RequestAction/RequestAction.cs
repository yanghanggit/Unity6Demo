using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

/* * RequestAction.cs
 * 
 * 该类用于处理 HTTP 请求的基本功能。
 * 它提供了发送 POST 请求和处理响应的功能。
 * 
 * 作者: Your Name
 * 日期: 2023-10-01
 */
public class RequestAction : MonoBehaviour
{
    /**
     * 请求是否成功。
     * @return true 如果请求成功，否则 false。
     */
    private bool _requestSuccess = false;

    /**
     * 下载处理程序响应文本。
     * @return 响应文本。
     */
    private string _downloadHandlerResponseText = "";

    /**
     * 请求是否成功。
     * @return true 如果请求成功，否则 false。
     */
    public bool RequestSuccess
    {
        get { return _requestSuccess; }
    }

    /*
     * 获取下载处理程序响应文本。
     * @return 响应文本。
     */
    public string DownloadHandlerResponseText
    {
        get { return _downloadHandlerResponseText; }
    }

    /**
     * 重置请求状态。
     */
    public void ResetStatus()
    {
        Debug.Log(this.GetType().Name + ":ResetStatus");
        _requestSuccess = false;
        _downloadHandlerResponseText = "";
    }

    /**
     * 标记请求为成功。
     */
    public void MarkRequestAsSuccessful()
    {
        Debug.Log(this.GetType().Name + ":MarkRequestAsSuccessful");
        _requestSuccess = true;
    }

    /**
     * 创建 POST 请求。
     * @param uri 请求的 URI。
     * @param jsonData JSON 数据。
     * @return UnityWebRequest 对象。
     */
    private UnityWebRequest CreatePOSTRequest(string uri, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(uri, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;

    }

    /**
     * 发送 POST 请求。
     * @param url 请求的 URL。
     * @param jsonData JSON 数据。
     * @return 协程。
     */
    public IEnumerator PostRequest(string url, string jsonData)
    {
        Debug.Log(this.GetType().Name + ":PostRequest = " + url + ", jsonData= " + jsonData);

        UnityWebRequest request = CreatePOSTRequest(url, jsonData);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
            yield break;
        }

        _downloadHandlerResponseText = request.downloadHandler.text;
        Debug.Log(this.GetType().Name + ":PostResponse = " + _downloadHandlerResponseText);
    }
}
