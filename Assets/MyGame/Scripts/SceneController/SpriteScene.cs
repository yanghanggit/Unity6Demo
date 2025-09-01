using UnityEngine;

public class SpriteScene : MonoBehaviour
{
    [Header("Actor图像组件")]
    [SerializeField] private ActorImage[] actorImages;

    void Start()
    {
        // 自动获取场景内所有带有 ActorImage 组件的游戏对象
        actorImages = FindObjectsByType<ActorImage>(FindObjectsSortMode.None);

        Debug.Log($"Found {actorImages.Length} ActorImage components in the scene");

        // 打印找到的所有 ActorImage 组件信息
        for (int i = 0; i < actorImages.Length; i++)
        {
            Debug.Log($"ActorImage {i}: {actorImages[i].gameObject.name}");
        }
    }

    public void OnClickTest()
    {
        Debug.Log("Test button clicked - no action assigned");
        UpdateAllActorImages();
    }

    /// <summary>
    /// 更新所有 ActorImage 组件的图片
    /// </summary>
    public void UpdateAllActorImages()
    {
        if (actorImages == null || actorImages.Length == 0)
        {
            Debug.LogWarning("No ActorImage components found in scene");
            return;
        }

        Debug.Log($"Updating {actorImages.Length} ActorImage components");

        foreach (var actorImage in actorImages)
        {
            if (actorImage != null)
            {
                actorImage.UpdateImage();
            }
        }
    }

    /// <summary>
    /// 获取 ActorImage 组件数量
    /// </summary>
    public int GetActorImageCount()
    {
        return actorImages?.Length ?? 0;
    }

    /// <summary>
    /// 根据索引获取 ActorImage 组件
    /// </summary>
    public ActorImage GetActorImage(int index)
    {
        if (actorImages == null || index < 0 || index >= actorImages.Length)
        {
            Debug.LogWarning($"Invalid ActorImage index: {index}");
            return null;
        }

        return actorImages[index];
    }

    /// <summary>
    /// 重新扫描场景中的 ActorImage 组件
    /// </summary>
    public void RefreshActorImages()
    {
        actorImages = FindObjectsByType<ActorImage>(FindObjectsSortMode.None);
        Debug.Log($"Refreshed: Found {actorImages.Length} ActorImage components in the scene");
    }
}