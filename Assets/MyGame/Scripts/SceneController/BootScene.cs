using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using Newtonsoft.Json;

public class BootScene : MonoBehaviour
{
    public string _nextScene = "LoginScene";

    public ApiEndpointConfigurationAction _apiEndpointConfigurationAction;

    public GameConfig _gameConfig;

    public TMP_Text _mainText;

    private bool _isInitialized = false;

    void Start()
    {
        Debug.Assert(_apiEndpointConfigurationAction != null, "_bootAction is null");
        Debug.Assert(_gameConfig != null, "_gameConfig is null");
        Debug.Assert(_mainText != null, "_mainText is null");
        StartCoroutine(InitializeApiEndpoints());
    }

    public void OnClickNextSceneLogin()
    {
        if (!_isInitialized)
        {
            Debug.LogError("Game is not initialized");
            return;
        }
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator InitializeApiEndpoints()
    {
        yield return StartCoroutine(_apiEndpointConfigurationAction.Call(_gameConfig.LocalNet));
        if (_apiEndpointConfigurationAction.RequestSuccess)
        {
            _isInitialized = true;
            _mainText.text = JsonConvert.SerializeObject(GameContext.Instance.ApiEndpointConfiguration);
            yield break;
        }

        yield return StartCoroutine(_apiEndpointConfigurationAction.Call(_gameConfig.LocalHost));
        if (_apiEndpointConfigurationAction.RequestSuccess)
        {
            _isInitialized = true;
            _mainText.text = JsonConvert.SerializeObject(GameContext.Instance.ApiEndpointConfiguration);
            yield break;
        }

        _mainText.text = "Failed to load API routes";
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene(_nextScene);
    }

}
