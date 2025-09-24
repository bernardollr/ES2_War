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

    private static TerritorioHandler territorioSelecionado = null;

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

            if (col == Physics2D.OverlapPoint(mousePos2D))
            {
                // Clique em território do jogador atual
                if (donoDoTerritorio == playerDoTurno)
                {
                    if (territorioSelecionado != null && territorioSelecionado != this)
                    {
                        territorioSelecionado.Desselecionar();
                    }

                    if (territorioSelecionado == this)
                    {
                        Desselecionar();
                        territorioSelecionado = null;
                    }
                    else
                    {
                        Selecionar();
                        territorioSelecionado = this;
                    }
                }
                // Clique em território inimigo
                else
                {
                    // Só ataca se houver um território selecionado do jogador atual e for vizinho
                    if (territorioSelecionado != null && territorioSelecionado.vizinhos.Contains(this))
                    {
                        Atacar(territorioSelecionado, this);
                    }
                }
            }
        }
    }
    void Atacar(TerritorioHandler atacante, TerritorioHandler defensor)
    {
        Debug.Log($"{atacante.name} ataca {defensor.name}!");

        // Transferir o território para o atacante
        defensor.donoDoTerritorio = atacante.donoDoTerritorio;
        defensor.AtualizarVisual();
        defensor.borderScript.MudarCor(Color.white);
        defensor.borderScript.AlternaVisibilidade();

        // Dessseleciona todos
        DesselecionarTodos();

        // Troca de turno
        GameManager.instance.TrocarTurno();
    }

    // Métodos auxiliares
    void Selecionar()
    {
        borderScript.AlternaVisibilidade();
        borderScript.MudarCor(Color.green);

        foreach (var vizinho in vizinhos)
        {
            if (vizinho != null && vizinho.borderScript != null && vizinho.donoDoTerritorio != donoDoTerritorio)
            {
                vizinho.borderScript.AlternaVisibilidade();
                vizinho.borderScript.MudarCor(Color.red);
            }
        }
    }

    void Desselecionar()
    {
        borderScript.AlternaVisibilidade();
        borderScript.MudarCor(Color.white);

        foreach (var vizinho in vizinhos)
        {
            if (vizinho != null && vizinho.borderScript != null && vizinho.donoDoTerritorio != donoDoTerritorio)
            {
                vizinho.borderScript.AlternaVisibilidade();
                vizinho.borderScript.MudarCor(Color.white);
            }
        }
    }

    public static void DesselecionarTodos()
    {
        if (territorioSelecionado != null)
        {
            territorioSelecionado.Desselecionar();
            territorioSelecionado = null;
        }
    }


}











