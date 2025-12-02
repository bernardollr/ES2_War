using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NSubstitute;
using UnityEngine.UI;
using TMPro;

public class AITests
{
    private GameObject gameManagerGO;
    private GameManager gameManager;
    private AIController aiController;

    private GameObject t1GO, t2GO;
    private TerritorioHandler t_OrigemIA, t_Vizinho;
    private Player botPlayer, humanPlayer;

    [SetUp]
    public void Setup()
    {
        if (GameManager.instance != null) Object.DestroyImmediate(GameManager.instance.gameObject);
        if (BattleManager.instance != null) Object.DestroyImmediate(BattleManager.instance.gameObject);
        if (UIManager.instance != null) Object.DestroyImmediate(UIManager.instance.gameObject);

        var uiGO = new GameObject("UIStub");
        var ui = uiGO.AddComponent<UIManager>();
        ui.textoNomeJogador = new GameObject().AddComponent<TextMeshProUGUI>();
        ui.textoStatus = new GameObject().AddComponent<TextMeshProUGUI>();
        ui.textoBotaoAvancarFase = new GameObject().AddComponent<TextMeshProUGUI>();
        ui.iconeStatusAttack = new GameObject().AddComponent<Image>();
        ui.iconeStatusDeploy = new GameObject().AddComponent<Image>();
        ui.iconeStatusMove = new GameObject().AddComponent<Image>();
        ui.iconeSoldier = new GameObject().AddComponent<Image>();
        UIManager.instance = ui;

        var bmGO = new GameObject("BattleManager");
        var bm = bmGO.AddComponent<BattleManager>();
        bm.painelBatalha = new GameObject(); bm.painelBatalha.SetActive(false);
        bm.textoResultadoBatalha = new GameObject().AddComponent<TextMeshProUGUI>();
        bm.imagensDadosAtaque = new Image[0];
        bm.imagensDadosDefesa = new Image[0];
        BattleManager.instance = bm;

        gameManagerGO = new GameObject("GameManager");
        gameManagerGO.SetActive(false);
        gameManager = gameManagerGO.AddComponent<GameManager>();

        aiController = gameManagerGO.AddComponent<AIController>();

        gameManager.battleManager = bm;
        gameManager.uiManager = ui;
        gameManager.aiController = aiController;
        aiController.gameManager = gameManager;

        botPlayer = new Player("Bot", Color.black, "Preto", true);
        humanPlayer = new Player("Humano", Color.white, "Branco", false);
        gameManager.todosOsJogadores = new List<Player> { botPlayer, humanPlayer };

        t1GO = new GameObject("T_IA");
        t1GO.AddComponent<SpriteRenderer>(); t1GO.AddComponent<PolygonCollider2D>();
        t_OrigemIA = t1GO.AddComponent<TerritorioHandler>();

        t2GO = new GameObject("T_Vizinho");
        t2GO.AddComponent<SpriteRenderer>(); t2GO.AddComponent<PolygonCollider2D>();
        t_Vizinho = t2GO.AddComponent<TerritorioHandler>();

        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_OrigemIA, t_Vizinho };

