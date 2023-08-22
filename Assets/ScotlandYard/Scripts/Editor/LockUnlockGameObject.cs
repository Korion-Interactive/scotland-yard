using UnityEngine;
using UnityEditor;

public class LockUnlockGameObjects : ScriptableObject
{
    [MenuItem("Edit/Lock/Lock selected object %l")]
    static void LockObject()
    {
        int total_locked = 0;
        HideIncludingChildren(Selection.activeTransform, ref total_locked);
        Debug.Log("Locked " + total_locked + " objects");
    }

    //this enables the button
    [MenuItem("Edit/Lock/Lock selected object %l", true)]
    static bool LockObjectValidate()
    {
        return Selection.activeGameObject;
    }

    [MenuItem("Edit/Lock/Unlock all locked objects %#l")]
    static void UnlockAllObjects()
    {
        int total_unlocked = 0;
        Object[] targets = FindObjectsOfType(typeof(GameObject));
        foreach (GameObject target in targets)
        {
            if (target.hideFlags == HideFlags.HideInHierarchy)
            {
                target.hideFlags = (HideFlags)0;
                total_unlocked++;
            };  // else nothing to unhide
        }
        Debug.Log("Unlocked " + total_unlocked + " objects");
    }

    static void HideIncludingChildren(Transform root_transform, ref int total)
    {
        if (root_transform.gameObject.hideFlags == (HideFlags)0)
        {
            root_transform.gameObject.hideFlags = HideFlags.HideInHierarchy;
            total++;
        };
        foreach (Transform transform in root_transform)
        {
            HideIncludingChildren(transform, ref total);
        }
    }
}