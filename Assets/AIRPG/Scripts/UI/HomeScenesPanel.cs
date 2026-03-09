using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HomeScenesPanel : MonoBehaviour
{

    [SerializeField] private LoopVerticalScrollRect _scrollView; // 动态滚动视图

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Assert(_scrollView != null, "_scrollView is null");

        //
        RereshViewAsync().Forget();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private async UniTaskVoid RereshViewAsync()
    {
        await UniTask.Yield();
        // 强制刷新 Canvas 布局，确保 Rect Mask 2D 的裁剪区域已计算完毕
        // 否则 LoopScrollRect 初始化时判断可视范围为空，不会生成任何 Cell
        //Canvas.ForceUpdateCanvases();
        _scrollView.totalCount = 10;
        _scrollView.RefillCells(); // 重建列表并回到顶部
    }


}
