using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NSubstitute;

public class GameManagerInicializacaoTests
{
    private GameObject gameManagerGO;
    private GameManager gameManager;

    [TearDown]
    public void Teardown()
    {
        if (gameManagerGO != null) Object.DestroyImmediate(gameManagerGO);

        if (GameManager.instance != null && GameManager.instance.gameObject != null)
            Object.DestroyImmediate(GameManager.instance.gameObject);

        if (BattleManager.instance != null && BattleManager.instance.gameObject != null)
            Object.DestroyImmediate(BattleManager.instance.gameObject);

        if (UIManager.instance != null && UIManager.instance.gameObject != null)
            Object.DestroyImmediate(UIManager.instance.gameObject);
    }

    [UnityTest]
    public IEnumerator Start_SemConfiguracaoPrevia_DeveCriar6JogadoresEInicializarJogo()
    {
        var uiGO = new GameObject("UIManagerStub");
        var uiManager = uiGO.AddComponent<UIManager>();
        UIManager.instance = uiManager;

        uiManager.textoNomeJogador = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.textoStatus = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.textoBotaoAvancarFase = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.iconeStatusAttack = new GameObject().AddComponent<UnityEngine.UI.Image>();
        uiManager.iconeStatusDeploy = new GameObject().AddComponent<UnityEngine.UI.Image>();
        uiManager.iconeStatusMove = new GameObject().AddComponent<UnityEngine.UI.Image>();
        uiManager.iconeSoldier = new GameObject().AddComponent<UnityEngine.UI.Image>();

        var battleGO = new GameObject("BattleManager");
        var bm = battleGO.AddComponent<BattleManager>();
        bm.painelBatalha = new GameObject();
        bm.painelBatalha.SetActive(false);
        bm.textoResultadoBatalha = new GameObject().AddComponent<TMPro.TextMeshProUGUI>();
        bm.imagensDadosAtaque = new UnityEngine.UI.Image[0];
        bm.imagensDadosDefesa = new UnityEngine.UI.Image[0];
        BattleManager.instance = bm;

        var t1 = new GameObject("T1").AddComponent<TerritorioHandler>();
        t1.gameObject.AddComponent<SpriteRenderer>();
        t1.gameObject.AddComponent<PolygonCollider2D>();

        var t2 = new GameObject("T2").AddComponent<TerritorioHandler>();
        t2.gameObject.AddComponent<SpriteRenderer>();
        t2.gameObject.AddComponent<PolygonCollider2D>();

        gameManagerGO = new GameObject("GameManager");

        gameManager = gameManagerGO.AddComponent<GameManager>();

        gameManager.uiManager = uiManager;
        gameManager.battleManager = bm;

        yield return null;

        Assert.AreEqual(6, gameManager.todosOsJogadores.Count, "Deveria ter criado 6 jogadores padrão.");

        Assert.AreEqual("Jogador 1", gameManager.todosOsJogadores[0].nome);
        Assert.IsFalse(gameManager.todosOsJogadores[0].ehIA);

        Assert.IsTrue(gameManager.todosOsJogadores[1].ehIA);

        Assert.IsNotNull(t1.donoDoTerritorio, "Território 1 deveria ter dono.");
        Assert.IsNotNull(t2.donoDoTerritorio, "Territorio 2 deveria ter dono.");

        Assert.IsNotNull(gameManager.todosOsJogadores[0].objetivoSecreto);


    }
}
