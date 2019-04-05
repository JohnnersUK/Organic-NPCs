using System;


public class Output : IComparable<Output>
{
    public Output()
    {
        ID = 0;
        Value = 0;
    }

    public int CompareTo(Output other)
    {
        if (other == null)
        {
            return 1;
        }
        else
        {
            return Value.CompareTo(other.Value);
        }
    }

    public int ID;
    public float Value;
}
