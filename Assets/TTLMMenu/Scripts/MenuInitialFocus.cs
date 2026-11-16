using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuInitialFocus : MonoBehaviour
{
    [SerializeField] private Button primeiroBotao;

    private IEnumerator Start()
    {
        yield return null; // Espera o EventSystem iniciar
        if (EventSystem.current != null && primeiroBotao != null && primeiroBotao.interactable)
            EventSystem.current.SetSelectedGameObject(primeiroBotao.gameObject);
    }
}
