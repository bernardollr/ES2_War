using UnityEngine; 
using UnityEngine.EventSystems; // Importa o sistema de eventos, necessário para usar as interfaces IPointerEnterHandler e IPointerExitHandler.
using System.Collections.Generic; 
using System.Linq; // Oferece métodos adicionais para trabalhar com coleções

// IPointerEnterHandler, IPointerExitHandler: Interfaces que "contratam" o script com o EventSystem.
// Elas garantem que os métodos OnPointerEnter e OnPointerExit serão chamados quando o mouse entrar ou sair do objeto.
public class TerritorioHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("Hover Settings")]
    [Tooltip("A cor que o território terá quando o mouse estiver sobre ele")] 
    public Color hoverColor = new Color(1f, 0.5f, 0.5f, 1f); // Define a cor do hover, com um valor padrão vermelho claro.

    private SpriteRenderer spriteRenderer; // Armazena a referência ao componente visual do território.
    private Color originalColor; 
    public List<TerritorioHandler> vizinhos;

    [Header("Dados do Jogo")]
    public Player donoDoTerritorio; 
    public int numeroDeTropas;

    // O método Start() é chamado pela Unity apenas uma vez, no primeiro frame em que o script está ativo. É usado para configurar o estado inicial do objeto.
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        FindAndStoreNeighbors();
    }
    // Este método é chamado pelo EventSystem quando o ponteiro do mouse ENTRA na área do collider deste objeto.
    public void OnPointerEnter(PointerEventData eventData)
    {
        spriteRenderer.color = hoverColor;
    }

    // Este método é chamado pelo EventSystem quando o ponteiro do mouse SAI da área do collider deste objeto
    public void OnPointerExit(PointerEventData eventData)
    {
        spriteRenderer.color = originalColor;
    }
    // Método responsável por usar o sistema de física 2D para detectar e armazenar todos os territórios adjacentes
    void FindAndStoreNeighbors()
    {
        vizinhos = new List<TerritorioHandler>();
        // Cria uma lista temporária que será preenchida pelo método de física
        List<Collider2D> collidingColliders = new List<Collider2D>();

        // Cria um filtro de contato. `noFilter` é uma propriedade estática que retorna um filtro que não filtra nada,
        // ou seja, ele considerará todos os resultados. Esta é a forma moderna e correta de fazer isso
        ContactFilter2D filter = ContactFilter2D.noFilter;

        // Pede ao componente Collider2D deste objeto para verificar todos os outros colliders que o estão tocando
        // e preencher a lista `collidingColliders` com os resultados
        GetComponent<Collider2D>().Overlap(filter, collidingColliders);

        // Percorre cada um dos colliders que foram encontrados
        foreach (var collider in collidingColliders)
        {
            // Se o collider for do mesmo game object que este script, ignora
            if (collider.gameObject == gameObject)
            {
                continue;
            }

            // Tenta obter o script "TerritorioHandler" do GameObject vizinho.
            TerritorioHandler neighbor = collider.GetComponent<TerritorioHandler>();

            // Se o objeto vizinho de fato tem o script (ou seja, `neighbor` não é nulo),
            // significa que ele é um território.
            if (neighbor != null)
            {
                // Adiciona o território vizinho encontrado à nossa lista de vizinhos.
                vizinhos.Add(neighbor);
            }
        }
    }
    /// <summary>
    /// Atualiza a aparência do território (cor) com base nos seus dados atuais.
    /// </summary>
    public void AtualizarVisual()
    {
        // Verifica se há um dono atribuído
        if (donoDoTerritorio != null)
        {
            // Muda a cor do sprite para a cor do jogador dono.
            spriteRenderer.color = donoDoTerritorio.cor;
        }
        else
        {
            // Se não houver dono (neutro), define uma cor padrão (ex: cinza).
            spriteRenderer.color = Color.gray;
        }

        // MUITO IMPORTANTE: Atualiza a "cor original" para a nova cor do dono.
        // Isso garante que o efeito de hover funcione corretamente depois que o dono for definido.
        originalColor = spriteRenderer.color;
    }
}