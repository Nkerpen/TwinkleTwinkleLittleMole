using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnvironmentHazardAndHeal : MonoBehaviour
{
    public enum TipoObjeto
    {
        DanoLava,
        ItemDeCura
    }

    [Header("Configuração de Dano (Lava) ou Cura")]
    public TipoObjeto tipoDoObjeto = TipoObjeto.DanoLava;

    [Tooltip("Plano do objeto (preenchido pelo DefinidorDePlano; se AfetarAmbosOsPlanos estiver ativo, afeta os 2)")]
    public PlanoDeJogo planoDesteObjeto = PlanoDeJogo.Frente;

    [Tooltip("Na lava de fundo de cenário, permite afetar qualquer toupeira que caia nela")]
    public bool afetarAmbosOsPlanos = true;

    [Tooltip("Quantidade de vida retirada (Lava) ou restaurada (Cura)")]
    public int quantidade = 1;

    [Tooltip("Se for Lava e o jogador ainda tiver vida (> 0), reposiciona no último chão seguro")]
    public bool reposicionarNoUltimoChaoSeguro = true;

    private BoxCollider2D boxCollider;
    private bool foiColetado = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
            if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr) && sr.sprite != null && boxCollider.size.sqrMagnitude < 0.01f)
            {
                boxCollider.size = sr.sprite.bounds.size;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (foiColetado || collision == null || !collision.CompareTag("Player")) return;

        if (collision.TryGetComponent<PlayerPunch>(out PlayerPunch punch))
        {
            if (!afetarAmbosOsPlanos && punch.planoDestePersonagem != this.planoDesteObjeto)
                return;

            PlayerHealth health = PlayerHealth.EnsureInstance();
            PlayerMovement pm = collision.GetComponent<PlayerMovement>();

            if (tipoDoObjeto == TipoObjeto.DanoLava)
            {
                health.TakeDamage(quantidade, pm, reposicionarNoUltimoChaoSeguro);
            }
            else if (tipoDoObjeto == TipoObjeto.ItemDeCura)
            {
                if (health.Heal(quantidade))
                {
                    foiColetado = true;
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
