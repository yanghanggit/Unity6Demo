using UnityEngine;

public class TestBackgroundScene : MonoBehaviour
{
    [SerializeField] private GameObject _spritePrototype;

    [SerializeField] private int _numberOfSprites = 10;

    [SerializeField] private float _spacingMultiplier = 1.1f;

    //添加一个数组用于存储实例化的精灵
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

        // 创建多个精灵实例
        _spriteInstances = CreateSpriteInstances(_spritePrototype, _numberOfSprites, _spacingMultiplier);
        _spritePrototype.SetActive(false);
    }

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
            // 记录实例做最终返回
            instances[i] = spriteInstance;
        }

        return instances;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
