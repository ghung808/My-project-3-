using UnityEngine;

public class Waypoints : MonoBehaviour
{
    // Mảng chứa tất cả các điểm mốc Transform
    public static Transform[] points;

    void Awake()
    {
        // Tự động tìm và nạp tất cả các ô con (Waypoint 0, 1, 2...) vào mảng
        points = new Transform[transform.childCount];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }
}