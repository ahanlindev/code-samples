using UnityEngine;

/// <summary> A permanent upgrade that creates a damaging area of effect when the player is hurt. </summary>
[CreateAssetMenu(fileName = "Thorn", menuName = "Upgrades/Thorn", order = 6)]
public class ThornUpgrade : BaseUpgrade
{
    [Tooltip("Thorn Area of Effect prefab to instantiate on taking damage")]
    public ThornAOE thornAOE;

    [Tooltip("Amount of damage the thorn effect should deal")]
    public int damage = 6;

    [Tooltip("The scale multiplier of the thorn effect at level 1")]
    public float baseScale = 1;

    [Tooltip("Additional scale multiplier of the thorn effect to add for subsequent levels")]
    public float scalePerLevel = 1;

    public override void OnPlayerDamaged(PlayerDamageContext context, int upgradeLevel)
    {
        ThornAOE thornWave = Instantiate(thornAOE, new Vector3(context.Position.x, context.Position.y, 0), Quaternion.identity);
        float scale = baseScale + scalePerLevel * (upgradeLevel - 1);
        thornWave.Init(scale, damage);
    }
}
