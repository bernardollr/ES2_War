// SigaObjetoUI.cs

using UnityEngine;
using TMPro; // Necessário para controlar o TextMeshPro

public class SigaObjetoUI : MonoBehaviour
{
    // Este é um "espaço" público que o backend vai preencher.
    // Ele vai colocar aqui o objeto de texto que deve nos seguir.
    public TextMeshProUGUI textoParaSeguir;

    // Uma margem para o texto ficar um pouco acima do exército.
    public Vector3 margem = new Vector3(0, 30, 0);

    // O método Update é chamado a cada frame do jogo.
    void Update()
    {
        // Se já temos um texto para seguir...
        if (textoParaSeguir != null)
        {
            // 1. Pega a nossa posição no mundo do jogo.
            Vector3 nossaPosicaoNoMundo = transform.position;

            // 2. Converte essa posição do mundo para uma coordenada na tela.
            Vector2 posicaoNaTela = Camera.main.WorldToScreenPoint(nossaPosicaoNoMundo);

            // 3. Move o objeto de texto para essa coordenada na tela, adicionando a margem.
            textoParaSeguir.transform.position = new Vector3(posicaoNaTela.x, posicaoNaTela.y, 0) + margem;
        }
    }
}