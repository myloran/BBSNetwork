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
[UpdateAfter(typeof(NetworkReceiveSystem))]
public class NetworkSendSystem : ComponentSystem {

    struct AddedEntityData {
        public ComponentDataArray<NetworkSync> networkSyncComponents;
        public SubtractiveComponent<NetworkSyncState> networkSyncStateComponents;
        public EntityArray entities;
        public readonly int Length;
    }

    struct RemovedEntityData {
        public SubtractiveComponent<NetworkSync> networkSyncComponents;
        public ComponentDataArray<NetworkSyncState> networkSyncStateComponents;
        public EntityArray entities;
        public readonly int Length;
    }

    [Inject] AddedEntityData addedSyncEntities;
    [Inject] RemovedEntityData removedSyncEntities;

    public static bool LogSendMessages;

    private NetworkFactory networkFactory;

    //private readonly NetworkSyncDataContainer ownNetworkSyncDataContainer = new NetworkSyncDataContainer();
    //private readonly Dictionary<Entity, NetworkSyncDataEntityContainer> ownEntityContainerMap = new Dictionary<Entity, NetworkSyncDataEntityContainer>();
    private readonly Syncing ownNetworkSendMessageUtility = new Syncing();
    internal static readonly Syncing AllNetworkSendMessageUtility = new Syncing();


    private readonly List<NetworkMethodInfo<NetworkSendSystem>> AddedComponentsMethods = new List<NetworkMethodInfo<NetworkSendSystem>>();
    private readonly List<NetworkMethodInfo<NetworkSendSystem>> RemovedComponentsMethods = new List<NetworkMethodInfo<NetworkSendSystem>>();
    private readonly List<NetworkMethodInfo<NetworkSendSystem>> UpdateComponentsMethods = new List<NetworkMethodInfo<NetworkSendSystem>>();
    private readonly List<NetworkInOutMethodInfo<NetworkSendSystem, Entity, NetworkComponent>> AddComponentDataOnEntityAddedMethods = new List<NetworkInOutMethodInfo<NetworkSendSystem, Entity, NetworkComponent>>();
    private readonly List<NetworkMethodInfo<NetworkSendSystem, Entity>> RemoveComponentOnDestroyEntityMethods = new List<NetworkMethodInfo<NetworkSendSystem, Entity>>();

    private NetworkMessageSerializer<SyncEntities> messageSerializer;
    private int lastSend = (Environment.TickCount - SendInterval) & Int32.MaxValue;
    private INetworkManager networkManager;
    internal const int SendInterval = 100;
    private readonly ReflectionUtility reflectionUtility = new ReflectionUtility();

    internal static string LastSendMessage { get; private set; }


