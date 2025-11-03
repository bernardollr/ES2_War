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

    public Player jogador1;
    public Player jogador2;
    public Player jogadorAtual;

    public List<TerritorioHandler> todosOsTerritorios;

    public TerritorioHandler territorio;

    public TextMeshProUGUI turnoText; // arraste o Text do Canvas aqui pelo Inspector

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        jogador1 = new Player("Jogador 1", Color.blue);
        jogador2 = new Player("Jogador 2", Color.red);

        jogadorAtual = jogador1; // come�a sempre pelo jogador 1

        AtualizarTextoDoTurno();

        todosOsTerritorios = FindObjectsByType<TerritorioHandler>(FindObjectsSortMode.None).ToList();
        DistribuirTerritoriosIniciais();

<<<<<<< Updated upstream
        // Atualiza os territ�rios com o jogador do turno atual
=======
        // Inicia o primeiro turno
        IniciarNovoTurno(); 

        // Garantir que o botão de avançar fase fique em um Canvas overlay de alta prioridade
        TryMoveAdvanceButtonToOverlay();
        
        Debug.Log("GameManager iniciado.");
        PrintTerritoriosPorJogador();
    }

    // Cria (se necessário) um Canvas overlay com sortingOrder alto e move o botão de avançar fase para lá
    void TryMoveAdvanceButtonToOverlay()
    {
        if (botaoAvancarFase == null) return;

        // Se já tem um Canvas no botão com overrideSorting true e sortingOrder alto, assume OK
        Canvas existing = botaoAvancarFase.GetComponentInParent<Canvas>();
        if (existing != null && existing.overrideSorting && existing.sortingOrder >= 50) return;

        // Procura um overlay já existente na cena
        Canvas overlay = FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .FirstOrDefault(c => c.gameObject.name == "UI_Overlay_Canvas");

        if (overlay == null)
        {
            GameObject go = new GameObject("UI_Overlay_Canvas");
            overlay = go.AddComponent<Canvas>();
            overlay.renderMode = RenderMode.ScreenSpaceOverlay;
            overlay.overrideSorting = true;
            overlay.sortingOrder = 100; // alto o suficiente para ficar acima de painéis
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            // Make it a child of the main Canvas if available (keeps scaling consistent)
            Canvas main = FindObjectsByType<Canvas>(FindObjectsSortMode.None).FirstOrDefault(c => c.gameObject.name == "Canvas");
            if (main != null)
            {
                go.transform.SetParent(main.transform, false);
            }
        }

        // Move o botão para o overlay (mantendo sua posição visual)
        RectTransform btnRect = botaoAvancarFase.GetComponent<RectTransform>();
        if (btnRect != null)
        {
            Vector2 anchoredPos = btnRect.anchoredPosition;
            botaoAvancarFase.transform.SetParent(overlay.transform, false);
            RectTransform newRect = botaoAvancarFase.GetComponent<RectTransform>();
            newRect.anchoredPosition = anchoredPos;
        }
        else
        {
            botaoAvancarFase.transform.SetParent(overlay.transform, false);
        }
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
>>>>>>> Stashed changes
        AtualizarPlayerDoTurnoNosTerritorios();

        Debug.Log("GameManager iniciado. Turno de: " + jogadorAtual.nome);
        PrintTerritoriosPorJogador();
    }
