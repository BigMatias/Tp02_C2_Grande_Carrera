using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Game Event", menuName = "Events/Game Event")]
// Suggestion: Alta - Buena implementación del patrón SO-Event-Channel. Bien hecho.
public class GameEventSO : ScriptableObject
{
    private event Action onEventRaised;

    public void Raise()
    {
        onEventRaised?.Invoke();
    }

    public void Subscribe(Action listener)
    {
        onEventRaised += listener;
    }

    public void Unsubscribe(Action listener)
    {
        onEventRaised -= listener;
    }
}