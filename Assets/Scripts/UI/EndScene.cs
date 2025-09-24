using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndScene : MonoBehaviour
{
    public TextMeshProUGUI vencedorText; // arraste o Text do Canvas no Inspector

    void Start()
    {
        if (vencedorText != null)
        {
            string corHex = ColorUtility.ToHtmlStringRGB(VencedorInfo.corVencedor);
            vencedorText.text = $"Vencedor: <color=#{corHex}>{VencedorInfo.nomeVencedor}</color>";
        }
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void LoadMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
