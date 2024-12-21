/*
 * Taken from com.unity.testtools.codecoverage v. 1.2.6
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;

namespace Editor.Assemblies
{
    internal class AssemblyFiltering
    {
        public const string kCoreAssemblies = "unityeditor*,unityengine*";
        public const string kAssetsAlias = "<assets>";
        public const string kPackagesAlias = "<packages>";
        public const string kAllAlias = "<all>";
        public const string kCoreAlias = "<core>";

        public string includedAssemblies
        {
            get;
            private set;
        }

        public string excludedAssemblies
        {
            get;
            private set;
        }

        private Regex[] m_IncludeAssemblies;
        private Regex[] m_ExcludeAssemblies;
        private static HashSet<char> regexSpecialChars = new HashSet<char>(new[] { '[', '\\', '^', '$', '.', '|', '?', '*', '+', '(', ')' });

        public AssemblyFiltering()
        {
            m_IncludeAssemblies = new Regex[] { };
            m_ExcludeAssemblies = new Regex[] { };
        }

        public void Parse(string includeAssemblies, string excludeAssemblies)
        {
            includedAssemblies = includeAssemblies;
            excludedAssemblies = excludeAssemblies;

            string[] includeAssemblyFilters = includeAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            string[] excludeAssemblyFilters = excludeAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            
            //CoverageAnalytics.instance.CurrentCoverageEvent.includedAssemblies = includeAssemblyFilters;
            //CoverageAnalytics.instance.CurrentCoverageEvent.excludedAssemblies = excludeAssemblyFilters;

            m_IncludeAssemblies = includeAssemblyFilters
                .Select(f => CreateFilterRegex(f))
                .ToArray();

            m_ExcludeAssemblies = excludeAssemblyFilters
                .Select(f => CreateFilterRegex(f))
                .ToArray();
        }

        public bool IsAssemblyIncluded(string name)
        {
            name = name.ToLowerInvariant();

            if (m_ExcludeAssemblies.Any(f => f.IsMatch(name)))
            {
                return false;
            }
            else
            {
                return m_IncludeAssemblies.Any(f => f.IsMatch(name));
            }
        }

        public static System.Reflection.Assembly[] GetAllProjectAssembliesInternal()
        {
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Array.Sort(assemblies, (x, y) => String.Compare(x.GetName().Name, y.GetName().Name));

            string allAssemblyFiltersString = GetAllProjectAssembliesString() + ",unityeditor*,unityengine*,unity.*";
            string[] allAssemblyFilters = allAssemblyFiltersString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            Regex[] assembliesRegex = allAssemblyFilters
            .Select(f => CreateFilterRegex(f))
            .ToArray();

            System.Reflection.Assembly[] filteredAssemblies = assemblies.Where(assembly => assembliesRegex.Any(regex => regex.IsMatch(assembly.GetName().Name.ToLowerInvariant()))).ToArray();

            return filteredAssemblies;
        }

        public static Assembly[] GetAllProjectAssemblies()
        {
            Assembly[] assemblies = CompilationPipeline.GetAssemblies();
            Array.Sort(assemblies, (x, y) => String.Compare(x.name, y.name));
            return assemblies;
        }

        public static string GetAllProjectAssembliesString()
        {
            Assembly[] assemblies = GetAllProjectAssemblies();

            string assembliesString = "";
            int assembliesLength = assemblies.Length;
            for (int i=0; i<assembliesLength; ++i)
            {
                assembliesString += assemblies[i].name;
                if (i < assembliesLength - 1)
                    assembliesString += ",";
            }

            return assembliesString;
        }

        public static string GetUserOnlyAssembliesString()
        {
            return GetStartsWithAssembliesString("Assets");
        }

        public static string GetPackagesOnlyAssembliesString()
        {
            return GetStartsWithAssembliesString("Packages");
        }

        private static string GetStartsWithAssembliesString(string startsWithStr)
        {
            Assembly[] assemblies = GetAllProjectAssemblies();
            List<string> foundAssemblies = new List<string>();

            string assembliesString = "";
            int assembliesLength = assemblies.Length;
            int i;
            for (i = 0; i < assembliesLength; ++i)
            {
                string name = assemblies[i].name;
                string[] sourceFiles = assemblies[i].sourceFiles;

                if (sourceFiles.Length > 0 &&
                    sourceFiles[0].StartsWith(startsWithStr, StringComparison.InvariantCultureIgnoreCase))
                {
                    foundAssemblies.Add(name);
                }
            }

            int foundAssembliesLength = foundAssemblies.Count;
            for (i = 0; i < foundAssembliesLength; ++i)
            {
                assembliesString += foundAssemblies[i];
                if (i < foundAssembliesLength - 1)
                    assembliesString += ",";
            }

            return assembliesString;
        }

        public static Regex CreateFilterRegex(string filter)
        {
            filter = filter.ToLowerInvariant();

            return new Regex(GlobToRegex(filter), RegexOptions.Compiled);
        }

        public static string GlobToRegex(string glob, bool startEndConstrains = true)
        {
            var regex = new StringBuilder();
            var characterClass = false;
            char prevChar = Char.MinValue;

            if (startEndConstrains)
                regex.Append("^");
            foreach (var c in glob)
            {
                if (characterClass)
                {
                    if (c == ']')
                    {
                        characterClass = false;
                    }
                    regex.Append(c);
                    continue;
                }
                switch (c)
                {
                    case '*':
                        if (prevChar == '*')
                            regex.Append(".*"); //if it's double * pattern then don't stop at folder separator
                        else
                            regex.Append("[^\\n\\r/]*"); //else match everything except folder separator (and new line)
                        break;
                    case '?':
                        regex.Append("[^\\n\\r/]");
                        break;
                    case '[':
                        characterClass = true;
                        regex.Append(c);
                        break;
                    default:
                        if (regexSpecialChars.Contains(c))
                        {
                            regex.Append('\\');
                        }
                        regex.Append(c);
                        break;
                }

                prevChar = c;
            }
            if (startEndConstrains)
                regex.Append("$");
            return regex.ToString();
        }

        public static string RemoveAssembliesThatNoLongerExist(string assembliesString)
        {
            IEnumerable<string> currentAssemblies;

            bool developerMode = EditorPrefs.GetBool("DeveloperMode", false);
            if (developerMode)
            {
                currentAssemblies = GetAllProjectAssembliesInternal().Select(x => x.GetName().Name);
            }
            else
            {
                currentAssemblies = GetAllProjectAssemblies().Select(x => x.name);
            }

            string[] assemblyNames = assembliesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<string> filteredAssemblyNames = assemblyNames.Where(x => currentAssemblies.Contains(x));

            return string.Join(",", filteredAssemblyNames);
        }
    }
}
