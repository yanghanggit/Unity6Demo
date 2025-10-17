
public partial class WerewolfGameContext
{
    private static WerewolfGameContext _instance;

    public static WerewolfGameContext Instance
    {
        get
        {
            lock (lockObj)
            {
                if (_instance == null)
                {
                    _instance = new WerewolfGameContext();
                }
                return _instance;
            }
        }
    }

    private WerewolfGameContext()
    {
    }

    private static readonly object lockObj = new object();

    public readonly  string UserName = "Player1";

    public  readonly string GameName = "Game2";

    private RootResponse _rootResponse = new RootResponse();


    public RootResponse Root
    {
        get
        {
            return _rootResponse;
        }

        set
        {
            if (value == null)
            {
                UnityEngine.Debug.LogError("_rootResponse is null");
                return;
            }
            _rootResponse = value;
            UnityEngine.Debug.Assert(_rootResponse.endpoints != null, "endpoints is null");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("werewolf_game_start"), "endpoints does not contain werewolf_game_start");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("werewolf_gameplay"), "endpoints does not contain werewolf_gameplay");
            UnityEngine.Debug.Assert(_rootResponse.endpoints.ContainsKey("werewolf_game_state"), "endpoints does not contain werewolf_game_state");
        }
    }

    public string StartUrl
    {
        get
        {
            return _rootResponse.endpoints["werewolf_game_start"];
        }
    }


    public string ActorDetailsUrl
    {
        get
        {
            var baseUrl = _rootResponse.endpoints["werewolf_game_actor_details"];
            return $"{baseUrl}{UserName}/{GameName}/details";
        }
    }

}