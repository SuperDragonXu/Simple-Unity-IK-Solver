using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FindUtils
{
    public static Transform FindMyChild(Transform parentTF, string childName)
    {
        //���������в���
        Transform childTF = parentTF.Find(childName);
        if (childTF != null) return childTF;
        //�����⽻��������
        int count = parentTF.childCount;
        for (int i = 0; i < count; i++)
        {
            childTF = FindMyChild(parentTF.GetChild(i), childName);
            if (childTF != null) return childTF;
        }
        return null;
    }

    public static Transform FindChildExpend(this Transform parentTF, string childName)
    {
        //���������в���
        Transform childTF = parentTF.Find(childName);
        if (childTF != null) return childTF;
        //�����⽻��������
        int count = parentTF.childCount;
        for (int i = 0; i < count; i++)
        {
            childTF = FindMyChild(parentTF.GetChild(i), childName);
            if (childTF != null) return childTF;
        }
        return null;
    }

    public static T FindChildCompomentExpend<T>(this Transform parentTF, string childName)
    {
        return FindChildExpend(parentTF, childName).GetComponent<T>();
    }

}
