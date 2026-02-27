using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// GenerateImage API 客户端，用于处理图片生成请求
/// </summary>
public class GenerateImageApi : BaseApiClient
{
    /// <summary>
    /// 请求结果
    /// </summary>
    private RequestResult _requestResult;

    /// <summary>
    /// 获取请求结果
    /// </summary>
    public override RequestResult ReqResult => _requestResult;

    /// <summary>
    /// 响应数据
    /// </summary>
    private ImageGenerationResponse _responseData;

    /// <summary>
    /// 获取响应数据
    /// </summary>
    public ImageGenerationResponse RespData => _responseData;

    /// <summary>
    /// 调用图片生成 API
    /// </summary>
    /// <param name="url">请求 URL</param>
    /// <param name="configs">图片生成配置列表</param>
    /// <returns>协程枚举器</returns>
    public async UniTask Call(string url, List<ImageGenerationConfig> configs)
    {
        // 记录请求信息
        Debug.Log("Starting GenerateImageApi call...");
        Debug.Log($"URL: {url}");
        Debug.Log($"Configs: {JsonConvert.SerializeObject(configs)}");

        // 清除请求状态
        _requestResult = null;
        _responseData = null;


        // 检查网络连接
        if (!IsNetworkReachable())
        {
            Debug.LogError("No network connection available");
            return;
        }

        // 创建请求数据
        var requestData = new ImageGenerationRequest
        {
            configs = configs
        };
        var jsonData = JsonConvert.SerializeObject(requestData);

        // 发送请求
        _requestResult = await PostRequestAsync(url, jsonData);

        // 处理请求结果
        if (!_requestResult.isSuccess)
        {
            Debug.LogError($"Request failed: {_requestResult.error}");
            return;
        }

        // 解析响应数据
        if (string.IsNullOrEmpty(_requestResult.responseText))
        {
            Debug.LogError("Response text is empty");
            return;
        }

        try
        {
            _responseData = JsonConvert.DeserializeObject<ImageGenerationResponse>(_requestResult.responseText);
            if (_responseData == null)
            {
                Debug.LogError("Deserialized response data is null");
                return;
            }

            Debug.Log($"Image generation successful, generated {_responseData.images.Count} images, elapsed time: {_responseData.elapsed_time}s");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response data: {ex.Message}");
        }
    }
}
