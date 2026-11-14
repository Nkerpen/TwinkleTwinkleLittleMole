using UnityEngine;

public class AmmoStation : MonoBehaviour
{
    [Header("Configuração da Base")]
    [Tooltip("Só permite que a toupeira deste respectivo plano pegue esta munição")]
    public PlanoDeJogo planoDestaBase;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se quem pisou tem o script de atirar...
        if (collision.TryGetComponent<PlayerShoot>(out PlayerShoot shooter))
        {
            // ...está no mesmo plano físico E precisa de munição
            if (shooter.planoDestePersonagem == planoDestaBase && shooter.NeedsAmmo())
            {
                shooter.ReloadAmmo();
                
                // Agora você pode destruir a base se quiser que ela seja uso único!
                // Destroy(gameObject); 
            }
            else if (shooter.planoDestePersonagem == planoDestaBase && !shooter.NeedsAmmo())
            {
                Debug.Log("Munição já está cheia! Base ignorada.");
            }
        }
    }
}