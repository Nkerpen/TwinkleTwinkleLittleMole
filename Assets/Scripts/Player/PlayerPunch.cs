using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class PlayerPunch : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField, Tooltip("Referência para a ação de bater (Ex: Botão X ou Tecla J)")]
    private InputActionReference attackAction;

    [Header("Configurações do Ataque")]
    [SerializeField, Tooltip("Objeto vazio posicionado na frente do personagem")]
    private Transform attackPoint;
    [SerializeField, Tooltip("Tamanho da área de impacto do soco")]
    private float attackRadius = 0.5f;
    [Tooltip("Defina se ESTE personagem é o da Frente ou do Fundo")]
    public PlanoDeJogo planoDestePersonagem;

    [Header("Iluminação e Refração do Soco de Fogo")]
    [Tooltip("Se deixar vazio, busca automaticamente uma Light2D filha do PunchPoint (attackPoint)")]
    [SerializeField] private Light2D punchLight;
    [SerializeField] private float peakLightIntensity = 4.5f;
    [SerializeField] private float lightFadeDuration = 0.2f;
    [Tooltip("Opcional: SpriteRenderer filho do PunchPoint com o Material de Refração")]
    [SerializeField] private SpriteRenderer heatRefractionSprite;

    [Header("Progresso da Habilidade")]
    [Tooltip("Marque como true pelo seu SaveGame após terminar a fase de Fogo")]
    public bool isUnlockedPermanently = false;
    [SerializeField, Tooltip("Nome da cena onde a habilidade é nativa")]
    private string nativeScene = "F-FogoScene";

    private PlayerMovement playerMovement;
    private Coroutine punchLightRoutine;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (attackPoint != null)
        {
            if (punchLight == null)
                punchLight = attackPoint.GetComponentInChildren<Light2D>(true);
            if (heatRefractionSprite == null)
                heatRefractionSprite = attackPoint.GetComponentInChildren<SpriteRenderer>(true);
        }

        if (punchLight != null)
        {
            float scale = Mathf.Abs(transform.localScale.x);
            punchLight.pointLightOuterRadius *= scale;
            punchLight.intensity = 0f;
            punchLight.enabled = false;
        }

        if (heatRefractionSprite != null)
        {
            heatRefractionSprite.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.action.performed += OnAttackPerformed;
            attackAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.action.performed -= OnAttackPerformed;
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (playerMovement != null && !playerMovement.enabled) return;
        if (!CanUseAbility()) return;

        PerformPunch();
    }

    private bool CanUseAbility()
    {
        if (isUnlockedPermanently) return true;
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.hasFire) return true;
        if (SceneManager.GetActiveScene().name == nativeScene) return true;
        return false;
    }

    private void PerformPunch()
    {
        // Pulso da Point Light 2D no attackPoint (pico no impacto -> zero em 0.2s)
        if (punchLightRoutine != null) StopCoroutine(punchLightRoutine);
        punchLightRoutine = StartCoroutine(PulsePunchLightCoroutine());

        float scaledRadius = attackRadius * Mathf.Abs(transform.localScale.x);
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, scaledRadius);

        foreach (Collider2D hit in hitObjects)
        {
            if (hit.TryGetComponent<RochaQuebravel>(out RochaQuebravel rock))
            {
                if (rock.planoDestaRocha == this.planoDestePersonagem)
                {
                    rock.Break();
                }
            }
        }
    }

    private IEnumerator PulsePunchLightCoroutine()
    {
        if (punchLight != null)
        {
            punchLight.enabled = true;
            punchLight.intensity = peakLightIntensity;
        }

        if (heatRefractionSprite != null)
        {
            heatRefractionSprite.enabled = true;
        }

        float elapsed = 0f;
        while (elapsed < lightFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lightFadeDuration);
            if (punchLight != null)
                punchLight.intensity = Mathf.Lerp(peakLightIntensity, 0f, t);
            yield return null;
        }

        if (punchLight != null)
        {
            punchLight.intensity = 0f;
            punchLight.enabled = false;
        }

        if (heatRefractionSprite != null)
        {
            heatRefractionSprite.enabled = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            float scaledRadius = attackRadius * Mathf.Abs(transform.localScale.x);
            Gizmos.DrawWireSphere(attackPoint.position, scaledRadius);
        }
    }
}
