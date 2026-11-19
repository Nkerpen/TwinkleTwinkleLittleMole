using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Cenas (Altere aqui no Inspector na continuação do projeto)")]
    [SerializeField, Tooltip("Cena que abre ao clicar em Novo Jogo (Demo: F-Fogo2)")]
    private string cenaNovoJogo = "F-Fogo2";

    [SerializeField, Tooltip("Cena que abre ao clicar em Continuar (Demo: F-Fogo2)")]
    private string cenaContinuar = "F-Fogo2";

    [Header("Foco do Controle (EventSystem)")]
    [SerializeField, Tooltip("Arraste o botão 'Novo Jogo' para cá para o controle nunca perder a seleção caso você clique fora com o mouse")]
    private Button primeiroBotao;

    [Header("Transição de Tela (Igual ao Portal)")]
    [SerializeField, Tooltip("Duração do Fade Out e do Fade In (em segundos)")]
    private float fadeDuration = 1f;

    [SerializeField, Tooltip("Cor da cortina de transição")]
    private Color fadeColor = Color.black;

    private GameObject ultimoSelecionado;
    private bool isTransitioning = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (primeiroBotao != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(primeiroBotao.gameObject);
            ultimoSelecionado = primeiroBotao.gameObject;
        }
    }

    private void Update()
    {
        if (isTransitioning || EventSystem.current == null) return;

        // Se o jogador clicar no vazio com o mouse, a Unity tira a seleção do botão e o controle para de responder.
        // Este bloco devolve a seleção automaticamente para o último botão focado!
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            ultimoSelecionado = EventSystem.current.currentSelectedGameObject;
        }
        else if (ultimoSelecionado != null)
        {
            EventSystem.current.SetSelectedGameObject(ultimoSelecionado);
        }
        else if (primeiroBotao != null)
        {
            EventSystem.current.SetSelectedGameObject(primeiroBotao.gameObject);
        }
    }

    // --- FUNÇÕES PÚBLICAS PARA O 'ON CLICK ()' DO INSPECTOR DA UNITY ---

    public void IniciarNovoJogo()
    {
        if (isTransitioning) return;

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.hasFire = false;
            GameProgressManager.Instance.hasWater = false;
            GameProgressManager.Instance.hasEarth = false;
            GameProgressManager.Instance.hasWind = false;
            GameProgressManager.Instance.hasFinalArtifact = false;
        }

        StartCoroutine(TransitionToScene(cenaNovoJogo));
    }

    public void ContinuarJogo()
    {
        if (isTransitioning) return;

        StartCoroutine(TransitionToScene(cenaContinuar));
    }

    public void SairDoJogo()
    {
        if (isTransitioning) return;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// Executa exatamente a mesma transição de Fade Out -> LoadSceneAsync -> Fade In dos Portais!
    /// </summary>
    private IEnumerator TransitionToScene(string sceneToLoad)
    {
        isTransitioning = true;

        // Cria o Canvas de transição independente (para o Menu poder ser destruído normalmente ao carregar a fase)
        GameObject canvasObject = new GameObject("TransitionCanvas");
        DontDestroyOnLoad(canvasObject);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);
        Image fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        // Estica o Fade Preto na tela inteira
        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        // Delegate o ciclo completo de Fade Out + LoadSceneAsync + Fade In para o próprio TransitionCanvas
        MenuTransitionRunner runner = canvasObject.AddComponent<MenuTransitionRunner>();
        runner.Iniciar(sceneToLoad, fadeImage, fadeDuration, fadeColor);

        yield break;
    }
}

/// <summary>
/// Componente auxiliar que vive apenas no TransitionCanvas durante a troca de cena,
/// garantindo que o Fade Out e o Fade In terminem suavemente dentro da nova fase antes de se autodestruir.
/// </summary>
public class MenuTransitionRunner : MonoBehaviour
{
    public void Iniciar(string sceneToLoad, Image fadeImage, float fadeDuration, Color fadeColor)
    {
        StartCoroutine(ExecutarTransicao(sceneToLoad, fadeImage, fadeDuration, fadeColor));
    }

    private IEnumerator ExecutarTransicao(string sceneToLoad, Image fadeImage, float fadeDuration, Color fadeColor)
    {
        // 1. Fade Out (Escurece a tela no Menu)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        // 2. Carrega a fase de forma assíncrona (igual ao Portal.cs)
        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        // 3. Fade In (Clareia a tela já dentro da fase carregada)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
