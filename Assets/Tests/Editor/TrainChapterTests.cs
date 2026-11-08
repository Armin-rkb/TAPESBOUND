using NUnit.Framework;
using UnityEngine;

public class TrainChapterTests
{
    private readonly Vector2 halfView = new Vector2(4f, 3.5f);

    [Test]
    public void ClampToRoom_KeepsARoomWiderThanTheScreenInView()
    {
        Bounds hallway = new Bounds(Vector3.zero, new Vector3(12f, 3f, 0f));

        Assert.AreEqual(0f, RoomCamera.ClampToRoom(Vector2.zero, hallway, halfView).x, 0.001f);
        Assert.AreEqual(2f, RoomCamera.ClampToRoom(new Vector2(5f, 0f), hallway, halfView).x, 0.001f, "Stops at the right wall.");
        Assert.AreEqual(-2f, RoomCamera.ClampToRoom(new Vector2(-5f, 0f), hallway, halfView).x, 0.001f, "Stops at the left wall.");
    }

    [Test]
    public void ClampToRoom_CentersARoomSmallerThanTheScreen()
    {
        Bounds room = new Bounds(new Vector3(20f, 1f, 0f), new Vector3(6.5f, 4.6f, 0f));

        Assert.AreEqual(new Vector2(20f, 1f), RoomCamera.ClampToRoom(new Vector2(22f, -1f), room, halfView));
    }

    [Test]
    public void TouchDpad_DoesNothingInTheMiddle_AndGivesFullDirectionsAroundIt()
    {
        Assert.AreEqual(Vector2.zero, TouchDpad.GetDirection(new Vector2(5f, 0f), 120f, 0.12f));
        Assert.AreEqual(Vector2.right, TouchDpad.GetDirection(new Vector2(40f, 0f), 120f, 0.12f));
        Assert.AreEqual(new Vector2(-1f, 1f).normalized, TouchDpad.GetDirection(new Vector2(-30f, 30f), 120f, 0.12f));
    }

    [Test]
    public void TouchControls_AreSizedForThePhone_AndFitInTheBlackBars()
    {
        // iPhone held upright: lots of room below the picture.
        Assert.AreEqual(3f, TouchControls.GetScale(1170f, 2532f, new Rect(73f, 818f, 1024f, 896f), 390f, 152f), 0.001f);
        // iPad held sideways: the bars are narrow, so the controls shrink.
        Assert.AreEqual(256f / 152f, TouchControls.GetScale(2048f, 1536f, new Rect(256f, 96f, 1536f, 1344f), 390f, 152f), 0.001f);
        // No bars at all: never smaller than one pixel per unit.
        Assert.AreEqual(1f, TouchControls.GetScale(768f, 672f, new Rect(0f, 0f, 768f, 672f), 390f, 152f), 0.001f);
    }

    [Test]
    public void KnockingDoor_CountsKnocksAndStopsAfterTheLast()
    {
        GameObject doorObject = new GameObject("Door");
        try
        {
            KnockingDoor door = doorObject.AddComponent<KnockingDoor>();
            typeof(KnockingDoor).GetField("knocks", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(door, new[] { new KnockingDoor.Knock(), new KnockingDoor.Knock() });

            door.Interact();
            Assert.AreEqual(1, door.KnockCount);
            door.Interact();
            door.Interact();
            Assert.AreEqual(2, door.KnockCount, "Nothing left to knock for.");
        }
        finally
        {
            Object.DestroyImmediate(doorObject);
        }
    }
}
