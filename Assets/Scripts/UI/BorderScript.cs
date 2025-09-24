using UnityEngine;

public class BorderScript : MonoBehaviour
{
    public SpriteRenderer spriteRend;

    void Start()
    {
        if (spriteRend == null)
            spriteRend = GetComponent<SpriteRenderer>();
        spriteRend.color = Color.white; // Cor padrão branca

        spriteRend.enabled = false;
    }

    public void AlternaVisibilidade()
    {
        spriteRend.enabled = !spriteRend.enabled;
    }

    // Novo método para mudar cor
    public void MudarCor(Color novaCor)
    {
        if (spriteRend != null)
            spriteRend.color = novaCor;
    }
}
