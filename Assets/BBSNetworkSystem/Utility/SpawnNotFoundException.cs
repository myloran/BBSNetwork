using System;
using System.Runtime.Serialization;

[Serializable]
internal class SpawnNotFoundException : Exception {
  public SpawnNotFoundException() { }

  public SpawnNotFoundException(
      int id
  ) : base(string.Format("Could not found a Spawn with id: {0}", id)) { }

  public SpawnNotFoundException(
      string message
  ) : base(message) { }

  public SpawnNotFoundException(
      string message, 
      Exception innerException
  ) : base(message, innerException) { }

  protected SpawnNotFoundException(
      SerializationInfo info, 
      StreamingContext context
  ) : base(info, context) { }
}