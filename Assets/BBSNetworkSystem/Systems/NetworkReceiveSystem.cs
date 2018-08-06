using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[AlwaysUpdateSystem]
[UpdateInGroup(typeof(NetworkUpdateGroup))]
public class NetworkReceiveSystem : ComponentSystem {
  public static bool LogReceivedMessages;
  const float DeltaTimeMessage = NetworkSendSystem.SendInterval / 1000f;
  NetworkMessageSerializer<SyncEntities> messageSerializer;
  INetworkManager networkManager;
  readonly Dictionary<ComponentType, NetworkMethodInfo<NetworkReceiveSystem, Entity, List<ComponentField>>> AddComponentsMethods = new Dictionary<ComponentType, NetworkMethodInfo<NetworkReceiveSystem, Entity, List<ComponentField>>>();
  readonly Dictionary<ComponentType, NetworkMethodInfo<NetworkReceiveSystem, Entity>> RemoveComponentsMethods = new Dictionary<ComponentType, NetworkMethodInfo<NetworkReceiveSystem, Entity>>();
  readonly Dictionary<ComponentType, NetworkMethodInfo<NetworkReceiveSystem, Entity, List<ComponentField>>> SetComponentsMethods = new Dictionary<ComponentType, NetworkMethodInfo<NetworkReceiveSystem, Entity, List<ComponentField>>>();
  readonly List<NetworkMethodInfo<NetworkReceiveSystem, Entity>> RemoveComponentOnDestroyEntityMethods = new List<NetworkMethodInfo<NetworkReceiveSystem, Entity>>();
  readonly List<NetworkMethod<NetworkReceiveSystem>> UpdateComponentsMethods = new List<NetworkMethod<NetworkReceiveSystem>>();
  NetworkFactory networkFactory;
  readonly ReflectionUtility reflectionUtility = new ReflectionUtility();
  readonly List<GameObject> gameObjectsToDestroy = new List<GameObject>();

  protected override void OnCreateManager(int capacity) {
    networkFactory = new NetworkFactory(EntityManager);
    var types = reflectionUtility.ComponentTypes;
    var type = typeof(NetworkReceiveSystem);

    for (int i = 0; i < types.Length; i++) {
      var addMethod = GetMethod(types[i], type, "AddComponent");
      var addInfo = new NetworkMethodInfo<NetworkReceiveSystem, Entity, List<ComponentField>>(addMethod);
      AddComponentsMethods.Add(types[i], addInfo);

      var removeMethod = GetMethod(types[i], type, "RemoveComponent");
      var removeInfo = new NetworkMethodInfo<NetworkReceiveSystem, Entity>(removeMethod);
      RemoveComponentsMethods.Add(types[i], removeInfo);

      var setMethod = GetMethod(types[i], type, "SetComponent");
      var setInfo = new NetworkMethodInfo<NetworkReceiveSystem, Entity, List<ComponentField>>(setMethod);
      SetComponentsMethods.Add(types[i], setInfo);

      var updateMethod = GetMethod(types[i], type, "UpdateComponent");
      var updateInfo = new NetworkMethod<NetworkReceiveSystem>(updateMethod);
      UpdateComponentsMethods.Add(updateInfo);

      var destroyMethod = GetMethod(types[i], type, "RemoveComponentOnDestroyEntity");
      var destroyInfo = new NetworkMethodInfo<NetworkReceiveSystem, Entity>(destroyMethod);
      RemoveComponentOnDestroyEntityMethods.Add(destroyInfo);
    }
    messageSerializer = new NetworkMessageSerializer<SyncEntities>();
  }

  MethodInfo GetMethod(ComponentType componentType, Type type, string methodName) =>
    type
      .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
      .MakeGenericMethod(componentType.GetManagedType());

  protected override void OnDestroyManager() {
    messageSerializer.Dispose();
    networkFactory.Dispose();
  }

