// Mock 数据源：用正式 ECS 结构（EntitySerialization / ComponentSerialization + 组件 DTO）
// 模拟后端返回，供 HomeOverview / HomeStage 在未接真实 API 时使用。
// 后续接入真实 API：替换为 GameServerClient.FetchStagesStateAsync / FetchEntitiesDetailsAsync。
using System.Collections.Generic;
using System.Linq;

public static class MockGameData
{
    /// <summary>模拟 /api/stages/.../state：stage 全名 → 该 stage 内 actor 全名列表。</summary>
    public static StagesStateResponse BuildStagesState()
    {
        var mapping = new Dictionary<string, List<string>>();
        foreach (var s in StageDefs)
            mapping[s.Name] = s.Actors.ToList();
        return new StagesStateResponse { mapping = mapping };
    }

    /// <summary>模拟 /api/entities/.../details：返回给定实体名的完整详情。</summary>
    public static EntitiesDetailsResponse BuildEntitiesDetails(IEnumerable<string> entityNames)
    {
        var wanted = new HashSet<string>(entityNames);
        var entities = AllEntities().Where(e => wanted.Contains(e.name)).ToList();
        return new EntitiesDetailsResponse { entities = entities };
    }

    /// <summary>全部实体（stage + actor）。</summary>
    public static List<EntitySerialization> AllEntities()
    {
        var list = new List<EntitySerialization>();
        foreach (var s in StageDefs)
        {
            list.Add(BuildStageEntity(s));
            foreach (var actor in s.Actors)
                list.Add(BuildActorEntity(actor, s.Name));
        }
        return list;
    }

    // ── 实体构造 ──

    private static EntitySerialization BuildStageEntity(StageDef s)
    {
        return new EntitySerialization
        {
            name = s.Name,
            components = new List<ComponentSerialization>
            {
                ComponentUtils.ToComp(new IdentityComponent { name = s.Name }),
                ComponentUtils.ToComp(new StageComponent { name = s.Name, code_name = s.CodeName }),
                ComponentUtils.ToComp(new StageDescriptionComponent { name = s.Name, narrative = s.Narrative }),
                ComponentUtils.ToComp(new HomeComponent { name = s.Name }),
            },
        };
    }

    private static EntitySerialization BuildActorEntity(string actorName, string currentStage)
    {
        return new EntitySerialization
        {
            name = actorName,
            components = new List<ComponentSerialization>
            {
                ComponentUtils.ToComp(new IdentityComponent { name = actorName }),
                ComponentUtils.ToComp(new ActorComponent { name = actorName, current_stage = currentStage }),
                ComponentUtils.ToComp(new AppearanceComponent
                {
                    name = actorName,
                    base_body = "人类",
                    appearance = $"{EntityNameUtils.GetDisplayName(actorName)}，朴素衣着。",
                }),
                ComponentUtils.ToComp(new NPCComponent { name = actorName }),
            },
        };
    }

    // ── 数据定义 ──

    private sealed class StageDef
    {
        public string Name;      // 完整实体名，如 "场景.村口"
        public string CodeName;  // 英文代号，用于后续图片映射
        public string Narrative;
        public string[] Actors;  // 该 stage 内的 actor 完整实体名
    }

    private static readonly StageDef[] StageDefs =
    {
        new StageDef { Name = "场景.村口", CodeName = "village_hall", Narrative = "村庄入口，来往行人的必经之地。", Actors = new[] { "角色.旅行者", "角色.村长" } },
        new StageDef { Name = "场景.训练场", CodeName = "training_ground", Narrative = "村民习武练兵的空地。", Actors = new[] { "角色.铁匠", "角色.教官" } },
        new StageDef { Name = "场景.猎人小屋", CodeName = "hunter_storage", Narrative = "猎人的居所，堆放着猎物与工具。", Actors = new[] { "角色.猎人", "角色.药师" } },
        new StageDef { Name = "场景.史家宅", CodeName = "shi_family_house", Narrative = "史家宅院，门庭森严。", Actors = new[] { "角色.史员外" } },
    };
}
