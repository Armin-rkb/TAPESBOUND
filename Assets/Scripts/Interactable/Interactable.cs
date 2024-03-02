using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Interactable : MonoBehaviour, IInteractable
{
    protected GameObject playerRef = null;

    private void OnTriggerEnter2D(Collider2D a_collision)
    {
        if (a_collision != null)
        {
            if (a_collision.gameObject.tag == Tags.Player)
            {
                a_collision.GetComponent<PlayerController>().currentInteractable = this;
                playerRef = a_collision.gameObject;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D a_collision)
    {
        if (a_collision != null)
        {
            if (a_collision.gameObject.tag == Tags.Player)
            {
                a_collision.GetComponent<PlayerController>().currentInteractable = null;
                playerRef = null;
            }
        }
    }

    public virtual void Interact() {}
}
