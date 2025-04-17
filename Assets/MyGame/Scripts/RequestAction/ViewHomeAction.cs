using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class ViewHomeAction : RequestAction
{

    public IEnumerator Request(string url, string user, string game)
    {
        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new ViewHomeRequest { user_name = user, game_name = game });
        yield return PostRequest(url, jsonData);

        // 解析响应数据。
        var response = JsonConvert.DeserializeObject<ViewHomeResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("ViewHomeAction response is null");
            yield break;
        }

        // 检查响应数据。
        if (response.error != 0)
        {
            Debug.LogError("ViewHomeAction.error = " + response.error);
            Debug.LogError("ViewHomeAction.message = " + response.message);
            yield break;
        }

        Debug.Log("ViewHomeAction.message = " + response.message);

        // 标记成功。
        MarkRequestAsSuccessful();

        // 设置游戏上下文。
        GameContext.Instance.Mapping = response.mapping;
    }
}
