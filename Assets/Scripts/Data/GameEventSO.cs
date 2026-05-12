using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Game Event", menuName = "Events/Game Event")]
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