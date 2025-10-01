using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
    public static Transform FindChildRecursively(this Transform transform, string childName)
    {
        foreach (Transform child in transform)
        {
            if (child.name == childName)
                return child;
            
            Transform finding = FindChildRecursively(child, childName);

            if (finding != null)
                return finding;
        }

        return null;
    }

    public static void CachedChildTransform(this Transform transform, Dictionary<string, Transform> childTransform)
    {
        foreach (Transform child in transform)
        {
            if (childTransform.ContainsKey(child.name) == false)
                childTransform.Add(child.name, child);

            if (child.childCount > 0)
                CachedChildTransform(child, childTransform);
        }
    }
}
