using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Fabric.Description;

namespace ChatRoomsWeb
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class ChatRoomsWeb : StatelessService
    {
        private IConfiguration _configuration;

        private static readonly object metricsLock = new object();
        private static int requestCountLastPeriod = 0;

        public ChatRoomsWeb(StatelessServiceContext context, IConfiguration configuration)
            : base(context)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);

                        builder.Services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        })
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters()
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = _configuration["Jwt:Issuer"],
                                ValidAudience = _configuration["Jwt:Audience"],
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]))
                            };
                        });

                        builder.Services.AddAuthorization();
                        builder.Services.AddControllers();
                        builder.Services.AddEndpointsApiExplorer();

                        builder.Services.AddSwaggerGen(c =>
                        {
                            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatRoomsWeb", Version = "v1" });
                            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                            {
                                Name = "Authorization",
                                Scheme = "Bearer",
                                BearerFormat = "JWT",
                                In = ParameterLocation.Header,
                                Description = "\"JWT Authorization header using the Bearer scheme. \\r\\n\\r\\n Enter 'Bearer' [space] and then your token in the text input below.\\r\\n\\r\\nExample: \\\"Bearer 1safsfsdfdfd\\\"\""
                            });
                            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                            {
                                {
                                    new OpenApiSecurityScheme
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.SecurityScheme,
                                            Id = "Bearer"
                                        }
                                    }, new string[] { }
                                }
                            });
                        });

                        var app = builder.Build();
                        if (app.Environment.IsDevelopment())
                        {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                        }

                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.MapControllers();
                        app.UseCors(builder => builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader().AllowCredentials());

                        return app;

                    }))
            };
        }

        public static void RegisterRequestForMetrics()
        {
            lock (metricsLock)
            {
                requestCountLastPeriod++;
            }
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await DefineMetricsAndPolicies();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                lock (metricsLock)
                {
                    Partition.ReportLoad(new List<LoadMetric> { new LoadMetric("TotalRequestsPerPeriod", requestCountLastPeriod) });
                    requestCountLastPeriod = 0;
                }

                await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken);
            }
        }

        private async Task DefineMetricsAndPolicies()
        {
            FabricClient fabricClient = new FabricClient();
            StatelessServiceUpdateDescription updateDescription = new();

            RegisterMetrics(updateDescription);
            RegisterScaling(updateDescription);

            await fabricClient.ServiceManager.UpdateServiceAsync(Context.ServiceName, updateDescription);
        }

        private void RegisterMetrics(StatelessServiceUpdateDescription updateDescription)
        {
            var requestsPerSecondMetric = new StatelessServiceLoadMetricDescription
            {
                Name = "TotalRequestsPerPeriod",
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };

            updateDescription.Metrics ??= new MetricsCollection();
            updateDescription.Metrics.Add(requestsPerSecondMetric);
        }

        private void RegisterScaling(StatelessServiceUpdateDescription updateDescription)
        {
            PartitionInstanceCountScaleMechanism partitionInstanceCountScaleMechanism = new PartitionInstanceCountScaleMechanism
            {
                MinInstanceCount = 2,
                MaxInstanceCount = 4,
                ScaleIncrement = 1
            };

            AveragePartitionLoadScalingTrigger averagePartitionLoadScalingTrigger = new AveragePartitionLoadScalingTrigger
            {
                MetricName = "TotalRequestsPerPeriod",
                LowerLoadThreshold = 14.0,
                UpperLoadThreshold = 44.0,
                ScaleInterval = TimeSpan.FromMinutes(4)
            };

            ScalingPolicyDescription scalingPolicyDescription = new ScalingPolicyDescription(partitionInstanceCountScaleMechanism, averagePartitionLoadScalingTrigger);

            updateDescription.ScalingPolicies ??= new List<ScalingPolicyDescription>();
            updateDescription.ScalingPolicies.Add(scalingPolicyDescription);
        }
    }
}
