using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;

[UpdateInGroup(typeof(NetworkUpdateGroup))]
[UpdateAfter(typeof(NetworkSendSystem))]
public class NetworkDataAssignParentSystem : ComponentSystem {
  readonly List<NetworkMethodInfo<NetworkDataAssignParentSystem>> SetComponentParentMethods = new List<NetworkMethodInfo<NetworkDataAssignParentSystem>>();
  readonly ReflectionUtility reflectionUtility = new ReflectionUtility();

  protected override void OnCreateManager(int capacity) {
    var types = reflectionUtility.ComponentTypes;
    var type = typeof(NetworkDataAssignParentSystem);

    for (int i = 0; i < types.Length; i++) {
      var info = type
          .GetMethod("SetComponentParent", BindingFlags.Instance | BindingFlags.NonPublic)
          .MakeGenericMethod(types[i].GetManagedType());
      SetComponentParentMethods.Add(
        new NetworkMethodInfo<NetworkDataAssignParentSystem>(info));
    }
  }

  void SetComponentParent<T>() {
    var group = GetComponentGroup(
      ComponentType.ReadOnly<NetworkComponentData<T>>(), 
      ComponentType.Create<NetworkComponentEntityReference>());
    var entities = group.GetEntityArray();
    var reference = group.GetComponentDataArray<NetworkComponentEntityReference>();

    for (int i = 0; i < entities.Length; i++) {
      var entity = new Entity {
        Index = reference[i].Index,
        Version = reference[i].Version
      };
      PostUpdateCommands.SetComponent(
        entity, 
        new NetworkComponentState<T> { dataEntity = entities[i] });
      PostUpdateCommands.RemoveComponent<NetworkComponentEntityReference>(entities[i]);
    }
  }

  protected override void OnUpdate() {
    for (int i = 0; i < SetComponentParentMethods.Count; i++) {
      SetComponentParentMethods[i].Invoke(this);
    }
  }
}