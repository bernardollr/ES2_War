using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using ZeusUnite.Dice;
using System.Runtime.CompilerServices;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour {
    [Header("Configuração Visual dos Dados")]
    public Sprite[] facesDosDados;

    [Header("UI Geral")]
    public GameObject painelBatalha;

    [Header("Locais dos Dados na UI")]
    public Image[] imagensDadosAtaque;
    public Image[] imagensDadosDefesa;

    [Header("Resultados")]
    public TextMeshProUGUI textoResultadoBatalha;

    private bool estaRolando = false;

    [Header("Animação")]
    public float duracaoAnimacao = 1.0f;


    public void IniciarBatalha()
    {
        if (estaRolando) return;

        StartCoroutine(ProcessarBatalha());
        

        
    }

    private IEnumerator ProcessarBatalha() {
        estaRolando = true;
        painelBatalha.SetActive(true);
        textoResultadoBatalha.text = "Rolando dados...";

        List<int> resultadosFinaisAtaque = RolarVariosDados(3);
        List<int> resultadosFinaisDefesa = RolarVariosDados(2);

        float tempoInicio = Time.time;
        while (Time.time < tempoInicio + duracaoAnimacao) {
            AtualizarImagensDados(RolarVariosDados(3, false), imagensDadosAtaque);
            AtualizarImagensDados(RolarVariosDados(2, false), imagensDadosDefesa);

            yield return null;
        }

        AtualizarImagensDados(resultadosFinaisAtaque, imagensDadosAtaque);
        AtualizarImagensDados(resultadosFinaisDefesa, imagensDadosDefesa);

        int perdasAtaque = 0;
        int perdasDefesa = 0;
        int comparacoes = Mathf.Min(resultadosFinaisAtaque.Count, resultadosFinaisDefesa.Count);

        for (int i = 0; i < comparacoes; i++) {
            if (resultadosFinaisAtaque[i] > resultadosFinaisDefesa[i])
                perdasDefesa++;
            else
                perdasAtaque++;
        }

        textoResultadoBatalha.text = $"Ataque perde: {perdasAtaque}\nDefesa perde: {perdasDefesa}";

        estaRolando = false;
    }

    void AtualizarImagensDados(List<int> resultados, Image[] imagensUI) {
        for (int i = 0; i < imagensUI.Length; i++) {
            if (i >= resultados.Count)
            {
                imagensUI[i].gameObject.SetActive(false);
            }
            else {
                int numeroDoDado = resultados[i];

                Sprite spriteDoDado = facesDosDados[numeroDoDado - 1];

                imagensUI[i].gameObject.SetActive(true);
                imagensUI[i].sprite = spriteDoDado;
            }
        }
    }
     private List<int> RolarVariosDados(int quantidade, bool ordenar = true) {
        if (quantidade > 3)
            quantidade = 3;
       List<int> resultados = new List<int>();
        for (int i = 0; i < quantidade; i++) {
            DiceRoller dr = new DiceRoller(1, 6);
            resultados.Add(dr.rolledValue);
        }
        if (ordenar) {
            return resultados.OrderByDescending(d => d).ToList();
        }
        return resultados;
     }
 }
