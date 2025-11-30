using UnityEngine;

/// <summary>
/// 测试场景控制器 - 用于动态生成精灵实例的原型工厂模式演示
/// 功能：
/// 1. 从一个精灵原型（Template）动态创建多个实例
/// 2. 实例从中心向两侧交替排列（左右左右）
/// 3. 自动计算并更新父级碰撞体以适应所有子对象
/// </summary>
public class TestBackgroundScene : MonoBehaviour
{
    /// <summary>
    /// 精灵原型对象，作为工厂模式的"原型"
    /// 必须包含：SpriteRenderer、BoxCollider2D、SpriteClickHandler 组件
    /// 在 Unity Editor 中赋值，通常指向名为 "Template" 的 GameObject
    /// </summary>
    [SerializeField] private GameObject _spritePrototype;

    /// <summary>
    /// 要生成的精灵实例数量
    /// 默认值：10
    /// </summary>
    [SerializeField] private int _numberOfSprites = 10;

    /// <summary>
    /// 精灵之间的间距倍数
    /// 计算公式：spacing = spriteWidth * spacingMultiplier
    /// 默认值：1.1（留 10% 间隙）
    /// </summary>
    [SerializeField] private float _spacingMultiplier = 1.1f;

    /// <summary>
    /// 存储所有实例化的精灵对象
    /// 在运行时动态创建并填充
    /// </summary>
    private GameObject[] _spriteInstances;

    void Start()
    {
        Debug.Assert(_spritePrototype != null, "_spritePrototype is null");
        Debug.Assert(_numberOfSprites > 0, "_numberOfSprites must be greater than zero");
        Debug.Assert(_spacingMultiplier > 0, "_spacingMultiplier must be greater than zero");

        // 这里检查一下spritePrototype必须有的组件:SpriteRenderer
        SpriteRenderer spriteRenderer = _spritePrototype.GetComponent<SpriteRenderer>();
        Debug.Assert(spriteRenderer != null, "spritePrototype must have a SpriteRenderer component");

        //BoxCollider2D，
        BoxCollider2D boxCollider2D = _spritePrototype.GetComponent<BoxCollider2D>();
        Debug.Assert(boxCollider2D != null, "spritePrototype must have a BoxCollider2D component");

        //SpriteClickHandler
        SpriteClickHandler spriteClickHandler = _spritePrototype.GetComponent<SpriteClickHandler>();
        Debug.Assert(spriteClickHandler != null, "spritePrototype must have a SpriteClickHandler component");

        // 父级需要有 BoxCollider2D 组件，用于整体碰撞检测
        GameObject parentObject = _spritePrototype.transform.parent.gameObject;
        BoxCollider2D parentCollider = parentObject.GetComponent<BoxCollider2D>();
        Debug.Assert(parentCollider != null, "Parent of spritePrototype must have a BoxCollider2D component");

        // 创建多个精灵实例
        _spriteInstances = CreateSpriteInstances(_spritePrototype, _numberOfSprites, _spacingMultiplier);
        _spritePrototype.SetActive(false);

        // 更新父级碰撞体大小
        UpdateParentCollider(_spritePrototype.transform.parent.gameObject);
    }

    /// <summary>
    /// 创建精灵实例的工厂方法
    /// 使用原型模式，从中心点开始向左右两侧交替排列实例
    /// </summary>
    /// <param name="spritePrototype">精灵原型对象，用于实例化</param>
    /// <param name="createNumber">要创建的实例数量</param>
    /// <param name="spacingMultiplier">间距倍数，用于计算精灵之间的间隔</param>
    /// <returns>包含所有创建的精灵实例的数组</returns>
    private GameObject[] CreateSpriteInstances(GameObject spritePrototype, int createNumber, float spacingMultiplier)
    {
        SpriteRenderer spriteRenderer = spritePrototype.GetComponent<SpriteRenderer>();
        Debug.Assert(spriteRenderer != null, "spritePrototype must have a SpriteRenderer component");

        // 获取精灵的实际宽度（考虑 scale）
        // Sprite 的尺寸 * Transform 的 scale = 实际显示宽度
        float spriteWidth = spriteRenderer.bounds.size.x; // bounds.size 已经包含了 scale 的影响

        // 设置精灵之间的间距（可以是精灵宽度的一定比例，比如留 10% 的间隙）
        float spacing = spriteWidth * spacingMultiplier;

        // 获取原型的初始位置（作为中心点）
        Vector3 centerPosition = spritePrototype.transform.position;

        // 创建数组存储实例
        GameObject[] instances = new GameObject[createNumber];

        // 使用 number 这个参数，生成多个精灵实例
        // 从中间开始，左右交替排列：右1、左1、右2、左2...
        for (int i = 0; i < createNumber; i++)
        {
            Vector3 position;

            if (i == 0)
            {
                // 第一个精灵保持在中心位置（原型位置）
                position = centerPosition;
            }
            else
            {
                // 计算偏移：奇数向右，偶数向左
                // i=1 -> 右侧第1个 (+1 * spacing)
                // i=2 -> 左侧第1个 (-1 * spacing)
                // i=3 -> 右侧第2个 (+2 * spacing)
                // i=4 -> 左侧第2个 (-2 * spacing)
                int offset = (i + 1) / 2; // 1,1,2,2,3,3...
                float direction = (i % 2 == 1) ? 1f : -1f; // 奇数为正(右)，偶数为负(左)

                position = centerPosition + new Vector3(direction * offset * spacing, 0, 0);
            }

            // 实例化精灵
            GameObject spriteInstance = Instantiate(spritePrototype, position, Quaternion.identity);
            // 设置名字
            spriteInstance.name = $"Actor_{i}";
            // 设置父级，与原型平级
            spriteInstance.transform.SetParent(spritePrototype.transform.parent);
            // 记录实例做最终返回
            instances[i] = spriteInstance;
        }

        return instances;
    }

    /// <summary>
    /// 更新父级对象的 BoxCollider2D 大小和偏移
    /// 计算所有子对象的包围盒，并调整父级碰撞体以覆盖所有子对象
    /// </summary>
    /// <param name="parent">父级 GameObject，必须已包含 BoxCollider2D 组件</param>
    private void UpdateParentCollider(GameObject parent)
    {
        //
        BoxCollider2D parentCollider = parent.GetComponent<BoxCollider2D>();
        Debug.Assert(parentCollider != null, "Parent must have a BoxCollider2D component");

        // 收集所有子对象的 SpriteRenderer（排除已禁用的）
        SpriteRenderer[] childRenderers = parent.GetComponentsInChildren<SpriteRenderer>();
        if (childRenderers.Length == 0)
        {
            Debug.LogWarning("No child renderers found.");
            return;
        }

        // 初始化包围盒
        Bounds totalBounds = childRenderers[0].bounds;

        // 合并所有子对象的包围盒
        for (int i = 1; i < childRenderers.Length; i++)
        {
            totalBounds.Encapsulate(childRenderers[i].bounds);
        }

        // 将世界坐标的包围盒转换为父级的局部坐标
        Vector3 localCenter = parent.transform.InverseTransformPoint(totalBounds.center);
        Vector3 localSize = totalBounds.size;

        // 设置碰撞体的大小和偏移
        parentCollider.size = new Vector2(localSize.x, localSize.y);
        parentCollider.offset = new Vector2(localCenter.x, localCenter.y);

        Debug.Log($"Parent '{parent.name}' collider updated - Size: {parentCollider.size}, Offset: {parentCollider.offset}");
    }

    void Update()
    {

    }
}


