// AIController.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : MonoBehaviour
{
    // Singleton simples para facilitar acesso, se precisar
    public static AIController instance;

    [Header("Configurações")]
    public float tempoEntreAcoes = 1.0f; // Tempo para o humano ler o log e ver a ação

    private GameManager gameManager;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gameManager = GameManager.instance;
    }

    // Método público chamado pelo GameManager para iniciar o turno da IA
    public void IniciarTurnoIA(Player playerIA)
    {
        StartCoroutine(FluxoDeTurno(playerIA));
    }

    // O "Cérebro" sequencial do turno
    IEnumerator FluxoDeTurno(Player bot)
    {
        Debug.Log($"<color={bot.nomeDaCor}>[IA] {bot.nome} iniciou o turno.</color>");

        // 1. FASE DE ALOCAÇÃO
        yield return new WaitForSeconds(tempoEntreAcoes);
        ExecutarAlocacao(bot);

        // Avança para Ataque
        gameManager.OnBotaoAvancarFaseClicado();

        // 2. FASE DE ATAQUE
        yield return new WaitForSeconds(tempoEntreAcoes);
        // O ataque é um processo que pode demorar (batalhas), então usamos uma Coroutine interna
        yield return StartCoroutine(ExecutarAtaques(bot));

        // Se a fase ainda for Ataque (pode ter mudado se ganhou o jogo), avança
        if (gameManager.faseAtual == GameManager.GamePhase.Ataque)
        {
            gameManager.OnBotaoAvancarFaseClicado();
        }

        // 3. FASE DE REMANEJAMENTO
        yield return new WaitForSeconds(tempoEntreAcoes);
        ExecutarRemanejamento(bot);

        // FIM DO TURNO
        gameManager.OnBotaoAvancarFaseClicado();
    }

    #region LÓGICA DE ALOCAÇÃO (Balanceamento)
    void ExecutarAlocacao(Player bot)
    {
        Debug.Log($"[IA] Iniciando alocação de {gameManager.reforcosPendentes} tropas.");

        // Enquanto houver reforços
        while (gameManager.reforcosPendentes > 0)
        {
            // Estratégia: Encontrar o território com MENOS tropas para equilibrar
            var meusTerritorios = gameManager.todosOsTerritorios
                .Where(t => t.donoDoTerritorio == bot)
                .OrderBy(t => t.numeroDeTropas) // Do menor para o maior
                .ToList();

            if (meusTerritorios.Count > 0)
            {
                TerritorioHandler alvo = meusTerritorios[0];

                // Adiciona tropa (simulando a lógica do GameManager)
                alvo.numeroDeTropas++;
                alvo.AtualizarVisual();
                gameManager.reforcosPendentes--;

                Debug.Log($"[IA] Reforçou {alvo.name} (Agora tem {alvo.numeroDeTropas}). Restam {gameManager.reforcosPendentes}.");
            }
            else
            {
                // Caso de erro bizarro onde a IA não tem territórios (já deveria ter perdido)
                break;
            }
        }

        // Atualiza a UI global apenas por garantia
        UIManager.instance.AtualizarPainelStatus(gameManager.faseAtual, bot);
    }
    #endregion

    #region LÓGICA DE ATAQUE
    IEnumerator ExecutarAtaques(Player bot)
    {
        bool querAtacar = true;
        int maxTentativas = 10; // Evitar loops infinitos se a IA travar

        while (querAtacar && maxTentativas > 0)
        {
            // 1. Identificar meus territórios que PODEM atacar (> 1 tropa)
            var basesDeAtaque = gameManager.todosOsTerritorios
                .Where(t => t.donoDoTerritorio == bot && t.numeroDeTropas > 1)
                .ToList();

            TerritorioHandler origem = null;
            TerritorioHandler alvo = null;
            int menorDefesaEncontrada = 999;

            // 2. Procurar a melhor oportunidade (Vizinho mais fraco)
            foreach (var baseTerritorio in basesDeAtaque)
            {
                // Pega vizinhos inimigos
                var vizinhosInimigos = baseTerritorio.vizinhos
                    .Where(v => v.donoDoTerritorio != bot)
                    .ToList();

                foreach (var vizinho in vizinhosInimigos)
                {
                    // Regra simples: Atacar se eu tiver vantagem ou se o inimigo for muito fraco
                    // Aqui priorizamos o inimigo com MENOS tropas
                    if (vizinho.numeroDeTropas < menorDefesaEncontrada)
                    {
                        // Se empatar, o código atual simplesmente pega o novo (poderia ser random)
                        menorDefesaEncontrada = vizinho.numeroDeTropas;
                        origem = baseTerritorio;
                        alvo = vizinho;
                    }
                    // Critério de desempate aleatório simples
                    else if (vizinho.numeroDeTropas == menorDefesaEncontrada && Random.value > 0.5f)
                    {
                        origem = baseTerritorio;
                        alvo = vizinho;
                    }
                }
            }

            // 3. Executar o ataque se achou um alvo válido
            if (origem != null && alvo != null)
            {
                // Verifica se vale a pena (ex: não atacar um com 10 tropas usando 2)
                // A IA simples vai ser agressiva: se ela tem mais tropas que o alvo, ataca.
                if (origem.numeroDeTropas > alvo.numeroDeTropas)
                {
                    Debug.Log($"[IA] Decidiu atacar de {origem.name} ({origem.numeroDeTropas}) contra {alvo.name} ({alvo.numeroDeTropas}) do {alvo.donoDoTerritorio.nome}.");

                    // Configura o GameManager para o ataque
                    gameManager.territorioSelecionado = origem;
                    gameManager.territorioAlvo = alvo;

                    // Inicia a batalha
                    gameManager.faseAtual = GameManager.GamePhase.JogoPausado;
                    try
                    {
                        gameManager.battleManager.IniciarBatalha(origem, alvo);
                    }
                    catch
                    {
                        Debug.LogError("[IA] Erro ao iniciar batalha.");
                        querAtacar = false;
                    }

                    // ESPERA ATIVA: A IA espera o GameManager sair do estado "JogoPausado"
                    // Isso significa que a batalha acabou (você deve chamar BatalhaConcluida no GM quando acabar os dados)
                    while (gameManager.faseAtual == GameManager.GamePhase.JogoPausado)
                    {
                        yield return null;
                    }

                    // Pequena pausa pós-batalha
                    yield return new WaitForSeconds(1.0f);
                }
                else
                {
                    Debug.Log("[IA] Nenhuma oportunidade de ataque vantajosa encontrada. Encerrando ataques.");
                    querAtacar = false;
                }
            }
            else
            {
                Debug.Log("[IA] Sem vizinhos inimigos acessíveis.");
                querAtacar = false;
            }

            maxTentativas--;
        }
    }
    #endregion

    #region LÓGICA DE REMANEJAMENTO (Balanceamento)
    void ExecutarRemanejamento(Player bot)
    {
        Debug.Log("[IA] Analisando remanejamento...");

        // Estratégia: Pegar do território com MAIS tropas e mover para um vizinho AMIGO com MENOS tropas.
        // A regra do War oficial permite apenas UM movimento. Vamos respeitar isso.

        var meusTerritoriosRicos = gameManager.todosOsTerritorios
            .Where(t => t.donoDoTerritorio == bot && t.numeroDeTropas > 1)
            .OrderByDescending(t => t.numeroDeTropas) // Do maior para o menor
            .ToList();

        foreach (var origem in meusTerritoriosRicos)
        {
            // Procura vizinho amigo
            var vizinhoAmigoPobre = origem.vizinhos
                .Where(v => v.donoDoTerritorio == bot)
                .OrderBy(v => v.numeroDeTropas) // Procura o mais fraco
                .FirstOrDefault();

            if (vizinhoAmigoPobre != null)
            {
                // Verifica se faz sentido mover (se a origem tem muito mais que o destino)
                if (origem.numeroDeTropas > vizinhoAmigoPobre.numeroDeTropas + 1)
                {
                    // Balanceamento simples: move metade do excedente
                    int tropasTotal = origem.numeroDeTropas + vizinhoAmigoPobre.numeroDeTropas;
                    int media = tropasTotal / 2;
                    int mover = origem.numeroDeTropas - media;

                    // Garante que fica pelo menos 1 na origem
                    if (origem.numeroDeTropas - mover < 1) mover = origem.numeroDeTropas - 1;

                    if (mover > 0)
                    {
                        origem.numeroDeTropas -= mover;
                        vizinhoAmigoPobre.numeroDeTropas += mover;

                        origem.AtualizarVisual();
                        vizinhoAmigoPobre.AtualizarVisual();

                        Debug.Log($"[IA] Remanejou {mover} tropas de {origem.name} para {vizinhoAmigoPobre.name}.");
                        return; // Fez o movimento único, encerra.
                    }
                }
            }
        }
        Debug.Log("[IA] Decidiu não remanejar tropas.");
    }
    #endregion
}