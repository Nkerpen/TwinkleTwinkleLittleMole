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
    
    [Header("Transição de Vitória")]
    public float fadeDuration = 1.5f;

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;

        if (collision.CompareTag("Player"))
        {
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

    private IEnumerator ReturnToHub()
    {
        // 1. Tira o coletável de qualquer grupo e o protege da destruição durante a mudança de cena
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // 2. Esconde o coletável e desativa sua colisão para não virar um fantasma invisível no Hub
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) sr.enabled = false;
        if (TryGetComponent<Collider2D>(out Collider2D col)) col.enabled = false;

        // 3. Cria a tela de transição
        GameObject canvasObject = new GameObject("TransitionCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);
        Image fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = new Color(1f, 1f, 1f, 0f); // Fade branco!

        // 4. Estica a imagem branca para cobrir 100% do monitor do jogador
        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        // 5. O Canvas vira filho do Coletável sobrevivente
        canvasObject.transform.SetParent(transform);

        // --- COMEÇA O FADE BRANCO ---
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }

        // --- CARREGA O HUB (A antiga cena morre aqui, mas o coletável com a Corotina sobreviveu) ---
        yield return SceneManager.LoadSceneAsync(hubSceneName);

        // --- CLAREIA A TELA NOVAMENTE ---
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01(timer / fadeDuration));
            yield return null;
        }

        // Ao se destruir, o Coletável leva o Canvas com a tela branca junto com ele!
        Destroy(gameObject);
    }

    private void WinGame()
    {
        Debug.Log("VOCÊ VENCEU A ALPHA!");
    }
}