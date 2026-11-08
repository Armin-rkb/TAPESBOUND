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
