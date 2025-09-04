using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CampScene : MonoBehaviour
{
    public string _preScene = "MainScene2";
    public GameObject _backgroundImage;
    public SpriteRenderer _templateActor;
    public float _spacingOffset = 2.0f; // 额外间距（可调整）
    public GameObject _inputBackground;
    public TMP_InputField _inputField;

    public HomeGamePlayAction _homeGamePlayAction;

    private List<GameObject> _createdSprites;

    private string _currentSpriteName;

    void Start()
    {
        Debug.Assert(_templateActor != null, "sampleSprite is null");
        Debug.Assert(_backgroundImage != null, "background is null");
        Debug.Assert(_inputBackground != null, "inputBackground is null");
        Debug.Assert(_inputField != null, "inputField is null");
        Debug.Assert(_homeGamePlayAction != null, "_homeRunAction is null");

        // 隐藏输入背景
        _inputBackground.SetActive(false);

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


        if (GameContext.Instance.ImagePath.TryGetValue("角色.战士.卡恩", out var warriorPath))
        {
            imagePaths.Add("角色.战士.卡恩", warriorPath);
        }

        if (GameContext.Instance.ImagePath.TryGetValue("角色.法师.奥露娜", out var wizardPath))
        {
            imagePaths.Add("角色.法师.奥露娜", wizardPath);
        }

        return imagePaths;
    }

    public void OnClickBack()
    {
        Debug.Log("Back button clicked");
        StartCoroutine(ReturnToMainScene());
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
            _inputBackground.SetActive(false);
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
            Texture2D fallbackTexture = CreateSimpleTexture(64, 64, Color.gray);
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
    /// 创建简单的纯色纹理
    /// </summary>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="color">颜色</param>
    /// <returns>纹理</returns>
    private Texture2D CreateSimpleTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
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

        string logContent = MyUtils.AgentLogsDisplayText(GameContext.Instance.AgentEventLogs);
        Debug.Log(logContent);
    }
}