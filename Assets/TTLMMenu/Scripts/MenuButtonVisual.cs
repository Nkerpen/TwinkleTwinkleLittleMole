using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))]
public class MenuButtonVisual : MonoBehaviour, IPointerEnterHandler
{
    [Header("Artes do botao")]
    [SerializeField] private Sprite spriteAP; // Joia cinza
    [SerializeField] private Sprite spriteES; // Joia vermelha

    [Header("Animacao")]
    [SerializeField, Range(1f, 1.25f)] private float escalaSelecionado = 1.07f;
    [SerializeField, Min(0.1f)] private float velocidade = 12f;

    private Image imagem;
    private Button botao;
    private Vector3 escalaOriginal;
    private bool estavaSelecionado;

    private void Awake()
    {
        imagem = GetComponent<Image>();
        botao = GetComponent<Button>();
        escalaOriginal = transform.localScale;
        if (spriteAP != null) imagem.sprite = spriteAP;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || !botao.interactable) return;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

    private void Update()
    {
        bool selecionado = botao.interactable &&
            EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == gameObject;

        if (selecionado != estavaSelecionado)
        {
            estavaSelecionado = selecionado;
            Sprite escolhido = selecionado ? spriteES : spriteAP;
            if (escolhido != null) imagem.sprite = escolhido;
        }

        Vector3 escalaAlvo = escalaOriginal * (selecionado ? escalaSelecionado : 1f);
        float passo = 1f - Mathf.Exp(-velocidade * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, escalaAlvo, passo);
    }

    private void OnDisable()
    {
        transform.localScale = escalaOriginal;
        estavaSelecionado = false;
        if (imagem != null && spriteAP != null) imagem.sprite = spriteAP;
    }
}
