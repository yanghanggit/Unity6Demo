using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// GameContext数据查看器 - UI Toolkit版本
/// 使用现代化的UI Toolkit框架替代传统IMGUI
/// 通过菜单 Tools/Game Context Viewer (UI Toolkit) 打开
/// </summary>
public class GameContextViewerUIToolkit : EditorWindow
{
    // UI元素引用
    private Label statusLabel;
    private TextField usernameField;
    private TextField gamenameField;
    private TextField actornameField;
    private TextField sequenceIdField;
    private VisualElement urlsContainer;
    private VisualElement stageMappingContainer;
    private VisualElement actorEntitiesContainer;
    private VisualElement stageEntitiesContainer;
    private TextField dungeonField;
    private VisualElement agentEventsContainer;
    private Foldout agentEventsFoldout;

    [MenuItem("Tools/Game Context Viewer (UI Toolkit)")]
    public static void ShowWindow()
    {
        var window = GetWindow<GameContextViewerUIToolkit>("Game Context Viewer");
        window.minSize = new Vector2(400, 600);
    }

    public void CreateGUI()
    {
        // 加载UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/GameContextViewer.uxml");
        
        if (visualTree != null)
        {
            visualTree.CloneTree(rootVisualElement);
        }
        else
        {
            // 如果UXML文件不存在，创建基础UI
            CreateUIFallback();
        }

        // 获取UI元素引用
        CacheUIElements();

        // 绑定按钮事件
        BindButtons();

        // 初始化数据
        RefreshData();

        // 设置定时刷新
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDestroy()
    {
        // 清理事件订阅
        EditorApplication.update -= OnEditorUpdate;
    }

    private void CreateUIFallback()
    {
        // 如果UXML加载失败，用代码创建基础UI
        var root = new VisualElement();
        root.style.flexGrow = 1;

        var header = new Label("Game Context Viewer");
        header.style.fontSize = 16;
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.paddingTop = 10;
        header.style.paddingBottom = 10;
        root.Add(header);

        statusLabel = new Label("GameContext 状态");
        root.Add(statusLabel);

        var scrollView = new ScrollView();
        scrollView.style.flexGrow = 1;
        root.Add(scrollView);

        var refreshButton = new Button(() => RefreshData()) { text = "刷新数据" };
        refreshButton.style.height = 30;
        root.Add(refreshButton);

        rootVisualElement.Add(root);
    }

    private void CacheUIElements()
    {
        // 缓存常用UI元素引用
        statusLabel = rootVisualElement.Q<Label>("status-label");
        usernameField = rootVisualElement.Q<TextField>("username-field");
        gamenameField = rootVisualElement.Q<TextField>("gamename-field");
        actornameField = rootVisualElement.Q<TextField>("actorname-field");
        sequenceIdField = rootVisualElement.Q<TextField>("sequenceid-field");
        urlsContainer = rootVisualElement.Q<VisualElement>("urls-container");
        stageMappingContainer = rootVisualElement.Q<VisualElement>("stage-mapping-container");
        actorEntitiesContainer = rootVisualElement.Q<VisualElement>("actor-entities-container");
        stageEntitiesContainer = rootVisualElement.Q<VisualElement>("stage-entities-container");
        dungeonField = rootVisualElement.Q<TextField>("dungeon-field");
        agentEventsContainer = rootVisualElement.Q<VisualElement>("agent-events-container");
        agentEventsFoldout = rootVisualElement.Q<Foldout>("agent-events-foldout");
    }

    private void BindButtons()
    {
        var refreshButton = rootVisualElement.Q<Button>("refresh-button");
        if (refreshButton != null)
        {
            refreshButton.clicked += RefreshData;
        }

        var clearEventsButton = rootVisualElement.Q<Button>("clear-events-button");
        if (clearEventsButton != null)
        {
            clearEventsButton.clicked += () =>
            {
                if (EditorUtility.DisplayDialog("确认", "确定要清除所有事件历史吗?", "确定", "取消"))
                {
                    GameContext.Instance?.ClearAgentEventsHistory();
                    RefreshData();
                }
            };
        }

        var clearContextButton = rootVisualElement.Q<Button>("clear-context-button");
        if (clearContextButton != null)
        {
            clearContextButton.clicked += () =>
            {
                if (EditorUtility.DisplayDialog("确认", "确定要清除GameContext实例吗?", "确定", "取消"))
                {
                    GameContext.ClearInstance();
                    RefreshData();
                }
            };
        }
    }

    private float lastUpdateTime;
    private void OnEditorUpdate()
    {
        // 每0.5秒自动刷新一次
        if (EditorApplication.timeSinceStartup - lastUpdateTime > 0.5f)
        {
            lastUpdateTime = (float)EditorApplication.timeSinceStartup;
            RefreshData();
        }
    }

    private void RefreshData()
    {
        if (GameContext.Instance == null)
        {
            if (statusLabel != null)
            {
                statusLabel.text = "GameContext实例不存在";
                statusLabel.style.color = new Color(1f, 0.5f, 0.5f);
            }
            return;
        }

        if (statusLabel != null)
        {
            statusLabel.text = "GameContext 已加载";
            statusLabel.style.color = new Color(0.5f, 1f, 0.5f);
        }

        UpdateUserInfo();
        UpdateUrls();
        UpdateStageMapping();
        UpdateActorEntities();
        UpdateStageEntities();
        UpdateDungeonData();
        UpdateAgentEvents();
    }

    private void UpdateUserInfo()
    {
        if (usernameField != null)
            usernameField.value = GameContext.Instance.UserName ?? "未设置";
        if (gamenameField != null)
            gamenameField.value = GameContext.Instance.GameName ?? "未设置";
        if (actornameField != null)
            actornameField.value = GameContext.Instance.ActorName ?? "未设置";
        if (sequenceIdField != null)
            sequenceIdField.value = GameContext.Instance.LastSequenceId.ToString();
    }

    private void UpdateUrls()
    {
        if (urlsContainer == null) return;

        urlsContainer.Clear();

        var urls = new Dictionary<string, string>
        {
            { "Login URL", GameContext.Instance.LoginUrl },
            { "Logout URL", GameContext.Instance.LogoutUrl },
            { "Home GamePlay URL", GameContext.Instance.HomeGamePlayUrl },
            { "Stages State URL", GameContext.Instance.StagesStateUrl },
            { "Dungeon State URL", GameContext.Instance.DungeonStateUrl },
            { "Entity Details URL", GameContext.Instance.EntityDetailsUrl },
            { "Start URL", GameContext.Instance.StartUrl },
            { "Home Trans Dungeon URL", GameContext.Instance.HomeTransDungeonUrl },
            { "Dungeon GamePlay URL", GameContext.Instance.DungeonGamePlayUrl },
            { "Dungeon Trans Home URL", GameContext.Instance.DungeonTransHomeUrl },
            { "Session Messages URL", GameContext.Instance.SessionMessagesUrl }
        };

        foreach (var kvp in urls)
        {
            var row = CreateUrlRow(kvp.Key, kvp.Value);
            urlsContainer.Add(row);
        }
    }

    private VisualElement CreateUrlRow(string label, string url)
    {
        var row = new VisualElement();
        row.AddToClassList("url-row");

        var labelElement = new Label(label);
        labelElement.AddToClassList("url-label");
        row.Add(labelElement);

        var valueElement = new Label(url ?? "未设置");
        valueElement.AddToClassList("url-value");
        row.Add(valueElement);

        if (!string.IsNullOrEmpty(url))
        {
            var copyButton = new Button(() => EditorGUIUtility.systemCopyBuffer = url)
            {
                text = "复制"
            };
            copyButton.AddToClassList("copy-button");
            row.Add(copyButton);
        }

        return row;
    }

    private void UpdateStageMapping()
    {
        if (stageMappingContainer == null) return;

        stageMappingContainer.Clear();

        var mapping = GameContext.Instance.StageActorMapping;
        var foldout = rootVisualElement.Q<Foldout>("stage-mapping-foldout");
        if (foldout != null)
        {
            foldout.text = $"场景-角色映射 (场景数: {mapping.Count})";
        }

        if (mapping.Count == 0)
        {
            var emptyLabel = new Label("暂无数据");
            emptyLabel.AddToClassList("empty-message");
            stageMappingContainer.Add(emptyLabel);
            return;
        }

        foreach (var kvp in mapping)
        {
            var stageLabel = new Label($"场景: {kvp.Key}");
            stageLabel.AddToClassList("bold-label");
            stageMappingContainer.Add(stageLabel);

            foreach (var actor in kvp.Value)
            {
                var actorLabel = new Label($"  → {actor}");
                actorLabel.AddToClassList("info-label");
                stageMappingContainer.Add(actorLabel);
            }

            stageMappingContainer.Add(new VisualElement { style = { height = 5 } });
        }
    }

    private void UpdateActorEntities()
    {
        if (actorEntitiesContainer == null) return;

        actorEntitiesContainer.Clear();

        var entities = GameContext.Instance.ActorEntitiesSerialization;
        var foldout = rootVisualElement.Q<Foldout>("actor-entities-foldout");
        if (foldout != null)
        {
            foldout.text = $"角色实体序列化 (数量: {entities.Count})";
        }

        if (entities.Count == 0)
        {
            var emptyLabel = new Label("暂无数据");
            emptyLabel.AddToClassList("empty-message");
            actorEntitiesContainer.Add(emptyLabel);
            return;
        }

        foreach (var entity in entities)
        {
            var card = CreateEntityCard(entity);
            actorEntitiesContainer.Add(card);
        }
    }

    private void UpdateStageEntities()
    {
        if (stageEntitiesContainer == null) return;

        stageEntitiesContainer.Clear();

        var entities = GameContext.Instance.StageEntitiesSerialization;
        var foldout = rootVisualElement.Q<Foldout>("stage-entities-foldout");
        if (foldout != null)
        {
            foldout.text = $"场景实体序列化 (数量: {entities.Count})";
        }

        if (entities.Count == 0)
        {
            var emptyLabel = new Label("暂无数据");
            emptyLabel.AddToClassList("empty-message");
            stageEntitiesContainer.Add(emptyLabel);
            return;
        }

        foreach (var entity in entities)
        {
            var card = CreateEntityCard(entity);
            stageEntitiesContainer.Add(card);
        }
    }

    private VisualElement CreateEntityCard(EntitySerialization entity)
    {
        if (entity == null) return new VisualElement();

        var card = new VisualElement();
        card.AddToClassList("entity-card");

        var nameLabel = new Label(entity.name ?? "未知实体");
        nameLabel.AddToClassList("entity-name");
        card.Add(nameLabel);

        if (entity.components != null && entity.components.Count > 0)
        {
            var componentCountLabel = new Label($"组件数量: {entity.components.Count}");
            componentCountLabel.AddToClassList("info-label");
            card.Add(componentCountLabel);

            foreach (var component in entity.components)
            {
                var componentCard = CreateComponentCard(component);
                card.Add(componentCard);
            }
        }
        else
        {
            var emptyLabel = new Label("(无组件)");
            emptyLabel.AddToClassList("empty-message");
            card.Add(emptyLabel);
        }

        return card;
    }

    private VisualElement CreateComponentCard(ComponentSerialization component)
    {
        var card = new VisualElement();
        card.AddToClassList("component-card");

        var nameLabel = new Label($"组件: {component.name}");
        nameLabel.AddToClassList("component-name");
        card.Add(nameLabel);

        if (component.data != null && component.data.Count > 0)
        {
            try
            {
                string json = JsonConvert.SerializeObject(component.data, Formatting.Indented);
                var jsonField = new TextField { value = json, multiline = true, isReadOnly = true };
                jsonField.AddToClassList("json-field");
                card.Add(jsonField);
            }
            catch (System.Exception ex)
            {
                var errorLabel = new Label($"序列化失败: {ex.Message}");
                errorLabel.style.color = new Color(1f, 0.5f, 0.5f);
                card.Add(errorLabel);
            }
        }
        else
        {
            var emptyLabel = new Label("(无数据)");
            emptyLabel.AddToClassList("empty-message");
            card.Add(emptyLabel);
        }

        return card;
    }

    private void UpdateDungeonData()
    {
        if (dungeonField == null) return;

        if (GameContext.Instance.Dungeon != null)
        {
            try
            {
                string json = JsonConvert.SerializeObject(GameContext.Instance.Dungeon, Formatting.Indented);
                dungeonField.value = json;
            }
            catch (System.Exception ex)
            {
                dungeonField.value = $"无法序列化地牢数据: {ex.Message}";
            }
        }
        else
        {
            dungeonField.value = "地牢数据为空";
        }
    }

    private void UpdateAgentEvents()
    {
        if (agentEventsContainer == null) return;

        agentEventsContainer.Clear();

        var allEvents = GameContext.Instance.AgentEventsHistory;
        int totalEvents = 0;
        foreach (var kvp in allEvents)
        {
            totalEvents += kvp.Value.Count;
        }

        if (agentEventsFoldout != null)
        {
            agentEventsFoldout.text = $"代理事件历史 (角色数: {allEvents.Count}, 总事件数: {totalEvents})";
        }

        // 显示最后一轮事件
        var lastRound = GameContext.Instance.LastAgentEventsHistory;
        if (lastRound.Count > 0)
        {
            var lastRoundLabel = new Label("最后一轮事件:");
            lastRoundLabel.AddToClassList("bold-label");
            agentEventsContainer.Add(lastRoundLabel);

            foreach (var kvp in lastRound)
            {
                var eventLabel = new Label($"{kvp.Key}: {kvp.Value.Count} 个事件");
                eventLabel.AddToClassList("info-label");
                agentEventsContainer.Add(eventLabel);
            }

            agentEventsContainer.Add(new VisualElement { style = { height = 10 } });
        }

        // 显示所有历史事件
        if (allEvents.Count > 0)
        {
            var allEventsLabel = new Label("所有历史事件:");
            allEventsLabel.AddToClassList("bold-label");
            agentEventsContainer.Add(allEventsLabel);

            foreach (var kvp in allEvents)
            {
                var actorFoldout = new Foldout
                {
                    text = $"角色: {kvp.Key} (事件数: {kvp.Value.Count})",
                    value = false
                };

                foreach (var agentEvent in kvp.Value)
                {
                    var eventCard = CreateEventCard(agentEvent);
                    actorFoldout.Add(eventCard);
                }

                agentEventsContainer.Add(actorFoldout);
            }
        }
        else
        {
            var emptyLabel = new Label("暂无事件数据");
            emptyLabel.AddToClassList("empty-message");
            agentEventsContainer.Add(emptyLabel);
        }
    }

    private VisualElement CreateEventCard(AgentEvent agentEvent)
    {
        if (agentEvent == null) return new VisualElement();

        var card = new VisualElement();
        card.AddToClassList("event-card");

        string eventType = ((EventHead)agentEvent.head).ToString();
        var typeLabel = new Label($"事件类型: {eventType}");
        typeLabel.AddToClassList("event-type");
        card.Add(typeLabel);

        try
        {
            string json = JsonConvert.SerializeObject(agentEvent, Formatting.Indented);
            var jsonLabel = new Label(json);
            jsonLabel.AddToClassList("event-json");
            card.Add(jsonLabel);
        }
        catch (System.Exception ex)
        {
            var errorLabel = new Label($"无法序列化事件: {ex.Message}");
            errorLabel.style.color = new Color(1f, 0.5f, 0.5f);
            card.Add(errorLabel);
        }

        return card;
    }
}
