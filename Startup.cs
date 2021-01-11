using API.Data;
using API.Utilities;
using Imageflow.Server;
using Imageflow.Server.HybridCache;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.IO;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            HomeFolder = (Environment.OSVersion.Platform == PlatformID.Unix ||
                   Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }

        public IConfiguration Configuration { get; }
        public string HomeFolder { get; }

        public void ConfigureEntityFramework(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("OmgImagesServerDb");
            services.AddDbContextPool<OmgImageServerDbContext>(
                options => options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 22)),
                    mysqlOptions =>
                    {
                        mysqlOptions.CharSetBehavior(CharSetBehavior.NeverAppend);
                        mysqlOptions.UseNewtonsoftJson();
                        mysqlOptions.EnableRetryOnFailure();
                    }
            ));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureEntityFramework(services);

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin()
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                                  });
            });

            services.AddControllers();
            // You can add a hybrid cache (in-memory persisted database for tracking filenames, but files used for bytes)
            // But remember to call ImageflowMiddlewareOptions.SetAllowCaching(true) for it to take effect
            // If you're deploying to azure, provide a disk cache folder *not* inside ContentRootPath
            // to prevent the app from recycling whenever folders are created.
            services.AddImageflowHybridCache(
                new HybridCacheOptions(Path.Combine(HomeFolder, "omgimgflow_cache"))
                {
                    // How long after a file is created before it can be deleted
                    MinAgeToDelete = TimeSpan.FromSeconds(10),
                    // How much RAM to use for the write queue before switching to synchronous writes
                    QueueSizeLimitInBytes = 100 * 1000 * 1000,
                    // The maximum size of the cache (1GB)
                    CacheSizeLimitInBytes = 1024 * 1024 * 1024,
                });

            var physicalProvider = new PhysicalFileProvider($"{HomeFolder}");

            services.AddSingleton<IFileProvider>(physicalProvider);

            services.AddSingleton<IDateTimeManager, DateTimeManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else 
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseImageflow(new ImageflowMiddlewareOptions()
                // Maps / to WebRootPath
                .SetMapWebRoot(false)
                //.SetMyOpenSourceProjectUrl("https://github.com/imazen/imageflow-dotnet-server")
                // Maps /folder to WebRootPath/folder
                .MapPath("/photos", Path.Combine(HomeFolder, "omgimgflow_photos"))
                // Allow HybridCache or other registered IStreamCache to run
                .SetAllowCaching(true)
                // Cache publicly (including on shared proxies and CDNs) for 30 days
                .SetDefaultCacheControlString("public, max-age=2592000")
                .HandleExtensionlessRequestsUnder("/omgimgflow_photos/", StringComparison.OrdinalIgnoreCase)
                .AddCommandDefault("down.filter", "mitchell")
                .AddCommandDefault("f.sharpen", "15")
                .AddCommandDefault("webp.quality", "80")
                .AddCommandDefault("ignore_icc_errors", "true")
                //When set to true, this only allows ?preset=value URLs, returning 403 if you try to use any other commands. 
                .SetUsePresetsExclusively(false)
                .AddPreset(new PresetOptions("large", PresetPriority.DefaultValues)
                    .SetCommand("width", "1024")
                    .SetCommand("height", "1024")
                    .SetCommand("mode", "max")));

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors("AllowAll");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
