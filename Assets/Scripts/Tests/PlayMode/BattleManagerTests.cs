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
        UIManager.instance = uiManager; 

        p1 = new Player("P1", Color.blue, "Azul");
        p2 = new Player("P2", Color.red, "Vermelho");

        gameManager.todosOsJogadores = new List<Player> { p1, p2 };
        gameManager.jogadorAtual = p1;

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

        var tSobrevivenciaGO = new GameObject("BaseSeguraP2");
        tSobrevivenciaGO.AddComponent<SpriteRenderer>();
        tSobrevivenciaGO.AddComponent<PolygonCollider2D>();
        var t_Sobrevivencia = tSobrevivenciaGO.AddComponent<TerritorioHandler>();
        t_Sobrevivencia.donoDoTerritorio = p2;


        gameManager.todosOsTerritorios = new List<TerritorioHandler> { t_Atacante, t_Defensor, t_Sobrevivencia };

        gameManagerGO.SetActive(true);
    }

    [TearDown]
    public void Teardown()
    {
        if (battleManagerGO != null) Object.DestroyImmediate(battleManagerGO);
        if (gameManagerGO != null) Object.DestroyImmediate(gameManagerGO);
        if (t1GO != null) Object.DestroyImmediate(t1GO);
        if (t2GO != null) Object.DestroyImmediate(t2GO);


        var uiStub = GameObject.Find("UIStub");
        if (uiStub != null) Object.DestroyImmediate(uiStub);
    }

    [UnityTest]
    public IEnumerator Batalha_Normal_DeveSubtrairTropasEFecharPainel()
    {

        t_Atacante.numeroDeTropas = 10;
        t_Defensor.numeroDeTropas = 10;
        int tropasAtacantesAntes = 10;
        int tropasDefensorAntes = 10;


        battleManager.IniciarBatalha(t_Atacante, t_Defensor);


        Assert.IsTrue(battleManager.painelBatalha.activeSelf, "Painel deveria abrir.");


        yield return new WaitForSeconds(0.6f);


        bool houveMudanca = (t_Atacante.numeroDeTropas < tropasAtacantesAntes) || (t_Defensor.numeroDeTropas < tropasDefensorAntes);
        Assert.IsTrue(houveMudanca, "As tropas deveriam ter mudado.");

        Assert.IsFalse(battleManager.painelBatalha.activeSelf, "Painel deveria fechar.");
    }

    [UnityTest]
    public IEnumerator Batalha_Conquista_DeveMudarDonoEMoverTropas()
    {
        t_Atacante.numeroDeTropas = 50;
        t_Defensor.numeroDeTropas = 1;

        int tentativas = 0;

        while (t_Defensor.donoDoTerritorio == p2 && tentativas < 20)
        {
            battleManager.IniciarBatalha(t_Atacante, t_Defensor);
            yield return new WaitForSeconds(0.4f); 
            tentativas++;
        }


        Assert.AreEqual(p1, t_Defensor.donoDoTerritorio, "P1 deveria ter conquistado.");

        Assert.GreaterOrEqual(t_Defensor.numeroDeTropas, 1);
    }

    [UnityTest]
    public IEnumerator Batalha_SemUI_NaoDeveQuebrar()
    {
        battleManager.painelBatalha = null; 
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

    /*
     *FUNÇÃO: AtualizarImagensDados(List<int>, Image[])
     *Complexidade Ciclomática: 10
     */

    // Teste 1: Lista de imagens nula ou vazia (Cobre o primeiro if)
    [UnityTest]
    public IEnumerator AtualizarImagens_ListaNula_NaoDeveDarErro()
    {
        List<int> resultados = new List<int> { 1, 2, 3 };

        battleManager.AtualizarImagensDados(resultados, null);
        battleManager.AtualizarImagensDados(resultados, new Image[0]); // Lista vazia

        yield return null;

        Assert.Pass("A função tratou listas nulas/vazias corretamente.");
    }

    // Teste 2: Elemento Nulo (Cobre o 'continue')
    [UnityTest]
    public IEnumerator AtualizarImagens_SlotNulo_DeveIgnorar()
    {

        List<int> resultados = new List<int> { 6 };
        Image[] imagensComNull = new Image[] { null };


        battleManager.AtualizarImagensDados(resultados, imagensComNull);

        yield return null;

        Assert.Pass("Ignorou slot nulo com sucesso.");
    }

    // Teste 3: Desativar Sobras (Cobre o 'if (i >= resultados.Count)')
    [UnityTest]
    public IEnumerator AtualizarImagens_MenosDadosQueSlots_DeveDesativarExtras()
    {
        List<int> resultados = new List<int> { 5 };

        GameObject slot1GO = new GameObject("Slot1");
        GameObject slot2GO = new GameObject("Slot2");
        Image img1 = slot1GO.AddComponent<Image>();
        Image img2 = slot2GO.AddComponent<Image>();

        Image[] slots = new Image[] { img1, img2 };

        battleManager.AtualizarImagensDados(resultados, slots);
        yield return null;

        Assert.IsTrue(img1.gameObject.activeSelf, "Slot 1 (usado) deveria estar ativo.");
        Assert.IsFalse(img2.gameObject.activeSelf, "Slot 2 (extra) deveria estar inativo.");

        Object.DestroyImmediate(slot1GO);
        Object.DestroyImmediate(slot2GO);
    }

    // Teste 4: Caminho Feliz (Cobre atribuição de Sprite)
    [UnityTest]
    public IEnumerator AtualizarImagens_ResultadoValido_DeveTrocarSprite()
    {
        List<int> resultados = new List<int> { 1 };

        battleManager.facesDosDados = new Sprite[6];

        Sprite spriteTeste = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.zero);
        battleManager.facesDosDados[0] = spriteTeste; 


        GameObject slotGO = new GameObject("Slot");
        Image img = slotGO.AddComponent<Image>();
        Image[] slots = new Image[] { img };


        battleManager.AtualizarImagensDados(resultados, slots);
        yield return null;

        Assert.IsTrue(img.gameObject.activeSelf);
        Assert.AreEqual(spriteTeste, img.sprite, "O sprite da imagem deveria ter mudado para a face do dado 1.");

        Object.DestroyImmediate(slotGO);
    }

    // Teste 5: Sem Sprites Configurados (Cobre o último else)
    [UnityTest]
    public IEnumerator AtualizarImagens_SemSpritesNoManager_DeveApenasAtivar()
    {
        List<int> resultados = new List<int> { 1 };

        battleManager.facesDosDados = null;

        GameObject slotGO = new GameObject("Slot");
        slotGO.SetActive(false); 
        Image img = slotGO.AddComponent<Image>();
        Image[] slots = new Image[] { img };

        battleManager.AtualizarImagensDados(resultados, slots);
        yield return null;

        Assert.IsTrue(img.gameObject.activeSelf, "Deveria ter ativado o objeto mesmo sem sprites configurados.");
        Assert.IsNull(img.sprite, "O sprite não deveria ter mudado (deve ser null ou padrão).");

        Object.DestroyImmediate(slotGO);
    }
}