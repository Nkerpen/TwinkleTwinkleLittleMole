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
        // 1. Distribui os crachás de plano para TODOS os scripts interativos dentro desta pasta
        foreach (var rocha in GetComponentsInChildren<RochaQuebravel>()) rocha.planoDestaRocha = planoDoGrupo;
        foreach (var botao in GetComponentsInChildren<ShootableButton>()) botao.planoDesteBotao = planoDoGrupo;
        foreach (var ancora in GetComponentsInChildren<WhipAnchor>()) ancora.planoDesteAncoradouro = planoDoGrupo;
        foreach (var vento in GetComponentsInChildren<WindGust>()) vento.planoDesteVento = planoDoGrupo;
        foreach (var portal in GetComponentsInChildren<Portal>()) portal.planoDestePortal = planoDoGrupo;
        foreach (var col in GetComponentsInChildren<Collectible>()) col.planoDesteColetavel = planoDoGrupo;
        foreach (var baseM in GetComponentsInChildren<AmmoStation>()) baseM.planoDestaBase = planoDoGrupo;

        // 2. Muda a Layer de colisão de TODOS os filhos automaticamente
        if (!string.IsNullOrEmpty(nomeDaLayerFisica))
        {
            int layerID = LayerMask.NameToLayer(nomeDaLayerFisica);
            if (layerID != -1) // Verifica se você digitou o nome da Layer corretamente
            {
                // Pega todos os objetos filhos (rochas, chão, portais) e joga na Layer certa
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