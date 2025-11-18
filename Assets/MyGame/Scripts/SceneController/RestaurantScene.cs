using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class RestaurantScene : MonoBehaviour
{
    public string _preScene = "MainScene2";
    public GameObject _backgroundImage;
    public SpriteRenderer _templateActor;
    public float _spacingOffset = 0.5f; // 额外间距（可调整）
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

        List<string> actors = new List<string>();
        bool hasMapping = GameContext.Instance.Mapping.TryGetValue(GameContext.RestaurantName, out actors);
        if (!hasMapping || actors == null || actors.Count == 0 || GameContext.Instance.Root == null)
        {
            Debug.LogWarning($"GameContext is not set up. hasMapping={hasMapping}, actors={(actors == null ? "null" : actors.Count.ToString())}, Root={(GameContext.Instance.Root == null ? "null" : "exists")}. Using debug actors.");
            actors = new List<string>
            {
                GameContext.WarriorName,
                GameContext.WizardName
            };
        }

        // 创建精灵（自动根据尺寸计算位置）
        _createdSprites = InstantiateAndPositionSprites(actors);

        // 隐藏原始的sampleSprite，因为第一个创建的精灵会覆盖它的位置
        _templateActor.gameObject.SetActive(false);

        // 通知服务器场景转换到餐馆
        StartCoroutine(ExecuteTransRestaurant());
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

        if (GameContext.Instance.Root != null)
        {
            Debug.Log("Returning to MainScene2");
            SceneManager.LoadScene(_preScene);
        }
        else
        {
            Debug.LogWarning("Game is not set up. Staying in RestaurantScene.");
        }
    }

    /// <summary>
    /// 通知服务器场景转换到餐馆
    /// </summary>
    private IEnumerator ExecuteTransRestaurant()
    {
        yield return _homeGamePlayAction.Call(
            GameContext.Instance.HomeGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "/trans_home",
            new Dictionary<string, string>
            {
                ["stage_name"] = GameContext.RestaurantName
            });

        Debug.Log("场景转换至餐馆");
        
        if (_homeGamePlayAction.ResponseData == null)
        {
            Debug.LogError("TransRestaurant request failed");
            yield break;
        }

        // 处理服务器返回的消息
        GameContext.Instance.ProcessClientMessages(_homeGamePlayAction.ResponseData.client_messages);

        string joinedLogs = string.Join("\n", GameContext.Instance.AgentEventLogs);
        Debug.Log(joinedLogs);

        // 显示对话
        DisplayAllDialogues();
    }

    /// <summary>
    /// 创建精灵到当前场景，从左到右排列且不重叠
    /// </summary>
    /// <param name="actors">角色名称列表</param>
    private List<GameObject> InstantiateAndPositionSprites(List<string> actors)
    {
        // 根据传入的路径创建精灵
        var sprites = new List<GameObject>();
        for (int i = 0; i < actors.Count; i++)
        {
            GameObject sprite = CreateSprite(actors[i], TextureManager.Instance.GetSprite(actors[i]));
            sprites.Add(sprite);
        }

        // 使用提取的函数来计算和设置精灵位置
        PositionSpritesFromLeftToRight(sprites, _templateActor.transform.position.x, _spacingOffset);

        Debug.Log($"Created {sprites.Count} sprites positioned from left to right");

        return sprites;
    }

    /// <summary>
    /// 根据精灵的实际尺寸动态计算位置，从指定起始位置开始从左到右排列
    /// </summary>
    /// <param name="sprites">需要排列的精灵列表</param>
    /// <param name="startX">起始X坐标位置</param>
    /// <param name="spacingOffset">精灵间的额外间距</param>
    private void PositionSpritesFromLeftToRight(List<GameObject> sprites, float startX, float spacingOffset)
    {
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogWarning("PositionSpritesFromLeftToRight: sprites list is null or empty");
            return;
        }

        float currentX = startX; // 当前X位置，从起始位置开始

        for (int i = 0; i < sprites.Count; i++)
        {
            if (sprites[i] == null)
            {
                Debug.LogWarning($"PositionSpritesFromLeftToRight: sprite at index {i} is null, skipping");
                continue;
            }

            // 获取当前精灵的SpriteRenderer
            SpriteRenderer spriteRenderer = sprites[i].GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"PositionSpritesFromLeftToRight: sprite {sprites[i].name} has no SpriteRenderer, skipping");
                continue;
            }

            // 获取精灵的实际宽度
            float spriteWidth = spriteRenderer.size.x;

            // 计算精灵的中心位置（当前X + 精灵宽度的一半）
            float centerX = currentX + spriteWidth / 2f;

            // 设置精灵位置（保持原有的Y和Z坐标）
            Vector3 originalPosition = sprites[i].transform.position;
            Vector3 newPosition = new Vector3(centerX, originalPosition.y, originalPosition.z);
            sprites[i].transform.position = newPosition;

            Debug.Log($"Set {sprites[i].name} (width: {spriteWidth}) position to: {newPosition}");

            // 更新下一个精灵的起始位置（当前精灵右边缘 + 间距）
            currentX += spriteWidth + spacingOffset;
        }
    }

    /// <summary>
    /// 从图片资源创建精灵（复制sampleSprite的设置），仅负责外观
    /// </summary>
    /// <param name="spriteName">精灵名称</param>
    /// <param name="cachedSprite">缓存的精灵资源</param>
    /// <returns>创建的GameObject</returns>
    private GameObject CreateSprite(string spriteName, Sprite cachedSprite)
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
        Texture2D texture = cachedSprite.texture;
        if (texture != null)
        {
            // 从纹理创建精灵，保持与原始sampleSprite相同的设置
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // 只替换sprite，保持所有其他设置不变
            spriteRenderer.sprite = newSprite;
        }
        else
        {
            // 如果加载失败，创建一个简单的替代纹理
            Texture2D fallbackTexture = MyUtils.CreateSimpleTexture(64, 64, Color.gray);
            Sprite fallbackSprite = Sprite.Create(fallbackTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            spriteRenderer.sprite = fallbackSprite;
        }

        Debug.Log($"Final {spriteName} settings: drawMode={spriteRenderer.drawMode}, size={spriteRenderer.size}");

        return spriteObject;
    }

    /// <summary>
    /// InputField (TMP) - On Value Changed 事件处理器
    /// </summary>
    /// <param name="value">输入字段的当前值</param>
    public void OnInputFieldValueChanged(string value)
    {
        Debug.Log($"InputField value changed: {value}");
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
    }

    /// <summary>
    /// InputField (TMP) - On Select 事件处理器
    /// </summary>
    /// <param name="value">输入字段被选中时的值</param>
    public void OnInputFieldSelect(string value)
    {
        Debug.Log($"InputField selected: {value}");
    }

    /// <summary>
    /// InputField (TMP) - On Deselect 事件处理器
    /// </summary>
    /// <param name="value">输入字段被取消选中时的值</param>
    public void OnInputFieldDeselect(string value)
    {
        Debug.Log($"InputField deselected: {value}");
    }

    private GameObject GetCurrentSprite()
    {
        return GetCreatedSprite(_currentSpriteName);
    }

    private GameObject GetCreatedSprite(string name)
    {
        return _createdSprites.FirstOrDefault(sprite => sprite.name == name);
    }

    public void OnClickSendMessage()
    {
        Debug.Log("Send Message button clicked");
        if (GameContext.Instance.Root != null && !string.IsNullOrEmpty(_currentSpriteName) && !string.IsNullOrEmpty(_inputField.text))
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
        yield return _homeGamePlayAction.Call(
            GameContext.Instance.HomeGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "/speak", new Dictionary<string, string>
            {
                ["target"] = target,
                ["content"] = content
            });

        Debug.Log("Speak action executed");
        if (_homeGamePlayAction.ResponseData == null)
        {
            Debug.LogError("RunHomeAction request failed");
            yield break;
        }

        GameContext.Instance.ProcessClientMessages(_homeGamePlayAction.ResponseData.client_messages);

        HideInputBackground();

        string joinedLogs = string.Join("\n", GameContext.Instance.AgentEventLogs);
        Debug.Log(joinedLogs);

        DisplayAllDialogues();
    }

    /// <summary>
    /// 创建测试用的UI对话泡泡（使用预制体）
    /// </summary>
    /// <param name="targetSprite">目标精灵</param>
    /// <param name="message">对话内容</param>
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

        // 动态计算部分：使用提取的坐标转换函数
        Vector2 canvasPos = MyUtils.ConvertSpriteToCanvasPosition(targetSprite, _canvas, _mainCamera, 1.5f);

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
        yield return _homeGamePlayAction.Call(
            GameContext.Instance.HomeGameplayUrl,
            GameContext.Instance.UserName,
            GameContext.Instance.GameName,
            "/advancing");
        
        if (_homeGamePlayAction.ResponseData == null)
        {
            Debug.LogError("RunHomeAction request failed");
            yield break;
        }

        GameContext.Instance.ProcessClientMessages(_homeGamePlayAction.ResponseData.client_messages);

        string joinedLogs = string.Join("\n", GameContext.Instance.AgentEventLogs);
        Debug.Log(joinedLogs);

        // 一次性显示所有对话
        DisplayAllDialogues();
    }

    /// <summary>
    /// 一次性显示所有对话事件的语音泡泡
    /// </summary>
    private void DisplayAllDialogues()
    {
        Dictionary<string, List<string>> speakerLastDialogue = new Dictionary<string, List<string>>();
        for (int i = 0; i < GameContext.Instance.AgentEvents.Count; i++)
        {
            switch ((EventHead)GameContext.Instance.AgentEvents[i].head)
            {
                case EventHead.NONE:
                    var message = GameContext.Instance.AgentEvents[i].message;
                    Debug.Log("NONE: " + message);
                    break;

                case EventHead.SPEAK_EVENT:
                    SpeakEvent speakEvent = (SpeakEvent)GameContext.Instance.AgentEvents[i];
                    Debug.Log($"SPEAK_EVENT: {speakEvent.actor} => {speakEvent.target}: {speakEvent.content}");
                    if (!speakerLastDialogue.ContainsKey(speakEvent.actor))
                    {
                        speakerLastDialogue[speakEvent.actor] = new List<string>();
                    }
                    speakerLastDialogue[speakEvent.actor].Add($"(speak) @{speakEvent.target} {speakEvent.content}");
                    break;

                case EventHead.WHISPER_EVENT:
                    WhisperEvent whisperEvent = (WhisperEvent)GameContext.Instance.AgentEvents[i];
                    Debug.Log($"WHISPER_EVENT: {whisperEvent.actor} => {whisperEvent.target}: {whisperEvent.content}");
                    if (!speakerLastDialogue.ContainsKey(whisperEvent.actor))
                    {
                        speakerLastDialogue[whisperEvent.actor] = new List<string>();
                    }
                    speakerLastDialogue[whisperEvent.actor].Add($"(whisper) @{whisperEvent.target} {whisperEvent.content}");
                    break;

                case EventHead.ANNOUNCE_EVENT:
                    AnnounceEvent announceEvent = (AnnounceEvent)GameContext.Instance.AgentEvents[i];
                    Debug.Log($"ANNOUNCE_EVENT: {announceEvent.actor} from {announceEvent.stage}: {announceEvent.content}");
                    if (!speakerLastDialogue.ContainsKey(announceEvent.actor))
                    {
                        speakerLastDialogue[announceEvent.actor] = new List<string>();
                    }
                    speakerLastDialogue[announceEvent.actor].Add($"(announce) [{announceEvent.stage}] {announceEvent.content}");
                    break;

                case EventHead.MIND_EVENT:
                    MindEvent mindVoiceEvent = (MindEvent)GameContext.Instance.AgentEvents[i];
                    Debug.Log($"MIND_VOICE_EVENT: {mindVoiceEvent.actor}: {mindVoiceEvent.content}");
                    if (!speakerLastDialogue.ContainsKey(mindVoiceEvent.actor))
                    {
                        speakerLastDialogue[mindVoiceEvent.actor] = new List<string>();
                    }
                    speakerLastDialogue[mindVoiceEvent.actor].Add($"(mind) {mindVoiceEvent.content}");
                    break;

                case EventHead.TRANS_STAGE_EVENT:
                    AgentEvent transStageEvent = (AgentEvent)GameContext.Instance.AgentEvents[i];
                    Debug.Log($"TRANS_STAGE_EVENT: {transStageEvent.message}");
                    break;

                case EventHead.COMBAT_KICK_OFF_EVENT:
                case EventHead.COMBAT_COMPLETE_EVENT:
                case EventHead.DISCUSSION_EVENT:
                case EventHead.NIGHT_ACTION_EVENT:
                case EventHead.VOTE_EVENT:
                case EventHead.QUERY_EVENT:
                    // 这些事件类型在餐厅场景中不需要显示对话泡泡
                    break;

                default:
                    Debug.LogWarning("Unknown agent event head: " + GameContext.Instance.AgentEvents[i].head);
                    break;
            }
        }

        if (speakerLastDialogue.Count > 0)
        {
            foreach (var kvp in speakerLastDialogue)
            {
                string speaker = kvp.Key;
                List<string> dialogues = kvp.Value;
                string combinedDialogue = string.Join("\n", dialogues);
                DisplaySpeechBubbleAtTarget(GetCreatedSprite(speaker), combinedDialogue);
            }
        }
    }
}
