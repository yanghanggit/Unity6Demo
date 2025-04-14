using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class ViewHomeAction : RequestAction
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
        Reset();

        var jsonData = JsonConvert.SerializeObject(new ViewHomeRequest { user_name = user, game_name = game });
        yield return PostRequest(url, jsonData);

        var response = JsonConvert.DeserializeObject<ViewHomeResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("response is null");
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
        Debug.Log("ViewHomeAction is success!!!! = " + response.message);
        GameContext.Instance.Mapping = response.mapping;
        Debug.Log("ViewHomeAction mapping is success!!!! = " + JsonConvert.SerializeObject(GameContext.Instance.Mapping));
    }
}
