using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class NetworkMessageUtility {
    private const int tab1 = 4;
    private const int tab2 = 8;
    private const int tab3 = 12;

    public static string ToString(NetworkSyncDataContainer networkDataContainer) {

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("NetworkSyncDataEntityContainers: {");
        foreach (NetworkEntity networkSyncDataEntityContainer in networkDataContainer.Entities) {
            stringBuilder.AppendLine(string.Format("{0}NetworkSyncEntity: ", new String(' ', tab1))+"{");
            stringBuilder.AppendLine(string.Format("{0}NetworkId: {1}", new String(' ', tab2), networkSyncDataEntityContainer.Id.NetworkId));
            stringBuilder.AppendLine(string.Format("{0}ActorId: {1}", new String(' ', tab2), networkSyncDataEntityContainer.Id.ActorId));
            stringBuilder.AppendLine(string.Format("{0}", new String(' ', tab1)) + "}");

            stringBuilder.Append(string.Format("{0}AddedComponents: [ ", new String(' ', tab1)));
            if (networkSyncDataEntityContainer.AddedComponents.Any()) {
                stringBuilder.AppendLine();
            }
            foreach (NetworkComponent componentDataContainer in networkSyncDataEntityContainer.AddedComponents) {
                stringBuilder.AppendLine(string.Format("{0}componentDataContainer: ", new String(' ', tab2)) + "{");
                stringBuilder.AppendLine(string.Format("{0}ComponentTypeId: {1}", new String(' ', tab3), componentDataContainer.TypeId));
                stringBuilder.AppendLine(string.Format("{0}MemberData: [ {1} ]", new String(' ', tab3), string.Join(", ", componentDataContainer.Fields.Select(x => x.Value))));
                stringBuilder.AppendLine(string.Format("{0}", new String(' ', tab2)));
            }
            if (networkSyncDataEntityContainer.AddedComponents.Any()) {
                stringBuilder.Append(new String(' ', tab1));
            }
            stringBuilder.AppendLine("]");

            stringBuilder.AppendLine(string.Format("{0}RemovedComponents: [ {1} ]", new String(' ', tab1), string.Join(", ", networkSyncDataEntityContainer.RemovedComponents)));

            stringBuilder.Append(string.Format("{0}ComponentData: [ ", new String(' ', tab1)));
            if (networkSyncDataEntityContainer.Components.Any()) {
                stringBuilder.AppendLine();
            }
            foreach (NetworkComponent componentDataContainer in networkSyncDataEntityContainer.Components) {
                stringBuilder.AppendLine(string.Format("{0}ComponentDataContainer: ", new String(' ', tab2)) + "{");
                stringBuilder.AppendLine(string.Format("{0}ComponentTypeId: {1}", new String(' ', tab3), componentDataContainer.TypeId));
                stringBuilder.AppendLine(string.Format("{0}MemberData: [ {1} ]", new String(' ', tab3), string.Join(", ", componentDataContainer.Fields.Select(x=>x.Value))));
                stringBuilder.AppendLine(string.Format("{0}", new String(' ', tab2))+"}");
            }
            if (networkSyncDataEntityContainer.Components.Any()) {
                stringBuilder.Append(new String(' ', tab1));
            }
            stringBuilder.AppendLine("]");
        }
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("AddedNetworkSyncEntities: {");
        foreach (NetworkEntityData networkEntityData in networkDataContainer.AddedEntities) {
            stringBuilder.AppendLine(string.Format("{0}NetworkEntityData: ", new String(' ', tab1)) + "{");
            stringBuilder.AppendLine(string.Format("{0}NetworkSyncEntity: ", new String(' ', tab2)) + "{");
            stringBuilder.AppendLine(string.Format("{0}NetworkId: {1}", new String(' ', tab3), networkEntityData.Id.NetworkId));
            stringBuilder.AppendLine(string.Format("{0}ActorId: {1}", new String(' ', tab3), networkEntityData.Id.ActorId));
            stringBuilder.AppendLine(string.Format("{0}", new String(' ', tab2))+ "}");

            stringBuilder.Append(string.Format("{0}ComponentData: [ ", new String(' ', tab1)));
            if (networkEntityData.Components.Any()) {
                stringBuilder.AppendLine();
            }
            foreach (NetworkComponent componentDataContainer in networkEntityData.Components) {
                stringBuilder.AppendLine(string.Format("{0}ComponentDataContainer: ", new String(' ', tab2)) + "{");
                stringBuilder.AppendLine(string.Format("{0}ComponentTypeId: {1}", new String(' ', tab3), componentDataContainer.TypeId));
                stringBuilder.AppendLine(string.Format("{0}MemberData: [ {1} ]", new String(' ', tab3), string.Join(", ", componentDataContainer.Fields.Select(x => x.Value))));
                stringBuilder.AppendLine(string.Format("{0}", new String(' ', tab2)) + "}");
            }
            if (networkEntityData.Components.Any()) {
                stringBuilder.Append(new String(' ', tab1));
            }
            stringBuilder.AppendLine("]");
            stringBuilder.AppendLine(new String(' ', tab1) + "}");
        }
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("AddedNetworkSyncEntities: {");
        foreach (EntityId networkSyncEntity in networkDataContainer.RemovedEntities) {
            stringBuilder.AppendLine(string.Format("{0}NetworkSyncEntity: ", new String(' ', tab1)) + "{");
            stringBuilder.AppendLine(string.Format("{0}NetworkId: {1}", new String(' ', tab2), networkSyncEntity.NetworkId));
            stringBuilder.AppendLine(string.Format("{0}ActorId: {1}", new String(' ', tab2), networkSyncEntity.ActorId));
            stringBuilder.AppendLine(string.Format("{0}", new String(' ', tab1)) + "}");
        }
        stringBuilder.AppendLine("}");

        return stringBuilder.ToString();
    }
}

