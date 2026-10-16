using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerPunch : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField, Tooltip("Referência para a ação de bater")]
    private InputActionReference attackAction;

    [Header("Configurações do Ataque")]
    [SerializeField, Tooltip("Objeto vazio posicionado na frente do personagem")]
    private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField, Tooltip("Layer que define o que é uma rocha quebrável")]
    private LayerMask rockLayer;

    [Header("Progresso da Habilidade")]
    [Tooltip("Marque como true pelo seu SaveGame após terminar a fase de Fogo")]
    public bool isUnlockedPermanently = false;
    [SerializeField] private string nativeScene = "F-FogoScene";

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.action.performed += OnAttackPerformed;
            attackAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.action.performed -= OnAttackPerformed;
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        // Só ataca se estiver habilitado globalmente ou na fase de origem
        if (!CanUseAbility()) return;

        PerformPunch();
    }

    private bool CanUseAbility()
    {
        if (isUnlockedPermanently) return true;
        if (SceneManager.GetActiveScene().name == nativeScene) return true;
        
        return false;
    }

    private void PerformPunch()
    {
        // Cria um círculo invisível na frente do jogador e coleta tudo que ele tocou na Layer "Rochas"
        Collider2D[] hitRocks = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, rockLayer);

        foreach (Collider2D rock in hitRocks)
        {
            Debug.Log($"Quebrou a rocha: {rock.gameObject.name}");
            
            // Aqui você destrói a rocha. Futuramente pode chamar um script nela para rodar animação/som
            Destroy(rock.gameObject);
        }
    }

    // Desenha o círculo de ataque no Editor para facilitar o ajuste do tamanho
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}