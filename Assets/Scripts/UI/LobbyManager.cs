using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro; // Necessário para o Dropdown

public class LobbyManager : MonoBehaviour
{
    // --- NOVO: ESTRUTURA PARA MAPAS ---
    [System.Serializable]
    public struct OpcaoDeMapa
    {
        public string nomeVisivel; // Ex: "Niterói (2-4 Jogadores)"
        public string nomeDaCena;  // Ex: "Scene_Niteroi"
    }
    // ----------------------------------

    [Header("Configuração de Mapas")]
    public TMP_Dropdown dropdownMapas;    // Arraste o componente Dropdown da UI aqui
    public List<OpcaoDeMapa> listaDeMapas; // Preencha os 3 mapas aqui no Inspector

    [Header("Lista de Slots")]
    public List<PlayerSetupSlot> slotsDeJogadores;

    [Header("Botões")]
    public Button botaoIniciar;

    void Start()
    {
        // 1. Configura os Slots de jogadores (P1, P2...)
        for (int i = 0; i < slotsDeJogadores.Count; i++)
        {
            slotsDeJogadores[i].ConfigurarSlot(i + 1);
        }

        // 2. Preenche o Dropdown de Mapas automaticamente
        ConfigurarDropdownDeMapas();

        botaoIniciar.onClick.AddListener(TentarIniciarJogo);
    }

    // Pega a lista do Inspector e joga dentro do componente visual Dropdown
    void ConfigurarDropdownDeMapas()
    {
        dropdownMapas.ClearOptions();
        List<string> opcoesTexto = new List<string>();

        foreach (var mapa in listaDeMapas)
        {
            opcoesTexto.Add(mapa.nomeVisivel);
        }

        dropdownMapas.AddOptions(opcoesTexto);
    }

    void TentarIniciarJogo()
    {
        // A. Limpa dados antigos
        GameLaunchData.LimparConfiguracoes();

        List<string> coresUsadas = new List<string>();
        int contagemJogadores = 0;

        // B. Coleta dados de cada slot
        foreach (var slot in slotsDeJogadores)
        {
            var configOrNull = slot.PegarConfiguracao();

            if (configOrNull != null)
            {
                var config = configOrNull.Value;

                if (coresUsadas.Contains(config.nomeDaCor))
                {
                    Debug.LogError($"Erro: A cor {config.nomeDaCor} já foi escolhida!");
                    // Dica: Adicione um feedback visual para o usuário aqui
                    return;
                }

                coresUsadas.Add(config.nomeDaCor);
                GameLaunchData.configuracaoJogadores.Add(config);
                contagemJogadores++;
            }
        }

        // C. Validação de Mínimo de Jogadores
        if (contagemJogadores < 3)
        {
            Debug.LogError("Erro: O jogo precisa de no mínimo 3 jogadores ativos.");
            return;
        }

        // --- NOVO: CARREGAMENTO DO MAPA SELECIONADO ---

        // Descobre qual índice está selecionado no Dropdown (0, 1 ou 2...)
        int indexSelecionado = dropdownMapas.value;

        // Pega o nome da cena correspondente na sua lista
        string cenaParaCarregar = listaDeMapas[indexSelecionado].nomeDaCena;

        Debug.Log($"Iniciando jogo no mapa: {listaDeMapas[indexSelecionado].nomeVisivel} ({cenaParaCarregar})");

        // Carrega a cena específica
        SceneManager.LoadScene(cenaParaCarregar);
    }
}