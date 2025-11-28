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
    public AIController aiController; // <-- ARRASTE SEU SCRIPT AICONTROLLER AQUI
    public List<InfoCorJogador> coresDisponiveis; // Será preenchida automaticamente se vazia

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
    public int numeroDeTrocasRealizadas = 0; // Para controlar o valor (4, 6, 8, 10...)

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
        // 1. Configura as cores padrão se não foram definidas no Inspector
        InicializarCoresPadrao();

        // 2. Inicializa lista e cria os jogadores
        todosOsJogadores = new List<Player>();

        // Cria o Jogador HUMANO (Player 1)
        CriarJogador("Jogador 1", false); // false = não é IA

        // Cria os Jogadores BOT (Preenche até ter 6 no total)
        int totalJogadores = 5;
        int contadorBots = 1;
        while (todosOsJogadores.Count < totalJogadores)
        {
            CriarJogador($"CPU {contadorBots}", true); // true = é IA
            contadorBots++;
        }

        // Configura o primeiro jogador
        jogadorAtual = todosOsJogadores[0];
        indiceJogadorAtual = 0;

        // 3. Busca territórios e distribui
        todosOsTerritorios = FindObjectsByType<TerritorioHandler>(FindObjectsSortMode.None).ToList();
        DistribuirTerritoriosIniciais();

        // 4. Objetivos
        InicializarEAssinlarObjetivos();

        // 5. Cartas
        todosOsTerritorios = FindObjectsByType<TerritorioHandler>(FindObjectsSortMode.None).ToList();    
        // ADICIONE ISSO AQUI:
        InicializarBaralho();
        
        DistribuirTerritoriosIniciais();

        // 6. Inicia o jogo
        Debug.Log("GameManager iniciado com " + todosOsJogadores.Count + " jogadores.");
        IniciarNovoTurno();
    }

    // Função auxiliar para criar jogadores com cores únicas
    void CriarJogador(string nomeBase, bool ehIA)
    {
        if (coresDisponiveis.Count == 0)
        {
            Debug.LogError("Acabaram as cores disponíveis! Não é possível criar mais jogadores.");
            return;
        }

        // Pega a primeira cor da lista e remove para ninguém mais usar
        InfoCorJogador info = coresDisponiveis[0];
        coresDisponiveis.RemoveAt(0);

        // Instancia o Player (Certifique-se que seu Player.cs tem o bool ehIA no construtor)
        Player novoPlayer = new Player(nomeBase, info.cor, info.nome, ehIA);
        todosOsJogadores.Add(novoPlayer);
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

        // --- INTEGRAÇÃO COM IA ---
        if (jogadorAtual.ehIA)
        {
            if (aiController != null)
            {
                // Inicia o cérebro da IA
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
        // Se for turno da IA, o botão (se clicado manualmente por bug) não deve funcionar,
        // mas a IA chama essa função via código, então permitimos se a origem for código.
        // Como não dá pra saber a origem fácil, confiamos na lógica do AIController.

        if (faseAtual == GamePhase.Alocacao && reforcosPendentes > 0)
        {
            Debug.Log("Alerta: Aloque todas as tropas antes de avançar!");
            return;
        }

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
        // 1. ENTREGA DE CARTAS (Lógica que já fizemos)
        if (jogadorAtual.conquistouTerritorioNesteTurno)
        {
            DarCartaAoJogador(jogadorAtual);
            jogadorAtual.conquistouTerritorioNesteTurno = false;
        }

        // 2. LOOP PARA ENCONTRAR O PRÓXIMO JOGADOR VIVO
        // Ele vai rodar pelo menos uma vez, e se o próximo estiver morto, roda de novo.
        // Adicionamos uma segurança (loopCount) para evitar travamento infinito caso todos morram (bug raro)
        int loopCount = 0;
        do
        {
            indiceJogadorAtual = (indiceJogadorAtual + 1) % todosOsJogadores.Count;
            jogadorAtual = todosOsJogadores[indiceJogadorAtual];
            loopCount++;

        } while (!JogadorEstaVivo(jogadorAtual) && loopCount <= todosOsJogadores.Count);

        // Se rodou a lista toda e não achou ninguém (impossível se a checagem de vitória funcionar), evita crash
        if (!JogadorEstaVivo(jogadorAtual)) 
        {
            Debug.LogError("ERRO CRÍTICO: Nenhum jogador vivo encontrado para o próximo turno!");
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

        // --- BLOQUEIO PARA IA ---
        // Se for a vez da IA, o jogador humano não pode clicar em nada
        if (jogadorAtual.ehIA) return;
        // ------------------------

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
            if (territorioClicado.donoDoTerritorio == jogadorAtual)
            {
                if (territorioClicado.numeroDeTropas > 1)
                {
                    territorioSelecionado = territorioClicado;
                    territorioSelecionado.Selecionar(true);
                }
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
                        //OnBotaoAvancarFaseClicado();
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
        int totalTerritorios = todosOsTerritorios.Count;
        poolDeObjetivos.Add(new ObjetivoConquistarNTerritorios(totalTerritorios, "Conquistar o mundo"));
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
            // Se o jogador foi eliminado (0 territórios), pula a checagem dele
            // (Opcional: você pode remover o jogador da lista se quiser)
            int numTerritorios = todosOsTerritorios.Count(t => t.donoDoTerritorio == jogador);
            if (numTerritorios == 0) continue;

            if (jogador.objetivoSecreto != null && jogador.objetivoSecreto.FoiConcluido(jogador, this))
            {
                AnunciarVencedor(jogador);
                return;
            }
        }

        // Fallback: Dominação total
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
        SceneManager.LoadScene(2);
    }

    // Retorna true se o jogador tiver pelo menos 1 território
    bool JogadorEstaVivo(Player jogador)
    {
        return todosOsTerritorios.Any(t => t.donoDoTerritorio == jogador);
    }

    #endregion

    #region SISTEMA DE CARTAS E TROCAS

    void InicializarBaralho()
    {
        baralho = new List<Carta>();
        descarte = new List<Carta>();

        // Padrão de símbolos para distribuir equilibradamente
        Simbolo[] padraoSimbolos = { Simbolo.Quadrado, Simbolo.Triangulo, Simbolo.Circulo };
        int indexSimbolo = 0;

        foreach (var territorio in todosOsTerritorios)
        {
            // Cria uma carta para cada território
            baralho.Add(new Carta(padraoSimbolos[indexSimbolo], territorio));
            
            // Cicla entre 0, 1, 2 (Quadrado, Triangulo, Circulo)
            indexSimbolo = (indexSimbolo + 1) % 3;
        }

        // Adiciona 2 Coringas (sem território associado)
        baralho.Add(new Carta(Simbolo.Coringa, null));
        baralho.Add(new Carta(Simbolo.Coringa, null));

        EmbaralharDeck();
    }

    void EmbaralharDeck()
    {
        // Algoritmo Fisher-Yates para embaralhar
        for (int i = 0; i < baralho.Count; i++)
        {
            Carta temp = baralho[i];
            int randomIndex = Random.Range(i, baralho.Count);
            baralho[i] = baralho[randomIndex];
            baralho[randomIndex] = temp;
        }
    }

    public void DarCartaAoJogador(Player jogador)
    {
        if (baralho.Count == 0)
        {
            // Se o baralho acabou, recicla o descarte
            if (descarte.Count > 0)
            {
                baralho.AddRange(descarte);
                descarte.Clear();
                EmbaralharDeck();
            }
            else
            {
                Debug.LogWarning("Não há mais cartas no jogo!");
                return;
            }
        }

        Carta cartaComprada = baralho[0];
        baralho.RemoveAt(0);
        jogador.maoDeCartas.Add(cartaComprada);
        Debug.Log($"Jogador {jogador.nome} recebeu a carta: {cartaComprada.simbolo} ({cartaComprada.territorioAssociado?.name ?? "Coringa"})");
    }

    // --- FUNÇÃO PRINCIPAL QUE VOCÊ VAI CHAMAR NO BOTÃO DE TROCA ---
    public bool TentarRealizarTroca(List<Carta> cartasSelecionadas)
    {
        if (cartasSelecionadas.Count != 3)
        {
            Debug.Log("Você precisa selecionar exatamente 3 cartas.");
            return false;
        }

        if (ValidarCombinacao(cartasSelecionadas))
        {
            // 1. Calcula exércitos ganhos
            int exercitosGanhos = CalcularExercitosDaTroca();
            
            // 2. Adiciona aos reforços do jogador
            reforcosPendentes += exercitosGanhos;
            
            // 3. Incrementa o contador global de trocas
            numeroDeTrocasRealizadas++;

            // 4. Verifica Bônus de Território (+2)
            // Regra: Se a carta trocada for de um território que o jogador possui, ganha +2 tropas LÁ.
            foreach (Carta c in cartasSelecionadas)
            {
                if (c.territorioAssociado != null && c.territorioAssociado.donoDoTerritorio == jogadorAtual)
                {
                    Debug.Log($"Bônus de território! +2 exércitos em {c.territorioAssociado.name}");
                    c.territorioAssociado.numeroDeTropas += 2;
                    c.territorioAssociado.AtualizarVisual();
                }
            }

            // 5. Remove cartas da mão e joga no descarte
            foreach (Carta c in cartasSelecionadas)
            {
                jogadorAtual.maoDeCartas.Remove(c);
                descarte.Add(c);
            }

            // Atualiza UI
            if (textoReforcosPendentes != null) 
                textoReforcosPendentes.text = reforcosPendentes.ToString();
            
            Debug.Log($"Troca realizada! Ganhou {exercitosGanhos} exércitos.");
            return true;
        }
        else
        {
            Debug.Log("Combinação inválida de cartas.");
            return false;
        }
    }

    // Lógica Matemática da Troca (4, 6, 8, 10, 12, 15, 20, 25...)
    private int CalcularExercitosDaTroca()
    {
        int n = numeroDeTrocasRealizadas; // Primeira troca é index 0

        if (n < 5)
        {
            // 0->4, 1->6, 2->8, 3->10, 4->12
            return 4 + (2 * n);
        }
        else
        {
            // 5->15, 6->20, 7->25...
            return 15 + ((n - 5) * 5);
        }
    }

    // Lógica das Formas Geométricas
    private bool ValidarCombinacao(List<Carta> cartas)
    {
        bool temCoringa = cartas.Any(c => c.simbolo == Simbolo.Coringa);
        
        if (temCoringa) return true; // Coringa valida qualquer trio no War tradicional

        // Sem coringa, verifica formas
        Simbolo s1 = cartas[0].simbolo;
        Simbolo s2 = cartas[1].simbolo;
        Simbolo s3 = cartas[2].simbolo;

        bool todasIguais = (s1 == s2 && s2 == s3);
        bool todasDiferentes = (s1 != s2 && s1 != s3 && s2 != s3);

        return todasIguais || todasDiferentes;
    }

    #endregion
}