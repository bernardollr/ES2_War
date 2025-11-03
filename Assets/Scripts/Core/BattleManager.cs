<<<<<<< Updated upstream
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using ZeusUnite.Dice;
using System.Runtime.CompilerServices;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour {
    [Header("Configura��o Visual dos Dados")]
    public Sprite[] facesDosDados;

    [Header("UI Geral")]
    public GameObject painelBatalha;

    [Header("Locais dos Dados na UI")]
    public Image[] imagensDadosAtaque;
    public Image[] imagensDadosDefesa;
=======
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
>>>>>>> Stashed changes

        [Header("Resultados")]
        public TextMeshProUGUI textoResultadoBatalha;

        private bool estaRolando = false;

<<<<<<< Updated upstream
    [Header("Anima��o")]
    public float duracaoAnimacao = 1.0f;


    public void IniciarBatalha()
    {
        if (estaRolando) return;

        StartCoroutine(ProcessarBatalha());
    }

    private IEnumerator ProcessarBatalha() {
        estaRolando = true;
        painelBatalha.SetActive(true);
        textoResultadoBatalha.text = "Rolando dados...";

        List<int> resultadosFinaisAtaque = RolarVariosDados(3);
        List<int> resultadosFinaisDefesa = RolarVariosDados(2);

        float tempoInicio = Time.time;
        while (Time.time < tempoInicio + duracaoAnimacao) {
            AtualizarImagensDados(RolarVariosDados(3, false), imagensDadosAtaque);
            AtualizarImagensDados(RolarVariosDados(2, false), imagensDadosDefesa);

            yield return null;
        }

        AtualizarImagensDados(resultadosFinaisAtaque, imagensDadosAtaque);
        AtualizarImagensDados(resultadosFinaisDefesa, imagensDadosDefesa);

        int perdasAtaque = 0;
        int perdasDefesa = 0;
        int comparacoes = Mathf.Min(resultadosFinaisAtaque.Count, resultadosFinaisDefesa.Count);

        for (int i = 0; i < comparacoes; i++) {
            if (resultadosFinaisAtaque[i] > resultadosFinaisDefesa[i])
                perdasDefesa++;
            else
                perdasAtaque++;
        }

        textoResultadoBatalha.text = $"Ataque perde: {perdasAtaque}\nDefesa perde: {perdasDefesa}";

        estaRolando = false;
    }

    void AtualizarImagensDados(List<int> resultados, Image[] imagensUI) {
        for (int i = 0; i < imagensUI.Length; i++) {
            if (i >= resultados.Count)
            {
                imagensUI[i].gameObject.SetActive(false);
            }
            else {
                int numeroDoDado = resultados[i];

                Sprite spriteDoDado = facesDosDados[numeroDoDado - 1];

                imagensUI[i].gameObject.SetActive(true);
                imagensUI[i].sprite = spriteDoDado;
            }
        }
    }
     private List<int> RolarVariosDados(int quantidade, bool ordenar = true) {
        if (quantidade > 3)
            quantidade = 3;
       List<int> resultados = new List<int>();
        for (int i = 0; i < quantidade; i++) {
            DiceRoller dr = new DiceRoller(1, 6);
            resultados.Add(dr.rolledValue);
        }
        if (ordenar) {
            return resultados.OrderByDescending(d => d).ToList();
        }
        return resultados;
     }
 }
=======
        [Header("Animação")]
        public float duracaoAnimacao = 1.0f;

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
            
            // Guarda a referência do estado do botão antes da batalha
            Button botaoAvancarFase = null;
            if (GameManager.instance != null)
            {
                botaoAvancarFase = GameManager.instance.botaoAvancarFase;
            }

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
                // Garante que o botão de avançar fase continue visível sobre o painel
                if (GameManager.instance != null && GameManager.instance.botaoAvancarFase != null)
                {
                    var bot = GameManager.instance.botaoAvancarFase;
                    bot.gameObject.SetActive(true);
                    // Tenta mover na hierarquia (caso estejam no mesmo Canvas)
                    try { bot.transform.SetAsLastSibling(); } catch {}

                    // Se estiverem em Canvases diferentes, garante ordering adicionando/ajustando um Canvas local
                    Canvas painelCanvas = painelBatalha.GetComponentInParent<Canvas>();
                    Canvas botCanvas = bot.GetComponentInParent<Canvas>();
                    if (painelCanvas != null)
                    {
                        // Se o botão não tiver Canvas próprio ou tiver sortingOrder <= painel, adiciona/ajusta
                        if (botCanvas == null || botCanvas.sortingOrder <= painelCanvas.sortingOrder)
                        {
                            Canvas c = bot.gameObject.GetComponent<Canvas>();
                            if (c == null) c = bot.gameObject.AddComponent<Canvas>();
                            c.overrideSorting = true;
                            c.sortingOrder = painelCanvas.sortingOrder + 10;
                            // Garante que o botão receba cliques
                            if (bot.gameObject.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                                bot.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                        }
                    }
                    else
                    {
                        // Se não houver canvas do painel, assegura um canvas para o botão apenas por precaução
                        Canvas c = bot.gameObject.GetComponent<Canvas>();
                        if (c == null) c = bot.gameObject.AddComponent<Canvas>();
                        c.overrideSorting = true;
                        c.sortingOrder = 100;
                        if (bot.gameObject.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                            bot.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    }
                }
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

            // Aguarda 2 segundos e fecha automaticamente
            yield return new WaitForSeconds(2f);
            FecharPainelBatalha();
            estaRolando = false;
        }

        // Função para quando o defensor perde todas as tropas
        void ConquistarTerritorio(TerritorioHandler atacante, TerritorioHandler defensor)
        {
            Debug.Log($"{atacante.donoDoTerritorio.nome} conquistou {defensor.name}!");
            textoResultadoBatalha.text += $"\n{defensor.name} foi conquistado!";
            
            // Troca o dono do território
            defensor.donoDoTerritorio = atacante.donoDoTerritorio;
            // Atualiza a cor (AtualizarVisual() fará isso)
        }


        void FecharPainelBatalha()
        {
            Debug.Log("Fechando painel de batalha");
            if (painelBatalha != null)
            {
                painelBatalha.SetActive(false);
            }
            else
            {
                Debug.LogWarning("painelBatalha é nulo!");
            }

            // Restaura o botão de avançar fase
            if (GameManager.instance != null && GameManager.instance.botaoAvancarFase != null)
            {
                GameManager.instance.botaoAvancarFase.gameObject.SetActive(true);
            }

            // Avisa o GameManager que a batalha terminou e o jogo pode continuar
            if (GameManager.instance != null)
            {
                GameManager.instance.BatalhaConcluida();
            }
            else
            {
                Debug.LogError("GameManager.instance é nulo ao tentar concluir batalha!");
            }

            estaRolando = false; // Garante que o estado de batalha é resetado
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
>>>>>>> Stashed changes
