// ObjetivoDestruirJogador.cs
using System.Linq;

public class ObjetivoDestruirJogador : Objetivo
{
    // Vamos guardar o jogador alvo para referência
    public Player JogadorAlvo { get; private set; }

    // O construtor usa o "nomeDaCor" do alvo para montar a descrição
    public ObjetivoDestruirJogador(Player alvo)
        : base($"Destruir o exército {alvo.nomeDaCor}") // <-- REQUISIÇÃO 1
    {
        this.JogadorAlvo = alvo;
    }

    public override bool FoiConcluido(Player jogador, GameManager gameManager)
    {
        int territoriosDoAlvo = gameManager.todosOsTerritorios.Count(t => t.donoDoTerritorio == this.JogadorAlvo);
        return territoriosDoAlvo == 0;
    }
}