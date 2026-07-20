using System.IO;
using Xunit;

namespace Edi.Dictionaries.Tests
{
    public class DeploymentTests
    {
        [Fact]
        public void DictionaryFiles_AreCopiedToOutputDirectory()
        {
            var assemblyPath = typeof(JsonEdiDictionary).Assembly.Location;
            var dir = Path.GetDirectoryName(assemblyPath) ?? ".";
            var dataDir = Path.Combine(dir, "Data");

            Assert.True(Directory.Exists(dataDir), $"Data directory should exist at {dataDir}");
            Assert.True(File.Exists(Path.Combine(dataDir, "edifact_D96A.json")), "edifact_D96A.json should be copied to output");
        }
    }
}
