using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class PlayerShootSystem : ComponentSystem {
  struct Data {
    public readonly int Length;
    //[ReadOnly]
    //[WriteOnly]
    [ReadOnly] public ComponentDataArray<PlayerInput> playerInput;
    [WriteOnly] public ComponentDataArray<WeaponAction> weaponAction;
    //public ComponentArray<> comp;		
    //public EntityArray entities;
  }
  [Inject] Data data;

  protected override void OnUpdate() {
    Ray ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));
    for (int i = 0; i < data.Length; i++) {
      data.weaponAction[i] = new WeaponAction {
        fire = data.playerInput[i].fire,
        reload = data.playerInput[i].reload,
        shootOrigin = ray.origin,
        shootDir = ray.direction,
      };
    }
  }
}