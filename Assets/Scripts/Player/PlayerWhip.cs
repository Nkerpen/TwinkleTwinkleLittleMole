using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LineRenderer))]
public class PlayerWhip : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private InputActionReference whipAction;
    [SerializeField, Tooltip("Mesma ação de andar para embalar o pêndulo")] 
    private InputActionReference moveAction;

    [Header("Identidade")]
    public PlanoDeJogo planoDestePersonagem;

    [Header("Configurações do Chicote")]
    [SerializeField] private Transform whipPoint; 
    [SerializeField] private float whipRange = 6f;
    [SerializeField] private float swingForce = 15f;
    
    [Header("Progresso")]
    public bool isUnlockedPermanently = false;
    [SerializeField] private string nativeScene = "F-TerraScene";

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private DistanceJoint2D distanceJoint;
    private LineRenderer lineRenderer;
    
    private bool isSwinging;
    private Transform currentAnchor;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        
        // CÁLCULO PROPORCIONAL
        // Aplicamos a escala permanentemente APENAS na força física.
        // O tamanho da corda (whipRange) será calculado só na hora de atirar!
        float scaleMultiplier = Mathf.Abs(transform.localScale.x);
        swingForce *= scaleMultiplier;
    }

    private void OnEnable()
    {
        if (whipAction != null)
        {
            whipAction.action.performed += OnWhipStarted;
            whipAction.action.canceled += OnWhipCanceled;
            whipAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (whipAction != null)
        {
            whipAction.action.performed -= OnWhipStarted;
            whipAction.action.canceled -= OnWhipCanceled;
        }
    }

    private void OnWhipStarted(InputAction.CallbackContext context)
    {
        if (playerMovement != null && !playerMovement.enabled) return;
        if (!CanUseAbility()) return;

        AttemptGrapple();
    }

    private void OnWhipCanceled(InputAction.CallbackContext context)
    {
        if (isSwinging)
        {
            StopSwing();
        }
    }

    private void AttemptGrapple()
    {
        float facingDirection = Mathf.Sign(transform.localScale.x);
        
        // MÁGICA CORRIGIDA: Calculamos o tamanho da corda proporcional sem destruir o valor base
        float scaledRange = whipRange * Mathf.Abs(transform.localScale.x); 
        
        Vector2 searchCenter = (Vector2)whipPoint.position + new Vector2(facingDirection * scaledRange * 0.5f, scaledRange * 0.5f);
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(searchCenter, scaledRange);
        
        Transform bestAnchor = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<WhipAnchor>(out WhipAnchor anchor))
            {
                if (anchor.planoDesteAncoradouro == planoDestePersonagem)
                {
                    float dist = Vector2.Distance(whipPoint.position, anchor.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        bestAnchor = anchor.transform;
                    }
                }
            }
        }

        if (bestAnchor != null)
        {
            StartSwing(bestAnchor);
        }
    }

    private void StartSwing(Transform anchor)
    {
        isSwinging = true;
        currentAnchor = anchor;
        
        // PAUSA A CAMINHADA DO PLAYERMOVEMENT! A física assume o controle absoluto.
        playerMovement.canMove = false; 

        distanceJoint = gameObject.AddComponent<DistanceJoint2D>();
        distanceJoint.autoConfigureConnectedAnchor = false;
        distanceJoint.connectedAnchor = currentAnchor.position;
        distanceJoint.anchor = transform.InverseTransformPoint(whipPoint.position); 
        distanceJoint.distance = Vector2.Distance(whipPoint.position, currentAnchor.position);

        lineRenderer.enabled = true;
    }

    private void StopSwing()
    {
        isSwinging = false;
        currentAnchor = null;
        
        // DEVOLVE O CONTROLE PARA O JOGADOR, PRESERVANDO TODO O IMPULSO DO BALANÇO!
        playerMovement.canMove = true; 

        if (distanceJoint != null)
        {
            Destroy(distanceJoint);
        }

        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (isSwinging && currentAnchor != null)
        {
            lineRenderer.SetPosition(0, whipPoint.position);
            lineRenderer.SetPosition(1, currentAnchor.position);
        }
    }

    private void FixedUpdate()
    {
        if (isSwinging && currentAnchor != null && moveAction != null)
        {
            float horizontalInput = moveAction.action.ReadValue<Vector2>().x;
            
            if (horizontalInput != 0)
            {
                // Direção da corda
                Vector2 ropeDir = (currentAnchor.position - transform.position).normalized;
                
                // Rotaciona 90 graus para criar a Tangente do arco
                Vector2 swingDirection = new Vector2(ropeDir.y, -ropeDir.x); 

                if (horizontalInput < 0 && swingDirection.x > 0) swingDirection *= -1;
                if (horizontalInput > 0 && swingDirection.x < 0) swingDirection *= -1;

                rb.AddForce(swingDirection * swingForce, ForceMode2D.Force);
            }
        }
    }

private bool CanUseAbility()
    {
        if (isUnlockedPermanently) return true; // Sua trava de debug do Inspector
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.hasEarth) return true; // Destravado globalmente!
        if (SceneManager.GetActiveScene().name == nativeScene) return true;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (whipPoint != null)
        {
            float facingDirection = Mathf.Sign(transform.localScale.x);
            float scaledRange = whipRange * Mathf.Abs(transform.localScale.x);
            Vector2 searchCenter = (Vector2)whipPoint.position + new Vector2(facingDirection * scaledRange * 0.5f, scaledRange * 0.5f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(searchCenter, scaledRange);
        }
    }
}