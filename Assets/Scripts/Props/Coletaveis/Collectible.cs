using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class Collectible : MonoBehaviour
{
    public enum CollectibleType { DiamanteDaFase, Fogo, Agua, Terra, Vento, ArtefatoFinal }

    [Header("Configuração do Coletável")]
    [Tooltip("Tipo do coletável (DiamanteDaFase atualiza o contador do HUD e desaparece)")]
    public CollectibleType myType = CollectibleType.DiamanteDaFase;

    [Tooltip("Plano deste coletável (preenchido pelo DefinidorDePlano ou detectado automaticamente pela escala)")]
    public PlanoDeJogo planoDesteColetavel = PlanoDeJogo.Frente;

    // --- CONTROLE ESTÁTICO DOS COLETÁVEIS DA CENA E CONEXÃO COM HUDColetaveis ---
    private static readonly List<Collectible> coletaveisDaCena = new List<Collectible>();
    private static int totalColetaveisNaCena = 0;
    private static int quantidadeColetadaNaCena = 0;
    private static HUDColetaveis hudColetaveis;
    private static int cenaAtualIndex = -1;
    private static bool autoScanExecutadoNestaCena = false;

    private bool isCollected = false;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (cenaAtualIndex != activeSceneIndex)
        {
            cenaAtualIndex = activeSceneIndex;
            coletaveisDaCena.Clear();
            totalColetaveisNaCena = 0;
            quantidadeColetadaNaCena = 0;
            hudColetaveis = null;
            autoScanExecutadoNestaCena = false;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
            if (spriteRenderer != null && spriteRenderer.sprite != null && boxCollider.size.sqrMagnitude < 0.01f)
            {
                boxCollider.size = spriteRenderer.sprite.bounds.size;
            }
        }

        // Se o diamante estiver solto na raiz da cena com escala pequena (~0.2), define como Fundo automaticamente
        if (transform.parent == null)
        {
            if (Mathf.Abs(transform.lossyScale.x) < 0.35f)
            {
                planoDesteColetavel = PlanoDeJogo.Fundo;
                int layerFundo = LayerMask.NameToLayer("ChaoFundo");
                if (layerFundo != -1) gameObject.layer = layerFundo;
            }
            else
            {
                planoDesteColetavel = PlanoDeJogo.Frente;
                int layerFrente = LayerMask.NameToLayer("ChaoFrente");
                if (layerFrente != -1) gameObject.layer = layerFrente;
            }
        }

        if (!coletaveisDaCena.Contains(this))
        {
            coletaveisDaCena.Add(this);
            totalColetaveisNaCena = coletaveisDaCena.Count;
        }
    }

    private void Start()
    {
        if (!autoScanExecutadoNestaCena)
        {
            autoScanExecutadoNestaCena = true;
            ConfigurarDiamantesDaCenaAutomaticamente();
        }

        AtualizarHUD();
    }

    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded)
        {
            coletaveisDaCena.Clear();
            totalColetaveisNaCena = 0;
            quantidadeColetadaNaCena = 0;
            hudColetaveis = null;
            autoScanExecutadoNestaCena = false;
        }
    }

    public static void ConfigurarDiamantesDaCenaAutomaticamente()
    {
        SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (SpriteRenderer sr in allRenderers)
        {
            if (!sr.gameObject.scene.IsValid()) continue;

            string objName = sr.gameObject.name;
            if (objName.StartsWith("diamante", System.StringComparison.OrdinalIgnoreCase))
            {
                if (!sr.TryGetComponent<Collectible>(out Collectible col))
                {
                    col = sr.gameObject.AddComponent<Collectible>();
                    col.myType = CollectibleType.DiamanteDaFase;
                }
            }
        }

        coletaveisDaCena.RemoveAll(item => item == null);
        totalColetaveisNaCena = coletaveisDaCena.Count;
        AtualizarHUD();
    }

    private static void AtualizarHUD()
    {
        if (hudColetaveis == null)
        {
            hudColetaveis = FindFirstObjectByType<HUDColetaveis>(FindObjectsInactive.Include);
        }

        if (hudColetaveis != null)
        {
            hudColetaveis.Atualizar(quantidadeColetadaNaCena, totalColetaveisNaCena);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected || collision == null) return;

        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<PlayerPunch>(out PlayerPunch punch))
            {
                // Verifica se a toupeira está no mesmo plano deste coletável (Frente ou Fundo)
                if (punch.planoDestePersonagem != planoDesteColetavel) return;

                isCollected = true;
                quantidadeColetadaNaCena++;

                Debug.Log($"[COLETÁVEL] Diamante coletado ({quantidadeColetadaNaCena}/{totalColetaveisNaCena}) no plano {planoDesteColetavel}!");

                // 1. Atualiza o contador no HUD (ex: 1/8, 2/8 ... 8/8)
                AtualizarHUD();

                // 2. Faz o coletável desaparecer imediatamente ao ser coletado
                gameObject.SetActive(false);
            }
        }
    }
}