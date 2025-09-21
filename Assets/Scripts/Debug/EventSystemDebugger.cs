using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem; // <-- ADICIONAR ESTA LINHA

// Este script serve para debugar o que o EventSystem da Unity está detectando sob o mouse.
public class EventSystemDebugger : MonoBehaviour
{
    void Update()
    {
        // Verifica se o EventSystem existe na cena
        if (EventSystem.current == null)
        {
            Debug.LogError("CENA SEM EVENTSYSTEM. Adicione um via GameObject > UI > Event System.");
            return;
        }

        // Verifica se há um mouse presente (útil para builds mobile, etc.)
        if (Mouse.current == null)
        {
            return;
        }

        // Cria os dados do ponteiro (mouse) na posição atual
        PointerEventData pointerData = new PointerEventData(EventSystem.current);

        // ##### LINHA CORRIGIDA AQUI #####
        // Usando o novo Input System para pegar a posição do mouse
        pointerData.position = Mouse.current.position.ReadValue();

        // Cria uma lista para armazenar os resultados do "disparo" do raio
        List<RaycastResult> results = new List<RaycastResult>();

        // Pede ao EventSystem para disparar um raio e preencher a lista de resultados
        EventSystem.current.RaycastAll(pointerData, results);

        // Agora, vamos analisar os resultados
        if (results.Count > 0)
        {
            // O raio atingiu pelo menos um objeto.
            //Debug.Log($"<color=green>EventSystem detectou o mouse sobre:</color> {results[0].gameObject.name}");
        }
        else
        {
            // O raio não atingiu nenhum objeto que o EventSystem consiga detectar.
            //Debug.Log("<color=red>EventSystem não detectou nenhum objeto sob o mouse.</color>");
        }
    }
}