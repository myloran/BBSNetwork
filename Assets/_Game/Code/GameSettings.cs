using UnityEngine;

[CreateAssetMenu]
public class GameSettings : ScriptableObject {
  public GameObject NetworkPlayerPrefab;
  public float MouseSmoothness;
  public float WalkSpeed;
  public float HorizontalLookSpeed;
  public float JumpPower;
  public float GravityScale;

  static GameSettings instance;
  public static GameSettings Instance {
    get {
      if (instance == null)
        instance = FindObjectOfType<GameSettings>();
      if (instance == null)
        instance = Resources.Load<GameSettings>("Game Settings");
      if (instance == null)
        instance = CreateInstance<GameSettings>();

      return instance;
    }
  }
}