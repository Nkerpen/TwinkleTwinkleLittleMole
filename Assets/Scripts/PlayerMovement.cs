using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Pulo & Gravidade")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float maxFallSpeed = 20f;
    [SerializeField] private float fallGravityMultiplier = 3f;
    
    [Header("Hang Time (Pico do Pulo)")]
    [SerializeField] private float apexVelocityThreshold = 2f;
    [SerializeField] private float apexGravityMultiplier = 0.5f;

    [Header("Detecção de Chão")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float defaultGravity;
    private bool isGrounded;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
    }

    private void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.action.performed += OnJumpPerformed;
            // Habilita a ação para garantir que o input esteja funcionando
            jumpAction.action.Enable(); 
        }
        if (moveAction != null) moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (jumpAction != null)
        {
            // Apenas desvincula o personagem atual do evento, 
            // mas NÃO desativa o Input para não atrapalhar o outro personagem!
            jumpAction.action.performed -= OnJumpPerformed;
        }
    }

    private void Update()
    {
        if (moveAction != null)
        {
            Vector2 inputVector = moveAction.action.ReadValue<Vector2>();
            horizontalInput = inputVector.x;
        }
// --- INÍCIO DO CÓDIGO DE DIAGNÓSTICO ---
        Collider2D hitInfo = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
        
        if (hitInfo != null)
        {
            Debug.Log($"[DEBUG] O raio detectou o objeto: {hitInfo.gameObject.name} | Layer dele: {LayerMask.LayerToName(hitInfo.gameObject.layer)}");
        }
        else
        {
            Debug.Log("[DEBUG] O raio não está encostando em NENHUM colisor.");
        }

        // Mantemos a verificação original rodando para não quebrar nada
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        // --- FIM DO CÓDIGO DE DIAGNÓSTICO ---
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyJumpLogic();
        ApplyGravityModifiers();
    }

private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log($"Botão de pulo apertado pelo {gameObject.name}!");

        if (isGrounded)
        {
            Debug.Log($"{gameObject.name} está no chão. Pulando!");
            jumpRequested = true;
        }
        else
        {
            Debug.Log($"{gameObject.name} tentou pular, mas o isGrounded é FALSO.");
        }
    }

   private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Vira o personagem preservando a escala original dele (seja 1 ou 0.2)
        if (horizontalInput != 0)
        {
            Vector3 currentScale = transform.localScale;
            // Mathf.Abs pega o valor positivo da escala. Mathf.Sign pega a direção (1 ou -1).
            currentScale.x = Mathf.Abs(currentScale.x) * Mathf.Sign(horizontalInput);
            transform.localScale = currentScale;
        }
    }
    private void ApplyJumpLogic()
    {
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
        }
    }

    private void ApplyGravityModifiers()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = defaultGravity * fallGravityMultiplier;
            
            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
            }
        }
        else if (rb.linearVelocity.y > 0 && rb.linearVelocity.y < apexVelocityThreshold)
        {
            rb.gravityScale = defaultGravity * apexGravityMultiplier;
        }
        else
        {
            rb.gravityScale = defaultGravity;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}