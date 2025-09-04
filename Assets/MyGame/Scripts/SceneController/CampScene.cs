using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
//using Newtonsoft.Json;

public class CampScene : MonoBehaviour
{
    public string _preScene = "MainScene2";
    public GameObject _backgroundImage;
    public SpriteRenderer _templateActor;
    public float _spacingOffset = 2.0f; // 额外间距（可调整）
    public GameObject _inputBackground;
    public TMP_InputField _inputField;
    public GameObject _speechBubblePrefab;
    public float _hideBubbleDuration = 5.0f;
    public HomeGamePlayAction _homeGamePlayAction;
    private List<GameObject> _createdSprites;
    private string _currentSpriteName;
    // UI系统组件
    private Canvas _canvas;
    private Camera _mainCamera;

    void Start()
    {
        Debug.Assert(_templateActor != null, "sampleSprite is null");
        Debug.Assert(_backgroundImage != null, "background is null");
        Debug.Assert(_inputBackground != null, "inputBackground is null");
        Debug.Assert(_inputField != null, "inputField is null");
        Debug.Assert(_homeGamePlayAction != null, "_homeRunAction is null");
        Debug.Assert(_speechBubblePrefab != null, "_speechBubblePrefab is null");

        // 隐藏输入背景
        HideInputBackground();

        // 创建对话泡泡
        _speechBubblePrefab.SetActive(false);

        // 初始化UI系统组件
        _canvas = FindFirstObjectByType<Canvas>();
        Debug.Assert(_canvas != null, "Canvas not found in scene");
        _mainCamera = Camera.main;
        if (_mainCamera == null)
            _mainCamera = FindFirstObjectByType<Camera>();
        Debug.Assert(_mainCamera != null, "Main Camera not found in scene");

        // 获取复制过来的点击处理器组件
        SpriteClickHandler clickHandler = _backgroundImage.GetComponent<SpriteClickHandler>();
        if (clickHandler != null)
        {
            // 订阅点击事件
            clickHandler.OnSpriteClicked += OnSpriteClicked;
        }

        // 创建精灵（自动根据尺寸计算位置）
        _createdSprites = CreateSprites(ParseImagePaths());

        // 隐藏原始的sampleSprite，因为第一个创建的精灵会覆盖它的位置
        _templateActor.gameObject.SetActive(false);

        // 测试：为第一个精灵创建对话泡泡
        if (_createdSprites.Count > 0)
        {
            //DisplaySpeechBubbleAtTarget(_createdSprites[0], "Hello World! This is a speech bubble using prefab!");
        }
    }

    void Update()
    {

    }

    private Dictionary<string, string> ParseImagePaths()
    {
        if (!GameContext.Instance.SetupGame)
        {
            Debug.LogWarning("GameContext is not set up. Using debug image paths.");
            return ParseDebugImagePaths();
        }

        return ParseActorImagePaths();
    }

    /// <summary>
    /// 从GameContext的Mapping中解析角色图片路径
    /// </summary>
    /// <returns>图片路径列表</returns>
    private Dictionary<string, string> ParseActorImagePaths()
    {
        var imagePaths = new Dictionary<string, string>();

        List<string> actors = new List<string>();
        GameContext.Instance.Mapping.TryGetValue(GameContext.CampName, out actors);
        for (int i = 0; i < actors.Count; i++)
        {
            string actor = actors[i];
            // 在这里处理每个actor，例如打印或存储
            Debug.Log($"Found actor in {GameContext.CampName}: {actor}");

            if (GameContext.Instance.ImagePath.TryGetValue(actor, out var imagePath))
            {
                Debug.Log($"Found image path for {actor}: {imagePath}");
                imagePaths.Add(actor, imagePath);
            }
        }

        return imagePaths;
    }

