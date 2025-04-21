using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class ApiEndpointConfigurationAction : RequestAction
{
    public IEnumerator Call(string apiEndpointUrl)
    {
        // 重置请求状态。
        ResetStatus();

        // 创建请求数据。
        yield return GetRequest(apiEndpointUrl, "");

        // 解析响应数据。
        var response = JsonConvert.DeserializeObject<APIEndpointConfigurationResponse>(DownloadHandlerResponseText);
        if (response == null)
        {
            Debug.LogError("ApiEndpointConfigurationAction:Request response is null");
            yield break;
        }

        // 检查响应数据。
        if (response.error != 0)
        {
            Debug.LogError("ApiEndpointConfigurationAction.error = " + response.error);
            Debug.LogError("ApiEndpointConfigurationAction.message = " + response.message);
            yield break;
        }

        //
        Debug.Log("ApiEndpointConfigurationAction.message = " + response.message);

        // 标记成功。
        MarkRequestAsSuccessful();

        // 设置 API 端点配置。
        GameContext.Instance.ApiEndpointConfiguration = response.api_endpoints;
    }
}
