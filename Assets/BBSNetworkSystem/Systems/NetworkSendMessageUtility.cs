using System.Collections.Generic;
using Unity.Entities;

public class NetworkSendMessageUtility {
  public readonly NetworkSyncDataContainer Container = new NetworkSyncDataContainer();
  readonly Dictionary<Entity, NetworkEntityContainer> EntityContainers = new Dictionary<Entity, NetworkEntityContainer>();

  public void AddEntity(NetworkEntityData networkEntityData) {
    Container.AddedEntities.Add(networkEntityData);
  }

  public void RemoveEntity(NetworkEntity networkSyncEntity) {
    Container.RemovedEntities.Add(networkSyncEntity);
  }

  public void AddComponent(Entity entity, int actorId, int networkId, ComponentDataContainer componentData) {
    GetEntity(entity, actorId, networkId)
      .AddedComponents.Add(componentData);
  }

  public void RemoveComponent(Entity entity, int actorId, int networkId, int componentId) {
    GetEntity(entity, actorId, networkId)
      .RemovedComponents.Add(componentId);
  }

  public void SetComponentData(Entity entity, int actorId, int networkId, ComponentDataContainer componentDataContainer) {
    GetEntity(entity, actorId, networkId)
      .ComponentData.Add(componentDataContainer);
  }

  public void AddComponents(Entity entity, int actorId, int networkId, List<ComponentDataContainer> componentIds) {
    GetEntity(entity, actorId, networkId)
      .AddedComponents.AddRange(componentIds);
  }

  public void RemoveComponents(Entity entity, int actorId, int networkId, List<int> componentIds) {
    GetEntity(entity, actorId, networkId)
      .RemovedComponents.AddRange(componentIds);
  }

  public void SetComponentData(Entity entity, int actorId, int networkId, List<ComponentDataContainer> componentDataContainers) {
    GetEntity(entity, actorId, networkId)
      .ComponentData.AddRange(componentDataContainers);
  }

  NetworkEntityContainer GetEntity(Entity entity, int actorId, int networkId) {
    if (!EntityContainers.TryGetValue(entity, out NetworkEntityContainer container)) {
      container = new NetworkEntityContainer() {
        Entity = new NetworkEntity() {
          ActorId = actorId,
          NetworkId = networkId,
        }
      };
      Container.Entities.Add(container);
      EntityContainers.Add(entity, container);
    }
    return container;
  }

  public void Reset() {
    Container.AddedEntities.Clear();
    Container.RemovedEntities.Clear();
    Container.Entities.Clear();
    EntityContainers.Clear();
  }
}