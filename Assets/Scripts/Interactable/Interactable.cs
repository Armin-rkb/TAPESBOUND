using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Interactable : MonoBehaviour, IInteractable
{
    // Who talks to / checks this: the leader, or a party member played by another player.
    protected GameObject playerRef = null;

    /// <summary>
    /// Interact as a_interactor, so an NPC turns to whoever talked to them.
    /// </summary>
    public static void InteractAs(IInteractable a_interactable, GameObject a_interactor)
    {
        if (a_interactable is Interactable interactable)
        {
            interactable.playerRef = a_interactor;
        }
        a_interactable.Interact();
    }

    private void OnTriggerEnter2D(Collider2D a_collision)
    {
        if (a_collision != null)
        {
            if (a_collision.CompareTag(Tags.Player) && a_collision.TryGetComponent(out PlayerController player))
            {
                player.currentInteractable = this;
                playerRef = a_collision.gameObject;
            }
            else if (a_collision.TryGetComponent(out Follower follower) && follower.IsControlled)
            {
                follower.currentInteractable = this;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D a_collision)
    {
        if (a_collision != null)
        {
            if (a_collision.CompareTag(Tags.Player) && a_collision.TryGetComponent(out PlayerController player))
            {
                // Another interactable may have taken over already.
                if (player.currentInteractable == (IInteractable)this)
                {
                    player.currentInteractable = null;
                }
                if (playerRef == a_collision.gameObject)
                {
                    playerRef = null;
                }
            }
            else if (a_collision.TryGetComponent(out Follower follower) && follower.currentInteractable == (IInteractable)this)
            {
                follower.currentInteractable = null;
            }
        }
    }

    public virtual void Interact() {}
}
