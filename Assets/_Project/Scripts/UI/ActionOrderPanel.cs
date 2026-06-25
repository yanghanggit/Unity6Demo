// using System.Collections.Generic;
// using UnityEngine;

// public class ActionOrderPanel : MonoBehaviour
// {

//     [Header("Prefab References")]
//     [SerializeField] private ActionOrderObject _actionOrderObjectPrefab; // 角色槽位预制体

//     private readonly List<ActionOrderObject> _pool = new(); // 对象池

//     void Start()
//     {
//         Debug.Assert(_actionOrderObjectPrefab != null, "_actionOrderObjectPrefab is null");
//     }

//     /// <summary>
//     /// 从对象池中取出一个可用对象；池中无空闲时实例化新对象并纳入池
//     /// </summary>
//     private ActionOrderObject GetOrCreate()
//     {
//         foreach (var obj in _pool)
//         {
//             if (!obj.gameObject.activeSelf)
//                 return obj;
//         }

//         // 池中无空闲，扩容
//         var newObj = Instantiate(_actionOrderObjectPrefab, transform);
//         _pool.Add(newObj);
//         return newObj;
//     }

//     /// <summary>
//     /// 根据传入的角色实体序列化数据列表，从对象池复用或扩容槽位对象，并设置数据。
//     /// 多余的槽位对象会被隐藏而非销毁，供下次复用。
//     /// </summary>
//     public void RefresView(List<EntitySerialization> activeActors)
//     {
//         // 先将池中所有对象隐藏，标记为可复用
//         foreach (var obj in _pool)
//         {
//             obj.gameObject.SetActive(false);
//         }

//         // 按需从池中取出或新建对象，设置数据后激活
//         for (int i = 0; i < activeActors.Count; i++)
//         {
//             var actionOrderObject = GetOrCreate();
//             actionOrderObject.name = $"ActionOrder_{i}";
//             actionOrderObject.gameObject.SetActive(true);
//             actionOrderObject.RefresView(activeActors[i]);
//         }
//     }

//     /// <summary>
//     /// 清空对象池，销毁所有槽位对象。通常在场景退出时调用。
//     /// </summary>
//     public void Clear()
//     {
//         foreach (var obj in _pool)
//         {
//             if (obj != null)
//                 Destroy(obj.gameObject);
//         }
//         _pool.Clear();
//     }
// }
