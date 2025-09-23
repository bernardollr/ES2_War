using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class BorderScript : MonoBehaviour

{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public SpriteRenderer spriteRend;
    void Start()
    {
        if (spriteRend == null)
            spriteRend = GetComponent<SpriteRenderer>(); // pega o SpriteRenderer do mesmo GameObject

        spriteRend.enabled = false;
    }
    public void AlternaVisibilidade()
    {
        spriteRend.enabled = !spriteRend.enabled;
    }
}
