using UnityEngine;

[CreateAssetMenu(fileName = "CarData", menuName = "Game/CarData")]
public class CarDataSO : ScriptableObject
{
    public string carName;
    public GameObject prefab;
    public Sprite previewImage;
}