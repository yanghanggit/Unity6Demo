using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class TransHomeAction : RequestAction
{

    void Start()
    {

    }

    void Update()
    {

    }


    public IEnumerator Request(string url, string user, string game)
    {
        Debug.Log("url= " + url);

        // 重置请求状态。
        Reset();

        var jsonData = JsonConvert.SerializeObject(new DungeonTransHomeRequest { user_name = user, game_name = game });
        yield return PostRequest(url, jsonData);

        var response = JsonConvert.DeserializeObject<DungeonTransHomeResponse>(DownloadHandlerResponseText);
       
        if (response == null)
        {
            Debug.LogError("transDungeonResponse is null");
            yield break;
        }
       
        if (response.error != 0)
        {
            Debug.Log("transDungeonResponse.error = " + response.error);
            Debug.Log("transDungeonResponse.message = " + response.message);
            yield break;
        }

        // 标记成功。
        Debug.Log("TransDungeonResponse is success!!!! = " + response.message);
        OnSuccess();

        //GameContext.Instance.ProcessClientMessages(response.client_messages);
    }
}
