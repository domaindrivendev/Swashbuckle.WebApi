namespace Swashbuckle.Swagger20
{
    public interface ISwaggerProvider
    {
        SwaggerDocument GetSwaggerFor(string apiVersion);
    }
}