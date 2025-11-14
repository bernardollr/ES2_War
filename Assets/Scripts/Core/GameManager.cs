// GameManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Enum para controlar as fases do turno
    public enum GamePhase
    {
        Alocacao,       // Receber reforços e posicionar
        Ataque,         // Fase de combate
        Remanejamento,  // Mover exércitos entre territórios amigos
        JogoPausado     // Usado durante a batalha ou no fim do jogo
    }

    [Header("Controle de Jogo")]
    public List<Player> todosOsJogadores; // <-- MUDANÇA: Agora é uma lista
    public Player jogadorAtual;
    private int indiceJogadorAtual = 0; // <-- MUDANÇA: Controla a "roda" de jogadores

    [Tooltip("A fase atual do turno do jogador")]
    public GamePhase faseAtual;

    [Tooltip("Quantas tropas o jogador ainda pode alocar")]
    public int reforcosPendentes;

    [Header("Gerenciamento de Seleção")]
    public TerritorioHandler territorioSelecionado;
    public TerritorioHandler territorioAlvo;

    [Header("Gerenciamento de Objetivos")]
    private List<Objetivo> poolDeObjetivos; // <-- NOVO: Lista de objetivos para sortear

    [Header("Referências da UI")]
    public List<TerritorioHandler> todosOsTerritorios;

    [Tooltip("Arraste o Botão 'Passar Turno / Próxima Fase'")]
    public Button botaoAvancarFase;

    [Tooltip("Arraste o seu BattleManager para cá")]
    public BattleManager battleManager;

    [Tooltip("Arraste o Texto (TMP) que mostra os reforços pendentes")]
    public TextMeshProUGUI textoReforcosPendentes;


    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (battleManager == null)
        {
            battleManager = GetComponent<BattleManager>();
            if (battleManager != null)
                Debug.Log("BattleManager atribuído automaticamente a partir do mesmo GameObject.");
            else
                Debug.LogWarning("GameManager: battleManager não está atribuído no Inspector e não foi encontrado no mesmo GameObject.");
        }
    }

    void Start()
    {
        // --- MUDANÇA: Criando N jogadores ---
        todosOsJogadores = new List<Player>();
        // (Lembre-se de ter seu Player.cs com o construtor (nome, cor, nomeDaCor))
        todosOsJogadores.Add(new Player("Jogador 1", Color.blue, "Azul"));
        todosOsJogadores.Add(new Player("Jogador 2", Color.red, "Vermelho"));
        // Adicione quantos jogadores quiser aqui

        jogadorAtual = todosOsJogadores[0];
        indiceJogadorAtual = 0;
        // ------------------------------------

        todosOsTerritorios = FindObjectsByType<TerritorioHandler>(FindObjectsSortMode.None).ToList();
        DistribuirTerritoriosIniciais(); // Atualizado para N jogadores

        // --- NOVO: Lógica de Objetivos ---
        InicializarEAssinlarObjetivos();
        // ---------------------------------

        // Inicia o primeiro turno
        IniciarNovoTurno();

        Debug.Log("GameManager iniciado.");
        PrintTerritoriosPorJogador(); // Atualizado para N jogadores
    }

    #region LÓGICA DE TURNO E FASES

    // Calcula quantos exércitos de reforço o jogador deve receber
    public int CalcularReforcos(Player player)
    {
        int numTerritorios = todosOsTerritorios.Count(t => t.donoDoTerritorio == player);
        int reforcosBase = Mathf.FloorToInt(numTerritorios / 3f);

        // Regra do War: mínimo de 3 exércitos
        return Mathf.Max(3, reforcosBase);
    }

    // Inicia o turno na fase de Alocação
    public void IniciarNovoTurno()
    {
        DesselecionarTerritorios();
        AtualizarPlayerDoTurnoNosTerritorios();

        faseAtual = GamePhase.Alocacao;
        reforcosPendentes = CalcularReforcos(jogadorAtual);

        if (textoReforcosPendentes != null)
        {
            textoReforcosPendentes.text = reforcosPendentes.ToString();
        }

        Debug.Log($"{jogadorAtual.nome} iniciou o turno. Reforços: {reforcosPendentes}");
        UIManager.instance.AtualizarPainelStatus(faseAtual, jogadorAtual);
    }

    // Função chamada pelo botão "Próxima Fase / Encerrar Turno"
    public void OnBotaoAvancarFaseClicado()
    {
        // Proteção: Não pode sair da fase de alocação com tropas pendentes
        if (faseAtual == GamePhase.Alocacao && reforcosPendentes > 0)
        {
            Debug.Log("Alerta: Você deve alocar todas as suas tropas de reforço antes de avançar!");
            return;
        }

        DesselecionarTerritorios();

        switch (faseAtual)
        {
            case GamePhase.Alocacao:
                faseAtual = GamePhase.Ataque;
                Debug.Log("Fase alterada para: Ataque");
                break;

            case GamePhase.Ataque:
                faseAtual = GamePhase.Remanejamento;
                Debug.Log("Fase alterada para: Remanejamento");
                break;

            case GamePhase.Remanejamento:
                MudarParaProximoJogador();
                return; // Sai da função, MudarParaProximoJogador chamará IniciarNovoTurno()
        }
        UIManager.instance.AtualizarPainelStatus(faseAtual, jogadorAtual);
    }

    // --- MUDANÇA: Lógica de "roda" para N jogadores ---
    void MudarParaProximoJogador()
    {
        // Avança o índice e dá a volta se chegar ao fim
        indiceJogadorAtual = (indiceJogadorAtual + 1) % todosOsJogadores.Count;
        jogadorAtual = todosOsJogadores[indiceJogadorAtual];

        Debug.Log("--- FIM DO TURNO. AGORA É O TURNO DE: " + jogadorAtual.nome + " ---");

        ChecarVitoria(); // Checa no início do próximo turno

        if (faseAtual != GamePhase.JogoPausado) // Só inicia um novo turno se o jogo não acabou
        {
            IniciarNovoTurno();
        }
    }

    // Chamada pelo BattleManager quando a batalha termina
    public void BatalhaConcluida()
    {
        faseAtual = GamePhase.Ataque; // Volta para a fase de ataque
        DesselecionarTerritorios();
        ChecarVitoria(); // Verifica se o jogo acabou após a batalha
        UIManager.instance.AtualizarPainelStatus(faseAtual, jogadorAtual);
    }

    #endregion

    #region LÓGICA DE CLIQUES (O Cérebro)

    // Esta é a função central que o TerritorioHandler vai chamar
    // (NENHUMA MUDANÇA NECESSÁRIA AQUI - JÁ USAVA 'jogadorAtual')
    public void OnTerritorioClicado(TerritorioHandler territorioClicado)
    {
        if (faseAtual == GamePhase.JogoPausado) return; // Não faz nada se estiver em batalha

        // Roda a lógica da fase atual
        switch (faseAtual)
        {
            case GamePhase.Alocacao:
                HandleCliqueAlocacao(territorioClicado);
                break;
            case GamePhase.Ataque:
                HandleCliqueAtaque(territorioClicado);
                break;
            case GamePhase.Remanejamento:
                HandleCliqueRemanejamento(territorioClicado);
                break;
        }
    }

    // LÓGICA DE ALOCAÇÃO (Sem mudanças)
    void HandleCliqueAlocacao(TerritorioHandler territorio)
    {
        if (reforcosPendentes <= 0)
        {
            Debug.Log("Você não tem mais reforços para alocar.");
            return;
        }

        if (territorio.donoDoTerritorio == jogadorAtual)
        {
            territorio.numeroDeTropas++;
            territorio.AtualizarVisual(); // Atualiza o contador na tela
            reforcosPendentes--;

            if (textoReforcosPendentes != null)
            {
                textoReforcosPendentes.text = reforcosPendentes.ToString();
            }

            UIManager.instance.AtualizarPainelStatus(faseAtual, jogadorAtual); // Atualiza UI para mostrar reforços restantes
            Debug.Log($"Reforço alocado em {territorio.name}. Restam {reforcosPendentes}.");
        }
        else
        {
            Debug.Log("Você só pode alocar tropas em seus próprios territórios.");
        }
    }

    // LÓGICA DE ATAQUE (Sem mudanças)
    void HandleCliqueAtaque(TerritorioHandler territorioClicado)
    {
        if (battleManager == null)
        {
            Debug.LogError("BattleManager não está configurado no GameManager! Configure-o no Inspector.");
            return;
        }

        if (territorioSelecionado == null)
        {
            // 1. Primeiro clique: Selecionar território de origem (ataque)
            if (territorioClicado.donoDoTerritorio == jogadorAtual)
            {
                if (territorioClicado.numeroDeTropas > 1) // Precisa de pelo menos 2 tropas para atacar
                {
                    territorioSelecionado = territorioClicado;
                    territorioSelecionado.Selecionar(true); // Feedback visual
                }
                else
                {
                    Debug.Log("Você precisa de pelo menos 2 tropas para atacar deste território.");
                }
            }
            else
            {
                Debug.Log("Selecione um território seu para atacar.");
            }
        }
        else
        {
            // 2. Segundo clique:
            if (territorioClicado == territorioSelecionado)
            {
                // Clicou no mesmo território: Desselecionar
                DesselecionarTerritorios();
            }
            else if (territorioClicado.donoDoTerritorio != jogadorAtual)
            {
                // Clicou no inimigo: Definir como alvo
                territorioAlvo = territorioClicado;

                // Verifica se são vizinhos
                if (territorioSelecionado != null && territorioSelecionado.vizinhos.Contains(territorioAlvo))
                {
                    faseAtual = GamePhase.JogoPausado; // Pausa o jogo
                    try
                    {
                        battleManager.IniciarBatalha(territorioSelecionado, territorioAlvo);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Erro ao iniciar batalha: {e.Message}");
                        faseAtual = GamePhase.Ataque; // Volta para a fase de ataque se der erro
                        DesselecionarTerritorios();
                    }
                }
                else
                {
                    Debug.Log("Territórios não são vizinhos!");
                    DesselecionarTerritorios();
                }
            }
            else if (territorioClicado.donoDoTerritorio == jogadorAtual)
            {
                // Mudou de ideia e clicou em outro território seu
                DesselecionarTerritorios();
                if (territorioClicado.numeroDeTropas > 1)
                {
                    territorioSelecionado = territorioClicado;
                    territorioSelecionado.Selecionar(true);
                }
            }
        }
    }

    // LÓGICA DE REMANEJAMENTO (Sem mudanças)
    void HandleCliqueRemanejamento(TerritorioHandler territorioClicado)
    {
        if (territorioSelecionado == null)
        {
            // 1. Primeiro clique: Selecionar origem (tem que ser seu e ter tropas extras)
            if (territorioClicado.donoDoTerritorio == jogadorAtual && territorioClicado.numeroDeTropas > 1)
            {
                territorioSelecionado = territorioClicado;
                territorioSelecionado.Selecionar(false); // Seleciona, mas sem highlight vermelho
            }
        }
        else
        {
            // 2. Segundo clique:
            if (territorioClicado == territorioSelecionado)
            {
                DesselecionarTerritorios(); // Desseleciona
            }
            else if (territorioClicado.donoDoTerritorio == jogadorAtual)
            {
                // Clicou em outro território seu: Definir como alvo
                territorioAlvo = territorioClicado;

                // TODO: Verificar se há um *caminho* de territórios amigos (não apenas vizinhos)
                if (territorioSelecionado.vizinhos.Contains(territorioAlvo))
                {
                    // IMPLEMENTAÇÃO SIMPLES: Move 1 tropa
                    // O ideal seria abrir um pop-up perguntando quantas tropas mover
                    if (territorioSelecionado.numeroDeTropas > 1)
                    {
                        territorioSelecionado.numeroDeTropas--;
                        territorioAlvo.numeroDeTropas++;
                        territorioSelecionado.AtualizarVisual();
                        territorioAlvo.AtualizarVisual();

                        Debug.Log($"Moveu 1 tropa de {territorioSelecionado.name} para {territorioAlvo.name}");
                        DesselecionarTerritorios();

                        // No War, você só pode fazer UM remanejamento. 
                        // Força o avanço da fase
                        OnBotaoAvancarFaseClicado();
                    }
                }
                else
                {
                    Debug.Log("Não é possível remanejar para um território não adjacente.");
                    DesselecionarTerritorios();
                }
            }
        }
    }

    #endregion

    #region FUNÇÕES AUXILIARES E VISUAIS

    public void DesselecionarTerritorios()
    {
        if (territorioSelecionado != null)
        {
            territorioSelecionado.Desselecionar();
            territorioSelecionado = null;
        }
        if (territorioAlvo != null)
        {
            // O alvo não é "selecionado", apenas resetamos a referência
            territorioAlvo = null;
        }
    }

    // --- MUDANÇA: Atualizado para N jogadores ---
    void DistribuirTerritoriosIniciais()
    {
        List<TerritorioHandler> territoriosEmbaralhados = todosOsTerritorios.OrderBy(a => Random.value).ToList();
        int jogadorIndex = 0;
        int numJogadores = todosOsJogadores.Count; // Pega o número de jogadores

        foreach (var territorio in territoriosEmbaralhados)
        {
            // Lógica de distribuição para N jogadores
            Player dono = todosOsJogadores[jogadorIndex % numJogadores];

            territorio.donoDoTerritorio = dono;
            territorio.numeroDeTropas = 1;
            territorio.AtualizarVisual(); // Garante que o contador inicial (1) apareça
            jogadorIndex++;
        }
        Debug.Log("Territórios iniciais distribuídos!");
    }

    // --- MUDANÇA: Atualizado para N jogadores ---
    void PrintTerritoriosPorJogador()
    {
        // Itera sobre a lista de jogadores
        foreach (Player p in todosOsJogadores)
        {
            Debug.Log($"=== Territórios do {p.nome} ===");
            // Filtra os territórios para cada jogador
            foreach (var t in todosOsTerritorios.Where(t => t.donoDoTerritorio == p))
            {
                Debug.Log($"- {t.name} com {t.numeroDeTropas} tropa(s)");
            }
        }
    }

    void AtualizarPlayerDoTurnoNosTerritorios()
    {
        foreach (var territorio in todosOsTerritorios)
        {
            territorio.playerDoTurno = jogadorAtual;
        }
    }

    // --- NOVO: Função de distribuição de objetivos ---
    void InicializarEAssinlarObjetivos()
    {
        poolDeObjetivos = new List<Objetivo>();

        // 1. Criar um objetivo de "Destruir" para CADA jogador
        foreach (Player jogadorAlvo in todosOsJogadores)
        {
            // (Isso assume que 'ObjetivoDestruirJogador.cs' está como discutimos)
            poolDeObjetivos.Add(new ObjetivoDestruirJogador(jogadorAlvo));
        }

        // 2. Adicionar outros objetivos genéricos
        int totalTerritorios = todosOsTerritorios.Count;
        poolDeObjetivos.Add(new ObjetivoConquistarNTerritorios(totalTerritorios, "Conquistar o mundo (todos os territórios)"));
        poolDeObjetivos.Add(new ObjetivoConquistarNTerritorios(6, "Conquistar 6 territórios")); // Exemplo
        // Adicione objetivos de continente aqui...

        // 3. Embaralhar a lista
        List<Objetivo> objetivosEmbaralhados = poolDeObjetivos.OrderBy(o => Random.value).ToList();

        // 4. Distribuir, garantindo que ninguém pegue o objetivo de se destruir
        for (int i = 0; i < todosOsJogadores.Count; i++)
        {
            Player jogador = todosOsJogadores[i];
            Objetivo objParaAssinlar = null;
            int objIndex = 0;

            while (objParaAssinlar == null && objIndex < objetivosEmbaralhados.Count)
            {
                Objetivo objCandidato = objetivosEmbaralhados[objIndex];

                // É um objetivo de Destruir?
                if (objCandidato is ObjetivoDestruirJogador objDestruir)
                {
                    // Se for, é um objetivo de destruir a si mesmo?
                    if (objDestruir.JogadorAlvo == jogador)
                    {
                        // Sim, é. Pula este objetivo.
                        objIndex++;
                        continue;
                    }
                }

                // Se chegou aqui, o objetivo é válido
                objParaAssinlar = objCandidato;
                objetivosEmbaralhados.RemoveAt(objIndex); // Remove para não ser pego por outro
            }

            if (objParaAssinlar == null)
            {
                Debug.LogError($"Não foi possível encontrar um objetivo válido para {jogador.nome}!");
                // Damos um objetivo padrão para evitar erros
                objParaAssinlar = new ObjetivoConquistarNTerritorios(10, "Conquistar 10 territórios");
            }

            jogador.objetivoSecreto = objParaAssinlar;
            Debug.Log($"{jogador.nome} recebeu o objetivo: {jogador.objetivoSecreto.Descricao}");
        }
    }


    // --- MUDANÇA: Lógica de vitória atualizada para checar objetivos ---
    public void ChecarVitoria()
    {
        if (todosOsTerritorios.Count == 0) return;

        // --- LÓGICA DE OBJETIVO SECRETO ---
        // Itera por todos os jogadores para ver se algum deles venceu
        foreach (Player jogador in todosOsJogadores)
        {
            // (Isso assume que 'Objetivo.cs' e suas classes filhas estão corretas)
            if (jogador.objetivoSecreto != null && jogador.objetivoSecreto.FoiConcluido(jogador, this))
            {
                AnunciarVencedor(jogador);
                return; // Jogo acaba
            }
        }

        // --- LÓGICA DE DOMINAÇÃO TOTAL (Fallback) ---
        Player donoReferencia = todosOsTerritorios[0].donoDoTerritorio;
        bool todosIguais = true;
        foreach (var territorio in todosOsTerritorios)
        {
            if (territorio.donoDoTerritorio != donoReferencia)
            {
                todosIguais = false;
                break;
            }
        }

        if (todosIguais)
        {
            AnunciarVencedor(donoReferencia);
        }
    }

    // --- NOVO: Função auxiliar para anunciar o vencedor ---
    void AnunciarVencedor(Player vencedor)
    {
        // Pausa o jogo para evitar mais cliques
        faseAtual = GamePhase.JogoPausado;

        Debug.Log($"--- JOGO ACABOU! VENCEDOR: {vencedor.nome} ---");
        Debug.Log($"Objetivo concluído: {vencedor.objetivoSecreto.Descricao}");

        VencedorInfo.nomeVencedor = vencedor.nome;
        VencedorInfo.corVencedor = vencedor.cor;
        SceneManager.LoadScene(2); // (Assumindo que a cena 2 é a tela de vitória)
    }

    #endregion
}