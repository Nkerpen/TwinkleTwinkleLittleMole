using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Vida Compartilhada (Frente + Fundo)")]
    [SerializeField, Tooltip("Vida máxima compartilhada entre as duas toupeiras (3 = 3 corações cheios)")]
    private int maxHealth = 3;

    [SerializeField, Tooltip("Vida atual compartilhada entre as duas toupeiras")]
    private int currentHealth = 3;

    [Header("Conexão com o HUD (HUDVida.cs)")]
    [SerializeField, Tooltip("Arraste o objeto do HUD que tem o script HUDVida (se vazio, busca automaticamente na cena)")]
    private HUDVida hudVida;

    [SerializeField, Tooltip("Quantos meios-corações do HUD equivalem a 1 ponto de vida (2 = cada hit tira 1 coração inteiro de 3; 1 = cada hit tira meio coração de 6)")]
    private int meiosCoracoesPorPonto = 2;

    [Header("Invencibilidade e Hit Effect (Shader)")]
    [SerializeField, Tooltip("Tempo de invencibilidade (em segundos) após levar um hit")]
    private float invincibilityDuration = 1.2f;

    [SerializeField, Tooltip("Fallback: intervalo do pisca-pisca de transparência APENAS se o personagem ainda não estiver usando o material TTLM_HitEffect2D")]
    private float blinkInterval = 0.1f;

    [SerializeField, Tooltip("Impede que a toupeira INATIVA (parada no outro plano longe da câmera) tome dano sozinha fora da tela")]
    private bool protegerToupeiraInativa = true;

    [Header("Auto-Configuração de Cenário (F-Fogo2)")]
    [SerializeField, Tooltip("Configura colisores automaticamente nos objetos 'lava' e 'Cura' da cena caso ainda não tenham")]
    private bool autoConfigurarLavaECuraNaCena = true;

    [Header("Eventos")]
    public UnityEvent<int> onHealthChanged;
    public UnityEvent onPlayerDied;

    private bool isInvincible = false;
    private bool isDead = false;
    private readonly List<PlayerMovement> registeredPlayers = new List<PlayerMovement>();
    private readonly List<SpriteRenderer> playerRenderers = new List<SpriteRenderer>();
    private readonly List<PlayerHitEffect> playerHitEffects = new List<PlayerHitEffect>();

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        ResetarEstadoParaCenaAtual();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        InicializarCena();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetarEstadoParaCenaAtual();
        InicializarCena();
    }

    private void ResetarEstadoParaCenaAtual()
    {
        StopAllCoroutines();
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;
        registeredPlayers.Clear();
        playerRenderers.Clear();
        playerHitEffects.Clear();

        if (hudVida == null || !hudVida.gameObject.scene.IsValid())
        {
            hudVida = FindFirstObjectByType<HUDVida>(FindObjectsInactive.Include);
        }
    }

    private void InicializarCena()
    {
        RegistrarPersonagensDaCena();

        if (autoConfigurarLavaECuraNaCena)
        {
            ConfigurarObjetosDeLavaECuraAutomaticamente();
        }

        SetPlayersAlpha(1f);
        AtualizarInterface();
    }

    public static PlayerHealth EnsureInstance()
    {
        if (Instance != null) return Instance;

        PlayerHealth existing = FindFirstObjectByType<PlayerHealth>();
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        CharacterSwitcher switcher = FindFirstObjectByType<CharacterSwitcher>();
        GameObject host = switcher != null ? switcher.gameObject : new GameObject("PlayerHealthManager");
        Instance = host.AddComponent<PlayerHealth>();
        return Instance;
    }

    public void RegistrarPersonagem(PlayerMovement player)
    {
        if (player == null || registeredPlayers.Contains(player)) return;

        registeredPlayers.Add(player);

        if (!player.TryGetComponent<PlayerDamageReceiver>(out var receiver))
        {
            receiver = player.gameObject.AddComponent<PlayerDamageReceiver>();
        }
        receiver.Initialize(this, player);

        // Anexa/registra automaticamente o PlayerHitEffect para usar o Shader de Hit Effect!
        if (!player.TryGetComponent<PlayerHitEffect>(out var hitEffect))
        {
            hitEffect = player.gameObject.AddComponent<PlayerHitEffect>();
        }
        if (!playerHitEffects.Contains(hitEffect))
        {
            playerHitEffects.Add(hitEffect);
        }

        foreach (var sr in player.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.enabled && !playerRenderers.Contains(sr))
            {
                playerRenderers.Add(sr);
            }
        }
    }

    private void RegistrarPersonagensDaCena()
    {
        registeredPlayers.RemoveAll(p => p == null);
        playerRenderers.RemoveAll(sr => sr == null);
        playerHitEffects.RemoveAll(he => he == null);

        PlayerMovement[] players = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p != null && p.gameObject.scene.IsValid())
            {
                RegistrarPersonagem(p);
            }
        }
    }

    private void ConfigurarObjetosDeLavaECuraAutomaticamente()
    {
        SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var sr in allRenderers)
        {
            if (sr == null || !sr.gameObject.scene.IsValid()) continue;

            string objName = sr.gameObject.name;
            // Ignora o objeto pai decorativo "lava" caso ele possua filhos (lava (1), lava (2), lava (3))
            if (objName.StartsWith("lava", System.StringComparison.OrdinalIgnoreCase))
            {
                if (sr.transform.childCount > 0 && objName.Equals("lava", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!sr.TryGetComponent<EnvironmentHazardAndHeal>(out var hazard))
                {
                    hazard = sr.gameObject.AddComponent<EnvironmentHazardAndHeal>();
                    hazard.tipoDoObjeto = EnvironmentHazardAndHeal.TipoObjeto.DanoLava;
                    hazard.quantidade = 1;
                    hazard.reposicionarNoUltimoChaoSeguro = true;
                }
            }
            else if (objName.StartsWith("Cura", System.StringComparison.OrdinalIgnoreCase))
            {
                if (!sr.TryGetComponent<EnvironmentHazardAndHeal>(out var heal))
                {
                    heal = sr.gameObject.AddComponent<EnvironmentHazardAndHeal>();
                    heal.tipoDoObjeto = EnvironmentHazardAndHeal.TipoObjeto.ItemDeCura;
                    heal.quantidade = 1;
                }
            }
        }
    }

    /// <summary>
    /// Reduz a vida compartilhada entre os 2 personagens e atualiza o HUDVida.
    /// Se chegar a 0, reinicia a fase.
    /// </summary>
    public void TakeDamage(int damageAmount = 1, PlayerMovement hitPlayer = null, bool respawnAtSafeSpot = false)
    {
        if (isDead) return;

        // Se a toupeira atingida estiver inativa (parada no outro plano), ignora o dano fora da tela
        if (protegerToupeiraInativa && hitPlayer != null && !hitPlayer.enabled)
        {
            return;
        }

        if (isInvincible && !respawnAtSafeSpot) return;

        currentHealth = Mathf.Clamp(currentHealth - Mathf.Max(1, damageAmount), 0, maxHealth);
        Debug.Log($"[VIDA COMPARTILHADA] Jogador sofreu {damageAmount} de dano! Vida atual: {currentHealth}/{maxHealth}");

        AtualizarInterface();
        onHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (respawnAtSafeSpot && hitPlayer != null)
        {
            hitPlayer.RespawnAtLastSafePosition();
        }

        // Dispara o Hit Effect via Shader (TTLM_HitEffect2D) em todas as toupeiras que usam o material!
        bool algumUsouShaderDeHit = DispararHitEffectViaShader();

        if (gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(InvincibilityRoutine(!algumUsouShaderDeHit));
        }
    }

    private bool DispararHitEffectViaShader()
    {
        bool usouShader = false;
        for (int i = playerHitEffects.Count - 1; i >= 0; i--)
        {
            PlayerHitEffect he = playerHitEffects[i];
            if (he == null) continue;

            he.TriggerHitFlash();
            if (he.HasHitEffectShader)
            {
                usouShader = true;
            }
        }
        return usouShader;
    }

    /// <summary>
    /// Recupera vida compartilhada até o limite máximo e atualiza o HUDVida.
    /// </summary>
    public bool Heal(int healAmount = 1)
    {
        if (isDead || currentHealth >= maxHealth) return false;

        currentHealth = Mathf.Clamp(currentHealth + Mathf.Max(1, healAmount), 0, maxHealth);
        Debug.Log($"[VIDA COMPARTILHADA] Jogador recuperou {healAmount} de vida! Vida atual: {currentHealth}/{maxHealth}");

        AtualizarInterface();
        onHealthChanged?.Invoke(currentHealth);
        return true;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        SetPlayersAlpha(1f);

        Debug.Log("[VIDA COMPARTILHADA] Vida chegou a 0! Reiniciando a fase...");
        onPlayerDied?.Invoke();

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private IEnumerator InvincibilityRoutine(bool usarFallbackDeTransparencia)
    {
        isInvincible = true;

        if (usarFallbackDeTransparencia)
        {
            // Fallback caso o material TTLM_HitEffect2D ainda não tenha sido colocado no SpriteRenderer
            float elapsed = 0f;
            bool visible = true;

            while (elapsed < invincibilityDuration)
            {
                visible = !visible;
                SetPlayersAlpha(visible ? 1f : 0.35f);
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            SetPlayersAlpha(1f);
        }
        else
        {
            // Quando o Shader TTLM_HitEffect2D está ativo, o PlayerHitEffect cuida do flash visual!
            yield return new WaitForSeconds(invincibilityDuration);
        }

        isInvincible = false;
    }

    private void SetPlayersAlpha(float alpha)
    {
        for (int i = playerRenderers.Count - 1; i >= 0; i--)
        {
            SpriteRenderer sr = playerRenderers[i];
            if (sr == null) continue;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void AtualizarInterface()
    {
        if (hudVida == null || !hudVida.gameObject.scene.IsValid())
        {
            hudVida = FindFirstObjectByType<HUDVida>(FindObjectsInactive.Include);
        }

        if (hudVida != null)
        {
            int valorEmMeiosCoracoes = Mathf.Clamp(currentHealth * Mathf.Max(1, meiosCoracoesPorPonto), 0, 6);
            hudVida.Atualizar(valorEmMeiosCoracoes);
        }
    }
}

public class PlayerDamageReceiver : MonoBehaviour
{
    private PlayerHealth healthManager;
    private PlayerMovement playerMovement;
    private PlayerPunch playerPunch;

    public void Initialize(PlayerHealth manager, PlayerMovement movement)
    {
        healthManager = manager;
        playerMovement = movement;
        playerPunch = GetComponent<PlayerPunch>();
    }

    public PlanoDeJogo ObterPlanoDoPersonagem()
    {
        if (playerPunch != null) return playerPunch.planoDestePersonagem;
        if (gameObject.layer == LayerMask.NameToLayer("ChaoFundo") || transform.localScale.x < 0.5f)
            return PlanoDeJogo.Fundo;
        return PlanoDeJogo.Frente;
    }
}
