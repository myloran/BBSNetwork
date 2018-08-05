using Unity.Entities;
using UnityEngine;

public class PlayerMovementSystem : ComponentSystem {
  struct PlayerData {
    public readonly int Length;
    public ComponentDataArray<PlayerInput> input;
    public ComponentArray<CharacterController> characterControllers;
    public ComponentDataArray<Velocity> velocities;
    public ComponentDataArray<Position> positions;
    public ComponentDataArray<Heading> headings;
    public EntityArray entity;
  }
  [Inject] PlayerData playerData;

  protected override void OnUpdate() {
    var dt = Time.deltaTime;
    for (int i = 0; i < playerData.Length; i++) {
      var input = playerData.input[i];
      var characterController = playerData.characterControllers[i];
      var transform = characterController.transform;
      var velocity = playerData.velocities[i].Value;
      var heading = playerData.headings[i].Value;
      if (heading == Vector3.zero)
        heading = transform.forward;
      float speed = GameSettings.Instance.WalkSpeed;
      var movement = transform.right * input.move.x
        + transform.forward * input.move.y;
      movement = movement.normalized * speed * dt;
      velocity = new Vector3(movement.x, velocity.y, movement.z);
      var euler = new Vector3(0, input.look.x, 0)
        * GameSettings.Instance.HorizontalLookSpeed
        * dt;
      heading = Quaternion.Euler(euler) * heading;
      velocity.y += Physics.gravity.y
        * GameSettings.Instance.GravityScale
        * dt;
      if (characterController.isGrounded && input.jump)
        velocity.y = GameSettings.Instance.JumpPower;
      playerData.velocities[i] = new Velocity { Value = velocity };
      playerData.headings[i] = new Heading(heading);
    }
  }
}