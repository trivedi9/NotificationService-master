/*
 *  File Name : SafeDirectoryCatalog.cs
 *  Author : Rajesh
 *  @ PCC Technology Group LLC
 *  Created Date : 12/10/2010
 */


namespace CooperAtkins.Generic
{
    using System.Linq;
    using System.ComponentModel.Composition.Primitives;
    using System.IO;
    using System.ComponentModel.Composition.Hosting;
    using System.Reflection;

    public class SafeDirectoryCatalog : ComposablePartCatalog
    {
        private readonly AggregateCatalog _catalog;

        public SafeDirectoryCatalog(string directory)
        {
            var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);

            _catalog = new AggregateCatalog();

            foreach (var file in files)
            {
                try
                {
                    var asmCat = new AssemblyCatalog(file);

                    if (asmCat.Parts.ToList().Count > 0)
                        _catalog.Catalogs.Add(asmCat);
                }
                catch (ReflectionTypeLoadException)
                {
                    
                }
            }
        }
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return _catalog.Parts; }
        }
    }
}
