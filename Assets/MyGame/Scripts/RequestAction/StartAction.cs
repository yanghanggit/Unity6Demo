using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class StartAction : RequestAction
{

    void Start()
    {

    }

    void Update()
    {

    }

    public IEnumerator Request(string url, string user, string game, string actor)
    {
        Debug.Log("url= " + url);

        // 重置请求状态。
        Reset();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new StartRequest { user_name = user, game_name = game, actor_name = actor });
        yield return PostRequest(url, jsonData);

        Debug.Log("request.downloadHandler.text = " + DownloadHandlerResponseText);
        var response = JsonConvert.DeserializeObject<StartResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("startResponse is null");
            yield break;
        }

        if (response.error != 0)
        {
            Debug.Log("startResponse.error = " + response.error);
            Debug.Log("startResponse.message = " + response.message);
            yield break;
        }

        // 标记成功。
        Debug.Log("StartResponse is success!!!! = " + response.message);
        OnSuccess();

        //GameContext.Instance.ProcessClientMessages(response.client_messages);
    }
}
