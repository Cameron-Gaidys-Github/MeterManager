using MeterManager.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddDbContext<SBCorpInetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SBCorpInetDb")));

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enables serving files from wwwroot

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages(); // This is what enables /Api/ActiveFlow

app.Run();
