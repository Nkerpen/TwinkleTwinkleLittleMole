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



    public void Break()

    {

        Debug.Log($"A rocha {gameObject.name} ({planoDestaRocha}) foi destruída!");
        

        // Espaço reservado para adicionar efeitos futuros:

        // Instantiate(particulasDePedra, transform.position, Quaternion.identity);

        // AudioSource.PlayClipAtPoint(somDeQuebra, transform.position);

        
        Destroy(gameObject);

    }

}