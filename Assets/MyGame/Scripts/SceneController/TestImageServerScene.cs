using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TestImageServerScene : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private string _baseImageServerUrl = "http://192.168.2.121:8300/";

    [Header("API Components")]
    [SerializeField] private ImageRootApi _imageRootApi;
    [SerializeField] private GenerateImageApi _generateImageApi;

    void Start()
    {
        Debug.Assert(!string.IsNullOrEmpty(_baseImageServerUrl), "_baseImageServerUrl is null");
        Debug.Assert(_imageRootApi != null, "_imageRootApi is null");
        Debug.Assert(_generateImageApi != null, "_generateImageApi is null");

        StartCoroutine(InitializeApiEndpoints());
    }

    public void OnClickLoad()
    {
        //StartCoroutine(LoadTextureAndApplyToImage());
    }

    /// <summary>
    /// 新增：点击生成按钮 - 单张图片生成
    /// </summary>
    public void OnClickGenerateSingle()
    {
        //StartCoroutine(GenerateSingleImageAndApply());
        StartCoroutine(TestGenerateImage());
    }

    /// <summary>
    /// 新增：点击生成按钮 - 批量图片生成（选择第一张）
    /// </summary>
    public void OnClickGenerateBatch()
    {
        //StartCoroutine(GenerateBatchImageAndApply());
    }

    /// <summary>
    /// 异步初始化API端点配置
    /// 从指定的基础URL获取根API配置，成功后激活登录按钮
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator InitializeApiEndpoints()
    {
        yield return _imageRootApi.Call(_baseImageServerUrl);

        if (_imageRootApi.ReqResult == null)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseImageServerUrl}: request result is null");
            yield break;
        }

        if (!_imageRootApi.ReqResult.isSuccess)
        {
            Debug.LogError($"Failed to initialize API endpoints from {_baseImageServerUrl}: {_imageRootApi.ReqResult.responseText}");
            yield break;
        }

        Debug.Assert(_imageRootApi.RespData != null, "ImageRootApi response data is null");

        // 设置ImageServerContext的基础URL
        ImageServerContext.Instance.BaseUrl = _baseImageServerUrl;

        // 设置图片服务根响应数据
        RootResp.SetImageRoot(_imageRootApi.RespData);
    }

    /// <summary>
    /// 使用 _generateImageApi 组件进行图片生成测试
    /// 只生成一张图片，提示词为 "A cute cat sitting on a beach"
    /// </summary>
    private IEnumerator TestGenerateImage()
    {
        var configs = new List<ImageGenerationConfig>
        {
            new() { prompt = "A cute cat sitting on a beach", model = "nano-banana", width = 768, height = 1024, num_inference_steps = 4}
        };

        yield return _generateImageApi.Call(ImageServerContext.Instance.GenerateImageApiUrl, configs);

        if (_generateImageApi.ReqResult == null)
        {
            Debug.LogError("GenerateImageApi request result is null");
            yield break;
        }

        if (!_generateImageApi.ReqResult.isSuccess)
        {
            Debug.LogError($"GenerateImageApi call failed: {_generateImageApi.ReqResult.responseText}");
            yield break;
        }

        Debug.Assert(_generateImageApi.RespData != null, "GenerateImageApi response data is null");

        Debug.Log($"Generated {_generateImageApi.RespData.images.Count} images, elapsed time: {_generateImageApi.RespData.elapsed_time}s");
        foreach (var img in _generateImageApi.RespData.images)
        {
            Debug.Log($"Image: {img.filename}, URL: {img.url}, Prompt: {img.prompt}, Model: {img.model}");
        }
    }
}