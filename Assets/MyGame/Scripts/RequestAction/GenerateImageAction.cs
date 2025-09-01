using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Text;

/**
 * GenerateImageAction.cs
 * 
 * 专门用于向 FastAPI 服务器发送图片生成请求的 MonoBehaviour
 * 基于 LoadTextureAction 的设计模式
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
    private const string SINGLE_GENERATE_ENDPOINT = "/api/generate";
    private const string BATCH_GENERATE_ENDPOINT = "/api/generate/batch";
    private const string IMAGES_LIST_ENDPOINT = "/api/images";

    // 当前生成结果
    private GenerateImageResponse _lastSingleResult;
    private GenerateBatchImagesResponse _lastBatchResult;

    public GenerateImageResponse LastSingleResult => _lastSingleResult;
    public GenerateBatchImagesResponse LastBatchResult => _lastBatchResult;

    #region 协程版本（兼容现有代码）

    /// <summary>
    /// 生成单张图片（协程版本）
    /// </summary>
    public IEnumerator GenerateSingleImage(string prompt, System.Action<GenerateImageResponse> onComplete = null)
    {
        var request = new GenerateImageRequest(prompt)
        {
            model_name = defaultModelName,
            negative_prompt = defaultNegativePrompt,
            width = defaultWidth,
            height = defaultHeight,
            num_inference_steps = defaultInferenceSteps,
            guidance_scale = defaultGuidanceScale
        };

        yield return StartCoroutine(GenerateSingleImageCoroutine(request, onComplete));
    }

    /// <summary>
    /// 生成单张图片（协程版本，完整参数）
    /// </summary>
    public IEnumerator GenerateSingleImageCoroutine(GenerateImageRequest request, System.Action<GenerateImageResponse> onComplete = null)
    {
        GenerateImageResponse result = null;

        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            string jsonData = JsonConvert.SerializeObject(request);
            Debug.Log($"Generating single image (Attempt {attempt + 1}): {request.prompt}");

            using (var webRequest = CreatePostRequest(SINGLE_GENERATE_ENDPOINT, jsonData))
            {
                yield return webRequest.SendWebRequest();
                result = ProcessSingleImageResponse(webRequest);

                if (result.success)
                {
                    break; // 成功，跳出重试循环
                }

                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Single image generation failed, retrying in {retryDelay} seconds...");
                    yield return new WaitForSeconds(retryDelay);
                }
            }
        }

        _lastSingleResult = result;
        onComplete?.Invoke(result);
    }

    /// <summary>
    /// 批量生成图片（协程版本）
    /// </summary>
    public IEnumerator GenerateBatchImages(List<string> prompts, System.Action<GenerateBatchImagesResponse> onComplete = null)
    {
        if (prompts == null || prompts.Count == 0)
        {
            var errorResult = new GenerateBatchImagesResponse
            {
                success = false,
                message = "提示词列表不能为空"
            };
            onComplete?.Invoke(errorResult);
            yield break;
        }

        if (prompts.Count > 10)
        {
            var errorResult = new GenerateBatchImagesResponse
            {
                success = false,
                message = "单次最多生成10张图片"
            };
            onComplete?.Invoke(errorResult);
            yield break;
        }

        var request = new GenerateBatchImagesRequest(prompts)
        {
            model_name = defaultModelName,
            negative_prompt = defaultNegativePrompt,
            width = defaultWidth,
            height = defaultHeight,
            num_inference_steps = defaultInferenceSteps,
            guidance_scale = defaultGuidanceScale
        };

        yield return StartCoroutine(GenerateBatchImagesCoroutine(request, onComplete));
    }

    /// <summary>
    /// 批量生成图片（协程版本，完整参数）
    /// </summary>
    public IEnumerator GenerateBatchImagesCoroutine(GenerateBatchImagesRequest request, System.Action<GenerateBatchImagesResponse> onComplete = null)
    {
        GenerateBatchImagesResponse result = null;

        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            string jsonData = JsonConvert.SerializeObject(request);
            Debug.Log($"Generating batch images (Attempt {attempt + 1}): {request.prompts.Count} images");

            using (var webRequest = CreatePostRequest(BATCH_GENERATE_ENDPOINT, jsonData))
            {
                yield return webRequest.SendWebRequest();
                result = ProcessBatchImagesResponse(webRequest);

                if (result.success)
                {
                    break; // 成功，跳出重试循环
                }

                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Batch images generation failed, retrying in {retryDelay} seconds...");
                    yield return new WaitForSeconds(retryDelay);
                }
            }
        }

        _lastBatchResult = result;
        onComplete?.Invoke(result);
    }

    #endregion

    #region Async/Await 版本（Unity 6 推荐）

    /// <summary>
    /// 生成单张图片（Async 版本）
    /// </summary>
    public async Task<GenerateImageResponse> GenerateSingleImageAsync(string prompt)
    {
        var request = new GenerateImageRequest(prompt)
        {
            model_name = defaultModelName,
            negative_prompt = defaultNegativePrompt,
            width = defaultWidth,
            height = defaultHeight,
            num_inference_steps = defaultInferenceSteps,
            guidance_scale = defaultGuidanceScale
        };

        return await GenerateSingleImageAsync(request);
    }

    /// <summary>
    /// 生成单张图片（Async 版本，完整参数）
    /// </summary>
    public async Task<GenerateImageResponse> GenerateSingleImageAsync(GenerateImageRequest request)
    {
        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            string jsonData = JsonConvert.SerializeObject(request);
            Debug.Log($"Generating single image async (Attempt {attempt + 1}): {request.prompt}");

            using (var webRequest = CreatePostRequest(SINGLE_GENERATE_ENDPOINT, jsonData))
            {
                var operation = webRequest.SendWebRequest();

                // 等待请求完成
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                var result = ProcessSingleImageResponse(webRequest);

                if (result.success)
                {
                    _lastSingleResult = result;
                    return result;
                }

                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Single image generation failed, retrying in {retryDelay} seconds...");
                    await Task.Delay((int)(retryDelay * 1000));
                }
            }
        }

        var errorResult = new GenerateImageResponse
        {
            success = false,
            message = "Max retry attempts reached"
        };
        _lastSingleResult = errorResult;
        return errorResult;
    }

    /// <summary>
    /// 批量生成图片（Async 版本）
    /// </summary>
    public async Task<GenerateBatchImagesResponse> GenerateBatchImagesAsync(List<string> prompts)
    {
        if (prompts == null || prompts.Count == 0)
        {
            return new GenerateBatchImagesResponse
            {
                success = false,
                message = "提示词列表不能为空"
            };
        }

        if (prompts.Count > 10)
        {
            return new GenerateBatchImagesResponse
            {
                success = false,
                message = "单次最多生成10张图片"
            };
        }

        var request = new GenerateBatchImagesRequest(prompts)
        {
            model_name = defaultModelName,
            negative_prompt = defaultNegativePrompt,
            width = defaultWidth,
            height = defaultHeight,
            num_inference_steps = defaultInferenceSteps,
            guidance_scale = defaultGuidanceScale
        };

        return await GenerateBatchImagesAsync(request);
    }

    /// <summary>
    /// 批量生成图片（Async 版本，完整参数）
    /// </summary>
    public async Task<GenerateBatchImagesResponse> GenerateBatchImagesAsync(GenerateBatchImagesRequest request)
    {
        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            string jsonData = JsonConvert.SerializeObject(request);
            Debug.Log($"Generating batch images async (Attempt {attempt + 1}): {request.prompts.Count} images");

            using (var webRequest = CreatePostRequest(BATCH_GENERATE_ENDPOINT, jsonData))
            {
                var operation = webRequest.SendWebRequest();

                // 等待请求完成
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                var result = ProcessBatchImagesResponse(webRequest);

                if (result.success)
                {
                    _lastBatchResult = result;
                    return result;
                }

                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetryAttempts - 1)
                {
                    Debug.LogWarning($"Batch images generation failed, retrying in {retryDelay} seconds...");
                    await Task.Delay((int)(retryDelay * 1000));
                }
            }
        }

        var errorResult = new GenerateBatchImagesResponse
        {
            success = false,
            message = "Max retry attempts reached"
        };
        _lastBatchResult = errorResult;
        return errorResult;
    }

    /// <summary>
    /// 获取服务器图片列表（Async 版本）
    /// </summary>
    public async Task<ImageListResponse> GetImageListAsync()
    {
        using (var webRequest = UnityWebRequest.Get(serverBaseUrl + IMAGES_LIST_ENDPOINT))
        {
            SetCommonHeaders(webRequest);

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = webRequest.downloadHandler.text;
                    var result = JsonConvert.DeserializeObject<ImageListResponse>(responseText);
                    Debug.Log($"Got image list: {result.total_count} images");
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse image list response: {ex.Message}");
                    return new ImageListResponse();
                }
            }
            else
            {
                Debug.LogError($"Failed to get image list: {webRequest.error}");
                return new ImageListResponse();
            }
        }
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
    /// 处理单张图片生成响应
    /// </summary>
    private GenerateImageResponse ProcessSingleImageResponse(UnityWebRequest request)
    {
        // 检查网络错误
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            string error = $"Connection Error: {request.error}";
            Debug.LogError(error);
            return new GenerateImageResponse { success = false, message = error };
        }

        // 检查协议错误
        if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            string error = $"Protocol Error: {request.error} (Response Code: {request.responseCode})";
            Debug.LogError(error);
            return new GenerateImageResponse { success = false, message = error };
        }

        // 检查数据处理错误
        if (request.result == UnityWebRequest.Result.DataProcessingError)
        {
            string error = $"Data Processing Error: {request.error}";
            Debug.LogError(error);
            return new GenerateImageResponse { success = false, message = error };
        }

        // 成功 - 解析响应
        try
        {
            string responseText = request.downloadHandler.text;
            var result = JsonConvert.DeserializeObject<GenerateImageResponse>(responseText);

            if (result.success)
            {
                Debug.Log($"Single image generated successfully: {result.filename}");
            }
            else
            {
                Debug.LogWarning($"Single image generation failed: {result.message}");
            }

            return result;
        }
        catch (Exception ex)
        {
            string error = $"Exception while parsing single image response: {ex.Message}";
            Debug.LogError(error);
            return new GenerateImageResponse { success = false, message = error };
        }
    }

    /// <summary>
    /// 处理批量图片生成响应
    /// </summary>
    private GenerateBatchImagesResponse ProcessBatchImagesResponse(UnityWebRequest request)
    {
        // 检查网络错误
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            string error = $"Connection Error: {request.error}";
            Debug.LogError(error);
            return new GenerateBatchImagesResponse { success = false, message = error };
        }

        // 检查协议错误
        if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            string error = $"Protocol Error: {request.error} (Response Code: {request.responseCode})";
            Debug.LogError(error);
            return new GenerateBatchImagesResponse { success = false, message = error };
        }

        // 检查数据处理错误
        if (request.result == UnityWebRequest.Result.DataProcessingError)
        {
            string error = $"Data Processing Error: {request.error}";
            Debug.LogError(error);
            return new GenerateBatchImagesResponse { success = false, message = error };
        }

        // 成功 - 解析响应
        try
        {
            string responseText = request.downloadHandler.text;
            var result = JsonConvert.DeserializeObject<GenerateBatchImagesResponse>(responseText);

            if (result.success)
            {
                Debug.Log($"Batch images generated successfully: {result.total_count} images");
            }
            else
            {
                Debug.LogWarning($"Batch images generation failed: {result.message}");
            }

            return result;
        }
        catch (Exception ex)
        {
            string error = $"Exception while parsing batch images response: {ex.Message}";
            Debug.LogError(error);
            return new GenerateBatchImagesResponse { success = false, message = error };
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
    public bool HasSingleResult()
    {
        return _lastSingleResult != null && _lastSingleResult.success;
    }

    /// <summary>
    /// 检查是否有批量生成结果
    /// </summary>
    public bool HasBatchResult()
    {
        return _lastBatchResult != null && _lastBatchResult.success;
    }

    /// <summary>
    /// 清理结果
    /// </summary>
    public void ClearResults()
    {
        _lastSingleResult = null;
        _lastBatchResult = null;
    }

    #endregion
}
