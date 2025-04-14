using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class HomeRunAction : RequestAction
{
    void Start()
    {

    }

    void Update()
    {

    }

    public IEnumerator Request(string url, string user, string game, string userInput)
    {
        Debug.Log("HomeRunAction url= " + url);

        // 重置请求状态。
        Reset();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new HomeGamePlayRequest { user_name = user, game_name = game, user_input = userInput });
        yield return PostRequest(url, jsonData);

        var response = JsonConvert.DeserializeObject<HomeGamePlayResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("HomeRunAction response is null");
            yield break;
        }

        if (response.error != 0)
        {
            Debug.Log("HomeRunAction.error = " + response.error);
            Debug.Log("HomeRunAction.message = " + response.message);
            yield break;
        }
        // 标记成功。
        Debug.Log("HomeRunAction is success!!!! = " + response.message);
        OnSuccess();
        GameContext.Instance.ProcessClientMessages(response.client_messages);
    }
}
