using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla o Hit Effect via Shader (TTLM_HitEffect2D), substituindo o antigo pisca-pisca de alpha.
/// Replica o efeito suave com curva OutExpo do tutorial sem depender de plugins externos (como DOTween),
/// e suporta múltiplos SpriteRenderers filhos (PlayerSprite / touperaFundoSprite) interrompendo e reiniciando limpo a cada hit.
/// </summary>
public class PlayerHitEffect : MonoBehaviour
{
    [Header("Configurações do Flash de Dano (Shader)")]
    [SerializeField, Tooltip("Duração de subida e descida de cada flash (em segundos)")]
    private float flashDuration = 0.14f;

    [SerializeField, Tooltip("Quantos pulsos de flash acontecem durante a invencibilidade (1 = flash único clássico; 2-3 = pulsos suaves)")]
    private int quantidadeDePulsos = 2;

    [SerializeField, Tooltip("Se marcado, usa uma cor HDR customizada no shader ao levar dano")]
    private bool sobrescreverCorHDR = false;

    [ColorUsage(true, true)]
    [SerializeField, Tooltip("Cor HDR do flash (valores > 1 ativam o Bloom do Global Volume)")]
    private Color corDoFlashHDR = new Color(2.5f, 2.5f, 2.5f, 1f);

    private static readonly int HitEffectAmountId = Shader.PropertyToID("_HitEffectAmount");
    private static readonly int HitEffectColorId = Shader.PropertyToID("_HitEffectColor");

    private readonly List<Material> targetMaterials = new List<Material>();
    private Coroutine activeFlashRoutine;

    public bool HasHitEffectShader => targetMaterials.Count > 0;

    private void Awake()
    {
        ColetarMateriaisDosSprites();
    }

    /// <summary>
    /// Encontra todos os SpriteRenderers no objeto e nos filhos (ex: PlayerSprite e touperaFundoSprite)
    /// que possuam o shader com a propriedade _HitEffectAmount.
    /// </summary>
    public void ColetarMateriaisDosSprites()
    {
        targetMaterials.Clear();

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in renderers)
        {
            if (sr == null) continue;

            // Acessar sr.material cria uma instância segura apenas para este personagem
            Material mat = sr.material;
            if (mat != null && mat.HasProperty(HitEffectAmountId))
            {
                if (!targetMaterials.Contains(mat))
                {
                    targetMaterials.Add(mat);
                }
            }
        }

        SetHitAmount(0f);
    }

    /// <summary>
    /// Dispara o efeito de Hit Effect no shader (interrompe e reinicia imediatamente se já estiver rodando).
    /// </summary>
    public void TriggerHitFlash()
    {
        if (targetMaterials.Count == 0)
        {
            ColetarMateriaisDosSprites();
        }

        if (targetMaterials.Count == 0 || !gameObject.activeInHierarchy) return;

        if (sobrescreverCorHDR)
        {
            for (int i = 0; i < targetMaterials.Count; i++)
            {
                if (targetMaterials[i] != null && targetMaterials[i].HasProperty(HitEffectColorId))
                {
                    targetMaterials[i].SetColor(HitEffectColorId, corDoFlashHDR);
                }
            }
        }

        if (activeFlashRoutine != null)
        {
            StopCoroutine(activeFlashRoutine);
        }

        activeFlashRoutine = StartCoroutine(HitFlashRoutine());
    }

    public void ResetarVisual()
    {
        if (activeFlashRoutine != null)
        {
            StopCoroutine(activeFlashRoutine);
            activeFlashRoutine = null;
        }
        SetHitAmount(0f);
    }

    private IEnumerator HitFlashRoutine()
    {
        int pulsos = Mathf.Max(1, quantidadeDePulsos);
        float duracao = Mathf.Max(0.03f, flashDuration);

        for (int p = 0; p < pulsos; p++)
        {
            // 1. Sobe rapidamente até 1.0 usando curva Ease Out Expo (impacto imediato igual ao DOTween do vídeo!)
            float elapsed = 0f;
            while (elapsed < duracao)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duracao);
                float eased = EaseOutExpo(t);
                SetHitAmount(eased);
                yield return null;
            }
            SetHitAmount(1f);

            // 2. Desce suavemente de volta para 0.0
            elapsed = 0f;
            while (elapsed < duracao)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duracao);
                float eased = 1f - EaseOutExpo(t);
                SetHitAmount(eased);
                yield return null;
            }
            SetHitAmount(0f);
        }

        activeFlashRoutine = null;
    }

    private void SetHitAmount(float value)
    {
        for (int i = 0; i < targetMaterials.Count; i++)
        {
            if (targetMaterials[i] != null)
            {
                targetMaterials[i].SetFloat(HitEffectAmountId, value);
            }
        }
    }

    /// <summary>
    /// Curva matemática Ease Out Expo idêntica à usada no vídeo (DOTween Ease.OutExpo):
    /// atinge o brilho máximo quase instantaneamente e desacelera suavemente.
    /// </summary>
    private static float EaseOutExpo(float x)
    {
        return x >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * x);
    }

    private void OnDisable()
    {
        ResetarVisual();
    }
}