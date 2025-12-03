using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [Header("UI")]
    public Image imagemDaCarta; 
    public Button botaoDaCarta; // O componente Button do próprio prefab

    // Variáveis internas para saber quem sou eu
    private Carta _minhaCartaLogica;
    private DeckManager _gerente;
    private bool _estaSelecionada = false;

    // Novo Setup que recebe a carta lógica e o gerente
    public void Setup(CardData dadosVisuais, Carta cartaLogica, DeckManager gerente)
    {
        _minhaCartaLogica = cartaLogica;
        _gerente = gerente;

        // Visual
        if (imagemDaCarta != null)
        {
            imagemDaCarta.sprite = dadosVisuais.arteCompleta;
        }

        // Configura o Clique
        if (botaoDaCarta != null)
        {
            botaoDaCarta.onClick.RemoveAllListeners();
            botaoDaCarta.onClick.AddListener(AoClicar);
        }
        
        // Garante que comece desmarcada
        AlternarSelecao(false);
    }

    void AoClicar()
    {
        // Avisa o DeckManager que fui clicada
        if (_gerente != null)
        {
            _gerente.OnCartaClicada(this, _minhaCartaLogica);
        }
    }

    public void AlternarSelecao(bool selecionar)
    {
        _estaSelecionada = selecionar;

        // VISUAL DE SELEÇÃO:
        // Aqui estamos escurecendo a carta quando selecionada.
        // Se preferir uma borda amarela, você pode ativar/desativar um objeto Image aqui.
        if (imagemDaCarta != null)
        {
            imagemDaCarta.color = _estaSelecionada ? new Color(0.6f, 0.6f, 0.6f) : Color.white; // Cinza se selecionado, Branco se normal
        }
    }
}