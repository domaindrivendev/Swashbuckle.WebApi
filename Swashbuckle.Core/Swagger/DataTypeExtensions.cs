namespace Swashbuckle.Swagger
{
    public static class DataTypeExtensions
    {
        public static void CopyValuesFrom(this DataType dataType, DataType source)
        {
            dataType.Type = source.Type;
            dataType.Ref = source.Ref;
            dataType.Format = source.Format;
            dataType.DefaultValue = source.DefaultValue;
            dataType.Enum = source.Enum;
            dataType.Minimum = source.Minimum;
            dataType.Maximum = source.Maximum;
            dataType.Items = source.Items;
            dataType.UniqueItems = source.UniqueItems;
        }
    }
}