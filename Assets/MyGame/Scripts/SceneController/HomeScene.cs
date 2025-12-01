using UnityEngine;

public class HomeScene : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject _background;
    [SerializeField] private GameObject _currentActor;
    [SerializeField] private GameObject _speechBubble;
    [SerializeField] private GameObject _state1;
   
    [Header("Scene Config")]
    [SerializeField] private HomeSceneConfig _homeSceneConfig;

    [Header("API Components")]
    [SerializeField] private HomeGamePlayApi _homeGamePlayApi;


    void Start()
    {
        Debug.Assert(_background != null, "_background is null");
        Debug.Assert(_currentActor != null, "_currentActor is null");
        Debug.Assert(_speechBubble != null, "_speechBubble is null");
        Debug.Assert(_homeSceneConfig != null, "_homeSceneConfig is null");
        Debug.Assert(_homeGamePlayApi != null, "_homeGamePlayApi is null");
        Debug.Assert(_state1 != null, "_state1 is null");
    }

    void Update()
    {

    }

    public void OnRunButtonClicked()
    {
        Debug.Log("Run button clicked in HomeScene.");
        //StartCoroutine(StartGameFlow(_playerIdentifier, _gameName, _actorName));
    }

    public void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked in HomeScene.");
        //StartCoroutine(StartGameFlow(_playerIdentifier, _gameName, _actorName));
    }
}
