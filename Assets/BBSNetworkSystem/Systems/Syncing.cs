using System.Collections.Generic;
using Unity.Entities;

public class Syncing {
  public readonly SyncEntities SyncEntities = new SyncEntities();
  readonly Dictionary<Entity, SyncEntity> Entities = new Dictionary<Entity, SyncEntity>();

  public void AddEntity(NetworkEntity entity) {
    SyncEntities.Added.Add(entity);
  }

  public void RemoveEntity(EntityId id) {
    SyncEntities.Removed.Add(id);
  }

  public void AddComponent(Entity entity, int actorId, int networkId, NetworkComponent component) {
    GetEntity(entity, actorId, networkId)
      .AddedComponents.Add(component);
  }

  public void RemoveComponent(Entity entity, int actorId, int networkId, int componentId) {
    GetEntity(entity, actorId, networkId)
      .RemovedComponents.Add(componentId);
  }

  public void SetComponentData(Entity entity, int actorId, int networkId, NetworkComponent component) {
    GetEntity(entity, actorId, networkId)
      .Components.Add(component);
  }

  public void AddComponents(Entity entity, int actorId, int networkId, List<NetworkComponent> componentIds) {
    GetEntity(entity, actorId, networkId)
      .AddedComponents.AddRange(componentIds);
  }

  public void RemoveComponents(Entity entity, int actorId, int networkId, List<int> componentIds) {
    GetEntity(entity, actorId, networkId)
      .RemovedComponents.AddRange(componentIds);
  }

  public void SetComponentData(Entity entity, int actorId, int networkId, List<NetworkComponent> components) {
    GetEntity(entity, actorId, networkId)
      .Components.AddRange(components);
  }

  SyncEntity GetEntity(Entity entity, int actorId, int networkId) {
    if (!Entities.TryGetValue(entity, out SyncEntity syncEntity)) {
      syncEntity = new SyncEntity() {
        Id = new EntityId() {
          ActorId = actorId,
          NetworkId = networkId,
        }
      };
      SyncEntities.Entities.Add(syncEntity);
      Entities[entity] = syncEntity;
    }
    return syncEntity;
  }

  public void Reset() {
    SyncEntities.Added.Clear();
    SyncEntities.Removed.Clear();
    SyncEntities.Entities.Clear();
    Entities.Clear();
  }
}