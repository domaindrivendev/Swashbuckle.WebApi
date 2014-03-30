namespace Swashbuckle.TestApp.Core.Models
{
    public class MyGenericType<T>
    {
        public string TypeName
        {
            get { return typeof (T).Name; }
        }
    }
}