using UnityEngine;

[CreateAssetMenu (fileName = "Character", menuName = "Character/New Character", order = 1)]
public class CharacterProfile : ScriptableObject
{
    [Header("Name of character")]
    public string characterName;
    [Header("Animator controller")]
    public RuntimeAnimatorController animatorController;
    [Header("Current Level")]
    public int level;
    public int xp;
    public int hp;
    public int pp;
}
