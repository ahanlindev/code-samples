# Player Upgrade System

A core aspect of Draggin's gameplay is its player upgrades. When the player defeats a wave of enemies, they are brought to an upgrade screen where they are able to select either a persistent upgrade or a temporary boost.

The system needed to satisfy the following constraints:

1. Upgrades must be able to respond to important gameplay events.
2. The same upgrade may be equipped multiple times, up to a maximum limit.
3. The player will have a limited number of permanent upgrade types they can equip during a run, but temporary upgrades should not count towards this limit.
4. When the player has the maximum number of permanent upgrade types equipped, permanent upgrades of new types should not be offered to the player on the upgrade screen.

To satisfy this design, I built a framework that manages currently-equipped player upgrades and provides the team with a quick and flexible interface for building new upgrades.

## Structure

The Player Upgrade System is built upon two main classes: [PlayerUpgradeSystem](./PlayerUpgradeSystem.cs), which acts as a global container and controller for equipped upgrades, and [BaseUpgrade](./BaseUpgrade.cs), which provides the API each upgrade implementation needs to interface with the game world.

### PlayerUpgradeSystem

The `PlayerUpgradeSystem` is a component attached to a `PlayerManager` singleton object in the game's main scene. The `PlayerUpgradeSystem`'s primary responsibilities are to: 

- Track the player's currently equipped upgrades.
- Act as source of truth for which upgrades are currently equipped and which are eligible to be equipped.
- Allow external systems to access information about equipped and available upgrades.
- Allow external systems to add and remove upgrades from the player's loadout.
- Propagate gameplay events to each equipped upgrade.

#### Equipped Upgrade List

This system maintains a list of the player's equipped upgrades, alongside an integer value representing that upgrade's current level. Upon adding an upgrade, if it is already present in the list, the level is incremented instead of adding a new list item.

Assuming the player equipped the "Charge" upgrade once, and the "Fire" upgrade twice during their run, the upgrade list would appear like so: 

![Screenshot of the equipped upgrade list in the inspector, demonstrating upgrade entries with multiple levels](./ReadmeImages/equipped-upgrade-list.jpg)

#### Public API

As the system is exposed by a global singleton `PlayerManager` object, other scripts within the game can access its public API. 

In addition to adding and removing upgrades and retrieving the player's current upgrade loadout, the class offers the ability to retrieve all upgrades in the game that are eligible to be equipped. 

As the player has limited upgrade slots for permanent upgrades, some logic is required to determine which upgrades are eligible to be equipped.

The following logical flow determines whether an upgrade will appear as eligible

```
if this upgrade is the reward of an uncompleted challenge: 
    return false // upgrade is locked

else if this upgrade is a temporary upgrade:
    return true // temporary upgrades are not constrained by loadout slots

else: // this upgrade is a permanent upgrade
    if player has the max level of this upgrade:
        return false

    else if player has any open loadout slots:
        return true

    else:
        if player has at least one level of this upgrade:
            return true
        else:
            return false
```

#### Event Propagation

As the `PlayerUpgradeSystem` is responsible for maintaining all active upgrades, it is also the main source of the information each upgrade script receives.

In the `Start` method of `PlayerUpgradeSystem`, the system subscribes to a multitude of global events from various sources. The listeners added to these events receive the information and call a corresponding `BaseUpgrade` method on each equipped upgrade. 

```c#
// The Start method of PlayerUpgradeSystem.cs at time of writing
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
```

```c#
// One of the propagation methods within PlayerUpgradeSystem.cs. Others are of a similar format
public void PropagateBeginDrag(DragContext context)
{
    allEquippedUpgrades.ForEach(row => row.Upgrade.OnBeginDrag(context, upgradeLevel: row.Level));
}
```

The implementation of each of these `BaseUpgrade` methods is dependent on the upgrade type.

### BaseUpgrade

The `BaseUpgrade` class forms the basis of each individual upgrade type. It inherits from `ScriptableObject`, and consists of numerous fields that define the upgrade's appearance, description, and other information. 

In addition to these fields, the base class contains an empty implementation of each of the event handler methods propagated by the `PlayerUpgradeSystem`, with an extra int parameter encoding the current level of the equipped upgrade.

```c#
// One of the trivial implementations described above in BaseUpgrade.cs
public virtual void OnBeginDrag(DragContext context, int upgradeLevel) { }
```

