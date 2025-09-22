using UnityEngine;

[System.Serializable]
public class Player
{
    public string nome;
    public Color cor;

    public Player(string nome, Color cor)
    {
        this.nome = nome;
        this.cor = cor;
    }
}