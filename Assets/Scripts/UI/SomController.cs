using UnityEngine;
using UnityEngine.UI;

public class SomController : MonoBehaviour
{
    [Header("Configuração")]
    public Button botaoSom;
    public Image imagemDoIcone;

    [Header("Sprites")]
    public Sprite iconeSomLigado;
    public Sprite iconeSomDesligado;

    private bool estaMudo = false;

    void Start()
    {
        if (PlayerPrefs.HasKey("Mudo"))
        {
            estaMudo = PlayerPrefs.GetInt("Mudo") == 1;
        }
        else
        {
            estaMudo = false;
        }

        AtualizarEstadoAudio();

        if (botaoSom != null)
        {
            botaoSom.onClick.AddListener(AlternarSom);
        }
    }

    public void AlternarSom()
    {
        estaMudo = !estaMudo;

        PlayerPrefs.SetInt("Mudo", estaMudo ? 1 : 0);

        AtualizarEstadoAudio();
    }

    void AtualizarEstadoAudio()
    {
        if (estaMudo)
        {
            AudioListener.volume = 0;
            if (imagemDoIcone != null) imagemDoIcone.sprite = iconeSomDesligado;
        }
        else
        {
            AudioListener.volume = 1;

            if (imagemDoIcone != null) imagemDoIcone.sprite = iconeSomLigado;
        }
    }
}
