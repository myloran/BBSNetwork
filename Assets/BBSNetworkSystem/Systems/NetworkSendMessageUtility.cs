using System.Collections.Generic;
using Unity.Entities;

public class NetworkSendMessageUtility {
  public readonly NetworkSyncDataContainer Container = new NetworkSyncDataContainer();
  readonly Dictionary<Entity, NetworkEntity> EntityContainers = new Dictionary<Entity, NetworkEntity>();

  public void AddEntity(NetworkEntityData networkEntityData) {
    Container.AddedEntities.Add(networkEntityData);
  }

  public void RemoveEntity(EntityId networkSyncEntity) {
    Container.RemovedEntities.Add(networkSyncEntity);
  }

  public void AddComponent(Entity entity, int actorId, int networkId, Components componentData) {
    GetEntity(entity, actorId, networkId)
      .AddedComponents.Add(componentData);
  }

  public void RemoveComponent(Entity entity, int actorId, int networkId, int componentId) {
    GetEntity(entity, actorId, networkId)
      .RemovedComponents.Add(componentId);
  }

  public void SetComponentData(Entity entity, int actorId, int networkId, Components componentDataContainer) {
    GetEntity(entity, actorId, networkId)
      .Components.Add(componentDataContainer);
  }

  public void AddComponents(Entity entity, int actorId, int networkId, List<Components> componentIds) {
    GetEntity(entity, actorId, networkId)
      .AddedComponents.AddRange(componentIds);
  }

  public void RemoveComponents(Entity entity, int actorId, int networkId, List<int> componentIds) {
    GetEntity(entity, actorId, networkId)
      .RemovedComponents.AddRange(componentIds);
  }

  public void SetComponentData(Entity entity, int actorId, int networkId, List<Components> componentDataContainers) {
    GetEntity(entity, actorId, networkId)
      .Components.AddRange(componentDataContainers);
  }

  NetworkEntity GetEntity(Entity entity, int actorId, int networkId) {
    if (!EntityContainers.TryGetValue(entity, out NetworkEntity container)) {
      container = new NetworkEntity() {
        Id = new EntityId() {
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