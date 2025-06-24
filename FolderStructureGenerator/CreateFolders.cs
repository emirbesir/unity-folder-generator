using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CreateFolders : EditorWindow 
    {
        private const string AssetsPath = "Assets";
        private const int WindowWidth = 500;
        private const int WindowHeight = 400;
        
        private string projectName = "";
        private FolderStructureConfig config;
        private Vector2 scrollPosition;
        private bool showAdvancedOptions;
    
        // Menu item to open the folder creation window
        [MenuItem("Assets/Create Folders")]
        private static void ShowFolderCreationWindow()
        {
            CreateFolders window = GetWindow<CreateFolders>();
            window.titleContent = new GUIContent("Folder Structure Generator");
            window.minSize = new Vector2(WindowWidth, WindowHeight);
            window.Show();
        }
    
        // Method to create all folders based on the selected configuration
        private void CreateAllFolders()
        {
            if (!IsValidProjectName(projectName))
            {
                EditorUtility.DisplayDialog("Invalid Project Name", "Please enter a valid project name without special characters.", "OK");
                return;
            }
        
            var mainFolderStructure = config.GetMainFolderStructure();
            var standaloneFolderStructure = config.GetStandaloneFolderStructure();
            int totalFoldersCreated = 0;
            
            try
            {
                // Create main project folder structure
                string projectRootPath = Path.Combine(AssetsPath, projectName);
                foreach (var folder in mainFolderStructure)
                {
                    string mainFolderPath = Path.Combine(projectRootPath, folder.Key);
                    if (CreateDirectoryAndKeep(mainFolderPath, config.createGitKeepFiles && folder.Value.Count == 0))
                    {
                        totalFoldersCreated++;
                    }
                
                    // Create subfolders
                    foreach (string subfolder in folder.Value)
                    {
                        string subFolderPath = Path.Combine(mainFolderPath, subfolder);
                        if (CreateDirectoryAndKeep(subFolderPath, config.createGitKeepFiles))
                            {
                            totalFoldersCreated++;
                        }
                    }
                }

                // Create standalone folders
                foreach (var folder in standaloneFolderStructure)
                {
                    string standaloneFolderPath = Path.Combine(AssetsPath, folder.Key);
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
    
        // Refactored Helper Method for creating directories
        private bool CreateDirectoryAndKeep(string path, bool createGitKeep)
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
                File.WriteAllText(gitKeepPath, "# This file ensures the folder is tracked by Git\n");
            }
        }
            return true;
        }
    
        private bool IsValidProjectName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return name.IndexOfAny(invalidChars) < 0;
        }
    
        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Unity Folder Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
        
            EditorGUILayout.LabelField("Project Name:");
            projectName = EditorGUILayout.TextField(projectName);
            GUILayout.Space(10);
        
            EditorGUILayout.LabelField("Configuration File:", EditorStyles.boldLabel);
            FolderStructureConfig newConfig = (FolderStructureConfig)EditorGUILayout.ObjectField("Folder Config", config, typeof(FolderStructureConfig), false);
        
            if (newConfig != config)
            {
                config = newConfig;
                if (config != null && string.IsNullOrEmpty(projectName))
                {
                    projectName = config.defaultProjectName;
                }
            }
        
            if (config == null)
            {
                EditorGUILayout.HelpBox("No configuration file selected. Please select a FolderStructureConfig asset.", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }
        
            GUILayout.Space(10);
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");

            if (showAdvancedOptions)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Configuration Preview:", EditorStyles.boldLabel);
            
                var mainFolderStructure = config.GetMainFolderStructure();
                var standaloneFolderStructure = config.GetStandaloneFolderStructure();
                
                EditorGUILayout.LabelField($"📁 {projectName}");
                EditorGUI.indentLevel++;
                
                foreach (var folder in mainFolderStructure)
                {
                    EditorGUILayout.LabelField($"📁 {folder.Key}");
                    if (folder.Value.Count > 0)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var subfolder in folder.Value)
                        {
                            EditorGUILayout.LabelField($"📂 {subfolder}");
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                
                EditorGUI.indentLevel--;
                
                foreach (var folder in standaloneFolderStructure)
                {
                    EditorGUILayout.LabelField($"📁 {folder.Key}");
                }
            
                EditorGUI.indentLevel--;
            }
        
            GUILayout.Space(10);
        
            string infoText = $"This will create folders based on the selected configuration.\n";
            infoText += $"Git keep files: {(config.createGitKeepFiles ? "Enabled" : "Disabled")}\n";
            infoText += $"Total folder groups: {config.folderGroups.FindAll(g => g.enabled).Count}";
            EditorGUILayout.HelpBox(infoText, MessageType.Info);
        
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
        
            if (GUILayout.Button("Open Config", GUILayout.Height(30)))
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
        
            GUI.enabled = IsValidProjectName(projectName) && config != null;
            if (GUILayout.Button("Generate Folders!", GUILayout.Height(30)))
            {
                CreateAllFolders();
            }
            GUI.enabled = true;
        
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }
    }
}