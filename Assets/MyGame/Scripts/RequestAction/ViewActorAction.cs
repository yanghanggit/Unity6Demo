using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ViewActorAction : RequestAction
{
    public IEnumerator Request(List<string> actors = null)
    {
        if (actors == null)
        {
            actors = new List<string>();
        }

        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new ViewActorRequest { user_name = GameContext.Instance.UserName, game_name = GameContext.Instance.GameName, actors = actors });
        yield return PostRequest(GameContext.Instance.VIEW_ACTOR_URL, jsonData);

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
