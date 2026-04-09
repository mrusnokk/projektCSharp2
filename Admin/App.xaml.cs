using Admin.Services;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Admin
{
    public partial class App : Application
    {
        public static ApiService ApiService { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ApiService = new ApiService();
        }
    }

}
