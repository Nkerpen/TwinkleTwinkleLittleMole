using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField, Tooltip("Referência para a ação de trocar de personagem (SwitchCharacters)")]
    private InputActionReference switchAction;

    [Header("Personagens (Da Cena / Hierarchy)")]
    [SerializeField] private PlayerMovement playerForeground;
    [SerializeField] private PlayerMovement playerBackground;

    [Header("Câmeras (Da Cena / Hierarchy)")]
    [SerializeField] private GameObject vcamForeground;
    [SerializeField] private GameObject vcamBackground;

    [Header("Proteção de Troca e Distância")]
    [SerializeField, Tooltip("Tempo mínimo entre trocas para evitar duplo acionamento no mesmo clique")]
    private float switchCooldown = 0.15f;

    [SerializeField, Tooltip("Sincroniza a posição da câmera virtual com a toupeira ativa antes de ativá-la (evita travamento quando estão muito longe)")]
    private bool sincronizarPosicaoDaCameraNaTroca = true;

    private bool isForegroundActive = true;
    private float lastSwitchTime = -10f;

    public bool IsForegroundActive => isForegroundActive;

    private void Awake()
    {
        ValidarReferenciasDaCena();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        HabilitarInputDeTroca();
    }

    private void OnDisable()
    {
        if (switchAction != null && switchAction.action != null)
        {
            // IMPORTANTE: Removemos apenas o callback! NUNCA chamamos switchAction.action.Disable() aqui,
            // pois InputActionReference é um asset global e desativá-lo durante o recarregamento da cena
            // faz o botão de troca parar de responder após morrer!
            switchAction.action.performed -= OnSwitchPerformed;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (switchAction != null && switchAction.action != null)
        {
            switchAction.action.performed -= OnSwitchPerformed;
        }
    }

    private void Start()
    {
        ValidarReferenciasDaCena();
        UpdateCharacterStates(true);
    }

    private void Update()
    {
        // Garante que a Action e o ActionMap nunca fiquem desativados após reiniciar a cena ou interagir com UI
        if (switchAction != null && switchAction.action != null)
        {
            if (switchAction.action.actionMap != null && !switchAction.action.actionMap.enabled)
            {
                switchAction.action.actionMap.Enable();
            }

            if (!switchAction.action.enabled)
            {
                switchAction.action.Enable();
            }

            // Polling direto em Update: garante que a troca funcione mesmo se o callback 'performed'
            // tiver sido perdido ao recarregar a cena ou ao segurar múltiplas teclas andando para longe
            if (switchAction.action.WasPressedThisFrame())
            {
                TentarTrocarPersonagem();
            }
        }
    }

    private void HabilitarInputDeTroca()
    {
        if (switchAction != null && switchAction.action != null)
        {
            if (switchAction.action.actionMap != null && !switchAction.action.actionMap.enabled)
            {
                switchAction.action.actionMap.Enable();
            }

            switchAction.action.performed -= OnSwitchPerformed;
            switchAction.action.performed += OnSwitchPerformed;

            if (!switchAction.action.enabled)
            {
                switchAction.action.Enable();
            }
        }
    }

    /// <summary>
    /// Chamado automaticamente toda vez que a cena é recarregada (ex: ao morrer e reiniciar a fase).
    /// Garante que mesmo se o GameManager tiver DontDestroyOnLoad, ele reconecte as novas toupeiras e câmeras!
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isForegroundActive = true;
        lastSwitchTime = -10f;

        HabilitarInputDeTroca();
        ValidarReferenciasDaCena();
        UpdateCharacterStates(true);
    }

    private void OnSwitchPerformed(InputAction.CallbackContext context)
    {
        TentarTrocarPersonagem();
    }

    public void TentarTrocarPersonagem()
    {
        if (Time.unscaledTime - lastSwitchTime < switchCooldown) return;

        ValidarReferenciasDaCena();

        if (playerForeground == null || playerBackground == null)
        {
            Debug.LogWarning("[CharacterSwitcher] Uma das toupeiras não foi encontrada na cena ativa!");
            return;
        }

        lastSwitchTime = Time.unscaledTime;
        isForegroundActive = !isForegroundActive;
        UpdateCharacterStates(sincronizarPosicaoDaCameraNaTroca);
    }

    /// <summary>
    /// Garante que as referências apontem sempre para as instâncias REAIS da cena ativa (F-Fogo2),
    /// e nunca para objetos destruídos da vida anterior ou para prefabs da aba Project.
    /// </summary>
    private void ValidarReferenciasDaCena()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        bool fgInvalido = playerForeground == null || !playerForeground.gameObject.scene.IsValid() || playerForeground.gameObject.scene != activeScene;
        bool bgInvalido = playerBackground == null || !playerBackground.gameObject.scene.IsValid() || playerBackground.gameObject.scene != activeScene;

        if (fgInvalido || bgInvalido)
        {
            PlayerMovement[] playersNaCena = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (PlayerMovement pm in playersNaCena)
            {
                if (pm == null || !pm.gameObject.scene.IsValid()) continue;

                bool ehFundo = false;
                if (pm.TryGetComponent<PlayerPunch>(out PlayerPunch punch))
                {
                    ehFundo = (punch.planoDestePersonagem == PlanoDeJogo.Fundo);
                }
                else
                {
                    ehFundo = pm.gameObject.name.Contains("Fundo") || Mathf.Abs(pm.transform.localScale.x) < 0.5f;
                }

                if (ehFundo && bgInvalido)
                {
                    playerBackground = pm;
                    bgInvalido = false;
                }
                else if (!ehFundo && fgInvalido)
                {
                    playerForeground = pm;
                    fgInvalido = false;
                }
            }
        }

        // Garante que ambas as toupeiras estejam ativas na Hierarchy (caso alguma tenha sido desativada)
        if (playerForeground != null && !playerForeground.gameObject.activeSelf)
            playerForeground.gameObject.SetActive(true);
        if (playerBackground != null && !playerBackground.gameObject.activeSelf)
            playerBackground.gameObject.SetActive(true);

        // Verifica também se as câmeras virtuais apontam para instâncias reais da cena ativa (mesmo se estiverem dentro de pastas)
        bool vcamFgInvalida = vcamForeground == null || !vcamForeground.scene.IsValid() || vcamForeground.scene != activeScene;
        bool vcamBgInvalida = vcamBackground == null || !vcamBackground.scene.IsValid() || vcamBackground.scene != activeScene;

        if (vcamFgInvalida || vcamBgInvalida)
        {
            foreach (GameObject rootObj in activeScene.GetRootGameObjects())
            {
                Transform[] allTransforms = rootObj.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    if (t.name.Contains("vcam_Foreground") && vcamFgInvalida)
                    {
                        vcamForeground = t.gameObject;
                        vcamFgInvalida = false;
                    }
                    else if (t.name.Contains("vcam_Background") && vcamBgInvalida)
                    {
                        vcamBackground = t.gameObject;
                        vcamBgInvalida = false;
                    }
                }
            }
        }

        // Garante que o TrackingTarget de cada vcam aponte para a toupeira real da cena (e não para o Prefab)
        if (vcamForeground != null && playerForeground != null)
            VincularAlvoDaCameraVirtual(vcamForeground, playerForeground.transform);

        if (vcamBackground != null && playerBackground != null)
            VincularAlvoDaCameraVirtual(vcamBackground, playerBackground.transform);
    }

    private void UpdateCharacterStates(bool snapCameraToTarget = false)
    {
        PlayerMovement activePlayer = isForegroundActive ? playerForeground : playerBackground;
        PlayerMovement inactivePlayer = isForegroundActive ? playerBackground : playerForeground;

        // 1. Prepara e acorda a toupeira que vai assumir o controle (mesmo se estiver a 400+ unidades de distância!)
        if (activePlayer != null)
        {
            if (!activePlayer.gameObject.activeSelf)
                activePlayer.gameObject.SetActive(true);

            activePlayer.canMove = true;
            activePlayer.enabled = true;

            if (activePlayer.TryGetComponent<Rigidbody2D>(out Rigidbody2D rbActive))
            {
                rbActive.simulated = true;
                rbActive.constraints = RigidbodyConstraints2D.FreezeRotation;
                rbActive.WakeUp();
            }
        }

        // 2. Pausa a toupeira inativa e trava seu eixo X para ela não escorregar para a lava ou cair de plataformas longe da câmera
        if (inactivePlayer != null)
        {
            inactivePlayer.enabled = false;

            if (inactivePlayer.TryGetComponent<Rigidbody2D>(out Rigidbody2D rbInactive))
            {
                rbInactive.linearVelocity = new Vector2(0f, rbInactive.linearVelocity.y);
                rbInactive.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }
        }

        // 3. Alterna as câmeras do Cinemachine ativando a NOVA antes de desativar a ANTIGA
        // (evita que o CinemachineBrain fique sem câmera ativa ou trave a grandes distâncias)
        GameObject vcamAtiva = isForegroundActive ? vcamForeground : vcamBackground;
        GameObject vcamInativa = isForegroundActive ? vcamBackground : vcamForeground;

        if (vcamAtiva != null)
        {
            if (snapCameraToTarget && activePlayer != null)
            {
                SincronizarCameraComAlvo(vcamAtiva, activePlayer.transform);
            }

            vcamAtiva.SetActive(true);
        }

        if (vcamInativa != null)
        {
            vcamInativa.SetActive(false);
        }
    }

    /// <summary>
    /// Move a câmera virtual para perto da toupeira alvo antes de ativá-la e avisa o Cinemachine,
    /// garantindo que a transição funcione mesmo quando as duas toupeiras estão a mais de 400 unidades de distância!
    /// </summary>
    private void SincronizarCameraComAlvo(GameObject vcamObj, Transform targetTransform)
    {
        if (vcamObj == null || targetTransform == null) return;

        Vector3 oldPos = vcamObj.transform.position;
        Vector3 newPos = new Vector3(targetTransform.position.x, targetTransform.position.y, oldPos.z);
        vcamObj.transform.position = newPos;

        VincularAlvoDaCameraVirtual(vcamObj, targetTransform);

        // Reseta o estado interno de amortecimento (Damping) do CinemachineCamera caso exista
        Component cmCam = vcamObj.GetComponent("CinemachineCamera");
        if (cmCam != null)
        {
            PropertyInfo prevStateProp = cmCam.GetType().GetProperty("PreviousStateIsValid", BindingFlags.Public | BindingFlags.Instance);
            if (prevStateProp != null && prevStateProp.CanWrite)
            {
                prevStateProp.SetValue(cmCam, false);
            }
        }
    }

    /// <summary>
    /// Garante via Reflection (compatível com Cinemachine 2 e Cinemachine 3) que o Follow / TrackingTarget
    /// da câmera virtual aponte sempre para o Transform da toupeira instanciada na cena.
    /// </summary>
    private void VincularAlvoDaCameraVirtual(GameObject vcamObj, Transform targetTransform)
    {
        if (vcamObj == null || targetTransform == null) return;

        Component cmCam = vcamObj.GetComponent("CinemachineCamera");
        if (cmCam != null)
        {
            PropertyInfo followProp = cmCam.GetType().GetProperty("Follow", BindingFlags.Public | BindingFlags.Instance);
            if (followProp != null && followProp.CanWrite)
            {
                Transform currentFollow = followProp.GetValue(cmCam) as Transform;
                if (currentFollow == null || !currentFollow.gameObject.scene.IsValid())
                {
                    followProp.SetValue(cmCam, targetTransform);
                }
            }
        }
    }
}