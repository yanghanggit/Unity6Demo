using System.Collections;
using UnityEngine;

public class TestSpriteAtlasScene : MonoBehaviour
{

    [Header("UI Components")]
    [SerializeField] private Sprite[] _characterSprites; // 在Inspector中赋值
    [SerializeField] private GameObject _character; // 角色GameObject

    void Start()
    {
        Debug.Assert(_characterSprites.Length >= 2, "请在Inspector中赋值至少两个角色Sprite");
        Debug.Assert(_character != null, "请在Inspector中赋值角色GameObject");
        StartCoroutine(ChangeSpriteWithDelay(1f, 1)); // 1秒后更换为第二个角色   
    }

    // 创建一个函数 延迟一下 更换角色的sprite
    private IEnumerator ChangeSpriteWithDelay(float delay, int spriteIndex)
    {
        yield return new WaitForSeconds(delay);
        SpriteRenderer spriteRenderer = _character.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = _characterSprites[spriteIndex];
    }
}
