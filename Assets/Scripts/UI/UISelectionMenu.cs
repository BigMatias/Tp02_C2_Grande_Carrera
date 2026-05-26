using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UISelectionMenu : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private GameModeDataSO gameModeData;
    [SerializeField] private CarSelectionDataSO carSelectionData;
    [SerializeField] private CarDataSO[] cars;
    [SerializeField] private TrackDataSO[] tracks;

    [Header("Car Selection")]
    [SerializeField] private Image carPreviewImage;
    [SerializeField] private TextMeshProUGUI carNameText;
    [SerializeField] private Button prevCarButton;
    [SerializeField] private Button nextCarButton;

    [Header("Track Selection")]
    [SerializeField] private GameObject trackSelectionPanel;
    [SerializeField] private Image trackPreviewImage;
    [SerializeField] private TextMeshProUGUI trackNameText;
    [SerializeField] private Button prevTrackButton;
    [SerializeField] private Button nextTrackButton;

    [Header("Confirm")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    private int _currentCarIndex = 0;
    private int _currentTrackIndex = 0;
    private bool _isEndless;

    private void Awake()
    {
        prevCarButton.onClick.AddListener(OnPrevCar);
        nextCarButton.onClick.AddListener(OnNextCar);
        prevTrackButton.onClick.AddListener(OnPrevTrack);
        nextTrackButton.onClick.AddListener(OnNextTrack);
        confirmButton.onClick.AddListener(OnConfirm);
        backButton.onClick.AddListener(OnBack);
    }

    private void OnDestroy()
    {
        prevCarButton.onClick.RemoveListener(OnPrevCar);
        nextCarButton.onClick.RemoveListener(OnNextCar);
        prevTrackButton.onClick.RemoveListener(OnPrevTrack);
        nextTrackButton.onClick.RemoveListener(OnNextTrack);
        confirmButton.onClick.RemoveListener(OnConfirm);
        backButton.onClick.RemoveListener(OnBack);
    }

    private void Start()
    {
        _isEndless = gameModeData.selectedMode == GameMode.Endless;
        trackSelectionPanel.SetActive(_isEndless);

        _currentCarIndex = 0;
        _currentTrackIndex = 0;

        UpdateCarUI();
        UpdateTrackUI();
    }


    private void OnPrevCar()
    {
        _currentCarIndex = (_currentCarIndex - 1 + cars.Length) % cars.Length;
        UpdateCarUI();
    }

    private void OnNextCar()
    {
        _currentCarIndex = (_currentCarIndex + 1) % cars.Length;
        UpdateCarUI();
    }

    private void UpdateCarUI()
    {
        if (cars.Length == 0) return;
        CarDataSO car = cars[_currentCarIndex];
        if (carNameText) carNameText.text = car.carName;
        if (carPreviewImage) carPreviewImage.sprite = car.previewImage;

        prevCarButton.interactable = cars.Length > 1;
        nextCarButton.interactable = cars.Length > 1;
    }


    private void OnPrevTrack()
    {
        _currentTrackIndex = (_currentTrackIndex - 1 + tracks.Length) % tracks.Length;
        UpdateTrackUI();
    }

    private void OnNextTrack()
    {
        _currentTrackIndex = (_currentTrackIndex + 1) % tracks.Length;
        UpdateTrackUI();
    }

    private void UpdateTrackUI()
    {
        if (tracks.Length == 0) return;
        TrackDataSO track = tracks[_currentTrackIndex];
        if (trackNameText) trackNameText.text = track.trackName;
        if (trackPreviewImage) trackPreviewImage.sprite = track.previewImage;

        prevTrackButton.interactable = tracks.Length > 1;
        nextTrackButton.interactable = tracks.Length > 1;
    }


    // Warning: Media - Modificar un ScriptableObject en runtime y serializarlo con SetDirty PERSISTE el valor entre sesiones del editor 
    // Warning: Baja - "GameScene" hardcoded. Si en el futuro hay más escenas de juego (una por modo), conviene seleccionarla por tipo de modo o SO.
    private void OnConfirm()
    {
        carSelectionData.selectedCarIndex = _currentCarIndex;
        carSelectionData.selectedTrackIndex = _currentTrackIndex;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(carSelectionData);
#endif

        SceneManager.LoadScene("GameScene");
    }

    private void OnBack()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}