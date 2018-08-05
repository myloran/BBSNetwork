using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerInputSystem : ComponentSystem {
  struct LivingPlayer {
    public readonly int Length;
    [WriteOnly] public ComponentDataArray<PlayerInput> playerInput;
    [ReadOnly] public SubtractiveComponent<DeathComponent> death;
  }
  struct DeadPlayer {
    public readonly int Length;
    [WriteOnly] public ComponentDataArray<PlayerInput> playerInput;
    [ReadOnly] public ComponentDataArray<DeathComponent> death;
  }
  [Inject] private LivingPlayer livingPlayers;
  [Inject] private DeadPlayer deadPlayers;

  protected override void OnUpdate() {
    var input = new PlayerInput {
      move = new float2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")),
      lookRaw = new float2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")),
      jump = Input.GetButtonDown("Jump"),
      fire = Input.GetButton("Fire1"),
      reload = Input.GetButton("Reload"),
    };
    for (int i = 0; i < livingPlayers.Length; i++) {
      input.look = math.lerp(
        livingPlayers.playerInput[i].look, 
        input.lookRaw, 
        Time.deltaTime * GameSettings.Instance.MouseSmoothness);
      livingPlayers.playerInput[i] = input;
    }
    for (int i = 0; i < deadPlayers.Length; i++) {
      deadPlayers.playerInput[i] = new PlayerInput {
        jump = false,
        fire = false,
        reload = false,
        move = new float2(0, 0),
        lookRaw = new float2(0, 0),
        look = new float2(0, 0),
      };
    }
  }
}