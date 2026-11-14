using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(10)]
public class PlayerGlide : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private InputActionReference jumpAction;

    [Header("Identidade")]
    public PlanoDeJogo planoDestePersonagem;

    [Header("Configurações do Planador")]
    [SerializeField] private float glideFallSpeed = 2f;
    [Tooltip("Quão rápido o vento levanta a toupeira? Valores menores = mais suave")]
    [SerializeField] private float windAcceleration = 15f;

    [Header("Progresso da Habilidade")]
    public bool isUnlockedPermanently = false;
    [SerializeField] private string nativeScene = "F-VentoScene";

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    
    [HideInInspector] public bool isInWind = false;
    [HideInInspector] public Vector2 currentWindVelocity;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        
        // --- CÁLCULO PROPORCIONAL DA HABILIDADE ---
        // Adapta a velocidade de queda e a aceleração do vento para o tamanho da toupeira (ex: 0.2)
        float scaleMultiplier = Mathf.Abs(transform.localScale.x);
        glideFallSpeed *= scaleMultiplier;
        windAcceleration *= scaleMultiplier;
    }

    private void FixedUpdate()
    {
        if (playerMovement != null && !playerMovement.enabled) return;
        if (!CanUseAbility()) return;

        bool isHoldingJump = jumpAction.action.ReadValue<float>() > 0.1f;
        
        // O planador abre se o jogador estiver caindo (<= 0) OU se estiver dentro do vento
        bool canGlide = isHoldingJump && (rb.linearVelocity.y <= 0f || isInWind);

        if (canGlide)
        {
            if (isInWind)
            {
                // A MÁGICA DA FLUIDEZ: Acelera suavemente a velocidade vertical até o limite do vento
                float smoothVelocityY = Mathf.MoveTowards(rb.linearVelocity.y, currentWindVelocity.y, windAcceleration * Time.fixedDeltaTime);
                
                // Mantém o controle do Player (andar pros lados) e soma a força horizontal do vento
                float smoothVelocityX = rb.linearVelocity.x + currentWindVelocity.x;
                
                rb.linearVelocity = new Vector2(smoothVelocityX, smoothVelocityY);
            }
            else
            {
                // Queda suave normal
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -glideFallSpeed));
            }
        }
    }

private bool CanUseAbility()
    {
        if (isUnlockedPermanently) return true; // Sua trava de debug do Inspector
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.hasWind) return true; // Destravado globalmente!
        if (SceneManager.GetActiveScene().name == nativeScene) return true;
        return false;
    }

    public void ApplyWind(Vector2 windVel)
    {
        isInWind = true;
        
        // --- CÁLCULO PROPORCIONAL DO VENTO ---
        // Diminui a força bruta da rajada para não arremessar o personagem minúsculo na estratosfera
        float scaleMultiplier = Mathf.Abs(transform.localScale.x);
        currentWindVelocity = windVel * scaleMultiplier;
    }

    public void RemoveWind()
    {
        isInWind = false;
        currentWindVelocity = Vector2.zero;
    }
}