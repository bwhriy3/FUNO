using Funo.Web.Components;
using Funo.Web.Data;
using Funo.Web.Hubs;
using Funo.Web.Localization;
using Funo.Web.Rooms;
using Funo.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Her tarayici baglantisi kendi tek kisilik oyun oturumunu tasir.
builder.Services.AddScoped<GameSession>();

// Her oyuncu kendi arayuz dilini secer.
builder.Services.AddScoped<LanguageState>();

// Mac gecmisi ve lider tablosu icin SQLite. Oyuncular isimle tanimlanir,
// hesap sistemi yok.
string dbPath = Path.Combine(builder.Environment.ContentRootPath, "funo.db");
builder.Services.AddDbContextFactory<FunoDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddSingleton<MatchRecorder>();

// Cok oyunculu odalar tum uygulama boyunca yasar.
builder.Services.AddSingleton<RoomManager>();
builder.Services.AddSignalR();

// Bosta kalan odalari periyodik olarak temizler.
builder.Services.AddHostedService<RoomCleanupService>();

var app = builder.Build();

// Veritabanini (yoksa) olustur.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FunoDbContext>();
    db.Database.Migrate();
}

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
