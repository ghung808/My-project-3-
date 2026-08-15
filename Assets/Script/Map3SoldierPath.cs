using UnityEngine;

public class Map3SoldierPath : MonoBehaviour
{
    [Header("Waypoints Map 3")]
    public WaypointsMap3 waypointsMap3;

    [Header("Đường lính")]
    public Map3Lane lane = Map3Lane.Middle;

    public Transform[] GetPath()
    {
        if (waypointsMap3 == null)
        {
            Debug.LogError("Map3SoldierPath: Chưa gán WaypointsMap3!");
            return null;
        }

        switch (lane)
        {
            case Map3Lane.Top:
                return waypointsMap3.topPoints;

            case Map3Lane.Bottom:
                return waypointsMap3.bottomPoints;

            case Map3Lane.Middle:
            default:
                return waypointsMap3.middlePoints;
        }
    }

    public Transform GetFirstPoint()
    {
        Transform[] path = GetPath();

        if (path == null || path.Length == 0)
        {
            Debug.LogError("Map3SoldierPath: Đường đang chọn chưa có waypoint!");
            return null;
        }

        return path[0];
    }
}

public enum Map3Lane
{
    Middle,
    Top,
    Bottom
}