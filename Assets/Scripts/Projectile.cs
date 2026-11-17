using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Configurações")]
    public float baseSpeed = 15f;
    public float lifeTime = 3f; // Destrói o tiro após 3 segundos para não pesar o jogo
    
    private PlanoDeJogo planoDesteProjetil;
    private Rigidbody2D rb;

    // Essa função é chamada pela Toupeira no momento do tiro
    public void Setup(float directionX, float playerScale, PlanoDeJogo plano)
    {
        rb = GetComponent<Rigidbody2D>();
        planoDesteProjetil = plano;

        // 1. O tiro nasce com o tamanho proporcional ao boneco que atirou (0.2 se for no fundo!)
        transform.localScale *= playerScale;

        // 2. A velocidade também é ajustada para não cruzar a tela do fundo em 1 frame
        float actualSpeed = baseSpeed * playerScale;

        // 3. Aplica o movimento
        rb.linearVelocity = new Vector2(directionX * actualSpeed, 0f);
        
        // Destrói automaticamente se não bater em nada
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Verifica se bateu em uma rocha
        if (collision.TryGetComponent<RochaQuebravel>(out RochaQuebravel rock))
        {
            if (rock.planoDestaRocha == planoDesteProjetil)
            {
                rock.Break();
                Destroy(gameObject); // O tiro some ao bater
                return; // Para a verificação aqui
            }
        }

        // 2. Verifica se bateu em um Botão
        if (collision.TryGetComponent<ShootableButton>(out ShootableButton btn))
        {
            if (btn.planoDesteBotao == planoDesteProjetil)
            {
                btn.Activate();
                Destroy(gameObject); // O tiro some ao bater no botão
                return; 
            }
        }

        // 3. Se bater no chão/parede da sua respectiva Layer, o tiro também some
        if (collision.gameObject.layer == LayerMask.NameToLayer("ChaoFrente") && planoDesteProjetil == PlanoDeJogo.Frente)
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("ChaoFundo") && planoDesteProjetil == PlanoDeJogo.Fundo)
        {
            Destroy(gameObject);
        }
    }
}