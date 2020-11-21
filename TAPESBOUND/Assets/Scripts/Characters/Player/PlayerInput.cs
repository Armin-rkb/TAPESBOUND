using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Movement keys")]
    [SerializeField] private string horizontal = "";
    [SerializeField] private string vertical = "";
    [HideInInspector] public Vector2 movementDirection;

    [Header("Using menu key")]
    [SerializeField] private KeyCode menuKey = KeyCode.LeftShift;
    [HideInInspector] public bool menuKeyPressed;

    [Header("Interacting key")]
    [SerializeField] private KeyCode interactKey = KeyCode.Space;
    [HideInInspector] public bool interactKeyPressed;

    void Update()
    {
        movementDirection = new Vector2(Input.GetAxisRaw(horizontal), Input.GetAxisRaw(vertical));
        movementDirection.Normalize();

        menuKeyPressed = Input.GetKeyDown(menuKey);

        interactKeyPressed = Input.GetKeyDown(interactKey);
    }
}
