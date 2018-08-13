using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
[assembly: InternalsVisibleTo("Test.TimeArchiver")]
[assembly: InternalsVisibleTo("Test.Integration.TimeArchiver")]
[assembly: InternalsVisibleTo("Durability.TimeArchiver")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace TimeArchiver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}

