using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace FoxMakerAPI
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        private IHostingEnvironment currentEnvironment { get; set; }
        private string OAuth = "";//是否需啟用OAuth2.0驗證
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            OAuth = Configuration["OAuth"];
        }
        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //JSON大小寫
            services.AddMvc().AddJsonOptions(
                op => op.SerializerSettings.ContractResolver =
                new Newtonsoft.Json.Serialization.DefaultContractResolver());
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddHttpClient("4BOrders", c => { c.BaseAddress = new Uri(Configuration["appSettings:EPI_API_T"]); })
           .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
           {
               AllowAutoRedirect = false,
               ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
               SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
           });
            services.AddCors(options =>

            {

                options.AddPolicy("any", builder =>

                {

                    builder.AllowAnyOrigin() //允许任何来源的主机访问

                    .AllowAnyMethod()

                    .AllowAnyHeader()

                    .AllowCredentials();//指定处理cookie

                });

            });
            services.AddAuthentication(
        options =>
        {

        }).AddCookie(opts =>
        {
            opts.Cookie.HttpOnly = false;
        });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Version = "v1",
                        Title = "FoxMaker Api",
                        Description = "ASP.NET Core Web API",
                        //Contact = new Contact
                        //{
                        //    Email = "nsd-b2b-web@mail.foxconn.com",
                        //},
                    });
                // Set the comments path for the Swagger JSON and UI.
                //var xmlFileUI = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPathUI = Path.Combine(AppContext.BaseDirectory, xmlFileUI);
                //c.IncludeXmlComments(xmlPathUI);

                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "FoxMakerAPI.xml");
                //var xmlModelPath = Path.Combine(basePath, "MODEL.xml");
                c.IncludeXmlComments(xmlPath, true);
                //c.IncludeXmlComments(xmlModelPath);
                //#region Token绑定到ConfigureServices
                ////添加Header验证信息
                var security = new Dictionary<string, IEnumerable<string>> { { "FoxMakerAPI", new string[] { } }, };
                c.AddSecurityRequirement(security);
                //方案名称“HRSystemMicroApi”可自定义，上下一致即可
                c.AddSecurityDefinition("FoxMakerAPI", new ApiKeyScheme
                {
                    Description = "直接在下框中输入token",
                    Name = "Authorization",
                    In = "Header",//存放token位置
                    Type = "apiKey"
                });

                //var securityCSP = new Dictionary<string, IEnumerable<string>> { { "Content-Security-Policy", new string[] { "style-src https:; img-src 'self'; frame-src 'none'; script-src 'self';" } }, };
                //c.AddSecurityRequirement(securityCSP);
                //var securityXTO = new Dictionary<string, IEnumerable<string>> { { "X-Content-Type-Options", new string[] { "nosniff" } }, };
                //c.AddSecurityRequirement(securityXTO);
                //#endregion
            });

            //// #region Token服务注册
            // services.AddAuthorization(options =>
            // {
            //     //註冊權限管理，可以自定義多個，
            //     //這裡是驗證token之後的返回值
            //     options.AddPolicy("HR61App", policy => policy.RequireClaim("HR61AppType").Build());
            // });
            //JWT
            services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {

                    // 一般我們都會驗證 Issuer
                    ValidateIssuer = true,
                    // 若是單一伺服器通常不太需要驗證 Audience
                    ValidateAudience = true,
                    // 一般我們都會驗證 Token 的有效期間
                    ValidateLifetime = true,
                    //AudienceValidator = (m, n, z) =>
                    //{
                    //    return m != null && m.FirstOrDefault().Equals("");
                    //},

                    ClockSkew = TimeSpan.FromMinutes(Convert.ToInt32(Configuration["Jwt:Expires"])),
                    // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
                    ValidateIssuerSigningKey = true,
                    ValidAudience = Configuration["Jwt:Audience"],
                    ValidIssuer = Configuration["Jwt:Issuer"], // 從 IConfiguration 取得
                    //應該從 IConfiguration 取得
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:signKey"]))
                };
                //options.Events = new JwtBearerEvents
                //{
                //    //此处为权限验证失败后触发的事件
                //    OnChallenge = context =>
                //    {
                //        //此处代码为终止.Net Core默认的返回类型和数据结果，这个很重要哦，必须
                //        context.HandleResponse();

                //        //自定义自己想要返回的数据结果，我这里要返回的是Json对象，通过引用Newtonsoft.Json库进行转换
                //        var payload = JsonConvert.SerializeObject(new { Code = "401", Message = "很抱歉，您无权访问该接口" });
                //        //自定义返回的数据类型
                //        context.Response.ContentType = "application/json";
                //        //自定义返回状态码，默认为401 我这里改成 200
                //        context.Response.StatusCode = StatusCodes.Status200OK;
                //        //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //        //输出Json数据结果
                //        context.Response.WriteAsync(payload);
                //        return Task.FromResult(0);
                //    }
                //};
            });
            //services.AddHttpsRedirection(options =>
            //{
            //    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
            //    options.HttpsPort = 5443;
            //});
            //IP使用
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //使用Autofac實現IOC
            var containerBuilder = new ContainerBuilder();
            //模塊化注入
            containerBuilder.RegisterModule<HelperTool.AutofacModuleRegister>();
            containerBuilder.Populate(services);
            var container = containerBuilder.Build();

            return new AutofacServiceProvider(container);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            currentEnvironment = env;
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //允许跨域设置
            //app.UseCors("AllRequests");
            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FoxMakerAPI API V1");
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add(
                    "Content-Security-Policy",
                    "style-src https:; img-src 'self'; frame-src 'none'; script-src 'self';"
                );
                context.Response.Headers.Add(
                   "X-Frame-Options",
                   "SAMEORIGIN"
               );
                await next();
            });
            //啟用OAuth
            if (OAuth == "Yes")
            {
                app.UseMiddleware<OAuth.TokenAuth>();
            }
            app.UseMvc();
        }
    }
}
