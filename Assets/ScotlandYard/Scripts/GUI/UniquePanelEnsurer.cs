using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UniquePanelEnsurer : MonoBehaviour, IComparable<UniquePanelEnsurer>
{
    // [Comment("lower value: higher priority.", "priority which of two panels should be shown when both are there simultaniously.")]
    public int Priority;

    public ActiveAnimation Animation { get { return GetComponent<ActiveAnimation>(); } }


    void OnEnable()
    {
        UniquePanelManager.Instance.OnEnablePanel(this);
    }


    public int CompareTo(UniquePanelEnsurer other)
    {
        if (this.enabled && other.enabled)
        {
            bool ownIdentity = this.transform.localRotation == Quaternion.identity;
            bool otherIdentity = other.transform.localRotation == Quaternion.identity;
            if (ownIdentity != otherIdentity)
            {
                if (ownIdentity)
                    return -1;
                else
                    return 1;
            }
            else
            {
                return this.Priority.CompareTo(other.Priority);
            }

        }

        if (this.enabled && !other.enabled)
            return -1;

        if (!this.enabled && other.enabled)
            return 1;

        return 0;
    }

    public void Activate()
    {
        transform.ResetToIdentity();
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        transform.ResetToIdentity();
        gameObject.SetActive(false);
    }
}