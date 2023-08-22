using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Duo<TA, TB>
{
       
    public TA A;
    public TB B;


    public Duo(TA a, TB b)
    {
        this.A = a;
        this.B = b;
    }
    public Duo(ref TA a, ref TB b)
    {
        this.A = a;
        this.B = b;
    }

    public override int GetHashCode()
    {
        return A.GetHashCode() ^ B.GetHashCode();
    }
}

public class Trio<TA, TB, TC>
{

    public TA A;
    public TB B;
    public TC C;


    public Trio(TA a, TB b, TC c)
    {
        this.A = a;
        this.B = b;
        this.C = c;
    }
    public Trio(ref TA a, ref TB b, ref TC c)
    {
        this.A = a;
        this.B = b;
        this.C = c;
    }

    public override int GetHashCode()
    {
        return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
    }
}