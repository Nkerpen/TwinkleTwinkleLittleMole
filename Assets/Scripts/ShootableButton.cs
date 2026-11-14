using UnityEngine;
using UnityEngine.Events;

public class ShootableButton : MonoBehaviour
{
    [Header("Configuração do Botão")]
    [Tooltip("Em qual plano este botão existe?")]
    public PlanoDeJogo planoDesteBotao;

    [Tooltip("Coloque aqui o que deve acontecer quando o tiro acertar o botão")]
    public UnityEvent onActivate;

    // Trava para não ativar o botão várias vezes seguidas
    private bool isActivated = false;

    public void Activate()
    {
        if (isActivated) return; 
        
        isActivated = true;
        Debug.Log($"[{gameObject.name}] O botão foi ATIVADO pelo tiro da {planoDesteBotao}!");

        // Dispara todos os eventos que você arrastar lá no Inspector!
        onActivate.Invoke();

        // Feedback visual rápido para o seu teste (Muda o botão para verde)
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
        {
            sr.color = Color.green;
        }
    }
}