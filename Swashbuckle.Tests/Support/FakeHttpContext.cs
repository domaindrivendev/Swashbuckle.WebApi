//using System.IO;
//using System.Web;
//using Moq;
//
//namespace Swashbuckle.Tests.Support
//{
//    public class FakeHttpContext : HttpContextBase
//    {
//        private readonly HttpRequestBase _request;
//        private readonly HttpResponseBase _response = new FakeHttpResponse();
//
//        public FakeHttpContext(string requestPath)
//        {
//            var requestMock = new Mock<HttpRequestBase>();
//            requestMock.SetupGet(r => r.Path).Returns(requestPath);
//            _request = requestMock.Object;
//        }
//
//        public override HttpRequestBase Request
//        {
//            get { return _request;  }
//        }
//
//        public override HttpResponseBase Response
//        {
//            get { return _response; }
//        }
//    }
//
//    public class FakeHttpResponse : HttpResponseBase
//    {
//        private readonly Stream _outputStream = new MemoryStream();
//
//        public override string ContentType { get; set; }
//
//        public override Stream Filter { get; set; }
//
//        public override Stream OutputStream
//        {
//            get { return _outputStream; }
//        }
//
//        public override void Clear()
//        {
//        }
//    }
//}