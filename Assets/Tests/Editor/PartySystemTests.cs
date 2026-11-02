using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PartySystemTests
{
    private readonly List<Object> createdObjects = new List<Object>();
    private readonly List<InputDevice> createdDevices = new List<InputDevice>();

    [SetUp]
    public void SetUp()
    {
        PartyManager.ResetAll();
        LocalCoop.ResetAll();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (Object createdObject in createdObjects)
        {
            Object.DestroyImmediate(createdObject);
        }
        createdObjects.Clear();

        foreach (InputDevice device in createdDevices)
        {
            if (device.added)
            {
                InputSystem.RemoveDevice(device);
            }
        }
        createdDevices.Clear();

        PartyManager.ResetAll();
        LocalCoop.ResetAll();
    }

    [Test]
    public void Join_AddsUpToThreeMembers_AndLeaveRemovesThem()
    {
        CharacterProfile paula = CreateCharacter("Paula");
        CharacterProfile jeff = CreateCharacter("Jeff");
        CharacterProfile poo = CreateCharacter("Poo");
        CharacterProfile pokey = CreateCharacter("Pokey");

        int changes = 0;
        PartyManager.OnPartyChanged countChanges = (a_character, a_hasJoined) => changes++;
        PartyManager.onPartyChanged += countChanges;
        try
        {
            Assert.IsTrue(PartyManager.Join(paula, false));
            Assert.IsFalse(PartyManager.Join(paula, false), "Can't join twice.");
            Assert.IsTrue(PartyManager.Join(jeff, false));
            Assert.IsTrue(PartyManager.Join(poo, false));
            Assert.IsFalse(PartyManager.Join(pokey, false), "The party is full.");

            CollectionAssert.AreEqual(new[] { paula, jeff, poo }, PartyManager.Members);

            Assert.IsTrue(PartyManager.Leave(jeff, false));
            Assert.IsFalse(PartyManager.IsInParty(jeff));
            Assert.IsFalse(PartyManager.Leave(jeff, false));
            Assert.AreEqual(4, changes);
        }
        finally
        {
            PartyManager.onPartyChanged -= countChanges;
        }
    }

    [Test]
    public void CommandsAndConditions_UseTheParty()
    {
        CharacterProfile paula = CreateCharacter("Paula");
        DialogueCondition inParty = new DialogueCondition { type = DialogueConditionType.InParty, character = paula };
        DialogueCondition notInParty = new DialogueCondition { type = DialogueConditionType.InParty, character = paula, not = true };

        Assert.IsFalse(inParty.IsMet());
        Assert.IsTrue(notInParty.IsMet());

        DialogueCommand.ExecuteAll(new List<DialogueCommand> { new DialogueCommand { type = DialogueCommandType.JoinParty, character = paula, silent = true } });
        Assert.IsTrue(inParty.IsMet());
        Assert.IsFalse(notInParty.IsMet());

        DialogueCommand.ExecuteAll(new List<DialogueCommand> { new DialogueCommand { type = DialogueCommandType.LeaveParty, character = paula, silent = true } });
        Assert.IsFalse(inParty.IsMet());
    }

    [Test]
    public void TrailPoint_WalksBackAlongThePath()
    {
        // Newest first: the leader walked from (2,2) to (0,2), down to (0,0).
        List<Vector2> trail = new List<Vector2> { new Vector2(0f, 1f), new Vector2(0f, 2f), new Vector2(2f, 2f) };

        Assert.AreEqual(Vector2.zero, PartyManager.GetTrailPoint(Vector2.zero, trail, 0f));
        Assert.AreEqual(new Vector2(0f, 0.5f), PartyManager.GetTrailPoint(Vector2.zero, trail, 0.5f));
        Assert.AreEqual(new Vector2(0f, 1.5f), PartyManager.GetTrailPoint(Vector2.zero, trail, 1.5f));
        Assert.AreEqual(new Vector2(1f, 2f), PartyManager.GetTrailPoint(Vector2.zero, trail, 3f));
        Assert.AreEqual(new Vector2(2f, 2f), PartyManager.GetTrailPoint(Vector2.zero, trail, 10f), "Stops at the end of the path.");
    }

    [Test]
    public void LimitVelocity_StopsAtTheEdge_ButAllowsComingBack()
    {
        Vector2 min = new Vector2(-1f, -1f);
        Vector2 max = new Vector2(1f, 1f);

        Assert.AreEqual(new Vector2(2f, 0f), PartyManager.LimitVelocity(Vector2.zero, new Vector2(2f, 0f), 0.1f, min, max));
        Assert.AreEqual(1f, PartyManager.LimitVelocity(new Vector2(0.9f, 0f), new Vector2(2f, 0f), 0.1f, min, max).x, 0.0001f, "Slows down to stop at the edge.");
        Assert.AreEqual(Vector2.zero, PartyManager.LimitVelocity(new Vector2(0f, -1.5f), new Vector2(0f, -2f), 0.1f, min, max), "Outside: can't go further.");
        Assert.AreEqual(new Vector2(0f, 2f), PartyManager.LimitVelocity(new Vector2(0f, -1.5f), new Vector2(0f, 2f), 0.1f, min, max), "Outside: can come back.");
    }

    [Test]
    public void ToEightDirections_WorksLikeADpad()
    {
        Assert.AreEqual(Vector2.zero, ControllerInput.ToEightDirections(new Vector2(0.2f, 0.1f)), "Dead zone.");
        Assert.AreEqual(Vector2.right, ControllerInput.ToEightDirections(new Vector2(1f, 0.3f)));
        Assert.AreEqual(Vector2.down, ControllerInput.ToEightDirections(new Vector2(-0.3f, -1f)));
        Assert.AreEqual(new Vector2(1f, 1f).normalized, ControllerInput.ToEightDirections(new Vector2(0.6f, 0.8f)));
    }

    [Test]
    public void ControllerInput_ReadsEveryGamepadOnItsOwn()
    {
        Gamepad first = AddGamepad();
        Gamepad second = AddGamepad();

        InputState.Change(first, new GamepadState { leftStick = new Vector2(1f, 0.1f) });
        InputState.Change(second, new GamepadState().WithButton(GamepadButton.DpadUp));

        Assert.AreEqual(Vector2.right, ControllerInput.Read(first).move);
        Assert.AreEqual(Vector2.up, ControllerInput.Read(second).move);
        Assert.IsTrue(ControllerInput.GetControllers().Contains(first));
    }

    [Test]
    public void LocalCoop_GivesEveryControllerItsOwnPlayer()
    {
        Gamepad leaderPad = AddGamepad();
        Gamepad secondPad = AddGamepad();
        Gamepad thirdPad = AddGamepad();
        CharacterProfile paula = CreateCharacter("Paula");
        CharacterProfile jeff = CreateCharacter("Jeff");

        LocalCoop.SetLeaderDevice(leaderPad);
        Assert.IsNull(LocalCoop.Join(leaderPad, paula), "Player 1's controller plays the leader.");

        CoopPlayer player2 = LocalCoop.Join(secondPad, paula);
        Assert.AreEqual(2, player2.Number);
        Assert.IsNull(LocalCoop.Join(thirdPad, paula), "Somebody already plays Paula.");

        CoopPlayer player3 = LocalCoop.Join(thirdPad, jeff);
        Assert.AreEqual(3, player3.Number);

        LocalCoop.Leave(player2);
        Assert.IsNull(LocalCoop.GetPlayer(secondPad));
        Assert.AreEqual(2, LocalCoop.Join(secondPad, paula).Number, "Numbers are reused.");

        // Unplugging a controller drops its player.
        LocalCoop.ListenForDevices();
        InputSystem.RemoveDevice(thirdPad);
        Assert.IsNull(LocalCoop.GetPlayer(jeff));
        InputSystem.RemoveDevice(leaderPad);
        Assert.IsNull(LocalCoop.LeaderDevice);
    }

    [Test]
    public void LeavingTheParty_DropsWhoeverPlayedThem()
    {
        CharacterProfile paula = CreateCharacter("Paula");
        PartyManager.Join(paula, false);
        LocalCoop.Join(AddGamepad(), paula);

        PartyManager.Leave(paula, false);
        Assert.IsFalse(LocalCoop.IsActive);
    }

    [Test]
    public void Follower_GetsABodyWhilePlayed_AndWalksBackIntoLine()
    {
        BoxCollider2D leaderShape = Track(new GameObject("Leader")).AddComponent<BoxCollider2D>();
        leaderShape.size = new Vector2(0.4f, 0.2f);

        Follower follower = Track(new GameObject("Paula")).AddComponent<Follower>();
        // Edit mode doesn't call Awake.
        typeof(Follower).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(follower, null);
        CoopPlayer player = new CoopPlayer(2, AddGamepad(), CreateCharacter("Paula"));

        follower.SetController(player, leaderShape);
        Rigidbody2D body = follower.GetComponent<Rigidbody2D>();
        Assert.IsTrue(follower.IsControlled);
        Assert.IsNotNull(body);
        Assert.IsTrue(body.simulated);
        Assert.AreEqual(leaderShape.size, ((BoxCollider2D)follower.BodyCollider).size, "Same size as the leader.");

        follower.Warp(new Vector2(3f, 0f));
        follower.SetController(null);
        Assert.IsTrue(follower.IsReturning);
        Assert.IsFalse(body.simulated, "Walks through walls while following.");

        follower.FollowTo(Vector2.zero, 0.1f);
        Assert.Less(follower.transform.position.x, 3f);
        Assert.Greater(follower.transform.position.x, 0f, "Walks back instead of teleporting.");

        for (int i = 0; i < 20; i++)
        {
            follower.FollowTo(Vector2.zero, 0.1f);
        }
        Assert.IsFalse(follower.IsReturning);
        Assert.AreEqual(Vector3.zero, follower.transform.position);
    }

    private T Track<T>(T a_object) where T : Object
    {
        createdObjects.Add(a_object);
        return a_object;
    }

    private CharacterProfile CreateCharacter(string a_name)
    {
        CharacterProfile character = Track(ScriptableObject.CreateInstance<CharacterProfile>());
        character.characterName = a_name;
        return character;
    }

    private Gamepad AddGamepad()
    {
        Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
        createdDevices.Add(gamepad);
        return gamepad;
    }
}
