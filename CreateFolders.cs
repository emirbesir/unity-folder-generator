using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CreateFolders : EditorWindow 
    {
        // Constants for folder paths
        private const string AssetsPath = "Assets/";
        private const string DefaultConfigPath = "Assets/Editor/FolderStructureConfig.asset";
        // Window dimensions
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
    
        // Ensure a default config is loaded or created when the window is enabled
        private void OnEnable()
        {
            LoadOrCreateConfig();
        }
    
    
        private void LoadOrCreateConfig()
        {
            // Load existing config if it exists
            config = AssetDatabase.LoadAssetAtPath<FolderStructureConfig>(DefaultConfigPath);
        
            // If no config exists, create a new one
            if (config == null)
            {
                // Create default config
                config = CreateInstance<FolderStructureConfig>();
                config.SetDefaultStructure();
            
                // Ensure the Editor folder exists
                string editorPath = Path.GetDirectoryName(DefaultConfigPath);
                if (!Directory.Exists(editorPath))
                {
                    Directory.CreateDirectory(editorPath);
                }
            
                // Save the new config asset
                AssetDatabase.CreateAsset(config, DefaultConfigPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created default folder structure config at: {DefaultConfigPath}");
            }
        
            // Set the project name from the config
            if (string.IsNullOrEmpty(projectName))
            {
                projectName = config.defaultProjectName;
            }
        }
    
        // Method to create all folders based on the selected configuration
        private void CreateAllFolders()
        {
            if (!IsValidProjectName(projectName))
            {
                EditorUtility.DisplayDialog("Invalid Project Name", 
                    "Please enter a valid project name without special characters.", "OK");
                return;
            }
        
            var folderStructure = config.GetFolderStructure();
        
            try
            {
                int totalFolders = 0;
            
                // Create main folders and their subfolders
                foreach (var folder in folderStructure)
                {
                    string mainFolderPath = AssetsPath + projectName + "/" + folder.Key;
                
                    // Create main folder if it doesn't exist
                    if (!Directory.Exists(mainFolderPath))
                    {
                        Directory.CreateDirectory(mainFolderPath);
                        Debug.Log($"Created folder: {mainFolderPath}");
                        totalFolders++;
                    
                        // Create .gitkeep if enabled and no subfolders
                        if (config.createGitKeepFiles && folder.Value.Count == 0)
                        {
                            CreateGitKeepFile(mainFolderPath);
                        }
                    }
                
                    // Create subfolders
                    foreach (string subfolder in folder.Value)
                    {
                        string subFolderPath = mainFolderPath + "/" + subfolder;
                        if (!Directory.Exists(subFolderPath))
                        {
                            Directory.CreateDirectory(subFolderPath);
                            Debug.Log($"Created subfolder: {subFolderPath}");
                            totalFolders++;
                        
                            // Create .gitkeep if enabled
                            if (config.createGitKeepFiles)
                            {
                                CreateGitKeepFile(subFolderPath);
                            }
                        }
                    }
                }
            
                AssetDatabase.Refresh();
            
                EditorUtility.DisplayDialog("Success", 
                    $"Successfully created {totalFolders} folders for '{projectName}'!", "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create folder structure: {e.Message}");
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to create folders: {e.Message}", "OK");
            }
        }
    
        private void CreateGitKeepFile(string folderPath)
        {
            string gitKeepPath = Path.Combine(folderPath, ".gitkeep");
            if (!File.Exists(gitKeepPath))
            {
                File.WriteAllText(gitKeepPath, "# This file ensures the folder is tracked by Git\n");
            }
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
        
            // Project name input
            EditorGUILayout.LabelField("Project Name:");
            projectName = EditorGUILayout.TextField(projectName);
        
            GUILayout.Space(10);
        
            // Config file reference
            EditorGUILayout.LabelField("Configuration File:", EditorStyles.boldLabel);
            FolderStructureConfig newConfig = (FolderStructureConfig)EditorGUILayout.ObjectField(
                "Folder Config", config, typeof(FolderStructureConfig), false);
        
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
                EditorGUILayout.HelpBox("No configuration file selected. Please select or create a FolderStructureConfig asset.", MessageType.Warning);
                if (GUILayout.Button("Create New Config"))
                {
                    LoadOrCreateConfig();
                }
                EditorGUILayout.EndScrollView();
                return;
            }
        
            GUILayout.Space(10);
        
            // Advanced options toggle
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");
            if (showAdvancedOptions)
            {
                EditorGUI.indentLevel++;
            
                EditorGUILayout.LabelField("Configuration Preview:", EditorStyles.boldLabel);
            
                var folderStructure = config.GetFolderStructure();
                foreach (var folder in folderStructure)
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
            }
        
            GUILayout.Space(10);
        
            // Info box
            string infoText = $"This will create folders based on the selected configuration.\n";
            infoText += $"Git keep files: {(config.createGitKeepFiles ? "Enabled" : "Disabled")}\n";
            infoText += $"Total folder groups: {config.folderGroups.FindAll(g => g.enabled).Count}";
        
            EditorGUILayout.HelpBox(infoText, MessageType.Info);
        
            GUILayout.Space(15);
        
            // Buttons
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