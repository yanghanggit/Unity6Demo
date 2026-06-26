// using TMPro;
// using UnityEngine;

// public class HomeSceneInputStatePanel : MonoBehaviour
// {
//     // UI组件引用
//     [Header("UI Components")]
//     [SerializeField] private TMP_InputField _inputField;       // 输入字段 (TMP)

//     void Start()
//     {
//         Debug.Assert(_inputField != null, "_inputField is null");
//     }

//     /// <summary>
//     /// InputField (TMP) - On Value Changed 事件处理器
//     /// </summary>
//     /// <param name="value">输入字段的当前值</param>
//     public void OnInputFieldValueChanged(string value)
//     {
//         Debug.Log($"InputField value changed: {value}");
//         Debug.Log("OnValueChanged: " + _inputField.text);
//     }

//     /// <summary>
//     /// InputField (TMP) - On End Edit 事件处理器
//     /// </summary>
//     /// <param name="value">输入字段的最终值</param>
//     public void OnInputFieldEndEdit(string value)
//     {
//         Debug.Log($"InputField end edit: {value}");
//     }

//     /// <summary>
//     /// InputField (TMP) - On Select 事件处理器
//     /// </summary>
//     /// <param name="value">输入字段被选中时的值</param>
//     public void OnInputFieldSelect(string value)
//     {
//         Debug.Log($"InputField selected: {value}");
//     }

//     /// <summary>
//     /// InputField (TMP) - On Deselect 事件处理器
//     /// </summary>
//     /// <param name="value">输入字段被取消选中时的值</param>
//     public void OnInputFieldDeselect(string value)
//     {
//         Debug.Log($"InputField deselected: {value}");
//     }

//     ///
//     public string GetInputText()
//     {
//         return _inputField.text;
//     }

//     public void OnActivate(string targetActorName)
//     {
//         if (!GameContext.Instance.IsLoggedIn)
//         {
//             Debug.LogWarning("User is not logged in. Input may not be functional.");
//             _inputField.text = targetActorName;
//             return;
//         }

//         _inputField.text = string.Empty; // 激活时清空输入字段

//     }
// }
