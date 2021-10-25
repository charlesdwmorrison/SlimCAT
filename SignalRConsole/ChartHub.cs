using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


public class ChartHub
{

    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    // Get the appsetting.json config settings. 
    // See https://dejanstojanovic.net/aspnet/2018/december/setting-up-kestrel-port-in-configuration-file-in-aspnet-core/
    // appsettings.json must be deployed as an embedded file. 
    public static IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseUrls($"{config.GetValue<string>("Host:HubURL")}")
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .UseStartup<Startup>();

}


// https://www.yogihosting.com/aspnet-core-enable-cors/
// https://dzone.com/articles/cors-in-net-core-net-core-security-part-vi
// https://forums.asp.net/t/2164579.aspx?Host+a+SignalR+Hub+in+a+NET+Core+3+1+Console
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }


    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // I know, I am doing this a second way! Not sure what is cleaner to do here. . . 
        // There is possibly a third way to get the connection:
        // https://stackoverflow.com/questions/60552768/how-to-acess-the-appsettings-in-blazor-webassembly
        // https://www.c-sharpcorner.com/article/appsettings-6-ways-to-read-the-config-in-asp-net-core-3-0/
        // https://quizdeveloper.com/tips/read-configuration-value-from-appsettingsdotjson-in-aspdotnet-core-aid24
        //services.AddSingleton(GetConfiguration());
        string curHubURL = Extensions.GetConfiguration("HubURL");
        Debug.WriteLine($"HubURL = {curHubURL}");

        // IF it's not working , try here: https://stackoverflow.com/questions/44379560/how-to-enable-cors-in-asp-net-core-webapi
        services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
        {
            builder.AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader()
             .AllowCredentials()
             .SetIsOriginAllowed((host) => true)
             .WithOrigins(curHubURL);
        }));

        // Might want to try
        // .WithHeaders(HeaderNames.AccessControlAllowHeaders, "Content-Type")

        // or

        //services.AddCors(options =>
        //{
        //    options.AddPolicy("AllowOrigin", builder =>
        //    builder.WithOrigins(new[] { "http://localhost:5000", "https://localhost:5000" })
        //    .AllowAnyHeader()
        //    .AllowAnyMethod()
        //    .AllowCredentials()
        //    .SetIsOriginAllowed((host) => true) //for signalr cors  
        //    );

        // https://stackoverflow.com/questions/37365277/how-to-specify-the-port-an-asp-net-core-application-is-hosted-on

        //services.AddSignalR();

        services.AddSignalR(hubOptions =>
        {
            hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(60);
            hubOptions.EnableDetailedErrors = true;
        });

    }



    // https://stackoverflow.com/questions/60745737/failed-to-connect-to-signalr-in-blazor-webassembly
    // https://stackoverflow.com/questions/57233956/why-azure-signalr-function-returns-cors-error-on-localhost-testing
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseCors("CorsPolicy");
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            Debug.WriteLine("Made it here to endpoint.");
            endpoints.MapHub<ChartHub>("/chartHub");
        });
    }

    public class ChartHub : Hub
    {
        public async Task MakeChartJsChart()
        {
            Console.WriteLine("MakeChartJsChart() on the Hub has been invoked.");
            await Clients.All.SendAsync("MakeChartJsChart");
        }

        public async Task AddData(int datasetId, int curDataPointForChartDataSet, double responseTime)
        {
            //Console.WriteLine("ChartHub:AddData() on the Hub has been invoked.");
            //Console.WriteLine($"Response Time ={responseTime}");
            Console.WriteLine($"SignalRHub:AddData(): Sending DataSetId = {datasetId}, DataPosition={curDataPointForChartDataSet},  ResponseTime = {responseTime}");

            await Clients.All.SendAsync("AddData", datasetId, curDataPointForChartDataSet, responseTime);
        }

        public async Task InitializeChartLine(string lineLabel, string rgbaStr, int lineId)
        {
            Console.WriteLine("ChartHub:InitializeChartLine() on the Hub has been started.");
            await Clients.All.SendAsync("InitializeChartLine", lineLabel, rgbaStr, lineId);
            Console.WriteLine("ChartHub:InitializeChartLine() on the Hub has been completed.");
        }

    }
}



/// <summary>
/// This is another way to load the appsettings.json file, but probably the  most correct approach is:
/// https://dejanstojanovic.net/aspnet/2018/december/setting-up-kestrel-port-in-configuration-file-in-aspnet-core/
/// </summary>
public static class Extensions
{
    public static string GetConfiguration(string configValue)
    {       
        string jsonConfigStr = "";
        string execPath = Assembly.GetExecutingAssembly().Location;
        string executionDir = Path.GetDirectoryName(execPath);
        string fileNameAndPath = Path.Combine(executionDir, "appsettings.json");
        if (File.Exists(fileNameAndPath))
        {
            jsonConfigStr = File.ReadAllText(fileNameAndPath);
        }

        var result = (string)JObject.Parse(jsonConfigStr)
                    .DescendantsAndSelf()
                    .OfType<JProperty>()
                    .Single(x => x.Name.Equals(configValue))
                    .Value;

        return result;
    }
}