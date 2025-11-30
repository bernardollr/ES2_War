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

    



}
