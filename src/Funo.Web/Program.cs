using Funo.Web.Components;
using Funo.Web.Hubs;
using Funo.Web.Localization;
using Funo.Web.Rooms;
using Funo.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Her tarayici baglantisi kendi tek kisilik oyun oturumunu tasir.
builder.Services.AddScoped<GameSession>();

// Her oyuncu kendi arayuz dilini secer.
builder.Services.AddScoped<LanguageState>();

// Cok oyunculu odalar tum uygulama boyunca yasar.
builder.Services.AddSingleton<RoomManager>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<GameHub>("/gamehub");

app.Run();
