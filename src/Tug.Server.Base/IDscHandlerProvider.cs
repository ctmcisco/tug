// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tug.Ext;
using Tug.Ext.Util;
using Tug.Server.Configuration;

namespace Tug.Server
{
    public interface IDscHandlerProvider : IProvider<IDscHandler>
    { }

    public class DscHandlerManager
        : ProviderManagerBase<IDscHandlerProvider, IDscHandler>
    {
        public DscHandlerManager(
                ILogger<DscHandlerManager> logger,
                ILogger<ServiceProviderExportDescriptorProvider> spLogger,
                IOptions<HandlerSettings> settings,
                IServiceProvider sp)
            : base(logger, new ServiceProviderExportDescriptorProvider(spLogger, sp))
        {
            var extAssms = settings.Value?.Ext?.SearchAssemblies;
            var extPaths = settings.Value?.Ext?.SearchPaths;

            // Add assemblies to search context
            if ((settings.Value?.Ext?.ReplaceExtAssemblies).GetValueOrDefault())
                ClearSearchAssemblies();
            if (extAssms?.Length > 0)
            {
                logger.LogInformation("Adding Search Assemblies");
                AddSearchAssemblies(
                    extAssms.Select(x =>
                    {
                        var an = GetAssemblyName(x);
                        if (an == null)
                            throw new ArgumentException("invalid assembly name");
                        return an;
                    }).Select(x =>
                    {
                        var asm = GetAssembly(x);
                        if (asm == null)
                            throw new InvalidOperationException("unable to resolve assembly from name");
                        
                        if (logger.IsEnabled(LogLevel.Debug))
                            logger.LogDebug($"  * [{x.FullName}]");

                        return asm;
                    }));
            }

            // Add dir paths to search context
            if ((settings.Value?.Ext?.ReplaceExtPaths).GetValueOrDefault())
                ClearSearchPaths();
            if (extPaths?.Length > 0)
            {
                logger.LogInformation("Adding Search Paths");
                AddSearchPath(extPaths.Select(x =>
                {
                    var y = Path.GetFullPath(x);
                    if (logger.IsEnabled(LogLevel.Debug))
                        logger.LogDebug($"  * [{y}]");
                    return y;
                }));
            }

            base.Init();
        }

        protected override void Init()
        {
            // Skipping the initialization till
            // after constructor parameters are applied
        }
    }
}