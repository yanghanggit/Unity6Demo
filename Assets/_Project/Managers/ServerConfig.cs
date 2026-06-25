// 服务器连接配置。
// 在 Unity 中右键 Create > Game > Server Config 创建资源文件，
// 保存到 Assets/_Project/Resources/ServerConfig.asset。
// 代码和 Scene 两条路径都从这里读取，保证行为一致。

using UnityEngine;

[CreateAssetMenu(fileName = "ServerConfig", menuName = "Game/Server Config")]
public class ServerConfig : ScriptableObject
{
    [Header("服务器连接")]
    public string host = "192.168.192.103";
    public int port = 8000;
    public int timeoutSeconds = 30;

    public string BaseUrl => $"http://{host}:{port}";
}
