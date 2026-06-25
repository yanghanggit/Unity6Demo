// MonoBehaviour 持有者，在 Unity 场景中配置并暴露 GameServerClient 单例。
// 挂载到常驻 GameObject（如 GameManager）即可，其他脚本通过 GameServerClientHolder.Instance.Client 使用。

using UnityEngine;

public class GameServerClientHolder : MonoBehaviour
{
    public static GameServerClientHolder Instance { get; private set; }

    [Header("服务器连接")]
    [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 8000;
    [SerializeField] private int timeoutSeconds = 30;

    public GameServerClient Client { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Client = new GameServerClient($"http://{host}:{port}", timeoutSeconds);
    }
}
