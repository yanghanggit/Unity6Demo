using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class LoginAction : RequestAction
{
    void Start()
    {

    }

    void Update()
    {

    }

    public IEnumerator Request(string url, string user, string game, string actor)
    {
        Debug.Log("LoginAction url= " + url);

        // 重置请求状态。
        Reset();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new LoginRequest { user_name = user, game_name = game, actor_name = actor });
        yield return PostRequest(url, jsonData);

        Debug.Log("request.downloadHandler.text = " + DownloadHandlerResponseText);
        var response = JsonConvert.DeserializeObject<LoginResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("loginResponse is null");
            yield break;
        }

        if (response.error != 0)
        {
            Debug.Log("loginResponse.error = " + response.error);
            Debug.Log("loginResponse.message = " + response.message);
            yield break;
        }

        // 标记成功。
        Debug.Log("LoginResponse is success!!!! = " + response.message);
        OnSuccess();
        GameContext.Instance.UserName = user;
        GameContext.Instance.GameName = game;
        GameContext.Instance.ActorName = actor;
    }
}
