using System;
using UnityEngine;

// Warning: Alta - Evento `static` sin método de Reset. Si la escena se recarga, los listeners de la escena anterior pueden quedar suscritos
public class UIButton : MonoBehaviour
{
    public static event Action onButtonClicked;


    public void ButtonClicked()
    {
        onButtonClicked?.Invoke();
    }
}
