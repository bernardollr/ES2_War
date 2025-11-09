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
        JogoPausado     // Usado durante a batalha
    }

    [Header("Controle de Jogo")]
    public Player jogador1;
    public Player jogador2;
    public Player jogadorAtual;
    
    [Tooltip("A fase atual do turno do jogador")]
    public GamePhase faseAtual;
    
    [Tooltip("Quantas tropas o jogador ainda pode alocar")]
    public int reforcosPendentes; 

    [Header("Gerenciamento de Seleção")]
    public TerritorioHandler territorioSelecionado;
    public TerritorioHandler territorioAlvo;
    
    [Header("Referências da UI")]
    public List<TerritorioHandler> todosOsTerritorios;
    
    [Tooltip("Arraste o Botão 'Passar Turno / Próxima Fase'")]
    public Button botaoAvancarFase;

    [Tooltip("Arraste o seu BattleManager para cá")]
    public BattleManager battleManager;
    

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        // Auto-assign BattleManager if it's on the same GameObject but wasn't set in the Inspector
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
        jogador1 = new Player("Jogador 1", Color.blue);
        jogador2 = new Player("Jogador 2", Color.red);
        jogadorAtual = jogador1; 

        todosOsTerritorios = FindObjectsByType<TerritorioHandler>(FindObjectsSortMode.None).ToList();
        DistribuirTerritoriosIniciais();

        // Inicia o primeiro turno
        IniciarNovoTurno(); 
        
        Debug.Log("GameManager iniciado.");
        PrintTerritoriosPorJogador();
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
                //botaoAvancarFase.text
                Debug.Log("Fase alterada para: Remanejamento");
                break;

            case GamePhase.Remanejamento:
                MudarParaProximoJogador();
                return; // Sai da função, MudarParaProximoJogador chamará IniciarNovoTurno()
        }
        UIManager.instance.AtualizarPainelStatus(faseAtual, jogadorAtual);
    }

    // Função que troca o jogador e inicia o próximo turno
    void MudarParaProximoJogador()
    {
        jogadorAtual = (jogadorAtual == jogador1) ? jogador2 : jogador1;
        Debug.Log("--- FIM DO TURNO. AGORA É O TURNO DE: " + jogadorAtual.nome + " ---");
        
        ChecarVitoria();
        IniciarNovoTurno();
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

    // LÓGICA DE ALOCAÇÃO
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
            UIManager.instance.AtualizarPainelStatus(faseAtual, jogadorAtual); // Atualiza UI para mostrar reforços restantes
            Debug.Log($"Reforço alocado em {territorio.name}. Restam {reforcosPendentes}.");
        }
        else
        {
            Debug.Log("Você só pode alocar tropas em seus próprios territórios.");
        }
    }

    // LÓGICA DE ATAQUE
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

    // LÓGICA DE REMANEJAMENTO
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
                        // Avançamos para o próximo jogador.
                        //OnBotaoAvancarFaseClicado(); 
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

    void DistribuirTerritoriosIniciais()
    {
        List<TerritorioHandler> territoriosEmbaralhados = todosOsTerritorios.OrderBy(a => Random.value).ToList();
        int jogadorIndex = 0;
        foreach (var territorio in territoriosEmbaralhados)
        {
            Player dono = (jogadorIndex % 2 == 0) ? jogador1 : jogador2;
            territorio.donoDoTerritorio = dono;
            territorio.numeroDeTropas = 1;
            territorio.AtualizarVisual(); // Garante que o contador inicial (1) apareça
            jogadorIndex++;
        }
        Debug.Log("Territórios iniciais distribuídos!");
    }

    void PrintTerritoriosPorJogador()
    {
        Debug.Log($"=== Territórios do {jogador1.nome} ===");
        foreach (var t in todosOsTerritorios.Where(t => t.donoDoTerritorio == jogador1))
        {
            Debug.Log($"- {t.name} com {t.numeroDeTropas} tropa(s)");
        }
        Debug.Log($"=== Territórios do {jogador2.nome} ===");
        foreach (var t in todosOsTerritorios.Where(t => t.donoDoTerritorio == jogador2))
        {
            Debug.Log($"- {t.name} com {t.numeroDeTropas} tropa(s)");
        }
    }
    
    void AtualizarPlayerDoTurnoNosTerritorios()
    {
        foreach (var territorio in todosOsTerritorios)
        {
            territorio.playerDoTurno = jogadorAtual;
        }
    }

    public void ChecarVitoria()
    {
        if (todosOsTerritorios.Count == 0) return;
        Player donoReferencia = todosOsTerritorios[0].donoDoTerritorio;
        foreach (var territorio in todosOsTerritorios)
        {
            if (territorio.donoDoTerritorio != donoReferencia)
                return; 
        }
        Debug.Log("Jogo acabou! Vencedor: " + donoReferencia.nome);
        VencedorInfo.nomeVencedor = donoReferencia.nome;
        VencedorInfo.corVencedor = donoReferencia.cor;
        SceneManager.LoadScene(2); 
    }

    #endregion
}