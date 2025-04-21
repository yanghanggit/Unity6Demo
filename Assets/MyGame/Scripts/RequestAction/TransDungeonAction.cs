using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class TransDungeonAction : RequestAction
{

    public IEnumerator Call()
    {
        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new HomeTransDungeonRequest { user_name = GameContext.Instance.UserName, game_name = GameContext.Instance.GameName });
        yield return PostRequest(GameContext.Instance.HOME_TRANS_DUNGEON_URL, jsonData);

        // 解析响应数据。
        var response = JsonConvert.DeserializeObject<HomeTransDungeonResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("TransDungeonAction response is null");
            yield break;
        }

        // 检查响应数据。
        if (response.error != 0)
        {
            Debug.LogError("TransDungeonAction.error = " + response.error);
            Debug.LogError("TransDungeonAction.message = " + response.message);
            yield break;
        }

        Debug.Log("TransDungeonAction.message = " + response.message);

        // 标记成功。
        MarkRequestAsSuccessful();
    }
}
