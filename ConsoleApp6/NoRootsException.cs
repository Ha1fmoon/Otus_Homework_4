namespace ConsoleApp6;

public class NoRootsException : Exception
{
    public NoRootsException(int discriminant) : base($"No roots was found. Discriminant is negative ({discriminant}).") {}
}