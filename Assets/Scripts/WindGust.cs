using UnityEngine;

public class WindGust : MonoBehaviour
{
    [Header("Configuração do Vento")]
    [Tooltip("Em qual plano este vento atua?")]
    public PlanoDeJogo planoDesteVento;
    
    [Tooltip("Força e direção do vento. Ex: (X: 10, Y: 0) empurra para a direita. (X: 0, Y: 5) empurra para cima.")]
    public Vector2 windVelocity = new Vector2(0f, 5f);

    [Header("Partículas da Corrente de Vento")]
    [Tooltip("Se deixar vazio, busca automaticamente um ParticleSystem filho deste objeto")]
    [SerializeField] private ParticleSystem windParticles;

    private void OnValidate()
    {
        if (windParticles == null)
            windParticles = GetComponentInChildren<ParticleSystem>(true);
        SyncWindParticles();
    }

    private void Start()
    {
        if (windParticles == null)
            windParticles = GetComponentInChildren<ParticleSystem>(true);
        SyncWindParticles();
    }

    public void SyncWindParticles()
    {
        if (windParticles == null) return;

        float scaleFactor = (planoDesteVento == PlanoDeJogo.Fundo) ? 0.2f : 1f;

        var main = windParticles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f * scaleFactor, 0.14f * scaleFactor);

        // Alimenta o Velocity over Lifetime matematicamente com windVelocity
        var vel = windParticles.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(windVelocity.x * scaleFactor);
        vel.y = new ParticleSystem.MinMaxCurve(windVelocity.y * scaleFactor);
        vel.z = new ParticleSystem.MinMaxCurve(0.5f);

        // Noise Module: baixa força (~0.1) e alta frequência
        var noise = windParticles.noise;
        noise.enabled = true;
        noise.strength = 0.2f * scaleFactor;
        noise.frequency = 1.5f;
        noise.scrollSpeed = 0.5f;

        // Render Mode: None
        var psRenderer = windParticles.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            psRenderer.renderMode = ParticleSystemRenderMode.None;
            psRenderer.velocityScale = 0.15f;
            psRenderer.lengthScale = 1.5f;
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