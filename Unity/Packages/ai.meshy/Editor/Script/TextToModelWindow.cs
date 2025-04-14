#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Plastic.Newtonsoft.Json;

namespace Meshy
{
    public class TextToModelWindows
    {
        static string _apiKeyField = "Meshy API Keys";
        private string prompt = "";
        private string negativePrompt = "";
        private string taskName = "Meshy-model";
        private int artStyleIndex;
        private string[] artStyleKeys = new string[] { "realistic", "cartoon", "low-poly" };
        private string seed = "";

        private float timer = 5;
        public bool isRefreshing = false;
        private string buttonLabel = "Enable Auto Refreshing Task List";
        private Vector2 scrollPosition = new Vector2(0, 0);
        private bool showGenerationSettings = true;
        private bool showTaskList = true;

        [System.Serializable]
        public class ModelUrls
        {
            public string glb;
            public string fbx;
            public string usdz;
            public string obj;
            public string mtl;
        }

        [System.Serializable]
        public class TextureUrls
        {
            public string base_color;
        }

        [System.Serializable]
        public class TaskInfo
        {
            public string id;
            public string mode;
            public string name;
            public long seed;
            public string art_style;
            public string texture_richness;
            public string prompt;
            public string negative_prompt;
            public string status;
            public string created_at;
            public long progress;
            public long started_at;
            public long finished_at;
            public string task_error;
            public ModelUrls model_urls;
            public string thumbnail_url;
            public string video_url;
            public List<TextureUrls> texture_urls;
        }

        public class TaskObjects
        {
            public List<TaskInfo> tasks;
        }

        private TaskObjects tasks = null;
        
        private static TextToModelWindows instance;
        
        public static void DrawGUI()
        {
            if (instance == null)
            {
                instance = new TextToModelWindows();
            }
            instance.OnGUI();
            instance.OnInspectorUpdate(); // 确保自动刷新功能正常工作
        }
        
