using Microsoft.AspNetCore.SignalR;
using Serilog;
using System.Xml.Linq;
using TRPO_Web_start.Controllers;

namespace TRPO_Web_start
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSignalR();

            builder.Services.AddSingleton<IChatStateService, ChatStateService>();

            builder.Services.AddScoped<ChatController>(provider =>
            {
                var hubContext = provider.GetRequiredService<IHubContext<MainHub>>();
                var chatStateService = provider.GetRequiredService<IChatStateService>();
                return new ChatController(hubContext, chatStateService);
            });

            var appName = typeof(Program).Assembly.GetName().Name;

            var loggerConfiguration = new LoggerConfiguration();

            Log.Logger = loggerConfiguration.MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            builder.Host.UseSerilog();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.MapHub<MainHub>("/hub");

            app.Run();
        }
    }

}
