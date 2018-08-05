using Unity.Entities;
using UnityEngine;

public class LocalPositionUpdateSystem : ComponentSystem {
  struct Data {
    public readonly int Length;
    public ComponentDataArray<Velocity> velocities;
    public ComponentDataArray<Position> positions;
    public ComponentDataArray<Heading> headings;
    public ComponentArray<CharacterController> characterControllers;
  }
  [Inject] Data data;

  protected override void OnUpdate() {
    for (int i = 0; i < data.Length; i++) {
      var characterController = data.characterControllers[i];
      var transform = characterController.transform;
      var velocity = data.velocities[i].Value;
      var position = data.positions[i].Value;
      var heading = data.headings[i].Value;
      if (heading != Vector3.zero)
        transform.rotation = Quaternion.LookRotation(heading);
      position += velocity;
      characterController.Move(velocity);
      data.positions[i] = new Position { Value = characterController.transform.position };
    }
  }
}