using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsController : MonoBehaviour
{
    [Header("Panel Cài Đặt Chính")]
    public GameObject panelCaiDat;

    [Header("Menu Lựa Chọn Ngôn Ngữ (Bật/Tắt)")]
    public GameObject languageOptionsPanel; // Kéo Panel_LanguageOptions vào đây

    [Header("Sliders Âm Thanh")]
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public AudioSource bgmAudioSource;

    [Header("Các Text Thay Đổi Ngôn Ngữ")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI thanhTriText;
    public TextMeshProUGUI tuongText;
    public TextMeshProUGUI settingsText;

    private string currentLang = "VN";

    void Start()
    {
        if (panelCaiDat != null) panelCaiDat.SetActive(false);
        if (languageOptionsPanel != null) languageOptionsPanel.SetActive(false);
        LoadSettings();
    }

    // --- MỞ / ĐÓNG BẢNG CÀI ĐẶT ---
    public void OpenSettings()
    {
        if (panelCaiDat != null) panelCaiDat.SetActive(true);
        if (languageOptionsPanel != null) languageOptionsPanel.SetActive(false); // Đảm bảo menu con tắt khi mới mở cài đặt
    }

    public void CloseSettings()
    {
        if (panelCaiDat != null) panelCaiDat.SetActive(false);
        SaveSettings();
    }

    // --- MENU LỰA CHỌN NGÔN NGỮ ---
    // Gắn vào nút Ngôn Ngữ chính: Bấm để mở bảng chọn Anh/Việt
    public void OpenLanguageMenu()
    {
        if (languageOptionsPanel != null)
        {
            languageOptionsPanel.SetActive(true);
        }
    }

    // Gắn vào Nút Thoát (Nút quay lại bảng Cài Đặt)
    public void CloseLanguageMenu()
    {
        if (languageOptionsPanel != null)
        {
            languageOptionsPanel.SetActive(false);
        }
    }

    // --- CHỌN NGÔN NGỮ ---
    public void SelectVietnamese()
    {
        currentLang = "VN";
        ApplyLanguage();
        CloseLanguageMenu(); // Chọn xong tự động ẩn menu ngôn ngữ đi
    }

    public void SelectEnglish()
    {
        currentLang = "EN";
        ApplyLanguage();
        CloseLanguageMenu(); // Chọn xong tự động ẩn menu ngôn ngữ đi
    }

    void ApplyLanguage()
    {
        if (currentLang == "VN")
        {
            if (titleText != null) titleText.text = "CÀI ĐẶT";
            if (thanhTriText != null) thanhTriText.text = "THÀNH TRÌ";
            if (tuongText != null) tuongText.text = "TƯỚNG";
            if (settingsText != null) settingsText.text = "CÀI ĐẶT";
        }
        else
        {
            if (titleText != null) titleText.text = "SETTINGS";
            if (thanhTriText != null) thanhTriText.text = "CASTLE";
            if (tuongText != null) tuongText.text = "HEROES";
            if (settingsText != null) settingsText.text = "SETTINGS";
        }
    }

    // --- ÂM LƯỢNG ---
    public void OnMasterVolumeChanged(float value) { AudioListener.volume = value; }
    public void OnBGMVolumeChanged(float value) { if (bgmAudioSource != null) bgmAudioSource.volume = value; }

    void SaveSettings()
    {
        if (masterVolumeSlider != null) PlayerPrefs.SetFloat("MasterVol", masterVolumeSlider.value);
        if (bgmVolumeSlider != null) PlayerPrefs.SetFloat("BGMVol", bgmVolumeSlider.value);
        PlayerPrefs.SetString("GameLanguage", currentLang);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        float savedMaster = PlayerPrefs.GetFloat("MasterVol", 1f);
        float savedBGM = PlayerPrefs.GetFloat("BGMVol", 1f);
        currentLang = PlayerPrefs.GetString("GameLanguage", "VN");

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = savedMaster;
            AudioListener.volume = savedMaster;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = savedBGM;
            if (bgmAudioSource != null) bgmAudioSource.volume = savedBGM;
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }
        ApplyLanguage();
    }
}