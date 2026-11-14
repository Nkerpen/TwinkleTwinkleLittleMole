using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerShoot : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private InputActionReference shootAction;

    [Header("Identidade")]
    public PlanoDeJogo planoDestePersonagem;

    [Header("Configurações de Munição")]
    public int maxAmmo = 3;
    [SerializeField] private int currentAmmo;

    [Header("Configurações do Disparo")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField, Tooltip("Ponto vazio na frente da arma/mão de onde o tiro sai")]
    private Transform shootPoint;

    [Header("Progresso da Habilidade")]
    public bool isUnlockedPermanently = false;
    [SerializeField] private string nativeScene = "F-TerraScene";

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        currentAmmo = maxAmmo; // Começa com munição cheia
    }

    private void OnEnable()
    {
        if (shootAction != null)
        {
            shootAction.action.performed += OnShootPerformed;
            shootAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (shootAction != null)
        {
            shootAction.action.performed -= OnShootPerformed;
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (playerMovement != null && !playerMovement.enabled) return;
        if (!CanUseAbility()) return;

        // Se tem munição, atira!
        if (currentAmmo > 0)
        {
            PerformShoot();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Sem munição! Precisa recarregar.");
        }
    }

    private void PerformShoot()
    {
        currentAmmo--;
        Debug.Log($"[{gameObject.name}] Atirou! Munição restante: {currentAmmo}");

        // Cria o tiro na posição do ShootPoint
        Projectile newProjectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        // Identifica para que lado o boneco está olhando (1 para direita, -1 para esquerda)
        float facingDirection = Mathf.Sign(transform.localScale.x);

        // Identifica a proporção real do boneco (ex: 1 ou 0.2)
        float scaleMultiplier = Mathf.Abs(transform.localScale.x);

        // Passa todas as informações para o tiro recém-criado
        newProjectile.Setup(facingDirection, scaleMultiplier, planoDestePersonagem);
    }

    // Retorna verdadeiro se a munição atual for menor que a máxima
    public bool NeedsAmmo()
    {
        return currentAmmo < maxAmmo;
    }

    // Função que será chamada pela base de recarga
    public void ReloadAmmo()
    {
        currentAmmo = maxAmmo;
        Debug.Log($"[{gameObject.name}] Recarregado! Munição: {currentAmmo}");
    }

private bool CanUseAbility()
    {
        if (isUnlockedPermanently) return true; // Sua trava de debug do Inspector
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.hasWater) return true; // Destravado globalmente!
        if (SceneManager.GetActiveScene().name == nativeScene) return true;
        return false;
    }
}