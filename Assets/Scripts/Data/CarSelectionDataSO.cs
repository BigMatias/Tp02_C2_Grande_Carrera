using UnityEngine;

[CreateAssetMenu(fileName = "CarSelectionData", menuName = "Game/CarSelectionData")]
public class CarSelectionDataSO : ScriptableObject
{
    public int selectedCarIndex;
    public int selectedTrackIndex; // solo endless
}