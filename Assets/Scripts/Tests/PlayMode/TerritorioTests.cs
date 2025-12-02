using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class TerritorioTests
{
    private GameObject t1GO, t2GO, t3GO;
    private TerritorioHandler t_Principal, t_VizinhoInimigo, t_VizinhoAmigo;
    private Player p1, p2;
    private GameObject gameManagerGO;

    [SetUp]
    public void Setup()
    {
        if (GameManager.instance != null && GameManager.instance.gameObject != null)
            Object.DestroyImmediate(GameManager.instance.gameObject);
        if (UIManager.instance != null && UIManager.instance.gameObject != null)
            Object.DestroyImmediate(UIManager.instance.gameObject);

        var uiGO = new GameObject("UIStub");
        var uiManager = uiGO.AddComponent<UIManager>();
        uiManager.textoNomeJogador = new GameObject("Txt").AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.textoStatus = new GameObject("Txt").AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.textoBotaoAvancarFase = new GameObject("Txt").AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.iconeStatusAttack = new GameObject("Img").AddComponent<Image>();
        uiManager.iconeStatusDeploy = new GameObject("Img").AddComponent<Image>();
        uiManager.iconeStatusMove = new GameObject("Img").AddComponent<Image>();
        uiManager.iconeSoldier = new GameObject("Img").AddComponent<Image>();
        UIManager.instance = uiManager;

        gameManagerGO = new GameObject("GameManagerStub");
        gameManagerGO.SetActive(false);
        var gm = gameManagerGO.AddComponent<GameManager>();


        var bmGO = new GameObject("BattleManager");
        var bm = bmGO.AddComponent<BattleManager>();
        bm.painelBatalha = new GameObject("Painel"); bm.painelBatalha.SetActive(false);
        bm.textoResultadoBatalha = new GameObject("Txt").AddComponent<TMPro.TextMeshProUGUI>();
        bm.imagensDadosAtaque = new Image[0];
        bm.imagensDadosDefesa = new Image[0];
        gm.battleManager = bm;
        BattleManager.instance = bm;

        gm.uiManager = uiManager;



        p1 = new Player("P1", Color.blue, "Azul");
        p2 = new Player("P2", Color.black, "Preto");

        gm.todosOsJogadores = new List<Player> { p1, p2 };
        gm.jogadorAtual = p1;


        t1GO = CriarTerritorio("Principal", p1);
        t_Principal = t1GO.GetComponent<TerritorioHandler>();

        t2GO = CriarTerritorio("Inimigo", p2);
        t_VizinhoInimigo = t2GO.GetComponent<TerritorioHandler>();

        t3GO = CriarTerritorio("Amigo", p1);
        t_VizinhoAmigo = t3GO.GetComponent<TerritorioHandler>();

        gm.todosOsTerritorios = new List<TerritorioHandler> { t_Principal, t_VizinhoInimigo, t_VizinhoAmigo };

        gameManagerGO.SetActive(true); 

        t_Principal.vizinhos = new List<TerritorioHandler> { t_VizinhoInimigo, t_VizinhoAmigo };
    }

    private GameObject CriarTerritorio(string nome, Player dono)
    {
        var go = new GameObject(nome);
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<PolygonCollider2D>();
        go.AddComponent<BorderScript>();
        var handler = go.AddComponent<TerritorioHandler>();
        handler.donoDoTerritorio = dono;
        handler.borderScript = go.GetComponent<BorderScript>();
        handler.vizinhos = new List<TerritorioHandler>();
        return go;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(t1GO);
        Object.DestroyImmediate(t2GO);
        Object.DestroyImmediate(t3GO);
        Object.DestroyImmediate(gameManagerGO);
        var uiStub = GameObject.Find("UIStub");
        if (uiStub != null) Object.DestroyImmediate(uiStub);
        var bmStub = GameObject.Find("BattleManager");
        if (bmStub != null) Object.DestroyImmediate(bmStub);
    }

    // --- TESTES ---

    [UnityTest]
    public IEnumerator Selecionar_SemHighlight_DevePintarApenasOProprioVerde()
    {

        t_Principal.Selecionar(false);

        yield return null;

        var srPrincipal = t1GO.GetComponent<SpriteRenderer>();
        Assert.AreEqual(Color.green, srPrincipal.color);

        var srInimigo = t2GO.GetComponent<SpriteRenderer>();
        Assert.AreNotEqual(Color.red, srInimigo.color, "Inimigo não deveria ficar vermelho (deve estar Preto).");
    }

    [UnityTest]
    public IEnumerator Selecionar_ComHighlight_DevePintarInimigosDeVermelho()
    {

        t_Principal.Selecionar(true);

        yield return null;

        Assert.AreEqual(Color.green, t1GO.GetComponent<SpriteRenderer>().color);

        var srInimigo = t2GO.GetComponent<SpriteRenderer>();
        Assert.AreEqual(Color.red, srInimigo.color);

        var srAmigo = t3GO.GetComponent<SpriteRenderer>();
        Assert.AreNotEqual(Color.red, srAmigo.color);
    }

    [UnityTest]
    public IEnumerator Desselecionar_DeveLimparCoresEVisibilidade()
    {
        t_Principal.Selecionar(true);
        yield return null;

        t_Principal.Desselecionar();
        yield return null;

        var srPrincipal = t1GO.GetComponent<SpriteRenderer>();
        var srInimigo = t2GO.GetComponent<SpriteRenderer>();

        Assert.AreEqual(Color.white, srPrincipal.color);
        Assert.AreEqual(Color.white, srInimigo.color);
    }

    [UnityTest]
    public IEnumerator DesselecionarTodos_DeveChamarGameManager()
    {
        GameManager.instance.territorioSelecionado = t_Principal;
        GameManager.instance.territorioAlvo = t_VizinhoInimigo;

        TerritorioHandler.DesselecionarTodos();

        yield return null;

        Assert.IsNull(GameManager.instance.territorioSelecionado, "territorioSelecionado deveria ser null");
        Assert.IsNull(GameManager.instance.territorioAlvo, "territorioAlvo deveria ser null");
    }
    private void CriarEstruturaVisualTexto(GameObject territorioGO)
    {
        var exercitoVisual = new GameObject("ExercitoVisual");
        exercitoVisual.transform.SetParent(territorioGO.transform);

        var contadorCanvas = new GameObject("ContadorCanvas");
        contadorCanvas.transform.SetParent(exercitoVisual.transform);

        var contadorText = new GameObject("Contador_Text");
        contadorText.transform.SetParent(contadorCanvas.transform);
        contadorText.AddComponent<TMPro.TextMeshProUGUI>();
    }

    // TESTE 1: SpriteRenderer Nulo (Cobre o primeiro if de erro)
    [UnityTest]
    public IEnumerator AtualizarVisual_SemSpriteRenderer_DeveLogarErro()
    {
        var tVazioGO = new GameObject("TerritorioQuebrado");

        tVazioGO.AddComponent<PolygonCollider2D>();
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("SpriteRenderer não encontrado.*"));

        var tVazio = tVazioGO.AddComponent<TerritorioHandler>();

        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("SpriteRenderer não encontrado.*"));

        tVazio.AtualizarVisual();

        yield return null;
        Object.DestroyImmediate(tVazioGO);
    }

    // TESTE 2: Cor do Dono (Cobre o if dono != null)
    [UnityTest]
    public IEnumerator AtualizarVisual_ComDono_DeveMudarCor()
    {
        p1.cor = Color.magenta;

        t_Principal.AtualizarVisual();
        yield return null;

        var sr = t1GO.GetComponent<SpriteRenderer>();
        Assert.AreEqual(Color.magenta, sr.color);
    }

    // TESTE 3: Sem Dono (Cobre o else)
    [UnityTest]
    public IEnumerator AtualizarVisual_SemDono_DeveFicarCinza()
    {
        t_Principal.donoDoTerritorio = null;

        t_Principal.AtualizarVisual();
        yield return null;

        var sr = t1GO.GetComponent<SpriteRenderer>();
        Assert.AreEqual(Color.gray, sr.color);
    }

    // TESTE 4: Texto Já Vinculado (Caminho Curto)
    [UnityTest]
    public IEnumerator AtualizarVisual_TextoJaVinculado_DeveAtualizarNumero()
    {
        var txtGO = new GameObject("TextoManual");
        var tmp = txtGO.AddComponent<TMPro.TextMeshProUGUI>();
        t_Principal.contadorTropasTexto = tmp;

        t_Principal.numeroDeTropas = 42;


        t_Principal.AtualizarVisual();
        yield return null;


        Assert.AreEqual("42", tmp.text);
        Object.DestroyImmediate(txtGO);
    }

    // TESTE 5: Texto Não Vinculado (Caminho Longo - Busca na Hierarquia)
    [UnityTest]
    public IEnumerator AtualizarVisual_TextoNaoVinculado_DeveEncontrarFilhosEAtualizar()
    {
        t_Principal.contadorTropasTexto = null;
        t_Principal.numeroDeTropas = 99;

        CriarEstruturaVisualTexto(t1GO);

        t_Principal.AtualizarVisual();
        yield return null;

        Assert.IsNotNull(t_Principal.contadorTropasTexto, "Deveria ter encontrado o componente TextMeshPro.");
        Assert.AreEqual("99", t_Principal.contadorTropasTexto.text);
    }
}