using UnityEngine;

[System.Serializable]
public class Wave
{
    public string waveName;        // Tên đợt (Ví dụ: Wave 1, Wave 7 - BOSS)
    public GameObject enemyPrefab; // Loại quái sẽ xuất hiện trong đợt này
    public int count;              // Số lượng quái xuất hiện
    public float rate;             // Khoảng thời gian giãn cách giữa mỗi con (giây)
}