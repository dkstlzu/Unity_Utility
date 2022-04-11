public class trigger
{
    private bool value;
    public trigger(bool set = false)
    {
        value = set;
    }
    public trigger set()
    {
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
            tr.value = false;
            return true;
        } else
        {
            return false;
        }
    }
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