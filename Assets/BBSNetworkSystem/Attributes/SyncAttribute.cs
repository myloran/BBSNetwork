using System;
/// <summary>
/// This attribte enables you to synchronize data through the network. 
/// It's required to all structs that will contains  <see cref="FieldSyncAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class SyncAttribute : Attribute { }