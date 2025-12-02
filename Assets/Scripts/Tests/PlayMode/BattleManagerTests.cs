using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

public class BattleManagerTests
{
    private GameObject battleManagerGO;
    private BattleManager battleManager;

    private GameObject gameManagerGO;
    private GameManager gameManager;

    private GameObject t1GO, t2GO;
    private TerritorioHandler t_Atacante, t_Defensor;

    private Player p1, p2;

    [SetUp]
    public void Setup()
    {
        if (BattleManager.instance != null && BattleManager.instance.gameObject != null)
            Object.DestroyImmediate(BattleManager.instance.gameObject);

        if (GameManager.instance != null && GameManager.instance.gameObject != null)
            Object.DestroyImmediate(GameManager.instance.gameObject);

        battleManagerGO = new GameObject("BattleManager");
        battleManager = battleManagerGO.AddComponent<BattleManager>();

        battleManager.painelBatalha = new GameObject("PainelUI");
        battleManager.painelBatalha.SetActive(false);

        battleManager.textoResultadoBatalha = new GameObject("TxtResult").AddComponent<TextMeshProUGUI>();

        battleManager.imagensDadosAtaque = new Image[0];
        battleManager.imagensDadosDefesa = new Image[0];

        battleManager.duracaoAnimacao = 0.1f;
        battleManager.delayResultado = 0.1f;

        BattleManager.instance = battleManager;

        gameManagerGO = new GameObject("GameManagerStub");
        gameManagerGO.SetActive(false);
        gameManager = gameManagerGO.AddComponent<GameManager>();

        var uiGO = new GameObject("UIStub");
        var uiManager = uiGO.AddComponent<UIManager>();

        uiManager.textoNomeJogador = new GameObject("TxtNome").AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.textoStatus = new GameObject("TxtStatus").AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.textoBotaoAvancarFase = new GameObject("TxtBtn").AddComponent<TMPro.TextMeshProUGUI>();


        gameManagerGO.SetActive(true);
        GameManager.instance = gameManager;

        p1 = new Player("P1", Color.blue, "Azul");
        p2 = new Player("P2", Color.red, "Vermelho");

        t1GO = new GameObject("Atacante");
        t1GO.AddComponent<SpriteRenderer>();
        t1GO.AddComponent<PolygonCollider2D>();
        t_Atacante = t1GO.AddComponent<TerritorioHandler>();
        t_Atacante.donoDoTerritorio = p1;

        t2GO = new GameObject("Defensor");
        t2GO.AddComponent<SpriteRenderer>();
        t2GO.AddComponent<PolygonCollider2D>();
        t_Defensor = t2GO.AddComponent<TerritorioHandler>();
        t_Defensor.donoDoTerritorio = p2;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(battleManagerGO);
        Object.DestroyImmediate(gameManagerGO);
        Object.DestroyImmediate(t1GO);
        Object.DestroyImmediate(t2GO);
    }

    /*
     * FUNÇÃO: ProcessarBatalha(TerritorioHander, TerritorioHandler)
     * Complexidade ciclomátca: 20
     * Acoplamento de classes: 11
     */

    // -- TESTE 1: Cobre o while e os ifs de dados e a subtração de tropas)
    [UnityTest]
    public IEnumerator Batalha_Normal_DeveSubtrairTropasEFecharPainel()
    {
        t_Atacante.numeroDeTropas = 10;
        t_Defensor.numeroDeTropas = 10;

        int tropasAtacantesAntes = 10;
        int tropasDefensorAntes = 10;

        battleManager.IniciarBatalha(t_Atacante, t_Defensor);

        Assert.IsTrue(battleManager.painelBatalha.activeSelf, "Painel deveria abrir no início.");

        yield return new WaitForSeconds(0.5f);

        bool houveMudanca = (t_Atacante.numeroDeTropas < tropasAtacantesAntes) || (t_Defensor.numeroDeTropas < tropasDefensorAntes);
        Assert.IsTrue(houveMudanca, "As tropas deveriam ter mudado após a batalha.");

        Assert.IsFalse(battleManager.painelBatalha.activeSelf, "Painel deveria fechar no final");
    }

    // -- TESTE 2: Cobre o if(defensor < 1) e a movimentação)
    [UnityTest]
    public IEnumerator Batalha_Conquista_DeveMudarDonoEMoverTropas()
    {
        t_Atacante.numeroDeTropas = 50;
        t_Defensor.numeroDeTropas = 1;

        int tentativas = 0;
        while(t_Defensor.donoDoTerritorio == p2 && tentativas < 20)
        {
            battleManager.IniciarBatalha(t_Atacante, t_Defensor);
            yield return new WaitForSeconds(0.3f);
            tentativas++;
        }

        Assert.AreEqual(p1, t_Defensor.donoDoTerritorio, "P1 deveria ter conquistado o território.");

        Assert.GreaterOrEqual(t_Defensor.numeroDeTropas, 1);
        Assert.LessOrEqual(t_Defensor.numeroDeTropas, 3);
     }

    // -- TETE 3: Cobre o ifs de null da UI
    [UnityTest]
    public IEnumerator Batalha_SemUI_NaoDeveQuebrar()
    {
        battleManager.painelBatalha = null;
        battleManager.textoResultadoBatalha = null;

        t_Atacante.numeroDeTropas = 5;
        t_Defensor.numeroDeTropas = 2;

        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*painelBatalha.*"));
        UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*textoResultadoBatalha*"));

        battleManager.IniciarBatalha(t_Atacante, t_Defensor);

        yield return new WaitForSeconds(0.5f);

        bool houveMudanca = (t_Atacante.numeroDeTropas < 5) || (t_Defensor.numeroDeTropas < 2);
        Assert.IsTrue(houveMudanca, "A batalha deveria acontecer mesmo sem UI.");
    }
}
