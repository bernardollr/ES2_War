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

    private GameObject t1GO, t2GO, tSobrevivenciaGO; // Adicionei tSobrevivenciaGO
    private TerritorioHandler t_Atacante, t_Defensor;

    private Player p1, p2;

    [SetUp]
    public void Setup()
    {
        // 1. LIMPEZA DE SINGLETONS (CRUCIAL)
        if (BattleManager.instance != null)
        {
            if (BattleManager.instance.gameObject != null)
                Object.DestroyImmediate(BattleManager.instance.gameObject);
            BattleManager.instance = null;
        }

        if (GameManager.instance != null)
        {
            if (GameManager.instance.gameObject != null)
                Object.DestroyImmediate(GameManager.instance.gameObject);
            GameManager.instance = null;
        }

        // 2. BATTLE MANAGER
        battleManagerGO = new GameObject("BattleManager");
        battleManager = battleManagerGO.AddComponent<BattleManager>();

        battleManager.painelBatalha = new GameObject("PainelUI");
        battleManager.painelBatalha.SetActive(false);
        battleManager.textoResultadoBatalha = new GameObject("TxtResult").AddComponent<TextMeshProUGUI>();
        battleManager.imagensDadosAtaque = new Image[0];
        battleManager.imagensDadosDefesa = new Image[0];

        // Tempos curtos para teste rápido
        battleManager.duracaoAnimacao = 0.05f;
        battleManager.delayResultado = 0.05f;

        BattleManager.instance = battleManager;

        // 3. GAME MANAGER (COM SEGURANÇA DE SETACTIVE)
        gameManagerGO = new GameObject("GameManagerStub");
        gameManagerGO.SetActive(false); // <--- O SEGREDO ESTÁ AQUI
        gameManager = gameManagerGO.AddComponent<GameManager>();

        // Configura UI Fake dentro do GM
        var uiGO = new GameObject("UIStub");
        var uiManager = uiGO.AddComponent<UIManager>();
        uiManager.textoNomeJogador = new GameObject("TxtNome").AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.textoStatus = new GameObject("TxtStatus").AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.textoBotaoAvancarFase = new GameObject("TxtBtn").AddComponent<TMPro.TextMeshProUGUI>();
        uiManager.iconeStatusAttack = new GameObject("ImgAtk").AddComponent<Image>();
        uiManager.iconeStatusDeploy = new GameObject("ImgDep").AddComponent<Image>();
        uiManager.iconeStatusMove = new GameObject("ImgMov").AddComponent<Image>();
        uiManager.iconeSoldier = new GameObject("ImgSol").AddComponent<Image>();

        gameManager.uiManager = uiManager;
        UIManager.instance = uiManager; // Singleton da UI também

        // 4. JOGADORES (INJETADOS ANTES DE LIGAR O GM)
        p1 = new Player("P1", Color.blue, "Azul");
        p2 = new Player("P2", Color.red, "Vermelho");

        // Injeta a lista para o Start() não criar novos
        gameManager.todosOsJogadores = new List<Player> { p1, p2 };
        gameManager.jogadorAtual = p1;

        // 5. TERRITÓRIOS
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

        // --- TERRITÓRIO DE SOBREVIVÊNCIA (PARA O P2 NÃO PERDER O JOGO NO TESTE) ---
        // Isso evita que o SceneManager carregue a cena de vitória e destrua o teste
        var tSobrevivenciaGO = new GameObject("BaseSeguraP2");
        tSobrevivenciaGO.AddComponent<SpriteRenderer>();
        tSobrevivenciaGO.AddComponent<PolygonCollider2D>();
        var t_Sobrevivencia = tSobrevivenciaGO.AddComponent<TerritorioHandler>();
        t_Sobrevivencia.donoDoTerritorio = p2;
        // --------------------------------------------------------------------------

        // Lista de todos os territórios
        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_Atacante, t_Defensor, t_Sobrevivencia };

        // LIGA O GM AGORA (O Start roda, vê os jogadores e territórios preenchidos e não faz nada)
        gameManagerGO.SetActive(true);
    }

    [TearDown]
    public void Teardown()
    {
        if (battleManagerGO != null) Object.DestroyImmediate(battleManagerGO);
        if (gameManagerGO != null) Object.DestroyImmediate(gameManagerGO);
        if (t1GO != null) Object.DestroyImmediate(t1GO);
        if (t2GO != null) Object.DestroyImmediate(t2GO);

        // Limpar UI
        var uiStub = GameObject.Find("UIStub");
        if (uiStub != null) Object.DestroyImmediate(uiStub);
    }

    // --- TESTES ---

    [UnityTest]
    public IEnumerator Batalha_Normal_DeveSubtrairTropasEFecharPainel()
    {
        // ARRANGE
        t_Atacante.numeroDeTropas = 10;
        t_Defensor.numeroDeTropas = 10;
        int tropasAtacantesAntes = 10;
        int tropasDefensorAntes = 10;

        // ACT
        battleManager.IniciarBatalha(t_Atacante, t_Defensor);

        // Check inicial
        Assert.IsTrue(battleManager.painelBatalha.activeSelf, "Painel deveria abrir.");

        // Espera tempo suficiente (animação + delay)
        yield return new WaitForSeconds(0.6f);

        // ASSERT
        bool houveMudanca = (t_Atacante.numeroDeTropas < tropasAtacantesAntes) || (t_Defensor.numeroDeTropas < tropasDefensorAntes);
        Assert.IsTrue(houveMudanca, "As tropas deveriam ter mudado.");

        Assert.IsFalse(battleManager.painelBatalha.activeSelf, "Painel deveria fechar.");
    }

    [UnityTest]
    public IEnumerator Batalha_Conquista_DeveMudarDonoEMoverTropas()
    {
        t_Atacante.numeroDeTropas = 50; // Exército grande
        t_Defensor.numeroDeTropas = 1;

        int tentativas = 0;
        // Tenta atacar até conquistar
        while (t_Defensor.donoDoTerritorio == p2 && tentativas < 20)
        {
            battleManager.IniciarBatalha(t_Atacante, t_Defensor);
            yield return new WaitForSeconds(0.4f); // Tempo da batalha
            tentativas++;
        }

        // Se o P2 ainda for dono, o teste falhou (azar estatístico ou erro lógico)
        Assert.AreEqual(p1, t_Defensor.donoDoTerritorio, "P1 deveria ter conquistado.");

        // Verifica movimentação (pelo menos 1 tropa foi)
        Assert.GreaterOrEqual(t_Defensor.numeroDeTropas, 1);
    }

    [UnityTest]
    public IEnumerator Batalha_SemUI_NaoDeveQuebrar()
    {
        battleManager.painelBatalha = null; // Simula erro de config
        battleManager.textoResultadoBatalha = null;

        t_Atacante.numeroDeTropas = 5;
        t_Defensor.numeroDeTropas = 2;

        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*painelBatalha.*"));
        UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*textoResultadoBatalha.*"));

        battleManager.IniciarBatalha(t_Atacante, t_Defensor);

        yield return new WaitForSeconds(0.4f);

        bool houveMudanca = (t_Atacante.numeroDeTropas < 5) || (t_Defensor.numeroDeTropas < 2);
        Assert.IsTrue(houveMudanca, "A batalha deveria acontecer mesmo sem UI.");
    }
}