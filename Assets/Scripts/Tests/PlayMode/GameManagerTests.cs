using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NSubstitute;


public class GameManagerTests
{
    private GameObject gameManagerGO;
    private GameManager gameManager;
    private GameObject battleManagerGO;
    private IUIManager mockUI;
    private Player p1;
    private Player p2;

    private GameObject t1GO;
    private TerritorioHandler t_Atacante;

    private GameObject t2GO;
    private TerritorioHandler t_Defensor;

    [SetUp]
    public void Setup()
    {
        if(BattleManager.instance != null)
        {
            if(BattleManager.instance.gameObject != null)
            {
                Object.DestroyImmediate(BattleManager.instance.gameObject);
            }
            BattleManager.instance = null;
        }

        if(GameManager.instance != null)
        {
            if (GameManager.instance.gameObject != null)
            {
                Object.DestroyImmediate(GameManager.instance.gameObject);
            }
            GameManager.instance = null;
        }

        gameManagerGO = new GameObject();
        // --- Mock do UIManager ---
        mockUI = Substitute.For<IUIManager>();

        // --- Stub do UIManager (Singleton) ---
        var uiGO = new GameObject("UIManagerStub");
        var uiManagerComponent = uiGO.AddComponent<UIManager>();
        UIManager.instance = uiManagerComponent;

        uiManagerComponent.textoNomeJogador = new GameObject("TxtNome").AddComponent<TMPro.TextMeshProUGUI>();
        uiManagerComponent.textoStatus = new GameObject("TxtStatus").AddComponent<TMPro.TextMeshProUGUI>();
        uiManagerComponent.textoBotaoAvancarFase = new GameObject("TxtBotao").AddComponent<TMPro.TextMeshProUGUI>();
        uiManagerComponent.iconeStatusAttack = new GameObject("ImgAtk").AddComponent<UnityEngine.UI.Image>();
        uiManagerComponent.iconeStatusDeploy = new GameObject("ImgDep").AddComponent<UnityEngine.UI.Image>();
        uiManagerComponent.iconeStatusMove = new GameObject("ImgMov").AddComponent<UnityEngine.UI.Image>();
        uiManagerComponent.iconeSoldier = new GameObject("ImgSol").AddComponent<UnityEngine.UI.Image>();


        // --- Stub do BattleManager ---
        battleManagerGO = new GameObject("BattleManager");
        var bm = battleManagerGO.AddComponent<BattleManager>();
        bm.painelBatalha = new GameObject("PainelFake");
        bm.painelBatalha.SetActive(false);
        var textoGO = new GameObject("TextoFake");
        bm.textoResultadoBatalha = textoGO.AddComponent<TMPro.TextMeshProUGUI>();
        bm.imagensDadosAtaque = new UnityEngine.UI.Image[0];
        bm.imagensDadosDefesa = new UnityEngine.UI.Image[0];
        BattleManager.instance = bm;


        gameManagerGO = new GameObject("GameManager");

        gameManagerGO.SetActive(false);

        gameManager = gameManagerGO.AddComponent<GameManager>();

        gameManager.battleManager = bm;
        gameManager.uiManager = mockUI;

        // Configurar Jogadores
        p1 = new Player("P1", Color.blue, "Azul");
        p2 = new Player("P2", Color.red, "Vermelho");
        gameManager.todosOsJogadores = new List<Player> { p1, p2 };

        // Configurar Territórios
        t1GO = new GameObject("TerritorioAtacante");
        t1GO.AddComponent<SpriteRenderer>();
        t1GO.AddComponent<PolygonCollider2D>();
        t_Atacante = t1GO.AddComponent<TerritorioHandler>();
        t_Atacante.donoDoTerritorio = p1; // Já define o dono
        t_Atacante.numeroDeTropas = 1;

        t2GO = new GameObject("TerritorioDefensor");
        t2GO.AddComponent<SpriteRenderer>();
        t2GO.AddComponent<PolygonCollider2D>();
        t_Defensor = t2GO.AddComponent<TerritorioHandler>();
        t_Defensor.donoDoTerritorio = p2; // Já define o dono
        t_Defensor.numeroDeTropas = 1;

        t_Atacante.vizinhos = new List<TerritorioHandler>();
        t_Defensor.vizinhos = new List<TerritorioHandler>();

        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_Atacante, t_Defensor };

        gameManagerGO.SetActive(true);

