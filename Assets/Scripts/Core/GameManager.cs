using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton simples

    public Player jogador1;
    public Player jogador2;
    public Player jogadorAtual;

    public List<TerritorioHandler> todosOsTerritorios;

    void Awake()
    {
        // Garante que s� exista 1 GM
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Cria jogadores com cores diferentes
        jogador1 = new Player("Jogador 1", Color.blue);
        jogador2 = new Player("Jogador 2", Color.red);

        jogadorAtual = jogador1;

        // --- NOVO: Encontra e distribui os territ�rios no in�cio do jogo ---
        // Busca na cena todos os objetos que possuem o script TerritorioHandler (forma moderna e otimizada).
        todosOsTerritorios = FindObjectsByType<TerritorioHandler>(FindObjectsSortMode.None).ToList();
        DistribuirTerritoriosIniciais(); // Chama o novo m�todo de distribui��o.

        Debug.Log("GameManager iniciado. Turno de: " + jogadorAtual.nome);
    }

    public void TrocarTurno()
    {
        if (jogadorAtual == jogador1)
        {
            // Se o jogador atual era o jogador 1, mude para o jogador 2.
            jogadorAtual = jogador2;
        }
        else
        {
            // Sen�o (se era o jogador 2), mude para o jogador 1.
            jogadorAtual = jogador1;
        }
        Debug.Log("Agora � o turno de: " + jogadorAtual.nome);
    }

    void DistribuirTerritoriosIniciais()
    {
        // Cria uma nova lista com os territ�rios em ordem aleat�ria.
        List<TerritorioHandler> territoriosEmbaralhados = todosOsTerritorios.OrderBy(a => Random.value).ToList();

        int jogadorIndex = 0;
        foreach (var territorio in territoriosEmbaralhados)
        {
            // Define o dono do territ�rio, alternando entre jogador 1 e 2.
            Player dono = (jogadorIndex % 2 == 0) ? jogador1 : jogador2;

            // ATEN��O: As 3 linhas abaixo dependem de c�digo a ser adicionado no TerritorioHandler.
            territorio.donoDoTerritorio = dono;
            territorio.numeroDeTropas = 1;
            territorio.AtualizarVisual();

            jogadorIndex++;
        }

        Debug.Log("Territ�rios iniciais distribu�dos!");
    }
}