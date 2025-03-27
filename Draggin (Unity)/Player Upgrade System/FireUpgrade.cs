using UnityEngine;

/// <summary>
/// A permanent upgrade that applies damage over time to enemies currently inside the player's drag box.
/// </summary>
[CreateAssetMenu(fileName = "Fire", menuName = "Upgrades/Fire", order = 0)]
public class FireUpgrade : BaseUpgrade
{
    [Tooltip("Vfx to apply to the drag zone to signal that this upgrade is equipped")]
    public DragZoneVfx vfxParticlePrefab;

    [Tooltip("Time in seconds between fire damage ticks")]
    public float tickRate = 0.3f;
    
    [Tooltip("Base damage that fire will deal at level 1")]
    public int startDamage = 1;
    
    [Tooltip("Fire damage to add when subsequent levels are gained")]
    public int extraDamagePerLevel = 1;

    private DragZoneVfx _vfxInstance;
    private float _timer;

    public override void OnGainedLevels(int levelsGained, int newUpgradeLevel)
    {
        bool justAddedToLoadout = newUpgradeLevel - levelsGained == 0;
        if (justAddedToLoadout)
        {
            _vfxInstance = PlayerManager.Instance.DragZone.InstantiateVfx(vfxParticlePrefab);
        }
    }

    public override void OnLostLevels(int levelsLost, int newUpgradeLevel)
    {
        bool justRemovedFromLoadout = newUpgradeLevel == 0;
        if (justRemovedFromLoadout)
        {
            PlayerManager.Instance.DragZone.DestroyVfx(_vfxInstance);
        }
    }

    public override void OnBeginDrag(DragContext context, int upgradeLevel)
    {
        _timer = 0;
    }

    public override void OnContinueDrag(DragContext context, int upgradeLevel)
    {
        _timer += Time.deltaTime;
        if (_timer < tickRate) return;

        _timer = 0;
        int actualDamage = startDamage + extraDamagePerLevel * (upgradeLevel - 1);

        foreach (BasicEnemy enemy in context.ContainedEnemies)
        {
            enemy.QueueDamageThisFrame(dmg: actualDamage, source: displayName);
        }
    }
}
