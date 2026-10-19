using UnityEngine;

public class DefinidorDePlano : MonoBehaviour
{
    [Tooltip("Qual crachá todos os objetos dentro deste grupo devem receber?")]
    public PlanoDeJogo planoDoGrupo;

    private void Awake()
    {
        // Pega TODOS os scripts RochaQuebravel que estão nos objetos filhos desta pasta
        RochaQuebravel[] rochasNoGrupo = GetComponentsInChildren<RochaQuebravel>();

        foreach (RochaQuebravel rocha in rochasNoGrupo)
        {
            // Entrega o crachá automaticamente para a rocha
            rocha.planoDestaRocha = planoDoGrupo;
        }
    }
}