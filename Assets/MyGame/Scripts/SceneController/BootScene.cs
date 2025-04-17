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
        StartCoroutine(ConfigureAndLoadApiEndpoints());
    }

    void Update()
    {
    }

    public void OnClickStartGame()
    {
        //Debug.Log("OnClickStartGame");
        if (!_isInitialized)
        {
            Debug.LogError("Game is not initialized");
            return;
        }
        //Debug.Log("Game is initialized");
        StartCoroutine(NextScene());
    }

    private IEnumerator ConfigureAndLoadApiEndpoints()
    {
        yield return StartCoroutine(_apiEndpointConfigurationAction.Request(_gameConfig.LocalNet));
        if (_apiEndpointConfigurationAction.Success)
        {
            _isInitialized = true;
            _mainText.text = JsonConvert.SerializeObject(GameContext.Instance.ApiEndpointConfiguration);
            yield break;
        }

        yield return StartCoroutine(_apiEndpointConfigurationAction.Request(_gameConfig.LocalHost));
        if (_apiEndpointConfigurationAction.Success)
        {
            _isInitialized = true;
            _mainText.text = JsonConvert.SerializeObject(GameContext.Instance.ApiEndpointConfiguration);
            yield break;
        }

        Debug.LogError("Failed to load API routes");
        _mainText.text = "Failed to load API routes";
    }

    private IEnumerator NextScene()
    {
        Debug.Log("NextScene");
        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene(_nextScene);
    }

}
