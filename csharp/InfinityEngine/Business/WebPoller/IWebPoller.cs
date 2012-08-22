using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfinityEngine.Business.WebPoller
{
    public interface IWebPoller
    {
        object PollURL(string url);
    }
}
