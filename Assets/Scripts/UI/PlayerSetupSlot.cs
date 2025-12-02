using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSetupSlot : MonoBehaviour
{
    [Header("UI References")]
    public Toggle toggleAtivo;       // Checkbox "Jogar?"
    public TextMeshProUGUI labelID;  // Texto "P1", "P2"
    public TMP_InputField inputNome; // Nome do jogador
    public TMP_Dropdown dropdownCor; // Cor escolhida
    public Toggle toggleEhIA;        // Checkbox "É Bot?"

    // Configura o visual inicial (chamado pelo Manager)
    public void ConfigurarSlot(int idJogador)
    {
        labelID.text = $"Jogador {idJogador}";
        inputNome.text = $"Jogador {idJogador}";

        // Padrão: P1 e P2 ativos, o resto inativo
        toggleAtivo.isOn = (idJogador <= 2);

        // Padrão: Cores diferentes para cada um (baseado no ID)
        dropdownCor.value = (idJogador - 1) % dropdownCor.options.Count;
    }

    // Retorna os dados formatados para o GameLaunchData
    // Se o slot estiver inativo, retorna null
    public GameLaunchData.PlayerConfig? PegarConfiguracao()
    {
        if (!toggleAtivo.isOn) return null;

        GameLaunchData.PlayerConfig config = new GameLaunchData.PlayerConfig();

        config.nome = inputNome.text;
        config.ehIA = toggleEhIA.isOn;
        config.nomeDaCor = dropdownCor.options[dropdownCor.value].text;
        config.cor = ConverterNomeParaCor(config.nomeDaCor);

        return config;
    }

    // Auxiliar simples para converter a string do Dropdown em Color
    private Color ConverterNomeParaCor(string nomeCor)
    {
        switch (nomeCor)
        {
            case "Azul": return Color.blue;
            case "Vermelho": return Color.red;
            case "Preto": return Color.black;
            case "Branco": return Color.white;
            case "Verde": return Color.green;
            case "Amarelo": return Color.yellow;
            default: return Color.gray;
        }
    }
}