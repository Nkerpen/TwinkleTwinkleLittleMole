using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Fogo, Agua, Terra, Vento, ArtefatoFinal }

    [Header("Configuração")]
    [Tooltip("Qual elemento este coletável representa?")]
    public CollectibleType myType;
    public string hubSceneName = "TempleScene";
    
    [HideInInspector] public PlanoDeJogo planoDesteColetavel; // O DefinidorDePlano preenche isso automaticamente!

    [Header("Transição de Vitória")]
    public float fadeDuration = 1.5f;

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;

        if (collision.CompareTag("Player"))
        {
            // Checa se o plano do boneco bate com o do coletável!
            if (collision.TryGetComponent<PlayerPunch>(out PlayerPunch punch))
            {
                if (punch.planoDestePersonagem != planoDesteColetavel) return; // Toupeira de outro plano! Aborta.

                isCollected = true;
                Debug.Log($"Coletou o artefato de {myType}!");

                // Salva no Cérebro do jogo!
                if (GameProgressManager.Instance != null)
                {
                    switch (myType)
                    {
                        case CollectibleType.Fogo: GameProgressManager.Instance.hasFire = true; break;
                        case CollectibleType.Agua: GameProgressManager.Instance.hasWater = true; break;
                        case CollectibleType.Terra: GameProgressManager.Instance.hasEarth = true; break;
                        case CollectibleType.Vento: GameProgressManager.Instance.hasWind = true; break;
                        case CollectibleType.ArtefatoFinal: 
                            GameProgressManager.Instance.hasFinalArtifact = true; 
                            WinGame();
                            return;
                    }
                }

                StartCoroutine(ReturnToHub());
            }
        }
    }

    private IEnumerator ReturnToHub()
    {
        // Protege o Coletável de ser destruído no loading
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // Deixa ele invisível no Templo
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
        fadeImage.color = new Color(1f, 1f, 1f, 0f);

        // Estica o Fade Branco
        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        canvasObject.transform.SetParent(transform);

        // Fade Out (Clareia pra Branco)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }

        // Carrega Fase do Templo
        yield return SceneManager.LoadSceneAsync(hubSceneName);

        // Fade In (Volta a enxergar)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }

        Destroy(gameObject);
    }

    private void WinGame()
    {
        Debug.Log("VOCÊ VENCEU A ALPHA!");
    }
}