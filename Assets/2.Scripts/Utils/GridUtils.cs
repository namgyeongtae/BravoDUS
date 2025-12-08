using UnityEngine;

public class GridUtils
{
    public static Vector3 CalcCenterPosition(Vector3 position, int size, Vector3 CellSize)
    {
        bool isEven = size % 2 == 0;

        Vector3 startPos = Vector3.zero;

        if (!isEven)
        {
            float startPosX = position.x - (CellSize.x * (size / 2));
            float startPosZ = position.z - (CellSize.y * (size / 2));

            startPos = new Vector3(startPosX, 0, startPosZ);
        }
        else
        {
            float startPosX = position.x - CellSize.x / 2 - (CellSize.x * (size / 2 - 1));
            float startPosZ = position.z - CellSize.y / 2 - (CellSize.y * (size / 2 - 1));

            startPos = new Vector3(startPosX, 0, startPosZ);
        }

        return startPos;
    }
}
