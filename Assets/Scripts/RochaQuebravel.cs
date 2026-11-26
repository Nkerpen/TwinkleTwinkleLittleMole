using UnityEngine;

public enum PlanoDeJogo 
{ 
    Frente, 
    Fundo 
}

public class RochaQuebravel : MonoBehaviour
{
    [Header("Configuração da Rocha")]
    [Tooltip("Escolha em qual plano esta rocha existe")]
    public PlanoDeJogo planoDestaRocha;

    [Header("Efeito Visual (Automático do Rocha.prefab)")]
    [Tooltip("Se deixar vazio, usa automaticamente o Particle System que já é filho do Rocha.prefab")]
    [SerializeField] private ParticleSystem particulasDePedra;

    private SpriteRenderer rockRenderer;
    private Collider2D rockCollider;
    private bool isBroken = false;

    private void Awake()
    {
        rockRenderer = GetComponent<SpriteRenderer>();
        rockCollider = GetComponent<Collider2D>();

        if (particulasDePedra == null)
        {
            particulasDePedra = GetComponentInChildren<ParticleSystem>(true);
        }

        if (particulasDePedra != null)
        {
            var main = particulasDePedra.main;
            // CRÍTICO: Muda stopAction para None ANTES de chamar Stop(),
            // caso contrário a Unity destrói o ParticleSystem no frame 0!
            main.stopAction = ParticleSystemStopAction.None;
            main.playOnAwake = false;
            main.loop = false;
            particulasDePedra.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void Break()
    {
        if (isBroken) return;
        isBroken = true;

        Debug.Log($"A rocha {gameObject.name} ({planoDestaRocha}) foi destruída!");

        if (particulasDePedra != null)
        {
            // 1. Garante que está ativo e desparenta antes de destruir a rocha
            particulasDePedra.gameObject.SetActive(true);
            particulasDePedra.transform.SetParent(null);
            particulasDePedra.transform.position = transform.position;
            particulasDePedra.transform.rotation = Quaternion.identity;
            particulasDePedra.transform.localScale = Vector3.one;

            float scaleFactor = (planoDestaRocha == PlanoDeJogo.Fundo) ? 0.2f : 1f;

            // 2. Main Module: agora sim pode ativar StopAction = Destroy
            var main = particulasDePedra.main;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.gravityModifier = 1.5f * scaleFactor;
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f * scaleFactor, 7f * scaleFactor);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f * scaleFactor, 0.35f * scaleFactor);
            main.stopAction = ParticleSystemStopAction.Destroy;

            // 3. Shape Module: Box do tamanho do colisor da rocha
            var shape = particulasDePedra.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            if (rockCollider != null)
            {
                shape.scale = rockCollider.bounds.size;
            }

            // 4. Collision Module (2D): Colide apenas com ChaoFrente ou ChaoFundo
            var collision = particulasDePedra.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision2D;
            collision.bounce = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
            collision.dampen = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);

            string nomeLayerChao = (planoDestaRocha == PlanoDeJogo.Frente) ? "ChaoFrente" : "ChaoFundo";
            int mask = LayerMask.GetMask(nomeLayerChao);
            if (mask != 0)
            {
                collision.collidesWith = mask;
            }

            // 5. Sincroniza Sorting Layer e desenha logo à frente do plano da rocha
            var psRenderer = particulasDePedra.GetComponent<ParticleSystemRenderer>();
            if (rockRenderer != null && psRenderer != null)
            {
                psRenderer.sortingLayerID = rockRenderer.sortingLayerID;
                psRenderer.sortingOrder = rockRenderer.sortingOrder + 1;
            }

            // 6. Reinicia o relógio interno e dispara o Burst
            particulasDePedra.Clear(true);
            particulasDePedra.Play(true);
            Destroy(particulasDePedra.gameObject, 2.5f);
        }
        else
        {
            Debug.LogWarning($"[RochaQuebravel] '{gameObject.name}' quebrou sem ParticleSystem filho!");
        }

        Destroy(gameObject);
    }
}