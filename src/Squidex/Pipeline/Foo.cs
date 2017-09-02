using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;

namespace Squidex.Pipeline
{
    public class ReferencesMetadataReferenceFeatureProvider : IApplicationFeatureProvider<MetadataReferenceFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, MetadataReferenceFeature feature)
        {
            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var providerPart in parts.OfType<ICompilationReferencesProvider>())
            {
                try
                {
                    var referencePaths = providerPart.GetReferencePaths();
                    foreach (var path in referencePaths)
                    {
                        if (libraryPaths.Add(path))
                        {
                            var metadataReference = CreateMetadataReference(path);
                            feature.MetadataReferences.Add(metadataReference);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("FOO");
                }
            }
        }

        private static MetadataReference CreateMetadataReference(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

                return assemblyMetadata.GetReference(filePath: path);
            }
        }
    }
}
