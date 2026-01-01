using System.Diagnostics;


/// <summary>
/// ImageServer上下文管理类
/// 用于管理图片服务器的全局状态，包括API端点配置
/// 采用线程安全的单例模式实现
/// </summary>
public partial class ImageServiceContext
{
    /// <summary>
    /// 单例实例
    /// </summary>
    private static ImageServiceContext _instance;

    /// <summary>
    /// 线程锁对象，用于确保单例模式的线程安全
    /// </summary>
    /// <summary>
    /// 线程锁对象,用于确保单例模式的线程安全
    /// </summary>
    private static readonly object _lockObj = new();

    /// <summary>
    /// 获取GameContext的单例实例
    /// 使用双重检查锁定模式确保线程安全
    /// </summary>
    public static ImageServiceContext Instance
    {
        get
        {
            lock (_lockObj)
            {
                _instance ??= new ImageServiceContext();
                return _instance;
            }
        }
    }

    /// <summary>
    /// 清除单例实例和所有静态数据
    /// 用于登出或重新初始化游戏状态
    /// </summary>
    public static void ClearInstance()
    {
        lock (_lockObj)
        {
            // 清空单例实例（这会导致实例字段如 UserName、GameName 等也被清除）
            _instance = null;
        }
    }

    /// <summary>
    /// 私有构造函数，防止外部实例化
    /// </summary>
    private ImageServiceContext()
    {
    }

    /// <summary>
    /// 基础URL，用于构建API请求的完整地址
    /// </summary>
    private string _baseUrl;

    /// <summary>
    /// 获取或设置基础URL
    /// </summary>
    public string BaseUrl
    {
        get
        {
            return _baseUrl;
        }
        set
        {
            _baseUrl = value;
        }
    }


    /// <summary>
    /// 获取生成图片API的URL地址
    /// </summary>
    public string GenerateImageApiUrl
    {
        get
        {
            return _baseUrl.TrimEnd('/') + RootResp.GetImageRoot().endpoints["generate"];
        }
    }

    //"static_images"
    public string StaticImagesApiUrl
    {
        get
        {
            return _baseUrl.TrimEnd('/') + RootResp.GetImageRoot().endpoints["static_images"];
        }
    }
}