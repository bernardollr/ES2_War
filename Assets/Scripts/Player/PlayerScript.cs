using UnityEngine;
using System.Collections.Generic; // <--- Necessário para List

[System.Serializable]
public class Player
{
    public string nome;
    public Color cor;
    public string nomeColorido;
    public string nomeDaCor;
    public Objetivo objetivoSecreto;
    public bool ehIA;

    // --- NOVO: CARTAS ---
    public List<Carta> maoDeCartas; 
    public bool conquistouTerritorioNesteTurno; // Resetar isso no início de cada turno
    // --------------------

    public Player(string nome, Color cor, string nomeDaCor, bool ehIA = false)
    {
        this.nome = nome;
        this.cor = cor;
        this.nomeDaCor = nomeDaCor;
        this.objetivoSecreto = null;
        this.ehIA = ehIA;

        string hexCor = ColorUtility.ToHtmlStringRGB(cor);
        this.nomeColorido = $"<color=#{hexCor}>{nome}</color>";
        
        // Inicializa a lista
        this.maoDeCartas = new List<Carta>();
        this.conquistouTerritorioNesteTurno = false;
    }
}