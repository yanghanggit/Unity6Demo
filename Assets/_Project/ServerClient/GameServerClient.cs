// 对应 Python tui/server_client.py
// WebGL 兼容：使用 UnityWebRequest + UniTask，无 SSE 流。
// 当前主要面向 Web（WebGL）平台，因此暂不实现 SSE，统一以轮询（poll）为准：
//   - stream_session_messages → 以 FetchSessionMessagesAsync 轮询 /since 为准
//   - watch_task_until_done   → 以 WatchTaskUntilDoneAsync 轮询 /api/tasks/v1/status 为准

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

/// <summary>
/// 游戏服务器 HTTP 客户端（纯 C# 服务类，非 MonoBehaviour）。
/// 通过 GameServerClientHolder 在 Unity 场景中持有和配置。
/// </summary>
public class GameServerClient
{
    private readonly string _baseUrl;
    private readonly int _timeoutSeconds;

    public GameServerClient(string baseUrl, int timeoutSeconds = 30)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _timeoutSeconds = timeoutSeconds;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Exceptions
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>HTTP 请求失败（网络错误或 4xx/5xx）时抛出。</summary>
    public class ServerException : Exception
    {
        public long StatusCode { get; }
        public ServerException(long code, string message) : base(message) { StatusCode = code; }
    }

    /// <summary>后台任务执行失败（status=FAILED）时抛出。</summary>
    public class TaskFailedException : Exception
    {
        public TaskFailedException(string message) : base(message) { }
    }

    // ────────────────────────────────────────────────────────────────────────
    // HTTP helpers
    // ────────────────────────────────────────────────────────────────────────

    private async UniTask<T> GetAsync<T>(string relativeUrl, CancellationToken ct = default)
    {
        string url = _baseUrl + relativeUrl;
        using var req = UnityWebRequest.Get(url);
        req.timeout = _timeoutSeconds;
        req.SetRequestHeader("Accept", "application/json");

        try
        {
            await req.SendWebRequest().ToUniTask(cancellationToken: ct);
        }
        catch (UnityWebRequestException)
        {
            throw new ServerException(req.responseCode,
                $"GET {url} failed [{req.responseCode}]: {req.error}");
        }

        return JsonConvert.DeserializeObject<T>(req.downloadHandler.text);
    }

