using UnityEditor.IMGUI.Controls;

namespace Editor.Assemblies
{
    /// <summary>
    ///     Represents a specialized TreeView item for assemblies,
    ///     including a property to track whether each assembly is enabled or not.
    /// </summary>
    internal class AssembliesTreeViewItem : TreeViewItem
    {
        /// <summary>
        ///     Indicates whether the assembly represented by this <see cref="AssembliesTreeViewItem" />
        ///     is currently enabled, typically used for filtering or inclusion logic within the tree view.
        /// </summary>
        public bool Enabled { get; set; }
    }
}