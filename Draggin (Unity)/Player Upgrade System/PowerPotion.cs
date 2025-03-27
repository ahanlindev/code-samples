using UnityEngine;

/// <summary>
/// A single-wave temporary upgrade that applies extra damage to enemies when the player attacks them
/// </summary>
[CreateAssetMenu(fileName = "PowerPotion", menuName = "Upgrades/Potions/Power", order = 0)]
public class PowerPotion : BaseUpgrade
{
    [Tooltip("Amount of damage to deal to each enemy")]
    public int bonusDamage = 3;

    public override void OnCompleteDrag(DragContext context, int upgradeLevel)
    {
        base.OnCompleteDrag(context, upgradeLevel);
        foreach (BasicEnemy enemy in context.ContainedEnemies)
        {
            enemy.QueueDamageThisFrame(bonusDamage, displayName);
        }
    }
}
