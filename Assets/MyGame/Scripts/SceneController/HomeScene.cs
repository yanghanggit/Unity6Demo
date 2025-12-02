using UnityEngine;
using Mosframe;
using TMPro;

public class HomeScene : MonoBehaviour, IStringGameEventListener
{
    [Header("UI Components")]
    [SerializeField] private GameObject _background;
    [SerializeField] private GameObject _currentActor;
    [SerializeField] private GameObject _speechBubble;
    [SerializeField] private TMP_Text _speechBubbleText;
    [SerializeField] private GameObject _state1;
    [SerializeField] private DynamicScrollView _scrollView;

    [Header("Scene Config")]
    [SerializeField] private HomeSceneConfig _homeSceneConfig;

    [Header("API Components")]
    [SerializeField] private HomeGamePlayApi _homeGamePlayApi;

    [Header("Events")]
    [SerializeField] private StringGameEvent onActorClickedEvent;


    void OnEnable()
    {
        Debug.Assert(onActorClickedEvent != null, "onActorClickedEvent is null");
        onActorClickedEvent.RegisterListener(this);
        
    }

    void OnDisable()
    {
        Debug.Assert(onActorClickedEvent != null, "onActorClickedEvent is null");
        onActorClickedEvent.UnregisterListener(this);
    }

    void Start()
    {
        Debug.Assert(_background != null, "_background is null");
        Debug.Assert(_currentActor != null, "_currentActor is null");
        Debug.Assert(_speechBubble != null, "_speechBubble is null");
        Debug.Assert(_speechBubbleText != null, "_speechBubbleText is null");
        Debug.Assert(_state1 != null, "_state1 is null");
        Debug.Assert(_scrollView != null, "_scrollView is null");
        Debug.Assert(_homeSceneConfig != null, "_homeSceneConfig is null");
        Debug.Assert(_homeGamePlayApi != null, "_homeGamePlayApi is null");
        Debug.Assert(onActorClickedEvent != null, "onActorClickedEvent is null");

        //
        _currentActor.SetActive(false);
        _speechBubble.SetActive(false);
        _state1.SetActive(true);
        _scrollView.totalItemCount = 10;
        //_speechBubbleText.text = "欢迎来到游戏！";
    }

    void Update()
    {

    }

    public void OnRunButtonClicked()
    {
        Debug.Log("Run button clicked in HomeScene.");
    }

    public void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked in HomeScene.");
    }

    // IStringGameEventListener 接口实现
    public void OnEventRaised(string actorName)
    {
        Debug.Log($"[HomeScene] Actor clicked: {actorName}");

        // 在这里处理 Actor 被点击的逻辑
        _currentActor.SetActive(true);
        _speechBubble.SetActive(true);
        _speechBubbleText.text = $"你选择了: {actorName}";
    }
}
