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

    [Header("Animações (Auto-Detectado nos Filhos)")]
    [SerializeField, Tooltip("Referência ao Animator (busca automaticamente em PlayerSprite / touperaFundoSprite se vazio)")]
    private Animator characterAnimator;
    [SerializeField, Tooltip("Duração prioritária da animação de soco/arranhão")]
    private float punchAnimDuration = 0.45f;
    [SerializeField, Tooltip("Duração da animação de pouso (landedPlayer) na toupeira da frente")]
    private float landAnimDuration = 0.2f;
    [SerializeField, Tooltip("Se marcado, congela a animação de queda (fallPlayer) no último frame até tocar no chão; se desmarcado, repete apenas o trecho final da queda")]
    private bool congelarNoFinalDaQueda = true;
    [SerializeField, Range(0.5f, 0.9f), Tooltip("Caso não congele, ponto normalizado (0.5 a 0.9) de onde o final da queda reinicia")]
    private float pontoDeLoopFinalDaQueda = 0.75f;

    // --- VARIÁVEIS PÚBLICAS DE CONTROLE DE ESTADO ---
    [HideInInspector] public bool canMove = true; 

    private Rigidbody2D rb;
    private float horizontalInput;
    private float defaultGravity;
    private float scaleMultiplier = 1f;
    private bool isGrounded;
    private bool wasGrounded = true;
    private bool jumpRequested;

    // --- CONTROLE DE ANIMAÇÃO ---
    private bool isMiniAnimator = false;
    private bool hasFallState = false;
    private bool hasLandState = false;
    private string stateIdle = "IdlePlayer";
    private string stateRun = "Runplayer";
    private string stateJump = "Jump";
    private string stateFall = "fallPlayer";
    private string stateLand = "landedPlayer";
    private string statePunch = "punchPlayer";
    private string currentAnimState = "";
    private float punchAnimTimer = 0f;
    private float landAnimTimer = 0f;

    // --- POSIÇÃO SEGURA PARA RESPAWN DE LAVA ---
    private Vector3 lastSafeGroundedPosition;

    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lastSafeGroundedPosition = transform.position;
        
        // --- CÁLCULO PROPORCIONAL AUTOMÁTICO (ESCALA DO FUNDO VS FRENTE) ---
        scaleMultiplier = Mathf.Abs(transform.localScale.x);

        moveSpeed *= scaleMultiplier;
        jumpForce *= scaleMultiplier;
        maxFallSpeed *= scaleMultiplier;
        apexVelocityThreshold *= scaleMultiplier;
        
        // A gravidade base do Rigidbody é reduzida proporcionalmente
        rb.gravityScale *= scaleMultiplier;
        
        // Salvamos essa gravidade base para usar como âncora nos cálculos de força
        defaultGravity = rb.gravityScale;

        SetupAnimatorAndSprites();
    }

    private void Start()
    {
        PlayerHealth health = PlayerHealth.EnsureInstance();
        if (health != null)
        {
            health.RegistrarPersonagem(this);
        }
    }

    private void SetupAnimatorAndSprites()
    {
        if (characterAnimator == null)
        {
            characterAnimator = GetComponentInChildren<Animator>(true);
        }

        if (characterAnimator != null)
        {
            characterAnimator.enabled = true;

            if (characterAnimator.gameObject != this.gameObject && TryGetComponent<SpriteRenderer>(out SpriteRenderer rootSr))
            {
                rootSr.enabled = false;
            }

            int miniIdleHash = Animator.StringToHash("IdleMini");
            if (characterAnimator.HasState(0, miniIdleHash))
            {
                isMiniAnimator = true;
                stateIdle = "IdleMini";
                stateRun = "WalkMini";
                stateJump = "JumpMini";
                stateFall = "JumpMini";
                stateLand = "IdleMini";
                statePunch = "PunchMini";
                hasFallState = false;
                hasLandState = false;
            }
            else
            {
                isMiniAnimator = false;
                stateIdle = "IdlePlayer";
                stateRun = "Runplayer";
                stateJump = "Jump";
                stateFall = "fallPlayer";
                stateLand = "landedPlayer";
                statePunch = "punchPlayer";
                hasFallState = characterAnimator.HasState(0, Animator.StringToHash(stateFall));
                hasLandState = characterAnimator.HasState(0, Animator.StringToHash(stateLand));
            }
        }
    }

    private void OnEnable()
    {
        if (jumpAction != null && jumpAction.action != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
            jumpAction.action.performed += OnJumpPerformed;
            jumpAction.action.Enable(); 
        }
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (jumpAction != null && jumpAction.action != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
        }

        horizontalInput = 0f;
        jumpRequested = false;
        punchAnimTimer = 0f;
        if (characterAnimator != null) characterAnimator.speed = 1f;
        PlayAnimationState(stateIdle, true);
    }

    private void Update()
    {
        if (moveAction != null && moveAction.action != null)
        {
            Vector2 inputVector = moveAction.action.ReadValue<Vector2>();
            horizontalInput = inputVector.x;
        }

        bool groundedNow = groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && groundedNow)
        {
            if (hasLandState && Mathf.Abs(horizontalInput) < 0.1f)
            {
                landAnimTimer = landAnimDuration;
            }
        }

        wasGrounded = isGrounded;
        isGrounded = groundedNow;

        if (isGrounded)
        {
            lastSafeGroundedPosition = transform.position;
        }

        UpdateAnimations();
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
            landAnimTimer = 0f;
        }
    }

    private void ApplyMovement()
    {
        if (!canMove) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

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
            if (characterAnimator != null) characterAnimator.speed = 1f;
            PlayAnimationState(stateJump, true);
        }
    }

    private void ApplyGravityModifiers()
    {
        if (!canMove) return;

        if (rb.linearVelocity.y < 0)
        {
            float extraGravityForce = Mathf.Abs(Physics2D.gravity.y) * defaultGravity * (fallGravityMultiplier - 1f);
            rb.AddForce(Vector2.down * extraGravityForce * rb.mass, ForceMode2D.Force);

            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
            }
        }
        else if (rb.linearVelocity.y > 0 && rb.linearVelocity.y < apexVelocityThreshold)
        {
            float floatForce = Mathf.Abs(Physics2D.gravity.y) * defaultGravity * (1f - apexGravityMultiplier);
            rb.AddForce(Vector2.up * floatForce * rb.mass, ForceMode2D.Force);
        }
    }

    public void TriggerPunchAnimation()
    {
        if (characterAnimator == null) SetupAnimatorAndSprites();
        if (characterAnimator == null) return;

        punchAnimTimer = punchAnimDuration;
        landAnimTimer = 0f;

        characterAnimator.speed = isMiniAnimator ? 1.15f : 2.4f;
        PlayAnimationState(statePunch, true);
    }

    private void UpdateAnimations()
    {
        if (characterAnimator == null) return;

        // 1. Prioridade máxima: Animação de Soco/Arranhão em execução
        if (punchAnimTimer > 0f)
        {
            punchAnimTimer -= Time.deltaTime;
            if (punchAnimTimer > 0f) return;
            characterAnimator.speed = 1f;
        }

        if (landAnimTimer > 0f)
        {
            landAnimTimer -= Time.deltaTime;
        }

        // 2. No ar (Pulando ou Caindo)
        if (!isGrounded)
        {
            float vyThreshold = 0.1f * scaleMultiplier;
            if (rb.linearVelocity.y > vyThreshold)
            {
                PlayAnimationState(stateJump);
                ControlarFimDaAnimacaoNoAr(stateJump, true);
            }
            else if (hasFallState)
            {
                PlayAnimationState(stateFall);
                ControlarFimDaAnimacaoNoAr(stateFall, congelarNoFinalDaQueda);
            }
            else
            {
                PlayAnimationState(stateJump);
                ControlarFimDaAnimacaoNoAr(stateJump, true);
            }
            return;
        }

        // Garante que a velocidade do Animator volte ao normal (1x) assim que tocar no chão
        if (characterAnimator.speed != 1f)
        {
            characterAnimator.speed = 1f;
        }

        // 3. No chão (Correndo, Pousando ou Parado)
        if (Mathf.Abs(horizontalInput) > 0.05f && canMove)
        {
            landAnimTimer = 0f;
            PlayAnimationState(stateRun);
        }
        else if (landAnimTimer > 0f && hasLandState)
        {
            PlayAnimationState(stateLand);
        }
        else
        {
            PlayAnimationState(stateIdle);
        }
    }

    /// <summary>
    /// Impede que a animação de queda/pulo fique reiniciando do zero no ar ("convulsionando").
    /// Ao atingir o final do clipe (~92%), congela no último frame ou reinicia apenas os últimos frames.
    /// </summary>
    private void ControlarFimDaAnimacaoNoAr(string targetState, bool congelarNoUltimoFrame)
    {
        AnimatorStateInfo stateInfo = characterAnimator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(targetState)) return;

        if (stateInfo.normalizedTime >= 0.92f)
        {
            if (congelarNoUltimoFrame)
            {
                characterAnimator.speed = 0f;
            }
            else
            {
                characterAnimator.speed = 1f;
                characterAnimator.Play(Animator.StringToHash(targetState), 0, pontoDeLoopFinalDaQueda);
            }
        }
    }

    private void PlayAnimationState(string newState, bool forceRestart = false)
    {
        if (characterAnimator == null || string.IsNullOrEmpty(newState)) return;
        if (!forceRestart && currentAnimState == newState) return;

        int stateHash = Animator.StringToHash(newState);
        if (characterAnimator.HasState(0, stateHash))
        {
            currentAnimState = newState;
            characterAnimator.speed = (newState == statePunch) ? (isMiniAnimator ? 1.15f : 2.4f) : 1f;
            characterAnimator.Play(stateHash, 0, 0f);
        }
    }

    public void RespawnAtLastSafePosition()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        transform.position = lastSafeGroundedPosition + Vector3.up * (0.5f * scaleMultiplier);
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