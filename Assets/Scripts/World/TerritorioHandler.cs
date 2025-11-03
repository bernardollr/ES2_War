// TerritorioHandler.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // ADICIONADO: Necessário para o contador de texto

public class TerritorioHandler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public List<TerritorioHandler> vizinhos;

    [Header("Dados do Jogo")]
    public Player donoDoTerritorio;
    public int numeroDeTropas;
    public Player playerDoTurno; // Controlado pelo GameManager

    [Header("Componentes Visuais")]
    public BorderScript borderScript;
    [Tooltip("O componente TextMeshPro que exibirá o número de tropas")]
    public TextMeshProUGUI contadorTropasTexto; // Referência ao Contador_Text existente

    // O GameManager agora controla quem está selecionado
    // private static TerritorioHandler territorioSelecionado = null; // Removido, GameManager gerencia

    void Start()
    {
        try
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"SpriteRenderer não encontrado em {gameObject.name}");
            }

            FindAndStoreNeighbors();
            AtualizarVisual(); // Garante que o contador apareça no início
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro no Start de {gameObject.name}: {e.Message}");
        }
    }

    void FindAndStoreNeighbors()
    {
        try
        {
            vizinhos = new List<TerritorioHandler>();
            var collider = GetComponent<Collider2D>();
            if (collider == null)
            {
                Debug.LogError($"Collider2D não encontrado em {gameObject.name}");
                return;
            }

            List<Collider2D> collidingColliders = new List<Collider2D>();
            ContactFilter2D filter = ContactFilter2D.noFilter;
            collider.Overlap(filter, collidingColliders);

            foreach (var otherCollider in collidingColliders)
            {
                if (otherCollider == null || otherCollider.gameObject == gameObject) continue;
                TerritorioHandler neighbor = otherCollider.GetComponent<TerritorioHandler>();
                if (neighbor != null) vizinhos.Add(neighbor);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao buscar vizinhos em {gameObject.name}: {e.Message}");
            vizinhos = new List<TerritorioHandler>();
        }
    }

    // MODIFICADO: Esta é a função chave para o contador
    public void AtualizarVisual()
    {
        try
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Debug.LogError($"SpriteRenderer não encontrado em {gameObject.name}");
                    return;
                }
            }

            if (donoDoTerritorio != null)
            {
                spriteRenderer.color = donoDoTerritorio.cor;
            }
            else
            {
                spriteRenderer.color = Color.gray;
            }

            // Atualiza o texto do contador na tela
            if (contadorTropasTexto != null)
            {
                contadorTropasTexto.text = numeroDeTropas.ToString();
            }
            else
            {
                // Tenta encontrar o contador no caminho padrão
                Transform exercitoVisual = transform.Find("ExercitoVisual");
                if (exercitoVisual != null)
                {
                    Transform contadorCanvas = exercitoVisual.Find("ContadorCanvas");
                    if (contadorCanvas != null)
                    {
                        Transform contadorText = contadorCanvas.Find("Contador_Text");
                        if (contadorText != null)
                        {
                            contadorTropasTexto = contadorText.GetComponent<TextMeshProUGUI>();
                            if (contadorTropasTexto != null)
                            {
                                contadorTropasTexto.text = numeroDeTropas.ToString();
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao atualizar visual em {gameObject.name}: {e.Message}");
        }
    }

    void Update()
    {
        CliqueEsquerdo();
    }

    // MODIFICADO: Lógica de clique drasticamente simplificada
    void CliqueEsquerdo()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            Collider2D col = GetComponent<Collider2D>();

            // Se o clique foi neste collider específico
            if (col == Physics2D.OverlapPoint(mousePos2D))
            {
                // Apenas informa o GameManager. 
                // O GameManager decide o que fazer (Alocar, Atacar, Selecionar, etc.)
                if (GameManager.instance != null)
                {
                    GameManager.instance.OnTerritorioClicado(this);
                }
            }
        }
    }

    // REMOVIDO: void Atacar(...) - Esta lógica agora está no GameManager/BattleManager

    // Métodos auxiliares (agora são públicos)
    public void Selecionar(bool highlightVizinhosInimigos)
    {
        if (borderScript == null) return;
        borderScript.AlternaVisibilidade(true); // Força a visibilidade
        borderScript.MudarCor(Color.green);

        if (highlightVizinhosInimigos)
        {
            foreach (var vizinho in vizinhos)
            {
                if (vizinho != null && vizinho.borderScript != null && vizinho.donoDoTerritorio != donoDoTerritorio)
                {
                    vizinho.borderScript.AlternaVisibilidade(true);
                    vizinho.borderScript.MudarCor(Color.red);
                }
            }
        }
    }

    public void Desselecionar()
    {
        if (borderScript == null) return;
        borderScript.AlternaVisibilidade(false);
        borderScript.MudarCor(Color.white);

        foreach (var vizinho in vizinhos)
        {
            if (vizinho != null && vizinho.borderScript != null && vizinho.donoDoTerritorio != donoDoTerritorio)
            {
                vizinho.borderScript.AlternaVisibilidade(false);
                vizinho.borderScript.MudarCor(Color.white);
            }
        }
    }

    public static void DesselecionarTodos()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.DesselecionarTerritorios();
        }
    }
}