using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FireTrap : MonoBehaviour
{
    [Header("Plano e Identificação")]
    [Tooltip("Em qual plano esta armadilha existe? (Preenchido pelo DefinidorDePlano)")]
    public PlanoDeJogo planoDestaArmadilha;

    [Header("Disparo de Fogo")]
    [Tooltip("Intervalo em segundos entre cada bola de fogo disparada")]
    public float intervaloDeDisparo = 2.2f;

    [Tooltip("Atraso inicial antes do primeiro tiro")]
    public float atrasoInicial = 0.6f;

    [Tooltip("Velocidade base da bola de fogo (ajustada automaticamente na pintura do fundo)")]
    public float velocidadeDoProjetil = 9f;

    [Tooltip("Direção base do disparo quando o sprite não está invertido (o sprite armadilhaReal olha para a esquerda)")]
    public Vector2 direcaoBaseDisparo = Vector2.left;

    [Tooltip("Posição local da boca de onde sai o fogo (ajustado automaticamente pelo tamanho do sprite)")]
    public Vector2 offsetBocaLocal = new Vector2(-2.2f, -0.35f);

    [Tooltip("Opcional: Prefab do projétil de fogo (se deixar vazio, o script cria a bola de fogo automaticamente!)")]
    public GameObject fireProjectilePrefab;

    [Header("Estado Destruído / Plataforma Pisável")]
    [Tooltip("Opcional: Sprite da armadilha fechada (armadilhaReal 1_0). Se vazio, usa o frame final do Animator ArmadilhaUM!")]
    public Sprite spriteBocaFechada;

    [Tooltip("Causar 1 de dano se o jogador encostar diretamente na boca aberta da armadilha ativa")]
    public bool causarDanoAoEncostarAtiva = true;

    private bool isDestroyed = false;
    private float fireTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Animator trapAnimator;
    private BoxCollider2D boxCollider;
    private readonly List<GameObject> activeProjectiles = new List<GameObject>();

    private static Sprite cachedProceduralFireSprite;

    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trapAnimator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Ajusta o BoxCollider2D automaticamente para envolver a cabeça da armadilha
        if (boxCollider != null)
        {
            if (spriteRenderer != null && spriteRenderer.sprite != null && boxCollider.size.sqrMagnitude < 0.1f)
            {
                Vector2 s = spriteRenderer.sprite.bounds.size;
                boxCollider.size = new Vector2(s.x * 0.88f, s.y * 0.75f);
                boxCollider.offset = new Vector2(0f, -s.y * 0.08f);
            }

            // Enquanto está ativa atirando fogo, fica como Trigger para detectar o Soco (PlayerPunch) e contato
            boxCollider.isTrigger = true;
        }

        // Mantém o Animator travado no frame 0 (boca aberta com rubi: armadilhaReal 1_1) sem piscar
        if (trapAnimator != null && trapAnimator.runtimeAnimatorController != null)
        {
            trapAnimator.enabled = true;
            trapAnimator.speed = 0f;
            trapAnimator.Play(0, 0, 0f);
            trapAnimator.Update(0f);
            trapAnimator.enabled = false;
        }

        fireTimer = Mathf.Max(0.1f, intervaloDeDisparo - atrasoInicial);
    }

    private void Update()
    {
        if (isDestroyed) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= intervaloDeDisparo)
        {
            fireTimer = 0f;
            ShootFireball();
        }
    }

    private float GetScaleMultiplier()
    {
        // Se estiver no plano do Fundo (pintura), respeita a proporção de 20% (0.2)
        if (planoDestaArmadilha == PlanoDeJogo.Fundo)
            return 0.2f;

        return Mathf.Clamp(Mathf.Abs(transform.lossyScale.x), 0.2f, 2f);
    }

    private float GetHorizontalDirection()
    {
        float dir = direcaoBaseDisparo.x >= 0f ? 1f : -1f;
        if (spriteRenderer != null && spriteRenderer.flipX)
            dir *= -1f;
        if (transform.lossyScale.x < 0f)
            dir *= -1f;
        return dir;
    }

    private void ShootFireball()
    {
        float dirX = GetHorizontalDirection();
        float scaleMult = GetScaleMultiplier();

        // Calcula o ponto de saída na boca da serpente/armadilha
        float offsetX = Mathf.Abs(offsetBocaLocal.x) * dirX;
        Vector3 spawnPos = transform.TransformPoint(new Vector3(offsetX, offsetBocaLocal.y, 0f));

        GameObject projObj;
        if (fireProjectilePrefab != null)
        {
            projObj = Instantiate(fireProjectilePrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            projObj = CreateProceduralFireball(spawnPos);
        }

        if (!projObj.TryGetComponent<FireTrapProjectile>(out FireTrapProjectile fireProj))
        {
            fireProj = projObj.AddComponent<FireTrapProjectile>();
        }

        int sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 2;
        fireProj.Initialize(this, dirX, velocidadeDoProjetil, scaleMult, planoDestaArmadilha, sortingOrder);
        activeProjectiles.Add(projObj);
    }

    private GameObject CreateProceduralFireball(Vector3 position)
    {
        GameObject go = new GameObject("ProjétilDeFogo_Armadilha");
        go.transform.position = position;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrCreateFireSprite();
        sr.color = new Color(1f, 0.38f, 0.08f, 1f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        return go;
    }

    private static Sprite GetOrCreateFireSprite()
    {
        if (cachedProceduralFireSprite != null) return cachedProceduralFireSprite;

        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float maxRadius = size * 0.48f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float t = Mathf.Clamp01(1f - (dist / maxRadius));
                if (t <= 0f)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else
                {
                    // Núcleo amarelo-esbranquiçado brilhante e borda vermelho-fogo
                    Color core = new Color(1f, 0.95f, 0.4f, 1f);
                    Color edge = new Color(1f, 0.2f, 0.05f, 0f);
                    Color c = Color.Lerp(edge, core, t * t);
                    c.a = Mathf.Clamp01(t * 1.4f);
                    tex.SetPixel(x, y, c);
                }
            }
        }
        tex.Apply();

        cachedProceduralFireSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        return cachedProceduralFireSprite;
    }

    /// <summary>
    /// Chamado quando o jogador acerta a armadilha com o Soco (PlayerPunch) ou reflete um projétil nela.
    /// Fecha a boca da armadilha (mudando o sprite) e a transforma em uma plataforma sólida pisável!
    /// </summary>
    public void DeactivateTrap()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        Debug.Log($"[ARMADILHA] '{gameObject.name}' ({planoDestaArmadilha}) foi destruída/fechada e virou uma plataforma pisável!");

        // Limpa bolas de fogo ativas desta armadilha
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            if (activeProjectiles[i] != null)
            {
                Destroy(activeProjectiles[i]);
            }
        }
        activeProjectiles.Clear();

        // 1. Muda para o sprite de boca fechada (armadilhaReal 1_0)
        if (spriteBocaFechada != null && spriteRenderer != null)
        {
            if (trapAnimator != null) trapAnimator.enabled = false;
            spriteRenderer.sprite = spriteBocaFechada;
        }
        else if (trapAnimator != null && trapAnimator.runtimeAnimatorController != null)
        {
            // Usa o próprio clipe ArmadilhaUM anexado para pular para o último frame (armadilhaReal 1_0) e congelar!
            trapAnimator.enabled = true;
            trapAnimator.speed = 0f;
            trapAnimator.Play(0, 0, 1f);
            trapAnimator.Update(0f);
            trapAnimator.enabled = false;
        }

        // 2. Transforma em plataforma sólida pisável na Layer de chão do seu respectivo plano
        if (boxCollider != null)
        {
            boxCollider.isTrigger = false;

            // Ajusta a superfície superior do colisor para ficar nivelada com a cabeça fechada da serpente
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Vector2 s = spriteRenderer.sprite.bounds.size;
                boxCollider.size = new Vector2(s.x * 0.86f, s.y * 0.62f);
                boxCollider.offset = new Vector2(0f, -s.y * 0.14f);
            }
        }

        string targetLayerName = (planoDestaArmadilha == PlanoDeJogo.Fundo) ? "ChaoFundo" : "ChaoFrente";
        int groundLayerId = LayerMask.NameToLayer(targetLayerName);
        if (groundLayerId != -1)
        {
            gameObject.layer = groundLayerId;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed || !causarDanoAoEncostarAtiva) return;

        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<PlayerPunch>(out PlayerPunch punch))
            {
                if (punch.planoDestePersonagem == this.planoDestaArmadilha)
                {
                    PlayerMovement pm = collision.GetComponent<PlayerMovement>();
                    PlayerHealth.EnsureInstance().TakeDamage(1, pm, false);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        float dirX = spriteRenderer != null && spriteRenderer.flipX ? 1f : (direcaoBaseDisparo.x >= 0f ? 1f : -1f);
        Vector3 spawnPos = transform.TransformPoint(new Vector3(Mathf.Abs(offsetBocaLocal.x) * dirX, offsetBocaLocal.y, 0f));
        Gizmos.color = Color.Lerp(Color.red, Color.yellow, 0.5f);
        Gizmos.DrawWireSphere(spawnPos, 0.35f);
        Gizmos.DrawLine(spawnPos, spawnPos + Vector3.right * (dirX * 2f));
    }
}
