using UnityEngine;

[CreateAssetMenu(fileName = "Vampire", menuName = "Upgrades/Vampire")]
public class VampireUpgrade : BaseUpgrade
{
    [Tooltip("Probability of the effect activating upon killing an enemy")]
    [Range(0,1)]
    public float activationChancePerLevel = 0.1f;
    
    [Tooltip("How much heath the player will gain when the effect activates")]
    public int amountToHeal = 1;

    public override void OnEnemyKilled(EnemyDamageContext ctx, int upgradeLevel) 
    {
        float leveledActivationChance = activationChancePerLevel * upgradeLevel;
        bool activated = Random.value <= leveledActivationChance;
        if (activated) 
        {
            PlayerManager.Instance.HealthSystem.Heal(amountToHeal);
        }
    }
}