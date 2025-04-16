using System.Collections.Generic;

/** 
 * 
 */
// # 以下是针对卡牌游戏中 牌组、弃牌堆、抽牌堆、手牌 的类名设计建议，结合常见游戏术语和编程习惯：
// # 方案 4：极简统一型
// # 组件	类名	说明
// # 牌组	Deck	直接命名为 Deck，表示通用牌组。
// # 抽牌堆	DrawDeck	与 Deck 统一，通过前缀区分功能。
// # 弃牌堆	DiscardDeck	同上，保持命名一致性。
// # 手牌	Hand	简洁无冗余。
// # play_card
// # draw_card
[System.Serializable]
public class HandDetail
{
    public string skill = "";
    public List<string> targets = new List<string>();
    public string reason = "";
    public string dialogue = "";
}

[System.Serializable]
public class HandComponent
{
    public string name = "";
    public List<Skill> skills = new List<Skill>();
    public List<HandDetail> details = new List<HandDetail>();
}

/**
 * 
 */
[System.Serializable]
public class RPGCharacterProfileComponent
{
    public string name = "";
    public RPGCharacterProfile rpg_character_profile = new RPGCharacterProfile();
    public List<StatusEffect> status_effects = new List<StatusEffect>();
}