  protected override void OnUpdate() {
    //return;
    networkManager.Update();
    for (int i = 0; i < UpdateComponentsMethods.Count; i++)
      UpdateComponentsMethods[i].Invoke(this);

    for (int i = 0; i < gameObjectsToDestroy.Count; i++)
      UnityEngine.Object.Destroy(gameObjectsToDestroy[i]);

    gameObjectsToDestroy.Clear();
    networkFactory.FlushNetworkManager();
  }

  void NetworkManager_OnEventData(byte eventId, int playerId, object data) {
    switch (eventId) {
      case NetworkEvents.DataSync:
        ReceiveNetworkUpdate((byte[])data);
        break;
    }
  }

  void NetworkManager_OnPlayerLeft(int actorId) {
    var group = GetComponentGroup(ComponentType.Create<SyncState>());
    var components = group.GetComponentDataArray<SyncState>();
    var entities = group.GetEntityArray();

    for (int i = 0; i < entities.Length; i++) {
      if (components[i].actorId != actorId) continue;

      PostUpdateCommands.RemoveComponent<SyncState>(entities[i]);
      PostUpdateCommands.DestroyEntity(entities[i]);

      if (EntityManager.HasComponent<Transform>(entities[i])) {
        var gameObject = EntityManager.GetComponentObject<Transform>(entities[i]).gameObject;
        gameObjectsToDestroy.Add(gameObject);
      }
      for (int j = 0; j < RemoveComponentOnDestroyEntityMethods.Count; j++)
        RemoveComponentOnDestroyEntityMethods[j].Invoke(this, entities[i]);
    }
  }

  void NetworkManager_OnDisconnect() {
    var group = GetComponentGroup(ComponentType.Create<SyncState>());
    var components = group.GetComponentDataArray<SyncState>();
    var entities = group.GetEntityArray();

    for (int i = 0; i < entities.Length; i++) {
      PostUpdateCommands.RemoveComponent<SyncState>(entities[i]);

      if (components[i].actorId != networkManager.LocalPlayerID) {
        PostUpdateCommands.DestroyEntity(entities[i]);
        if (EntityManager.HasComponent<Transform>(entities[i])) {
          var gameObject = EntityManager.GetComponentObject<Transform>(entities[i]).gameObject;
          gameObjectsToDestroy.Add(gameObject);
        }
      } else {
        PostUpdateCommands.RemoveComponent<NetworktOwner>(entities[i]);
      }
      for (int j = 0; j < RemoveComponentOnDestroyEntityMethods.Count; j++)
        RemoveComponentOnDestroyEntityMethods[j].Invoke(this, entities[i]);
    }
  }

