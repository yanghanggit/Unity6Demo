using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class StartAction : RequestAction
{
    public IEnumerator Request(string url, string user, string game, string actor)
    {
        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new StartRequest { user_name = user, game_name = game, actor_name = actor });
        yield return PostRequest(url, jsonData);

        // 解析响应数据。
        var response = JsonConvert.DeserializeObject<StartResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("StartAction response is null");
            yield break;
        }

        // 检查响应数据。
        if (response.error != 0)
        {
            Debug.LogError("StartAction.error = " + response.error);
            Debug.LogError("StartAction.message = " + response.message);
            yield break;
        }

        Debug.Log("StartAction.message = " + response.message);

        // 标记成功。
        MarkRequestAsSuccessful();
    }
}
