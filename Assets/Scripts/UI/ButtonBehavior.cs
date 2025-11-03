using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    public void EncerrarTurno()
    {
        GameManager.instance.OnBotaoAvancarFaseClicado();
    }
}
