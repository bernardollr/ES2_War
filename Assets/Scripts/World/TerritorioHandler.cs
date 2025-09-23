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

    public Player playerDoTurno;

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
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            Collider2D col = GetComponent<Collider2D>();

            if (col == Physics2D.OverlapPoint(mousePos2D) && donoDoTerritorio == playerDoTurno)
            {
                // Mostra a borda do território clicado
                borderScript.AlternaVisibilidade();
                borderScript.MudarCor(Color.green);

                // Mostra borda apenas dos vizinhos inimigos
                foreach (var vizinho in vizinhos)
                {
                    if (vizinho != null && vizinho.borderScript != null)
                    {
                        // só mostra se o vizinho não for do mesmo dono
                        if (vizinho.donoDoTerritorio != donoDoTerritorio)
                        {
                            vizinho.borderScript.AlternaVisibilidade();
                            vizinho.borderScript.MudarCor(Color.red);
                        }
                    }
                }

                Debug.Log($"Sprite {gameObject.name} clicado pelo {playerDoTurno.nome}!");
            }
        }
    }


}











