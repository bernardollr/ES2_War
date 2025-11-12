// ObjetivoConquistarNTerritorios.cs
using System.Linq;

public class ObjetivoConquistarNTerritorios : Objetivo
{
    private int numeroDeTerritorios;

    public ObjetivoConquistarNTerritorios(int numero, string descricao) : base(descricao)
    {
        this.numeroDeTerritorios = numero;
    }

    public override bool FoiConcluido(Player jogador, GameManager gameManager)
    {
        // O objetivo é concluído se o jogador que o possui tiver N ou mais territórios.
        int territoriosDoJogador = gameManager.todosOsTerritorios.Count(t => t.donoDoTerritorio == jogador);

        return territoriosDoJogador >= this.numeroDeTerritorios;
    }
}