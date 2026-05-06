using System;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    public static event Action onButtonClicked;


    public void ButtonClicked()
    {
        onButtonClicked?.Invoke();
    }
}
