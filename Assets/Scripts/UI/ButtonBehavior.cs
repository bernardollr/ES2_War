using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    public void EncerrarTurno()
    {
<<<<<<< Updated upstream
        GameManager.instance.TrocarTurno();
=======
        Debug.Log("ButtonBehavior: EncerrarTurno chamado");
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager.instance é nulo!");
            return;
        }
        GameManager.instance.OnBotaoAvancarFaseClicado();
>>>>>>> Stashed changes
    }
}
