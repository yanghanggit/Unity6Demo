using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

public class DungeonGamePlayAction : RequestAction
{
    private DungeonGamePlayResponse _response;

    public new void ResetStatus()
    {
        Debug.Log(this.GetType().Name + ":ResetStatus");
        base.ResetStatus();
        _response = null;
    }

    public string ResponseMessage
    {
        get
        {
            if (_response == null)
            {
                return "";
            }
            return _response.message;
        }
    }

    public IEnumerator Request(string url, string user, string game, string userInputTag, Dictionary<string, string> data)
    {

        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new DungeonGamePlayRequest { user_name = user, game_name = game, user_input = new DungeonGamePlayUserInput { tag = userInputTag, data = data } });
        yield return PostRequest(url, jsonData);

        // 解析响应数据。
        _response = JsonConvert.DeserializeObject<DungeonGamePlayResponse>(DownloadHandlerResponseText);
        if (_response == null)
        {
            Debug.LogError("DungeonGamePlayAction response is null");
            yield break;
        }

        // 检查响应数据。
        if (_response.error != 0)
        {
            Debug.LogError("DungeonGamePlayAction.error = " + _response.error);
            Debug.LogError("DungeonGamePlayAction.message = " + _response.message);
            yield break;
        }

        Debug.Log("DungeonGamePlayAction.message = " + _response.message);

        // 标记成功。
        MarkRequestAsSuccessful();

        // 设置游戏状态。
        GameContext.Instance.ProcessClientMessages(_response.client_messages);
    }
}
