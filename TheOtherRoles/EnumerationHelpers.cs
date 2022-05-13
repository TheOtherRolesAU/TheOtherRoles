using System;
using System.Collections;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;

namespace TheOtherRoles;

public static class EnumerationHelpers
{
    public static object ReferenceObj;

    [HarmonyPatch(typeof(Il2CppObjectBase), nameof(Il2CppObjectBase.Pointer), MethodType.Getter)]
    public static class ObjectPatches
    {
        [HarmonyPrefix]
        private static bool Prefix(Il2CppObjectBase __instance, bool ___isWrapped, uint ___myGcHandle, ref IntPtr __result)
        {
            if (__instance != ReferenceObj) return true;
            __result = (IntPtr) ___myGcHandle;
            return false;
        }
    }

    public static System.Collections.Generic.IEnumerable<T> GetFastRefEnumerator<T>(this List<T> list) where T : Il2CppSystem.Object => new Il2CppListEnumerable<T>(list);
}

public unsafe class Il2CppListEnumerable<T> : System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEnumerator<T> where T : Il2CppSystem.Object
{
    private static readonly int _elemSize;
    private static readonly int _offset;
    private static Func<T, IntPtr> _cctor;
    
    static Il2CppListEnumerable()
    {
        _elemSize = IntPtr.Size;
        _offset = 4 * IntPtr.Size;

        var constructor = typeof(T).GetConstructor(new[]
        {
            typeof(IntPtr)
        });


        ParameterExpression ptr = Expression.Parameter(typeof(IntPtr));
        var create = Expression.New(constructor!, ptr);
        create.
        
        _object = (T) FormatterServices.GetUninitializedObject(typeof(T));
        var field = AccessTools.Field(typeof(T), "myGcHandle");

        ParameterExpression target = Expression.Parameter(typeof(T));
        ParameterExpression value = Expression.Parameter(typeof(uint));

        MemberExpression fieldExp = Expression.Field(target, field);
        BinaryExpression setExp = Expression.Assign(fieldExp, value);

        _setMyGcHandle = Expression.Lambda<Action<T, uint>>(setExp, target, value).Compile();
    }


    private readonly IntPtr _arrayPointer;
    private readonly int _count;
    private int _index = -1;

    public Il2CppListEnumerable(List<T> list)
    {
        _count = list.Count;
        _arrayPointer = *(IntPtr*) list._items.Pointer;
    }

    object IEnumerator.Current => EnumerationHelpers.ReferenceObj = _object;
    public T Current => (T) (EnumerationHelpers.ReferenceObj = _object);

    public bool MoveNext()
    {
        if (++_index >= _count) return false;
        var refPtr = *(IntPtr*) IntPtr.Add(IntPtr.Add(_arrayPointer, _offset), _index * _elemSize);
        _setMyGcHandle(_object, (uint)refPtr);
        return true;
    }

    public void Reset()
    {
        _index = -1;
    }
    
    public System.Collections.Generic.IEnumerator<T> GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }

    public void Dispose()
    {
    }
}