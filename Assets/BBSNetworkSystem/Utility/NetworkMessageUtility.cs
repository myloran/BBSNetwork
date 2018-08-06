using System;
using System.Linq;
using System.Text;

static class NetworkMessageUtility {
  private const int tab1 = 4;
  private const int tab2 = 8;
  private const int tab3 = 12;

  public static string ToString(SyncEntities entities) {
    var builder = new StringBuilder()
      .AppendLine("NetworkSyncDataEntityContainers: {");

    foreach (var entity in entities.Entities) {
      builder.AppendLine(string.Format("{0}NetworkSyncEntity: ", new String(' ', tab1)) + "{")
        .AppendLine(string.Format("{0}NetworkId: {1}", new String(' ', tab2), entity.Id.NetworkId))
        .AppendLine(string.Format("{0}ActorId: {1}", new String(' ', tab2), entity.Id.ActorId))
        .AppendLine(string.Format("{0}", new String(' ', tab1)) + "}")
        .Append(string.Format("{0}AddedComponents: [ ", new String(' ', tab1)));

      if (entity.AddedComponents.Any())
        builder.AppendLine();

      foreach (var component in entity.AddedComponents) {
        builder.AppendLine(string.Format("{0}componentDataContainer: ", new String(' ', tab2)) + "{")
          .AppendLine(string.Format("{0}ComponentTypeId: {1}", new String(' ', tab3), component.TypeId))
          .AppendLine(string.Format("{0}MemberData: [ {1} ]", new String(' ', tab3), string.Join(", ", component.Fields.Select(x => x.Value))))
          .AppendLine(string.Format("{0}", new String(' ', tab2)));
      }

      if (entity.AddedComponents.Any())
        builder.Append(new String(' ', tab1));

      builder.AppendLine("]")
        .AppendLine(string.Format("{0}RemovedComponents: [ {1} ]", new String(' ', tab1), string.Join(", ", entity.RemovedComponents)))
        .Append(string.Format("{0}ComponentData: [ ", new String(' ', tab1)));

      if (entity.Components.Any())
        builder.AppendLine();

      foreach (var component in entity.Components) {
        builder.AppendLine(string.Format("{0}ComponentDataContainer: ", new String(' ', tab2)) + "{");
        builder.AppendLine(string.Format("{0}ComponentTypeId: {1}", new String(' ', tab3), component.TypeId));
        builder.AppendLine(string.Format("{0}MemberData: [ {1} ]", new String(' ', tab3), string.Join(", ", component.Fields.Select(x => x.Value))));
        builder.AppendLine(string.Format("{0}", new String(' ', tab2)) + "}");
      }


      if (entity.Components.Any())
        builder.Append(new String(' ', tab1));

      builder.AppendLine("]");
    }

    builder.AppendLine("}")
      .AppendLine()
      .AppendLine("AddedNetworkSyncEntities: {");

    foreach (var entity in entities.Added) {
      builder.AppendLine(string.Format("{0}NetworkEntityData: ", new String(' ', tab1)) + "{")
        .AppendLine(string.Format("{0}NetworkSyncEntity: ", new String(' ', tab2)) + "{")
        .AppendLine(string.Format("{0}NetworkId: {1}", new String(' ', tab3), entity.Id.NetworkId))
        .AppendLine(string.Format("{0}ActorId: {1}", new String(' ', tab3), entity.Id.ActorId))
        .AppendLine(string.Format("{0}", new String(' ', tab2)) + "}")
        .Append(string.Format("{0}ComponentData: [ ", new String(' ', tab1)));

      if (entity.Components.Any())
        builder.AppendLine();

      foreach (var component in entity.Components) {
        builder.AppendLine(string.Format("{0}ComponentDataContainer: ", new String(' ', tab2)) + "{")
          .AppendLine(string.Format("{0}ComponentTypeId: {1}", new String(' ', tab3), component.TypeId))
          .AppendLine(string.Format("{0}MemberData: [ {1} ]", new String(' ', tab3), string.Join(", ", component.Fields.Select(x => x.Value))))
          .AppendLine(string.Format("{0}", new String(' ', tab2)) + "}");
      }

      if (entity.Components.Any())
        builder.Append(new String(' ', tab1));

      builder.AppendLine("]")
        .AppendLine(new String(' ', tab1) + "}");
    }

    builder.AppendLine("}")
      .AppendLine()
      .AppendLine("AddedNetworkSyncEntities: {");

    foreach (var id in entities.Removed) {
      builder.AppendLine(string.Format("{0}NetworkSyncEntity: ", new String(' ', tab1)) + "{")
        .AppendLine(string.Format("{0}NetworkId: {1}", new String(' ', tab2), id.NetworkId))
        .AppendLine(string.Format("{0}ActorId: {1}", new String(' ', tab2), id.ActorId))
        .AppendLine(string.Format("{0}", new String(' ', tab1)) + "}");
    }

    builder.AppendLine("}");

    return builder.ToString();
  }
}