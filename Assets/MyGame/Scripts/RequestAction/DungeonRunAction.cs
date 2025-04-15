using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

public class DungeonRunAction : RequestAction
{
    public string _message = "";

    void Start()
    {

    }

    void Update()
    {

    }

    public IEnumerator Request(string url, string user, string game, string userInputTag, Dictionary<string, string> data)
    {
        Debug.Log("DungeonRunAction url= " + url);

        // 重置请求状态。
        Reset();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new DungeonGamePlayRequest { user_name = user, game_name = game, user_input = new DungeonGamePlayUserInput { tag = userInputTag, data = data } });
        yield return PostRequest(url, jsonData);

        var response = JsonConvert.DeserializeObject<DungeonGamePlayResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("startResponse is null");
            yield break;
        }

        if (response.error != 0)
        {
            Debug.Log("response.error = " + response.error);
            Debug.Log("response.message = " + response.message);
            yield break;
        }

        // 标记成功。
        OnSuccess();
        GameContext.Instance.ProcessClientMessages(response.client_messages);
        _message = response.message;
    }
}
