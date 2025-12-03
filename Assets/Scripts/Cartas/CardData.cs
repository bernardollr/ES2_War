using UnityEngine;

// Apagamos o enum SimboloWar. Vamos usar o Simbolo do Carta.cs

[CreateAssetMenu(fileName = "NovaCartaWar", menuName = "War/Carta de Territorio")]
public class CardData : ScriptableObject
{
    [Header("Dados Lógicos")]
    public string nomeTerritorio; 
    
    // AGORA USAMOS O ENUM ORIGINAL (Quadrado, Triangulo, Circulo)
    // Isso garante que o visual e a lógica falem a mesma língua.
    public Simbolo simbolo;    

    [Header("Visual")]
    public Sprite arteCompleta;
}