using UnityEngine;

public class BorderScript : MonoBehaviour
{
    public SpriteRenderer spriteRend;

    void Start()
    {
        if (spriteRend == null)
            spriteRend = GetComponent<SpriteRenderer>();
        spriteRend.color = Color.white; // Cor padr�o branca

        spriteRend.enabled = false;
    }

    public void AlternaVisibilidade(bool visivel)
    {
        spriteRend.enabled = visivel;
    }

    // Novo m�todo para mudar cor
    public void MudarCor(Color novaCor)
    {
        if (spriteRend != null)
            spriteRend.color = novaCor;
    }
}
