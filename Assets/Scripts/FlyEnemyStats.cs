using UnityEngine;

// A ScriptableObject: a data asset. Create assets with Create > Creatures > Crawler Stats.
[CreateAssetMenu(fileName = "NewFlyStats", menuName = "Creatures Stats")]
public class FlyEnemyStats : ScriptableObject
{
    public string creatureName;
    public float walkSpeed = 1.5f;
    public Color tint = Color.white;
}
