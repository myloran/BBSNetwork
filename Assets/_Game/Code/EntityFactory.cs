using Unity.Entities;
using UnityEngine;

[SpawnFactory]
public static class EntityFactory {
  [Spawn(1)]
  public static Entity CreateNetPlayer(EntityManager entityManager) {
    //return entityManager.Instantiate(GameSettings.Instance.NetworkPlayerPrefab);
    return GameObject
      .Instantiate(GameSettings.Instance.NetworkPlayerPrefab)
      .GetComponent<GameObjectEntity>().Entity;
  }
}