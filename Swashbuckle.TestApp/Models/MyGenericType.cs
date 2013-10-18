namespace Swashbuckle.TestApp.Models
{
    public class MyGenericType<T>
    {
        public string TypeName
        {
            get { return typeof (T).Name; }
        }
    }
}