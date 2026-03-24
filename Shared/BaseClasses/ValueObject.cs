namespace Shared.BaseClasses;

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        ValueObject other = (ValueObject)obj;
        return other.GetEqualityComponents().SequenceEqual(GetEqualityComponents());
    }
    
    public override int GetHashCode()
    {
        int hash = 1;
        foreach (object component in GetEqualityComponents())
        {
            hash = HashCode.Combine(hash, component?.GetHashCode() ?? 0);
        }
        return hash;
    }
    
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null)
        {
            return right is null;
        }
        return left.Equals(right);
    }
    
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}