using UnityEngine;

public interface IUIManager
{
    void AtualizarPainelStatus(GameManager.GamePhase fase, Player jogador);
    void AtualizarTextoObjetivo(string texto);
}


