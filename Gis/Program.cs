using System;
using System.Net;
using Gis.Crypto;
using Gis.Infrastructure.NsiCommonService;

namespace Gis
{
	class Program
	{
		public static string SENDERID = "b44a519c-0cb2-42f8-bdbb-a0177c140334";
		static void Main(string[] args)
		{
			//https://msdn.microsoft.com/en-us/library/system.net.servicepointmanager.servercertificatevalidationcallback.aspx
			//необходимо, что бы подавить эксэпшн из-за кривого сертификата на сервере
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

			var service = new  NsiPortsTypeAsyncClient();
            service.ClientCredentials.UserName.UserName = "lanit";
            service.ClientCredentials.UserName.Password = "tv,n8!Ya";

            var request = new exportNsiListRequest1
            {
                ISRequestHeader = new ISRequestHeader
                {
                    Date = DateTime.Now,
                    MessageGUID = Guid.NewGuid().ToString()
                },
                exportNsiListRequest = new exportNsiListRequest
                {
                    version = "10.0.1.2",
                    ListGroup = ListGroup.NSI,
                    ListGroupSpecified = true,
                    Id = CryptoConsts.CONTAINER_ID
                }
            };

            var result = service.exportNsiList(request);

            bool flag = true;

            var stateheader = new ISRequestHeader()
            {
                Date = DateTime.Now,
                MessageGUID = Guid.NewGuid().ToString(),
            };

            var stateRequest = new getStateRequest()
            {
                MessageGUID = (result).AckRequest.Ack.MessageGUID
            };

            var staterequest = new getStateRequest1()
            {
                ISRequestHeader = stateheader,
                getStateRequest = stateRequest,
            };

            DateTime timeStart = DateTime.Now;
            DateTime timeEnd;

            getStateResponse stateresponse = null;
            while (flag)
            {
                timeEnd = DateTime.Now;
                TimeSpan span = timeEnd.Subtract(timeStart);
                if (span.Seconds >= 5)
                {
                    stateresponse = service.getState(staterequest);
                    Console.WriteLine(stateresponse.getStateResult.RequestState.ToString());
                    if (stateresponse.getStateResult.RequestState == 3)
                    {
                        flag = false;
                        Console.WriteLine("Запрос обработан");
                    }
                    timeStart = DateTime.Now;
                }
            }
            if (stateresponse.getStateResult.Item.GetType().Name == "ErrorMessageType")
            {
                Console.WriteLine(((ErrorMessageType)stateresponse.getStateResult.Item).Description);
            }
            else
            {
                foreach (NsiElementType item in ((NsiItemType)stateresponse.getStateResult.Item).NsiElement)
                {
                    Console.WriteLine(item);
                }
            }
            Console.ReadLine();
        }
	}
}