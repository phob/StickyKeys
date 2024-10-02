using System.Threading.Tasks;
using StickyKeysService;

namespace StickyKeysAgent
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var worker = new Worker();
            await worker.RunAsync();
        }
    }
}
