using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class ShootableButton : MonoBehaviour
{
    public enum TipoGatilho
    {
        BotaoDePisar,    // Ativado apenas ao pisar/encostar com a toupeira do mesmo plano
        AlavancaDeSoco,  // Ativado ao bater com o Soco/Arranhão (PlayerPunch)
        Ambos            // Opcional: aceita pisar ou bater
    }

    [Header("Configuração do Botão / Alavanca")]
    [Tooltip("Em qual plano este botão/alavanca existe? (Preenchido automaticamente pelo DefinidorDePlano)")]
    public PlanoDeJogo planoDesteBotao;

    [Tooltip("Botões de chão usam 'BotaoDePisar'. Alavancas usam 'AlavancaDeSoco'.")]
    public TipoGatilho tipoDeGatilho = TipoGatilho.BotaoDePisar;

    [Tooltip("Se marcado, permite ligar e desligar repetidamente ao acionar")]
    public bool permiteAlternar = false;

    [Header("Feedback Visual (Sprite ou Animator)")]
    [Tooltip("Opcional: Sprite do botão/alavanca ativado (se vazio, usa o Animator anexado ao objeto)")]
    public Sprite spriteAtivado;

    [Tooltip("Se não houver Sprite Ativado nem Animator, aplica esta cor como confirmação visual")]
    public Color corQuandoAtivado = Color.white;

    [Header("Puzzles e Blocos Conectados")]
    [Tooltip("Arraste aqui os objetos com Animator que devem tocar sua animação ao ativar (ex: EscadadePilares, puzzlePilares---, blocos_mundo_real, blocosPintura)")]
    public List<Animator> animadoresParaAcionar = new List<Animator>();

    [Tooltip("Arraste aqui barreiras, pilares ou blocos que devem SUMIR/ABRIR ao acionar este gatilho")]
    public List<GameObject> objetosParaDesativar = new List<GameObject>();

    [Tooltip("Arraste aqui plataformas, pontes ou blocos que devem APARECER ao acionar este gatilho")]
    public List<GameObject> objetosParaAtivar = new List<GameObject>();

    [Tooltip("Eventos extras opcionais configuráveis pelo Inspector")]
    public UnityEvent onActivate = new UnityEvent();

    private bool isActivated = false;
    private float lastToggleTime = -10f;
    private SpriteRenderer spriteRenderer;
    private Sprite spriteInicial;
    private Animator buttonAnimator;
    private BoxCollider2D boxCollider;

    public bool IsActivated => isActivated;
    public bool AceitaSoco => tipoDeGatilho == TipoGatilho.AlavancaDeSoco || tipoDeGatilho == TipoGatilho.Ambos;
    public bool AceitaPisar => tipoDeGatilho == TipoGatilho.BotaoDePisar || tipoDeGatilho == TipoGatilho.Ambos;

    private void Awake()
    {
        animadoresParaAcionar ??= new List<Animator>();
        objetosParaDesativar ??= new List<GameObject>();
        objetosParaAtivar ??= new List<GameObject>();
        onActivate ??= new UnityEvent();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteInicial = spriteRenderer.sprite;
        }

        buttonAnimator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
            if (spriteRenderer != null && spriteRenderer.sprite != null && boxCollider.size.sqrMagnitude < 0.01f)
            {
                boxCollider.size = spriteRenderer.sprite.bounds.size;
            }
        }

        PausarAnimatorNoFrameInicial(buttonAnimator);
    }

    private void Start()
    {
        foreach (Animator targetAnim in animadoresParaAcionar)
        {
            PausarAnimatorNoFrameInicial(targetAnim);
        }
    }

    public void RegistrarPuzzleAlvo(Animator puzzleAnimator)
    {
        animadoresParaAcionar ??= new List<Animator>();
        if (puzzleAnimator != null && !animadoresParaAcionar.Contains(puzzleAnimator))
        {
            animadoresParaAcionar.Add(puzzleAnimator);
            PausarAnimatorNoFrameInicial(puzzleAnimator);
        }
    }

    private void PausarAnimatorNoFrameInicial(Animator anim)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return;

        anim.enabled = true;
        anim.speed = 0f;
        anim.Play(0, 0, 0f);
        anim.Update(0f);
    }

    private void AvancarAnimatorParaEstadoFinal(Animator anim, float playbackSpeed = 1f)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return;

        anim.enabled = true;
        anim.speed = playbackSpeed;
        anim.Play(0, 0, 0f);
        anim.Update(0f);
    }

    public void Activate()
    {
        if (Time.time - lastToggleTime < 0.25f) return;
        if (isActivated && !permiteAlternar) return;

        lastToggleTime = Time.time;
        isActivated = !isActivated;

        Debug.Log($"[{gameObject.name}] ({tipoDeGatilho}) {(isActivated ? "ATIVADO" : "DESATIVADO")} no plano {planoDesteBotao}!");

        // 1. Atualiza o visual do Botão / Alavanca
        if (isActivated)
        {
            if (spriteAtivado != null && spriteRenderer != null)
            {
                if (buttonAnimator != null) buttonAnimator.enabled = false;
                spriteRenderer.sprite = spriteAtivado;
            }
            else if (buttonAnimator != null && buttonAnimator.runtimeAnimatorController != null)
            {
                AvancarAnimatorParaEstadoFinal(buttonAnimator, 1f);
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.color = corQuandoAtivado != Color.white ? corQuandoAtivado : Color.cyan;
            }
        }
        else
        {
            if (spriteInicial != null && spriteRenderer != null)
            {
                PausarAnimatorNoFrameInicial(buttonAnimator);
                spriteRenderer.sprite = spriteInicial;
            }
        }

        // 2. Aciona os Animators dos pilares/blocos conectados
        if (animadoresParaAcionar != null)
        {
            foreach (Animator targetAnim in animadoresParaAcionar)
            {
                if (targetAnim == null) continue;
                if (isActivated)
                {
                    AvancarAnimatorParaEstadoFinal(targetAnim, 0.15f);
                }
                else
                {
                    PausarAnimatorNoFrameInicial(targetAnim);
                }
            }
        }

        // 3. Desativa barreiras/pilares configurados
        if (objetosParaDesativar != null)
        {
            foreach (GameObject obj in objetosParaDesativar)
            {
                if (obj != null) obj.SetActive(!isActivated);
            }
        }

        // 4. Ativa plataformas/objetos configurados
        if (objetosParaAtivar != null)
        {
            foreach (GameObject obj in objetosParaAtivar)
            {
                if (obj != null) obj.SetActive(isActivated);
            }
        }

        // 5. Dispara eventos extras do Inspector
        if (isActivated)
        {
            onActivate?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!AceitaPisar) return;
        TentarAtivarPorPisar(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!AceitaPisar) return;
        TentarAtivarPorPisar(collision.collider);
    }

    private void TentarAtivarPorPisar(Collider2D collision)
    {
        if (collision == null || !collision.CompareTag("Player")) return;

        if (collision.TryGetComponent<PlayerPunch>(out PlayerPunch punch))
        {
            if (punch.planoDestePersonagem == this.planoDesteBotao)
            {
                Activate();
            }
        }
    }
}