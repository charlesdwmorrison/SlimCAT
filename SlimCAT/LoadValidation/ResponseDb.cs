using System.Collections.Concurrent;

namespace SlimCAT
{
    public class ResponseDb
    {
        /// <summary>
        /// Collection to hold all the response information. 
        /// This must be accessible to both the calling method and this class, hence it is public. 
        /// ToDo:This should really be a singleton 
        /// </summary>
        public static ConcurrentDictionary<int, SlimCatResponse>
            conCurResponseDict = new ConcurrentDictionary<int, SlimCatResponse>();

    }
}
