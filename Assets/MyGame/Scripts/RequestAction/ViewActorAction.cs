using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ViewActorAction : RequestAction
{
    public IEnumerator Call(List<string> actors = null)
    {
        if (actors == null)
        {
            actors = new List<string>();
        }

        // 重置请求状态。
        ResetStatus();

        // 1. 构建请求参数。
        var parameters = new List<KeyValuePair<string, string>>();
        if (actors.Count == 0)
        {
            parameters.Add(new KeyValuePair<string, string>("actors", ""));
        }
        else
        {
            for (int i = 0; i < actors.Count; i++)
            {
                parameters.Add(new KeyValuePair<string, string>("actors", actors[i]));
            }
        }

        // 2. 构建完整URL
        string fullUrl = BuildUrlWithQueryParams(GameContext.Instance.VIEW_ACTOR_URL, parameters);

        // 创建请求数据。
        yield return GetRequest(fullUrl);

        // 解析响应数据。
        var response = JsonConvert.DeserializeObject<ViewActorResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("ViewActorAction response is null");
            yield break;
        }

        if (response.error != 0)
        {
            Debug.LogError("ViewActorAction.error = " + response.error);
            Debug.LogError("ViewActorAction.message = " + response.message);
            yield break;
        }

        Debug.Log("ViewActorAction.message = " + response.message);

        // 标记成功。
        MarkRequestAsSuccessful();

        // 更新游戏上下文中的角色快照。
        GameContext.Instance.ActorSnapshots = response.actor_snapshots;
    }
}
