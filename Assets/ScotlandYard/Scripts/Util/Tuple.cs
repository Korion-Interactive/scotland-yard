using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class Tuple<TA, TB>
{
    [FullInspector.ShowInInspector]
    [SerializeField]
    TA a;
    [FullInspector.ShowInInspector]
    [SerializeField]
    TB b;

    public TA A { get { return a; } }
    public TB B { get { return b; } }

    public Tuple(TA a, TB b)
    {
        this.a = a;
        this.b = b;
    }

    public override int GetHashCode()
    {
        int hashA = (a == null) ? 0 : a.GetHashCode();
        int hashB = (b == null) ? 0 : b.GetHashCode();

        return hashA ^ hashB;
    }
}


[Serializable]
public class Tuple<TA, TB, TC>
{
    [FullInspector.ShowInInspector]
    [SerializeField]
    TA a;
    [FullInspector.ShowInInspector]
    [SerializeField]
    TB b;
    [FullInspector.ShowInInspector]
    [SerializeField]
    TC c;

    public TA A { get { return a; } }
    public TB B { get { return b; } }
    public TC C { get { return c; } }

    public Tuple(TA a, TB b, TC c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
    public override int GetHashCode()
    {
        int hashA = (a == null) ? 0 : a.GetHashCode();
        int hashB = (b == null) ? 0 : b.GetHashCode();
        int hashC = (c == null) ? 0 : c.GetHashCode();
        
        return hashA ^ hashB ^ hashC;
    }
}


[Serializable]
public class Tuple<TA, TB, TC, TD>
{
    [FullInspector.ShowInInspector]
    [SerializeField]
    TA a;
    [FullInspector.ShowInInspector]
    [SerializeField]
    TB b;
    [FullInspector.ShowInInspector]
    [SerializeField]
    TC c;
    [FullInspector.ShowInInspector]
    [SerializeField]
    TD d;

    public TA A { get { return a; } }
    public TB B { get { return b; } }
    public TC C { get { return c; } }
    public TD D { get { return d; } }

    public Tuple(TA a, TB b, TC c, TD d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }
    public override int GetHashCode()
    {
        int hashA = (a == null) ? 0 : a.GetHashCode();
        int hashB = (b == null) ? 0 : b.GetHashCode();
        int hashC = (c == null) ? 0 : c.GetHashCode();
        int hashD = (d == null) ? 0 : d.GetHashCode();

        return hashA ^ hashB ^ hashC ^ hashD;
    }
}