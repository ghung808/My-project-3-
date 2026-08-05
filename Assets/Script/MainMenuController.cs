using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Hàm mở Thành Trì
    public void OnClickThanhTri()
    {
        Debug.Log("Đã bấm: Thành Trì");
        // SceneManager.LoadScene("ThanhTriScene"); // Bỏ comment nếu muốn đổi scene
    }

    // Hàm mở Tướng
    public void OnClickTuong()
    {
        Debug.Log("Đã bấm: Tướng");
        // SceneManager.LoadScene("TuongScene");
    }

    // Hàm mở Cửa Hàng
    public void OnClickCuaHang()
    {
        Debug.Log("Đã bấm: Cửa Hàng");
        // SceneManager.LoadScene("CuaHangScene");
    }

    // Hàm mở Thử Thách
    public void OnClickThuThach()
    {
        Debug.Log("Đã bấm: Thử Thách");
        // SceneManager.LoadScene("ThuThachScene");
    }

    // Hàm mở Hòm Thư
    public void OnClickThu()
    {
        Debug.Log("Đã bấm: Hòm Thư");
        // Hiển thị Panel Thư ở đây
    }

    // Hàm mở Cài Đặt
    public void OnClickCaiDat()
    {
        Debug.Log("Đã bấm: Cài Đặt");
        // Hiển thị Panel Cài đặt ở đây
    }
}