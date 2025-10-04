using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityFolderGenerator.Editor
{
    [CreateAssetMenu(fileName = "FolderStructureConfig", menuName = "Unity Tools/Folder Structure Config")]
    public class FolderStructureConfig : ScriptableObject
    {
        [Header("Project Settings")]
        [Tooltip("Default project name that will appear in the folder creation window")]
        public string defaultProjectName = "_PROJECT_NAME";
        [Tooltip("Define the main folders and their subfolders")]
        public List<FolderGroup> folderGroups = new List<FolderGroup>();
        [Tooltip("Additional standalone folders to create")]
        public List<string> standaloneFolders = new List<string>();

        [Header("Additional Options")]
        [Tooltip("Create .gitkeep files in empty folders for version control")]
        public bool createGitKeepFiles = true;

        [SerializeField, HideInInspector]
        private int version;

        public int Version => version;
    
        [System.Serializable]
        public class FolderGroup
        {
            [Tooltip("Name of the main folder")]
            public string mainFolder;
        
            [Tooltip("Subfolders to create inside the main folder")]
            public List<string> subfolders = new List<string>();
        
            [Tooltip("Enable/disable this folder group")]
            public bool enabled = true;
        }
    
        /// <summary>
        /// Returns a dictionary of main folders and their subfolders, filtering out disabled or empty entries.
        /// </summary>
        public Dictionary<string, List<string>> GetMainFolderStructure()
        {
            var structure = new Dictionary<string, List<string>>();
            foreach (var group in folderGroups)
            {
                if (group.enabled && !string.IsNullOrWhiteSpace(group.mainFolder))
                {
                    var validSubfolders = group.subfolders.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    structure[group.mainFolder] = validSubfolders;
                }
            }
            return structure;
        }
        
        /// <summary>
        /// Returns a clean list of standalone folders, filtering out any empty or whitespace entries.
        /// </summary>
        public List<string> GetStandaloneFolders()
        {
            return standaloneFolders.Where(folder => !string.IsNullOrWhiteSpace(folder)).ToList();
        }
    
        /// <summary>
        /// This method is called in the Editor whenever the script is loaded or a value is changed in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            // The following logic cleans up empty list entries to prevent clutter in the Inspector.
            // It intentionally leaves one empty slot available, allowing the user to easily add a new item.

            if (standaloneFolders.Count > 1)
            {
                int emptyCount = 0;
                for (int i = standaloneFolders.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrWhiteSpace(standaloneFolders[i]))
                    {
                        emptyCount++;
                        if (emptyCount > 1)
                        {
                            standaloneFolders.RemoveAt(i);
                        }
                    }
                }
            }
        
            foreach (var group in folderGroups)
            {
                if (group.subfolders != null && group.subfolders.Count > 1)
                {
                    int emptyCount = 0;
                    for (int i = group.subfolders.Count - 1; i >= 0; i--)
                    {
                        if (string.IsNullOrWhiteSpace(group.subfolders[i]))
                        {
                            emptyCount++;
                            if (emptyCount > 1)
                            {
                                group.subfolders.RemoveAt(i);
                            }
                        }
                    }
                }
            }

            unchecked
            {
                version++;
            }
        }
    }
}
