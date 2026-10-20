    using UnityEngine;

    public class DefinidorDePlano : MonoBehaviour
    {
        [Header("Configuração do Grupo")]
        [Tooltip("Qual crachá todos os objetos dentro deste grupo devem receber?")]
        public PlanoDeJogo planoDoGrupo;

        [Tooltip("Escreva o exato nome da Layer Física (ex: ChaoFrente ou ChaoFundo). O script mudará tudo para ela!")]
        public string nomeDaLayerFisica;

    private void Awake()
    {
        // 1. O parâmetro (true) garante que até os objetos desativados recebam o crachá!
        foreach (var rocha in GetComponentsInChildren<RochaQuebravel>(true)) rocha.planoDestaRocha = planoDoGrupo;
        foreach (var botao in GetComponentsInChildren<ShootableButton>(true)) botao.planoDesteBotao = planoDoGrupo;
        foreach (var ancora in GetComponentsInChildren<WhipAnchor>(true)) ancora.planoDesteAncoradouro = planoDoGrupo;
        foreach (var vento in GetComponentsInChildren<WindGust>(true)) vento.planoDesteVento = planoDoGrupo;
        foreach (var portal in GetComponentsInChildren<Portal>(true)) portal.planoDestePortal = planoDoGrupo;
        foreach (var col in GetComponentsInChildren<Collectible>(true)) col.planoDesteColetavel = planoDoGrupo;
        foreach (var baseM in GetComponentsInChildren<AmmoStation>(true)) baseM.planoDestaBase = planoDoGrupo;

        // 2. Muda a Layer de colisão de TODOS os filhos automaticamente[cite: 17]
        if (!string.IsNullOrEmpty(nomeDaLayerFisica))
        {
            int layerID = LayerMask.NameToLayer(nomeDaLayerFisica);
            if (layerID != -1) 
            {
                foreach (Transform filho in GetComponentsInChildren<Transform>(true))
                {
                    filho.gameObject.layer = layerID;
                }
            }
            else
            {
                Debug.LogWarning($"A Layer '{nomeDaLayerFisica}' não existe! Crie ela nas tags da Unity.");
            }
        }
    }
    }