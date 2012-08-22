using System.ServiceModel;
using System.ServiceModel.Web;
using System.Collections.Generic;
using InfinityEngine.Models;

namespace InfinityEngine.Interfaces
{
    [ServiceContract]
    public interface IInfinityEngineService
    {
        
        //Call to create or update a search route's configuration
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "configure/",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json)]
        bool ConfigureSearchRoute(Configuration newConfig);

        //Call to update all of the data for a search route
        [OperationContract]
        [WebGet(UriTemplate = "update/{routeName}", ResponseFormat = WebMessageFormat.Json)]
        bool UpdateSearchRouteAll(string routeName);

        //Call to update only part of the data for the search route.  The updateKey is uses as a search
        //param for the Update Url's call
        [OperationContract]
        [WebGet(UriTemplate = "update/{routeName}/{updateKey}", ResponseFormat = WebMessageFormat.Json)]
        bool UpdateSearchRoute(string routeName, string updateKey);

        //Call to make an autocomplete search with no limit (max 1000 or the max configured for the route)
        [OperationContract]
        [WebGet(UriTemplate = "search/{routeName}/{searchParam}", ResponseFormat = WebMessageFormat.Json)]
        string SearchTheRoute(string routeName, string searchParam);

        //Call to make an autocomplete search with a limit.  (max 1000 or the max configured for the route)
        [OperationContract]
        [WebGet(UriTemplate = "search/{routeName}/{searchParam}/{resultLimit}", ResponseFormat = WebMessageFormat.Json)]
        string SearchTheRouteWithLimit(string routeName, string searchParam, string resultLimit);

    }

}
