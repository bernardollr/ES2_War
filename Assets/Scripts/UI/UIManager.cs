using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pop-ups")]
    public GameObject popUpObjetivo;
    public TextMeshProUGUI textoObjetivo;

    public GameObject popUpCartas;
    public Transform containerCartasGrid;
    public GameObject prefabCartaUI;

    [Header("DEBUG: Objetivo Atual")]
    public string objetivoAtualDebug = "Conquistar 18 territórios.";

    public void AbrirPopUpObjetivo() {
        popUpObjetivo.SetActive(true);
        textoObjetivo.text = objetivoAtualDebug;
    }

    public void FecharPopUpObjetivo() {
        popUpObjetivo.SetActive(false);
    }

    public void AbrirPopUpCartas() {
        popUpCartas.SetActive(true);
        foreach (Transform child in containerCartasGrid) {
            Destroy(child.gameObject);
        }

        AdicionarCartasUI("Brasil");
        AdicionarCartasUI("África");
        AdicionarCartasUI("Brasil");
    }

    void AdicionarCartasUI(string nomeTerritorio) {
        GameObject novaCarta = Instantiate(prefabCartaUI, containerCartasGrid);
    }

    public void FecharPopUpCartas()
    {
        popUpCartas.SetActive(false);
    }
}
