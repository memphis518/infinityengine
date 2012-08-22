using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InfinityEngine.Business.WebPoller
{
    public class JsonWebPoller : IWebPoller
    {

        public object PollURL(string url)
        {
            WebRequest webRequest;
            webRequest = WebRequest.Create(url);

            WebResponse webResponse = webRequest.GetResponse();
            string contentType = webResponse.ContentType;

            List<Dictionary<string, string>> deserializedResponse = null;
            if (System.Text.RegularExpressions.Regex.IsMatch(contentType, "^application/json"))
            {
                Stream objStream = webResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);

                string line = "";
                string responseText = "";
                while (line != null)
                {
                    line = objReader.ReadLine();
                    if (line != null)
                        responseText = responseText + line;
                }

                deserializedResponse = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(responseText);
            }
            else
            {
                throw new System.InvalidOperationException("Return content type was not JSON");
            }
            return deserializedResponse;
        }

    }
}
