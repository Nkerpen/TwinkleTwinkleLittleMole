using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitcher : MonoBehaviour
{
    private static readonly int PlanoAtivoID = Shader.PropertyToID("_PlanoAtivo");

    [Header("Input System")]
    [SerializeField, Tooltip("Referência para a ação de trocar de personagem")]
    private InputActionReference switchAction;

    [Header("Personagens")]
    [SerializeField] private PlayerMovement playerForeground;
    [SerializeField] private PlayerMovement playerBackground;

    [Header("Câmeras")]
    [SerializeField] private GameObject vcamForeground;
    [SerializeField] private GameObject vcamBackground;

    [Header("Shader Cinematográfico Dimensional")]
    [SerializeField] private float shaderTransitionDuration = 0.35f;

    private bool isForegroundActive = true;
    private float currentPlanoAtivoValue = 0f;
    private Coroutine shaderTransitionRoutine;

    private void OnEnable()
    {
        if (switchAction != null)
        {
            switchAction.action.performed += OnSwitchPerformed;
            switchAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (switchAction != null)
        {
            switchAction.action.performed -= OnSwitchPerformed;
            switchAction.action.Disable();
        }
    }

    private void Start()
    {
        currentPlanoAtivoValue = isForegroundActive ? 0f : 1f;
        Shader.SetGlobalFloat(PlanoAtivoID, currentPlanoAtivoValue);
        UpdateCharacterStates();
    }

    private void OnSwitchPerformed(InputAction.CallbackContext context)
    {
        if (playerBackground == null) return;

        isForegroundActive = !isForegroundActive;
        UpdateCharacterStates();
    }

    private void UpdateCharacterStates()
    {
        // 1. Alterna os controles dos personagens, verificando se eles existem na cena
        if (playerForeground != null) playerForeground.enabled = isForegroundActive;
        if (playerBackground != null) playerBackground.enabled = !isForegroundActive;

        // 2. Zera a inércia do personagem que foi desativado
        if (isForegroundActive)
        {
            if (playerBackground != null)
                playerBackground.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
        else
        {
            if (playerForeground != null)
                playerForeground.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        // 3. Alterna as câmeras do Cinemachine
        if (vcamForeground != null) vcamForeground.SetActive(isForegroundActive);
        if (vcamBackground != null) vcamBackground.SetActive(!isForegroundActive);

        // 4. Atualiza globalmente _PlanoAtivo no Shader (0 = Frente em foco, 1 = Fundo em foco)
        float targetValue = isForegroundActive ? 0f : 1f;
        if (shaderTransitionRoutine != null) StopCoroutine(shaderTransitionRoutine);
        shaderTransitionRoutine = StartCoroutine(AnimatePlanoAtivoShader(targetValue));
    }

    private IEnumerator AnimatePlanoAtivoShader(float target)
    {
        float start = currentPlanoAtivoValue;
        float elapsed = 0f;

        while (elapsed < shaderTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / shaderTransitionDuration);
            currentPlanoAtivoValue = Mathf.Lerp(start, target, t);
            Shader.SetGlobalFloat(PlanoAtivoID, currentPlanoAtivoValue);
            yield return null;
        }

        currentPlanoAtivoValue = target;
        Shader.SetGlobalFloat(PlanoAtivoID, currentPlanoAtivoValue);
    }
}