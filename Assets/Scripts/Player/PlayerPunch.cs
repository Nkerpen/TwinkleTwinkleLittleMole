using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PlayerPunch : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField, Tooltip("Referência para a ação de bater (Ex: Botão X ou Tecla J)")]
    private InputActionReference attackAction;

    [Header("Configurações do Ataque")]
    [SerializeField, Tooltip("Objeto vazio posicionado na frente do personagem")]
    private Transform attackPoint;

    [SerializeField, Tooltip("Tamanho base da área de impacto do soco")]
    private float attackRadius = 0.85f;

    [SerializeField, Tooltip("Tempo mínimo entre socos seguidos")]
    private float attackCooldown = 0.22f;

    [Header("Luz do Soco (Ativa Apenas ao Socar)")]
    [SerializeField, Tooltip("Arraste aqui o objeto da Luz (Light2D) do soco. Se vazio, busca automaticamente um Light2D dentro do PunchPoint (attackPoint)")]
    private Light2D punchLight;

    [SerializeField, Tooltip("Tempo (em segundos) que a luz do soco permanece acesa antes de apagar")]
    private float punchLightDuration = 0.22f;

    [SerializeField, Tooltip("Se marcado, a luz apaga suavemente (fade out) em vez de desligar de uma vez")]
    private bool suavizarApagamentoDaLuz = true;

    [Header("Input Buffering & Janela de Parry / Quebra de Pedras")]
    [SerializeField, Tooltip("Se o jogador apertar o botão um pouco antes do cooldown terminar ou antes de encostar na pedra/projétil, guarda o comando por este tempo (em segundos)")]
    private float inputBufferTime = 0.22f;

    [SerializeField, Tooltip("Duração (em segundos) em que a hitbox do soco/parry permanece ativa após iniciar o golpe (permite quebrar pedras e rebater projéteis com muito mais folga!)")]
    private float activeAttackWindow = 0.28f;

    [SerializeField, Tooltip("Multiplicador extra do raio de alcance especificamente para rebater projéteis (Parry mais generoso)")]
    private float parryRadiusMultiplier = 1.65f;

    [Tooltip("Defina se ESTE personagem é o da Frente ou do Fundo")]
    public PlanoDeJogo planoDestePersonagem;

    [Header("Progresso da Habilidade")]
    [Tooltip("Marque como true pelo seu SaveGame após terminar a fase de Fogo")]
    public bool isUnlockedPermanently = false;

    [SerializeField, Tooltip("Nome da cena onde a habilidade é nativa")]
    private string nativeScene = "F-Fogo2";

    private PlayerMovement playerMovement;
    private float nextAttackTime = 0f;

    // --- CONTROLE DE INPUT BUFFER E JANELA ATIVA DE PARRY/ATAQUE ---
    private bool hasBufferedAttack = false;
    private float lastAttackInputTime = -10f;
    private float activeWindowEndTime = -10f;

    // --- CONTROLE DA LUZ DO SOCO ---
    private float basePunchLightIntensity = 1f;
    private Coroutine punchLightRoutine;

    /// <summary>
    /// Verdadeiro enquanto a janela ativa de soco/parry estiver aberta.
    /// Usado também pelo FireTrapProjectile para refletir automaticamente caso encoste na toupeira durante o parry!
    /// </summary>
    public bool IsParrying => Time.time <= activeWindowEndTime;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        InicializarLuzDoSoco();
    }

    private void InicializarLuzDoSoco()
    {
        if (punchLight == null && attackPoint != null)
        {
            punchLight = attackPoint.GetComponentInChildren<Light2D>(true);
        }

        if (punchLight != null)
        {
            basePunchLightIntensity = punchLight.intensity;
            // Garante que a luz comece totalmente desligada até o jogador socar
            punchLight.enabled = false;
            if (punchLight.gameObject != this.gameObject && (attackPoint == null || punchLight.gameObject != attackPoint.gameObject))
            {
                punchLight.gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        if (attackAction != null && attackAction.action != null)
        {
            attackAction.action.performed -= OnAttackPerformed;
            attackAction.action.performed += OnAttackPerformed;
            attackAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (attackAction != null && attackAction.action != null)
        {
            attackAction.action.performed -= OnAttackPerformed;
        }

        hasBufferedAttack = false;
        activeWindowEndTime = -10f;
        DesligarLuzDoSocoImediatamente();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        RegistrarInputNoBuffer();
    }

    private void RegistrarInputNoBuffer()
    {
        if (playerMovement != null && !playerMovement.enabled) return;
        if (!CanUseAbility()) return;

        hasBufferedAttack = true;
        lastAttackInputTime = Time.time;

        // Se o cooldown já estiver livre neste exato frame, inicia o ataque imediatamente!
        TentarConsumirBufferDeAtaque();
    }

    private void Update()
    {
        if (playerMovement != null && !playerMovement.enabled)
        {
            hasBufferedAttack = false;
            activeWindowEndTime = -10f;
            return;
        }

        // Garante que a Action esteja ativa e captura o clique mesmo se o evento performed falhar
        if (attackAction != null && attackAction.action != null)
        {
            if (!attackAction.action.enabled)
                attackAction.action.Enable();

            if (attackAction.action.WasPressedThisFrame())
            {
                RegistrarInputNoBuffer();
            }
        }

        // 1. Verifica se há um ataque guardado no Input Buffer pronto para disparar
        TentarConsumirBufferDeAtaque();

        // 2. Enquanto a janela ativa de ataque/parry estiver aberta (ex: 0.28s),
        // continua checando colisões a cada frame para quebrar pedras (RochaQuebravel) e rebater projéteis!
        if (Time.time <= activeWindowEndTime)
        {
            VerificarImpactosDoSocoEParry(false);
        }
    }

    private void FixedUpdate()
    {
        // Checa também no passo de física para nunca perder projéteis rápidos ou pedras durante o movimento
        if (Time.time <= activeWindowEndTime)
        {
            VerificarImpactosDoSocoEParry(false);
        }
    }

    private void TentarConsumirBufferDeAtaque()
    {
        if (!hasBufferedAttack) return;

        // Se o tempo do buffer expirou, descarta
        if (Time.time - lastAttackInputTime > inputBufferTime)
        {
            hasBufferedAttack = false;
            return;
        }

        // Assim que o cooldown permitir, consome o buffer e inicia o soco + janela de parry + luz do soco!
        if (Time.time >= nextAttackTime)
        {
            hasBufferedAttack = false;
            nextAttackTime = Time.time + attackCooldown;
            activeWindowEndTime = Time.time + activeAttackWindow;

            if (playerMovement != null)
            {
                playerMovement.TriggerPunchAnimation();
            }

            AcionarLuzDoSoco();
            VerificarImpactosDoSocoEParry(true);
        }
    }

    private void AcionarLuzDoSoco()
    {
        if (punchLight == null || !gameObject.activeInHierarchy) return;

        if (punchLightRoutine != null)
        {
            StopCoroutine(punchLightRoutine);
        }

        punchLightRoutine = StartCoroutine(RotinaLuzDoSoco());
    }

    private IEnumerator RotinaLuzDoSoco()
    {
        if (!punchLight.gameObject.activeSelf)
        {
            punchLight.gameObject.SetActive(true);
        }

        punchLight.enabled = true;
        punchLight.intensity = basePunchLightIntensity;

        float duracao = Mathf.Max(0.05f, punchLightDuration);

        if (suavizarApagamentoDaLuz)
        {
            float elapsed = 0f;
            while (elapsed < duracao)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duracao);
                punchLight.intensity = Mathf.Lerp(basePunchLightIntensity, 0f, t);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(duracao);
        }

        DesligarLuzDoSocoImediatamente();
        punchLightRoutine = null;
    }

    private void DesligarLuzDoSocoImediatamente()
    {
        if (punchLightRoutine != null)
        {
            StopCoroutine(punchLightRoutine);
            punchLightRoutine = null;
        }

        if (punchLight != null)
        {
            punchLight.intensity = basePunchLightIntensity;
            punchLight.enabled = false;
            if (punchLight.gameObject != this.gameObject && (attackPoint == null || punchLight.gameObject != attackPoint.gameObject))
            {
                punchLight.gameObject.SetActive(false);
            }
        }
    }

    private bool CanUseAbility()
    {
        if (isUnlockedPermanently) return true;
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.hasFire) return true;

        string activeScene = SceneManager.GetActiveScene().name;
        if (activeScene == nativeScene || activeScene == "F-Fogo2" || activeScene == "F-FogoScene")
            return true;

        return false;
    }

    private void VerificarImpactosDoSocoEParry(bool isInitialFrame)
    {
        Vector3 punchCenter = attackPoint != null ? attackPoint.position : transform.position;
        float scaleMultiplier = Mathf.Abs(transform.localScale.x);

        // Raio generoso para quebrar pedras, armadilhas e acionar alavancas durante toda a janela ativa
        float effectiveBaseRadius = Mathf.Max(attackRadius, 0.85f);
        float scaledAttackRadius = effectiveBaseRadius * 1.2f * scaleMultiplier;

        // 1. Checa objetos quebráveis/acionáveis na área do soco (e também ligeiramente ao redor do corpo ao andar)
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(punchCenter, scaledAttackRadius);
        ProcessarAlvosFisicos(hitObjects);

        // Também faz uma checagem complementar centrada no corpo da toupeira para quebrar pedras coladas no jogador
        Collider2D[] bodyObjects = Physics2D.OverlapCircleAll(transform.position, effectiveBaseRadius * 0.95f * scaleMultiplier);
        ProcessarAlvosFisicos(bodyObjects);

        // 2. Janela de Parry ampliada para rebater projéteis de fogo (FireTrapProjectile)
        float scaledParryRadius = effectiveBaseRadius * Mathf.Max(1.2f, parryRadiusMultiplier) * scaleMultiplier;
        Collider2D[] parryHitsFront = Physics2D.OverlapCircleAll(punchCenter, scaledParryRadius);
        ProcessarParryDeProjeteis(parryHitsFront);

        Collider2D[] parryHitsBody = Physics2D.OverlapCircleAll(transform.position, scaledParryRadius * 0.85f);
        ProcessarParryDeProjeteis(parryHitsBody);
    }

    private void ProcessarAlvosFisicos(Collider2D[] hits)
    {
        if (hits == null) return;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            // A) Quebrar Rochas (RochaQuebravel) do mesmo plano (com suporte ao Input Buffering e Janela Ativa!)
            RochaQuebravel rock = hit.GetComponent<RochaQuebravel>();
            if (rock == null) rock = hit.GetComponentInParent<RochaQuebravel>();
            if (rock != null && rock.planoDestaRocha == this.planoDestePersonagem)
            {
                rock.Break();
            }

            // B) Acionar Alavancas (ou gatilhos com AceitaSoco = true) do mesmo plano
            ShootableButton trigger = hit.GetComponent<ShootableButton>();
            if (trigger == null) trigger = hit.GetComponentInParent<ShootableButton>();
            if (trigger != null && trigger.planoDesteBotao == this.planoDestePersonagem && trigger.AceitaSoco)
            {
                trigger.Activate();
            }

            // C) Destruir / Fechar as Armadilhas de Fogo (FireTrap) do mesmo plano, transformando-as em plataforma
            FireTrap trap = hit.GetComponent<FireTrap>();
            if (trap == null) trap = hit.GetComponentInParent<FireTrap>();
            if (trap != null && trap.planoDestaArmadilha == this.planoDestePersonagem)
            {
                trap.DeactivateTrap();
            }
        }
    }

    private void ProcessarParryDeProjeteis(Collider2D[] hits)
    {
        if (hits == null) return;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            // D) Refletir Projéteis de Fogo (FireTrapProjectile) de volta para a armadilha
            if (hit.TryGetComponent<FireTrapProjectile>(out FireTrapProjectile fireball))
            {
                if (fireball.planoDoProjetil == this.planoDestePersonagem)
                {
                    fireball.Reflect();
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = attackPoint != null ? attackPoint.position : transform.position;
        float scaleMultiplier = Mathf.Abs(transform.localScale.x);
        float baseR = Mathf.Max(attackRadius, 0.85f);

        // Vermelho: Raio de quebra de pedras / ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, baseR * 1.2f * scaleMultiplier);

        // Ciano: Raio ampliado da Janela de Parry de projéteis
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, baseR * Mathf.Max(1.2f, parryRadiusMultiplier) * scaleMultiplier);
    }
}