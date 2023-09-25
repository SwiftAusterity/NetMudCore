using System.Net.Http;

namespace NetMudCore.Communication
{
    /// <summary>
    /// Negotiation and access to ALL player connection descriptors
    /// </summary>
    public static class SystemCommunicationsUtility
    {
        /// <summary>
        /// Sends a message to all live descriptors everywhere
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <returns>success status</returns>
        public static bool BroadcastToAll(string message)
        {
            //TODO: Make these find all servers cached and send to them
            return true;
        }

        /// <summary>
        /// Sends a message to all live descriptors on a port
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="portNumber">the port of the connection to send to</param>
        /// <returns>success status</returns>
        public static bool BroadcastToAll(string message, int portNumber)
        {
            //TODO: Make these find all servers of this port cached and send to them
            return true;
        }

        public static string GetPublicIP()
        {
            HttpClient webClient = new();

            HttpResponseMessage response = webClient.Send(new HttpRequestMessage(HttpMethod.Get, "http://checkip.dyndns.org/"));

            string direction = response.Content.ToString();

            //Search for the ip in the html
            int first = direction.IndexOf("Address: ") + 9;
            int last = direction.LastIndexOf("</body>");
            direction = direction[first..last];

            return direction;
        }
    }
}
