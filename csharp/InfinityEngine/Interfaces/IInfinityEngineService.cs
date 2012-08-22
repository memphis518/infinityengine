using System.ServiceModel;
using System.ServiceModel.Web;
using System.Collections.Generic;
using InfinityEngine.Models;

namespace InfinityEngine.Interfaces
{
    [ServiceContract]
    public interface IInfinityEngineService
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "configure/",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        bool ConfigureSearchRoute(Configuration newConfig);

        [OperationContract]
        [WebGet(UriTemplate = "update/{routeName}", ResponseFormat = WebMessageFormat.Json)]
        bool UpdateSearchRouteAll(string routeName);

        [OperationContract]
        [WebGet(UriTemplate = "update/{routeName}/{updateKey}", ResponseFormat = WebMessageFormat.Json)]
        bool UpdateSearchRoute(string routeName, string updateKey);

        [OperationContract]
        [WebGet(UriTemplate = "search/{routeName}/{searchParam}", ResponseFormat = WebMessageFormat.Json)]
        string SearchTheRoute(string routeName, string searchParam);

        [OperationContract]
        [WebGet(UriTemplate = "search/{routeName}/{searchParam}/{resultLimit}", ResponseFormat = WebMessageFormat.Json)]
        string SearchTheRouteWithLimit(string routeName, string searchParam, string resultLimit);

    }

}
