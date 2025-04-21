using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

public class HomeGamePlayAction : RequestAction
{

    public IEnumerator Call(string userInputTag, Dictionary<string, string> data = null)
    {
        if (data == null)
        {
            data = new Dictionary<string, string>();
        }

        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new HomeGamePlayRequest { user_name = GameContext.Instance.UserName, game_name = GameContext.Instance.GameName, user_input = new HomeGamePlayUserInput { tag = userInputTag, data = data } });
        yield return PostRequest(GameContext.Instance.HOME_GAMEPLAY_URL, jsonData);

        // 解析响应数据。
        var response = JsonConvert.DeserializeObject<HomeGamePlayResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("HomeGamePlayAction response is null");
            yield break;
        }

        if (response.error != 0)
        {
            Debug.LogError("HomeGamePlayAction.error = " + response.error);
            Debug.LogError("HomeGamePlayAction.message = " + response.message);
            yield break;
        }

        Debug.Log("HomeGamePlayAction.message = " + response.message);

        // 标记成功。
        MarkRequestAsSuccessful();

        // 设置游戏状态。
        GameContext.Instance.ProcessClientMessages(response.client_messages);
    }
}
