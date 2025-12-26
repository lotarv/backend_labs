namespace UniverseLabs.Oms.Consumer.Base;

public class Message<T>
{
    public string Key { get; set; } = string.Empty;

    public T Body { get; set; } = default!;
}
