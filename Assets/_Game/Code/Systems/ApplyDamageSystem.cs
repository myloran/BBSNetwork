using Unity.Entities;

public class ApplyDamageSystem : ComponentSystem {
  struct FilteredData {
    public readonly int Length;
    //[ReadOnly]
    //[WriteOnly]
    public ComponentDataArray<DamageInfo> damageInfo;
    //public ComponentArray<> comp;		
    public EntityArray entities;
  }
  [Inject] FilteredData filteredData;

  protected override void OnUpdate() {
    for (int i = 0; i < filteredData.Length; i++) {
      var damageInfo = filteredData.damageInfo[i];
      var entity = filteredData.entities[i];
      if (EntityManager.HasComponent<NetworktOwner>(damageInfo.receiver)) {
        var health = EntityManager.GetComponentData<Health>(damageInfo.receiver);
        health.value -= damageInfo.damage;
        if (health.value < 0) {
          health.value = 0;
          EntityManager.AddComponentData(damageInfo.receiver, new DeathComponent { timer = 3 });
        }
        EntityManager.SetComponentData(damageInfo.receiver, health);
      }
      PostUpdateCommands.DestroyEntity(entity);
    }
  }
}