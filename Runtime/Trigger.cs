using System;
using UnityEngine;

[Serializable]
public struct Trigger
{
    [SerializeField]
    private bool _value;
    [SerializeField]
    private bool _onlyFirst;
    [SerializeField]
    private bool _usedAlready;
    
    public Trigger(bool set = false, bool onlyFirst = false)
    {
        _value = set;
        _onlyFirst = onlyFirst;
        _usedAlready = false;
    }

    public Trigger Set()
    {
        if (_onlyFirst && _usedAlready) return this;
        _value = true;
        return this;
    }

    public Trigger Unset()
    {
        _value = false;
        return this;
    }

    public bool Get()
    {
        if (_value)
        {

            if (_onlyFirst) 
            {
                if (_usedAlready) return false;
                _usedAlready = true;
            }
            _value = false;
            return true;
        } else
        {
            return false;
        }
    }

    public static implicit operator bool(Trigger tr)
    {
        return tr.Get();
    }

    public static implicit operator Trigger(bool b) => new Trigger(b);

    public override string ToString()
    {
        if (_value)
        {
            return ("Set");
        } else
        {
            return ("Unset");
        }
    }
}