using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WindGust : MonoBehaviour
{
    [Header("Configuração do Vento")]
    [Tooltip("Em qual plano este vento atua?")]
    public PlanoDeJogo planoDesteVento;

    [Tooltip("Força e direção do vento. Ex: (X: 0, Y: 10) empurra para cima. (X: 10, Y: 0) empurra para a direita.")]
    public Vector2 windVelocity = new Vector2(0f, 10f);

    [Header("Partículas da Corrente de Vento")]
    [Tooltip("Se deixar vazio, busca automaticamente um ParticleSystem filho deste objeto")]
    [SerializeField] private ParticleSystem windParticles;

    [Range(0.05f, 0.25f)]
    [Tooltip("Faixa inicial do colisor onde as partículas nascem (ex: 0.15 = primeiros 15% da base)")]
    [SerializeField] private float porcentagemBaseEmissao = 0.15f;

    [Tooltip("Multiplicador visual de velocidade das partículas (1 = mesma velocidade do vento)")]
    [SerializeField] private float multiplicadorVelocidadeVisual = 1f;

    private BoxCollider2D windCollider;

    private void OnValidate()
    {
        windCollider = GetComponent<BoxCollider2D>();
        if (windParticles == null)
            windParticles = GetComponentInChildren<ParticleSystem>(true);
        SyncWindParticles();
    }

    private void Start()
    {
        windCollider = GetComponent<BoxCollider2D>();
        if (windParticles == null)
            windParticles = GetComponentInChildren<ParticleSystem>(true);
        SyncWindParticles();
    }

    public void SyncWindParticles()
    {
        if (windParticles == null) return;
        if (windCollider == null) windCollider = GetComponent<BoxCollider2D>();
        if (windCollider == null) return;

        float scaleFactor = (planoDesteVento == PlanoDeJogo.Fundo) ? 0.2f : 1f;

        // 1. Mede o tamanho real em unidades de mundo do BoxCollider2D (acompanha qualquer escala do objeto pai)
        Bounds bounds = windCollider.bounds;
        if (bounds.size.sqrMagnitude < 0.0001f)
        {
            // Fallback quando chamado no OnValidate dentro do Prefab Mode
            Vector3 lossy = transform.lossyScale;
            bounds = new Bounds(
                transform.position + Vector3.Scale(windCollider.offset, lossy),
                new Vector3(Mathf.Abs(windCollider.size.x * lossy.x), Mathf.Abs(windCollider.size.y * lossy.y), 0.1f)
            );
        }

        Vector2 dir = windVelocity.sqrMagnitude > 0.0001f ? windVelocity.normalized : Vector2.up;
        bool isVertical = Mathf.Abs(dir.y) >= Mathf.Abs(dir.x);

        float totalDistance = isVertical ? bounds.size.y : bounds.size.x;
        float crossWidth = isVertical ? bounds.size.x : bounds.size.y;
        float spawnBand = Mathf.Max(0.05f, totalDistance * porcentagemBaseEmissao);

        // 2. Posiciona o emissor na base (primeiros 5% a 20% do colisor)
        Vector3 emitterWorldPos = bounds.center;
        if (isVertical)
        {
            float baseY = (dir.y >= 0f)
                ? (bounds.min.y + spawnBand * 0.5f)
                : (bounds.max.y - spawnBand * 0.5f);
            emitterWorldPos = new Vector3(bounds.center.x, baseY, transform.position.z);
        }
        else
        {
            float baseX = (dir.x >= 0f)
                ? (bounds.min.x + spawnBand * 0.5f)
                : (bounds.max.x - spawnBand * 0.5f);
            emitterWorldPos = new Vector3(baseX, bounds.center.y, transform.position.z);
        }

        windParticles.transform.position = emitterWorldPos;
        windParticles.transform.rotation = Quaternion.identity;

        // 3. Ajusta o Shape: Box exatamente à largura do colisor e altura da faixa base
        Vector3 parentLossy = transform.lossyScale;
        float invX = Mathf.Abs(parentLossy.x) > 0.0001f ? 1f / Mathf.Abs(parentLossy.x) : 1f;
        float invY = Mathf.Abs(parentLossy.y) > 0.0001f ? 1f / Mathf.Abs(parentLossy.y) : 1f;
        windParticles.transform.localScale = new Vector3(invX, invY, 1f);

        var shape = windParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.position = Vector3.zero;
        shape.scale = isVertical
            ? new Vector3(crossWidth, spawnBand, 0.1f)
            : new Vector3(spawnBand, crossWidth, 0.1f);

        // 4. Cálculo exato de Lifetime = Distância / Velocidade para morrer no limite do colisor
        Vector2 particleVel = windVelocity * scaleFactor * Mathf.Max(0.1f, multiplicadorVelocidadeVisual);
        float speed = Mathf.Max(0.1f, particleVel.magnitude);

        float distMin = Mathf.Max(0.05f, totalDistance - spawnBand);
        float distMax = Mathf.Max(distMin, totalDistance);

        float minLifetime = distMin / speed;
        float maxLifetime = distMax / speed;

        var main = windParticles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        main.startSpeed = 0f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(minLifetime, maxLifetime);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f * scaleFactor, 0.14f * scaleFactor);

        // 5. Velocity over Lifetime em espaço World (com Z = 0)
        var vel = windParticles.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(particleVel.x);
        vel.y = new ParticleSystem.MinMaxCurve(particleVel.y);
        vel.z = new ParticleSystem.MinMaxCurve(0f);

        var noise = windParticles.noise;
        noise.enabled = true;
        noise.strength = 0.2f * scaleFactor;
        noise.frequency = 1.5f;
        noise.scrollSpeed = 0.5f;

        // 7. Mantém o Render Mode e ajusta apenas o Sorting Order
        var psRenderer = windParticles.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            psRenderer.sortingOrder = (planoDesteVento == PlanoDeJogo.Fundo) ? -1 : 1;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerGlide>(out PlayerGlide glider))
        {
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
