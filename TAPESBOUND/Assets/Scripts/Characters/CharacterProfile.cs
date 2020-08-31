using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu (fileName = "Character", menuName = "Character/New Character", order = 1)]
public class CharacterProfile : ScriptableObject
{
    [Header("Name of character")]
    public string characterName;
    [Header("Animator controller")]
    public AnimatorController animatorController;
    [Header("Current Level")]
    public int level;
    public int xp;
    public int hp;
    public int pp;
}
