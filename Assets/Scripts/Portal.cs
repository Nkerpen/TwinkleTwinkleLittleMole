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
    [Tooltip("Nome exato da cena que este portal vai carregar")]
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
            // Ouve o evento de pressionar o botão
            interactAction.action.performed += OnInteractPerformed;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Jogador entrou na área do portal
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Jogador saiu da área do portal
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // Se o jogador estiver na área e não estivermos no meio de um carregamento, inicia a transição
        if (isPlayerInRange && !isTransitioning)
        {
            StartCoroutine(TransitionToScene());
        }
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;

        // 1. Cria um Canvas e uma Imagem preta dinamicamente para o Fade
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

        DontDestroyOnLoad(canvasObject);

        // 2. Faz o Fade Out
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        // 3. Carrega a nova cena
        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        // 4. Faz o Fade In
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        // 5. Destrói o Canvas
        Destroy(canvasObject);
    }
}