using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TransportArgs : BaseArgs
{
    public TransportationType Transport;

    public TransportArgs(TransportationType transport)
    {
        this.Transport = transport;
    }

    public override string ToString()
    {
        return string.Format("Transport Args [{0}]", Transport.ToString());
    }
}