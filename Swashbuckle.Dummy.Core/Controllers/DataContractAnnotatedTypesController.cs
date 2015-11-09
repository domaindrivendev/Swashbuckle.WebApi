using System;
using System.Runtime.Serialization;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class DataContractAnnotatedTypesController : ApiController
    {
        public int Create(DataContractRequest request)
        {
            throw new NotImplementedException();
        }
    }

    public class DataContractRequest
    {
        public DataContractEnumWithOutValues OverriddenValues { get; set; }
        public DataContractEnumWithValues RawValues { get; set; }
    }

    [DataContract]
    public enum DataContractEnumWithValues
    {
        [EnumMember(Value = "aie")]
        A = 2,
        [EnumMember(Value = "bee")]
        B = 4
    }

    [DataContract]
    public enum DataContractEnumWithOutValues
    {
        [EnumMember]
        A = 2,
        [EnumMember]
        B = 4
    }
}