using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Pop-ups")]
    public GameObject popUpObjetivo;
    public TextMeshProUGUI textoObjetivo;

    public GameObject popUpCartas;
    public Transform containerCartasGrid;
    public GameObject prefabCartaUI;

    [Header("Painel de Status do Turno")]
    public GameObject painelStatusTurno;
    public Image iconeStatusDeploy;
    public Image iconeStatusAttack;
    public Image iconeStatusMove;
    public Image iconeSoldier;
    public TextMeshProUGUI textoStatus;
    public TextMeshProUGUI textoNomeJogador;
    public TextMeshProUGUI textoBotaoAvancarFase;

    [Header("DEBUG: Objetivo Atual")]
    public string objetivoAtualDebug = "Conquistar 18 territórios.";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }

        if (painelStatusTurno != null) {
            painelStatusTurno.SetActive(false);
        }
    }
    public void AbrirPopUpObjetivo()
    {
        popUpObjetivo.SetActive(true);
        // Puxa a descrição do objetivo secreto do jogador atual direto do GameManager
        if (GameManager.instance.jogadorAtual.objetivoSecreto != null)
             textoObjetivo.text = GameManager.instance.jogadorAtual.objetivoSecreto.Descricao;
    }

    public void FecharPopUpObjetivo() {
        popUpObjetivo.SetActive(false);
    }

    // --- AQUI ESTAVA O PROBLEMA ---
    public void AbrirPopUpCartas() {
        popUpCartas.SetActive(true);
        
        // COMENTEI ESTE BLOCO:
        // Não devemos destruir as cartas aqui, pois o DeckManager 
        // já preparou elas no início do turno.
        /*
        foreach (Transform child in containerCartasGrid) {
            Destroy(child.gameObject);
        }
        */
    }

    /*void AdicionarCartasUI(string nomeTerritorio) {
        GameObject novaCarta = Instantiate(prefabCartaUI, containerCartasGrid);
    }*/

    public void FecharPopUpCartas()
    {
        popUpCartas.SetActive(false);
    }

    public void TogglePainelStatus() {
        if (painelStatusTurno == null) return;

        bool estadoAtual = painelStatusTurno.activeSelf;
        painelStatusTurno.SetActive(!estadoAtual);
    }

    public void AtualizarPainelStatus(GameManager.GamePhase faseAtual, Player jogador)
    {
        textoNomeJogador.text = jogador.nome;
        Color corDoJogador = jogador.cor;

        Color corInativa = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        iconeStatusDeploy.color = corInativa;
        iconeStatusAttack.color = corInativa;
        iconeStatusMove.color = corInativa;
        iconeSoldier.color = corDoJogador;

        switch (faseAtual) {
            case GameManager.GamePhase.Alocacao:
                iconeStatusDeploy.color = corDoJogador;
                textoStatus.text = "Alocando Tropas";
                if (textoBotaoAvancarFase != null)
                    textoBotaoAvancarFase.text = "Atacar";
                break;

            case GameManager.GamePhase.Ataque:
                iconeStatusAttack.color = corDoJogador;
                textoStatus.text = "Fase de Ataque";
                if (textoBotaoAvancarFase != null)
                    textoBotaoAvancarFase.text = "Mover Tropas";
                break;

            case GameManager.GamePhase.Remanejamento:
                iconeStatusMove.color = corDoJogador;
                textoStatus.text = "Movimentando Tropas";
                if (textoBotaoAvancarFase != null)
                    textoBotaoAvancarFase.text = "Passar Turno";
                break;

            case GameManager.GamePhase.JogoPausado:
                textoStatus.text = "EM BATALHA...";
                break;
        }       

    }
}