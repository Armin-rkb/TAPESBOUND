using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Interactable : MonoBehaviour, iInteractable
{
    private void OnTriggerEnter2D(Collider2D a_collision)
    {
        if (a_collision != null)
        {
            if (a_collision.gameObject.tag == "Player")
            {
                a_collision.GetComponent<PlayerState>().currentInteractable = this;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D a_collision)
    {
        if (a_collision != null)
        {
            if (a_collision.gameObject.tag == "Player")
            {
                a_collision.GetComponent<PlayerState>().currentInteractable = null;
            }
        }
    }

    public virtual void Interact() {}
}
