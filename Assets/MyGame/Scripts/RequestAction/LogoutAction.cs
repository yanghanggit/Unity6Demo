using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class LogoutAction : RequestAction
{
    public IEnumerator Request()
    {
        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new LogoutRequest { user_name = GameContext.Instance.UserName, game_name = GameContext.Instance.GameName });
        yield return PostRequest(GameContext.Instance.LOGOUT_URL, jsonData);

        // 解析响应数据。
        var response = JsonConvert.DeserializeObject<LogoutResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("LogoutAction response is null");
            yield break;
        }

        // 检查响应数据。
        if (response.error != 0)
        {
            Debug.LogError("LogoutAction.error = " + response.error);
            Debug.LogError("LogoutAction.message = " + response.message);
            yield break;
        }

        Debug.Log("LogoutAction.message = " + response.message);

        // 标记成功。
        MarkRequestAsSuccessful();

        // 清除登录信息。
        GameContext.Instance.UserName = "";
        GameContext.Instance.GameName = "";
        GameContext.Instance.ActorName = "";
    }
}
