using System;
/// <summary>
/// This attribute signs that the field will be synchronized through the network. 
/// The containing class also requires the <see cref="SyncAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class SyncFieldAttribute : NetSyncBaseAttribute {
  /// <summary>
  /// This attribute signs that the field will be synchronized through the network. 
  /// The containing class also requires the <see cref="SyncAttribute"/>
  /// </summary>
  /// <param name="lerpDamp">use to smoothly interpolate the current value and the latest network value</param>
  //// <param name="reliable">send this field value reliable</param>
  /// <param name="accuracy">will only be applied to float fields</param>    
  public SyncFieldAttribute(float lerpDamp = 1, /*bool reliable = false,*/ int accuracy = 2, float jumpThreshold = 0, bool initOnly = false) 
        : base(lerpDamp, /*reliable,*/ accuracy, jumpThreshold, initOnly) { }
}
