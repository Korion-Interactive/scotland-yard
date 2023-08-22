﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Linq.Expressions;

using BitBarons.Util;


namespace BitBarons.Bindings
{
    [Serializable]
    public class ComplexBinding : BaseNamePart
    {
        public GameObject AffectedObject;
        public List<BaseNamePart> NamePartsA = new List<BaseNamePart>();
        public List<BaseNamePart> NamePartsB = new List<BaseNamePart>();
        public string FormatA = "";
        public string FormatB = "";
        public int TypeAIdx, TypeBIdx;
        public MathOperator Operator;

        List<Type> availableTypes;
        public Type TypeA { get { return GetAvailableTypes()[TypeAIdx]; } }
        public Type TypeB { get { return GetAvailableTypes()[TypeBIdx]; } }

        public event EventHandler<NamePartFailedArgs> GetNamePartFailed;
        
        public bool ProgressingNamePart;

        public override string NamePartType { get { return BaseNamePart.TYPE_COMPLEX; } }


        List<Type> GetAvailableTypes()
        {
            if (availableTypes == null)
            {
                availableTypes = Parser.GetAllRegisteredParseTypes().ToList();
                availableTypes.Insert(0, null);
            }
            
            return availableTypes;
        }
        public override void Initialize(GameObject previewObject)
        {
            
            AffectedObject = previewObject;
        }

    
        public override string GetNamePart(UnityEngine.GameObject prefab, int number)
        {
            if (ProgressingNamePart)
                return "< generating... >";

            ProgressingNamePart = true;

            object a, b;
            try
            {
                 a = Parser.Parse(TypeA, SharedMethods.GetStringValue(NamePartsA, FormatA, prefab));
                 b = Parser.Parse(TypeB, SharedMethods.GetStringValue(NamePartsB, FormatB, prefab));
            }
            catch(Exception)
            {
                if (GetNamePartFailed != null)
                    GetNamePartFailed(this, new NamePartFailedArgs() { Reason = FailTypes.Parse });

                ProgressingNamePart = false;
                return "< parse error >";
            }

            string methodName = Operator.ToString();

            string result;
            try
            {
                Type type = typeof(BitBarons.AutoGenerated.MathOperations);
                var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Where((o) =>
                    {
                        if(o.Name == methodName)
                        {
                            var parameters = o.GetParameters();
                            return parameters[0].ParameterType == TypeA && parameters[1].ParameterType == TypeB;
                        }
                        return false;
                    }).ToArray();

                result = (string)methods[0].Invoke(null, new object[] { a, b });

            }
            catch 
            {
                //result = CreateMethod(methodName, a, b) ?? "< generating... >";

                if (GetNamePartFailed != null)
                    GetNamePartFailed(this, new NamePartFailedArgs()
                {
                    A = a,
                    B = b,
                    InvokeMethodName = methodName,
                    Reason = FailTypes.Invoke,
                });

                return "< ... >";
            }

            ProgressingNamePart = false;
            return result;
        }



    }
}
