using System.Collections.Generic;
using Unity.Entities;

public class NetworkSendMessageUtility {
  public readonly NetworkSyncDataContainer Container = new NetworkSyncDataContainer();
  readonly Dictionary<Entity, SyncEntity> SyncEntities = new Dictionary<Entity, SyncEntity>();

  public void AddEntity(NetworkEntity networkEntityData) {
    Container.AddedEntities.Add(networkEntityData);
  }

  public void RemoveEntity(EntityId networkSyncEntity) {
    Container.RemovedEntities.Add(networkSyncEntity);
  }

  public void AddComponent(Entity entity, int actorId, int networkId, NetworkComponent componentData) {
    GetEntity(entity, actorId, networkId)
      .AddedComponents.Add(componentData);
  }

  public void RemoveComponent(Entity entity, int actorId, int networkId, int componentId) {
    GetEntity(entity, actorId, networkId)
      .RemovedComponents.Add(componentId);
  }

  public void SetComponentData(Entity entity, int actorId, int networkId, NetworkComponent componentDataContainer) {
    GetEntity(entity, actorId, networkId)
      .Components.Add(componentDataContainer);
  }

  public void AddComponents(Entity entity, int actorId, int networkId, List<NetworkComponent> componentIds) {
    GetEntity(entity, actorId, networkId)
      .AddedComponents.AddRange(componentIds);
  }

  public void RemoveComponents(Entity entity, int actorId, int networkId, List<int> componentIds) {
    GetEntity(entity, actorId, networkId)
      .RemovedComponents.AddRange(componentIds);
  }

  public void SetComponentData(Entity entity, int actorId, int networkId, List<NetworkComponent> componentDataContainers) {
    GetEntity(entity, actorId, networkId)
      .Components.AddRange(componentDataContainers);
  }

  SyncEntity GetEntity(Entity entity, int actorId, int networkId) {
    if (!SyncEntities.TryGetValue(entity, out SyncEntity syncEntity)) {
      syncEntity = new SyncEntity() {
        Id = new EntityId() {
          ActorId = actorId,
          NetworkId = networkId,
        }
      };
      Container.Entities.Add(syncEntity);
      SyncEntities[entity] = syncEntity;
    }
    return syncEntity;
  }

  public void Reset() {
    Container.AddedEntities.Clear();
    Container.RemovedEntities.Clear();
    Container.Entities.Clear();
    SyncEntities.Clear();
  }
}