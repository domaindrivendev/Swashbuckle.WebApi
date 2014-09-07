namespace Swashbuckle.Swagger2
{
    public interface ISwaggerProvider
    {
        SwaggerObject GetSwaggerFor(string apiVersion);
    }
}