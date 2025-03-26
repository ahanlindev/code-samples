using System;

[Serializable] 
public record SaveDataV1
{
    /// <summary>
    /// Do not edit. Acts as an identifier for this data type so we can track future save types if they exist.
    /// </summary>
    [UnityEngine.HideInInspector] public int version = 1;

    /// <summary> How much gold does the player have? </summary>
    public int goldCount;

    /// <summary> Has this player seen the FTUE tutorial? </summary>
    public bool seenTutorial;

    /// <summary> Names of equipped cosmetics. </summary>
    public string[] equippedCosmeticIds = {};

    /// <summary> In-progress and completed challenges. </summary>
    public ChallengeProgressSaveData[] inProgressChallenges = {};
}

[Serializable] 
public class ChallengeProgressSaveData
{
    /// <summary> Unique ID of the challenge. </summary>
    public string id;
    
    /// <summary> How much progress has the player made towards this challenge? </summary>
    public int progress;
} 