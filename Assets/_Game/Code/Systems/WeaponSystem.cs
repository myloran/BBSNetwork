using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class WeaponSystem : ComponentSystem {
  struct Data {
    public readonly int Length;
    public ComponentDataArray<WeaponState> weaponState;
    public ComponentDataArray<WeaponAction> weaponAction;
    public ComponentArray<WeaponComponent> weaponComponent;
    public EntityArray entities;
  }
  [Inject] Data data;

  protected override void OnUpdate() {
    for (int i = 0; i < data.Length; i++) {
      var weapon = data.weaponComponent[i];
      var state = data.weaponState[i];
      var action = data.weaponAction[i];
      var entity = data.entities[i];
      var renderer = weapon.lineRenderer;
      var transform = weapon.lineRenderer.transform;

      renderer.SetPosition(0, transform.position);
      state.reloadTimer = math.max(0, state.reloadTimer - Time.deltaTime);
      state.fireTimer = math.max(0, state.fireTimer - Time.deltaTime);
      state.effectTimer = math.max(0, state.effectTimer - Time.deltaTime);

      if (renderer.enabled && state.effectTimer == 0)
        renderer.enabled = false;

      if (state.reloading && state.reloadTimer == 0) {
        state.magazine = weapon.magazinSize;
        state.reloading = false;
      }

      if (action.reload
          && state.magazine < weapon.magazinSize
          && !state.reloading) {
        state.reloadTimer = weapon.reloadTime;
        state.reloading = true;
      }

      if (action.fire 
          && state.magazine > 0 
          && state.fireTimer == 0 
          && !state.reloading) {
        renderer.enabled = true;
        state.fireTimer = weapon.fireRate;
        state.magazine--;

        if (Physics.SphereCast(
            action.shootOrigin, 
            0.5f, 
            action.shootDir, 
            out RaycastHit hit, 
            weapon.range)) {
          renderer.SetPosition(1, hit.point);
          PostUpdateCommands.CreateEntity();
          var obj = hit.collider.GetComponent<GameObjectEntity>();
          if (obj) {
            var damage = new DamageInfo {
              source = entity,
              receiver = obj.Entity,
              damage = weapon.damage
            };
            PostUpdateCommands.AddComponent(damage);
          }
        } else {
          renderer.SetPosition(1, transform.position + transform.forward * weapon.range);
        }
      }
      data.weaponState[i] = state;
    }
  }
}