using System;
using System.Collections;
using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;

public static class EnumerationHelpers
{
    public static System.Collections.Generic.IEnumerator<T> GetFastStructEnumerator<T>(this List<T> list) where T : unmanaged => new Il2CppStructListEnumerator<T>(list);
    public static System.Collections.Generic.IEnumerator<T> GetFastRefEnumerator<T>(this List<T> list) where T : Il2CppObjectBase => new Il2CppRefListEnumerator<T>(list);
}

public unsafe class Il2CppStructListEnumerator<T> : System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEnumerator<T> where T : unmanaged
{
    private T* _arrayPtr;
    private int _index;
    private int _length;
    private T _current;

    internal Il2CppStructListEnumerator(List<T> list)
    {
        _arrayPtr = (T*) IntPtr.Add(list.Pointer, 4 * IntPtr.Size).ToPointer();
        _length = list.Count;
        _current = default;
    }

    public bool MoveNext()
    {
        if (_index >= _length)  return false;
        
        _current = _arrayPtr[_index];
        _index++;
        return true;
    }

    public void Reset()
    {
        _index = 0;
        _current = default;
    }

    public T Current => _current;

    object IEnumerator.Current => _current;
    
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

public class Il2CppRefListEnumerator<T> : System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEnumerator<T> where T : Il2CppObjectBase
{
    private Il2CppReferenceArray<T> _array;
    private int _index;
    private int _length;
    private T _current;

    internal Il2CppRefListEnumerator(List<T> list)
    {
        _array = new(list._items.Pointer);
        _length = list.Count;
        _current = default;
    }

    public bool MoveNext()
    {
        if (_index >= _length)  return false;
        
        _current = _array[_index];
        _index++;
        return true;
    }

    public void Reset()
    {
        _index = 0;
        _current = default;
    }

    public T Current => _current;

    object IEnumerator.Current => _current;

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