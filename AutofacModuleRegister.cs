using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FoxMakerAPI.HelperTool
{
    /// <summary>
    /// 
    /// </summary>
    public class AutofacModuleRegister : Autofac.Module
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            //注入單個服務
            // builder.RegisterType<Services.Common.Login.apiLoginService>().As<IServices.ICommon.ILogin.IApiLoginService>();
            //  builder.RegisterType<Services.Common.DBHelper.CommonOracleDBService>().As<IServices.ICommon.IDBHelper.ICommonOracleDBService>();

            //動態註入服務
            builder.RegisterAssemblyTypes(Assembly.Load("IServices"), Assembly.Load("Services"))
              .Where(t => t.Name.EndsWith("Service"))
              .AsImplementedInterfaces();
            //   .EnableClassInterceptors();
            // .InterceptedBy(typeof(AOPTest));
            // var aopt = new AOPTest();
            //  builder.Register(x => aopt);

        }
    }
}
