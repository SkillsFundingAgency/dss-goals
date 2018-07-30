using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetGoalFromRequest<T>(HttpRequestMessage req);
        Guid? GetTouchpointId(HttpRequestMessage req);
    }
}