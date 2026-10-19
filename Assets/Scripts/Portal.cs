using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Portal : MonoBehaviour
{
    [Header("Input System")]
    [Tooltip("Referência para a ação de interagir (ex: Tecla 'E' ou Botão do Controle)")]
    public InputActionReference interactAction;

    [Header("Configurações do Portal")]
    public string sceneToLoad;
    
    [HideInInspector] public PlanoDeJogo planoDestePortal; // O DefinidorDePlano preenche isso automaticamente!

    [Header("Transição")]
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;

    private bool isTransitioning = false;
    private bool isPlayerInRange = false;

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Pede o "crachá" da Toupeira (usamos o script de Soco pois as duas têm ele)
            if (collision.TryGetComponent<PlayerPunch>(out PlayerPunch punch))
            {
                if (punch.planoDestePersonagem == planoDestePortal)
                {
                    isPlayerInRange = true;
                    Debug.Log("[PORTAL] Jogador DO PLANO CORRETO reconhecido!");
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<PlayerPunch>(out PlayerPunch punch))
            {
                if (punch.planoDestePersonagem == planoDestePortal)
                {
                    isPlayerInRange = false;
                }
            }
        }
    }

    private void Update()
    {
        // Interação contínua que resolve o bug do analógico/d-pad!
        if (isPlayerInRange && !isTransitioning && interactAction != null)
        {
            float interactValue = interactAction.action.ReadValue<float>();

            if (interactValue > 0.5f)
            {
                StartCoroutine(TransitionToScene());
            }
        }
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;
        
        // Protege o Portal de ser destruído no loading
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // Deixa ele invisível/intocável na nova fase
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) sr.enabled = false;
        if (TryGetComponent<Collider2D>(out Collider2D col)) col.enabled = false;

        // Cria o Canvas
        GameObject canvasObject = new GameObject("TransitionCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);
        Image fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        
        // Estica o Fade Preto
        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        canvasObject.transform.SetParent(transform);

        // Fade Out
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        // Carrega Fase
        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        // Fade In
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}