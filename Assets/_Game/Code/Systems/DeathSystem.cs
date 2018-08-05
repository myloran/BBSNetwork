using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class DeathSystem : ComponentSystem {
  struct Data {
    public readonly int Length;
    //[ReadOnly]
    //[WriteOnly]
    public ComponentDataArray<DeathComponent> death;
    public EntityArray entities;
  }
  [Inject] Data data;

  protected override void OnUpdate() {
    var objects = new List<GameObject>();
    for (int i = 0; i < data.Length; i++) {
      var death = data.death[i];
      var entity = data.entities[i];
      death.timer -= Time.deltaTime;
      if (death.timer < 0) {
        PostUpdateCommands.DestroyEntity(entity);
        if (EntityManager.HasComponent<Transform>(entity)) {
          var obj = EntityManager.GetComponentObject<Transform>(entity).gameObject;
          objects.Add(obj);
        }
      }
      data.death[i] = death;
    }
    for (int i = 0; i < objects.Count; i++)
      Object.Destroy(objects[i]);
    objects.Clear();
  }
}