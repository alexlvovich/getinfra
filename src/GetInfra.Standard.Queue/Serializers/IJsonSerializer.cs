namespace GetInfra.Standard.Queue
{
    public interface IJsonSerializer
    {
        string Serialize(object value);

        T Deserialize<T>(string value);

        T Deserialize<T>(byte[] data);
    }
}
