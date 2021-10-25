using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace SlimCAT.SignalR
{
    class SignalRClient
    {
        private static string url = "http://localhost:5003/charthub";
        private static HubConnection connection;

        public SignalRClient()
        {
            connection = new HubConnectionBuilder()
              .WithUrl(url)
              .WithAutomaticReconnect()
              .Build();

            connection.StartAsync();
        }

        public async void MakeChartJsChart()
        {
            // The work of initialize the lines on the chart is done in the JavaScript
            // This is just here to clear the browser between load tests.
            Debug.WriteLine($"SignalRClient:MakeChartJsChart()");
            await connection.InvokeAsync("MakeChartJsChart");
        }

        public async void AddData(int dataSetId, int curDataPointForChartDataSet, double ttlb)
        {
            Debug.WriteLine($"SignalRClient:AddData(): Sending DataSetId = {dataSetId}, DataPosition={curDataPointForChartDataSet},  ResponseTime = {ttlb}");
            await connection.InvokeAsync("AddData", dataSetId, curDataPointForChartDataSet, ttlb);
        }

        // ToDo: I don't think I need the third parameter here.
        public async void InitializeChartLine(string axisLabel, string rgbaStr, int lineId)
        {
            Debug.WriteLine($"SignalRClient:InitializeChartLine(): Sending2 {lineId}");
            await connection.InvokeAsync("InitializeChartLine", axisLabel, rgbaStr, lineId);
        }
    }
}


