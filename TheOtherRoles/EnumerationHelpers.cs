using System;
using System.Collections;
using System.Linq.Expressions;
using Il2CppSystem.Collections.Generic;

namespace TheOtherRoles;

public static class EnumerationHelpers
{ 
    public static System.Collections.Generic.IEnumerable<T> GetFastEnumerator<T>(this List<T> list) where T : Il2CppSystem.Object => new Il2CppListEnumerable<T>(list);
}

public unsafe class Il2CppListEnumerable<T> : System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEnumerator<T> where T : Il2CppSystem.Object
{
    private static readonly int _elemSize;
    private static readonly int _offset;
    private static Func<IntPtr, T> _objFactory;
    
    static Il2CppListEnumerable()
    {
        _elemSize = IntPtr.Size;
        _offset = 4 * IntPtr.Size;

        var constructor = typeof(T).GetConstructor(new[] {typeof(IntPtr)});
        var ptr = Expression.Parameter(typeof(IntPtr));
        var create = Expression.New(constructor!, ptr);
        var lambda = Expression.Lambda<Func<IntPtr, T>>(create, ptr);
        _objFactory = lambda.Compile();
    }

    private readonly IntPtr _arrayPointer;
    private readonly int _count;
    private int _index = -1;

    public Il2CppListEnumerable(List<T> list)
    {
        _count = list.Count;
        _arrayPointer = *(IntPtr*) list._items.Pointer;
    }

    object IEnumerator.Current => Current;
    public T Current { get; private set; }

    public bool MoveNext()
    {
        if (++_index >= _count) return false;
        var refPtr = *(IntPtr*) IntPtr.Add(IntPtr.Add(_arrayPointer, _offset), _index * _elemSize);
        Current = _objFactory(refPtr);
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