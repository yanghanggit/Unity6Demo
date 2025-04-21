using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;


// 召唤闪电僵尸
// 召唤一个浑身冒着闪电的僵尸，长着6个手臂，一遍大叫“卡巴蒂，卡巴蒂！！”，一遍扑向所有的敌人，一顿撕咬。
// 造成物理伤害10000点。

public class XCardEditor : MonoBehaviour
{
    public DungeonGamePlayAction _dungeonGamePlayAction;
    public XCardPlayer _xCardPlayer;
    public TMP_InputField _editNameInputField;
    public TMP_InputField _editDescriptionInputField;
    public TMP_InputField _editEffectInputField;

    void Start()
    {
        Debug.Assert(_xCardPlayer != null, "_gameConfig is null");
        Debug.Assert(_editNameInputField != null, "_editNameInputField is null");
        Debug.Assert(_editDescriptionInputField != null, "_editDescriptionInputField is null");
        Debug.Assert(_editEffectInputField != null, "_editEffectInputField is null");
        Debug.Assert(_dungeonGamePlayAction != null, "_dungeonRunAction is null");
    }

    void OnEnable()
    {
        Debug.Log("XCardEditor OnEnable");
        _editNameInputField.text = _xCardPlayer.Name;
        _editDescriptionInputField.text = _xCardPlayer.Description;
        _editEffectInputField.text = _xCardPlayer.Effect;
    }

    void Update()
    {

    }

    public void OnClickClose()
    {
        Debug.Log("OnClickClose");
        this.gameObject.SetActive(false);
    }

    public void OnClickConfirm()
    {
        Debug.Log("OnClickConfirm");
        if (_editNameInputField.text == "")
        {
            Debug.LogError("Name is empty");
            return;
        }
        if (_editDescriptionInputField.text == "")
        {
            Debug.LogError("Description is empty");
            return;
        }
        if (_editEffectInputField.text == "")
        {
            Debug.LogError("Effect is empty");
            return;
        }
        StartCoroutine(ExecuteXCard(_editNameInputField.text, _editDescriptionInputField.text, _editEffectInputField.text));
    }

    private IEnumerator ExecuteXCard(string skillName, string skillDescription, string skillEffect)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["name"] = skillName;
        data["description"] = skillDescription;
        data["effect"] = skillEffect;
        yield return StartCoroutine(_dungeonGamePlayAction.Call("x_card", data));
        if (!_dungeonGamePlayAction.RequestSuccess)
        {
            Debug.LogError("ExecuteXCard request failed");
            yield break;
        }

        Debug.Log("ExecuteXCard request success");

        // 自动关闭。
        OnClickClose();
    }
}
