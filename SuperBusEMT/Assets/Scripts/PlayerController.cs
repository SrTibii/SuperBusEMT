using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer = 1;

    [Header("References")]
    [SerializeField] private Transform groundCheckPoint;

    public InputActionReference triggerMovement;
    public InputActionReference triggerJump;

    // Componentes
    private Rigidbody rb;
    private PlayerInput playerInput;

    // Input values
    private Vector2 moveInput;
    private bool jumpPressed;

    // States
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        // Si no hay un punto asignado para ground check, usar la posición del objeto
        if (groundCheckPoint == null)
            groundCheckPoint = transform;
    }

    void OnEnable()
    {
        /*
        // Suscribirse a los eventos de input
        if (playerInput != null)
        {
            playerInput.actions["Movement"].performed += OnMovementPerformed;
            playerInput.actions["Movement"].canceled += OnMovementCanceled;
            playerInput.actions["Salto"].performed += OnJumpPerformed;
        }
        */

        triggerMovement.action.performed += OnTriggerPressedMovement;
        triggerMovement.action.canceled += OnMovementCanceled;
        triggerMovement.action.Enable();

        triggerJump.action.performed += OnTriggerPressedJump;
        triggerJump.action.Enable();
    }

    void OnDisable()
    {

        triggerMovement.action.performed -= OnTriggerPressedMovement;
        triggerMovement.action.canceled -= OnMovementCanceled;
        triggerMovement.action.Disable();

        triggerJump.action.performed -= OnTriggerPressedJump;
        triggerJump.action.Disable();
        /*
        // Desuscribirse de los eventos
        if (playerInput != null)
        {
            playerInput.actions["Movement"].performed -= OnMovementPerformed;
            playerInput.actions["Movement"].canceled -= OnMovementCanceled;
            playerInput.actions["Salto"].performed -= OnJumpPerformed;
        }
        */
    }

    private void OnTriggerPressedMovement(InputAction.CallbackContext context)
    {
        //Debug.Log("Movimiento: " + moveInput);
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnTriggerPressedJump(InputAction.CallbackContext context)
    {
        Debug.Log("Salto presionado");
        jumpPressed = true;
    }

    private void Update()
    {
        // Ground check mediante raycast
        CheckGrounded();
    }

    private void FixedUpdate()
    {
        // Aplicar movimiento
        HandleMovement();

        // Aplicar salto
        HandleJump();
    }

    private void CheckGrounded()
    {
        // Lanzar un raycast hacia abajo para verificar si está en el suelo
        RaycastHit hit;
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, out hit, groundCheckDistance, groundLayer);

        // Debug visual (opcional)
        Debug.DrawRay(groundCheckPoint.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    private void HandleMovement()
    {
        // Crear vector de movimiento basado en el input
        Vector3 moveDirection = new Vector3(moveInput.y, 0, -moveInput.x).normalized; //Ejes invertidos para que adelante sea hacia adelante del personaje

        // Aplicar movimiento manteniendo la velocidad actual en Y (para no interferir con la gravedad)
        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;

        // Aplicar la velocidad al Rigidbody
        rb.linearVelocity = targetVelocity;
    }

    private void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            // Mantener la velocidad horizontal actual y aplicar fuerza de salto
            Vector3 jumpVelocity = rb.linearVelocity;
            jumpVelocity.y = jumpForce;
            rb.linearVelocity = jumpVelocity;

            jumpPressed = false;
        }
        else if (jumpPressed && !isGrounded)
        {
            // Si no está en el suelo, resetear el input de salto
            jumpPressed = false;
        }
    }

    // Métodos públicos para obtener estados (útiles para animaciones)
    public bool IsGrounded() => isGrounded;
    public Vector2 GetMoveInput() => moveInput;
    public bool IsMoving() => moveInput.magnitude > 0.1f;

}
