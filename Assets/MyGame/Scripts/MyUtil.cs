//using System;
using System.Collections.Generic;
using System.Linq;
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

    public static string DungeonOverviewDisplayText(Dungeon dungeon)
    {
        var dungeon_text = "";
        dungeon_text += "地下城 = " + dungeon.name + "\n";
        for (int i = 0; i < dungeon.levels.Count; i++)
        {
            dungeon_text += "第" + (i + 1) + "关 = " + dungeon.levels[i].name + "\n";
            dungeon_text += "怪物 = " + string.Join(", ", dungeon.levels[i].actors.Select(a => a.name)) + "\n";
        }

        return dungeon_text;
    }

    public static string DungeonCombatDisplayText(Dungeon dungeon)
    {
        var dungeon_text = "";
        if (dungeon.engagement.combats.Count > 0)
        {
            var last_combat = dungeon.engagement.combats[dungeon.engagement.combats.Count - 1];
            dungeon_text += "Last Combat: " + last_combat.name + "\n";
            dungeon_text += "Phase: " + last_combat.phase.ToString() + "\n";
            dungeon_text += "Result: " + last_combat.result.ToString() + "\n";

            if (last_combat.rounds.Count > 0)
            {
                var last_round = last_combat.rounds[last_combat.rounds.Count - 1];
                dungeon_text += "Last Round: " + last_round.tag + "\n";
                dungeon_text += "Stage Environment: " + last_round.stage_environment + "\n";
                dungeon_text += "Round Turn: " + string.Join("-->", last_round.round_turns) + "\n";
            }
        }

        return dungeon_text;
    }

    public static string GetActorLocation(string actor, Dictionary<string, List<string>> mapping)
    {
        // 在 mapping 遍历每一个 key-value 对, 在value中如果存在 actor, 则返回 key
        foreach (var kvp in mapping)
        {
            if (kvp.Value.Contains(actor))
            {
                return kvp.Key;
            }
        }
        return "";
    }

    public static List<string> RetrieveActorsForStage(string actor, Dictionary<string, List<string>> mapping)
    {
        var locateActorStageName = GetActorLocation(actor, mapping);
        if (locateActorStageName == "")
        {
            return new List<string>();
        }
        // 在 mapping 中找到 locateActorStageName 对应的 value
        if (mapping.TryGetValue(locateActorStageName, out var actors))
        {
            return actors;
        }

        return new List<string>();
    }

    public static string ActorDisplayText(EntitySnapshot actorSnapshot, HashSet<string> includedComponentNames = null)
    {
        var ret = "";
        for (int i = 0; i < actorSnapshot.components.Count; i++)
        {
            var component = actorSnapshot.components[i];
            if (includedComponentNames != null && !includedComponentNames.Contains(component.name))
            {
                continue;
            }

            if (component.name == typeof(RPGCharacterProfileComponent).Name)
            {
                var rpgCharacterProfileComponent = JsonConvert.DeserializeObject<RPGCharacterProfileComponent>(JsonConvert.SerializeObject(component.data));
                var rpgCharacterProfile = rpgCharacterProfileComponent.rpg_character_profile;
                var rpgCharacterProfileText = $"{actorSnapshot.name} = HP:{rpgCharacterProfile.hp}/{rpgCharacterProfile.max_hp}," +
                        $" Strength:{rpgCharacterProfile.strength}," +
                        $" Dexterity:{rpgCharacterProfile.dexterity}," +
                        $" Wisdom:{rpgCharacterProfile.wisdom}," +
                        $" Physical Attack:{rpgCharacterProfile.physical_attack}," +
                        $" Physical Defense:{rpgCharacterProfile.physical_defense}," +
                        $" Magic Attack:{rpgCharacterProfile.magic_attack}," +
                        $" Magic Defense:{rpgCharacterProfile.magic_defense}\n";

                if (rpgCharacterProfileComponent.status_effects.Count > 0)
                {
                    rpgCharacterProfileText += "Status Effects: ";
                    for (int j = 0; j < rpgCharacterProfileComponent.status_effects.Count; j++)
                    {
                        var statusEffect = rpgCharacterProfileComponent.status_effects[j];
                        rpgCharacterProfileText += $"{statusEffect.name} ({statusEffect.description}, {statusEffect.rounds})";
                        if (j < rpgCharacterProfileComponent.status_effects.Count - 1)
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
                var handCompText = $"{actorSnapshot.name} Hand: ";
                for (int j = 0; j < handComponent.skills.Count; j++)
                {
                    var skill = handComponent.skills[j];
                    handCompText += $"{skill.name} ({skill.description}, {skill.effect})";
                    if (j < handComponent.skills.Count - 1)
                    {
                        handCompText += ", ";
                    }
                }
                handCompText += "\n";
                for (int j = 0; j < handComponent.details.Count; j++)
                {
                    var detail = handComponent.details[j];
                    handCompText += $"Skill: {detail.skill}, Targets: {string.Join(", ", detail.targets)}\n";
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
        Vector2 canvasPos;
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.worldCamera,
            out canvasPos
        );

        if (!success)
        {
            Debug.LogWarning("Failed to convert screen point to canvas coordinates");
        }

        Debug.Log($"坐标转换: 世界({spriteWorldPos}) → 屏幕({screenPos}) → Canvas({canvasPos})");

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

