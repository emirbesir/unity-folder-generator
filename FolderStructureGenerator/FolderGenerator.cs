using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Contains the core logic for creating folder structures.
    /// This class is separate from the UI to promote modularity and reusability.
    /// </summary>
    public static class FolderGenerator
    {
        private const string AssetsPath = "Assets";

        /// <summary>
        /// Creates the complete folder structure based on the provided configuration and project name.
        /// </summary>
        /// <param name="config">The ScriptableObject configuration for the folders.</param>
        /// <param name="projectName">The root name for the project's main folder.</param>
        public static void CreateAllFolders(FolderStructureConfig config, string projectName)
        {
            if (!IsValidFolderName(projectName))
            {
                EditorUtility.DisplayDialog("Invalid Project Name", "Please enter a valid project name without special characters.", "OK");
                return;
            }
        
            var mainFolderStructure = config.GetMainFolderStructure();
            var standaloneFolders = config.GetStandaloneFolders(); // Directly use the list from the config
            int totalFoldersCreated = 0;
            
            try
            {
                string projectRootPath = Path.Combine(AssetsPath, projectName);
                foreach (var folder in mainFolderStructure)
                {
                    if (!IsValidFolderName(folder.Key)) continue;

                    string mainFolderPath = Path.Combine(projectRootPath, folder.Key);
                    if (CreateDirectoryAndKeep(mainFolderPath, config.createGitKeepFiles && folder.Value.Count == 0))
                    {
                        totalFoldersCreated++;
                    }

                    foreach (string subfolder in folder.Value)
                    {   
                        if (!IsValidFolderName(subfolder)) continue;

                        string subFolderPath = Path.Combine(mainFolderPath, subfolder);
                        if (CreateDirectoryAndKeep(subFolderPath, config.createGitKeepFiles))
                        {
                            totalFoldersCreated++;
                        }
                    }
                }

                foreach (var folder in standaloneFolders)
                {
                    if (!IsValidFolderName(folder)) continue;

                    string standaloneFolderPath = Path.Combine(AssetsPath, folder);
                    if (CreateDirectoryAndKeep(standaloneFolderPath, config.createGitKeepFiles))
                    {
                        totalFoldersCreated++;
                    }
                }
            
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", $"Successfully created {totalFoldersCreated} folders for '{projectName}'!", "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create folder structure: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to create folders: {e.Message}", "OK");
            }
        }
    
        /// <summary>
        /// Creates a directory at the given path and optionally adds a .gitkeep file.
        /// It checks if the directory already exists.
        /// </summary>
        /// <param name="path">The full path where the directory should be created.</param>
        /// <param name="createGitKeep">If true, a .gitkeep file will be added.</param>
        /// <returns>True if the directory was newly created, false if it already existed.</returns>
        private static bool CreateDirectoryAndKeep(string path, bool createGitKeep)
        {
            if (Directory.Exists(path))
            {
                return false;
            }
            
            Directory.CreateDirectory(path);
            Debug.Log($"Created folder: {path}");

            if (createGitKeep)
            {
                string gitKeepPath = Path.Combine(path, ".gitkeep");
                if (!File.Exists(gitKeepPath))
                {
                    File.WriteAllText(gitKeepPath, "# This file ensures the folder is tracked by Git.\n");
                }
            }
            return true;
        }
    
        /// <summary>
        /// Validates if the folder name is valid for use.
        /// A valid name is not null/whitespace and contains no invalid file name characters.
        /// </summary>
        /// <param name="name">The name to validate.</param>
        /// <returns>True if the name is valid, false otherwise.</returns>
        public static bool IsValidFolderName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            
            // GetInvalidFileNameChars() returns characters that cannot be used in a folder/file name
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return name.IndexOfAny(invalidChars) < 0;
        }
    }
}
