using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class BootAction : RequestAction
{
    void Start()
    {

    }

    void Update()
    {

    }

    public IEnumerator Request(string url)
    {
        Debug.Log("BootAction url= " + url);

        // 重置请求状态。
        Reset();

        // 创建请求数据。
        var jsonData = JsonConvert.SerializeObject(new APIEndpointConfigurationRequest());
        yield return PostRequest(url, jsonData);

        Debug.Log("request.downloadHandler.text = " + DownloadHandlerResponseText);
        var response = JsonConvert.DeserializeObject<APIEndpointConfigurationResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("routesConfigurationResponse is null");
            yield break;
        }

        if (response.error != 0)
        {
            Debug.Log("loginResponse.error = " + response.error);
            Debug.Log("loginResponse.message = " + response.message);
            yield break;
        }

        // 标记成功。
        OnSuccess();
        GameContext.Instance.ApiEndpointConfiguration = response.api_endpoints;
        Debug.Log("APIEndpointConfiguration = " + JsonConvert.SerializeObject(GameContext.Instance.ApiEndpointConfiguration));
    }
}
