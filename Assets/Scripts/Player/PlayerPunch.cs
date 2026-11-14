            using UnityEngine;
            using UnityEngine.InputSystem;
            using UnityEngine.SceneManagement;



            public class PlayerPunch : MonoBehaviour
            {
                [Header("Input System")]
                [SerializeField, Tooltip("Referência para a ação de bater (Ex: Botão X ou Tecla J)")]
                private InputActionReference attackAction;
                [Header("Configurações do Ataque")]
                [SerializeField, Tooltip("Objeto vazio posicionado na frente do personagem")]

                private Transform attackPoint;
                [SerializeField, Tooltip("Tamanho da área de impacto do soco")]

                private float attackRadius = 0.5f;
                [Tooltip("Defina se ESTE personagem é o da Frente ou do Fundo")]

                public PlanoDeJogo planoDestePersonagem;
                [Header("Progresso da Habilidade")]
                [Tooltip("Marque como true pelo seu SaveGame após terminar a fase de Fogo")]

                public bool isUnlockedPermanently = false;
                [SerializeField, Tooltip("Nome da cena onde a habilidade é nativa")]

                private string nativeScene = "F-FogoScene";
                // Referência para checar se o personagem atual está sendo controlado

                private PlayerMovement playerMovement;

                private void Awake()
                {
                    playerMovement = GetComponent<PlayerMovement>();
                }

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
                    // Só ataca se o script de movimento deste personagem estiver ATIVO (controlado pelo player)
                    if (playerMovement != null && !playerMovement.enabled) return;

                    // Só ataca se a habilidade estiver liberada na fase ou permanentemente
                    if (!CanUseAbility()) return;
                    PerformPunch();
                }


    private bool CanUseAbility()

        {
            if (isUnlockedPermanently) return true; // Sua trava de debug do Inspector
            if (GameProgressManager.Instance != null && GameProgressManager.Instance.hasFire) return true; // Destravado globalmente!
            if (SceneManager.GetActiveScene().name == nativeScene) return true;
            return false;
        }


            private void PerformPunch()
                {
                    // Adapta o tamanho do soco baseado na escala real do personagem
                    float scaledRadius = attackRadius * Mathf.Abs(transform.localScale.x);
                    Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, scaledRadius);

                    foreach (Collider2D hit in hitObjects)
                    {
                        if (hit.TryGetComponent<RochaQuebravel>(out RochaQuebravel rock))
                        {
                            if (rock.planoDestaRocha == this.planoDestePersonagem)
                            {
                                rock.Break();
                            }
                        }
                    }
                }

                private void OnDrawGizmosSelected()
                {
                    if (attackPoint != null)
                    {
                        Gizmos.color = Color.red;
                        // Adapta o desenho vermelho no Editor para você ver o tamanho correto também!
                        float scaledRadius = attackRadius * Mathf.Abs(transform.localScale.x);
                        Gizmos.DrawWireSphere(attackPoint.position, scaledRadius);
                    }
                }

            }
            
            