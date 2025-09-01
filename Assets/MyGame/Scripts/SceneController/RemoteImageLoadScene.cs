using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RemoteImageLoadScene : MonoBehaviour
{
    [SerializeField] private Image targetImage;   // 预置的占位图已在此
    [SerializeField] private string imageUrl;     // 远程图片URL
    [SerializeField] private float timeout = 10f; // 超时保护

    public Button _startButton;

    public LoadTextureAction _loadTextureAction;

    private Texture2D _currentTexture;

    void Start()
    {
        Debug.Assert(targetImage != null, "targetImage is null");
        Debug.Assert(!string.IsNullOrEmpty(imageUrl), "imageUrl is empty");
        Debug.Assert(_startButton != null, "_startButton is null");
        Debug.Assert(_loadTextureAction != null, "_loadTextureAction is null");
    }

    void OnDestroy()
    {
        if (_currentTexture != null)
        {
            Destroy(_currentTexture);
            _currentTexture = null;
        }
    }

    public void OnClickStart()
    {
        StartCoroutine(LoadTextureAndApplyToImage());
    }

    private IEnumerator LoadTextureAndApplyToImage()
    {
        // 使用 LoadTextureAction 加载纹理
        yield return StartCoroutine(_loadTextureAction.LoadAndApply(imageUrl));

        // 检查是否加载成功
        if (_loadTextureAction.HasTexture())
        {
            // 创建 Sprite 并应用到 Image
            var sprite = _loadTextureAction.CreateSpriteFromCurrentTexture(100f);
            if (sprite != null)
            {
                targetImage.sprite = sprite;
                Debug.Log($"Image applied successfully: {_loadTextureAction.GetTextureInfo()}");
            }
            else
            {
                Debug.LogError("Failed to create sprite from loaded texture");
            }
        }
        else
        {
            Debug.LogWarning("Texture loading failed, keeping placeholder image");
        }
    }

}