  void ReceiveNetworkUpdate(byte[] data) {
    var container = messageSerializer.Deserialize(data);
    if (LogReceivedMessages && (container.Added.Any()
        || container.Removed.Any()
        || container.Entities.Any())) {
      Debug.Log("ReceiveNetworkUpdate: " + NetworkMessageUtility.ToString(container));
    }
    var group = GetComponentGroup(ComponentType.Create<SyncState>());
    var components = group.GetComponentDataArray<SyncState>();
    var entities = group.GetEntityArray();
    var map = new NativeHashMap<int, int>(entities.Length, Allocator.Temp);

    for (int i = 0; i < entities.Length; i++) {
      int hash = GetHash(components[i].actorId, components[i].networkId);
      map.TryAdd(hash, i);
    }

    var addedEntities = container.Added;
    for (int i = 0; i < addedEntities.Count; i++) {
      if (addedEntities[i].Id.ActorId == networkManager.LocalPlayerID) continue;

      var addedEntity = reflectionUtility
        .GetSpawn(addedEntities[i].InstanceId)
        .Invoke(EntityManager);

      var state = new SyncState {
        actorId = addedEntities[i].Id.ActorId,
        networkId = addedEntities[i].Id.NetworkId,
      };
      PostUpdateCommands.AddComponent(addedEntity, state);

      var componentData = addedEntities[i].Components;
      for (int j = 0; j < componentData.Count; j++) {
        var componentType = reflectionUtility
          .GetType(componentData[j].TypeId);
        AddComponentsMethods[componentType]
          .Invoke(this, addedEntity, componentData[j].Fields);
      }
      if (addedEntities[i].Id.ActorId != networkManager.LocalPlayerID)
        NetworkSendSystem.AllNetworkSendMessageUtility.AddEntity(addedEntities[i]);
    }

    // removed Entities
    var removedEntities = container.Removed;
    for (int i = 0; i < removedEntities.Count; i++) {
      if (removedEntities[i].ActorId == networkManager.LocalPlayerID) continue;

      int hash = GetHash(removedEntities[i].ActorId, removedEntities[i].NetworkId);
      if (map.TryGetValue(hash, out int index)) {
        var entity = entities[index];
        PostUpdateCommands.RemoveComponent<SyncState>(entity);
        PostUpdateCommands.DestroyEntity(entity);

        if (EntityManager.HasComponent<Transform>(entity)) {
          var gameObject = EntityManager.GetComponentObject<Transform>(entity).gameObject;
          gameObjectsToDestroy.Add(gameObject);
        }
        for (int j = 0; j < RemoveComponentOnDestroyEntityMethods.Count; j++)
          RemoveComponentOnDestroyEntityMethods[j].Invoke(this, entity);
      }
      if (removedEntities[i].ActorId != networkManager.LocalPlayerID)
        NetworkSendSystem.AllNetworkSendMessageUtility.RemoveEntity(removedEntities[i]);
    }

    // update components
    var updateEntities = container.Entities;
    for (int i = 0; i < updateEntities.Count; i++) {
      var updateEntity = updateEntities[i].Id;
      if (updateEntity.ActorId == networkManager.LocalPlayerID) continue;

      int hash = GetHash(updateEntity.ActorId, updateEntity.NetworkId);
      if (!map.TryGetValue(hash, out int index)) continue;

      var entity = entities[index];
      var addedComponents = updateEntities[i].AddedComponents;
      var removedComponents = updateEntities[i].RemovedComponents;
      var componentData = updateEntities[i].Components;
      for (int j = 0; j < addedComponents.Count; j++) {
        var componentType = reflectionUtility.GetType(addedComponents[j].TypeId);
        AddComponentsMethods[componentType].Invoke(this, entity, addedComponents[j].Fields);
      }
      for (int j = 0; j < componentData.Count; j++) {
        var componentType = reflectionUtility.GetType(componentData[j].TypeId);
        SetComponentsMethods[componentType].Invoke(this, entity, componentData[j].Fields);
      }
      for (int j = 0; j < removedComponents.Count; j++) {
        var componentType = reflectionUtility.GetType(removedComponents[j]);
        RemoveComponentsMethods[componentType].Invoke(this, entity);
      }
      if (updateEntity.ActorId == networkManager.LocalPlayerID) continue;

      NetworkSendSystem.AllNetworkSendMessageUtility.AddComponents(
        entity, updateEntity.ActorId, updateEntity.NetworkId, addedComponents);
      NetworkSendSystem.AllNetworkSendMessageUtility.RemoveComponents(
        entity, updateEntity.ActorId, updateEntity.NetworkId, removedComponents);
      NetworkSendSystem.AllNetworkSendMessageUtility.SetComponent(
        entity, updateEntity.ActorId, updateEntity.NetworkId, componentData);
    }
    map.Dispose();
  }

  static int GetHash(int actorId, int networkId) {
    return (int)unchecked(math.pow(actorId, networkId));
  }

