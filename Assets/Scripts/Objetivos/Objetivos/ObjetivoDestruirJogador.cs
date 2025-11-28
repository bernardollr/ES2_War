using System.Linq;
using UnityEngine; // Necessário para Debug.Log

public class ObjetivoDestruirJogador : Objetivo
{
    // Vamos guardar o jogador alvo para referência
    public Player JogadorAlvo { get; private set; }

    // O construtor usa o "nomeDaCor" do alvo para montar a descrição
    public ObjetivoDestruirJogador(Player alvo)
        : base($"Destruir o exército {alvo.nomeDaCor}") 
    {
        this.JogadorAlvo = alvo;
    }

    public override bool FoiConcluido(Player jogador, GameManager gameManager)
    {
        // 1. Verifica quantos territórios o alvo ainda tem
        int territoriosDoAlvo = gameManager.todosOsTerritorios.Count(t => t.donoDoTerritorio == this.JogadorAlvo);

        // Se o alvo ainda tem territórios, ele está vivo. Objetivo não concluído.
        if (territoriosDoAlvo > 0)
        {
            return false;
        }

        // --- ALVO ESTÁ DESTRUÍDO (0 Territórios) ---
        // Agora precisamos saber: Quem matou?

        // Assumimos que a morte acontece no turno de quem atacou.
        // Se o jogador atual (quem está jogando agora) for o dono deste objetivo,
        // então foi ele quem deu o golpe final.
        if (gameManager.jogadorAtual == jogador)
        {
            return true; // VITÓRIA! Eu matei o alvo.
        }
        else
        {
            // O alvo morreu, mas NÃO é o meu turno.
            // Significa que OUTRA PESSOA (ou um terceiro) matou o alvo.
            
            Debug.Log($"O alvo de {jogador.nome} ({this.JogadorAlvo.nome}) foi destruído por outro jogador! Mudando objetivo para 24 territórios.");
            
            // --- REGRA DO WAR ---
            // O objetivo é substituído imediatamente.
            // Criamos um novo objetivo de 24 territórios e atribuímos ao jogador.
            jogador.objetivoSecreto = new ObjetivoConquistarNTerritorios(24, "Conquistar 24 territórios (Alvo destruído por terceiro)");
            
            // Retorna false, pois ele ainda não cumpriu o NOVO objetivo (apenas perdeu o antigo).
            return false; 
        }
    }
}