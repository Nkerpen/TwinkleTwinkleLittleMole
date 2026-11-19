using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireTrapProjectile : MonoBehaviour
{
    [Header("Configurações do Projétil de Fogo")]
    public float lifeTime = 5f;
    public PlanoDeJogo planoDoProjetil;

    [Header("Spot Light 2D (URP 2D)")]
    [SerializeField, Tooltip("Adiciona automaticamente uma Spot Light 2D (Point) na bola de fogo para iluminar o cenário")]
    private bool usarSpotLight2D = true;

    [SerializeField, Tooltip("Cor da luz da bola de fogo normal")]
    private Color corLuzNormal = new Color(1f, 0.45f, 0.1f, 1f);

    [SerializeField, Tooltip("Cor da luz quando o projétil é rebatido (Parry)")]
    private Color corLuzRefletida = new Color(0.2f, 0.95f, 1f, 1f);

    [SerializeField, Tooltip("Intensidade base da Spot Light 2D")]
    private float intensidadeLuz = 1.8f;

    [SerializeField, Tooltip("Raio externo base da luz na escala 1.0 (ajustado automaticamente para 0.2 no Fundo)")]
    private float raioExternoLuz = 2.6f;

    private FireTrap ownerTrap;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Light2D spotLight2D;
    private float directionX;
    private float speed;
    private bool isReflected = false;

    public bool IsReflected => isReflected;

    public void Initialize(FireTrap owner, float dirX, float baseSpeed, float scaleMultiplier, PlanoDeJogo plano, int sortingOrder)
    {
        ownerTrap = owner;
        directionX = Mathf.Sign(dirX);
        planoDoProjetil = plano;
        speed = baseSpeed * scaleMultiplier;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // Ajusta o tamanho proporcionalmente ao plano (1.0 na Frente, 0.2 na Pintura do Fundo)
        transform.localScale = Vector3.one * (0.95f * scaleMultiplier);

        if (sr != null)
        {
            sr.sortingOrder = sortingOrder;
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(directionX * speed, 0f);
        }

        if (usarSpotLight2D)
        {
            ConfigurarSpotLight2D(scaleMultiplier);
        }

        Destroy(gameObject, lifeTime);
    }

    private void ConfigurarSpotLight2D(float scaleMultiplier)
    {
        spotLight2D = GetComponentInChildren<Light2D>(true);
        if (spotLight2D == null)
        {
            GameObject lightObj = new GameObject("SpotLight2D_Projetil");
            lightObj.transform.SetParent(transform, false);
            lightObj.transform.localPosition = Vector3.zero;
            spotLight2D = lightObj.AddComponent<Light2D>();
        }

        // Em URP 2D, "Spot Light 2D" corresponde ao LightType.Point
        spotLight2D.lightType = Light2D.LightType.Point;
        spotLight2D.color = corLuzNormal;
        spotLight2D.intensity = intensidadeLuz;
        spotLight2D.pointLightInnerRadius = 0.4f * scaleMultiplier;
        spotLight2D.pointLightOuterRadius = raioExternoLuz * scaleMultiplier;
        spotLight2D.falloffIntensity = 0.6f;
    }

    private void Update()
    {
        // Caso o Rigidbody seja cinemático, garante o deslocamento contínuo
        if (rb == null)
        {
            transform.position += Vector3.right * (directionX * speed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Reflete a bola de fogo quando atingida pelo Soco (PlayerPunch), conforme o GDD Seção 4.7!
    /// </summary>
    public void Reflect()
    {
        if (isReflected) return;
        isReflected = true;

        directionX *= -1f;
        speed *= 1.6f;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(directionX * speed, 0f);
        }

        if (sr != null)
        {
            sr.color = corLuzRefletida; // Fica azul-ciano brilhante ao ser refletido
        }

        if (spotLight2D != null)
        {
            spotLight2D.color = corLuzRefletida;
            spotLight2D.intensity = intensidadeLuz * 1.35f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ProcessarColisao(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Se o jogador ativar o Parry enquanto o projétil já está sobreposto à borda do colisor,
        // garante a reflexão imediata!
        if (!isReflected && collision != null && collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<PlayerPunch>(out PlayerPunch punch) &&
                punch.planoDestePersonagem == this.planoDoProjetil &&
                punch.IsParrying)
            {
                Reflect();
            }
        }
    }

    private void ProcessarColisao(Collider2D collision)
    {
        if (collision == null) return;

        // 1. Se o projétil foi refletido pelo soco do jogador, ele pode destruir a própria armadilha!
        if (isReflected)
        {
            FireTrap trap = collision.GetComponent<FireTrap>();
            if (trap == null) trap = collision.GetComponentInParent<FireTrap>();

            if (trap != null && trap.planoDestaArmadilha == this.planoDoProjetil)
            {
                trap.DeactivateTrap();
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            // Ignora colisão com a própria armadilha no instante em que sai da boca
            if (ownerTrap != null && (collision.gameObject == ownerTrap.gameObject || collision.transform.IsChildOf(ownerTrap.transform)))
            {
                return;
            }

            // 2. Se atingir a toupeira do mesmo plano:
            //    - Se a toupeira estiver na Janela de Parry (punch.IsParrying), reflete o projétil sem causar dano!
            //    - Caso contrário, causa 1 de dano na vida compartilhada (3 HP).
            if (collision.CompareTag("Player"))
            {
                if (collision.TryGetComponent<PlayerPunch>(out PlayerPunch punch))
                {
                    if (punch.planoDestePersonagem == this.planoDoProjetil)
                    {
                        if (punch.IsParrying)
                        {
                            Reflect();
                            return;
                        }

                        PlayerMovement pm = collision.GetComponent<PlayerMovement>();
                        PlayerHealth.EnsureInstance().TakeDamage(1, pm, false);
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }

        // 3. Se atingir um botão/alavanca quando refletido, também ativa
        if (isReflected && collision.TryGetComponent<ShootableButton>(out ShootableButton btn))
        {
            if (btn.planoDesteBotao == this.planoDoProjetil)
            {
                btn.Activate();
                Destroy(gameObject);
                return;
            }
        }

        // 4. Destrói ao bater em paredes/chão sólidos do mesmo plano (ignorando Triggers)
        if (!collision.isTrigger)
        {
            int frenteLayer = LayerMask.NameToLayer("ChaoFrente");
            int fundoLayer = LayerMask.NameToLayer("ChaoFundo");

            if (planoDoProjetil == PlanoDeJogo.Frente && collision.gameObject.layer == frenteLayer)
            {
                Destroy(gameObject);
            }
            else if (planoDoProjetil == PlanoDeJogo.Fundo && collision.gameObject.layer == fundoLayer)
            {
                Destroy(gameObject);
            }
        }
    }
}
