using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class BootScene : MonoBehaviour
{
    public string _baseUrl = "http://192.168.2.121:8000/";

    public string _nextScene = "LoginScene";

    public RootApi _rootApi;

    public Button _nextButton;

    void Start()
    {
        Debug.Assert(_rootApi != null, "_rootApi is null");
        Debug.Assert(_nextButton != null, "_nextButton is null");

        _nextButton.gameObject.SetActive(false);
        StartCoroutine(InitializeApiEndpoints());
    }

    public void OnClickNextSceneLogin()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator InitializeApiEndpoints()
    {
        yield return _rootApi.Call(_baseUrl);
        if (_rootApi.RespData != null)
        {
            _nextButton.gameObject.SetActive(true);
            GameContext.Instance.Root = _rootApi.RespData;
            Debug.Log("Using LocalNet for API endpoints");
            yield break;
        }
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene(_nextScene);
    }
}
