using TMPro;
using UnityEngine;

public class HUDColetaveis : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoContador;

    private void Awake()
    {
        if (textoContador == null)
            textoContador = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    // Chame este metodo quando a fase carregar e quando o jogador coletar um cristal.
    // Por exemplo: hudColetaveis.Atualizar(quantidadeColetada, totalDaFase);
    public void Atualizar(int quantidadeColetada, int totalDaFase)
    {
        if (textoContador == null)
        {
            Debug.LogWarning("HUDColetaveis: vincule um texto TextMeshProUGUI no Inspector.", this);
            return;
        }

        int total = Mathf.Max(0, totalDaFase);
        int atual = Mathf.Clamp(quantidadeColetada, 0, total);
        textoContador.text = $"{atual}/{total}";
    }
}
