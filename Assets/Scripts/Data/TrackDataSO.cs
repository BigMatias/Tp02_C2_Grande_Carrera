using UnityEngine;

[CreateAssetMenu(fileName = "TrackData", menuName = "Game/TrackData")]
public class TrackDataSO : ScriptableObject
{
    public string trackName;
    public string sceneName;
    public Sprite previewImage;
}