        gameManager.jogadorAtual = p1;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(gameManagerGO);
    }

    /*
     * FUNÇÃO: OnBotaoAvancarFaseClicado()
     * Complexidade Ciclomática: 6
     */

    // -- TESTE 1: Complexidade do IF --
    [Test]
    public void AvancarFase_NaAlocacaoComReforcos_NaoDeveAvancar()
    {
        gameManager.faseAtual = GameManager.GamePhase.Alocacao;
        gameManager.reforcosPendentes = 5;

        gameManager.OnBotaoAvancarFaseClicado();

        Assert.AreEqual(GameManager.GamePhase.Alocacao, gameManager.faseAtual);

        mockUI.DidNotReceive().AtualizarPainelStatus(Arg.Any<GameManager.GamePhase>(), Arg.Any<Player>());
    }

    // -- TESTE 2: Alocação -> Ataque (Switch Case 1) --
    [Test]
    public void AvancarFase_NaAlocacaoSemReforcos_DeveIrParaAtaque()
    {
        gameManager.faseAtual = GameManager.GamePhase.Alocacao;
        gameManager.reforcosPendentes = 0;

        gameManager.OnBotaoAvancarFaseClicado();

        Assert.AreEqual(GameManager.GamePhase.Ataque, gameManager.faseAtual);
    }

    // -- TESTE 3: Ataque -> Remanejamento (Switch Case 2) --
    [Test]
    public void AvancarFase_NoAtaque_DeveIrParaRemanejamento() 
    {
        gameManager.faseAtual = GameManager.GamePhase.Ataque;

        gameManager.OnBotaoAvancarFaseClicado();

        Assert.AreEqual(GameManager.GamePhase.Remanejamento, gameManager.faseAtual);
    }

    // -- TESTE 4: Remanejamento -> Próximo Turno (Switch Case 3) --
    [Test]
    public void AvancarFase_NoRemanejamento_DeveTrocarDeJogador()
    {
        gameManager.faseAtual = GameManager.GamePhase.Remanejamento;
        gameManager.jogadorAtual = p1;

        gameManager.OnBotaoAvancarFaseClicado();

        Assert.AreEqual(p2, gameManager.jogadorAtual);
        Assert.AreEqual(GameManager.GamePhase.Alocacao, gameManager.faseAtual);
    }

    // -- TESTE 5: Robustez do IF (Lógica Booleana) --
    [Test]
    public void AvancarFase_NoAtaqueComReforcos_DeveAvancarMesmoAssim()
    {
        gameManager.faseAtual = GameManager.GamePhase.Ataque;
        gameManager.reforcosPendentes = 10;

        gameManager.OnBotaoAvancarFaseClicado();

        Assert.AreEqual(GameManager.GamePhase.Remanejamento, gameManager.faseAtual);
    }

    // -- TESTE 6: Validação de Chamada de UI (Integração com Interface) --
    [Test]
    public void AvancarFase_Sucesso_DeveChamarUIManager()
    {
        gameManager.faseAtual = GameManager.GamePhase.Ataque;

        gameManager.OnBotaoAvancarFaseClicado();

        mockUI.Received().AtualizarPainelStatus(GameManager.GamePhase.Remanejamento, p1);
    }

    [Test]
    public void AvancarFase_Bloqueia_SeTiverReforcosPendentes()
    {
        gameManager.faseAtual = GameManager.GamePhase.Alocacao;
        gameManager.reforcosPendentes = 5;

        gameManager.OnBotaoAvancarFaseClicado();

        Assert.AreEqual(GameManager.GamePhase.Alocacao, gameManager.faseAtual);
    }

    [Test]
    public void AvancarFase_Alocacao_Para_Ataque_SeSemReforcos()
    {
        gameManager.faseAtual = GameManager.GamePhase.Alocacao;
        gameManager.reforcosPendentes = 0;

        gameManager.OnBotaoAvancarFaseClicado();

        Assert.AreEqual(GameManager.GamePhase.Ataque, gameManager.faseAtual);
    }

    [Test]
    public void AvancarFase_Remanejamento_TrocaDeJogador()
    {
        gameManager.faseAtual = GameManager.GamePhase.Remanejamento;
        gameManager.jogadorAtual = p1;

        gameManager.OnBotaoAvancarFaseClicado();

        Assert.AreEqual(p2, gameManager.jogadorAtual);

        Assert.AreEqual(GameManager.GamePhase.Alocacao, gameManager.faseAtual);
    }

    /*
     * FUNÇÃO: OnTerritorioClicado(TerritorioHandler)
     * Complexidade Ciclomática: 6
     */

    // -- TESTE 1: Jogo Pausado --
    [Test]
    public void OnTerritorioClicado_JogoPausado_NaoFazNada()
    {
        gameManager.faseAtual = GameManager.GamePhase.JogoPausado;

        gameManager.reforcosPendentes = 5;
        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 1;

        gameManager.OnTerritorioClicado(t_Atacante);

        Assert.AreEqual(1, t_Atacante.numeroDeTropas, "Não deveria colocar tropas se o jogo está pausado.");

    }

    // -- TESTE 2: Turno da IA --
    [Test]
    public void OnTerritorioClicado_TurnoIA_NaoFazNada()
    {
        gameManager.faseAtual = GameManager.GamePhase.Alocacao;
        gameManager.reforcosPendentes = 5;

        Player botPlayer = new Player("Bot", Color.black, "Preto", true);
        gameManager.jogadorAtual = botPlayer;
        t_Atacante.donoDoTerritorio = botPlayer;
        t_Atacante.numeroDeTropas = 1;

        gameManager.OnTerritorioClicado(t_Atacante);

        Assert.AreEqual(1, t_Atacante.numeroDeTropas, "Não deveria processar clique no turno da IA.");
    }

    // -- TESTE 3: Fase Alocação --
    [Test]
    public void OnTerritorioClicado_FaseAlocacao_ChamaLogicaAlocacao()
    {
        gameManager.faseAtual = GameManager.GamePhase.Alocacao;
        gameManager.jogadorAtual = p1;
        gameManager.reforcosPendentes = 5;
        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 1;

        gameManager.OnTerritorioClicado(t_Atacante);

        Assert.AreEqual(2, t_Atacante.numeroDeTropas);
    }

    // -- TESTE 4: Fase Ataque --
    [Test]
    public void OnTerritorioClicado_FaseAtual_ChamaLogicaAtaque()
    {
        gameManager.faseAtual = GameManager.GamePhase.Ataque;
        gameManager.jogadorAtual = p1;
        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 3;

        gameManager.OnTerritorioClicado(t_Atacante);

        Assert.AreEqual(t_Atacante, gameManager.territorioSelecionado);
    }

    // - TESTE 5: Fase Remanejamento --
    [Test]
    public void OnTerritorioClicado_FaseRemanejamento_ChamaLogicaRemanejamento()
    {
        gameManager.faseAtual = GameManager.GamePhase.Remanejamento;
        gameManager.jogadorAtual = p1;
        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 2;

        gameManager.OnTerritorioClicado(t_Atacante);

        Assert.AreEqual(t_Atacante, gameManager.territorioSelecionado);
    }

    /*
     * FUNÇÃO: HandleCliqueAtaque(TerritorioHandler)
     * Complexidade Ciclomática: 9
     */
    [UnityTest]
    public IEnumerator AoClicarNoMesmoTerritorio_DeveDeselecionar()
    {
        gameManager.faseAtual = GameManager.GamePhase.Ataque;
        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 2;

        gameManager.territorioSelecionado = t_Atacante;

        gameManager.OnTerritorioClicado(t_Atacante);

        yield return null;

        Assert.IsNull(gameManager.territorioSelecionado, "O território deveria ter sido deselecionado");
    }

    // -- TESTE 2: Trocar Seleção --
    [UnityTest]
    public IEnumerator AoClicarEmOutroTerritorioProprio_DeveTrocarSelecao()
    {

        gameManager.faseAtual = GameManager.GamePhase.Ataque;


        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 2;
        gameManager.territorioSelecionado = t_Atacante;

        t_Defensor.donoDoTerritorio = p1; 
        t_Defensor.numeroDeTropas = 3;


        gameManager.OnTerritorioClicado(t_Defensor);

        yield return null;


        Assert.AreEqual(t_Defensor, gameManager.territorioSelecionado, "A seleção deveria ter trocado para o novo território.");
    }

    [UnityTest]
    public IEnumerator AoClicarEmTerritorioCom1Tropa_NaoDeveSelecionar()
    {
    
        gameManager.faseAtual = GameManager.GamePhase.Ataque;
        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 1; // Pouca tropa!
        gameManager.territorioSelecionado = null;


        gameManager.OnTerritorioClicado(t_Atacante);

        yield return null;

        Assert.IsNull(gameManager.territorioSelecionado, "Não deveria selecionar território com apenas 1 tropa (não pode atacar).");
    }

    [UnityTest]
    public IEnumerator AoClicarEmVizinhoInimigo_DeveIniciarBatalha()
    {

        gameManager.faseAtual = GameManager.GamePhase.Ataque;

        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 3;

        t_Defensor.donoDoTerritorio = p2;

        t_Atacante.vizinhos = new List<TerritorioHandler> { t_Defensor };

        gameManager.OnTerritorioClicado(t_Atacante);

        gameManager.OnTerritorioClicado(t_Defensor);

        yield return null;

        Assert.AreEqual(t_Defensor, gameManager.territorioAlvo);
        Assert.AreEqual(GameManager.GamePhase.JogoPausado, gameManager.faseAtual);
    }

    [UnityTest]
    public IEnumerator AoClicarEmInimigoLonge_NaoDeveAtacar()
    {

        gameManager.faseAtual = GameManager.GamePhase.Ataque;
        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 3;
        t_Defensor.donoDoTerritorio = p2;


        t_Atacante.vizinhos = new List<TerritorioHandler>();

        gameManager.OnTerritorioClicado(t_Atacante); 
        gameManager.OnTerritorioClicado(t_Defensor); 

        yield return null;


        Assert.IsNull(gameManager.territorioSelecionado);
        Assert.AreEqual(GameManager.GamePhase.Ataque, gameManager.faseAtual);
    }

    // -- TESTE 6: Trocar para Território Inválido
    [UnityTest]
    public IEnumerator AoTrocarTerritorioCom1Tropa_DeveApenasDeselecionar()
    {
        gameManager.faseAtual = GameManager.GamePhase.Ataque;

        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 3;
        gameManager.territorioSelecionado = t_Atacante;

        t_Defensor.donoDoTerritorio = p1;
        t_Defensor.numeroDeTropas = 1;

        gameManager.OnTerritorioClicado(t_Defensor);

        yield return null;

        Assert.IsNull(gameManager.territorioSelecionado, "A seleção deveria ter sido limpa.");
    }

    [UnityTest]
    public IEnumerator AoIniciarBatalha_ComErro_DeveResetarParaAtaque()
    {
        gameManager.faseAtual = GameManager.GamePhase.Ataque;

        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 3;
        t_Defensor.donoDoTerritorio = p2;
        t_Atacante.vizinhos = new List<TerritorioHandler> { t_Defensor };

        Object.DestroyImmediate(battleManagerGO);

        gameManager.OnTerritorioClicado(t_Atacante);

        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Erro batalha:.*"));

        gameManager.OnTerritorioClicado(t_Defensor);

        yield return null;

        Assert.AreEqual(GameManager.GamePhase.Ataque, gameManager.faseAtual);

        Assert.IsNull(gameManager.territorioSelecionado);
    }
    /*
     * FUNÇÃO: HandleCliqueRemanejamento(TerritorioHandler)
     * Complexidade Ciclomática: 8
     */

    // -- TESTE 1: Cobre o primeiro if e o if(tropas > 1)
    [UnityTest]
    public IEnumerator Remanejar_SelecionarOrigem_ComMaisDeUmaTropa_Sucesso()
    {
        gameManager.faseAtual = GameManager.GamePhase.Remanejamento;
        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 2;
        gameManager.territorioSelecionado = null;

        gameManager.OnTerritorioClicado(t_Atacante);


        yield return null;

        Assert.AreEqual(t_Atacante, gameManager.territorioSelecionado, "Deveria ter selecionado o territorio de origem.");
    }

    // -- TESTE 2: Cobre o else implicito do if(tropas > 1) --
    [UnityTest]
    public IEnumerator Remanejar_NaoSeleciona_SeTiverApenasUmaTropa()
    {
        gameManager.faseAtual = GameManager.GamePhase.Remanejamento;
        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 1;
        gameManager.territorioSelecionado = null;

        gameManager.OnTerritorioClicado(t_Atacante);

        yield return null;

        Assert.IsNull(gameManager.territorioSelecionado, "Não deveria selecionar território com 1 tropa.");
    }

    // -- TESTE 3: Cobre if(territorioClicado == territorioSelecionado)
    [UnityTest]
    public IEnumerator Remanejar_AoClicarNoMesmo_DeveDeselecionar()
    {
        gameManager.faseAtual = GameManager.GamePhase.Remanejamento;
        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 3;

        gameManager.territorioSelecionado = t_Atacante;

        gameManager.OnTerritorioClicado(t_Atacante);

        yield return null;

        Assert.IsNull(gameManager.territorioSelecionado, "Deveria ter limpado  a seleção.");
    }

    // -- TESTE: Cobre o else if(dono == jogadorAtual), a verificação de vizinhos, a movimentação e o OnBotaoAvancarFaseClicado --
    [UnityTest]
    public IEnumerator Remanejar_MovimentoValido_DeveMoverTropasEAvancarFase()
    {
        gameManager.faseAtual = GameManager.GamePhase.Remanejamento;
        gameManager.jogadorAtual = p1;

        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 3;

        t_Defensor.donoDoTerritorio = p1;
        t_Defensor.numeroDeTropas = 1;

        var t3GO = new GameObject("TerritorioP2_Sobrevivencia");
        t3GO.AddComponent<SpriteRenderer>();
        t3GO.AddComponent<PolygonCollider2D>();
        var t_Sobrevivencia = t3GO.AddComponent<TerritorioHandler>();
        t_Sobrevivencia.donoDoTerritorio = p2;

        gameManager.todosOsTerritorios.Add(t_Sobrevivencia);

        t_Atacante.vizinhos = new List<TerritorioHandler> { t_Defensor };

        gameManager.territorioSelecionado = t_Atacante;

        gameManager.OnTerritorioClicado(t_Defensor);

        yield return null;

        Assert.AreEqual(2, t_Atacante.numeroDeTropas, "Origem deveria perder 1 tropa.");
        Assert.AreEqual(2, t_Defensor.numeroDeTropas, "Destino deveria ganhar 1 tropa");

        Assert.IsNull(gameManager.territorioSelecionado);

        Assert.AreEqual(GameManager.GamePhase.Alocacao, gameManager.faseAtual);
    }

    // -- TESTE 5: Cobre o else do if(vizinhos.Contains)
    [UnityTest]
    public IEnumerator Remanejar_Falha_SeDestinoNaoForVizinho()
    {
        gameManager.faseAtual = GameManager.GamePhase.Remanejamento;

        t_Atacante.donoDoTerritorio = p1;
        t_Atacante.numeroDeTropas = 3;
        t_Defensor.donoDoTerritorio = p1;

        t_Atacante.vizinhos = new List<TerritorioHandler>();

        gameManager.territorioSelecionado = t_Atacante;

        gameManager.OnTerritorioClicado(t_Defensor);

        yield return null;

        Assert.AreEqual(3, t_Atacante.numeroDeTropas);
        Assert.IsNull(gameManager.territorioSelecionado, "Deveria deselecionar ao tentar movimento inválido.");
    }

    /*
    * FUNÇÃO: InicializarEAssinlarObjetivos()
    * Complexidade Ciclomática: 8
    */

    // -- TESTE 1: Cobre o loop principal
    [Test]
    public void InicializarObjetivos_DeveAtribuirObjetivosParaTodos()
    {
        var p3 = new Player("P3", Color.green, "Verde");
        gameManager.todosOsJogadores = new List<Player> { p1, p2, p3 };

        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_Atacante, t_Defensor };

        gameManager.InicializarEAssinlarObjetivos();

        foreach(var jogador in gameManager.todosOsJogadores)
        {
            Assert.IsNotNull(jogador.objetivoSecreto, $"O jogador {jogador.nome} ficou sem objetivo!");
            Debug.Log($"Objetivo de {jogador.nome}: {jogador.objetivoSecreto.Descricao}");
        }
    }

    // -- TESTE 2: Cobre o if/continue da auto destruição
    [Test]
    public void InicializarObjetivos_NinguemDeveTerObjetivoDeDestruirSiMesmo()
    {
        gameManager.todosOsJogadores = new List<Player> { p1, p2 };
        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_Atacante };

        gameManager.InicializarEAssinlarObjetivos();

        foreach(var jogador in gameManager.todosOsJogadores)
        {
            if(jogador.objetivoSecreto is ObjetivoDestruirJogador objDestruir)
            {
                Assert.AreNotEqual(jogador, objDestruir.JogadorAlvo, $"ERRO: {jogador.nome} recebeu objetivo de destruir a si mesmo.");
            }
        }
    }

    // -- TESTE 3: Verifica se os tipos corretos estão sendo criados
    [Test]
    public void InicializarObjetivos_DeveCriarObjetivosDeConquistaEDestruicao()
    {
        gameManager.todosOsJogadores = new List<Player> { p1, p2 };
        gameManager.todosOsTerritorios = new List<TerritorioHandler>();

        bool gerouDestruicao = false;
        bool gerouConquista = false;

        for(int i=0; i<10; i++)
        {
            gameManager.InicializarEAssinlarObjetivos();

            if (p1.objetivoSecreto is ObjetivoDestruirJogador) gerouDestruicao = true;
            if (p1.objetivoSecreto is ObjetivoConquistarNTerritorios) gerouConquista = true;
        }

        Assert.IsTrue(gerouDestruicao, "Deveria ser capaz de gerar objetivos de Destruição.");
        Assert.IsTrue(gerouConquista, "Deveria ser capaz de gerar objetivos de Conquista.");
    }

    /*
     *FUNÇÃO: ChecarVitoria()
     *Complexidade Ciclomática: 9
     */

    // -- TESTE 1: Vitória por objetivo secreto
    [UnityTest]
    public IEnumerator ChecarVitoria_ObjetivoConcluido_DeveAnunciarVencedor()
    {
        p1.objetivoSecreto = new ObjetivoConquistarNTerritorios(2, "Conquistar 2");

        t_Atacante.donoDoTerritorio = p1;
        t_Defensor.donoDoTerritorio = p1;

        gameManager.ChecarVitoria();

        yield return null;

        Assert.AreEqual(GameManager.GamePhase.JogoPausado, gameManager.faseAtual);
        Assert.AreEqual(p1.nome, VencedorInfo.nomeVencedor);
    }

    // -- TESTE 2: Vitória por dominação
    [UnityTest]
    public IEnumerator ChecarVitoria_DominacaoTotal_DeveAnunciarVencedor()
    {
        p1.objetivoSecreto = new ObjetivoConquistarNTerritorios(99, "Impossível");

        t_Atacante.donoDoTerritorio = p1;
        t_Defensor.donoDoTerritorio = p1;

        gameManager.ChecarVitoria();

        yield return null;

        Assert.AreEqual(GameManager.GamePhase.JogoPausado, gameManager.faseAtual);
        Assert.AreEqual(p1.nome, VencedorInfo.nomeVencedor);
    }

    /*
     *FUNÇÃO: DistribuirTerritoriosIniciais()
     *Complexidade Ciclomática: 2
     *Acoplamento de Classes: 8
     */

    // -- TESTE 1: Verifica se todos receberam dono e tropa inicial
    [UnityTest]
    public IEnumerator DistribuiTerritorios_DeveAtribuirDonoETropasParaTodos()
    {
        t_Atacante.donoDoTerritorio = null;
        t_Atacante.numeroDeTropas = 0;
        t_Defensor.donoDoTerritorio = null;
        t_Defensor.numeroDeTropas = 0;

        gameManager.DistribuirTerritoriosIniciais();

        yield return null;

        Assert.IsNotNull(t_Atacante.donoDoTerritorio, "Território 1 ficou sem dono.");
        Assert.AreEqual(1, t_Atacante.numeroDeTropas, "Território 1 deveria ter 1 tropa inicial.");

        Assert.IsNotNull(t_Defensor.donoDoTerritorio, "Território 2 ficou sem dono.");
        Assert.AreEqual(1, t_Defensor.numeroDeTropas, "Territorio 2 deveria ter 1 tropa inicial.");
    }

    // -- TESTE 2: Verifica o equilíbrio da distribuição
    [UnityTest]
    public IEnumerator DistribuirTerritorios_DeveDividirIgualmenteEntreJogadores()
    {
        var t3GO = new GameObject("T3");
        t3GO.AddComponent<SpriteRenderer>();
        t3GO.AddComponent<PolygonCollider2D>();
        var t3 = t3GO.AddComponent <TerritorioHandler>();
        var t4GO = new GameObject("T4");
        t4GO.AddComponent<SpriteRenderer>();
        t4GO.AddComponent<PolygonCollider2D>();
        var t4 = t4GO.AddComponent<TerritorioHandler>();

        gameManager.todosOsTerritorios.Add(t3);
        gameManager.todosOsTerritorios.Add(t4);

        gameManager.DistribuirTerritoriosIniciais();

        yield return null;

        int contagemP1 = 0;
        int contagemP2 = 0;

        foreach(var t in gameManager.todosOsTerritorios)
        {
            if (t.donoDoTerritorio == p1) contagemP1++;
            if (t.donoDoTerritorio == p2) contagemP2++;
        }

        Assert.AreEqual(2, contagemP1, "Jogador 1 deveria ter 2 territórios.");
        Assert.AreEqual(2, contagemP2, "Jogador 2 deveria ter 2 territórios.");

        Object.DestroyImmediate(t3GO);
        Object.DestroyImmediate(t4GO);
    }

}
