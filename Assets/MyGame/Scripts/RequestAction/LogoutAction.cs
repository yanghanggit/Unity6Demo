using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class LogoutAction : RequestAction
{
    void Start()
    {

    }

    void Update()
    {

    }

    public IEnumerator Request(string url, string user, string game)
    {
        Debug.Log("LogoutAction url= " + url);

        // 重置请求状态。
        Reset();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new LogoutRequest { user_name = user, game_name = game });
        yield return PostRequest(url, jsonData);

        var response = JsonConvert.DeserializeObject<LogoutResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("logoutResponse is null");
            yield break;
        }

        Debug.Log("request.downloadHandler.text = " + DownloadHandlerResponseText);
        if (response.error != 0)
        {
            Debug.Log("logoutResponse.error = " + response.error);
            Debug.Log("logoutResponse.message = " + response.message);
            yield break;
        }

        // 标记成功。
        Debug.Log("LogoutResponse is success!!!! = " + response.message + " user_name = " + user + " game_name = " + game);
        OnSuccess();
        GameContext.Instance.UserName = "";
        GameContext.Instance.GameName = "";
        GameContext.Instance.ActorName = "";
    }
}
