using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class LoginAction : RequestAction
{

    public IEnumerator Request(string user, string game, string actor)
    {
        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new LoginRequest { user_name = user, game_name = game, actor_name = actor });
        yield return PostRequest(GameContext.Instance.LOGIN_URL, jsonData);

        // 解析响应数据。
        var response = JsonConvert.DeserializeObject<LoginResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("LoginAction is null");
            yield break;
        }

        if (response.error != 0)
        {
            Debug.LogError("LoginAction.error = " + response.error);
            Debug.LogError("LoginAction.message = " + response.message);
            yield break;
        }

        Debug.Log("LoginAction.message = " + response.message);

        // 标记成功。
        MarkRequestAsSuccessful();

        // 设置登录信息。
        GameContext.Instance.UserName = user;
        GameContext.Instance.GameName = game;
        GameContext.Instance.ActorName = actor;
    }
}
