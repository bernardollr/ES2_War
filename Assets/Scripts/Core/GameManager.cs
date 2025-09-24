// GameManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Player jogador1;
    public Player jogador2;
    public Player jogadorAtual;

    public List<TerritorioHandler> todosOsTerritorios;

    public TerritorioHandler territorio;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        jogador1 = new Player("Jogador 1", Color.blue);
        jogador2 = new Player("Jogador 2", Color.red);

        jogadorAtual = jogador1; // começa sempre pelo jogador 1

        todosOsTerritorios = FindObjectsByType<TerritorioHandler>(FindObjectsSortMode.None).ToList();
        DistribuirTerritoriosIniciais();

        // Atualiza os territórios com o jogador do turno atual
        AtualizarPlayerDoTurnoNosTerritorios();

        Debug.Log("GameManager iniciado. Turno de: " + jogadorAtual.nome);
        PrintTerritoriosPorJogador();
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

            Debug.Log($"[Distribuição] Território '{territorio.name}' atribuído a {dono.nome}");

            jogadorIndex++;
        }

        Debug.Log("Territórios iniciais distribuídos!");
    }

    void PrintTerritoriosPorJogador()
    {
        Debug.Log($"=== Territórios do {jogador1.nome} ===");
        foreach (var t in todosOsTerritorios.Where(t => t.donoDoTerritorio == jogador1))
        {
            Debug.Log($"- {t.name} com {t.numeroDeTropas} tropa(s)");
        }

        Debug.Log($"=== Territórios do {jogador2.nome} ===");
        foreach (var t in todosOsTerritorios.Where(t => t.donoDoTerritorio == jogador2))
        {
            Debug.Log($"- {t.name} com {t.numeroDeTropas} tropa(s)");
        }
    }

    public void TrocarTurno()
    {
        // Troca o jogador atual
        jogadorAtual = (jogadorAtual == jogador1) ? jogador2 : jogador1;
        Debug.Log("Agora é o turno de: " + jogadorAtual.nome);

        // Deseleciona todos os territórios selecionados (se houver)
        TerritorioHandler.DesselecionarTodos();

        // Atualiza todos os territórios com o novo jogador do turno
        AtualizarPlayerDoTurnoNosTerritorios();
    }

    void AtualizarPlayerDoTurnoNosTerritorios()
    {
        foreach (var territorio in todosOsTerritorios)
        {
            territorio.playerDoTurno = jogadorAtual;
        }
    }

}
