public interface IInput
{
    float Horizontal { get; }

    float Vertical { get; }

    int Jump { get; }

    bool Dodge { get; }

    bool Attack { get; }
}
