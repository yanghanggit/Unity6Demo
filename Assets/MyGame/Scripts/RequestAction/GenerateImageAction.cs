using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Text;

/**
 * GenerateImageAction.cs
 * 
 * 专门用于向 FastAPI 服务器发送图片生成请求的 MonoBehaviour
 * 基于新的简化数据结构
 * 支持单张和批量图片生成，协程和 async/await 两种方式
 */
public class GenerateImageAction : MonoBehaviour
{
    [Header("服务器配置")]
    [SerializeField] private string serverBaseUrl = "http://localhost:8300";
    [SerializeField] private float requestTimeout = 120f; // 图片生成需要更长时间
    [SerializeField] private int maxRetryAttempts = 3;
    [SerializeField] private float retryDelay = 2f;

    [Header("默认生成参数")]
    [SerializeField] private string defaultModelName = "sdxl-lightning";
    [SerializeField] private string defaultNegativePrompt = "worst quality, low quality, blurry";
    [SerializeField] private int defaultWidth = 768;
    [SerializeField] private int defaultHeight = 768;
    [SerializeField] private int defaultInferenceSteps = 4;
    [SerializeField] private float defaultGuidanceScale = 7.5f;

    // API 端点
    private const string GENERATE_ENDPOINT = "/api/generate";

    // 当前生成结果
    private GenerateImagesResponse _lastResult;

    public GenerateImagesResponse LastResult => _lastResult;

    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 生成图片（协程版本，完整参数）
    /// </summary>
    public IEnumerator GenerateImagesCoroutine(GenerateImagesRequest request, System.Action<GenerateImagesResponse> onComplete = null)
    {
        GenerateImagesResponse result = null;

        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            string jsonData = JsonUtility.ToJson(request);
            Debug.Log($"Generating images (Attempt {attempt + 1}): {request.prompts.Count} images");

            using (var webRequest = CreatePostRequest(GENERATE_ENDPOINT, jsonData))
            {
                yield return webRequest.SendWebRequest();
                result = ProcessGenerateImagesResponse(webRequest);

                if (result.success)
                {
                    break; // 成功，跳出重试循环
                }

                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Images generation failed, retrying in {retryDelay} seconds...");
                    yield return new WaitForSeconds(retryDelay);
                }
            }
        }

        _lastResult = result;
        onComplete?.Invoke(result);
    }

    #endregion

    #region Async/Await 版本（Unity 6 推荐）


    /// <summary>
    /// 生成图片（Async 版本，完整参数）
    /// </summary>
    public async Task<GenerateImagesResponse> GenerateImagesAsync(GenerateImagesRequest request)
    {
        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            string jsonData = JsonUtility.ToJson(request);
            Debug.Log($"Generating images async (Attempt {attempt + 1}): {request.prompts.Count} images");

            using (var webRequest = CreatePostRequest(GENERATE_ENDPOINT, jsonData))
            {
                var operation = webRequest.SendWebRequest();

                // 等待请求完成
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                var result = ProcessGenerateImagesResponse(webRequest);

                if (result.success)
                {
                    _lastResult = result;
                    return result;
                }

                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Images generation failed, retrying in {retryDelay} seconds...");
                    await Task.Delay((int)(retryDelay * 1000));
                }
            }
        }

        var errorResult = new GenerateImagesResponse
        {
            success = false,
            message = "Max retry attempts reached"
        };
        _lastResult = errorResult;
        return errorResult;
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 创建 POST 请求
    /// </summary>
    private UnityWebRequest CreatePostRequest(string endpoint, string jsonData)
    {
        string url = serverBaseUrl + endpoint;
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = Mathf.CeilToInt(requestTimeout);

        SetCommonHeaders(request);
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }

    /// <summary>
    /// 设置通用请求头
    /// </summary>
    private void SetCommonHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("Accept", "application/json");

        // WebGL 特殊处理
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 构建中避免某些可能被浏览器阻止的头部
#else
        request.SetRequestHeader("User-Agent", $"Unity-{Application.unityVersion}");
#endif
    }

    /// <summary>
    /// 处理图片生成响应
    /// </summary>
    private GenerateImagesResponse ProcessGenerateImagesResponse(UnityWebRequest request)
    {
        // 检查网络错误
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            string error = $"Connection Error: {request.error}";
            Debug.LogError(error);
            return new GenerateImagesResponse { success = false, message = error };
        }

        // 检查协议错误
        if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            string error = $"Protocol Error: {request.error} (Response Code: {request.responseCode})";
            Debug.LogError(error);
            return new GenerateImagesResponse { success = false, message = error };
        }

        // 检查数据处理错误
        if (request.result == UnityWebRequest.Result.DataProcessingError)
        {
            string error = $"Data Processing Error: {request.error}";
            Debug.LogError(error);
            return new GenerateImagesResponse { success = false, message = error };
        }

        // 成功 - 解析响应
        try
        {
            string responseText = request.downloadHandler.text;
            var result = JsonUtility.FromJson<GenerateImagesResponse>(responseText);

            if (result.success)
            {
                Debug.Log($"Images generated successfully: {result.images.Count} images");
            }
            else
            {
                Debug.LogWarning($"Images generation failed: {result.message}");
            }

            return result;
        }
        catch (Exception ex)
        {
            string error = $"Exception while parsing images response: {ex.Message}";
            Debug.LogError(error);
            return new GenerateImagesResponse { success = false, message = error };
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 设置服务器地址
    /// </summary>
    public void SetServerUrl(string url)
    {
        serverBaseUrl = url.TrimEnd('/');
        Debug.Log($"Server URL set to: {serverBaseUrl}");
    }

    /// <summary>
    /// 设置默认生成参数
    /// </summary>
    public void SetDefaultParameters(string modelName = null, string negativePrompt = null,
                                   int? width = null, int? height = null,
                                   int? inferenceSteps = null, float? guidanceScale = null)
    {
        if (!string.IsNullOrEmpty(modelName)) defaultModelName = modelName;
        if (!string.IsNullOrEmpty(negativePrompt)) defaultNegativePrompt = negativePrompt;
        if (width.HasValue) defaultWidth = width.Value;
        if (height.HasValue) defaultHeight = height.Value;
        if (inferenceSteps.HasValue) defaultInferenceSteps = inferenceSteps.Value;
        if (guidanceScale.HasValue) defaultGuidanceScale = guidanceScale.Value;

        Debug.Log($"Default parameters updated: {defaultModelName}, {defaultWidth}x{defaultHeight}");
    }

    /// <summary>
    /// 检查是否有生成结果
    /// </summary>
    public bool HasResult()
    {
        return _lastResult != null && _lastResult.success;
    }

    /// <summary>
    /// 清理结果
    /// </summary>
    public void ClearResults()
    {
        _lastResult = null;
    }

    #endregion
}
