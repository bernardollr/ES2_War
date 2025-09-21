using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton simples

    public Player jogador1;
    public Player jogador2;
    public Player jogadorAtual;

    void Awake()
    {
        // Garante que só exista 1 GM
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Cria jogadores com cores diferentes
        jogador1 = new Player("Jogador 1", Color.blue);
        jogador2 = new Player("Jogador 2", Color.red);

        jogadorAtual = jogador1;

        Debug.Log("GameManager iniciado. Turno de: " + jogadorAtual.nome);
    }

    public void TrocarTurno()
    {
        jogadorAtual = (jogadorAtual == jogador1) ? jogador2 : jogador1;
        Debug.Log("Agora é o turno de: " + jogadorAtual.nome);
    }
}