    private async UniTask<T> PostAsync<T>(string relativeUrl, object body, CancellationToken ct = default)
    {
        string url = _baseUrl + relativeUrl;
        byte[] jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));

        using var req = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonBytes),
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = _timeoutSeconds,
        };
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Accept", "application/json");

        try
        {
            await req.SendWebRequest().ToUniTask(cancellationToken: ct);
        }
        catch (UnityWebRequestException)
        {
            throw new ServerException(req.responseCode,
                $"POST {url} failed [{req.responseCode}]: {req.error}");
        }

        return JsonConvert.DeserializeObject<T>(req.downloadHandler.text);
    }

    /// <summary>将键值对列表拼接为 ?k1=v1&amp;k2=v2 格式的查询字符串。</summary>
    private static string Q(IEnumerable<(string key, string value)> pairs)
    {
        var parts = pairs
            .Select(p => $"{Uri.EscapeDataString(p.key)}={Uri.EscapeDataString(p.value)}")
            .ToList();
        return parts.Count > 0 ? "?" + string.Join("&", parts) : "";
    }

    // ────────────────────────────────────────────────────────────────────────
    // 服务信息
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>请求游戏服务器根路由，返回服务信息 JSON。</summary>
    public UniTask<JObject> FetchServerInfoAsync(CancellationToken ct = default)
        => GetAsync<JObject>("/", ct);

    // ────────────────────────────────────────────────────────────────────────
    // Auth
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>登录游戏服务器，返回服务器响应消息。</summary>
    public async UniTask<string> LoginAsync(string userName, string gameName, CancellationToken ct = default)
    {
        var resp = await PostAsync<LoginResponse>("/api/login/v1/",
            new LoginRequest { user_name = userName, game_name = gameName }, ct);
        return resp.message;
    }

    /// <summary>登出游戏服务器，返回服务器响应消息。</summary>
    public async UniTask<string> LogoutAsync(string userName, string gameName, CancellationToken ct = default)
    {
        var resp = await PostAsync<LogoutResponse>("/api/logout/v1/",
            new LogoutRequest { user_name = userName, game_name = gameName }, ct);
        return resp.message;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Game
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>创建新游戏，返回蓝图数据。</summary>
    public UniTask<NewGameResponse> NewGameAsync(string userName, string gameName, CancellationToken ct = default)
        => PostAsync<NewGameResponse>("/api/game/new/v1/",
            new NewGameRequest { user_name = userName, game_name = gameName }, ct);

    /// <summary>获取可用蓝图列表。</summary>
    public UniTask<BlueprintListResponse> FetchBlueprintListAsync(CancellationToken ct = default)
        => GetAsync<BlueprintListResponse>("/api/game/blueprint-list/v1/", ct);

    // ────────────────────────────────────────────────────────────────────────
    // Session Messages（WebGL 轮询替代 SSE stream）
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 增量获取玩家会话消息（从 lastSequenceId 之后开始）。
    /// 在 WebGL 下以此轮询代替 Python 端的 stream_session_messages SSE 接口。
    /// </summary>
    public UniTask<SessionMessageResponse> FetchSessionMessagesAsync(
        string userName, string gameName, int lastSequenceId, CancellationToken ct = default)
    {
        string qs = Q(new[] { ("last_sequence_id", lastSequenceId.ToString()) });
        return GetAsync<SessionMessageResponse>(
            $"/api/session_messages/v1/{userName}/{gameName}/since{qs}", ct);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Tasks
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>批量查询后台任务状态。</summary>
    public UniTask<TasksStatusResponse> FetchTasksStatusAsync(
        IEnumerable<string> taskIds, CancellationToken ct = default)
    {
        string qs = Q(taskIds.Select(id => ("task_ids", id)));
        return GetAsync<TasksStatusResponse>($"/api/tasks/v1/status{qs}", ct);
    }

    /// <summary>
    /// 轮询等待后台任务完成，返回终态 TaskRecord。
    /// 替代 Python 端基于 SSE 的 watch_task_until_done（WebGL 不支持流式读取）。
    /// </summary>
    /// <param name="taskId">要监听的任务 ID</param>
    /// <param name="timeoutSeconds">最大等待秒数</param>
    /// <param name="pollIntervalMs">两次轮询之间的间隔（毫秒）</param>
    /// <exception cref="TaskFailedException">任务失败</exception>
    /// <exception cref="TimeoutException">等待超时</exception>
    public async UniTask<TaskRecord> WatchTaskUntilDoneAsync(
        string taskId,
        int timeoutSeconds = 120,
        int pollIntervalMs = 1500,
        CancellationToken ct = default)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();

            var resp = await FetchTasksStatusAsync(new[] { taskId }, ct);
            var record = resp.tasks.FirstOrDefault(t => t.task_id == taskId);

            if (record != null)
            {
                if (record.status == TaskStatus.FAILED)
                    throw new TaskFailedException(record.error ?? "未知错误");
                if (record.status == TaskStatus.COMPLETED)
                    return record;
            }

            await UniTask.Delay(pollIntervalMs, cancellationToken: ct);
        }

        throw new TimeoutException($"任务 {taskId} 等待超时（{timeoutSeconds}s）");
    }

    // ────────────────────────────────────────────────────────────────────────
    // Stages
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>查询场景状态，返回场景与角色的分布映射。</summary>
    public UniTask<StagesStateResponse> FetchStagesStateAsync(
        string userName, string gameName, CancellationToken ct = default)
        => GetAsync<StagesStateResponse>($"/api/stages/v1/{userName}/{gameName}/state", ct);

    // ────────────────────────────────────────────────────────────────────────
    // Entities
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>批量查询实体详情。</summary>
    public UniTask<EntitiesDetailsResponse> FetchEntitiesDetailsAsync(
        string userName, string gameName, IEnumerable<string> entityNames, CancellationToken ct = default)
    {
        string qs = Q(entityNames.Select(n => ("entities", n)));
        return GetAsync<EntitiesDetailsResponse>(
            $"/api/entities/v1/{userName}/{gameName}/details{qs}", ct);
    }

    /// <summary>按组件条件分组查询实体（all_of=必须包含 / any_of=任意包含 / none_of=不得包含）。</summary>
    public UniTask<EntitiesDetailsResponse> FetchEntitiesGroupAsync(
        string userName, string gameName,
        IEnumerable<string> allOf, IEnumerable<string> anyOf, IEnumerable<string> noneOf,
        CancellationToken ct = default)
    {
        var pairs = new List<(string, string)>();
        if (allOf != null) foreach (var c in allOf) pairs.Add(("all_of", c));
        if (anyOf != null) foreach (var c in anyOf) pairs.Add(("any_of", c));
        if (noneOf != null) foreach (var c in noneOf) pairs.Add(("none_of", c));
        return GetAsync<EntitiesDetailsResponse>(
            $"/api/entities/v1/{userName}/{gameName}/group{Q(pairs)}", ct);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Home
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>触发家园推进流程，为 actors 指定的角色激活行动计划，返回后台任务信息。</summary>
    public UniTask<HomeAdvanceResponse> HomeAdvanceAsync(
        string userName, string gameName, List<string> actors, CancellationToken ct = default)
        => PostAsync<HomeAdvanceResponse>("/api/home/advance/v1/",
            new HomeAdvanceRequest { user_name = userName, game_name = gameName, actors = actors }, ct);

    /// <summary>传送玩家进入指定地下城。</summary>
    public UniTask<HomeEnterDungeonResponse> HomeEnterDungeonAsync(
        string userName, string gameName, string dungeonName, CancellationToken ct = default)
        => PostAsync<HomeEnterDungeonResponse>("/api/home/enter_dungeon/v1/",
            new HomeEnterDungeonRequest
            {
                user_name = userName,
                game_name = gameName,
                dungeon_name = dungeonName
            }, ct);

    /// <summary>触发地下城生成流程，返回后台任务信息。</summary>
    public UniTask<HomeGenerateDungeonResponse> HomeGenerateDungeonAsync(
        string userName, string gameName, CancellationToken ct = default)
        => PostAsync<HomeGenerateDungeonResponse>("/api/home/generate_dungeon/v1/",
            new HomeGenerateDungeonRequest { user_name = userName, game_name = gameName }, ct);

    /// <summary>触发家园玩家动作（对话、场景切换等），返回后台任务信息。</summary>
    public UniTask<HomePlayerActionResponse> HomePlayerActionAsync(
        string userName, string gameName,
        HomePlayerActionType action,
        Dictionary<string, string> arguments,
        CancellationToken ct = default)
        => PostAsync<HomePlayerActionResponse>("/api/home/player_action/v1/",
            new HomePlayerActionRequest
            {
                user_name = userName,
                game_name = gameName,
                action = action,
                arguments = arguments,
            }, ct);

    /// <summary>将成员加入远征队。</summary>
    public UniTask<HomeRosterAddResponse> HomeRosterAddAsync(
        string userName, string gameName, string memberName, CancellationToken ct = default)
        => PostAsync<HomeRosterAddResponse>("/api/home/roster/add/v1/",
            new HomeRosterAddRequest
            {
                user_name = userName,
                game_name = gameName,
                member_name = memberName
            }, ct);

    /// <summary>将成员从远征队移除。</summary>
    public UniTask<HomeRosterRemoveResponse> HomeRosterRemoveAsync(
        string userName, string gameName, string memberName, CancellationToken ct = default)
        => PostAsync<HomeRosterRemoveResponse>("/api/home/roster/remove/v1/",
            new HomeRosterRemoveRequest
            {
                user_name = userName,
                game_name = gameName,
                member_name = memberName
            }, ct);

    /// <summary>将道具从储物箱移入随身背包。</summary>
    public UniTask<HomeItemMoveToInventoryResponse> HomeItemMoveToInventoryAsync(
        string userName, string gameName, List<string> itemNames, CancellationToken ct = default)
        => PostAsync<HomeItemMoveToInventoryResponse>("/api/home/item/move_to_inventory/v1/",
            new HomeItemMoveToInventoryRequest
            {
                user_name = userName,
                game_name = gameName,
                item_names = itemNames
            }, ct);

    /// <summary>将道具从随身背包移入储物箱。</summary>
    public UniTask<HomeItemMoveToStorageResponse> HomeItemMoveToStorageAsync(
        string userName, string gameName, List<string> itemNames, CancellationToken ct = default)
        => PostAsync<HomeItemMoveToStorageResponse>("/api/home/item/move_to_storage/v1/",
            new HomeItemMoveToStorageRequest
            {
                user_name = userName,
                game_name = gameName,
                item_names = itemNames
            }, ct);

    /// <summary>为指定角色穿戴时装，返回后台任务信息。</summary>
    public UniTask<HomeWearCostumeResponse> HomeWearCostumeAsync(
        string userName, string gameName, string itemName, string targetName,
        CancellationToken ct = default)
        => PostAsync<HomeWearCostumeResponse>("/api/home/costume/wear/v1/",
            new HomeWearCostumeRequest
            {
                user_name = userName,
                game_name = gameName,
                item_name = itemName,
                target_name = targetName,
            }, ct);

    /// <summary>为指定角色脱下当前穿戴的时装，返回后台任务信息。</summary>
    public UniTask<HomeRemoveCostumeResponse> HomeRemoveCostumeAsync(
        string userName, string gameName, string targetName,
        CancellationToken ct = default)
        => PostAsync<HomeRemoveCostumeResponse>("/api/home/costume/remove/v1/",
            new HomeRemoveCostumeRequest
            {
                user_name = userName,
                game_name = gameName,
                target_name = targetName,
            }, ct);

    /// <summary>从储物箱材料合成消耗品，返回后台任务信息。</summary>
    public UniTask<HomeCraftItemResponse> HomeCraftItemAsync(
        string userName, string gameName, List<string> materials, CancellationToken ct = default)
        => PostAsync<HomeCraftItemResponse>("/api/home/craft/item/v1/",
            new HomeCraftItemRequest { user_name = userName, game_name = gameName, materials = materials }, ct);

    /// <summary>从储物箱材料锻造装备，返回后台任务信息。</summary>
    public UniTask<HomeCraftItemResponse> HomeCraftGearItemAsync(
        string userName, string gameName, List<string> materials, CancellationToken ct = default)
        => PostAsync<HomeCraftItemResponse>("/api/home/craft/gear/v1/",
            new HomeCraftItemRequest { user_name = userName, game_name = gameName, materials = materials }, ct);

    /// <summary>从储物箱材料制作时装，返回后台任务信息。</summary>
    public UniTask<HomeCraftItemResponse> HomeCraftCostumeItemAsync(
        string userName, string gameName, List<string> materials, CancellationToken ct = default)
        => PostAsync<HomeCraftItemResponse>("/api/home/craft/costume/v1/",
            new HomeCraftItemRequest { user_name = userName, game_name = gameName, materials = materials }, ct);

    // ────────────────────────────────────────────────────────────────────────
    // Dungeon (state queries)
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>获取可用地下城列表。</summary>
    public UniTask<DungeonListResponse> FetchDungeonListAsync(CancellationToken ct = default)
        => GetAsync<DungeonListResponse>("/api/home/dungeon-list/v1/", ct);

    /// <summary>查询当前地下城完整状态。</summary>
    public UniTask<DungeonStateResponse> FetchDungeonStateAsync(
        string userName, string gameName, CancellationToken ct = default)
        => GetAsync<DungeonStateResponse>($"/api/dungeons/v1/{userName}/{gameName}/state", ct);

    /// <summary>查询当前地下城房间（含 stage + combat）。</summary>
    public UniTask<DungeonRoomResponse> FetchDungeonRoomAsync(
        string userName, string gameName, CancellationToken ct = default)
        => GetAsync<DungeonRoomResponse>($"/api/dungeons/v1/{userName}/{gameName}/room", ct);

    // ────────────────────────────────────────────────────────────────────────
    // Dungeon (actions)
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>退出地下城，返回家园。</summary>
    public UniTask<DungeonExitResponse> DungeonExitAsync(
        string userName, string gameName, CancellationToken ct = default)
        => PostAsync<DungeonExitResponse>("/api/dungeon/exit/v1/",
            new DungeonExitRequest { user_name = userName, game_name = gameName }, ct);

    /// <summary>推进地下城到下一关卡（关卡内战斗结束进入 post_combat 后调用）。</summary>
    public UniTask<DungeonAdvanceStageResponse> DungeonAdvanceStageAsync(
        string userName, string gameName, CancellationToken ct = default)
        => PostAsync<DungeonAdvanceStageResponse>("/api/dungeon/progress/advance_stage/v1/",
            new DungeonAdvanceStageRequest { user_name = userName, game_name = gameName }, ct);

    /// <summary>触发入口房间初始化（叙事 + 牌库生成），返回后台任务信息。</summary>
    public UniTask<DungeonEntryInitResponse> DungeonEntryInitAsync(
        string userName, string gameName, CancellationToken ct = default)
        => PostAsync<DungeonEntryInitResponse>("/api/dungeon/entry/init/v1/",
            new DungeonEntryInitRequest { user_name = userName, game_name = gameName }, ct);

    // ────────────────────────────────────────────────────────────────────────
    // Dungeon: Combat
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>触发战斗初始化，返回后台任务信息。</summary>
    public UniTask<DungeonCombatInitResponse> DungeonCombatInitAsync(
        string userName, string gameName, CancellationToken ct = default)
        => PostAsync<DungeonCombatInitResponse>("/api/dungeon/combat/init/v1/",
            new DungeonCombatInitRequest { user_name = userName, game_name = gameName }, ct);

    /// <summary>触发战斗撤退，返回后台任务信息。</summary>
    public UniTask<DungeonCombatRetreatResponse> DungeonCombatRetreatAsync(
        string userName, string gameName, CancellationToken ct = default)
        => PostAsync<DungeonCombatRetreatResponse>("/api/dungeon/combat/retreat/v1/",
            new DungeonCombatRetreatRequest { user_name = userName, game_name = gameName }, ct);

    /// <summary>为全体战斗角色激活抽牌动作，返回后台任务信息。</summary>
    public UniTask<DungeonCombatDrawCardsResponse> DungeonCombatDrawCardsAsync(
        string userName, string gameName, CancellationToken ct = default)
        => PostAsync<DungeonCombatDrawCardsResponse>("/api/dungeon/combat/draw_cards/v1/",
            new DungeonCombatDrawCardsRequest { user_name = userName, game_name = gameName }, ct);

    /// <summary>让指定角色打出指定卡牌，返回后台任务信息。</summary>
    public UniTask<DungeonCombatPlayCardsResponse> DungeonCombatPlayCardsAsync(
        string userName, string gameName,
        string actorName, string cardName, List<string> targets,
        CancellationToken ct = default)
        => PostAsync<DungeonCombatPlayCardsResponse>("/api/dungeon/combat/play_cards/v1/",
            new DungeonCombatPlayCardsRequest
            {
                user_name = userName,
                game_name = gameName,
                actor_name = actorName,
                card_name = cardName,
                targets = targets,
            }, ct);

    /// <summary>让指定角色跳过当前回合出牌，返回后台任务信息。</summary>
    public UniTask<DungeonCombatPassTurnResponse> DungeonCombatPassTurnAsync(
        string userName, string gameName, string actorName, CancellationToken ct = default)
        => PostAsync<DungeonCombatPassTurnResponse>("/api/dungeon/combat/pass_turn/v1/",
            new DungeonCombatPassTurnRequest
            {
                user_name = userName,
                game_name = gameName,
                actor_name = actorName
            }, ct);

    /// <summary>使用背包内消耗品，返回后台任务信息。</summary>
    public UniTask<DungeonCombatUseConsumableItemResponse> DungeonCombatUseConsumableAsync(
        string userName, string gameName,
        string itemName, List<string> targets,
        CancellationToken ct = default)
        => PostAsync<DungeonCombatUseConsumableItemResponse>("/api/dungeon/combat/use_consumable/v1/",
            new DungeonCombatUseConsumableItemRequest
            {
                user_name = userName,
                game_name = gameName,
                item_name = itemName,
                targets = targets,
            }, ct);

    /// <summary>使用背包内装备，返回后台任务信息。</summary>
    public UniTask<DungeonCombatUseGearItemResponse> DungeonCombatUseGearAsync(
        string userName, string gameName,
        string itemName, List<string> targets,
        CancellationToken ct = default)
        => PostAsync<DungeonCombatUseGearItemResponse>("/api/dungeon/combat/use_gear/v1/",
            new DungeonCombatUseGearItemRequest
            {
                user_name = userName,
                game_name = gameName,
                item_name = itemName,
                targets = targets,
            }, ct);

    /// <summary>收取战斗战利品，将掉落物转入背包。</summary>
    public UniTask<DungeonCombatCollectLootResponse> DungeonCombatCollectLootAsync(
        string userName, string gameName, CancellationToken ct = default)
        => PostAsync<DungeonCombatCollectLootResponse>("/api/dungeon/combat/collect_loot/v1/",
            new DungeonCombatCollectLootRequest { user_name = userName, game_name = gameName }, ct);
}
