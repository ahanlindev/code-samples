using UnityEngine;
using UnityEngine.Serialization;

public abstract class BaseUpgrade : ScriptableObject
{
    public enum Lifetime
    {
        // This upgrade will remain equipped until the run is over, or it is otherwise explicitly unequipped
        Permanent,

        // This upgrade will remain equipped for the duration of a wave, then will be unequipped
        Wave,

        // This upgrade will be unequipped immediately after being equipped
        Instant,
    }

    [FormerlySerializedAs("sprite")]
    [Tooltip("Sprite of this upgrade on the upgrade screen when not selected.")]
    public Sprite outlineSprite;

    [Tooltip("Sprite of this upgrade on the upgrade screen when selected.")]
    public Sprite spriteSelected;

    [Tooltip("Sprite of this upgrade in contexts other than the upgrade screen.")]
    public Sprite spriteColored;

    [Tooltip("Name of this upgrade to display to the player.")]
    public string displayName;

    [Tooltip("How should this upgrade be equipped to and unequipped from the player?")]
    public Lifetime lifetime = Lifetime.Permanent;

    [TextArea]
    [Tooltip("Description of this upgrade to display on the upgrade menu when they do not have any copies equipped.")]
    public string firstDescription;

    [Tooltip("Description of this upgrade to display on the upgrade menu when they have a copy of it equipped.")]
    [TextArea] public string secondDescription;

    [Tooltip("How many copies of this upgrade can the player equip before maxing it out?")]
    public int maxUpgradeLevel = 1;

    /// <summary> Called when the player gains one or more levels of this upgrade. </summary>
    /// <param name="levelsGained"> How many levels were gained at once? (Will usually be 1.) </param>
    /// <param name="newUpgradeLevel"> Level of the upgrade after these levels are gained. </param>
    public virtual void OnGainedLevels(int levelsGained, int newUpgradeLevel)
    {
        Debug.Log($"Upgrade '{displayName}' gained {levelsGained} levels. New level: {newUpgradeLevel}");
    }

    /// <summary> Called when the player loses one or more levels of this upgrade. </summary>
    /// <param name="levelsLost"> How many levels were gained at once (will usually be 1) </param>
    /// <param name="newUpgradeLevel"> Level of the upgrade after these levels are lost. </param>
    public virtual void OnLostLevels(int levelsLost, int newUpgradeLevel)
    {
        Debug.Log($"Upgrade '{displayName}' lost {levelsLost} levels. New level: {newUpgradeLevel}");
    }

    /// <summary> Called when a wave begins. </summary>
    /// <param name="wave"> Index of the wave that began. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnStartedWave(int wave, int upgradeLevel) { }

    /// <summary> Called when the player finishes a wave. </summary>
    /// <param name="wave">Index of the wave that was finished. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnFinishedWave(int wave, int upgradeLevel) { }

    /// <summary> Called when the state of the game is changed. </summary>
    /// <param name="newState"> State that the game is entering. </param>
    /// <param name="oldState"> State that the game is exiting. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnGameStateChange(GameManager.GAME_STATE newState, GameManager.GAME_STATE oldState, int upgradeLevel) { }

    /// <summary> Called once per frame during Unity's Update loop. </summary>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnUpdate(int upgradeLevel) { }

    /// <summary> Called when the player begins a drag. </summary>
    /// <param name="context"> Information about the current drag. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnBeginDrag(DragContext context, int upgradeLevel) { }

    /// <summary> Called once per frame while the player is actively performing a drag. </summary>
    /// <param name="context"> Information about the current drag. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnContinueDrag(DragContext context, int upgradeLevel) { }

    /// <summary> Called when the player successfully completes a drag. </summary>
    /// <param name="context"> Information about the current drag. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnCompleteDrag(DragContext context, int upgradeLevel) { }

    /// <summary> Called when a draggable object enters the area of the player's drag zone. </summary>
    /// <param name="obj"> The object that has entered the drag zone. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnObjectEnteredDragZone(IDraggableObject obj, int upgradeLevel) { }

    /// <summary> Called when a draggable object exits the area of the player's drag zone. </summary>
    /// <param name="obj"> The object that has exited the drag zone. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnObjectExitedDragZone(IDraggableObject obj, int upgradeLevel) { }

    /// <summary> Called when the player takes damage. </summary>
    /// <param name="context"> Information about the damage the player took. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnPlayerDamaged(PlayerDamageContext context, int upgradeLevel) { }

    /// <summary> Called when the player prevents damage. </summary>
    /// <param name="context"> Information about the damage the player prevented. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnPlayerPreventedDamage(PlayerDamageContext context, int upgradeLevel) { }

    /// <summary> Called when the player gains health. </summary>
    /// <param name="amount"> Amount of health gained. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnPlayerHealed(int amount, int upgradeLevel) { }

    /// <summary> Called when the player deals damage to an enemy via any source. </summary>
    /// <param name="context"> Information about the damage dealt to the enemy. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnEnemyDamaged(EnemyDamageContext context, int upgradeLevel) { }

    /// <summary> Called when the player deals lethal damage to an enemy via any source. </summary>
    /// <param name="context"> Information about the damage that killed the enemy. </param>
    /// <param name="upgradeLevel"> Level of this upgrade. </param>
    public virtual void OnEnemyKilled(EnemyDamageContext context, int upgradeLevel) { }

    /// <summary> Called on application quit. Used for cleanup if needed. </summary>
    public virtual void OnApplicationQuit() { }
}