    protected override void OnCreateManager(int capacity) {
        messageSerializer = new NetworkMessageSerializer<SyncEntities>();
        ComponentType[] componentTypes = reflectionUtility.ComponentTypes;
        networkFactory = new NetworkFactory(EntityManager);
        Type networkSystemType = typeof(NetworkSendSystem);
        for (int i = 0; i < componentTypes.Length; i++) {
            AddedComponentsMethods.Add(
                new NetworkMethodInfo<NetworkSendSystem>(networkSystemType
                    .GetMethod("AddedComponents", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(componentTypes[i].GetManagedType())));

            RemovedComponentsMethods.Add(
                new NetworkMethodInfo<NetworkSendSystem>(networkSystemType
                    .GetMethod("RemovedComponents", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(componentTypes[i].GetManagedType())));

            UpdateComponentsMethods.Add(
                new NetworkMethodInfo<NetworkSendSystem>(networkSystemType
                    .GetMethod("UpdateDataState", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(componentTypes[i].GetManagedType())));

            AddComponentDataOnEntityAddedMethods.Add(
               new NetworkInOutMethodInfo<NetworkSendSystem, Entity, NetworkComponent>(networkSystemType
                   .GetMethod("AddComponentDataOnEntityAdded", BindingFlags.Instance | BindingFlags.NonPublic)
                   .MakeGenericMethod(componentTypes[i].GetManagedType())));

            RemoveComponentOnDestroyEntityMethods.Add(
                new NetworkMethodInfo<NetworkSendSystem, Entity>(networkSystemType
                    .GetMethod("RemoveComponentOnDestroyEntity", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(componentTypes[i].GetManagedType())));
        }


    }

    protected override void OnDestroyManager() {
        messageSerializer.Dispose();
        networkFactory.Dispose();
    }

    protected override void OnUpdate() {
        if (!networkManager.IsConnectedAndReady) {
            return;
        }

        AddedEntities();
        RemovedEntities();

        for (int i = 0; i < AddedComponentsMethods.Count; i++) {
            AddedComponentsMethods[i].Invoke(this);
        }

        for (int i = 0; i < RemovedComponentsMethods.Count; i++) {
            RemovedComponentsMethods[i].Invoke(this);
        }

        int currentTime = Environment.TickCount & Int32.MaxValue;
        if (math.abs(currentTime - lastSend) > SendInterval) {

            lastSend = currentTime;
            for (int i = 0; i < UpdateComponentsMethods.Count; i++) {
                UpdateComponentsMethods[i].Invoke(this);
            }

            SendData();
        }

        networkFactory.FlushNetworkManager();

    }
    
    private void AddedEntities() {
        EntityArray entities = addedSyncEntities.entities;
        ComponentDataArray<NetworkSync> networkSyncs = addedSyncEntities.networkSyncComponents;

        for (int i = 0; i < entities.Length; i++) {
            int instanceId = networkSyncs[i].instanceId;
            NetworkSyncState component = new NetworkSyncState() {
                actorId = networkManager.LocalPlayerID,
                networkId = networkManager.GetNetworkId(),
            };
            Entity entity = entities[i];
            PostUpdateCommands.AddComponent(entity, component);
            PostUpdateCommands.AddComponent(entity, new NetworktOwner());

            NetworkEntity networkEntityData = new NetworkEntity {
                InstanceId = networkSyncs[i].instanceId,

                Id = new EntityId {
                    ActorId = component.actorId,
                    NetworkId = component.networkId,
                }
            };

            for (int j = 0; j < AddComponentDataOnEntityAddedMethods.Count; j++) {
                if(AddComponentDataOnEntityAddedMethods[j].Invoke(this, ref entity, out NetworkComponent componentData)) {
                    networkEntityData.Components.Add(componentData);
                }
            }

            ownNetworkSendMessageUtility.AddEntity(networkEntityData);
            AllNetworkSendMessageUtility.AddEntity(networkEntityData);            
        }
    }

    private void RemovedEntities() {
        EntityArray entities = removedSyncEntities.entities;
        ComponentDataArray<NetworkSyncState> networkSyncs = removedSyncEntities.networkSyncStateComponents;

        for (int i = 0; i < entities.Length; i++) {
            NetworkSyncState component = new NetworkSyncState() {
                actorId = networkManager.LocalPlayerID,
                networkId = networkManager.GetNetworkId(),
            };
            PostUpdateCommands.RemoveComponent<NetworkSyncState>(entities[i]);
            for (int j = 0; j < RemoveComponentOnDestroyEntityMethods.Count; j++) {
                RemoveComponentOnDestroyEntityMethods[j].Invoke(this, entities[i]);
            }

            EntityId networkSyncEntity = new EntityId {
                ActorId = component.actorId,
                NetworkId = component.networkId,
            };
            ownNetworkSendMessageUtility.RemoveEntity(networkSyncEntity);
            AllNetworkSendMessageUtility.RemoveEntity(networkSyncEntity);
        }
    }

    void RemoveComponentOnDestroyEntity<T>(Entity entity) where T : struct, IComponentData {
        if (EntityManager.HasComponent<NetworkComponentState<T>>(entity)) {
            PostUpdateCommands.RemoveComponent<NetworkComponentState<T>>(entity);
            PostUpdateCommands.DestroyEntity(EntityManager.GetComponentData<NetworkComponentState<T>>(entity).dataEntity);
        }
    }

    private bool AddComponentDataOnEntityAdded<T>(ref Entity entity, out NetworkComponent componentDataContainer) where T : struct, IComponentData {
        componentDataContainer = null;
        if (EntityManager.HasComponent<T>(entity)) {
            ComponentType componentType = ComponentType.Create<T>();
            int numberOfMembers = reflectionUtility.GetNumberOfMembers(componentType.GetManagedType());
            Entity networkDataEntity = networkFactory.CreateNetworkComponentData<T>(entity, numberOfMembers);
            NativeArray<int> values = networkFactory.NetworkEntityManager.GetFixedArray<int>(networkDataEntity);
            PostUpdateCommands.AddComponent(entity, new NetworkComponentState<T>());

            T component = EntityManager.GetComponentData<T>(entity);
            NetworkMemberInfo[] networkMemberInfos = reflectionUtility.GetNetworkMemberInfo(componentType);
            List<ComponentField> memberDataContainers = new List<ComponentField>();
            for (int i = 0; i < numberOfMembers; i++) {
                int value = (networkMemberInfos[i] as NetworkMemberInfo<T>).GetValue(component);
                memberDataContainers.Add(new ComponentField() {
                    Id = i,
                    Value = value
                });
                values[i] = value;
            }

            componentDataContainer = new NetworkComponent() {
                TypeId = reflectionUtility.GetComponentTypeID(componentType),
                Fields = memberDataContainers
            };
            return true;
        }
        return false;
    }
     
    private void AddedComponents<T>() where T : struct, IComponentData {
        ComponentType componentType = ComponentType.Create<T>();
        ComponentGroup group = GetComponentGroup(ComponentType.Create<NetworkSyncState>(), componentType, ComponentType.Subtractive<NetworkComponentState<T>>(), ComponentType.Create<NetworktOwner>());
        ComponentDataArray<T> components = group.GetComponentDataArray<T>();
        ComponentDataArray<NetworkSyncState> networkSyncStateComponents = group.GetComponentDataArray<NetworkSyncState>();
        EntityArray entities = group.GetEntityArray();

        NetworkMemberInfo[] networkMemberInfos = reflectionUtility.GetNetworkMemberInfo(componentType);

        for (int i = 0; i < entities.Length; i++) {
            NetworkSyncState networkSyncState = networkSyncStateComponents[i];
            NetworkComponent componentData = new NetworkComponent {
                TypeId = reflectionUtility.GetComponentTypeID(componentType)
            };

            T component = components[i];
            for (int j = 0; j < networkMemberInfos.Length; j++) {
                componentData.Fields.Add(new ComponentField {
                    Id = j,
                    Value = (networkMemberInfos[j] as NetworkMemberInfo<T>).GetValue(component),
                });
            }


            ownNetworkSendMessageUtility.AddComponent(entities[i], networkSyncState.actorId, networkSyncState.networkId, componentData);
            AllNetworkSendMessageUtility.AddComponent(entities[i], networkSyncState.actorId, networkSyncState.networkId, componentData);

            int numberOfMembers = reflectionUtility.GetNumberOfMembers(componentType.GetManagedType());
            networkFactory.CreateNetworkComponentData<T>(entities[i], numberOfMembers);
            PostUpdateCommands.AddComponent(entities[i], new NetworkComponentState<T>());
        }
    }

    private void RemovedComponents<T>() where T : IComponentData {
        ComponentType componentType = ComponentType.Create<T>();
        ComponentGroup group = GetComponentGroup(ComponentType.Create<NetworkSyncState>(), ComponentType.Subtractive<T>(), ComponentType.Create<NetworkComponentState<T>>(), ComponentType.Create<NetworktOwner>());
        ComponentDataArray<NetworkSyncState> networkSyncStateComponents = group.GetComponentDataArray<NetworkSyncState>();
        ComponentDataArray<NetworkComponentState<T>> networkComponentStates = group.GetComponentDataArray<NetworkComponentState<T>>();
        EntityArray entities = group.GetEntityArray();

        for (int i = 0; i < entities.Length; i++) {
            NetworkSyncState networkSyncState = networkSyncStateComponents[i];
            ownNetworkSendMessageUtility.RemoveComponent(entities[i], networkSyncState.actorId, networkSyncState.networkId, reflectionUtility.GetComponentTypeID(componentType));
            AllNetworkSendMessageUtility.RemoveComponent(entities[i], networkSyncState.actorId, networkSyncState.networkId, reflectionUtility.GetComponentTypeID(componentType));

            PostUpdateCommands.DestroyEntity(networkComponentStates[i].dataEntity);
            PostUpdateCommands.RemoveComponent<NetworkComponentState<T>>(entities[i]);
        }
    }

    private void UpdateDataState<T>() where T : struct, IComponentData {
        ComponentType componentType = ComponentType.Create<T>();
        ComponentGroup group = GetComponentGroup(ComponentType.Create<NetworkSyncState>(), componentType, ComponentType.Create<NetworkComponentState<T>>(), ComponentType.Create<NetworktOwner>());
        ComponentDataArray<NetworkSyncState> networkSyncStateComponents = group.GetComponentDataArray<NetworkSyncState>();
        ComponentDataArray<T> networkComponents = group.GetComponentDataArray<T>();
        ComponentDataArray<NetworkComponentState<T>> networkComponentStates = group.GetComponentDataArray<NetworkComponentState<T>>();
        EntityArray entities = group.GetEntityArray();

        NetworkMemberInfo[] networkMemberInfos = reflectionUtility.GetNetworkMemberInfo(componentType);
        for (int i = 0; i < entities.Length; i++) {            
            NativeArray<int> values = EntityManager.GetFixedArray<int>(networkComponentStates[i].dataEntity);
            NetworkComponent componentDataContainer = new NetworkComponent {
                TypeId = reflectionUtility.GetComponentTypeID(componentType),
            };
            for (int j = 0; j < networkMemberInfos.Length; j++) {
                NetworkMemberInfo<T> networkMemberInfo = (networkMemberInfos[j] as NetworkMemberInfo<T>);
                if (networkMemberInfo.syncAttribute.InitOnly) {
                    continue;
                }

                int newValue = networkMemberInfo.GetValue(networkComponents[i]);  
                if(newValue != values[j]) {
                    componentDataContainer.Fields.Add(new ComponentField {
                        Id = j,
                        Value = newValue,
                    });
                }
                values[j] = newValue;
            }

            if (componentDataContainer.Fields.Count != 0) {
                NetworkSyncState networkSyncState = networkSyncStateComponents[i];
                ownNetworkSendMessageUtility.SetComponent(entities[i], networkSyncState.actorId, networkSyncState.networkId, componentDataContainer);
                AllNetworkSendMessageUtility.SetComponent(entities[i], networkSyncState.actorId, networkSyncState.networkId, componentDataContainer);
            }
        }
    }

    private void SendData() {
        NetworkEventOptions networkEventOptions = new NetworkEventOptions();
        byte[] data;
        if (networkManager.IsMaster) {
            if (!AllNetworkSendMessageUtility.SyncEntities.Added.Any()
                && !AllNetworkSendMessageUtility.SyncEntities.Removed.Any()
                && !AllNetworkSendMessageUtility.SyncEntities.Entities.Any()) {
                return;
            }

            networkEventOptions.Receiver = NetworkReceiverGroup.Others;
            if (LogSendMessages) {
                LastSendMessage = NetworkMessageUtility.ToString(AllNetworkSendMessageUtility.SyncEntities);
            }
            data = messageSerializer.Serialize(AllNetworkSendMessageUtility.SyncEntities);
        } else {
            if (!ownNetworkSendMessageUtility.SyncEntities.Added.Any()
                && !ownNetworkSendMessageUtility.SyncEntities.Removed.Any()
                && !ownNetworkSendMessageUtility.SyncEntities.Entities.Any()) {
                return;
            }

            networkEventOptions.Receiver = NetworkReceiverGroup.MasterClient;
            if (LogSendMessages) {
                LastSendMessage = NetworkMessageUtility.ToString(ownNetworkSendMessageUtility.SyncEntities);
            }
            data = messageSerializer.Serialize(ownNetworkSendMessageUtility.SyncEntities);

        }
        //Debug.Log("NetworkSendSystem:\n" + LastSendMessage);
        networkManager.SendMessage(NetworkEvents.DataSync, data, true, networkEventOptions);

        ownNetworkSendMessageUtility.Reset();
        AllNetworkSendMessageUtility.Reset();
    }

    internal void SetNetworkManager(INetworkManager networkManager) {
        this.networkManager = networkManager;
    }
}
