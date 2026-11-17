using System.Collections;
using UnityEngine;

public class HUDBannerTemporario : MonoBehaviour
{
    [Header("Configuração do Banner Superior (ex: CovilDeLogi)")]
    [SerializeField, Tooltip("Tempo em segundos que o banner fica parado visível no início da fase")]
    private float tempoVisivel = 3.0f;

    [SerializeField, Tooltip("Duração da animação subindo até sair da tela")]
    private float duracaoSubida = 0.7f;

    [SerializeField, Tooltip("Distância em pixels que o banner sobe no eixo Y até sumir da câmera")]
    private float deslocamentoSubidaY = 260f;

    [SerializeField, Tooltip("Se marcado, desativa o GameObject ao terminar de subir")]
    private bool desativarAoFinalizar = true;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 posicaoInicial;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (rectTransform != null)
        {
            posicaoInicial = rectTransform.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = posicaoInicial;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        StopAllCoroutines();
        StartCoroutine(RotinaEsconderBanner());
    }

    private IEnumerator RotinaEsconderBanner()
    {
        // 1. Aguarda os primeiros segundos da fase
        yield return new WaitForSeconds(tempoVisivel);

        // 2. Anima subindo suavemente para fora do topo da tela (EaseInBack leve / quadrático)
        Vector2 origem = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        Vector2 destino = origem + new Vector2(0f, deslocamentoSubidaY);

        float elapsed = 0f;
        while (elapsed < duracaoSubida)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duracaoSubida);

            // Curva suave acelerando para cima (EaseInQuad)
            float curva = t * t;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(origem, destino, curva);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - curva;
            }

            yield return null;
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = destino;
        }

        // 3. Desativa o elemento do HUD após sumir
        if (desativarAoFinalizar)
        {
            gameObject.SetActive(false);
        }
    }
}
