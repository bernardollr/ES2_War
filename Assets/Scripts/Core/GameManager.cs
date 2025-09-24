// GameManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Player jogador1;
    public Player jogador2;
    public Player jogadorAtual;

    public List<TerritorioHandler> todosOsTerritorios;

    public TerritorioHandler territorio;

    public TextMeshProUGUI turnoText; // arraste o Text do Canvas aqui pelo Inspector

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        jogador1 = new Player("Jogador 1", Color.blue);
        jogador2 = new Player("Jogador 2", Color.red);

        jogadorAtual = jogador1; // come�a sempre pelo jogador 1

        AtualizarTextoDoTurno();

        todosOsTerritorios = FindObjectsByType<TerritorioHandler>(FindObjectsSortMode.None).ToList();
        DistribuirTerritoriosIniciais();

        // Atualiza os territ�rios com o jogador do turno atual
        AtualizarPlayerDoTurnoNosTerritorios();

        Debug.Log("GameManager iniciado. Turno de: " + jogadorAtual.nome);
        PrintTerritoriosPorJogador();
    }
    public void AtualizarTextoDoTurno()
    {
        if (turnoText != null)
        {
            turnoText.text = "Turno do: " + jogadorAtual.nomeColorido;
        }
    }
    void DistribuirTerritoriosIniciais()
    {
        List<TerritorioHandler> territoriosEmbaralhados = todosOsTerritorios.OrderBy(a => Random.value).ToList();

        int jogadorIndex = 0;
        foreach (var territorio in territoriosEmbaralhados)
        {
            Player dono = (jogadorIndex % 2 == 0) ? jogador1 : jogador2;

            territorio.donoDoTerritorio = dono;
            territorio.numeroDeTropas = 1;
            territorio.AtualizarVisual();

            Debug.Log($"[Distribui��o] Territ�rio '{territorio.name}' atribu�do a {dono.nome}");

            jogadorIndex++;
        }

        Debug.Log("Territ�rios iniciais distribu�dos!");
    }

    void PrintTerritoriosPorJogador()
    {
        Debug.Log($"=== Territ�rios do {jogador1.nome} ===");
        foreach (var t in todosOsTerritorios.Where(t => t.donoDoTerritorio == jogador1))
        {
            Debug.Log($"- {t.name} com {t.numeroDeTropas} tropa(s)");
        }

        Debug.Log($"=== Territ�rios do {jogador2.nome} ===");
        foreach (var t in todosOsTerritorios.Where(t => t.donoDoTerritorio == jogador2))
        {
            Debug.Log($"- {t.name} com {t.numeroDeTropas} tropa(s)");
        }
    }

    public void TrocarTurno()
    {
        // Troca o jogador atual
        jogadorAtual = (jogadorAtual == jogador1) ? jogador2 : jogador1;
        Debug.Log("Agora � o turno de: " + jogadorAtual.nome);

        // Deseleciona todos os territ�rios selecionados (se houver)
        TerritorioHandler.DesselecionarTodos();

        // Atualiza todos os territ�rios com o novo jogador do turno
        AtualizarPlayerDoTurnoNosTerritorios();

        AtualizarTextoDoTurno();
        ChecarVitoria();
    }

    void AtualizarPlayerDoTurnoNosTerritorios()
    {
        foreach (var territorio in todosOsTerritorios)
        {
            territorio.playerDoTurno = jogadorAtual;
        }
    }

    public void ChecarVitoria()
    {
        if (todosOsTerritorios.Count == 0) return;

        Player donoReferencia = todosOsTerritorios[0].donoDoTerritorio;

        foreach (var territorio in todosOsTerritorios)
        {
            if (territorio.donoDoTerritorio != donoReferencia)
                return; // ainda h� territ�rios de outros jogadores
        }

        // Todos os territ�rios s�o do mesmo jogador
        Debug.Log("Jogo acabou! Vencedor: " + donoReferencia.nome);

        // Salva o vencedor
        VencedorInfo.nomeVencedor = donoReferencia.nome;
        VencedorInfo.corVencedor = donoReferencia.cor;

        // Carrega a cena de fim
        SceneManager.LoadScene(2); // substitua pelo nome da sua cena de fim
    }

}
