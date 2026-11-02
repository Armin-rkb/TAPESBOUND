using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Movement keys")]
    [SerializeField] private string horizontal = "";
    [SerializeField] private string vertical = "";
    [HideInInspector] public Vector2 movementDirection;

    // True on the frame up/down is pushed, for moving through menus.
    [HideInInspector] public bool menuUpPressed;
    [HideInInspector] public bool menuDownPressed;
    private float previousVertical = 0f;

    [Header("Using menu key")]
    [SerializeField] private KeyCode menuKey = KeyCode.LeftShift;
    [HideInInspector] public bool menuKeyPressed;

    [Header("Interacting key")]
    [SerializeField] private KeyCode interactKey = KeyCode.Space;
    [HideInInspector] public bool interactKeyPressed;

    [Header("Quest log key")]
    [SerializeField] private KeyCode questLogKey = KeyCode.Q;
    [HideInInspector] public bool questLogKeyPressed;

    void Update()
    {
        movementDirection = new Vector2(Input.GetAxisRaw(horizontal), Input.GetAxisRaw(vertical));
        movementDirection.Normalize();

        float verticalInput = Input.GetAxisRaw(vertical);
        menuUpPressed = verticalInput > 0.5f && previousVertical <= 0.5f;
        menuDownPressed = verticalInput < -0.5f && previousVertical >= -0.5f;
        previousVertical = verticalInput;

        menuKeyPressed = Input.GetKeyDown(menuKey);

        interactKeyPressed = Input.GetKeyDown(interactKey);

        questLogKeyPressed = Input.GetKeyDown(questLogKey);
    }
}
