using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    [Header("Configuração")]
    public List<CardData> todasAsCartas;
    public GameObject cardPrefab;
    public Transform areaDaMao;

    [Header("UI de Troca")]
    public Button botaoTrocar; // ARRASTE O BOTÃO "TROCAR" AQUI NA UNITY!

    private List<CardData> _baralhoAtual;
    
    // Listas para controle da troca
    private List<Carta> _cartasSelecionadas = new List<Carta>();
    private List<CardDisplay> _displaysSelecionados = new List<CardDisplay>();

    void Start()
    {
        InicializarBaralho();

        // Configura o botão de trocar para chamar a função TentarTrocar
        if (botaoTrocar != null)
        {
            botaoTrocar.onClick.AddListener(TentarTrocar);
            botaoTrocar.interactable = false; // Começa desativado (cinza)
        }
    }

    void InicializarBaralho()
    {
        _baralhoAtual = new List<CardData>(todasAsCartas);
        Embaralhar();
    }

    void Embaralhar()
    {
        for (int i = 0; i < _baralhoAtual.Count; i++)
        {
            CardData temp = _baralhoAtual[i];
            int randomIndex = Random.Range(i, _baralhoAtual.Count);
            _baralhoAtual[i] = _baralhoAtual[randomIndex];
            _baralhoAtual[randomIndex] = temp;
        }
    }

    // Chamado pelo GameManager
    public void ComprarUmaCarta()
    {
        if (_baralhoAtual == null || _baralhoAtual.Count == 0) return;

        CardData cartaSorteada = _baralhoAtual[0];
        _baralhoAtual.RemoveAt(0);
        
        // Nota: A visualização real acontece no AtualizarMaoVisual
    }

    // --- LÓGICA VISUAL E INTERAÇÃO ---

    public void AtualizarMaoVisual(List<Carta> maoLogicaDoJogador)
    {
        // Limpa as seleções antigas ao abrir a janela
        _cartasSelecionadas.Clear();
        _displaysSelecionados.Clear();
        AtualizarBotaoTrocar();

        // Limpeza do Grid
        for (int i = areaDaMao.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(areaDaMao.GetChild(i).gameObject);
        }

        // Reconstrução
        foreach (Carta cartaLogica in maoLogicaDoJogador)
        {
            CardData dadosVisuais = EncontrarCardDataCorrespondente(cartaLogica);
            
            if (dadosVisuais != null)
            {
                // Passamos "this" (o próprio DeckManager) para a carta saber quem avisar
                CriarCartaVisual(dadosVisuais, cartaLogica);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)areaDaMao);
    }

    void CriarCartaVisual(CardData dados, Carta cartaLogica)
    {
        GameObject novaCarta = Instantiate(cardPrefab, areaDaMao, false);
        novaCarta.transform.localPosition = Vector3.zero;
        novaCarta.transform.localScale = Vector3.one; 

        // Setup agora recebe a carta lógica e o gerente
        novaCarta.GetComponent<CardDisplay>().Setup(dados, cartaLogica, this);
    }

    // Chamado quando clicamos numa carta
    public void OnCartaClicada(CardDisplay display, Carta carta)
    {
        if (_cartasSelecionadas.Contains(carta))
        {
            // Se já estava selecionada, remove (Deselecionar)
            _cartasSelecionadas.Remove(carta);
            _displaysSelecionados.Remove(display);
            display.AlternarSelecao(false);
        }
        else
        {
            // Se não estava, adiciona (Selecionar) - Limite de 3
            if (_cartasSelecionadas.Count < 3)
            {
                _cartasSelecionadas.Add(carta);
                _displaysSelecionados.Add(display);
                display.AlternarSelecao(true);
            }
        }

        // Verifica se o botão pode ser ativado
        AtualizarBotaoTrocar();
    }

    void AtualizarBotaoTrocar()
    {
        if (botaoTrocar != null)
        {
            // Só ativa o botão se tiver EXATAMENTE 3 cartas
            botaoTrocar.interactable = (_cartasSelecionadas.Count == 3);
        }
    }

    void TentarTrocar()
    {
        // Chama o GameManager para processar a troca (regras do War)
        bool sucesso = GameManager.instance.TentarRealizarTroca(_cartasSelecionadas);

        if (sucesso)
        {
            Debug.Log("Troca realizada!");
            // Se funcionou, atualiza a mão para remover as cartas gastas
            AtualizarMaoVisual(GameManager.instance.jogadorAtual.maoDeCartas);
        }
        else
        {
            Debug.Log("Combinação inválida!");
            // (Opcional) Feedback visual de erro
        }
    }

private CardData EncontrarCardDataCorrespondente(Carta carta)
    {
        // Se for Coringa (não tem território associado)
        if (carta.territorioAssociado == null)
        {
            // CORREÇÃO: Mudamos de SimboloWar para Simbolo
            return todasAsCartas.Find(x => x.simbolo == Simbolo.Coringa);
        }

        // Se for Carta de Território
        return todasAsCartas.Find(x => x.nomeTerritorio == carta.territorioAssociado.name);
    }
}