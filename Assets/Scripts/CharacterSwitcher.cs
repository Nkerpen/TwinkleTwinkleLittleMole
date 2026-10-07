using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField, Tooltip("Referência para a ação de trocar de personagem")]
    private InputActionReference switchAction;

    [Header("Personagens")]
    [SerializeField] private PlayerMovement playerForeground;
    [SerializeField] private PlayerMovement playerBackground;

    [Header("Câmeras")]
    [SerializeField] private GameObject vcamForeground;
    [SerializeField] private GameObject vcamBackground;

    private bool isForegroundActive = true; 

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
        UpdateCharacterStates();
    }

    private void OnSwitchPerformed(InputAction.CallbackContext context)
    {
        isForegroundActive = !isForegroundActive;
        UpdateCharacterStates();
    }

    private void UpdateCharacterStates()
    {
        // 1. Alterna os controles dos personagens
        playerForeground.enabled = isForegroundActive;
        playerBackground.enabled = !isForegroundActive;

        // 2. Zera a inércia do personagem que foi desativado
        if (isForegroundActive)
        {
            playerBackground.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
        else
        {
            playerForeground.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        // 3. Alterna as câmeras do Cinemachine
        if (vcamForeground != null) vcamForeground.SetActive(isForegroundActive);
        if (vcamBackground != null) vcamBackground.SetActive(!isForegroundActive);
    }
}