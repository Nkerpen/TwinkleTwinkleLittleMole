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
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void Update()
    {
        // Se o jogador está na porta e a transição ainda não começou...
        if (isPlayerInRange && !isTransitioning && interactAction != null)
        {
            // Lemos a força atual da ação. 
            // Retorna 1 se o 'E' estiver apertado, ou > 0.5 se o analógico/d-pad estiver apontando para cima.
            float interactValue = interactAction.action.ReadValue<float>();

            // Se a força passou da metade, ativamos o portal!
            if (interactValue > 0.5f)
            {
                Debug.Log("[PORTAL] Interação detectada pelo Update (Teclado ou Controle)!");
                StartCoroutine(TransitionToScene());
            }
        }
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;
        
        // 1. Tira o portal de dentro de qualquer outro objeto (DontDestroyOnLoad exige isso)
        transform.SetParent(null);
        
        // 2. Impede que o Portal morra ao mudar de cena
        DontDestroyOnLoad(gameObject);

        // 3. Desativa o visual e a colisão do portal para ele ficar invisível na nova fase
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) sr.enabled = false;
        if (TryGetComponent<Collider2D>(out Collider2D col)) col.enabled = false;

        // -------------------------
        GameObject canvasObject = new GameObject("TransitionCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);
        Image fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        
        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        // Faz o Canvas ser "filho" do Portal. Assim, quando destruirmos o portal no final, o Canvas some junto.
        canvasObject.transform.SetParent(transform);

        // Faz o Fade Out (Escurece)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        // Carrega a nova cena
        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        // Faz o Fade In (Clareia a nova cena)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        // Destrói o Portal
        Destroy(gameObject);
    }
}