using UnityEngine;

public enum Simbolo
{
    Quadrado,
    Triangulo,
    Circulo,
    Coringa
}

[System.Serializable]
public class Carta
{
    public Simbolo simbolo;
    public TerritorioHandler territorioAssociado; // Referência direta ao território da Unity

    // Construtor
    public Carta(Simbolo s, TerritorioHandler t)
    {
        simbolo = s;
        territorioAssociado = t;
    }
}