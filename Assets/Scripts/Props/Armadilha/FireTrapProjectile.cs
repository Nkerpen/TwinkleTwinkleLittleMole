using UnityEngine;

public class FireTrapProjectile : MonoBehaviour
{
    [Header("Configurações do Projétil de Fogo")]
    public float lifeTime = 5f;
    public PlanoDeJogo planoDoProjetil;

    private FireTrap ownerTrap;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
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

        Destroy(gameObject, lifeTime);
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
            sr.color = new Color(0.2f, 0.95f, 1f, 1f); // Fica azul-ciano brilhante ao ser refletido
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