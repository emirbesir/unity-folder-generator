using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
public class CreateFolders : EditorWindow {
    private static string projectName = "PROJECT_NAME";
    [MenuItem("Assets/Create Default Folders")]
    private static void SetUpFolders()
    {
        CreateFolders window =
            ScriptableObject.CreateInstance<CreateFolders>();
        window.position = new Rect(Screen.width/2, Screen.height/2, 400, 150);
        window.ShowPopup();
    }
    private static void CreateAllFolders()
    {
        // Define main folders and their subfolders
        Dictionary<string, List<string>> folderStructure = new Dictionary<string, List<string>>
        {
            { 
                "Art", new List<string> { 
                    "Sprites",
                    "Animations",
                    "Tilemaps",
                    "Shaders",
                    "Materials", 
                    "Models", 
                    "Textures"
                } 
            },
            { 
                "Audio", new List<string> { 
                    "Music", 
                    "SFX"
                } 
            },
            { 
                "Prefabs", new List<string> { 
                    "Characters", 
                    "Environment", 
                    "UI"
                } 
            },
            { 
                "Scenes", new List<string> { 
                    "Gameplay", 
                    "Menus", 
                    "Test" 
                } 
            },
            { 
                "Scripts", new List<string> { 
                    "Core", 
                    "Gameplay", 
                    "UI", 
                    "Data", 
                    "Editor",
                    "Tests"
                } 
            },
            { 
                "Settings", new List<string> { 
                } 
            }
        };
        
        // Create main folders and their subfolders
        foreach (var folder in folderStructure)
        {
            string mainFolderPath = "Assets/" + projectName + "/" + folder.Key;
            
            // Create main folder if it doesn't exist
            if (!Directory.Exists(mainFolderPath))
            {
                Directory.CreateDirectory(mainFolderPath);
            }
            
            // Create subfolders
            foreach (string subfolder in folder.Value)
            {
                string subFolderPath = mainFolderPath + "/" + subfolder;
                if (!Directory.Exists(subFolderPath))
                {
                    Directory.CreateDirectory(subFolderPath);
                }
            }
        }
        
        string externalsPath = "Assets/Externals";
            
        // Create externals folder if it doesn't exist
        if (!Directory.Exists(externalsPath))
        { 
            Directory.CreateDirectory(externalsPath);
        }
        
        AssetDatabase.Refresh();
    }
    
    void OnGUI()
    {
        EditorGUILayout.LabelField("Insert the Project name used as the root folder");
        projectName = EditorGUILayout.TextField("Project Name: ",projectName);
        this.Repaint();
        GUILayout.Space(65);
        if (GUILayout.Button("Generate!")) {
            CreateAllFolders();
            this.Close();
        }
    }
}

