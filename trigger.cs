public class trigger
{
    private bool value;
    private bool onlyFirst;
    private bool usedAlready;
    public trigger(){}
    public trigger(bool set)
    {
        value = set;
    }
    public trigger(bool set, bool onlyFirst)
    {
        this.onlyFirst = onlyFirst;
    }

    public trigger set()
    {
        if (onlyFirst && usedAlready) return this;
        value = true;
        return this;
    }

    public trigger unset()
    {
        value = false;
        return this;
    }

    public bool get()
    {
        return value;
    }

    public static implicit operator bool(trigger tr)
    {
        if (tr.value)
        {

            if (tr.onlyFirst) 
            {
                if (tr.usedAlready) return false;
                tr.usedAlready = true;
            }
            tr.value = false;
            return true;
        } else
        {
            return false;
        }
    }

    public static implicit operator trigger(bool b) => new trigger(b);

    public override string ToString()
    {
        if (value)
        {
            return ("Set");
        } else
        {
            return ("Unset");
        }
    }
}