﻿using Meowv.Blog.BackgroundJobs;
using Meowv.Blog.Domain.Configurations;
using Meowv.Blog.EntityFrameworkCore;
using Meowv.Blog.HttpApi.Hosting.Middleware;
using Meowv.Blog.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Meowv.Blog.HttpApi.Hosting
{
    [DependsOn(typeof(AbpAspNetCoreMvcModule),
        typeof(MeowvBlogHttpApiModule),
        typeof(MeowvBlogSwaggerModule),
        typeof(AbpAutofacModule),
       typeof(MeowvBlogFrameworkCoreModule),
        typeof(MeowvBlogBackgroundJobsModule)
        )]
    public class MeowvBlogHttpApiHostingModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                        ValidAudience = AppSettings.JWT.Domain,
                        ValidIssuer = AppSettings.JWT.Domain,
                        IssuerSigningKey = new SymmetricSecurityKey(AppSettings.JWT.SecurityKey.GetBytes())
                    };

                });
            //认证授权
            context.Services.AddAuthorization();
            //http请求
            context.Services.AddHttpClient();

            context.Services.AddRouting(options =>
            {
                //设置URL为小写
                options.LowercaseUrls = true;
                //在生成的URL后面加斜杠
                options.AppendTrailingSlash = true;
            });

        }
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            // 环境变量，开发环境
            if (env.IsDevelopment())
            {
                //生成异常页面
                app.UseDeveloperExceptionPage();
            }

            // 使用路由
            app.UseRouting();

            // 路由映射
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //身份验证
            app.UseAuthentication();

            //认证授权
            app.UseAuthorization();

            //异常处理中间件
            app.UseMiddleware<ExceptionHandlerMiddleware>();

            // 使用HSTS中间件
            app.UseHsts();

            // 使用默认的跨域设置
            app.UseCors();

            // HTTP请求转HTTPS
            app.UseHttpsRedirection();

            // 转发将标头代理到当前请求，配合 Nginx 使用，获取用户真实IP
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders=ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }
    }
}