        private void OnGUI()
        {
            showGenerationSettings = EditorGUILayout.Foldout(showGenerationSettings, "Generation Settings");

            if (showGenerationSettings)
            {
                prompt = EditorGUILayout.TextField("Prompt", prompt);

                negativePrompt = EditorGUILayout.TextField("Negative Prompt", negativePrompt);

                taskName = EditorGUILayout.TextField("Task Name", taskName);

                artStyleIndex = EditorGUILayout.Popup("Art Style", artStyleIndex, artStyleKeys);

                seed = EditorGUILayout.TextField("Seed", seed);

                if (GUILayout.Button("Submit Task"))
                {
                    if (string.IsNullOrEmpty(prompt))
                    {
                        Debug.LogError("Prompt can not be empty!");
                        return;
                    }

                    SubmitTaskToRemote();
                }
            }

            GUILayout.Space(10);

            showTaskList = EditorGUILayout.Foldout(showTaskList, "Task List");
            if (showTaskList)
            {
                if (GUILayout.Button(buttonLabel))
                {
                    isRefreshing = !isRefreshing;
                    buttonLabel = isRefreshing
                        ? "Disable Auto Refreshing Task List"
                        : "Enable Auto Refreshing Task List";
                    if (isRefreshing)
                    {
                        RefreshTaskList();
                    }
                }

                if (tasks != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Download", GUILayout.Width(80));
                    GUILayout.Label("Task Name", GUILayout.Width(100));
                    GUILayout.Label("Mode", GUILayout.Width(60));
                    GUILayout.Label("Art Style", GUILayout.Width(60));
                    GUILayout.Label("Status", GUILayout.Width(100));
                    GUILayout.Label("Progress", GUILayout.Width(60));
                    GUILayout.Label("Refine", GUILayout.Width(60));
                    GUILayout.Label("Delete", GUILayout.Width(60));
                    GUILayout.EndHorizontal();

                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                    foreach (var task in tasks.tasks)
                    {
                        EditorGUILayout.BeginHorizontal();

                        if (task.status != "SUCCEEDED")
                        {
                            GUI.enabled = false;
                        }

                        if (GUILayout.Button("Download", GUILayout.Width(80)))
                        {
                            AcquireResultsFromRemote(task);
                        }

                        GUI.enabled = true;

                        GUILayout.Label(task.name, GUILayout.Width(100));
                        GUILayout.Label(task.mode, GUILayout.Width(60));
                        GUILayout.Label(task.art_style, GUILayout.Width(60));
                        GUILayout.Label(task.status, GUILayout.Width(100));
                        GUILayout.Label("", GUILayout.Width(60)); // For locating the position of progress bar
                        EditorGUI.ProgressBar(GUILayoutUtility.GetLastRect(), task.progress / 100.0f,
                            task.progress.ToString());

                        if (task.mode != "preview" || task.status != "SUCCEEDED")
                        {
                            GUI.enabled = false;
                        }

                        if (GUILayout.Button("Refine", GUILayout.Width(60)))
                        {
                            RefineModel(task.id, task.name);
                        }

                        GUI.enabled = true;

                        if (GUILayout.Button("Delete", GUILayout.Width(60)))
                        {
                            DeleteTask(task.id);
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private void SubmitTaskToRemote()
        {
            Dictionary<string, string> form = new Dictionary<string, string>
            {
                ["mode"] = "preview",
                ["prompt"] = prompt,
                ["art_style"] = artStyleKeys[artStyleIndex],
                ["negative_prompt"] = negativePrompt,
                ["name"] = taskName
            };

            if (!string.IsNullOrEmpty(seed))
            {
                form["seed"] = seed;
            }

            UnityWebRequest request = new UnityWebRequest(MeshyGlobalConfig.T2M_URL, "POST")
            {
                downloadHandler = new DownloadHandlerBuffer(),
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(form)))
            };
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(_apiKeyField));
            request.SetRequestHeader("Content-Type", "application/json");
            request.SendWebRequest().completed += (AsyncOperation operation) =>
            {
                UnityWebRequest request = ((UnityWebRequestAsyncOperation)operation).webRequest;
                if (!Utils.CheckRequestSuccess(request))
                {
                    Debug.LogError("Task submission failed.");
                    return;
                }

                string submittedTaskInfo = request.downloadHandler.text;
                Debug.Log(submittedTaskInfo);
            };
        }

        private void RefreshTaskList()
        {
            if (!EditorPrefs.HasKey(_apiKeyField))
            {
                Debug.LogError("No saved API key!");
                return;
            }

            UnityWebRequest request = UnityWebRequest.Get(MeshyGlobalConfig.T2M_URL + "?sortBy=-created_at");
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(_apiKeyField));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SendWebRequest().completed += (AsyncOperation operation) =>
            {
                UnityWebRequest request = ((UnityWebRequestAsyncOperation)operation).webRequest;
                if (!Utils.CheckRequestSuccess(request))
                {
                    Debug.LogError("Failed to refresh task list.");
                    return;
                }

                string json = "{\"tasks\":" + request.downloadHandler.text + "}";
                tasks = JsonUtility.FromJson<TaskObjects>(json);
            };
        }

        private void AcquireResultsFromRemote(TaskInfo task)
        {
            UnityWebRequest download = UnityWebRequest.Get(task.model_urls.fbx);
            download.SendWebRequest().completed += (AsyncOperation operation) =>
            {
                UnityWebRequest request = ((UnityWebRequestAsyncOperation)operation).webRequest;
                if (!Utils.CheckRequestSuccess(request))
                {
                    Debug.LogError("Model download failed.");
                    return;
                }

                Utils.ImportAsAsset(request.downloadHandler.data);
                request.Dispose();
                Debug.Log("Model downloaded and imported successfully.");
            };
        }

        private void RefineModel(string modelId, string taskName = null)
        {
            Dictionary<string, string> form = new Dictionary<string, string>
            {
                ["mode"] = "refine",
                ["preview_task_id"] = modelId
            };

            if (taskName != null)
            {
                form["name"] = taskName;
            }

            UnityWebRequest request = new UnityWebRequest(MeshyGlobalConfig.T2M_URL, "POST")
            {
                downloadHandler = new DownloadHandlerBuffer(),
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(form)))
            };
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(_apiKeyField));
            request.SetRequestHeader("Content-Type", "application/json");
            request.SendWebRequest().completed += (AsyncOperation operation) =>
            {
                UnityWebRequest request = ((UnityWebRequestAsyncOperation)operation).webRequest;
                if (!Utils.CheckRequestSuccess(request))
                {
                    Debug.LogError("Model refinement failed.");
                    return;
                }

                Debug.Log("Model refined successfully.");
            };
        }

        private void DeleteTask(string modelId)
        {
            UnityWebRequest request = UnityWebRequest.Delete(MeshyGlobalConfig.T2M_URL + "/" + modelId);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(_apiKeyField));
            request.SendWebRequest().completed += (_) => { RefreshTaskList(); };
        }

        private void OnInspectorUpdate()
        {
            timer += 0.167f;
            if (timer >= 5.0f)
            {
                if (isRefreshing)
                {
                    RefreshTaskList();
                }

                timer = 0;
            }
        }
    }
}
#endif
