using Serilog;
using System.Xml.Linq;

namespace TRPO_Web_start
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSignalR();

            //builder.Services.AddTransient<IProductService, ProductService>(); // при каждом запросе экземпляра будет создаваться новый класс 
            //builder.Services.AddSingleton<IProductService, ProductService>(); // создается один экземпляр

            builder.Services.AddScoped<IProductService>(provider =>
            {
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor.HttpContext;

                if (httpContext.Request.Headers.TryGetValue("X-I-AM-VIP", out var _))
                {
                    return new VipProductService();
                }
                return new ProductService();

            }); // создается на каждый HTTP запрос


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

    public interface IProductService
    {
        IEnumerable<Product> GetProducts();
    }
    public class ProductService : IProductService
    {
        private Guid _id = Guid.NewGuid();
        public IEnumerable<Product> GetProducts()
        {
            yield return new Product{Id = 1, Name = "Помидорчики", Price = 32};
            yield return new Product{Id = 1, Name = "Огурчики", Price = 56};
            yield return new Product{Id = 1, Name = "Кабачки", Price = 208};
        }
    }

    public class VipProductService : IProductService
    {
        private Guid _id = Guid.NewGuid();
        public IEnumerable<Product> GetProducts()
        {
            yield return new Product { Id = 1, Name = "Помидорчики", Price = 32 };
            yield return new Product { Id = 1, Name = "Огурчики", Price = 56 };
            yield return new Product { Id = 1, Name = "Кабачки", Price = 208 };
            yield return new Product { Id = 1, Name = "Премиум подписка", Price = 15000 };
        }
    }

    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        
    }
}