To implement unique behavior for each upgrade, a child class of `BaseUpgrade` must be created, and any required event handler methods may be overridden. Some key examples of these implementations are included, namely `FireUpgrade.cs`, `ThornUpgrade.cs`, `VampireUpgrade.cs`, and `PowerPotion.cs`. See those files for implementation details.

## Upgrade Creation Workflow

The workflow for creating a new upgrade is intended to be simple, without requiring deep understanding of the Player Upgrade System to implement new upgrades.

To demonstrate this process, let's walk through the process of creating the "Vampire" upgrade, which adds a chance to heal the player upon defeating an enemy.

### Create an Upgrade Script

First, create a basic script to house the implementation. It is important that the script inherits from `BaseUpgrade` and is given the `CreateAssetMenu` attribute.

```c#
using UnityEngine;

[CreateAssetMenu(fileName = "Vampire", menuName = "Upgrades/Vampire")]
public class VampireUpgrade : BaseUpgrade
{
}
```

### Override Event Listeners to Create Functionality

For the upgrade to work as intended, it will need to be notified whenever an enemy has been killed. Luckily, `BaseUpgrade` has the event handler `OnEnemyKilled` which we can override in the new upgrade script. Within this method, we will implement the functionality of the upgrade.

```c#
using UnityEngine;

[CreateAssetMenu(fileName = "Vampire", menuName = "Upgrades/Vampire")]
public class VampireUpgrade : BaseUpgrade
{
    public override void OnEnemyKilled(EnemyDamageContext ctx, int upgradeLevel) 
    {
        float leveledActivationChance = 0.1f * upgradeLevel;
        bool activated = Random.value <= leveledActivationChance;
        if (activated) 
        {
            PlayerManager.Instance.HealthSystem.Heal(1);
        }
    }
}
```

### Serialize Upgrade-specific Information

Because the script inherits from a ScriptableObject, we can add additional serialized fields to increase the configurability of the upgrade. For the purposes of this upgrade, we will serialize the activation chance per level and the amount of health the player will gain.

```c#
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
```

### Create ScriptableObject Instance

Now that the upgrade script is complete, we need to create a corresponding asset. Because we added the `CreateAssetMenu` attribute, we can do so by selecting `Create > Upgrades > Vampire`. Upon doing so we will create this asset:

![Screenshot of the Unity Inspector view for the Vampire upgrade.](ReadmeImages/vampire-inspector.jpg)

The fields in the inspector can then be filled in and updated as needed.

### Add Upgrade to Global List

For the upgrade to appear in the game, it will need to be added to the global list of upgrades utilized by the `PlayerUpgradeSystem`. This is an `UpgradePool` ScriptableObject kept within the project. Once the Vampire asset is added to this list, the upgrade will appear in game and the player will be able to equip and use it. 

![Screenshot of the upgrade pool](ReadmeImages/upgrade-pool.jpg)

## Areas for Improvement

### Automate New Upgrade Creation

As can be seen above, the workflow for creating a new upgrade has a lot of room for improvement. It requires creating a script, creating an asset of that script's type, and then adding that asset to another list within the project.

Automating these steps via an editor window or wizard would dramatically reduce potential points of friction in this process.

### Runtime Efficiency

At this time, when any of the global events that `PlayerUpgradeSystem` is subscribed to occurs, every equipped upgrade will call its implementation of the corresponding handler, regardless of whether a nontrivial overridden implementation exists or not. While this has not had any significant effects on performance at time of writing, it is a noteworthy point of inefficiency.

A possible solution for this would be to replace the virtual methods in `BaseUpgrade` with individual interfaces, each implementing a single event handler. `PlayerUpgradeSystem` could then filter its list based on which interfaces have been implemented. I have not yet explored whether this would have a significant effect on the runtime overhead of the system. 

### ScriptableObject Assets Versus Runtime Instances

The list of equipped upgrades in `PlayerUpgradeSystem` contains references to the upgrade ScriptableObject assets themselves. In general, it is preferable to avoid altering ScriptableObject assets at runtime, as it can have unpredictable effects particularly in the editor between play mode sessions. 

As it became clear that many upgrade scripts we implemented needed mutable private fields to accomplish their functionality, these issues have become more prevalent. 

A minor refactor could be made that would instantiate a copy of the ScriptableObject to occupy a place in the list, rather than referencing the asset itself. This solution is imperfect, but it would be simple to implement and resolve the main issue.
