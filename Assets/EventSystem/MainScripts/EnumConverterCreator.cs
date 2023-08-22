using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

/*
 * Credit to Michael Cederberg for the code below, which was taken from his second post on
 * this forum: http://social.msdn.microsoft.com/Forums/vstudio/en-US/d8ae015c-ccce-4e34-b848-a9c804a9465a/converting-between-generic-enum-and-intlong-without-boxing
 * I have changed the name of the class, but otherwide have not modified his code in any way.
 * 
 * This class is used in the EventManager class, and is necessary for it to function properly. DO NOT REMOVE!!
 * */
public static class EnumConverterCreator
{
    public static Func<TEnum, int> ConvertToInt32<TEnum>()
        where TEnum : struct
    {
        return (o) => Convert.ToInt32(o);
    }
}