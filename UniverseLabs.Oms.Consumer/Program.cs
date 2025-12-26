using Microsoft.Extensions.Hosting;
using UniverseLabs.Oms.Consumer.Clients;
using UniverseLabs.Oms.Consumer.Config;
using UniverseLabs.Oms.Consumer.Consumers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection(nameof(KafkaSettings)));
builder.Services.Configure<HostOptions>(options =>
{
    options.ServicesStartConcurrently = true;
    options.ServicesStopConcurrently = true;
});
builder.Services.AddHostedService<OmsOrderCreatedConsumer>();
builder.Services.AddHostedService<OmsOrderStatusChangedConsumer>();
builder.Services.AddHttpClient<OmsClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["HttpClient:Oms:BaseAddress"]));

var app = builder.Build();
await app.RunAsync();