  void AddComponent<T>(Entity entity, List<ComponentField> memberDataContainers) where T : struct, IComponentData {
    //Debug.Log(typeof(T));
    int numberOfMembers = reflectionUtility.GetFieldsCount(typeof(T));
    var infos = reflectionUtility.GetFields(ComponentType.Create<T>());
    if (!EntityManager.HasComponent<T>(entity)) {
      T component = new T();
      for (int i = 0; i < memberDataContainers.Count; i++) {
        int value = memberDataContainers[i].Value;
        (infos[i] as NetworkField<T>)
          .SetValue(ref component, value, value, Time.deltaTime, NetworkSendSystem.SendInterval);
      }
      PostUpdateCommands.AddComponent(entity, component);
    }
    if (EntityManager.HasComponent<NetworkComponentState<T>>(entity)) return;

    var syncEntity = networkFactory.CreateNetworkComponentData<T>(entity, numberOfMembers);
    var values = networkFactory.NetworkEntityManager.GetFixedArray<int>(syncEntity);
    for (int i = 0; i < memberDataContainers.Count; i++) {
      int index = i * 2;
      values[index] = memberDataContainers[i].Value;
      values[index + 1] = memberDataContainers[i].Value;
    }
    PostUpdateCommands.AddComponent(entity, new NetworkComponentState<T>());
  }

  void RemoveComponent<T>(Entity entity) where T : struct, IComponentData {
    if (EntityManager.HasComponent<T>(entity))
      PostUpdateCommands.RemoveComponent<T>(entity);

    if (EntityManager.HasComponent<NetworkComponentState<T>>(entity)) {
      PostUpdateCommands.RemoveComponent<NetworkComponentState<T>>(entity);
      var dataEntity = EntityManager.GetComponentData<NetworkComponentState<T>>(entity).dataEntity;
      PostUpdateCommands.DestroyEntity(dataEntity);
    }
  }

  void RemoveComponentOnDestroyEntity<T>(Entity entity) where T : struct, IComponentData {
    if (EntityManager.HasComponent<NetworkComponentState<T>>(entity)) {
      PostUpdateCommands.RemoveComponent<NetworkComponentState<T>>(entity);
      var dataEntity = EntityManager.GetComponentData<NetworkComponentState<T>>(entity).dataEntity;
      PostUpdateCommands.DestroyEntity(dataEntity);
    }
  }

  void SetComponent<T>(Entity entity, List<ComponentField> memberDataContainers) {
    if (!EntityManager.HasComponent<NetworkComponentState<T>>(entity)) return;

    var dataEntity = EntityManager.GetComponentData<NetworkComponentState<T>>(entity).dataEntity;
    var values = EntityManager.GetFixedArray<int>(dataEntity);

    for (int i = 0; i < memberDataContainers.Count; i++) {
      int index = memberDataContainers[i].Id * 2;
      values[index] = values[index + 1];
      values[index + 1] = memberDataContainers[i].Value;
    }
  }

  void UpdateComponent<T>() where T : struct, IComponentData {
    var group = GetComponentGroup(ComponentType.ReadOnly<NetworkSync>(), ComponentType.Create<T>(), ComponentType.ReadOnly<NetworkComponentState<T>>(), ComponentType.Subtractive<NetworktOwner>());
    var entities = group.GetEntityArray();
    var components = group.GetComponentDataArray<T>();
    var states = group.GetComponentDataArray<NetworkComponentState<T>>();
    var infos = reflectionUtility.GetFields(ComponentType.Create<T>());

    for (int i = 0; i < entities.Length; i++) {
      T component = components[i];
      //Debug.Log(componentStates[i].dataEntity);
      NativeArray<int> values = EntityManager.GetFixedArray<int>(states[i].dataEntity);
      for (int j = 0; j < values.Length; j += 2) {
        (infos[j / 2] as NetworkField<T>)
          .SetValue(ref component, values[j], values[j + 1], Time.deltaTime, DeltaTimeMessage);
      }
      components[i] = component;
    }
  }


  internal void SetNetworkManager(INetworkManager networkManager) {
    if (this.networkManager != null) {
      this.networkManager.OnEventData -= NetworkManager_OnEventData;
      this.networkManager.OnPlayerLeft -= NetworkManager_OnPlayerLeft;
      this.networkManager.OnDisconnected -= NetworkManager_OnDisconnect;
    }
    this.networkManager = networkManager;
    if (networkManager == null) return;

    networkManager.OnEventData += NetworkManager_OnEventData;
    this.networkManager.OnPlayerLeft += NetworkManager_OnPlayerLeft;
    this.networkManager.OnDisconnected += NetworkManager_OnDisconnect;
  }
}