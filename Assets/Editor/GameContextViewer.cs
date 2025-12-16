using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// GameContext数据查看器
/// 用于在Unity编辑器中查看GameContext单例的所有数据
/// 通过菜单 Tools/Game Context Viewer 打开
/// </summary>
public class GameContextViewer : EditorWindow
{
    private Vector2 scrollPosition;
    private bool showUserInfo = true;
    private bool showUrls = false;
    private bool showStageMapping = true;
    private bool showActorEntities = false;
    private bool showStageEntities = false;
    private bool showDungeonData = false;
    private bool showAgentEvents = true;

    [MenuItem("Tools/Game Context Viewer")]
    public static void ShowWindow()
    {
        GetWindow<GameContextViewer>("Game Context Viewer");
    }

    private void OnGUI()
    {
        // 检查GameContext实例是否存在
        if (GameContext.Instance == null)
        {
            EditorGUILayout.HelpBox("GameContext实例不存在", UnityEditor.MessageType.None);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawUserInfo();
        DrawUrls();
        DrawStageMapping();
        DrawActorEntities();
        DrawStageEntities();
        DrawDungeonData();
        DrawAgentEvents();

        EditorGUILayout.EndScrollView();

        // 刷新按钮
        EditorGUILayout.Space(10);
        if (GUILayout.Button("刷新数据", GUILayout.Height(30)))
        {
            Repaint();
        }

        // 清除按钮
        if (GUILayout.Button("清除GameContext", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("确认", "确定要清除GameContext实例吗?", "确定", "取消"))
            {
                GameContext.ClearInstance();
                Repaint();
            }
        }
    }

    private void DrawUserInfo()
    {
        showUserInfo = EditorGUILayout.Foldout(showUserInfo, "用户信息", true);
        if (showUserInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("用户名", GameContext.Instance.UserName ?? "未设置");
            EditorGUILayout.LabelField("游戏名称", GameContext.Instance.GameName ?? "未设置");
            EditorGUILayout.LabelField("角色名称", GameContext.Instance.ActorName ?? "未设置");
            EditorGUILayout.LabelField("最后序列ID", GameContext.Instance.LastSequenceId.ToString());
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
    }

    private void DrawUrls()
    {
        showUrls = EditorGUILayout.Foldout(showUrls, "API端点", true);
        if (showUrls)
        {
            EditorGUI.indentLevel++;
            try
            {
                DrawLabelWithCopy("Login URL", GameContext.Instance.LoginUrl);
                DrawLabelWithCopy("Logout URL", GameContext.Instance.LogoutUrl);
                DrawLabelWithCopy("Home GamePlay URL", GameContext.Instance.HomeGamePlayUrl);
                DrawLabelWithCopy("Stages State URL", GameContext.Instance.StagesStateUrl);
                DrawLabelWithCopy("Dungeon State URL", GameContext.Instance.DungeonStateUrl);
                DrawLabelWithCopy("Entity Details URL", GameContext.Instance.EntityDetailsUrl);
                DrawLabelWithCopy("Start URL", GameContext.Instance.StartUrl);
                DrawLabelWithCopy("Home Trans Dungeon URL", GameContext.Instance.HomeTransDungeonUrl);
                DrawLabelWithCopy("Dungeon GamePlay URL", GameContext.Instance.DungeonGamePlayUrl);
                DrawLabelWithCopy("Dungeon Trans Home URL", GameContext.Instance.DungeonTransHomeUrl);
                DrawLabelWithCopy("Session Messages URL", GameContext.Instance.SessionMessagesUrl);
            }
            catch (System.Exception e)
            {
                EditorGUILayout.HelpBox($"获取URL失败: {e.Message}", UnityEditor.MessageType.None);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
    }

    private void DrawStageMapping()
    {
        showStageMapping = EditorGUILayout.Foldout(showStageMapping, $"场景-角色映射 (场景数: {GameContext.Instance.StageActorMapping.Count})", true);
        if (showStageMapping)
        {
            EditorGUI.indentLevel++;
            foreach (var kvp in GameContext.Instance.StageActorMapping)
            {
                EditorGUILayout.LabelField($"场景: {kvp.Key}", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (var actor in kvp.Value)
                {
                    EditorGUILayout.LabelField($"→ {actor}");
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(3);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
    }

    private void DrawActorEntities()
    {
        showActorEntities = EditorGUILayout.Foldout(showActorEntities, $"角色实体序列化 (数量: {GameContext.Instance.ActorEntitiesSerialization.Count})", true);
        if (showActorEntities)
        {
            EditorGUI.indentLevel++;
            foreach (var entity in GameContext.Instance.ActorEntitiesSerialization)
            {
                DrawEntitySerialization(entity);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
    }

    private void DrawStageEntities()
    {
        showStageEntities = EditorGUILayout.Foldout(showStageEntities, $"场景实体序列化 (数量: {GameContext.Instance.StageEntitiesSerialization.Count})", true);
        if (showStageEntities)
        {
            EditorGUI.indentLevel++;
            foreach (var entity in GameContext.Instance.StageEntitiesSerialization)
            {
                DrawEntitySerialization(entity);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
    }

    private void DrawEntitySerialization(EntitySerialization entity)
    {
        if (entity == null) return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("名称", entity.name ?? "未知", EditorStyles.boldLabel);

        if (entity.components != null && entity.components.Count > 0)
        {
            EditorGUILayout.LabelField($"组件数量: {entity.components.Count}", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            foreach (var component in entity.components)
            {
                EditorGUILayout.BeginVertical("helpbox");
                EditorGUILayout.LabelField($"组件: {component.name}", EditorStyles.boldLabel);
                
                if (component.data != null && component.data.Count > 0)
                {
                    try
                    {
                        string componentDataJson = JsonConvert.SerializeObject(component.data, Formatting.Indented);
                        EditorGUI.indentLevel++;
                        
                        // 使用可滚动的文本区域显示JSON数据
                        EditorGUILayout.LabelField("数据:", EditorStyles.miniBoldLabel);
                        EditorGUILayout.TextArea(componentDataJson, GUILayout.MinHeight(100), GUILayout.MaxHeight(200));
                        
                        EditorGUI.indentLevel--;
                    }
                    catch (System.Exception ex)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField($"序列化失败: {ex.Message}", EditorStyles.centeredGreyMiniLabel);
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("(无数据)", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);
    }

    private void DrawDungeonData()
    {
        showDungeonData = EditorGUILayout.Foldout(showDungeonData, "地牢数据", true);
        if (showDungeonData)
        {
            EditorGUI.indentLevel++;
            if (GameContext.Instance.Dungeon != null)
            {
                EditorGUILayout.BeginVertical("box");
                try
                {
                    string dungeonJson = JsonConvert.SerializeObject(GameContext.Instance.Dungeon, Formatting.Indented);
                    EditorGUILayout.TextArea(dungeonJson, GUILayout.MinHeight(100));
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.HelpBox($"无法序列化地牢数据: {e.Message}", UnityEditor.MessageType.None);
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("地牢数据为空", UnityEditor.MessageType.None);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
    }

    private void DrawAgentEvents()
    {
        var allEvents = GameContext.Instance.AgentEventsHistory;
        int totalEvents = 0;
        foreach (var kvp in allEvents)
        {
            totalEvents += kvp.Value.Count;
        }

        showAgentEvents = EditorGUILayout.Foldout(showAgentEvents, $"代理事件历史 (角色数: {allEvents.Count}, 总事件数: {totalEvents})", true);
        if (showAgentEvents)
        {
            EditorGUI.indentLevel++;

            // 显示最后一轮事件
            var lastRound = GameContext.Instance.LastAgentEventsHistory;
            if (lastRound.Count > 0)
            {
                EditorGUILayout.LabelField("最后一轮事件:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (var kvp in lastRound)
                {
                    EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value.Count} 个事件");
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(3);
            }

            // 显示所有历史事件
            EditorGUILayout.LabelField("所有历史事件:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            foreach (var kvp in allEvents)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"角色: {kvp.Key} (事件数: {kvp.Value.Count})", EditorStyles.boldLabel);
                
                EditorGUI.indentLevel++;
                foreach (var agentEvent in kvp.Value)
                {
                    DrawAgentEvent(agentEvent);
                }
                EditorGUI.indentLevel--;
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
            }
            EditorGUI.indentLevel--;

            // 清除事件历史按钮
            EditorGUILayout.Space(5);
            if (GUILayout.Button("清除事件历史"))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要清除所有事件历史吗?", "确定", "取消"))
                {
                    GameContext.Instance.ClearAgentEventsHistory();
                    Repaint();
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
    }

    private void DrawAgentEvent(AgentEvent agentEvent)
    {
        if (agentEvent == null) return;

        EditorGUILayout.BeginVertical("helpbox");
        
        string eventType = ((EventHead)agentEvent.head).ToString();
        EditorGUILayout.LabelField("事件类型", eventType, EditorStyles.miniLabel);

        try
        {
            string eventJson = JsonConvert.SerializeObject(agentEvent, Formatting.Indented);
            EditorGUILayout.TextArea(eventJson, EditorStyles.wordWrappedLabel, GUILayout.MaxHeight(100));
        }
        catch (System.Exception e)
        {
            EditorGUILayout.HelpBox($"无法序列化事件: {e.Message}", UnityEditor.MessageType.None);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    private void DrawLabelWithCopy(string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, value ?? "未设置");
        if (!string.IsNullOrEmpty(value) && GUILayout.Button("复制", GUILayout.Width(50)))
        {
            EditorGUIUtility.systemCopyBuffer = value;
        }
        EditorGUILayout.EndHorizontal();
    }

    // 自动刷新（可选）
    private void OnInspectorUpdate()
    {
        // 每0.5秒刷新一次
        Repaint();
    }
}
