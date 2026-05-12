using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UIOptions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button backBtn;
    [SerializeField] private GameObject pauseMenu;

    [Header("Sound Sliders")]
    [SerializeField] private Slider master;
    [SerializeField] private Slider sfx;
    [SerializeField] private Slider music;
    [SerializeField] private AudioMixer audioMixer;

    private void Awake()
    {
        float masterVol;
        audioMixer.GetFloat("VolumeMaster", out masterVol);
        master.value = masterVol;

        float sfxVol;
        audioMixer.GetFloat("VolumeSfx", out sfxVol);
        sfx.value = sfxVol;

        float musicVol;
        audioMixer.GetFloat("VolumeMusic", out musicVol);
        music.value = musicVol;

        backBtn.onClick.AddListener(BackBtnClicked);
        master.onValueChanged.AddListener(OnSliderMasterChanged);
        sfx.onValueChanged.AddListener(OnSliderSfxChanged);
        music.onValueChanged.AddListener(OnSliderMusicChanged);

    }

    private void OnDestroy()
    {
        backBtn.onClick.RemoveListener(BackBtnClicked);
        master.onValueChanged.RemoveListener(OnSliderMasterChanged);
        sfx.onValueChanged.RemoveListener(OnSliderSfxChanged);
        music.onValueChanged.RemoveListener(OnSliderMusicChanged);
    }

    private void OnSliderMasterChanged(float value)
    {
        audioMixer.SetFloat("VolumeMaster", value);
    }

    private void OnSliderMusicChanged(float value)
    {
        audioMixer.SetFloat("VolumeMusic", value);
    }

    private void OnSliderSfxChanged(float value)
    {
        audioMixer.SetFloat("VolumeSfx", value);

    }

    private void BackBtnClicked()
    {
        pauseMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

}
