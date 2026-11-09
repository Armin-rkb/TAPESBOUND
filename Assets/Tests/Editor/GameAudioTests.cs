using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class GameAudioTests
{
    [Test]
    public void EverySoundHasAClip()
    {
        foreach (GameSound sound in Enum.GetValues(typeof(GameSound)))
        {
            AudioClip clip = Resources.Load<AudioClip>("GameAudio/" + sound);
            Assert.IsNotNull(clip, sound.ToString());
            Assert.Greater(clip.length, 0f, sound.ToString());
        }
    }

    [TestCase("Assets/Scenes/MC Bedroom.unity")]
    [TestCase("Assets/Scenes/MC Livingroom.unity")]
    [TestCase("Assets/Scenes/MasterScene.unity")]
    [TestCase("Assets/Scenes/Test Scene Management/Master-Scene.unity")]
    public void HomeScenesHaveMusic(string a_scenePath)
    {
        Assert.That(System.IO.File.ReadAllText(a_scenePath), Does.Contain(HomeThemeGuid()));
    }

    [TestCase("City", GameSound.StepStreet)]
    [TestCase("Underwhere Train", GameSound.StepMetal)]
    [TestCase("MC Bedroom", GameSound.StepWood)]
    public void FootstepsMatchTheScene(string a_scene, GameSound a_expected)
    {
        Assert.AreEqual(a_expected, GameAudio.GetStepSound(a_scene));
    }

    private static string HomeThemeGuid()
    {
        return AssetDatabase.AssetPathToGUID("Assets/Audio/Background/Home Theme.wav");
    }
}
