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

    [System.Serializable]
    public struct InfoCorJogador
    {
        public string nome;
        public Color cor;
    }

    [Header("Controle de Jogo")]
    public List<Player> todosOsJogadores;
    public Player jogadorAtual;
    private int indiceJogadorAtual = 0;

    [Tooltip("A fase atual do turno do jogador")]
    public GamePhase faseAtual;

    [Tooltip("Quantas tropas o jogador ainda pode alocar")]
    public int reforcosPendentes;

    [Header("Configuração de IA e Cores")]
    public AIController aiController;
    public List<InfoCorJogador> coresDisponiveis; // Usado apenas para o modo Debug/Fallback

    [Header("Gerenciamento de Seleção")]
    public TerritorioHandler territorioSelecionado;
    public TerritorioHandler territorioAlvo;

    [Header("Gerenciamento de Objetivos")]
    private List<Objetivo> poolDeObjetivos;

    [Header("Referências da UI")]
    public List<TerritorioHandler> todosOsTerritorios;
    public Button botaoAvancarFase;
    public BattleManager battleManager;
    public TextMeshProUGUI textoReforcosPendentes;

    [Header("Sistema de Cartas")]
    public List<Carta> baralho;
    public List<Carta> descarte;
    public int numeroDeTrocasRealizadas = 0;

    [Header("Sistema Visual de Cartas")]
    public DeckManager deckManagerVisual;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (battleManager == null)
        {
            battleManager = GetComponent<BattleManager>();
        }
    }

    void Start()
    {
        // 1. Inicializa lista de jogadores
        todosOsJogadores = new List<Player>();

        // 2. Tenta carregar a configuração vinda do Menu Principal
        if (GameLaunchData.configuracaoJogadores != null && GameLaunchData.configuracaoJogadores.Count > 0)
        {
            Debug.Log("Carregando configurações do Menu...");
            CarregarJogadoresDoSetup();
        }
        else
        {
            Debug.LogWarning("Nenhuma configuração encontrada. Iniciando modo DEBUG (Hardcoded).");
            ConfigurarModoDebug();
        }

        // Validação de segurança (Minimo 3 players para o jogo não quebrar regras de War)
        if (todosOsJogadores.Count < 2)
        {
            Debug.LogError("ERRO: O jogo precisa de pelo menos 2 jogadores para funcionar (Ideal 3+).");
        }

        // 3. Configura o primeiro jogador
        if (todosOsJogadores.Count > 0)
        {
            jogadorAtual = todosOsJogadores[0];
            indiceJogadorAtual = 0;
        }

        // 4. Busca territórios e distribui
        todosOsTerritorios = FindObjectsByType<TerritorioHandler>(FindObjectsSortMode.None).ToList();

        // Inicializa Objetivos
        InicializarEAssinlarObjetivos();

        // Inicializa Baralho
        InicializarBaralho();

        // Distribui territórios
        DistribuirTerritoriosIniciais();

        // 5. Inicia o jogo
        Debug.Log("GameManager iniciado com " + todosOsJogadores.Count + " jogadores.");
        IniciarNovoTurno();
    }

    #region CRIAÇÃO DE JOGADORES (SETUP VS DEBUG)

    void CarregarJogadoresDoSetup()
    {
        foreach (var config in GameLaunchData.configuracaoJogadores)
        {
            // Cria o jogador exatamente como definido no menu
            CriarJogadorEspecifico(config.nome, config.cor, config.nomeDaCor, config.ehIA);
        }
    }

    void ConfigurarModoDebug()
    {
        InicializarCoresPadrao();

        // Cria o Jogador HUMANO (Player 1)
        CriarJogadorAutomatico("Jogador 1 (Debug)", false);

        // Cria os Jogadores BOT (Preenche até ter 6 no total)
        int totalJogadores = 5;
        int contadorBots = 1;
        while (todosOsJogadores.Count < totalJogadores)
        {
            CriarJogadorAutomatico($"CPU {contadorBots} (Debug)", true);
            contadorBots++;
        }
    }

    // Função usada pelo Setup (Cores já definidas)
    void CriarJogadorEspecifico(string nome, Color cor, string nomeDaCor, bool ehIA)
    {
        Player novoPlayer = new Player(nome, cor, nomeDaCor, ehIA);
        todosOsJogadores.Add(novoPlayer);
    }

    // Função usada pelo Debug (Pega cores da lista interna)
    void CriarJogadorAutomatico(string nomeBase, bool ehIA)
    {
        if (coresDisponiveis.Count == 0) return;

        InfoCorJogador info = coresDisponiveis[0];
        coresDisponiveis.RemoveAt(0);

        CriarJogadorEspecifico(nomeBase, info.cor, info.nome, ehIA);
    }

    void InicializarCoresPadrao()
    {
        if (coresDisponiveis == null || coresDisponiveis.Count == 0)
        {
            coresDisponiveis = new List<InfoCorJogador>
            {
                new InfoCorJogador { nome = "Azul", cor = Color.blue },
                new InfoCorJogador { nome = "Vermelho", cor = Color.red },
                new InfoCorJogador { nome = "Preto", cor = Color.black },
                new InfoCorJogador { nome = "Branco", cor = Color.white },
                new InfoCorJogador { nome = "Verde", cor = Color.green },
                new InfoCorJogador { nome = "Amarelo", cor = Color.yellow }
            };
        }
    }

    #endregion

    #region LÓGICA DE TURNO E FASES

    public int CalcularReforcos(Player player)
    {
        int numTerritorios = todosOsTerritorios.Count(t => t.donoDoTerritorio == player);
        int reforcosBase = Mathf.FloorToInt(numTerritorios / 3f);
        return Mathf.Max(3, reforcosBase);
    }

    public void IniciarNovoTurno()
    {
        DesselecionarTerritorios();
        AtualizarPlayerDoTurnoNosTerritorios();

        faseAtual = GamePhase.Alocacao;
        reforcosPendentes = CalcularReforcos(jogadorAtual);

        if (textoReforcosPendentes != null)
            textoReforcosPendentes.text = reforcosPendentes.ToString();

        UIManager.instance.AtualizarPainelStatus(faseAtual, jogadorAtual);
        Debug.Log($"--- TURNO DE: {jogadorAtual.nome} ({jogadorAtual.nomeDaCor}) --- Reforços: {reforcosPendentes}");

        // --- ATUALIZAÇÃO VISUAL DAS CARTAS (NOVO) ---
        // Se for um humano, limpa o grid e desenha as cartas que ele tem na mão lógica
        if (!jogadorAtual.ehIA && deckManagerVisual != null)
        {
            deckManagerVisual.AtualizarMaoVisual(jogadorAtual.maoDeCartas);
        }
        // --------------------------------------------

        // --- INTEGRAÇÃO COM IA ---
        if (jogadorAtual.ehIA)
        {
            if (aiController != null)
            {
                aiController.IniciarTurnoIA(jogadorAtual);
            }
            else
            {
                Debug.LogError("AIController não está atribuído no GameManager!");
            }
        }
    }