<<<<<<< Updated upstream
=======

    // Função chamada pelo botão "Próxima Fase / Encerrar Turno"
    public void OnBotaoAvancarFaseClicado()
    {
        Debug.Log($"OnBotaoAvancarFaseClicado: Fase atual = {faseAtual}");
        
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
                Debug.Log("Fase alterada para: Remanejamento");
                break;

            case GamePhase.Remanejamento:
                MudarParaProximoJogador();
                return; // Sai da função, MudarParaProximoJogador chamará IniciarNovoTurno()
        }
        AtualizarTextoDoTurno();
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
        
        // Garante que o botão de avançar fase está visível
        if (botaoAvancarFase != null)
        {
            botaoAvancarFase.gameObject.SetActive(true);
        }
        
        ChecarVitoria(); // Verifica se o jogo acabou após a batalha
        AtualizarTextoDoTurno();
    }

    #endregion

    #region LÓGICA DE CLIQUES (O Cérebro)

    // Esta é a função central que o TerritorioHandler vai chamar
    public void OnTerritorioClicado(TerritorioHandler territorioClicado)
    {
        if (faseAtual == GamePhase.JogoPausado) 
        {
            Debug.Log("Jogo pausado, aguarde a batalha terminar.");
            return;
        }

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
                // Impede que o jogador faça ataques durante o remanejamento
                DesselecionarTerritorios();
                break;
            default:
                Debug.LogWarning($"Fase não tratada: {faseAtual}");
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
            AtualizarTextoDoTurno(); // Atualiza UI para mostrar reforços restantes
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
        // Garantir que estamos na fase correta
        if (faseAtual != GamePhase.Remanejamento)
        {
            Debug.LogWarning("Tentando remanejar fora da fase de remanejamento!");
            return;
        }

        if (territorioSelecionado == null)
        {
            // 1. Primeiro clique: Selecionar origem (tem que ser seu e ter tropas extras)
            if (territorioClicado.donoDoTerritorio == jogadorAtual && territorioClicado.numeroDeTropas > 1)
            {
                territorioSelecionado = territorioClicado;
                territorioSelecionado.Selecionar(false); // Seleciona, mas sem highlight vermelho
            }
            else
            {
                Debug.Log("Você só pode remanejar de territórios seus que tenham mais de uma tropa.");
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
                        OnBotaoAvancarFaseClicado(); 
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

    // Atualiza o texto para incluir a fase atual e os reforços
>>>>>>> Stashed changes
    public void AtualizarTextoDoTurno()
    {
        if (turnoText != null)
        {
            turnoText.text = "Turno do: " + jogadorAtual.nomeColorido;
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
            territorio.AtualizarVisual();

            Debug.Log($"[Distribui��o] Territ�rio '{territorio.name}' atribu�do a {dono.nome}");

            jogadorIndex++;
        }

        Debug.Log("Territ�rios iniciais distribu�dos!");
    }

    void PrintTerritoriosPorJogador()
    {
        Debug.Log($"=== Territ�rios do {jogador1.nome} ===");
        foreach (var t in todosOsTerritorios.Where(t => t.donoDoTerritorio == jogador1))
        {
            Debug.Log($"- {t.name} com {t.numeroDeTropas} tropa(s)");
        }

        Debug.Log($"=== Territ�rios do {jogador2.nome} ===");
        foreach (var t in todosOsTerritorios.Where(t => t.donoDoTerritorio == jogador2))
        {
            Debug.Log($"- {t.name} com {t.numeroDeTropas} tropa(s)");
        }
    }

    public void TrocarTurno()
    {
        // Troca o jogador atual
        jogadorAtual = (jogadorAtual == jogador1) ? jogador2 : jogador1;
        Debug.Log("Agora � o turno de: " + jogadorAtual.nome);

        // Deseleciona todos os territ�rios selecionados (se houver)
        TerritorioHandler.DesselecionarTodos();

        // Atualiza todos os territ�rios com o novo jogador do turno
        AtualizarPlayerDoTurnoNosTerritorios();

        AtualizarTextoDoTurno();
        ChecarVitoria();
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
                return; // ainda h� territ�rios de outros jogadores
        }

        // Todos os territ�rios s�o do mesmo jogador
        Debug.Log("Jogo acabou! Vencedor: " + donoReferencia.nome);

        // Salva o vencedor
        VencedorInfo.nomeVencedor = donoReferencia.nome;
        VencedorInfo.corVencedor = donoReferencia.cor;

        // Carrega a cena de fim
        SceneManager.LoadScene(2); // substitua pelo nome da sua cena de fim
    }

}
