using UnityEngine;

public class ExercitoHandler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    // 1. O campo privado para armazenar o dono. Ninguém de fora pode acessá-lo diretamente.
    private Player dono;

    // 2. A PROPRIEDADE PÚBLICA (o "Getter")
    // Outros scripts poderão ler "meuExercito.Dono" para saber quem é o dono.
    public Player Dono
    {
        get { return dono; }
        set
        {
            dono = value;
            //AtualizarCorDoSprite(); // <-- A LÓGICA EXTRA
        }
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

}