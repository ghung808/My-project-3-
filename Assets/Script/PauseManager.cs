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
            if (isPaused) ClosePauseMenu();
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

    public void ClosePauseMenu()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
    }

    // --- NÚT TIẾP TỤC NGOÀI MENU CHÍNH (Tải lại Map đã lưu hoặc Map tiếp theo) ---
    public void ContinueGame(string levelName)
    {
        Time.timeScale = 1f;
        // Hoặc bạn có thể dùng PlayerPrefs để lưu tên map gần nhất rồi LoadScene theo tên đó
        SceneManager.LoadScene(levelName);
    }

    public void QuitToMenu(string menuSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
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

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}