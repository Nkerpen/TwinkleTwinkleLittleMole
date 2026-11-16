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

    [Header("Partículas do Chicote (Rastro Reativo)")]
    [Tooltip("Se deixar vazio, procura automaticamente um ParticleSystem filho do WhipPoint")]
    [SerializeField] private ParticleSystem whipTrailParticles;
    
    [Header("Progresso")]
    public bool isUnlockedPermanently = false;
    [SerializeField] private string nativeScene = "F-TerraScene";

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private DistanceJoint2D distanceJoint;
    private LineRenderer lineRenderer;
    
    private bool isSwinging;
    private Transform currentAnchor;
    private float scaleMultiplier = 1f;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        
        // CÁLCULO PROPORCIONAL
        scaleMultiplier = Mathf.Abs(transform.localScale.x);
        swingForce *= scaleMultiplier;

        if (whipTrailParticles == null && whipPoint != null)
        {
            whipTrailParticles = whipPoint.GetComponentInChildren<ParticleSystem>(true);
        }

        SetupWhipParticles();
    }

    private void SetupWhipParticles()
    {
        if (whipTrailParticles == null) return;

        var main = whipTrailParticles.main;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.emitterVelocityMode = ParticleSystemEmitterVelocityMode.Transform;
        main.gravityModifier = 0.4f * scaleMultiplier;
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f * scaleMultiplier, 0.25f * scaleMultiplier);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f * scaleMultiplier, 0.6f * scaleMultiplier);

        // Emissão por Rate over Distance (só emite quando há movimento no balanço)
        var emission = whipTrailParticles.emission;
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 15f / Mathf.Max(scaleMultiplier, 0.2f);

        // Shape: Edge
        var shape = whipTrailParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;

        // Color over Lifetime: Verde-folha para Marrom-terra com Alpha indo até zero
        var col = whipTrailParticles.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.30f, 0.55f, 0.24f), 0f),
                new GradientColorKey(new Color(0.48f, 0.32f, 0.19f), 0.65f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        whipTrailParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
        
        playerMovement.canMove = false; 

        distanceJoint = gameObject.AddComponent<DistanceJoint2D>();
        distanceJoint.autoConfigureConnectedAnchor = false;
        distanceJoint.connectedAnchor = currentAnchor.position;
        distanceJoint.anchor = transform.InverseTransformPoint(whipPoint.position); 
        distanceJoint.distance = Vector2.Distance(whipPoint.position, currentAnchor.position);

        lineRenderer.enabled = true;

        if (whipTrailParticles != null)
        {
            whipTrailParticles.Play();
        }
    }

    private void StopSwing()
    {
        isSwinging = false;
        currentAnchor = null;
        
        playerMovement.canMove = true; 

        if (distanceJoint != null)
        {
            Destroy(distanceJoint);
        }

        lineRenderer.enabled = false;

        if (whipTrailParticles != null)
        {
            whipTrailParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void Update()
    {
        if (isSwinging && currentAnchor != null)
        {
            Vector3 startPos = whipPoint.position;
            Vector3 endPos = currentAnchor.position;

            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);

            // Posiciona e alinha o Shape: Edge exatamente sobre o LineRenderer da corda
            if (whipTrailParticles != null)
            {
                whipTrailParticles.transform.position = (startPos + endPos) * 0.5f;
                Vector3 dir = endPos - startPos;
                float length = dir.magnitude;
                if (length > 0.01f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    whipTrailParticles.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    var shape = whipTrailParticles.shape;
                    shape.radius = (length * 0.5f) / Mathf.Max(Mathf.Abs(whipTrailParticles.transform.lossyScale.x), 0.01f);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (isSwinging && currentAnchor != null && moveAction != null)
        {
            float horizontalInput = moveAction.action.ReadValue<Vector2>().x;
            
            if (horizontalInput != 0)
            {
                Vector2 ropeDir = (currentAnchor.position - transform.position).normalized;
                Vector2 swingDirection = new Vector2(ropeDir.y, -ropeDir.x); 

                if (horizontalInput < 0 && swingDirection.x > 0) swingDirection *= -1;
                if (horizontalInput > 0 && swingDirection.x < 0) swingDirection *= -1;

                rb.AddForce(swingDirection * swingForce, ForceMode2D.Force);
            }
        }
    }

    private bool CanUseAbility()
    {
        if (isUnlockedPermanently) return true;
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.hasEarth) return true;
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