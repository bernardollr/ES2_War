using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MapSelector : MonoBehaviour
{
    [System.Serializable]
    public class MapData
    {
        public string nomeVisual; //O que aparece escrito
        public Sprite imagePreview; //A foto do mapa
        public string nomeDaCena;
        public bool emBreve;
        [TextArea] public string descricao;
    }

    [Header("Configuração dos Mapas")]
    public List<MapData> mapasDisponiveis;

    [Header("Referências da UI")]
    public GameObject overlayJanela;
    public Image displayImagemMapa;
    public TextMeshProUGUI displayNomeMapa;
    public Button botaoIniciar;
    public TextMeshProUGUI textoBotaoIniciar;

    [Header("Configuração do Tooltip")]
    public GameObject janelaDescricao;
    public TextMeshProUGUI textoDescricao;

    private int indiceAtual = 0;
    void Start()
    {
        indiceAtual = 0;
        AtualizarUI();
    }

    public void MudarMapa(int direcao)
    {
        indiceAtual += direcao;
        if (indiceAtual >= mapasDisponiveis.Count)
            indiceAtual = 0;
        else if (indiceAtual < 0)
            indiceAtual = mapasDisponiveis.Count - 1;

        AtualizarUI();
    }

    void AtualizarUI()
    {
        if (mapasDisponiveis.Count == 0) return;

        MapData mapa = mapasDisponiveis[indiceAtual];

        displayNomeMapa.text = mapa.nomeVisual;

        if(mapa.imagePreview != null)
        {
            displayImagemMapa.sprite = mapa.imagePreview;
            displayImagemMapa.enabled = true;
        }
        else
        {
            displayImagemMapa.enabled = false;
        }

        if (mapa.emBreve)
        {
            botaoIniciar.interactable = false;
            if (textoBotaoIniciar) textoBotaoIniciar.text = "EM BREVE";
            displayNomeMapa.text += "(Em Breve)";
        }
        else
        {
            botaoIniciar.interactable = true;
            if (textoBotaoIniciar) textoBotaoIniciar.text = "INICIAR BATALHA";
        }
    }


    public void IniciarBatalha()
    {
        MapData mapa = mapasDisponiveis[indiceAtual];

        if (!mapa.emBreve)
        {
            Debug.Log("Carregando mapa:" + mapa.nomeDaCena);
            SceneManager.LoadScene(mapa.nomeDaCena);
        }
    }

    public void MostrarDescricao()
    {
        string texto = mapasDisponiveis[indiceAtual].descricao;

        if (string.IsNullOrEmpty(texto)) return;

        textoDescricao.text = texto;
        janelaDescricao.SetActive(true);
    }

    public void EsconderDescricao()
    {
        janelaDescricao.SetActive(false);
    }

    public void AbrirSelecaoDeMapa()
    {
        if(overlayJanela!=null)
            overlayJanela.SetActive(true);
    }

}
