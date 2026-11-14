using UnityEngine;

public class TempleManager : MonoBehaviour
{
    [Tooltip("O objeto que bloqueia o final (Parede, Portal desativado, etc)")]
    public GameObject finalBarrier;

    private void Start()
    {
        if (GameProgressManager.Instance != null)
        {
            int cristais = GameProgressManager.Instance.GetCollectedCount();
            Debug.Log($"O jogador retornou ao Templo com {cristais}/4 cristais.");

            // Se coletou todos os 4...
            if (cristais >= 4)
            {
                Debug.Log("Abrindo o caminho final!");
                // Desativa a barreira (ou ativa uma ponte, porta, etc)
                if (finalBarrier != null) finalBarrier.SetActive(false);
            }
        }
    }
}