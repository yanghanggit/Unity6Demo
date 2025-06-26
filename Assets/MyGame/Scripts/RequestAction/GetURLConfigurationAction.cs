// using UnityEngine;
// using System.Collections;
// using Newtonsoft.Json;

// public class GetURLConfigurationAction : RequestAction
// {
//     public IEnumerator Call(string apiEndpointUrl)
//     {
//         // 重置请求状态。
//         ResetStatus();

//         // 创建请求数据。
//         yield return GetRequest(apiEndpointUrl);

//         // 解析响应数据。
//         var response = JsonConvert.DeserializeObject<URLConfigurationResponse>(DownloadHandlerResponseText);
//         if (response == null)
//         {
//             Debug.LogError("ApiEndpointConfigurationAction:Request response is null");
//             yield break;
//         }

//         // 检查响应数据。
//         if (response.api_version == "")
//         {
//             yield break;
//         }

//         //
//         Debug.Log("URLConfiguration = " + DownloadHandlerResponseText);

//         // 标记成功。
//         MarkRequestAsSuccessful();

//         // 设置 API 端点配置。
//         GameContext.Instance.URLConfiguration = response;
//     }
// }