    private Dictionary<string, string> ParseDebugImagePaths()
    {
        var imagePaths = new Dictionary<string, string>();


        if (GameContext.Instance.ImagePath.TryGetValue(GameContext.WarriorName, out var warriorPath))
        {
            imagePaths.Add(GameContext.WarriorName, warriorPath);
        }

        if (GameContext.Instance.ImagePath.TryGetValue(GameContext.WizardName, out var wizardPath))
        {
            imagePaths.Add(GameContext.WizardName, wizardPath);
        }

        return imagePaths;
    }

    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        StartCoroutine(ReturnToMainScene());
    }

    public void OnClickNext()
    {
        Debug.Log("Next button clicked");
        StartCoroutine(ExecuteHomeAdvancing());
    }

    /// <summary>
    /// 处理精灵点击事件
    /// </summary>
    /// <param name="clickHandler">被点击的精灵的点击处理器</param>
    private void OnSpriteClicked(SpriteClickHandler clickHandler)
    {
        Debug.Log($"精灵 {clickHandler.gameObject.name} 被点击了！");

        if (clickHandler.gameObject == _backgroundImage)
        {
            Debug.Log("Background clicked, ignoring.");
            HideInputBackground();
            _currentSpriteName = string.Empty;
            return;
        }

        if (clickHandler.gameObject.name == GameContext.Instance.ActorName)
        {
            Debug.Log("Clicked on self, ignoring.");
            HideInputBackground();
            _currentSpriteName = string.Empty;
            return;
        }

        // 查看 clickHandler.gameObject 遍历 _createdSprites，如果是在其中就进行 
        foreach (var sprite in _createdSprites)
        {
            if (clickHandler.gameObject == sprite)
            {
                Debug.Log($"Clicked on created sprite: {sprite.name}");
                // 在这里添加对点击的精灵的处理逻辑
                _inputBackground.SetActive(true);
                _inputField.text = "";
                _currentSpriteName = sprite.name;
                break;
            }
        }
    }

    IEnumerator ReturnToMainScene()
    {
        yield return new WaitForSeconds(0);

        if (GameContext.Instance.SetupGame)
        {
            Debug.Log("Returning to MainScene2");
            SceneManager.LoadScene(_preScene);
        }
        else
        {
            Debug.LogWarning("Game is not set up. Staying in CampScene.");
        }
    }

    /// <summary>
    /// 创建精灵到当前场景，从左到右排列且不重叠
    /// </summary>
    /// <param name="imagePaths">图片路径列表</param>
    private List<GameObject> CreateSprites(Dictionary<string, string> imagePaths)
    {
        // 根据传入的路径创建精灵
        var sprites = new List<GameObject>();

        foreach (var kvp in imagePaths)
        {
            string spriteName = kvp.Key; // 使用角色名作为精灵名称
            string imagePath = kvp.Value;

            GameObject sprite = CreateSpriteFromImage(spriteName, imagePath);
            sprites.Add(sprite);
        }

        // 根据精灵的实际尺寸动态计算位置，从sampleSprite的位置开始
        float currentX = _templateActor.transform.position.x; // 当前X位置，初始值为sampleSprite的X位置

        for (int i = 0; i < sprites.Count; i++)
        {
            // 获取当前精灵的SpriteRenderer
            SpriteRenderer spriteRenderer = sprites[i].GetComponent<SpriteRenderer>();

            // 获取精灵的实际宽度
            float spriteWidth = spriteRenderer.size.x;

            // 计算精灵的中心位置（当前X + 精灵宽度的一半）
            float centerX = currentX + spriteWidth / 2f;

            // 设置精灵位置
            Vector3 position = new Vector3(centerX, 0f, 0f);
            sprites[i].transform.position = position;

            Debug.Log($"Set {sprites[i].name} (width: {spriteWidth}) position to: {position}");

            // 更新下一个精灵的起始位置（当前精灵右边缘 + 间距）
            currentX += spriteWidth + _spacingOffset;
        }

        Debug.Log($"Created {sprites.Count} sprites positioned from left to right");

        return sprites;
    }

    /// <summary>
    /// 从图片资源创建精灵（复制sampleSprite的设置），仅负责外观
    /// </summary>
    /// <param name="spriteName">精灵名称</param>
    /// <param name="imagePath">图片完整路径</param>
    /// <returns>创建的GameObject</returns>
    private GameObject CreateSpriteFromImage(string spriteName, string imagePath)
    {
        // 复制sampleSprite的GameObject
        GameObject spriteObject = Instantiate(_templateActor.gameObject);

        // 重命名（位置在外部设置）
        spriteObject.name = spriteName;

        // 获取复制过来的点击处理器组件
        SpriteClickHandler clickHandler = spriteObject.GetComponent<SpriteClickHandler>();
        if (clickHandler != null)
        {
            // 订阅点击事件
            clickHandler.OnSpriteClicked += OnSpriteClicked;
        }

        // 获取SpriteRenderer组件（从复制的对象中）
        SpriteRenderer spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();

        Debug.Log($"Copied sprite settings: drawMode={spriteRenderer.drawMode}, size={spriteRenderer.size}");

        // 直接使用传入的完整路径加载图片资源
        Texture2D texture = LoadImageFromPath(imagePath);

        if (texture != null)
        {
            // 从纹理创建精灵，保持与原始sampleSprite相同的设置
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // 只替换sprite，保持所有其他设置不变
            spriteRenderer.sprite = newSprite;

            Debug.Log($"Successfully loaded image from {imagePath} for {spriteName}, Original texture size: {texture.width}x{texture.height}");
        }
        else
        {
            Debug.LogError($"Failed to load image from path: {imagePath}");
            // 如果加载失败，创建一个简单的替代纹理
            Texture2D fallbackTexture = MyUtils.CreateSimpleTexture(64, 64, Color.gray);
            Sprite fallbackSprite = Sprite.Create(fallbackTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            spriteRenderer.sprite = fallbackSprite;
        }

        Debug.Log($"Final {spriteName} settings: drawMode={spriteRenderer.drawMode}, size={spriteRenderer.size}");

        return spriteObject;
    }

    /// <summary>
    /// 从指定路径加载图片
    /// </summary>
    /// <param name="imagePath">图片完整路径</param>
    /// <returns>加载的纹理</returns>
    private Texture2D LoadImageFromPath(string imagePath)
    {
        try
        {
            if (System.IO.File.Exists(imagePath))
            {
                byte[] imageData = System.IO.File.ReadAllBytes(imagePath);
                Texture2D loadedTexture = new Texture2D(2, 2);
                if (loadedTexture.LoadImage(imageData))
                {
                    Debug.Log($"Successfully loaded image from path: {imagePath}");
                    return loadedTexture;
                }
                else
                {
                    Debug.LogError($"Failed to decode image data from: {imagePath}");
                }
            }
            else
            {
                Debug.LogWarning($"Image file not found at path: {imagePath}");
            }

            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading image from {imagePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// InputField (TMP) - On Value Changed 事件处理器
    /// </summary>
    /// <param name="value">输入字段的当前值</param>
    public void OnInputFieldValueChanged(string value)
    {
        Debug.Log($"InputField value changed: {value}");
        // 在这里添加您的值改变处理逻辑
        Debug.Log("OnValueChanged: " + _inputField.text);
        Debug.Log($"你(/speak = @{_currentSpriteName} " + _inputField.text);
    }

    /// <summary>
    /// InputField (TMP) - On End Edit 事件处理器
    /// </summary>
    /// <param name="value">输入字段的最终值</param>
    public void OnInputFieldEndEdit(string value)
    {
        Debug.Log($"InputField end edit: {value}");
        // 在这里添加您的编辑结束处理逻辑
    }

    /// <summary>
    /// InputField (TMP) - On Select 事件处理器
    /// </summary>
    /// <param name="value">输入字段被选中时的值</param>
    public void OnInputFieldSelect(string value)
    {
        Debug.Log($"InputField selected: {value}");
        // 在这里添加您的选中处理逻辑
    }

    /// <summary>
    /// InputField (TMP) - On Deselect 事件处理器
    /// </summary>
    /// <param name="value">输入字段被取消选中时的值</param>
    public void OnInputFieldDeselect(string value)
    {
        Debug.Log($"InputField deselected: {value}");
        // 在这里添加您的取消选中处理逻辑
    }

    private GameObject GetCurrentSprite()
    {
        //return _createdSprites.FirstOrDefault(sprite => sprite.name == _currentSpriteName);
        return GetCreatedSprite(_currentSpriteName);
    }

    private GameObject GetCreatedSprite(string name)
    {
        return _createdSprites.FirstOrDefault(sprite => sprite.name == name);
    }

    public void OnClickSendMessage()
    {
        Debug.Log("Send Message button clicked");
        if (GameContext.Instance.SetupGame && !string.IsNullOrEmpty(_currentSpriteName) && !string.IsNullOrEmpty(_inputField.text))
        {
            if (_currentSpriteName != GameContext.Instance.ActorName)
            {
                StartCoroutine(ExecuteSpeakAction(_currentSpriteName, _inputField.text));
            }
            else
            {
                Debug.LogWarning("Cannot send message to self.");
            }
        }
        else
        {
            Debug.LogWarning("Cannot send message. Ensure game is set up, a sprite is selected, and input field is not empty.");
            DisplaySpeechBubbleAtTarget(GetCurrentSprite(), _inputField.text);

            // 隐藏输入框背景
            HideInputBackground();
        }
    }

    private IEnumerator ExecuteSpeakAction(string target, string content)
    {
        yield return _homeGamePlayAction.Call("/speak", new Dictionary<string, string>
        {
            ["target"] = target,
            ["content"] = content
        });
        Debug.Log("Speak action executed");
        if (!_homeGamePlayAction.LastRequestSuccess)
        {
            Debug.LogError("RunHomeAction request failed");
            yield break;
        }

        HideInputBackground();

        //请注意 List<string> GameContext.AgentEventLogs 的定义，将其用join('\n')连接成字符串
        string joinedLogs = string.Join("\n", GameContext.Instance.AgentEventLogs);
        Debug.Log(joinedLogs);


        for (int i = GameContext.Instance.AgentEvents.Count - 1; i >= 0; i--)
        {
            if (GameContext.Instance.AgentEvents[i].head == (int)AgentEventHead.SPEAK_EVENT)
            {
                SpeakEvent speakEvent = (SpeakEvent)GameContext.Instance.AgentEvents[i];
                DisplaySpeechBubbleAtTarget(GetCreatedSprite(speakEvent.speaker), $"@{speakEvent.listener} {speakEvent.dialogue}");
                break;
            }
        }

        // yield return new WaitForSeconds(0);
        // yield return StartCoroutine(ExecuteHomeAdvancing());
    }

    /// <summary>
    /// 创建测试用的UI对话泡泡（使用预制体）
    /// </summary>
    /// <param name="targetSprite">目标精灵</param>
    private void DisplaySpeechBubbleAtTarget(GameObject targetSprite, string message)
    {
        // 激活预制体
        _speechBubblePrefab.SetActive(true);

        // 获取预制体的RectTransform
        RectTransform rectTransform = _speechBubblePrefab.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("Speech bubble prefab does not have RectTransform component");
            return;
        }

        // 🔥 动态计算部分：使用提取的坐标转换函数
        Vector2 canvasPos = MyUtils.ConvertSpriteToCanvasPosition(targetSprite, _canvas, _mainCamera, 0.5f);

        // 设置UI位置 - 使用动态计算的坐标
        rectTransform.anchoredPosition = canvasPos;

        // 查找并设置文本内容
        TextMeshProUGUI textMesh = _speechBubblePrefab.GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.text = message;
            Debug.Log("Text content set successfully");
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI component not found in speech bubble prefab");
        }

        Debug.Log($"Positioned speech bubble prefab at canvas position {canvasPos}");

        // 延时隐藏泡泡
        //StartCoroutine(HideSpeechBubblePrefabAfterDelay(_hideBubbleDuration));
    }

    /// <summary>
    /// 延时隐藏对话泡泡预制体
    /// </summary>
    /// <param name="delay">延迟时间</param>
    /// <returns></returns>
    private IEnumerator HideSpeechBubblePrefabAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_speechBubblePrefab != null)
        {
            _speechBubblePrefab.SetActive(false);
            Debug.Log("Speech bubble prefab hidden");
        }
    }

    private void HideInputBackground()
    {
        _inputField.text = "";
        _inputBackground.SetActive(false);
    }

    private IEnumerator ExecuteHomeAdvancing()
    {
        yield return _homeGamePlayAction.Call("/advancing");
        if (!_homeGamePlayAction.LastRequestSuccess)
        {
            Debug.LogError("RunHomeAction request failed");
            yield break;
        }
        //
        //请注意 List<string> GameContext.AgentEventLogs 的定义，将其用join('\n')连接成字符串
        string joinedLogs = string.Join("\n", GameContext.Instance.AgentEventLogs);
        Debug.Log(joinedLogs);

        for (int i = GameContext.Instance.AgentEvents.Count - 1; i >= 0; i--)
        {
            if (GameContext.Instance.AgentEvents[i].head == (int)AgentEventHead.SPEAK_EVENT)
            {
                SpeakEvent speakEvent = (SpeakEvent)GameContext.Instance.AgentEvents[i];
                DisplaySpeechBubbleAtTarget(GetCreatedSprite(speakEvent.speaker), $"@{speakEvent.listener} {speakEvent.dialogue}");
                break;
            }
        }
    }
}