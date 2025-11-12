using UnityEngine;

public abstract class Objetivo
{
    public string Descricao { get; protected set; }

    public Objetivo(string descricao)
    {
        this.Descricao = descricao;
    }

    // O método "cérebro"
    // Cada objetivo específico (filho) vai implementar sua própria lógica aqui.
    // Ele recebe o jogador que o possui e o estado do jogo (GameManager).
    public abstract bool FoiConcluido(Player jogador, GameManager gameManager);
}