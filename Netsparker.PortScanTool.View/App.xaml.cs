using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netsparker.PortScanTool.Helpers;
using Netsparker.PortScanTool.ViewModel;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Netsparker.PortScanTool.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<MainWindow>();
            services.AddScoped<PortScanToolViewModel>();
            services.AddScoped<IIPDataProvider, IPDataProvider>();
            services.AddScoped<IIPPortScanner, IPPortScanner>();
            //Add NLog for logging.
            services.AddLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Information);
                x.AddNLog("nlog.config");
            });
        }
    }
}
