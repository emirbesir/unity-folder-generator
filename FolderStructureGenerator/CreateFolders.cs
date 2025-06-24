using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// The Editor Window for the Folder Structure Generator.
    /// This class is responsible for drawing the UI and capturing user input.
    /// It calls the FolderGenerator class to perform the actual folder creation logic.
    /// </summary>
    public class CreateFolders : EditorWindow 
    {
        private const int WindowWidth = 500;
        private const int WindowHeight = 400;
        
        private string projectName = "";
        private FolderStructureConfig config;
        private Vector2 scrollPosition;
        private bool showAdvancedOptions;
    
        /// <summary>
        /// Creates the menu item in the Unity Editor to open this window.
        /// </summary>
        [MenuItem("Assets/Create Project Folders")]
        private static void ShowFolderCreationWindow()
        {
            CreateFolders window = GetWindow<CreateFolders>("Folder Generator");
            window.minSize = new Vector2(WindowWidth, WindowHeight);
            window.Show();
        }
    
        /// <summary>
        /// This method is called by Unity to draw the editor window GUI.
        /// </summary>
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawHeader();
            DrawConfigurationSelector();

            // If no config is selected, stop drawing the rest of the UI
            if (config == null)
            {
                EditorGUILayout.HelpBox("Please select a FolderStructureConfig asset to begin.", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawAdvancedOptions();
            DrawInfoBox();
            DrawActionButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Unity Folder Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
        
            EditorGUILayout.LabelField("Project Name:", EditorStyles.label);
            projectName = EditorGUILayout.TextField(projectName);
            GUILayout.Space(10);
        }

        private void DrawConfigurationSelector()
        {
            EditorGUILayout.LabelField("Configuration File:", EditorStyles.boldLabel);
            FolderStructureConfig newConfig = (FolderStructureConfig)EditorGUILayout.ObjectField("Folder Config", config, typeof(FolderStructureConfig), false);
        
            // When the user assigns a new config file
            if (newConfig != config)
            {
                config = newConfig;
                // If the project name field is empty, populate it with the default from the config
                if (config != null && string.IsNullOrEmpty(projectName))
                {
                    projectName = config.defaultProjectName;
                }
            }
        }

        private void DrawAdvancedOptions()
        {
            GUILayout.Space(10);
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");

            if (showAdvancedOptions)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Configuration Preview:", EditorStyles.boldLabel);
            
                var mainFolderStructure = config.GetMainFolderStructure();
                var standaloneFolders = config.GetStandaloneFolders();
                
                GUIContent folderIcon = EditorGUIUtility.IconContent("Folder Icon");
                GUIContent subfolderIcon = EditorGUIUtility.IconContent("FolderEmpty Icon");

                EditorGUILayout.LabelField(new GUIContent($" {projectName}", folderIcon.image));
                EditorGUI.indentLevel++;
                
                foreach (var folder in mainFolderStructure)
                {
                    EditorGUILayout.LabelField(new GUIContent($" {folder.Key}", folderIcon.image));
                    if (folder.Value.Count > 0)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var subfolder in folder.Value)
                        {
                            EditorGUILayout.LabelField(new GUIContent($" {subfolder}", subfolderIcon.image));
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                
                EditorGUI.indentLevel--;
                
                foreach (var folder in standaloneFolders)
                {
                     if (string.IsNullOrWhiteSpace(folder)) continue;
                    EditorGUILayout.LabelField(new GUIContent($" {folder}", folderIcon.image));
                }
            
                EditorGUI.indentLevel = 0; // Reset indent level to be safe
            }
        }

        private void DrawInfoBox()
        {
            GUILayout.Space(10);
            string infoText = $"This will create folders based on the selected configuration.\n";
            infoText += $"Git keep files: {(config.createGitKeepFiles ? "Enabled" : "Disabled")}\n";
            infoText += $"Total folder groups: {config.folderGroups.FindAll(g => g.enabled).Count}";
            EditorGUILayout.HelpBox(infoText, MessageType.Info);
        }

        private void DrawActionButtons()
        {
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
        
            if (GUILayout.Button("Open Config", GUILayout.Height(30)))
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
        
            GUI.enabled = FolderGenerator.IsValidFolderName(projectName) && config != null;
            if (GUILayout.Button("Generate Folders!", GUILayout.Height(30)))
            {
                FolderGenerator.CreateAllFolders(config, projectName);
            }
            GUI.enabled = true;
        
            EditorGUILayout.EndHorizontal();
        }
    }
}
