using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using Newtonsoft.Json;

public class BootScene : MonoBehaviour
{
    public string _nextScene = "LoginScene";

    public BootAction _bootAction;

    public GameConfig _gameConfig;

    public TMP_Text _mainText;

    private bool _isInitialized = false;

    void Start()
    {
        Debug.Assert(_bootAction != null, "_bootAction is null");
        Debug.Assert(_gameConfig != null, "_gameConfig is null");
        Debug.Assert(_mainText != null, "_mainText is null");
        StartCoroutine(InitializeAndLoadAPIRoutes());
    }

    void Update()
    {
    }

    public void OnClickStartGame()
    {
        Debug.Log("OnClickStartGame");
        if (!_isInitialized)
        {
            Debug.LogError("Game is not initialized");
            return;
        }
        Debug.Log("Game is initialized");
        StartCoroutine(NextScene());
    }

    private IEnumerator InitializeAndLoadAPIRoutes()
    {
        yield return StartCoroutine(_bootAction.Request(_gameConfig.apiEndPoints));
        if (!_bootAction.Success)
        {
            Debug.LogError("BootAction request failed");
            yield break;
        }

        _isInitialized = true;
        _mainText.text = JsonConvert.SerializeObject(GameContext.Instance.ApiEndpointConfiguration);
    }

    private IEnumerator NextScene()
    {
        Debug.Log("NextScene");
        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene(_nextScene);
    }

}
