using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerUpgradeSystem : MonoBehaviour
{
    [Tooltip("All possible upgrade types")]
    [SerializeField] private UpgradePool upgradePool;
    
    [Tooltip("List of all upgrades that are currently active")]
    [SerializeField] private List<UpgradeWithLevel> allEquippedUpgrades = new();

    [Tooltip("Emitted when an upgrade is added to the player's loadout")]
    public UnityEvent<BaseUpgrade> upgradeAdded;

    [Tooltip("Emitted when an upgrade is removed from the player's loadout")]
    public UnityEvent<BaseUpgrade> upgradeRemoved;

    /// <summary>
    /// Add an upgrade to the player's equipped upgrades.
    /// If they already have a copy of this upgrade, levels it up instead.
    /// </summary>
    /// <param name="upgrade">Upgrade to add.</param>
    public void AddUpgrade(BaseUpgrade upgrade)
    {
        UpgradeWithLevel matchingRow = allEquippedUpgrades.FirstOrDefault(row => row.Upgrade == upgrade);

        if (matchingRow != default)
        {
            // Player has a copy of this upgrade. Level it up.
            matchingRow.Level++;
        }
        else
        {
            // Player does not have this upgrade yet. Add it.
            matchingRow = new UpgradeWithLevel { Upgrade = upgrade, Level = 1 };
            allEquippedUpgrades.Add(matchingRow);
        }

        upgrade.OnGainedLevels(1, matchingRow.Level);
        upgradeAdded.Invoke(upgrade);

        // instant effects get removed immediately after their OnGainedLevels method is run
        if (upgrade.lifetime == BaseUpgrade.Lifetime.Instant)
        {
            RemoveUpgrade(upgrade);
        }
    }

    /// <summary>
    /// Remove an upgrade from the player's equipped upgrades.
    /// If they have multiple copies, removes one level of the upgrade unless removeAllLevels is set to true
    /// </summary>
    /// <param name="upgrade">Upgrade to add.</param>
    /// <param name="removeAllLevels">If set to true, all copies of the upgrade will be removed.</param>
    public void RemoveUpgrade(BaseUpgrade upgrade, bool removeAllLevels = false)
    {
        UpgradeWithLevel matchingRow = allEquippedUpgrades.FirstOrDefault(row => row.Upgrade == upgrade);
        if (matchingRow == null) return;
        int levelsToRemove = (removeAllLevels) ? matchingRow.Level : 1;

        matchingRow.Level -= levelsToRemove;
        upgrade.OnLostLevels(levelsToRemove, matchingRow.Level);
        
        if (matchingRow.Level <= 0)
        {
            allEquippedUpgrades.Remove(matchingRow);
        }
        upgradeRemoved.Invoke(upgrade);
    }

    /// <summary> Retrieve all upgrades currently attached to the player. </summary>
    public List<UpgradeWithLevel> GetAllEquippedUpgrades()
    {
        return allEquippedUpgrades;
    }

    /// <summary> Retrieve upgrades with the provided lifetime currently attached to the player </summary>
    public List<UpgradeWithLevel> GetEquippedUpgradesByLifetime(BaseUpgrade.Lifetime lifetime)
    {
        return allEquippedUpgrades.Where(upgrade => upgrade.Upgrade.lifetime == lifetime).ToList();
    }

    /// <summary> Retrieve all upgrades that it is currently possible for the player to equip. </summary>
    public List<BaseUpgrade> GetAllEquipableUpgrades()
    {
        List<BaseUpgrade> result = new();

        foreach (var lifetime in Enum.GetValues(typeof(BaseUpgrade.Lifetime)).Cast<BaseUpgrade.Lifetime>())
        {
            result.AddRange(GetEquipableUpgradesByLifetime(lifetime));
        }

        return result;
    }

    /// <summary>
    /// Retrieve all upgrades of the provided lifetime that it is currently possible for the player to equip.
    /// </summary>
    public List<BaseUpgrade> GetEquipableUpgradesByLifetime(BaseUpgrade.Lifetime lifetime)
    {
        // Permanent upgrades require extra logic to determine equipability
        if (lifetime == BaseUpgrade.Lifetime.Permanent)
        {
            return GetEquipablePermanentUpgrades();
        }

        // Temporary upgrades can just query upgrade pool
        ChallengeSystem cs = GameManager.Instance.ChallengeSystem;
        return upgradePool.allUpgrades
            .Where(item => item) // Unity Object null check. More efficient than != null
            .Where(item => item.lifetime == lifetime)
            .Where(item => cs.UpgradeIsUnlocked(item))
            .ToList();
    }

    /// <returns>True if the passed-in upgrade is equipped, otherwise false. </returns>
    public bool UpgradeIsEquipped(BaseUpgrade upgrade)
    {
        return allEquippedUpgrades.Any(row => row.Upgrade == upgrade);
    }

    /// <summary> Reset the upgrade system to its initial state. </summary>
    public void ResetSystem()
    {
        List<UpgradeWithLevel> loadout = allEquippedUpgrades;
        for (int i = loadout.Count - 1; i >= 0; i--)
        {
            UpgradeWithLevel upgradeData = loadout[i];
            RemoveUpgrade(upgradeData.Upgrade, true);
        }
    }

    /// <summary>
    /// Retrieve all permanent upgrades in the upgrade pool it is currently possible for the player to equip.
    /// </summary>
    private List<BaseUpgrade> GetEquipablePermanentUpgrades()
    {
        // Get all upgrades the player has the max level of already
        List<BaseUpgrade> maxedOutUpgrades = allEquippedUpgrades
            .Where(row => row.Upgrade.maxUpgradeLevel <= row.Level)
            .Select(row => row.Upgrade)
            .ToList();

        // Check if player inventory is full. If so, only return upgrades they already have
        int slotCount = PlayerManager.Instance.Settings.UpgradeSlotCount;
        List<UpgradeWithLevel> equippedPermanents = GetEquippedUpgradesByLifetime(BaseUpgrade.Lifetime.Permanent);
        if (equippedPermanents.Count >= slotCount)
        {
            // Inventory full. Return upgrades player hasn't maxed out yet
            return equippedPermanents
                .Where(item => !maxedOutUpgrades.Contains(item.Upgrade))
                .Select(item => item.Upgrade)
                .ToList();
        }

        // Return all non-maxed upgrades that are unlocked, whether equipped already or not
        ChallengeSystem cs = GameManager.Instance.ChallengeSystem;
        return upgradePool.allUpgrades
            .Where(item => item) // Unity Object null check. More efficient than != null
            .Where(item => item.lifetime == BaseUpgrade.Lifetime.Permanent)
            .Where(item => cs.UpgradeIsUnlocked(item))
            .Where(item => !maxedOutUpgrades.Contains(item)) // Not maxed out
            .ToList();
    }

    private void OnGameStateChanged(GameManager.GAME_STATE newState, GameManager.GAME_STATE oldState)
    {
        PropagateGameStateChange(newState, oldState);

        // If exiting a wave, remove single-wave temp upgrades
        if (oldState == GameManager.GAME_STATE.BATTLE)
        {
            var upgradesToRemove = allEquippedUpgrades.Where(u=>u.Upgrade.lifetime == BaseUpgrade.Lifetime.Wave).ToList();

            foreach (UpgradeWithLevel item in upgradesToRemove)
            {
                RemoveUpgrade(item.Upgrade, true);
            }
        }
    }

    // MonoBehaviour methods
    private void Start()
    {
        GameManager gm = GameManager.Instance;
        gm.onGameStateChanged.AddListener(OnGameStateChanged); // Has extra logic beyond propagation

        PlayerManager pm = PlayerManager.Instance;

        DragZone dragZone = pm.DragZone;
        dragZone.beginDrag.AddListener(PropagateBeginDrag);
        dragZone.continueDrag.AddListener(PropagateContinueDrag);
        dragZone.completeDrag.AddListener(PropagateCompleteDrag);
        dragZone.objectEnteredDragZone.AddListener(PropagateObjectEnteredDragZone);
        dragZone.objectExitedDragZone.AddListener(PropagateObjectExitedDragZone);
    
        PlayerHealthSystem health = pm.HealthSystem;
        health.damaged.AddListener(PropagatePlayerDamaged);
        health.healed.AddListener(PropagatePlayerHealed);
        health.preventedDamage.AddListener(PropagatePlayerPreventedDamage);
        
        BattleManager bm = BattleManager.Instance;
        bm.enemyDamaged.AddListener(PropagateEnemyDamaged);
        bm.enemyKilled.AddListener(PropagateEnemyKilled);
        bm.startedWave.AddListener(PropagateStartedWave);
        bm.finishedWave.AddListener(PropagateFinishedWave);
    }

    private void Update()
    {
        PropagateUpdate();
    }

#region Event Propogation Methods

    public void PropagateGameStateChange(GameManager.GAME_STATE newState, GameManager.GAME_STATE oldState)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnGameStateChange(newState, oldState, row.Level));
    }

    public void PropagateBeginDrag(DragContext context)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnBeginDrag(context, row.Level));
    }

    public void PropagateContinueDrag(DragContext context)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnContinueDrag(context, row.Level));
    }

    public void PropagateCompleteDrag(DragContext context)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnCompleteDrag(context, row.Level));
    }

    public void PropagateObjectEnteredDragZone(IDraggableObject obj)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnObjectEnteredDragZone(obj, row.Level));
    }

    public void PropagateObjectExitedDragZone(IDraggableObject obj)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnObjectExitedDragZone(obj, row.Level));
    }

    public void PropagatePlayerDamaged(PlayerDamageContext ctx)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnPlayerDamaged(ctx, row.Level));
    }
    
    public void PropagatePlayerHealed(int healAmount)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnPlayerHealed(healAmount, row.Level));
    }

    public void PropagatePlayerPreventedDamage(PlayerDamageContext ctx)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnPlayerPreventedDamage(ctx, row.Level));
    }

    public void PropagateEnemyDamaged(EnemyDamageContext context)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnEnemyDamaged(context, row.Level));
    }

    public void PropagateEnemyKilled(EnemyDamageContext context)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnEnemyKilled(context, row.Level));
    }

    public void PropagateStartedWave(int wave)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnStartedWave(wave, row.Level));
    }
    
    public void PropagateFinishedWave(int wave)
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnFinishedWave(wave, row.Level));
    }

    public void PropagateApplicationQuit()
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnApplicationQuit());
    }

    private void PropagateUpdate()
    {
        allEquippedUpgrades.ForEach(row => row.Upgrade.OnUpdate(row.Level));
    }

    #endregion Upgrade Callbacks
}

[Serializable]
public class UpgradeWithLevel
{
    public BaseUpgrade Upgrade;
    public int Level;
}