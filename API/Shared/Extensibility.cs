﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Configuration;

namespace yieldtome
{
    public class Extensibility
    {
        public static bool IsContainerRunning { get; set; }
        public static CompositionContainer Container { get; set; }

        public static void StartContainer()
        {
            // Load MEF Catalog with Modules
            AggregateCatalog catalog = new AggregateCatalog();
            List<string> modulePaths = new List<string>();
            try
            {
                string modulePathString = ConfigurationManager.AppSettings["ModulePaths"];
                modulePaths = modulePathString.Split(';').ToList();
            }
            catch { }
            foreach(string path in modulePaths) catalog.Catalogs.Add(new DirectoryCatalog(path));
            
            // Filter which contracts should be loaded
            List<string> modulesToInclude = new List<string>();
            try
            {
                string filterString = ConfigurationManager.AppSettings["ModulesToInclude"];
                modulesToInclude = filterString.Split(';').ToList();
            }
            catch { }
            FilteredCatalog filteredCatalog = new FilteredCatalog(catalog, x => modulesToInclude.Contains(x.ToString()) == true);

            // Initialise the container with Module assemblies and this assembly
            Extensibility.Container = new CompositionContainer(filteredCatalog, true);

        }
    }
}
