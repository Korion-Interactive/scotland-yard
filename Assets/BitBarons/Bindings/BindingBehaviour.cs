using BitBarons.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BitBarons.Bindings
{
    public enum UpdateIntervallType
    {
        OnEnable,
        TimeIntervall,
        FrameIntervall,
    }

    [AddComponentMenu("Scripts/Bit Barons/Binding")]
    public class BindingBehaviour : MonoBehaviour
    {
        public VariableAccessor Accessor = new VariableAccessor();
        public List<BaseNamePart> NameParts = new List<BaseNamePart>();
        public string Format;

        public GameObject BoundObject;

        public UpdateIntervallType UpdateType;
        public float UpdateIntervall;

        void Awake()
        {
            if (Accessor.Object == null)
                Accessor.Object = this.gameObject;
        }

        public void OnEnable()
        {

            switch(UpdateType)
            {
                case UpdateIntervallType.OnEnable:
                    UpdateValue();
                    break;
                case UpdateIntervallType.FrameIntervall:
                    StartCoroutine(coUpdateFrameBased());
                    break;
                case UpdateIntervallType.TimeIntervall:
                    StartCoroutine(coUpdateTimeBased());
                    break;
                default:
                    throw new Exception("unexpected UpdateIntervallType");
            }
        }

        void OnDisable()
        {
            this.StopAllCoroutines();
        }

        public void UpdateValue()
        {
            try
            {
                Accessor.ParseAndSetValue(SharedMethods.GetStringValue(NameParts, Format, BoundObject));
            }
            catch(Exception ex)
            {
                this.LogError(ex);
            }
        }

        IEnumerator coUpdateFrameBased()
        {
            while (true)
            {
                UpdateValue();

                yield return new WaitForEndOfFrame(); // wait at least one frame

                for (int i = 0; i < UpdateIntervall - 1; i++)
                    yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator coUpdateTimeBased()
        {
            while (true)
            {
                UpdateValue();
                yield return new WaitForSeconds(UpdateIntervall);
            }
        }
    }
}
