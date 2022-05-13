using System;
using System.Linq.Expressions;

namespace TheOtherRoles;

public static class Il2CppHelpers
{
    private static class CastHelper<T> where T : Il2CppObjectBase
    {
        public static Func<IntPtr, T> Cast;
        static CastHelper()
        {
            var typeofT = typeof(T);
            var typeofIntPtr = typeof(T);
            var constructor = typeofT.GetConstructor(new[] {typeofIntPtr});
            var ptr = Expression.Parameter(typeofIntPtr);
            var create = Expression.New(constructor!, ptr);
            var lambda = Expression.Lambda<Func<IntPtr, T>>(create, ptr);
            Cast = lambda.Compile();
        }
    }
    
    public static T CastFast<T>(this Il2CppObjectBase obj) where T : Il2CppObjectBase
    {
        return obj.Pointer.CastFast<T>();
    }
    
    public static T CastFast<T>(this IntPtr ptr)where T : Il2CppObjectBase
    {
        return CastHelper<T>.Cast(ptr);
    }
}