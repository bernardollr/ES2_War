// TerritorioHandler.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TerritorioHandler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public List<TerritorioHandler> vizinhos;

    [Header("Dados do Jogo")]
    public Player donoDoTerritorio;
    public int numeroDeTropas;

    public BorderScript borderScript;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        FindAndStoreNeighbors();
    }

    void FindAndStoreNeighbors()
    {
        vizinhos = new List<TerritorioHandler>();
        List<Collider2D> collidingColliders = new List<Collider2D>();
        ContactFilter2D filter = ContactFilter2D.noFilter;
        GetComponent<Collider2D>().Overlap(filter, collidingColliders);

        foreach (var collider in collidingColliders)
        {
            if (collider.gameObject == gameObject) continue;
            TerritorioHandler neighbor = collider.GetComponent<TerritorioHandler>();
            if (neighbor != null) vizinhos.Add(neighbor);
        }
    }

    public void AtualizarVisual()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (donoDoTerritorio != null)
        {
            spriteRenderer.color = donoDoTerritorio.cor;
            Debug.Log($"[Visual] Território '{name}' agora é do {donoDoTerritorio.nome} e pintado de {donoDoTerritorio.cor}");
        }
        else
        {
            spriteRenderer.color = Color.gray;
        }
    }

    void Update()
    {
        CliqueEsquerdo();
    }

    void CliqueEsquerdo()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Pega a posição do mouse em mundo
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            // Checa se o mouse está colidindo com o collider deste território
            Collider2D col = GetComponent<Collider2D>();
            if (col == Physics2D.OverlapPoint(mousePos2D))
            {
                borderScript.AlternaVisibilidade();
                Debug.Log($"Sprite {gameObject.name} clicado!");
            }
        }
    }

}











