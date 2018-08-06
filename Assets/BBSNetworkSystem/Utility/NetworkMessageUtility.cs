using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class NetworkMessageUtility {
  private const int tab1 = 4;
  private const int tab2 = 8;
  private const int tab3 = 12;

  public static string ToString(SyncEntities networkDataContainer) {
    var builder = new StringBuilder()
      .AppendLine("NetworkSyncDataEntityContainers: {");

    foreach (SyncEntity networkSyncDataEntityContainer in networkDataContainer.Entities) {
      builder.AppendLine(string.Format("{0}NetworkSyncEntity: ", new String(' ', tab1)) + "{")
        .AppendLine(string.Format("{0}NetworkId: {1}", new String(' ', tab2), networkSyncDataEntityContainer.Id.NetworkId))
        .AppendLine(string.Format("{0}ActorId: {1}", new String(' ', tab2), networkSyncDataEntityContainer.Id.ActorId))
        .AppendLine(string.Format("{0}", new String(' ', tab1)) + "}")
        .Append(string.Format("{0}AddedComponents: [ ", new String(' ', tab1)));

      if (networkSyncDataEntityContainer.AddedComponents.Any())
        builder.AppendLine();

      foreach (NetworkComponent componentDataContainer in networkSyncDataEntityContainer.AddedComponents) {
        builder.AppendLine(string.Format("{0}componentDataContainer: ", new String(' ', tab2)) + "{")
          .AppendLine(string.Format("{0}ComponentTypeId: {1}", new String(' ', tab3), componentDataContainer.TypeId))
          .AppendLine(string.Format("{0}MemberData: [ {1} ]", new String(' ', tab3), string.Join(", ", componentDataContainer.Fields.Select(x => x.Value))))
          .AppendLine(string.Format("{0}", new String(' ', tab2)));
      }

      if (networkSyncDataEntityContainer.AddedComponents.Any())
        builder.Append(new String(' ', tab1));

      builder.AppendLine("]")
        .AppendLine(string.Format("{0}RemovedComponents: [ {1} ]", new String(' ', tab1), string.Join(", ", networkSyncDataEntityContainer.RemovedComponents)))
        .Append(string.Format("{0}ComponentData: [ ", new String(' ', tab1)));

      if (networkSyncDataEntityContainer.Components.Any())
        builder.AppendLine();

      foreach (NetworkComponent componentDataContainer in networkSyncDataEntityContainer.Components) {
        builder.AppendLine(string.Format("{0}ComponentDataContainer: ", new String(' ', tab2)) + "{");
        builder.AppendLine(string.Format("{0}ComponentTypeId: {1}", new String(' ', tab3), componentDataContainer.TypeId));
        builder.AppendLine(string.Format("{0}MemberData: [ {1} ]", new String(' ', tab3), string.Join(", ", componentDataContainer.Fields.Select(x => x.Value))));
        builder.AppendLine(string.Format("{0}", new String(' ', tab2)) + "}");
      }


      if (networkSyncDataEntityContainer.Components.Any())
        builder.Append(new String(' ', tab1));

      builder.AppendLine("]");
    }

    builder.AppendLine("}")
      .AppendLine()
      .AppendLine("AddedNetworkSyncEntities: {");

    foreach (NetworkEntity networkEntityData in networkDataContainer.Added) {
      builder.AppendLine(string.Format("{0}NetworkEntityData: ", new String(' ', tab1)) + "{")
        .AppendLine(string.Format("{0}NetworkSyncEntity: ", new String(' ', tab2)) + "{")
        .AppendLine(string.Format("{0}NetworkId: {1}", new String(' ', tab3), networkEntityData.Id.NetworkId))
        .AppendLine(string.Format("{0}ActorId: {1}", new String(' ', tab3), networkEntityData.Id.ActorId))
        .AppendLine(string.Format("{0}", new String(' ', tab2)) + "}")
        .Append(string.Format("{0}ComponentData: [ ", new String(' ', tab1)));

      if (networkEntityData.Components.Any())
        builder.AppendLine();

      foreach (NetworkComponent componentDataContainer in networkEntityData.Components) {
        builder.AppendLine(string.Format("{0}ComponentDataContainer: ", new String(' ', tab2)) + "{")
          .AppendLine(string.Format("{0}ComponentTypeId: {1}", new String(' ', tab3), componentDataContainer.TypeId))
          .AppendLine(string.Format("{0}MemberData: [ {1} ]", new String(' ', tab3), string.Join(", ", componentDataContainer.Fields.Select(x => x.Value))))
          .AppendLine(string.Format("{0}", new String(' ', tab2)) + "}");
      }

      if (networkEntityData.Components.Any())
        builder.Append(new String(' ', tab1));

      builder.AppendLine("]")
        .AppendLine(new String(' ', tab1) + "}");
    }

    builder.AppendLine("}")
      .AppendLine()
      .AppendLine("AddedNetworkSyncEntities: {");

    foreach (EntityId networkSyncEntity in networkDataContainer.Removed) {
      builder.AppendLine(string.Format("{0}NetworkSyncEntity: ", new String(' ', tab1)) + "{")
        .AppendLine(string.Format("{0}NetworkId: {1}", new String(' ', tab2), networkSyncEntity.NetworkId))
        .AppendLine(string.Format("{0}ActorId: {1}", new String(' ', tab2), networkSyncEntity.ActorId))
        .AppendLine(string.Format("{0}", new String(' ', tab1)) + "}");
    }

    builder.AppendLine("}");

    return builder.ToString();
  }
}