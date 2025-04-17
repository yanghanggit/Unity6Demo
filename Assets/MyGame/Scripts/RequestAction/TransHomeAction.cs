using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class TransHomeAction : RequestAction
{

    public IEnumerator Request(string url, string user, string game)
    {
        
        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new DungeonTransHomeRequest { user_name = user, game_name = game });
        yield return PostRequest(url, jsonData);

        // 解析响应数据。
        var response = JsonConvert.DeserializeObject<DungeonTransHomeResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("TransHomeAction response is null");
            yield break;
        }

        // 检查响应数据。
        if (response.error != 0)
        {
            Debug.LogError("TransHomeAction.error = " + response.error);
            Debug.LogError("TransHomeAction.message = " + response.message);
            yield break;
        }

        Debug.Log("TransHomeAction.message = " + response.message);

        // 标记成功。
        MarkRequestAsSuccessful();
    }
}