        gameManagerGO.SetActive(true);
        GameManager.instance = gameManager;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(gameManagerGO);
        Object.DestroyImmediate(t1GO);
        Object.DestroyImmediate(t2GO);
        var ui = GameObject.Find("UIStub"); if (ui) Object.DestroyImmediate(ui);
        var bm = GameObject.Find("BattleManager"); if (bm) Object.DestroyImmediate(bm);
    }

    [UnityTest]
    public IEnumerator IA_Ataque_InimigoMaisFraco_DeveIniciarBatalha()
    {
        t_OrigemIA.donoDoTerritorio = botPlayer;
        t_OrigemIA.numeroDeTropas = 10; 

        t_Vizinho.donoDoTerritorio = humanPlayer;
        t_Vizinho.numeroDeTropas = 1;   

        t_OrigemIA.vizinhos = new List<TerritorioHandler> { t_Vizinho };

        aiController.StartCoroutine("ExecutarAtaques", botPlayer);

        yield return new WaitForSeconds(0.5f);

        Assert.IsTrue(BattleManager.instance.painelBatalha.activeSelf, "A IA deveria ter iniciado uma batalha.");
    }

    [UnityTest]
    public IEnumerator IA_Ataque_InimigoMaisForte_NaoDeveAtacar()
    {
        t_OrigemIA.donoDoTerritorio = botPlayer;
        t_OrigemIA.numeroDeTropas = 2;

        t_Vizinho.donoDoTerritorio = humanPlayer;
        t_Vizinho.numeroDeTropas = 10;

        t_OrigemIA.vizinhos = new List<TerritorioHandler> { t_Vizinho };

        aiController.StartCoroutine("ExecutarAtaques", botPlayer);
        yield return new WaitForSeconds(0.5f);
        Assert.IsFalse(BattleManager.instance.painelBatalha.activeSelf, "A IA não deveria atacar um inimigo mais forte.");
    }

    // TESTE 1: Remanejamento Básico (Cobre o caminho feliz e cálculo de média)
    [UnityTest]
    public IEnumerator IA_Remanejamento_DeveEquilibrarTropas()
    {
        t_OrigemIA.donoDoTerritorio = botPlayer;
        t_OrigemIA.numeroDeTropas = 10;


        t_Vizinho.donoDoTerritorio = botPlayer;
        t_Vizinho.numeroDeTropas = 2;

        t_OrigemIA.vizinhos = new List<TerritorioHandler> { t_Vizinho };
        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_OrigemIA, t_Vizinho };

        aiController.SendMessage("ExecutarRemanejamento", botPlayer);
        yield return null;

        Assert.AreEqual(6, t_OrigemIA.numeroDeTropas, "Origem deveria ter doado.");
        Assert.AreEqual(6, t_Vizinho.numeroDeTropas, "Destino deveria ter recebido.");
    }

    // TESTE 2: Sem Riqueza (Cobre o filtro inicial 'Where tropas > 1')
    [UnityTest]
    public IEnumerator IA_Remanejamento_SemTropasExtras_NaoDeveMover()
    {
        t_OrigemIA.donoDoTerritorio = botPlayer;
        t_OrigemIA.numeroDeTropas = 1;

        t_Vizinho.donoDoTerritorio = botPlayer;
        t_Vizinho.numeroDeTropas = 1;

        t_OrigemIA.vizinhos = new List<TerritorioHandler> { t_Vizinho };
        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_OrigemIA, t_Vizinho };

        aiController.SendMessage("ExecutarRemanejamento", botPlayer);
        yield return null;

        Assert.AreEqual(1, t_OrigemIA.numeroDeTropas);
        Assert.AreEqual(1, t_Vizinho.numeroDeTropas);
    }

    // TESTE 3: Sem Vizinho Amigo (Cobre o 'FirstOrDefault' nulo)
    [UnityTest]
    public IEnumerator IA_Remanejamento_SemVizinhoAmigo_NaoDeveMover()
    {

        t_OrigemIA.donoDoTerritorio = botPlayer;
        t_OrigemIA.numeroDeTropas = 10;

        t_Vizinho.donoDoTerritorio = humanPlayer;
        t_Vizinho.numeroDeTropas = 1;

        t_OrigemIA.vizinhos = new List<TerritorioHandler> { t_Vizinho };
        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_OrigemIA, t_Vizinho };

        aiController.SendMessage("ExecutarRemanejamento", botPlayer);
        yield return null;

        Assert.AreEqual(10, t_OrigemIA.numeroDeTropas, "Não deveria mover para inimigo.");
    }

    // TESTE 4: Já Equilibrado (Cobre o 'if (origem > destino + 1)')
    [UnityTest]
    public IEnumerator IA_Remanejamento_Equilibrado_NaoDeveMover()
    {

        t_OrigemIA.donoDoTerritorio = botPlayer;
        t_OrigemIA.numeroDeTropas = 5;

        t_Vizinho.donoDoTerritorio = botPlayer;
        t_Vizinho.numeroDeTropas = 4; 

        t_OrigemIA.vizinhos = new List<TerritorioHandler> { t_Vizinho };
        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_OrigemIA, t_Vizinho };

        aiController.SendMessage("ExecutarRemanejamento", botPlayer);
        yield return null;

        Assert.AreEqual(5, t_OrigemIA.numeroDeTropas);
        Assert.AreEqual(4, t_Vizinho.numeroDeTropas);
    }

    // TESTE 1: Prioridade ao Mais Fraco (Cobre a lógica do OrderBy)
    [UnityTest]
    public IEnumerator IA_Alocacao_DevePriorizarTerritorioMaisFraco()
    {
        t_OrigemIA.donoDoTerritorio = botPlayer;
        t_OrigemIA.numeroDeTropas = 1;

        t_Vizinho.donoDoTerritorio = botPlayer;
        t_Vizinho.numeroDeTropas = 10;

        gameManager.reforcosPendentes = 5;

        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_OrigemIA, t_Vizinho };

        aiController.SendMessage("ExecutarAlocacao", botPlayer);

        yield return null;
        Assert.AreEqual(0, gameManager.reforcosPendentes, "Deve ter gasto todos os reforços.");
        Assert.AreEqual(6, t_OrigemIA.numeroDeTropas, "Território fraco deveria receber tudo.");
        Assert.AreEqual(10, t_Vizinho.numeroDeTropas, "Território forte não deveria mudar.");
    }

    // TESTE 2: Distribuição Equilibrada (Cobre o loop reavaliando a cada iteração)
    [UnityTest]
    public IEnumerator IA_Alocacao_DeveDistribuirEntreTerritoriosIguais()
    {
        t_OrigemIA.donoDoTerritorio = botPlayer;
        t_OrigemIA.numeroDeTropas = 2;

        t_Vizinho.donoDoTerritorio = botPlayer;
        t_Vizinho.numeroDeTropas = 2;

        gameManager.reforcosPendentes = 2;

        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_OrigemIA, t_Vizinho };

        aiController.SendMessage("ExecutarAlocacao", botPlayer);
        yield return null;

        Assert.AreEqual(0, gameManager.reforcosPendentes);

        Assert.AreEqual(3, t_OrigemIA.numeroDeTropas);
        Assert.AreEqual(3, t_Vizinho.numeroDeTropas);
    }

    // TESTE 3: Segurança / Sem Territórios (Cobre o 'else break')
    [UnityTest]
    public IEnumerator IA_Alocacao_SemTerritorios_NaoDeveEntrarEmLoopInfinito()
    {
        gameManager.reforcosPendentes = 10;

        t_OrigemIA.donoDoTerritorio = humanPlayer;
        t_Vizinho.donoDoTerritorio = humanPlayer;

        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_OrigemIA, t_Vizinho };

        aiController.SendMessage("ExecutarAlocacao", botPlayer);
        yield return null;

        Assert.AreEqual(10, gameManager.reforcosPendentes);
    }
}