public void OnBotaoAvancarFaseClicado()
    {
        // 1. Bloqueio de Reforços (Já existia) - Impede avançar se ainda tiver tropas para colocar
        if (faseAtual == GamePhase.Alocacao && reforcosPendentes > 0)
        {
            Debug.Log("Alerta: Aloque todas as tropas antes de avançar!");
            // Sugestão: Você pode chamar um UIManager.instance.MostrarAviso("Aloque suas tropas!"); aqui
            return;
        }

        // --- 2. BLOQUEIO DA REGRA DE 5 CARTAS (NOVO) ---
        // Regra: Se o jogador tiver 5 ou mais cartas, ele é OBRIGADO a trocar antes de atacar.
        if (faseAtual == GamePhase.Alocacao)
        {
            // Verifica se é humano (IA já deve tratar isso na lógica dela) e se tem 5+ cartas
            if (!jogadorAtual.ehIA && jogadorAtual.maoDeCartas.Count >= 5)
            {
                Debug.LogWarning("REGRA WAR: Você possui 5 cartas! A troca é obrigatória.");
                
                // Abre o menu de cartas automaticamente para forçar o jogador a ver
                if (UIManager.instance != null) 
                {
                    UIManager.instance.AbrirPopUpCartas();
                }
                
                return; // INTERROMPE: Não deixa mudar para a fase de Ataque
            }
        }
        // ------------------------------------------------

        DesselecionarTerritorios();

        switch (faseAtual)
        {
            case GamePhase.Alocacao:
                faseAtual = GamePhase.Ataque;
                break;

            case GamePhase.Ataque:
                faseAtual = GamePhase.Remanejamento;
                break;

            case GamePhase.Remanejamento:
                MudarParaProximoJogador();
                return;
        }
        UIManager.instance.AtualizarPainelStatus(faseAtual, jogadorAtual);
    }

    void MudarParaProximoJogador()
    {
        // LOG PARA DEBUG
        Debug.Log($"[DEBUG TURN] Trocando turno. Jogador: {jogadorAtual.nome}. Conquistou algo? {jogadorAtual.conquistouTerritorioNesteTurno}");
        
        // 1. ENTREGA DE CARTAS
        if (jogadorAtual.conquistouTerritorioNesteTurno)
        {
            DarCartaAoJogador(jogadorAtual);
            jogadorAtual.conquistouTerritorioNesteTurno = false;
        }

        // 2. LOOP PARA ENCONTRAR O PRÓXIMO JOGADOR VIVO
        int loopCount = 0;
        do
        {
            indiceJogadorAtual = (indiceJogadorAtual + 1) % todosOsJogadores.Count;
            jogadorAtual = todosOsJogadores[indiceJogadorAtual];
            loopCount++;

        } while (!JogadorEstaVivo(jogadorAtual) && loopCount <= todosOsJogadores.Count);

        if (!JogadorEstaVivo(jogadorAtual))
        {
            Debug.LogError("ERRO CRÍTICO: Nenhum jogador vivo encontrado!");
            return;
        }

        // 3. CHECAGEM DE VITÓRIA E INÍCIO
        ChecarVitoria();

        if (faseAtual != GamePhase.JogoPausado)
        {
            IniciarNovoTurno();
        }
    }

    public void BatalhaConcluida()
    {
        faseAtual = GamePhase.Ataque;
        DesselecionarTerritorios();
        ChecarVitoria();
        UIManager.instance.AtualizarPainelStatus(faseAtual, jogadorAtual);
    }

    #endregion

    #region LÓGICA DE CLIQUES

    public void OnTerritorioClicado(TerritorioHandler territorioClicado)
    {
        if (faseAtual == GamePhase.JogoPausado) return;

        // Se for a vez da IA, o jogador humano não pode clicar
        if (jogadorAtual.ehIA) return;

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

    void HandleCliqueAlocacao(TerritorioHandler territorio)
    {
        if (reforcosPendentes <= 0) return;

        if (territorio.donoDoTerritorio == jogadorAtual)
        {
            territorio.numeroDeTropas++;
            territorio.AtualizarVisual();
            reforcosPendentes--;

            if (textoReforcosPendentes != null)
                textoReforcosPendentes.text = reforcosPendentes.ToString();

            UIManager.instance.AtualizarPainelStatus(faseAtual, jogadorAtual);
        }
    }

    void HandleCliqueAtaque(TerritorioHandler territorioClicado)
    {
        if (territorioSelecionado == null)
        {
            if (territorioClicado.donoDoTerritorio == jogadorAtual && territorioClicado.numeroDeTropas > 1)
            {
                territorioSelecionado = territorioClicado;
                territorioSelecionado.Selecionar(true);
            }
        }
        else
        {
            if (territorioClicado == territorioSelecionado)
            {
                DesselecionarTerritorios();
            }
            else if (territorioClicado.donoDoTerritorio != jogadorAtual)
            {
                territorioAlvo = territorioClicado;
                if (territorioSelecionado.vizinhos.Contains(territorioAlvo))
                {
                    faseAtual = GamePhase.JogoPausado;
                    try
                    {
                        battleManager.IniciarBatalha(territorioSelecionado, territorioAlvo);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Erro batalha: {e.Message}");
                        faseAtual = GamePhase.Ataque;
                        DesselecionarTerritorios();
                    }
                }
                else
                {
                    DesselecionarTerritorios();
                }
            }
            else if (territorioClicado.donoDoTerritorio == jogadorAtual)
            {
                DesselecionarTerritorios();
                if (territorioClicado.numeroDeTropas > 1)
                {
                    territorioSelecionado = territorioClicado;
                    territorioSelecionado.Selecionar(true);
                }
            }
        }
    }

    void HandleCliqueRemanejamento(TerritorioHandler territorioClicado)
    {
        if (territorioSelecionado == null)
        {
            if (territorioClicado.donoDoTerritorio == jogadorAtual && territorioClicado.numeroDeTropas > 1)
            {
                territorioSelecionado = territorioClicado;
                territorioSelecionado.Selecionar(false);
            }
        }
        else
        {
            if (territorioClicado == territorioSelecionado)
            {
                DesselecionarTerritorios();
            }
            else if (territorioClicado.donoDoTerritorio == jogadorAtual)
            {
                territorioAlvo = territorioClicado;
                if (territorioSelecionado.vizinhos.Contains(territorioAlvo))
                {
                    if (territorioSelecionado.numeroDeTropas > 1)
                    {
                        territorioSelecionado.numeroDeTropas--;
                        territorioAlvo.numeroDeTropas++;
                        territorioSelecionado.AtualizarVisual();
                        territorioAlvo.AtualizarVisual();
                        DesselecionarTerritorios();
                    }
                }
                else
                {
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
            territorioAlvo = null;
        }
    }

    void DistribuirTerritoriosIniciais()
    {
        List<TerritorioHandler> territoriosEmbaralhados = todosOsTerritorios.OrderBy(a => Random.value).ToList();
        int jogadorIndex = 0;
        int numJogadores = todosOsJogadores.Count;

        foreach (var territorio in territoriosEmbaralhados)
        {
            Player dono = todosOsJogadores[jogadorIndex % numJogadores];
            territorio.donoDoTerritorio = dono;
            territorio.numeroDeTropas = 1;
            territorio.AtualizarVisual();
            jogadorIndex++;
        }
    }

    void AtualizarPlayerDoTurnoNosTerritorios()
    {
        foreach (var territorio in todosOsTerritorios)
        {
            territorio.playerDoTurno = jogadorAtual;
        }
    }

    void InicializarEAssinlarObjetivos()
    {
        poolDeObjetivos = new List<Objetivo>();

        // Objetivos de Destruir
        foreach (Player jogadorAlvo in todosOsJogadores)
        {
            poolDeObjetivos.Add(new ObjetivoDestruirJogador(jogadorAlvo));
        }

        // Objetivos de Conquista
        poolDeObjetivos.Add(new ObjetivoConquistarNTerritorios(todosOsTerritorios.Count, "Conquistar o mundo"));
        poolDeObjetivos.Add(new ObjetivoConquistarNTerritorios(24, "Conquistar 24 territórios"));

        List<Objetivo> objetivosEmbaralhados = poolDeObjetivos.OrderBy(o => Random.value).ToList();

        for (int i = 0; i < todosOsJogadores.Count; i++)
        {
            Player jogador = todosOsJogadores[i];
            Objetivo objParaAssinlar = null;
            int objIndex = 0;

            while (objParaAssinlar == null && objIndex < objetivosEmbaralhados.Count)
            {
                Objetivo objCandidato = objetivosEmbaralhados[objIndex];

                if (objCandidato is ObjetivoDestruirJogador objDestruir)
                {
                    if (objDestruir.JogadorAlvo == jogador)
                    {
                        objIndex++;
                        continue;
                    }
                }

                objParaAssinlar = objCandidato;
                objetivosEmbaralhados.RemoveAt(objIndex);
            }

            if (objParaAssinlar == null)
            {
                objParaAssinlar = new ObjetivoConquistarNTerritorios(15, "Conquistar 15 territórios");
            }

            jogador.objetivoSecreto = objParaAssinlar;
        }
    }

    public void ChecarVitoria()
    {
        if (todosOsTerritorios.Count == 0) return;

        foreach (Player jogador in todosOsJogadores)
        {
            int numTerritorios = todosOsTerritorios.Count(t => t.donoDoTerritorio == jogador);
            if (numTerritorios == 0) continue;

            if (jogador.objetivoSecreto != null && jogador.objetivoSecreto.FoiConcluido(jogador, this))
            {
                AnunciarVencedor(jogador);
                return;
            }
        }

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

    void AnunciarVencedor(Player vencedor)
    {
        faseAtual = GamePhase.JogoPausado;
        Debug.Log($"--- VENCEDOR: {vencedor.nome} ---");

        VencedorInfo.nomeVencedor = vencedor.nome;
        VencedorInfo.corVencedor = vencedor.cor;
        
        // Ative a linha abaixo quando adicionar a cena de vitória no Build Settings
        SceneManager.LoadScene(2);
    }

    public bool JogadorEstaVivo(Player jogador)
    {
        return todosOsTerritorios.Any(t => t.donoDoTerritorio == jogador);
    }

    // --- NOVO MÉTODO: RECICLAGEM DE ELIMINADOS ---
    public void ColetarCartasDoEliminado(Player eliminado)
    {
        if (eliminado.maoDeCartas.Count > 0)
        {
            Debug.Log($"GAME: O jogador {eliminado.nome} foi eliminado! Devolvendo {eliminado.maoDeCartas.Count} cartas ao descarte.");
            
            // Joga as cartas do morto no descarte para serem embaralhadas de volta
            descarte.AddRange(eliminado.maoDeCartas);
            
            // Limpa a mão dele (opcional, mas bom para garantir)
            eliminado.maoDeCartas.Clear();
        }
    }

    #endregion

    #region SISTEMA DE CARTAS E TROCAS

void InicializarBaralho()
    {
        baralho = new List<Carta>();
        descarte = new List<Carta>();

        foreach (var territorio in todosOsTerritorios)
        {
            // 1. Busca o CardData visual
            CardData dadosDoArquivo = null;
            
            if (deckManagerVisual != null)
            {
                dadosDoArquivo = deckManagerVisual.todasAsCartas.Find(c => c.nomeTerritorio == territorio.name);
            }

            // 2. Define o símbolo
            // MUDANÇA AQUI: Não precisa mais converter!
            // Se achou o arquivo, usa o símbolo dele direto. Se não, usa Quadrado.
            Simbolo simboloCorreto = (dadosDoArquivo != null) ? dadosDoArquivo.simbolo : Simbolo.Quadrado;

            if (dadosDoArquivo == null)
            {
                Debug.LogWarning($"AVISO: Não achei CardData para {territorio.name}. Usando Quadrado.");
            }

            // 3. Cria a carta lógica
            baralho.Add(new Carta(simboloCorreto, territorio));
        }

        // Adiciona os Coringas (Simbolo.Coringa já existe no enum)
        baralho.Add(new Carta(Simbolo.Coringa, null));
        baralho.Add(new Carta(Simbolo.Coringa, null));

        EmbaralharDeck();
    }

    void EmbaralharDeck()
    {
        for (int i = 0; i < baralho.Count; i++)
        {
            Carta temp = baralho[i];
            int randomIndex = Random.Range(i, baralho.Count);
            baralho[i] = baralho[randomIndex];
            baralho[randomIndex] = temp;
        }
        Debug.Log("Baralho LÓGICO embaralhado!");
    }

    public void DarCartaAoJogador(Player jogador)
    {
        Debug.Log($"[DEBUG] Entrou em DarCartaAoJogador para {jogador.nome}");

        // --- 1. VISUAL (PRIORIDADE) ---
        if (deckManagerVisual != null && jogador == jogadorAtual && !jogador.ehIA)
        {
            Debug.Log("VISUAL: Enviando comando para o DeckManager criar o visual...");
            deckManagerVisual.ComprarUmaCarta();
        }
        else
        {
            Debug.LogWarning($"VISUAL PULADO: DeckVisual null? {deckManagerVisual == null} | Player atual? {jogador == jogadorAtual} | É IA? {jogador.ehIA}");
        }

        // --- 2. LÓGICA ANTIGA ---
        if (baralho.Count == 0)
        {
            if (descarte.Count > 0)
            {
                baralho.AddRange(descarte);
                descarte.Clear();
                EmbaralharDeck();
            }
            else
            {
                Debug.LogError("ERRO LÓGICO: O baralho do GameManager está vazio e sem descarte!");
                return;
            }
        }

        Carta cartaComprada = baralho[0];
        baralho.RemoveAt(0);
        jogador.maoDeCartas.Add(cartaComprada);
        
        // --- CORREÇÃO DO ERRO DO CORINGA ---
        // Verifica se existe um território antes de tentar ler o nome.
        // Se for null, usamos a string "Coringa".
        string nomeDaCarta = (cartaComprada.territorioAssociado != null) 
            ? cartaComprada.territorioAssociado.name 
            : "Coringa";

        Debug.Log($"LOGICA: Carta {nomeDaCarta} adicionada à mão lógica.");
    }

    public bool TentarRealizarTroca(List<Carta> cartasSelecionadas)
    {
        if (cartasSelecionadas.Count != 3) return false;

        if (ValidarCombinacao(cartasSelecionadas))
        {
            int exercitosGanhos = CalcularExercitosDaTroca();
            reforcosPendentes += exercitosGanhos;
            numeroDeTrocasRealizadas++;

            foreach (Carta c in cartasSelecionadas)
            {
                if (c.territorioAssociado != null && c.territorioAssociado.donoDoTerritorio == jogadorAtual)
                {
                    c.territorioAssociado.numeroDeTropas += 2;
                    c.territorioAssociado.AtualizarVisual();
                }
            }

            foreach (Carta c in cartasSelecionadas)
            {
                jogadorAtual.maoDeCartas.Remove(c);
                descarte.Add(c);
            }

            if (textoReforcosPendentes != null)
                textoReforcosPendentes.text = reforcosPendentes.ToString();

            return true;
        }
        return false;
    }

    private int CalcularExercitosDaTroca()
    {
        int n = numeroDeTrocasRealizadas;
        if (n < 5) return 4 + (2 * n);
        else return 15 + ((n - 5) * 5);
    }

    private bool ValidarCombinacao(List<Carta> cartas)
    {
        bool temCoringa = cartas.Any(c => c.simbolo == Simbolo.Coringa);
        if (temCoringa) return true;

        Simbolo s1 = cartas[0].simbolo;
        Simbolo s2 = cartas[1].simbolo;
        Simbolo s3 = cartas[2].simbolo;

        bool todasIguais = (s1 == s2 && s2 == s3);
        bool todasDiferentes = (s1 != s2 && s1 != s3 && s2 != s3);

        return todasIguais || todasDiferentes;
    }

    #endregion
}