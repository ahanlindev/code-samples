using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Upgrade Pool", menuName = "Upgrades/UpgradePool", order = -1)]
public class UpgradePool : ScriptableObject
{
    [FormerlySerializedAs("upgrades")]
    public BaseUpgrade[] allUpgrades;
}