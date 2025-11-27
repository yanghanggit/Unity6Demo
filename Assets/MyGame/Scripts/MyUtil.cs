//using System;
using System.Collections.Generic;
//using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class MyUtils
{
    public static string MappingDisplayText(Dictionary<string, List<string>> mapping)
    {
        var mapping_text = "";
        foreach (var kvp in mapping)
        {
            mapping_text += kvp.Key + ": ";
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                mapping_text += kvp.Value[i];
                if (i < kvp.Value.Count - 1)
                {
                    mapping_text += ", ";
                }
            }
            mapping_text += "\n";
        }
        return mapping_text;
    }

    public static string DungeonCombatDisplayText(Dungeon dungeon)
    {
        var dungeon_text = "";
        if (dungeon.combat_sequence.combats.Count > 0)
        {
            var last_combat = dungeon.combat_sequence.combats[dungeon.combat_sequence.combats.Count - 1];
            dungeon_text += "Last Combat: " + last_combat.name + "\n";
            dungeon_text += "Phase: " + last_combat.phase.ToString() + "\n";
            dungeon_text += "Result: " + last_combat.result.ToString() + "\n";

            if (last_combat.rounds.Count > 0)
            {
                var last_round = last_combat.rounds[last_combat.rounds.Count - 1];
                dungeon_text += "Last Round: " + last_round.tag + "\n";
                //dungeon_text += "Stage Environment: " + last_round.environment + "\n";
                dungeon_text += "Action Order: " + string.Join("-->", last_round.action_order) + "\n";
            }
        }

        return dungeon_text;
    }

    public static string ActorDisplayText(EntitySerialization actorEntitySerialization, HashSet<string> includedComponentNames = null)
    {
        var ret = "";
        for (int i = 0; i < actorEntitySerialization.components.Count; i++)
        {
            var component = actorEntitySerialization.components[i];
            if (includedComponentNames != null && !includedComponentNames.Contains(component.name))
            {
                continue;
            }

            if (component.name == typeof(CombatStatsComponent).Name)
            {
                var rpgCharacterProfileComponent = JsonConvert.DeserializeObject<CombatStatsComponent>(JsonConvert.SerializeObject(component.data));
                var rpgCharacterProfile = rpgCharacterProfileComponent.stats;
                var rpgCharacterProfileText = $"{actorEntitySerialization.name} = HP:{rpgCharacterProfile.hp}/{rpgCharacterProfile.max_hp}," +
                        $" Strength:{rpgCharacterProfile.strength}," +
                        $" Dexterity:{rpgCharacterProfile.dexterity}," +
                        $" Wisdom:{rpgCharacterProfile.wisdom}," +
                        $" Physical Attack:{rpgCharacterProfile.physical_attack}," +
                        $" Physical Defense:{rpgCharacterProfile.physical_defense}," +
                        $" Magic Attack:{rpgCharacterProfile.magic_attack}," +
                        $" Magic Defense:{rpgCharacterProfile.magic_defense}\n";

                if (rpgCharacterProfileComponent.effects.Count > 0)
                {
                    rpgCharacterProfileText += "Status Effects: ";
                    for (int j = 0; j < rpgCharacterProfileComponent.effects.Count; j++)
                    {
                        var statusEffect = rpgCharacterProfileComponent.effects[j];
                        rpgCharacterProfileText += $"{statusEffect.name} ({statusEffect.description}, {statusEffect.duration})";
                        if (j < rpgCharacterProfileComponent.effects.Count - 1)
                        {
                            rpgCharacterProfileText += ", ";
                        }
                    }
                    rpgCharacterProfileText += "\n";
                }

                ret += rpgCharacterProfileText;
            }
            else if (component.name == typeof(HandComponent).Name)
            {
                var handComponent = JsonConvert.DeserializeObject<HandComponent>(JsonConvert.SerializeObject(component.data));
                var handCompText = $"{actorEntitySerialization.name} Hand: ";
                for (int j = 0; j < handComponent.cards.Count; j++)
                {
                    var card = handComponent.cards[j];
                    handCompText += $"{card.name} ({card.description})";
                    if (j < handComponent.cards.Count - 1)
                    {
                        handCompText += ", ";
                    }
                }
                handCompText += "\n";
                for (int j = 0; j < handComponent.cards.Count; j++)
                {
                    var card = handComponent.cards[j];
                    handCompText += $"Card: {card.name}, Targets: {string.Join(", ", card.targets)}\n";
                }
                ret += handCompText + "\n";
            }
        }

        return ret;
    }

    public static string AgentLogsDisplayText(List<string> agentEventLogs)
    {
        string mainTextUpdater = "";
        for (int i = 0; i < agentEventLogs.Count; i++)
        {
            mainTextUpdater += agentEventLogs[i] + "\n";
        }
        if (mainTextUpdater == "")
        {
            mainTextUpdater = "No logs";
        }
        return mainTextUpdater;
    }

    /// <summary>
    /// 将世界坐标的Sprite位置转换为Canvas UI坐标
    /// </summary>
    /// <param name="targetSprite">目标精灵</param>
    /// <param name="canvas">Canvas组件</param>
    /// <param name="camera">摄像机组件</param>
    /// <param name="offsetY">Y轴偏移量（用于调整泡泡位置）</param>
    /// <returns>Canvas坐标系中的位置</returns>
    public static Vector2 ConvertSpriteToCanvasPosition(GameObject targetSprite, Canvas canvas, Camera camera, float offsetY = 0.5f)
    {
        if (canvas == null || camera == null || targetSprite == null)
        {
            UnityEngine.Debug.LogError("Canvas, Camera or targetSprite is null for coordinate conversion");
            return Vector2.zero;
        }

        // 步骤1：获取精灵的世界坐标位置
        Vector3 spriteWorldPos = targetSprite.transform.position;
        SpriteRenderer spriteRenderer = targetSprite.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            UnityEngine.Debug.LogError("Target sprite does not have SpriteRenderer component");
            return Vector2.zero;
        }

        float spriteHeight = spriteRenderer.bounds.size.y;

        // 步骤2：计算泡泡在精灵头部上方的世界坐标
        Vector3 bubbleWorldPos = new Vector3(
            spriteWorldPos.x,
            spriteWorldPos.y + spriteHeight / 2 + offsetY,
            spriteWorldPos.z
        );

        // 步骤3：世界坐标 → 屏幕坐标
        Vector3 screenPos = camera.WorldToScreenPoint(bubbleWorldPos);

        // 步骤4：屏幕坐标 → Canvas坐标
        // 关键修复：根据Canvas渲染模式选择正确的相机参数
        // Screen Space - Overlay 模式使用 null
        // Screen Space - Camera 或 World Space 模式使用对应的相机
        Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera;

        Vector2 canvasPos;
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvasCamera,
            out canvasPos
        );

        if (!success)
        {
            Debug.LogWarning("Failed to convert screen point to canvas coordinates");
        }

        Debug.Log($"坐标转换: 世界({spriteWorldPos}) → 屏幕({screenPos}) → Canvas({canvasPos}), Canvas模式: {canvas.renderMode}");

        return canvasPos;
    }

    public static Texture2D CreateSimpleTexture(int width, int height, Color color)
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
}

