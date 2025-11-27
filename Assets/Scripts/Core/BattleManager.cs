// BattleManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using ZeusUnite.Dice; // Certifique-se que esta biblioteca existe ou substitua a rolagem
using UnityEngine.UI;

public class BattleManager : MonoBehaviour {

    public static BattleManager instance;

    [Header("Configuração Visual dos Dados")]
    public Sprite[] facesDosDados; // Deve ter 6 sprites (faces 1 a 6)

    [Header("UI Geral")]
    public GameObject painelBatalha;
    public Button botaoFecharPainel; // Crie um botão no painel de batalha para fechá-lo

    [Header("Locais dos Dados na UI")]
    public Image[] imagensDadosAtaque; // Deve ter 3 slots
    public Image[] imagensDadosDefesa; // Deve ter 2 slots (Regra do War)

    [Header("Resultados")]
    public TextMeshProUGUI textoResultadoBatalha;

    private bool estaRolando = false;

    [Header("Animação")]
    public float duracaoAnimacao = 1.0f;

    // --- ADICIONE ESTA LINHA ---
    [Tooltip("Tempo que o resultado da batalha fica na tela antes de fechar")]
    public float delayResultado = 2.0f;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Proteções para evitar NullReference em Editor/Inspector
        try
        {
            // Adiciona o listener para o botão fechar (se houver)
            if (botaoFecharPainel != null)
            {
                botaoFecharPainel.onClick.AddListener(FecharPainelBatalha);
            }

            // Garante que comece desligado apenas se o painel estiver atribuído
            if (painelBatalha != null)
            {
                painelBatalha.SetActive(false);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BattleManager Awake error: {e.Message}");
        }
    }

    // MODIFICADO: Inicia a batalha com base nos territórios
    public void IniciarBatalha(TerritorioHandler atacante, TerritorioHandler defensor)
    {
        if (estaRolando) return;

        // Validação de tropas (Regra do War: atacante deve ter > 1 tropa)
        if (atacante.numeroDeTropas <= 1)
        {
            Debug.LogError("Erro de Batalha: Atacante precisa de mais de 1 tropa para atacar.");
            GameManager.instance.BatalhaConcluida(); // Avisa o GM que a batalha falhou
            return;
        }

        StartCoroutine(ProcessarBatalha(atacante, defensor));
    }

    // MODIFICADO: Processa a batalha com base nos territórios
    private IEnumerator ProcessarBatalha(TerritorioHandler atacante, TerritorioHandler defensor) {
        estaRolando = true;

        bool useUI = true;
        if (painelBatalha == null)
        {
            Debug.LogError("BattleManager: 'painelBatalha' não está atribuído. A batalha continuará sem UI.");
            useUI = false;
        }

        if (useUI)
        {
            painelBatalha.SetActive(true);
            if (botaoFecharPainel != null)
                botaoFecharPainel.gameObject.SetActive(false);
        }
        else
        {
            if (botaoFecharPainel != null)
                botaoFecharPainel.gameObject.SetActive(false);
        }

        if (textoResultadoBatalha != null)
            textoResultadoBatalha.text = "Rolando dados...";
        else
            Debug.LogWarning("BattleManager: 'textoResultadoBatalha' não está atribuído. Resultados não serão exibidos na UI.");

        // --- Lógica de Dados Modificada ---
        // Atacante pode rolar no máximo 3 dados, e deve ter (N+1) tropas para rolar N dados
        int dadosAtaque = Mathf.Min(3, atacante.numeroDeTropas - 1);
        
        // Defensor pode rolar no máximo 2 dados (regra padrão War)
        int dadosDefesa = Mathf.Min(2, defensor.numeroDeTropas);
        
        List<int> resultadosFinaisAtaque = RolarVariosDados(dadosAtaque);
        List<int> resultadosFinaisDefesa = RolarVariosDados(dadosDefesa);
        // --- Fim da Lógica de Dados ---

        float tempoInicio = Time.time;
        while (Time.time < tempoInicio + duracaoAnimacao) {
            // Animação com o número correto de dados
            if (imagensDadosAtaque != null && imagensDadosAtaque.Length > 0)
                AtualizarImagensDados(RolarVariosDados(dadosAtaque, false), imagensDadosAtaque);
            if (imagensDadosDefesa != null && imagensDadosDefesa.Length > 0)
                AtualizarImagensDados(RolarVariosDados(dadosDefesa, false), imagensDadosDefesa);
            yield return null;
        }

        // Mostra os resultados finais
        if (imagensDadosAtaque != null && imagensDadosAtaque.Length > 0)
            AtualizarImagensDados(resultadosFinaisAtaque, imagensDadosAtaque);
        if (imagensDadosDefesa != null && imagensDadosDefesa.Length > 0)
            AtualizarImagensDados(resultadosFinaisDefesa, imagensDadosDefesa);

        int perdasAtaque = 0;
        int perdasDefesa = 0;
        
        // Compara o número mínimo de dados rolados
        int comparacoes = Mathf.Min(resultadosFinaisAtaque.Count, resultadosFinaisDefesa.Count);

        for (int i = 0; i < comparacoes; i++) {
            // Ataque (valor maior) ganha
            if (resultadosFinaisAtaque[i] > resultadosFinaisDefesa[i])
                perdasDefesa++;
            else // Defesa (valor maior ou igual) ganha
                perdasAtaque++;
        }

        if (textoResultadoBatalha != null)
            textoResultadoBatalha.text = $"Ataque perde: {perdasAtaque}\nDefesa perde: {perdasDefesa}";
        else
            Debug.Log($"Resultado batalha: Ataque perde: {perdasAtaque} / Defesa perde: {perdasDefesa}");

        // --- Atualiza as Tropas nos Territórios ---
        atacante.numeroDeTropas -= perdasAtaque;
        defensor.numeroDeTropas -= perdasDefesa;

        // Verifica se o território foi conquistado
        if (defensor.numeroDeTropas < 1)
        {
            ConquistarTerritorio(atacante, defensor);
            
            // Lógica de movimentação pós-conquista
            // Por enquanto, movemos o mínimo (o número de dados que atacou)
            int tropasMovidas = dadosAtaque;
            if (atacante.numeroDeTropas <= tropasMovidas)
            {
                // Deixa 1 tropa para trás
                tropasMovidas = atacante.numeroDeTropas - 1;
            }

            atacante.numeroDeTropas -= tropasMovidas;
            defensor.numeroDeTropas = tropasMovidas; // Território agora tem as tropas
        }
        
        // Atualiza os contadores na tela
        atacante.AtualizarVisual();
        defensor.AtualizarVisual();
        // --- Fim da Atualização ---

        /*

        if (botaoFecharPainel != null)
            botaoFecharPainel.gameObject.SetActive(true);

        if (useUI && painelBatalha != null)
        {
            // painel ficará aberto até o jogador fechar via botão
        }
        */
        // --- NOVO CÓDIGO (COM DELAY AUTOMÁTICO) ---

        // 1. Espera por 'delayResultado' segundos (os 2 segundos que você queria)
        // O jogador fica lendo o resultado na tela durante esse tempo.
        yield return new WaitForSeconds(delayResultado);

        // 2. Chama a função de fechar o painel automaticamente
        FecharPainelBatalha();

        // 3. O 'botaoFecharPainel' não é mais necessário para esta lógica.

        estaRolando = false;
    }

