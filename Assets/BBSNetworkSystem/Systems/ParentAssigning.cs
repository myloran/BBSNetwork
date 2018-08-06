using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;

[UpdateInGroup(typeof(NetworkUpdateGroup))]
[UpdateAfter(typeof(NetworkSendSystem))]
public class ParentAssigning : ComponentSystem {
  readonly List<NetworkMethod<ParentAssigning>> methods = new List<NetworkMethod<ParentAssigning>>();
  readonly ReflectionUtility reflectionUtility = new ReflectionUtility();

  protected override void OnCreateManager(int capacity) {
    var types = reflectionUtility.ComponentTypes;
    var type = typeof(ParentAssigning);

    for (int i = 0; i < types.Length; i++) {
      var info = type
        .GetMethod("SetComponentParent", BindingFlags.Instance | BindingFlags.NonPublic)
        .MakeGenericMethod(types[i].GetManagedType());

      methods.Add(new NetworkMethod<ParentAssigning>(info));
    }
  }

  void SetComponentParent<T>() {
    var group = GetComponentGroup(
      ComponentType.ReadOnly<NetworkComponentData<T>>(), 
      ComponentType.Create<ComponentEntity>());

    var entities = group.GetEntityArray();
    var reference = group.GetComponentDataArray<ComponentEntity>();

    for (int i = 0; i < entities.Length; i++) {
      var entity = new Entity {
        Index = reference[i].Index,
        Version = reference[i].Version
      };

      PostUpdateCommands.SetComponent(
        entity, 
        new NetworkComponentState<T> { dataEntity = entities[i] });

      PostUpdateCommands.RemoveComponent<ComponentEntity>(entities[i]);
    }
  }

  protected override void OnUpdate() {
    for (int i = 0; i < methods.Count; i++)
      methods[i].Invoke(this);
  }
}