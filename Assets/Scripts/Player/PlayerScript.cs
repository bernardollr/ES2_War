// Player.cs
using UnityEngine;

[System.Serializable]
public class Player
{
    public string nome;
    public Color cor;
    public string nomeColorido; // Para uso no TextMeshPro

    public Player(string nome, Color cor)
    {
        this.nome = nome;
        this.cor = cor;

        // Cria uma string rica em cor para a UI
        string hexCor = ColorUtility.ToHtmlStringRGB(cor);
        this.nomeColorido = $"<color=#{hexCor}>{nome}</color>";
    }
}