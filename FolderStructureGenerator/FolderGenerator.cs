using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityFolderGenerator.Editor
{
    /// <summary>
    /// Contains the core logic for creating folder structures.
    /// This class is separate from the UI to promote modularity and reusability.
    /// </summary>
    public static class FolderGenerator
    {
        /// <summary>
        /// Creates the complete folder structure based on the provided configuration and project name.
        /// </summary>
        /// <param name="config">The ScriptableObject configuration for the folders.</param>
        /// <param name="projectName">The root name for the project's main folder.</param>
        public static FolderGenerationResult CreateAllFolders(FolderStructureConfig config, string projectName)
        {
            if (!IsValidFolderName(projectName))
            {
                const string message = "Please enter a valid project name without special characters.";
                EditorUtility.DisplayDialog("Invalid Project Name", message, "OK");
                return FolderGenerationResult.Failure(message);
            }

            var mainFolderStructure = config.GetMainFolderStructure();
            var standaloneFolders = config.GetStandaloneFolders(); // Directly use the list from the config
            int createdFolders = 0;
            int skippedFolders = 0;

            try
            {
                string assetsAbsolutePath = GetAssetsAbsolutePath();
                string projectRootPath = Path.Combine(assetsAbsolutePath, projectName);
                foreach (var folder in mainFolderStructure)
                {
                    if (!IsValidFolderName(folder.Key)) continue;

                    string mainFolderPath = Path.Combine(projectRootPath, folder.Key);
                    if (CreateDirectoryAndKeep(mainFolderPath, config.createGitKeepFiles && folder.Value.Count == 0))
                    {
                        createdFolders++;
                    }
                    else
                    {
                        skippedFolders++;
                    }

                    foreach (string subfolder in folder.Value)
                    {
                        if (!IsValidFolderName(subfolder)) continue;

                        string subFolderPath = Path.Combine(mainFolderPath, subfolder);
                        if (CreateDirectoryAndKeep(subFolderPath, config.createGitKeepFiles))
                        {
                            createdFolders++;
                        }
                        else
                        {
                            skippedFolders++;
                        }
                    }
                }

                foreach (var folder in standaloneFolders)
                {
                    if (!IsValidFolderName(folder)) continue;

                    string standaloneFolderPath = Path.Combine(assetsAbsolutePath, folder);
                    if (CreateDirectoryAndKeep(standaloneFolderPath, config.createGitKeepFiles))
                    {
                        createdFolders++;
                    }
                    else
                    {
                        skippedFolders++;
                    }
                }

                AssetDatabase.Refresh();
                string summary = BuildSummaryMessage(projectName, createdFolders, skippedFolders);
                EditorUtility.DisplayDialog("Success", summary, "OK");
                return FolderGenerationResult.Success(projectName, createdFolders, skippedFolders, summary);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create folder structure: {e.Message}");
                string errorMessage = $"Failed to create folders: {e.Message}";
                EditorUtility.DisplayDialog("Error", errorMessage, "OK");
                return FolderGenerationResult.Failure(errorMessage);
            }
        }

        private static string BuildSummaryMessage(string projectName, int createdFolders, int skippedFolders)
        {
            string folderWord = createdFolders == 1 ? "folder" : "folders";
            string summary = $"Created {createdFolders} {folderWord} for '{projectName}'.";
            if (skippedFolders > 0)
            {
                summary += $" Skipped {skippedFolders} existing folder{(skippedFolders == 1 ? string.Empty : "s")}.";
            }

            if (createdFolders == 0 && skippedFolders == 0)
            {
                summary = $"No folders were created for '{projectName}'.";
            }

            return summary;
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
                Debug.Log($"Skipped existing folder: {ToRelativeAssetsPath(path)}");
                return false;
            }

            Directory.CreateDirectory(path);
            Debug.Log($"Created folder: {ToRelativeAssetsPath(path)}");

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

        private static string GetAssetsAbsolutePath()
        {
            return Application.dataPath;
        }

        private static string ToRelativeAssetsPath(string absolutePath)
        {
            string normalizedPath = absolutePath.Replace('\\', '/');
            string normalizedAssetsPath = GetAssetsAbsolutePath().Replace('\\', '/');

            if (normalizedPath.StartsWith(normalizedAssetsPath))
            {
                string relative = normalizedPath.Substring(normalizedAssetsPath.Length).TrimStart('/');
                return string.IsNullOrEmpty(relative) ? "Assets" : $"Assets/{relative}";
            }

            return normalizedPath;
        }

        public readonly struct FolderGenerationResult
        {
            public bool Succeeded { get; }
            public string ProjectName { get; }
            public int CreatedFolders { get; }
            public int SkippedFolders { get; }
            public string Message { get; }

            private FolderGenerationResult(bool succeeded, string projectName, int createdFolders, int skippedFolders, string message)
            {
                Succeeded = succeeded;
                ProjectName = projectName;
                CreatedFolders = createdFolders;
                SkippedFolders = skippedFolders;
                Message = message;
            }

            public static FolderGenerationResult Success(string projectName, int createdFolders, int skippedFolders, string message)
            {
                return new FolderGenerationResult(true, projectName, createdFolders, skippedFolders, message);
            }

            public static FolderGenerationResult Failure(string message)
            {
                return new FolderGenerationResult(false, string.Empty, 0, 0, message);
            }
        }
    }
}
