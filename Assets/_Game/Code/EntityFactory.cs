using Unity.Entities;
using UnityEngine;

[NetworkEntityFactory]
public static class EntityFactory {
  [NetworkEntityFactoryMethod(1)]
  public static Entity CreateNetPlayer(EntityManager entityManager) {
    //return entityManager.Instantiate(GameSettings.Instance.NetworkPlayerPrefab);
    return GameObject
      .Instantiate(GameSettings.Instance.NetworkPlayerPrefab)
      .GetComponent<GameObjectEntity>().Entity;
  }
}