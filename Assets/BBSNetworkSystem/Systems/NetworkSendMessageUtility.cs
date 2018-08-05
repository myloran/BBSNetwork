using System.Collections.Generic;
using Unity.Entities;

public class NetworkSendMessageUtility {
  public readonly NetworkSyncDataContainer DataContainer = new NetworkSyncDataContainer();
  readonly Dictionary<Entity, NetworkSyncDataEntityContainer> EntityContainerMap = new Dictionary<Entity, NetworkSyncDataEntityContainer>();

  public void AddEntity(NetworkEntityData networkEntityData) {
    DataContainer.AddedNetworkSyncEntities.Add(networkEntityData);
  }

  public void RemoveEntity(NetworkSyncEntity networkSyncEntity) {
    DataContainer.RemovedNetworkSyncEntities.Add(networkSyncEntity);
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

  NetworkSyncDataEntityContainer GetEntity(Entity entity, int actorId, int networkId) {
    if (!EntityContainerMap.TryGetValue(entity, out NetworkSyncDataEntityContainer dataContainer)) {
      dataContainer = new NetworkSyncDataEntityContainer() {
        NetworkSyncEntity = new NetworkSyncEntity() {
          ActorId = actorId,
          NetworkId = networkId,
        }
      };
      DataContainer.NetworkSyncDataEntities.Add(dataContainer);
      EntityContainerMap.Add(entity, dataContainer);
    }
    return dataContainer;
  }

  public void Reset() {
    DataContainer.AddedNetworkSyncEntities.Clear();
    DataContainer.RemovedNetworkSyncEntities.Clear();
    DataContainer.NetworkSyncDataEntities.Clear();
    EntityContainerMap.Clear();
  }
}