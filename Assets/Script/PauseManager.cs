using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Giao diện")]
    public GameObject pauseMenuUI;
    public GameObject pauseButton;

    [Header("Thanh âm lượng")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public AudioSource bgmAudioSource;

    [Header("Cấu hình Scene")]
    [Tooltip("Tên chính xác của Scene Menu chính")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Start()
    {
        float savedBGM = PlayerPrefs.GetFloat("BGMVol", 0.75f);
        if (bgmSlider != null)
        {
            bgmSlider.value = savedBGM;
            SetBGMVolume(savedBGM);
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        float savedSFX = PlayerPrefs.GetFloat("SFXVol", 0.75f);
        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFX;
            SetSFXVolume(savedSFX);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }

    // Hàm gọi khi bấm nút "Tiếp Tục" hoặc phím Esc để quay lại game
    public void ResumeGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
    }

    // Hàm gọi khi bấm nút "Thoát"
    public void QuitToMenu()
    {
        Time.timeScale = 1f; // Trả lại tốc độ thời gian bình thường trước khi chuyển Scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetBGMVolume(float value)
    {
        if (bgmAudioSource != null) bgmAudioSource.volume = value;
        PlayerPrefs.SetFloat("BGMVol", value);
    }

    public void SetSFXVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("SFXVol", value);
    }
}