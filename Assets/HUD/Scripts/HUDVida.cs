using UnityEngine;
using UnityEngine.UI;

public class HUDVida : MonoBehaviour
{
    [SerializeField] private Image imagemVida;
    // Arraste os sprites Vida_0, Vida_1, ... Vida_6 nesta ordem.
    [SerializeField] private Sprite[] estadosPorMeioCoracao = new Sprite[7];

    [Range(0, 6)] [SerializeField] private int vidaInicial = 6;

    private void Awake()
    {
        if (imagemVida == null)
            imagemVida = GetComponent<Image>();
        Atualizar(vidaInicial);
    }

    // 0 = vazio, 1 = meio coracao, ... 6 = tres coracoes completos.
    // Chame este metodo quando o sistema de vida mudar:
    // hudVida.Atualizar(vidaAtualEmMeiosCoracoes);
    public void Atualizar(int vidaAtualEmMeiosCoracoes)
    {
        if (imagemVida == null || estadosPorMeioCoracao == null || estadosPorMeioCoracao.Length < 7)
        {
            Debug.LogWarning("HUDVida: configure imagemVida e os 7 sprites.", this);
            return;
        }
        int indice = Mathf.Clamp(vidaAtualEmMeiosCoracoes, 0, 6);
        if (estadosPorMeioCoracao[indice] != null)
            imagemVida.sprite = estadosPorMeioCoracao[indice];
    }
}
