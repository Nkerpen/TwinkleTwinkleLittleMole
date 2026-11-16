using UnityEngine;

public class WindGust : MonoBehaviour
{
    [Header("Configuração do Vento")]
    [Tooltip("Em qual plano este vento atua?")]
    public PlanoDeJogo planoDesteVento;
    
    [Tooltip("Força e direção do vento. Ex: (X: 10, Y: 0) empurra para a direita. (X: 0, Y: 5) empurra para cima.")]
    public Vector2 windVelocity = new Vector2(0f, 5f);

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Se quem entrou no vento tem o planador...
        if (collision.TryGetComponent<PlayerGlide>(out PlayerGlide glider))
        {
            // ...e se os crachás forem iguais (Frente com Frente, Fundo com Fundo)
            if (glider.planoDestePersonagem == planoDesteVento)
            {
                glider.ApplyWind(windVelocity);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerGlide>(out PlayerGlide glider))
        {
            if (glider.planoDestePersonagem == planoDesteVento)
            {
                glider.RemoveWind();
            }
        }
    }
}