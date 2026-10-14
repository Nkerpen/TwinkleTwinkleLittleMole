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
        // 1º Ponto de checagem: Algo encostou no portal?
        Debug.Log($"[PORTAL] Objeto encostou no portal: {collision.gameObject.name} | Tag dele: {collision.tag}");

        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("[PORTAL] Jogador reconhecido! Pode apertar o botão de interação.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("[PORTAL] Jogador saiu do portal.");
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // 2º Ponto de checagem: O jogo detectou o aperto do botão?
        Debug.Log("[PORTAL] O botão de Interagir foi pressionado no teclado/controle!");

        if (isPlayerInRange && !isTransitioning)
        {
            Debug.Log("[PORTAL] Condições perfeitas! Iniciando transição de cena...");
            StartCoroutine(TransitionToScene());
        }
        else if (!isPlayerInRange)
        {
            Debug.Log("[PORTAL] Botão pressionado, mas o jogador NÃO está dentro do portal.");
        }
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;

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

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        Destroy(canvasObject);
    }
}