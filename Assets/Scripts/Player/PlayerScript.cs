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

    public string nomeColorido
    {
        get
        {
            string corHex = ColorUtility.ToHtmlStringRGB(cor); // converte Color para hexadecimal
            string nomeCor;
            if (cor == Color.blue) nomeCor = "Azul";
            else if (cor == Color.red) nomeCor = "Vermelho";
            else nomeCor = nome;

            return $"<color=#{corHex}>{nomeCor}</color>";
        }
    }
}