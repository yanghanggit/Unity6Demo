using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// LoopScrollRect 完整适配器
/// 同时实现 LoopScrollPrefabSource（Cell 实例化/回收）和 LoopScrollDataSource（数据填充）
/// 并将自身在 Awake 时自动注入到同一 GameObject 上的 LoopVerticalScrollRect。
///
/// Inspector 设置：
///   Cell Prefab  — 原来的 itemPrototype Prefab（含 IDynamicScrollViewItem 组件）
///
/// 无需在 Inspector 手动赋值 dataSource / prefabSource，代码自动完成。
/// </summary>
public class LoopScrollDataSourceAdapter : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
{
    [Tooltip("Cell Prefab，需含 IScrollViewItem 组件")]
    [SerializeField] private GameObject _cellPrefab;

    private readonly Stack<Transform> _pool = new Stack<Transform>();

    private void Awake()
    {
        var scrollRect = GetComponent<LoopHorizontalScrollRect>();
        if (scrollRect == null)
        {
            Debug.LogError("[LoopScrollDataSourceAdapter] 未找到同一 GameObject 上的 LoopHorizontalScrollRect");
            return;
        }
        scrollRect.prefabSource = this;
        scrollRect.dataSource   = this;
    }

    // -------- LoopScrollPrefabSource --------

    public GameObject GetObject(int index)
    {
        if (_pool.Count > 0)
        {
            var pooled = _pool.Pop();
            pooled.gameObject.SetActive(true);
            return pooled.gameObject;
        }
        var go = Instantiate(_cellPrefab);
        go.name = _cellPrefab.name;
        return go;
    }

    public void ReturnObject(Transform trans)
    {
        trans.gameObject.SetActive(false);
        trans.SetParent(transform, false); // 暂存在适配器节点下，脱离 Content
        _pool.Push(trans);
    }

    // -------- LoopScrollDataSource --------

    public void ProvideData(Transform trans, int idx)
    {
        var item = trans.GetComponent<IScrollViewItem>();
        if (item != null)
        {
            item.OnUpdateItem(idx);
        }
        else
        {
            Debug.LogWarning($"[LoopScrollDataSourceAdapter] Cell '{trans.name}' 缺少 IScrollViewItem 组件，index={idx}");
        }
    }
}
