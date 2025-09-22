using UnityEngine;

public class ExercitoHandler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    // 1. O campo privado para armazenar o dono. Ningu�m de fora pode acess�-lo diretamente.
    private Player dono;

    // 2. A PROPRIEDADE P�BLICA (o "Getter")
    // Outros scripts poder�o ler "meuExercito.Dono" para saber quem � o dono.
    public Player Dono
    {
        get { return dono; }
        set
        {
            dono = value;
            //AtualizarCorDoSprite(); // <-- A L�GICA EXTRA
        }
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

}