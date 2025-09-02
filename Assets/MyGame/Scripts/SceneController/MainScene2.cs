using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
public class MainScene2 : MonoBehaviour
{
    public string _preScene = "LoginScene";

    public LogoutAction _logoutAction;

    public ViewHomeAction _viewHomeAction;

    public ViewActorAction _viewActorAction;

    void Start()
    {
        Debug.Assert(_logoutAction != null, "_logoutAction is null");
        StartCoroutine(LoadHomeAndActorData());
    }

    void Update()
    {

    }

    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        StartCoroutine(ReturnToLoginScene());
    }

    public void OnClickCamp()
    {
        Debug.Log("OnClickCamp");
    }

    public void OnClickDungeon()
    {
        Debug.Log("OnClickDungeon");
    }


    IEnumerator ReturnToLoginScene()
    {
        yield return _logoutAction.Call();

        if (!_logoutAction.LastRequestSuccess)
        {
            Debug.LogError("LogoutAction request failed");
            yield break;
        }

        SceneManager.LoadScene(_preScene);
    }


    private IEnumerator LoadHomeAndActorData()
    {
        yield return _viewHomeAction.Call();
        if (!_viewHomeAction.LastRequestSuccess)
        {
            yield break;
        }

        //提取Mapping中所有的values组成一个List
        List<string> allActors = new List<string>();
        foreach (var kvp in GameContext.Instance.Mapping)
        {
            allActors.AddRange(kvp.Value);
        }
        //打印 allActors
        Debug.Log("All Actors: " + string.Join(", ", allActors));
        yield return _viewActorAction.Call(allActors);

        if (!_viewActorAction.LastRequestSuccess)
        {
            yield break;
        }

        Debug.Log("Home and Actor views updated");

        // 打印 GameContext.Instance.ActorSnapshots 的详细信息
        var actorSnapshots = GameContext.Instance.ActorSnapshots;
        for (int i = 0; i < actorSnapshots.Count; i++)
        {
            var snapshot = actorSnapshots[i];
            try
            {
                // 直接将 EntitySnapshot 序列化为 JSON 字符串
                string jsonString = JsonConvert.SerializeObject(snapshot, Formatting.Indented);
                Debug.Log($"Actor[{i}] JSON:\n{jsonString}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to serialize Actor[{i}] to JSON: {ex.Message}");
            }
        }
    }
}
