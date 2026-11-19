using UnityEngine;

public class DefinidorDePlano : MonoBehaviour
{
    [Header("Configuração do Grupo")]
    [Tooltip("Qual crachá todos os objetos dentro deste grupo devem receber?")]
    public PlanoDeJogo planoDoGrupo;

    [Tooltip("Escreva o exato nome da Layer Física (ex: ChaoFrente ou ChaoFundo). O script mudará tudo para ela!")]
    public string nomeDaLayerFisica;

    [Header("Auto-Conexão da Demo (F-Fogo2)")]
    [Tooltip("Garante que armadilhas (armadilha*), botões de pisar (buttonsPintura_1), alavancas (buttonsPintura_2, buttons_1), diamantes e curas recebam os scripts automaticamente")]
    public bool autoVincularComponentesNosFilhos = true;

    private void Awake()
    {
        if (autoVincularComponentesNosFilhos)
        {
            VincularComponentesDaDemoAutomaticamente();
            Collectible.ConfigurarDiamantesDaCenaAutomaticamente();
        }

        // 1. O parâmetro (true) garante que até os objetos desativados recebam o crachá!
        foreach (var rocha in GetComponentsInChildren<RochaQuebravel>(true)) rocha.planoDestaRocha = planoDoGrupo;
        foreach (var botao in GetComponentsInChildren<ShootableButton>(true)) botao.planoDesteBotao = planoDoGrupo;
        foreach (var armadilha in GetComponentsInChildren<FireTrap>(true)) armadilha.planoDestaArmadilha = planoDoGrupo;
        foreach (var item in GetComponentsInChildren<EnvironmentHazardAndHeal>(true))
        {
            item.planoDesteObjeto = planoDoGrupo;
            item.afetarAmbosOsPlanos = false;
        }
        foreach (var ancora in GetComponentsInChildren<WhipAnchor>(true)) ancora.planoDesteAncoradouro = planoDoGrupo;
        foreach (var vento in GetComponentsInChildren<WindGust>(true)) vento.planoDesteVento = planoDoGrupo;
        foreach (var portal in GetComponentsInChildren<Portal>(true)) portal.planoDestePortal = planoDoGrupo;
        foreach (var col in GetComponentsInChildren<Collectible>(true)) col.planoDesteColetavel = planoDoGrupo;
        foreach (var baseM in GetComponentsInChildren<AmmoStation>(true)) baseM.planoDestaBase = planoDoGrupo;

        // 2. Muda a Layer de colisão de TODOS os filhos automaticamente
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

    private void VincularComponentesDaDemoAutomaticamente()
    {
        Animator escadaAnimator = null;
        Animator pilaresAnimator = null;
        Animator blocosRealAnimator = null;
        Animator blocosPinturaAnimator = null;

        foreach (var anim in FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string animName = anim.gameObject.name;
            if (animName.StartsWith("EscadadePilares", System.StringComparison.OrdinalIgnoreCase))
                escadaAnimator = anim;
            else if (animName.StartsWith("puzzlePilares", System.StringComparison.OrdinalIgnoreCase))
                pilaresAnimator = anim;
            else if (animName.StartsWith("blocos_mundo_real", System.StringComparison.OrdinalIgnoreCase))
                blocosRealAnimator = anim;
            else if (animName.StartsWith("blocosPintura", System.StringComparison.OrdinalIgnoreCase) ||
                     animName.StartsWith("blocosPirtura", System.StringComparison.OrdinalIgnoreCase))
                blocosPinturaAnimator = anim;
        }

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            string n = child.gameObject.name;

            // A) Armadilhas de Fogo (ex: armadilhaReal 1_1, armadilhaReal 1_1 (1))
            if (n.StartsWith("armadilha", System.StringComparison.OrdinalIgnoreCase) && child.GetComponent<SpriteRenderer>() != null)
            {
                if (!child.TryGetComponent<FireTrap>(out _))
                {
                    child.gameObject.AddComponent<FireTrap>();
                }
            }
            // B) Botões de Pisar e Alavancas de Soco (ex: buttonsPintura_1, buttonsPintura_2, buttons_1)
            else if (n.StartsWith("buttons", System.StringComparison.OrdinalIgnoreCase) && child.GetComponent<SpriteRenderer>() != null)
            {
                bool jaTinhaComponente = child.TryGetComponent<ShootableButton>(out ShootableButton btn);
                if (!jaTinhaComponente)
                {
                    btn = child.gameObject.AddComponent<ShootableButton>();
                }

                if (!jaTinhaComponente)
                {
                    if (n.StartsWith("buttonsPintura_2", System.StringComparison.OrdinalIgnoreCase) ||
                        n.StartsWith("buttons_1", System.StringComparison.OrdinalIgnoreCase))
                    {
                        btn.tipoDeGatilho = ShootableButton.TipoGatilho.AlavancaDeSoco;
                    }
                    else
                    {
                        btn.tipoDeGatilho = ShootableButton.TipoGatilho.BotaoDePisar;
                    }
                }

                bool semAnimadores = btn.animadoresParaAcionar == null || btn.animadoresParaAcionar.Count == 0;
                bool semDesativar = btn.objetosParaDesativar == null || btn.objetosParaDesativar.Count == 0;
                bool semAtivar = btn.objetosParaAtivar == null || btn.objetosParaAtivar.Count == 0;
                bool semEventos = btn.onActivate == null || btn.onActivate.GetPersistentEventCount() == 0;

                bool semAlvoConfigurado = semAnimadores && semDesativar && semAtivar && semEventos;

                if (semAlvoConfigurado)
                {
                    if (n.StartsWith("buttonsPintura_2", System.StringComparison.OrdinalIgnoreCase))
                    {
                        btn.RegistrarPuzzleAlvo(escadaAnimator);
                        btn.RegistrarPuzzleAlvo(blocosPinturaAnimator);
                    }
                    else if (n.StartsWith("buttonsPintura_1", System.StringComparison.OrdinalIgnoreCase))
                    {
                        btn.RegistrarPuzzleAlvo(pilaresAnimator);
                        btn.RegistrarPuzzleAlvo(blocosRealAnimator);
                        btn.RegistrarPuzzleAlvo(blocosPinturaAnimator);
                    }
                    else if (n.StartsWith("buttons_1", System.StringComparison.OrdinalIgnoreCase))
                    {
                        btn.RegistrarPuzzleAlvo(pilaresAnimator);
                        btn.RegistrarPuzzleAlvo(blocosRealAnimator);
                        btn.RegistrarPuzzleAlvo(escadaAnimator);
                    }
                }
            }
            // C) Gemas de Cura (ex: Cura_0, Cura (1), Cura (2))
            else if (n.StartsWith("Cura", System.StringComparison.OrdinalIgnoreCase) && child.GetComponent<SpriteRenderer>() != null)
            {
                if (!child.TryGetComponent<EnvironmentHazardAndHeal>(out var heal))
                {
                    heal = child.gameObject.AddComponent<EnvironmentHazardAndHeal>();
                    heal.tipoDoObjeto = EnvironmentHazardAndHeal.TipoObjeto.ItemDeCura;
                    heal.quantidade = 1;
                }
            }
            // D) Diamantes Incolores Coletáveis (ex: diamante mundo Real incolor_0)
            else if (n.StartsWith("diamante", System.StringComparison.OrdinalIgnoreCase) && child.GetComponent<SpriteRenderer>() != null)
            {
                if (!child.TryGetComponent<Collectible>(out var diamante))
                {
                    diamante = child.gameObject.AddComponent<Collectible>();
                    diamante.myType = Collectible.CollectibleType.DiamanteDaFase;
                }
            }
        }
    }
}