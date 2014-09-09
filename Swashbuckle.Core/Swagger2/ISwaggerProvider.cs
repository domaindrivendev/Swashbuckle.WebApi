namespace Swashbuckle.Swagger2
{
    public interface ISwaggerProvider
    {
        SwaggerDocument GetSwaggerFor(string apiVersion);
    }
}