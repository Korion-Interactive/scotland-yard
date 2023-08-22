using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Bindings
{
    public class NamePartFailedArgs : System.EventArgs
    {
        public object A, B;
        public string InvokeMethodName;
        public FailTypes Reason;
    }
    public enum FailTypes
    {
        None,
        Parse,
        Invoke,
    }

}
