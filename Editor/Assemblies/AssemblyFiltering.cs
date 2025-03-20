/*
 * Taken from com.unity.testtools.codecoverage v. 1.2.6
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;

namespace Editor.Assemblies
{
    /// <summary>
    ///     Provides functionality for filtering assemblies based on inclusion and exclusion criteria.
    /// </summary>
    /// <remarks>
    ///     This class supports operations to include or exclude assemblies using glob-style patterns
    ///     and provides methods to retrieve and parse assembly information. It is designed for use
    ///     in workflows related to assembly management and filtering within the Unity Editor context.
    /// </remarks>
    internal class AssemblyFiltering
    {
        private static readonly HashSet<char> regexSpecialChars =
            new(new[] { '[', '\\', '^', '$', '.', '|', '?', '*', '+', '(', ')' });

        private Regex[] m_ExcludeAssemblies;

        private Regex[] m_IncludeAssemblies;

        /// <summary>
        ///     Represents a utility class for filtering assemblies based on specified inclusion and exclusion patterns.
        /// </summary>
        /// <remarks>
        ///     This class provides methods to parse and manage assembly filters and determine inclusion or exclusion of specific
        ///     assemblies.
        ///     It is useful in scenarios involving filtering of project assemblies in the Unity Editor environment.
        /// </remarks>
        public AssemblyFiltering()
        {
            m_IncludeAssemblies = new Regex[] { };
            m_ExcludeAssemblies = new Regex[] { };
        }

        /// <summary>
        ///     Gets the string of included assemblies used in the assembly filtering process.
        /// </summary>
        /// <remarks>
        ///     This property holds a comma-separated list of assembly names or patterns that are explicitly marked for inclusion.
        ///     The patterns are used to determine which assemblies should pass the inclusion filter during operations like loading
        ///     or filtering project assemblies.
        /// </remarks>
        public string includedAssemblies { get; private set; }

        /// <summary>
        ///     Gets the string of excluded assemblies used in the assembly filtering process.
        /// </summary>
        /// <remarks>
        ///     This property holds a comma-separated list of assembly names or patterns explicitly marked for exclusion.
        ///     The patterns are applied to determine which assemblies should be omitted during operations such as filtering
        ///     or loading project assemblies.
        /// </remarks>
        public string excludedAssemblies { get; private set; }

        /// <summary>
        ///     Retrieves all project assemblies filtered by glob-style patterns for inclusion and exclusion.
        /// </summary>
        /// <returns>
        ///     An array of assemblies that match the specified inclusion and exclusion patterns.
        /// </returns>
        /// <remarks>
        ///     This method collects all assemblies loaded in the current application domain,
        ///     applies sorting for deterministic results, and filters assemblies based on predefined glob-style rules.
        ///     It is intended for internal usage in tools or environments where precise assembly filtering is necessary, such as
        ///     in Unity Editor workflows.
        /// </remarks>
        public static Assembly[] GetAllProjectAssembliesInternal()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Array.Sort(assemblies, (x, y) => string.Compare(x.GetName().Name, y.GetName().Name));

            var allAssemblyFiltersString = GetAllProjectAssembliesString() + ",unityeditor*,unityengine*,unity.*";
            var allAssemblyFilters =
                allAssemblyFiltersString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var assembliesRegex = allAssemblyFilters
                .Select(f => CreateFilterRegex(f))
                .ToArray();

            var filteredAssemblies = assemblies.Where(assembly =>
                assembliesRegex.Any(regex => regex.IsMatch(assembly.GetName().Name.ToLowerInvariant()))).ToArray();

            return filteredAssemblies;
        }

        /// <summary>
        ///     Retrieves all assemblies included in the current Unity project.
        /// </summary>
        /// <remarks>
        ///     This method fetches and returns an array of assemblies associated with the current Unity project,
        ///     sorted alphabetically by their names. It is useful for obtaining a comprehensive list of project
        ///     assemblies for further analysis or processing within the Unity Editor environment.
        /// </remarks>
        /// <returns>An array of Assembly objects representing all project assemblies.</returns>
        public static UnityEditor.Compilation.Assembly[] GetAllProjectAssemblies()
        {
            var assemblies = CompilationPipeline.GetAssemblies();
            Array.Sort(assemblies, (x, y) => string.Compare(x.name, y.name));
            return assemblies;
        }

        /// <summary>
        ///     Retrieves a comma-separated string representation of all project assemblies' names.
        /// </summary>
        /// <remarks>
        ///     This method gathers all project assemblies, sorts them by name, and formats their names into a single string
        ///     for further usage, such as filtering or processing assembly-related data within the Unity Editor context.
        /// </remarks>
        /// <returns>
        ///     A string containing the names of all project assemblies, separated by commas.
        /// </returns>
        public static string GetAllProjectAssembliesString()
        {
            var assemblies = GetAllProjectAssemblies();

            var assembliesString = "";
            var assembliesLength = assemblies.Length;
            for (var i = 0; i < assembliesLength; ++i)
            {
                assembliesString += assemblies[i].name;
                if (i < assembliesLength - 1)
                    assembliesString += ",";
            }

            return assembliesString;
        }

        /// <summary>
        ///     Retrieves a string representation of assemblies that are specifically part of the user's project assets.
        /// </summary>
        /// <remarks>
        ///     This method filters assemblies based on a specified prefix ("Assets") and constructs a string containing
        ///     the names of those assemblies. It is primarily used to identify assemblies directly related to project-specific
        ///     content within the Unity Editor.
        /// </remarks>
        /// <returns>
        ///     A string containing the names of assemblies that belong exclusively to the user's project assets.
        /// </returns>
        public static string GetUserOnlyAssembliesString()
        {
            return GetStartsWithAssembliesString("Assets");
        }

        /// <summary>
        ///     Retrieves a concatenated string of assembly names that are located within the "Packages" directory.
        /// </summary>
        /// <remarks>
        ///     This method filters assemblies based on their location, specifically targeting those
        ///     that originate from the "Packages" directory within a Unity project. It is particularly
        ///     useful for assembly inclusion processes that require segmentation by source.
        /// </remarks>
        /// <returns>
        ///     A string containing the names of assemblies found in the "Packages" directory, separated by a delimiter.
        /// </returns>
        public static string GetPackagesOnlyAssembliesString()
        {
            return GetStartsWithAssembliesString("Packages");
        }

        /// <summary>
        ///     Retrieves a comma-separated string of assembly names whose source files start with the specified string.
        /// </summary>
        /// <param name="startsWithStr">The prefix string to match against the source files of the assemblies.</param>
        /// <returns>A comma-separated string of matching assembly names. Returns an empty string if no matches are found.</returns>
        private static string GetStartsWithAssembliesString(string startsWithStr)
        {
            var assemblies = GetAllProjectAssemblies();
            var foundAssemblies = new List<string>();

            var assembliesString = "";
            var assembliesLength = assemblies.Length;
            int i;
            for (i = 0; i < assembliesLength; ++i)
            {
                var name = assemblies[i].name;
                var sourceFiles = assemblies[i].sourceFiles;

                if (sourceFiles.Length > 0 &&
                    sourceFiles[0].StartsWith(startsWithStr, StringComparison.InvariantCultureIgnoreCase))
                    foundAssemblies.Add(name);
            }

            var foundAssembliesLength = foundAssemblies.Count;
            for (i = 0; i < foundAssembliesLength; ++i)
            {
                assembliesString += foundAssemblies[i];
                if (i < foundAssembliesLength - 1)
                    assembliesString += ",";
            }

            return assembliesString;
        }

        /// <summary>
        ///     Converts a filter string into a compiled regular expression for matching assembly names.
        /// </summary>
        /// <param name="filter">The filter string, typically in glob format, to be converted into a regular expression.</param>
        /// <returns>A compiled <see cref="Regex" /> object representing the equivalent of the provided filter.</returns>
        public static Regex CreateFilterRegex(string filter)
        {
            filter = filter.ToLowerInvariant();

            return new Regex(GlobToRegex(filter), RegexOptions.Compiled);
        }

        /// <summary>
        ///     Converts a glob pattern into a regular expression string that can be used for pattern matching.
        /// </summary>
        /// <param name="glob">The glob pattern to convert. Glob patterns may include wildcards such as '*' and '?'.</param>
        /// <param name="startEndConstrains">
        ///     A boolean indicating whether the resulting regular expression should constrain the pattern to match from the start
        ///     to the end of the input string.
        ///     If true, the pattern will be anchored with '^' at the beginning and '$' at the end.
        /// </param>
        /// <returns>
        ///     A string representing the equivalent regular expression that matches the input glob pattern.
        /// </returns>
        public static string GlobToRegex(string glob, bool startEndConstrains = true)
        {
            var regex = new StringBuilder();
            var characterClass = false;
            var prevChar = char.MinValue;

            if (startEndConstrains)
                regex.Append("^");
            foreach (var c in glob)
            {
                if (characterClass)
                {
                    if (c == ']') characterClass = false;
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
                        if (regexSpecialChars.Contains(c)) regex.Append('\\');
                        regex.Append(c);
                        break;
                }

                prevChar = c;
            }

            if (startEndConstrains)
                regex.Append("$");
            return regex.ToString();
        }
    }
}