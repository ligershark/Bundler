﻿using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace WebOptimizer.Test
{
    public class AssetTest
    {
        [Fact2]
        public void AssetCreate_Success()
        {
            string route = "route";
            string contentType = "text/css";
            var sourcefiles = new[] { "file1.css" };

            var asset = new Asset(route, contentType, sourcefiles);

            Assert.Equal(route, asset.Route);
            Assert.Equal(contentType, asset.ContentType);
            Assert.Equal(sourcefiles, asset.SourceFiles);
            Assert.Equal(0, asset.Processors.Count);
        }

        [Fact2]
        public void GenerateCacheKey_Success()
        {
            string route = "route";
            string contentType = "text/css";
            var sourcefiles = new[] { "file1.css" };
            var context = new Mock<HttpContext>().SetupAllProperties();
            var env = new Mock<IHostingEnvironment>();
            var cache = new Mock<IMemoryCache>();
            var fileProvider = new PhysicalFileProvider(Path.GetTempPath());
            var fileVersionProvider = new Mock<IFileVersionProvider>();

            var asset = new Asset(route, contentType, sourcefiles);
            asset.Items.Add("PhysicalFiles", new string[0] );

            StringValues ae = "gzip, deflate";
            context.SetupSequence(c => c.Request.Headers.TryGetValue("Accept-Encoding", out ae))
                   .Returns(false)
                   .Returns(true);

            context.Setup(c => c.RequestServices.GetService(typeof(IHostingEnvironment)))
                   .Returns(env.Object);

            context.Setup(c => c.RequestServices.GetService(typeof(IMemoryCache)))
                   .Returns(cache.Object);

            context.Setup(c => c.RequestServices.GetService(typeof(IFileVersionProvider)))
                .Returns(fileVersionProvider.Object);

            env.Setup(e => e.WebRootFileProvider)
                .Returns(fileProvider);

            // Check non-gzip value
            string key = asset.GenerateCacheKey(context.Object);
            Assert.Equal("_BZuuBNh_zEXnNPIPaO_4Ii4UdM", key);

            // Check gzip value
            string gzipKey = asset.GenerateCacheKey(context.Object);
            Assert.Equal("SvH6WGVAapgMXiPenaOGnKS_oMI", gzipKey);
        }

        [Fact2]
        public void AssetToString()
        {
            var asset = new Asset("/route", "content/type", Enumerable.Empty<string>());

            Assert.Equal(asset.Route, asset.ToString());
        }
    }
}
