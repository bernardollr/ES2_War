// Player.cs
using UnityEngine;

[System.Serializable]
public class Player
{
    public string nome;
    public Color cor;
    public string nomeColorido; // Para uso no TextMeshPro

    public string nomeDaCor; // <-- ADICIONE ESTA LINHA (para lógica interna)

    public Objetivo objetivoSecreto;

    public bool ehIA;

    // Construtor atualizado para receber o nome da cor
    public Player(string nome, Color cor, string nomeDaCor, bool ehIA = false) // <-- ADICIONE AQUI
    {
        this.nome = nome;
        this.cor = cor;
        this.nomeDaCor = nomeDaCor; // <-- ADICIONE ESTA LINHA
        this.objetivoSecreto = null;
        this.ehIA = ehIA;

        // Sua lógica de nome colorido continua perfeita
        string hexCor = ColorUtility.ToHtmlStringRGB(cor);
        this.nomeColorido = $"<color=#{hexCor}>{nome}</color>";
    }

    public override string ToString()
    {
        return $"[Player: {nome} - Cor: {nomeDaCor}]";
    }
}