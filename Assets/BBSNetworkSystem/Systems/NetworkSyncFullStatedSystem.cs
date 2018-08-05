using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(NetworkUpdateGroup))]
[UpdateAfter(typeof(NetworkReceiveSystem))]
public class NetworkSyncFullStatedSystem : ComponentSystem {

  struct AddedEntityData {
    public ComponentDataArray<NetworkSync> syncs;
    public ComponentDataArray<SyncState> states;
    public EntityArray entities;
    public readonly int Length;
  }
  [Inject] AddedEntityData added;

  //private readonly NetworkSyncDataContainer ownNetworkSyncDataContainer = new NetworkSyncDataContainer();
  //private readonly Dictionary<Entity, NetworkSyncDataEntityContainer> ownEntityContainerMap = new Dictionary<Entity, NetworkSyncDataEntityContainer>();
  //private readonly List<NetworkMethodInfo<NetworkSyncFullStatedSystem>> ComponentDataMethods = new List<NetworkMethodInfo<NetworkSyncFullStatedSystem>>();
  static bool isLogging;
  readonly SyncManager networkSendMessageUtility = new SyncManager();
  readonly List<NetworkInOutMethodInfo<NetworkSyncFullStatedSystem, Entity, NetworkComponent>> GetComponentDataMethods = new List<NetworkInOutMethodInfo<NetworkSyncFullStatedSystem, Entity, NetworkComponent>>();
  NetworkMessageSerializer<SyncEntities> messageSerializer;
  int lastSend = Environment.TickCount & Int32.MaxValue;
  INetworkManager networkManager;
  readonly List<int> jonedPlayer = new List<int>();
  readonly ReflectionUtility reflectionUtility = new ReflectionUtility();

  protected override void OnCreateManager(int capacity) {
    messageSerializer = new NetworkMessageSerializer<SyncEntities>();
    ComponentType[] componentTypes = reflectionUtility.ComponentTypes;
    GetComponentGroup(typeof(NetworkSync));
    Type networkSystemType = typeof(NetworkSyncFullStatedSystem);

    for (int i = 0; i < componentTypes.Length; i++) {
      var method = new NetworkInOutMethodInfo<NetworkSyncFullStatedSystem, Entity, NetworkComponent>(networkSystemType
        .GetMethod("GetComponentData", BindingFlags.Instance | BindingFlags.NonPublic)
        .MakeGenericMethod(componentTypes[i].GetManagedType()));

      GetComponentDataMethods.Add(method);
    }
  }

  protected override void OnDestroyManager() {
    messageSerializer.Dispose();
  }

  private void NetworkManager_OnPlayerJoined(int player) {
    if (player != networkManager.LocalPlayerID)
      jonedPlayer.Add(player);
  }

  private void NetworkManager_OnPlayerLeft(int player) {
    jonedPlayer.Remove(player);
  }


  protected override void OnUpdate() {
    if (!networkManager.IsConnectedAndReady) return;
    if (jonedPlayer.Count == 0 || !networkManager.IsMaster) {
      jonedPlayer.Clear();
      return;
    }

    Entities();
    SendData();
    jonedPlayer.Clear();
  }

  void Entities() {
    EntityArray entities = added.entities;
    ComponentDataArray<NetworkSync> networkSyncs = added.syncs;
    ComponentDataArray<SyncState> networkSyncStates = added.states;
    for (int i = 0; i < entities.Length; i++) {
      int instanceId = networkSyncs[i].instanceId;

      Entity entity = entities[i];
      NetworkEntity networkEntityData = new NetworkEntity {
        InstanceId = networkSyncs[i].instanceId,

        Id = new EntityId {
          ActorId = networkSyncStates[i].actorId,
          NetworkId = networkSyncStates[i].networkId,
        }
      };

      for (int j = 0; j < GetComponentDataMethods.Count; j++) {
        if (GetComponentDataMethods[j].Invoke(this, ref entity, out NetworkComponent componentData)) {
          networkEntityData.Components.Add(componentData);
        }
      }

      networkSendMessageUtility.AddEntity(networkEntityData);
    }
  }

  bool GetComponentData<T>(ref Entity entity, out NetworkComponent componentDataContainer) where T : struct, IComponentData {
    componentDataContainer = null;
    if (EntityManager.HasComponent<T>(entity)) {
      ComponentType componentType = ComponentType.Create<T>();
      int numberOfMembers = reflectionUtility.GetFieldCount(componentType.GetManagedType());

      T component = EntityManager.GetComponentData<T>(entity);
      NetworkField[] networkMemberInfos = reflectionUtility.GetFields(componentType);
      List<ComponentField> memberDataContainers = new List<ComponentField>();
      for (int i = 0; i < numberOfMembers; i++) {
        memberDataContainers.Add(new ComponentField() {
          Id = i,
          Value = (networkMemberInfos[i] as NetworkField<T>).GetValue(component)
        });
      }


      componentDataContainer = new NetworkComponent() {
        TypeId = reflectionUtility.GetId(componentType),
        Fields = memberDataContainers
      };
      return true;
    }
    return false;
  }

  void SendData() {
    NetworkEventOptions networkEventOptions = new NetworkEventOptions {
      TargetActors = jonedPlayer.ToArray(),
      Receiver = NetworkReceiverGroup.Target,
    };
    if (isLogging) {
      Debug.Log("SendFullState:\n" + NetworkMessageUtility.ToString(networkSendMessageUtility.SyncEntities));
    }
    networkManager.SendMessage(NetworkEvents.DataSync, messageSerializer.Serialize(networkSendMessageUtility.SyncEntities), true, networkEventOptions);
    networkSendMessageUtility.Reset();
  }

  internal void SetNetworkManager(INetworkManager networkManager) {
    if (this.networkManager != null) {
      this.networkManager.OnPlayerJoined -= NetworkManager_OnPlayerJoined;
      this.networkManager.OnPlayerLeft -= NetworkManager_OnPlayerLeft;
    }
    this.networkManager = networkManager;

    if (this.networkManager != null) {
      networkManager.OnPlayerJoined += NetworkManager_OnPlayerJoined;
      networkManager.OnPlayerLeft += NetworkManager_OnPlayerLeft;
    }
  }
}