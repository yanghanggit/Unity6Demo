// MonoBehaviour 持有者，在 Unity 场景中配置并暴露 GameServerClient 单例。
// 挂载到常驻 GameObject（如 GameManager）即可，其他脚本通过 GameServerClientHolder.Instance.Client 使用。
// 连接参数优先从 ServerConfig（ScriptableObject）读取：
//   1. Inspector 里拖入 _config 字段
//   2. 或放置 ServerConfig.asset 到任意 Resources/ 文件夹（自动加载）

using UnityEngine;

public class GameServerClientHolder : MonoBehaviour
{
    public static GameServerClientHolder Instance { get; private set; }

    [SerializeField] private ServerConfig _config;

    public GameServerClient Client { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 如果已经存在其他实例，销毁当前对象，保证单例唯一性
            Destroy(gameObject);
            return;
        }

        // 设置单例实例
        Instance = this;

        // 设置为常驻对象，切换场景时不会被销毁
        DontDestroyOnLoad(gameObject);

        // Inspector 未赋值时从 Resources 自动加载
        if (_config == null)
            _config = Resources.Load<ServerConfig>("ServerConfig");

        Debug.Assert(_config != null, "[GameServerClientHolder] ServerConfig is null after attempting to load from Resources.");
        if (_config == null)
        {
            Debug.LogError("[GameServerClientHolder] 未找到 ServerConfig，请创建 Resources/ServerConfig.asset。");
            return;
        }

        Client = new GameServerClient(_config.BaseUrl, _config.timeoutSeconds);
    }
}
