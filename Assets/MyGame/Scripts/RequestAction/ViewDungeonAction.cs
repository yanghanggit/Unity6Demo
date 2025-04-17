using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class ViewDungeonAction : RequestAction
{

    public IEnumerator Request(string url, string user, string game)
    {
        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new ViewDungeonRequest { user_name = user, game_name = game });
        yield return PostRequest(url, jsonData);

        // 解析响应数据。
        var response = JsonConvert.DeserializeObject<ViewDungeonResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("ViewDungeonAction response is null");
            yield break;
        }

        // 检查响应数据。
        if (response.error != 0)
        {
            Debug.LogError("ViewDungeonAction.error = " + response.error);
            Debug.LogError("ViewDungeonAction.message = " + response.message);
            yield break;
        }

        Debug.Log("ViewDungeonAction.message = " + response.message);

        // 标记成功。
        MarkRequestAsSuccessful();

        // 设置游戏上下文。
        GameContext.Instance.Mapping = response.mapping;
        GameContext.Instance.Dungeon = response.dungeon;
    }
}
