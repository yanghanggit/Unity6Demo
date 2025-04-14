using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ViewActorAction : RequestAction
{
    void Start()
    {

    }

    void Update()
    {

    }

    public IEnumerator Request(string url, string user, string game, List<string> actors)
    {
        Debug.Log("ViewActorAction url= " + url);

        Reset();

        var jsonData = JsonConvert.SerializeObject(new ViewActorRequest { user_name = user, game_name = game, actors = actors });
        yield return PostRequest(url, jsonData);

        var response = JsonConvert.DeserializeObject<ViewActorResponse>(DownloadHandlerResponseText);
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
        GameContext.Instance.ActorSnapshots = response.actor_snapshots;
    }
}
