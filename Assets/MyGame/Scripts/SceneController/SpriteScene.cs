using UnityEngine;

public class SpriteScene : MonoBehaviour
{
    [Header("Action 组件")]
    public LoadTextureAction _loadTextureAction; // 纹理加载组件
    public GenerateImageAction _generateImageAction; // 图片生成组件

    void Start()
    {
        Debug.Assert(_loadTextureAction != null, "_loadTextureAction is null");
        Debug.Assert(_generateImageAction != null, "_generateImageAction is null");
    }


    public void OnClickTest()
    {
        Debug.Log("Test button clicked - no action assigned");
    }



}