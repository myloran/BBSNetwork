using Unity.Entities;

//public struct NetworkMessageEvent : ISharedComponentData {
//    public byte id;
//}
//public struct NetworkSendEvent : IComponentData {
//    public boolean reliable;
//    public EventCaching eventCaching;
//    public byte group;
//    public byte channel;
//}
//public struct NetworkReceiveEvent : IComponentData {
//    public int senderId;
//}
//public struct NetworkPlayerJoined : IComponentData { public int id; }
//public struct NetworkPlayerLeft : IComponentData { public int id; }
//public struct NetworkRoomJoined : IComponentData { }
//public struct NetworkRoomLeft : IComponentData { }
//public struct NetworkJoinedLobby : IComponentData { }
//public struct NetworkJoinedGame : IComponentData { }
//public struct NetworkConnetedToGameServer : IComponentData { }
//public struct NetworkConnetedToMasterServer : IComponentData { }
//public struct NetworkDisconnetedFromMasterServer : IComponentData { }
//public struct NetworkDisconneted : IComponentData { }
//internal struct NetworkSyncState :ISystemStateComponentData {
//internal int networkId;
//internal int actorId;
//}
//internal struct NetworkMemberState<T> : ISystemStateComponentData { }

public struct NetworktOwner : IComponentData { }

public struct SyncState : ISystemStateComponentData {
    public int networkId;
    public int actorId;
}

public struct NetworkComponentState<T> : ISystemStateComponentData {
    public Entity dataEntity;
}

public struct NetworkComponentData<T> : IComponentData { }

public struct ComponentEntity : IComponentData {
    public int Index;
    public int Version;
}