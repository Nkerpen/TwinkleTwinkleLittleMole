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
    [Tooltip("Multiplicador da força puxando para baixo ao cair")]
    [SerializeField] private float fallGravityMultiplier = 3f;
    
    [Header("Hang Time (Pico do Pulo)")]
    [SerializeField] private float apexVelocityThreshold = 2f;
    [Tooltip("Multiplicador da gravidade no pico do pulo (ex: 0.5 = metade do peso)")]
    [SerializeField] private float apexGravityMultiplier = 0.5f;

    [Header("Detecção de Chão")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // --- VARIÁVEIS PÚBLICAS DE CONTROLE DE ESTADO ---
    [HideInInspector] public bool canMove = true; 

    private Rigidbody2D rb;
    private float horizontalInput;
    private float defaultGravity;
    private bool isGrounded;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // --- CÁLCULO PROPORCIONAL AUTOMÁTICO (ESCALA DO FUNDO VS FRENTE) ---
        float scaleMultiplier = Mathf.Abs(transform.localScale.x);

        moveSpeed *= scaleMultiplier;
        jumpForce *= scaleMultiplier;
        maxFallSpeed *= scaleMultiplier;
        apexVelocityThreshold *= scaleMultiplier;
        
        // A gravidade base do Rigidbody é reduzida proporcionalmente
        rb.gravityScale *= scaleMultiplier;
        
        // Salvamos essa gravidade base para usar como âncora nos cálculos de força
        defaultGravity = rb.gravityScale;
    }

    private void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.action.performed += OnJumpPerformed;
            jumpAction.action.Enable(); 
        }
        if (moveAction != null) moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (jumpAction != null)
        {
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

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyJumpLogic();
        ApplyGravityModifiers();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded && canMove)
        {
            jumpRequested = true;
        }
    }

    private void ApplyMovement()
    {
        // Se estiver no chicote ou paralisado, a toupeira não anda sozinha e preserva a inércia
        if (!canMove) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Vira o personagem
        if (horizontalInput != 0)
        {
            Vector3 currentScale = transform.localScale;
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
        // PULO DO GATO: Se estiver no cipó (!canMove), não interferimos na física! 
        // O motor da Unity resolve o pêndulo sozinho usando a defaultGravity base.
        if (!canMove) return;

        if (rb.linearVelocity.y < 0)
        {
            // O Unity já puxa o boneco para baixo na força de "1x".
            // Para chegar no fallGravityMultiplier (ex: 3x), adicionamos o que falta (2x).
            float extraGravityForce = Mathf.Abs(Physics2D.gravity.y) * defaultGravity * (fallGravityMultiplier - 1f);
            rb.AddForce(Vector2.down * extraGravityForce * rb.mass, ForceMode2D.Force);

            // Trava de segurança para não ultrapassar a velocidade terminal caindo
            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
            }
        }
        else if (rb.linearVelocity.y > 0 && rb.linearVelocity.y < apexVelocityThreshold)
        {
            // Hang Time: Se o apexGravityMultiplier for 0.5, anulamos 50% da gravidade empurrando o boneco para CIMA.
            float floatForce = Mathf.Abs(Physics2D.gravity.y) * defaultGravity * (1f - apexGravityMultiplier);
            rb.AddForce(Vector2.up * floatForce * rb.mass, ForceMode2D.Force);
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