    // Função para quando o defensor perde todas as tropas
    // Função para quando o defensor perde todas as tropas
    void ConquistarTerritorio(TerritorioHandler atacante, TerritorioHandler defensor)
    {
        Debug.Log($"{atacante.donoDoTerritorio.nome} conquistou {defensor.name}!");
        textoResultadoBatalha.text += $"\n{defensor.name} foi conquistado!";
        
        // 1. Troca o dono do território
        defensor.donoDoTerritorio = atacante.donoDoTerritorio;

        // --- LIGAÇÃO COM O SISTEMA DE CARTAS ---
        // Marca que o jogador realizou uma conquista.
        // O GameManager lerá isso no fim do turno para dar a carta.
        if (atacante.donoDoTerritorio != null)
        {
            atacante.donoDoTerritorio.conquistouTerritorioNesteTurno = true;
            Debug.Log($"[SISTEMA DE CARTAS] Flag de conquista ativada para {atacante.donoDoTerritorio.nome}.");
        }
        // ---------------------------------------

        // Atualiza a cor (AtualizarVisual() fará isso depois na função principal)
    }


    void FecharPainelBatalha()
    {
        if (painelBatalha != null)
            painelBatalha.SetActive(false);
        // Avisa o GameManager que a batalha terminou e o jogo pode continuar
        if (GameManager.instance != null)
            GameManager.instance.BatalhaConcluida();
    }


    void AtualizarImagensDados(List<int> resultados, Image[] imagensUI) {
        if (imagensUI == null || imagensUI.Length == 0)
            return;

        for (int i = 0; i < imagensUI.Length; i++)
        {
            if (imagensUI[i] == null)
                continue;

            if (i >= resultados.Count)
            {
                // Desativa slots de dados que não foram usados
                imagensUI[i].gameObject.SetActive(false);
            }
            else
            {
                int numeroDoDado = resultados[i]; // resultado (1 a 6)

                // Ajuste de índice (Sprite de "1" está no índice 0)
                if (facesDosDados != null && facesDosDados.Length > 0 && numeroDoDado > 0 && numeroDoDado <= facesDosDados.Length)
                {
                    Sprite spriteDoDado = facesDosDados[numeroDoDado - 1]; 
                    imagensUI[i].gameObject.SetActive(true);
                    imagensUI[i].sprite = spriteDoDado;
                }
                else
                {
                    // se não houver sprites definidos, apenas ativa o slot
                    imagensUI[i].gameObject.SetActive(true);
                }
            }
        }
    }

    private List<int> RolarVariosDados(int quantidade, bool ordenar = true) {
        if (quantidade > 3) quantidade = 3;
        if (quantidade < 0) quantidade = 0; // Impede erros

        List<int> resultados = new List<int>();
        for (int i = 0; i < quantidade; i++) {
            // Se você não tiver a biblioteca ZeusUnite, use a rolagem padrão do Unity:
            resultados.Add(Random.Range(1, 7)); // (Random.Range max é exclusivo para int)
            
            // Se você TEM o ZeusUnite:
            // DiceRoller dr = new DiceRoller(1, 6);
            // resultados.Add(dr.rolledValue);
        }
        
        if (ordenar) {
            // Ordena do maior para o menor para a comparação
            return resultados.OrderByDescending(d => d).ToList();
        }
        return resultados;
    }
}