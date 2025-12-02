using UnityEngine;
using System.Collections.Generic;

// Classe estática para guardar os dados entre cenas
public static class GameLaunchData
{
    public struct PlayerConfig
    {
        public string nome;
        public bool ehIA;
        public Color cor;
        public string nomeDaCor; // Ex: "Vermelho"
    }

    // Lista que o Menu vai preencher e o GameManager vai ler
    public static List<PlayerConfig> configuracaoJogadores = new List<PlayerConfig>();

    public static void LimparConfiguracoes()
    {
        configuracaoJogadores.Clear();
    }
}