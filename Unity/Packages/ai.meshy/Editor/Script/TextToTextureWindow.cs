#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Meshy
{
    public class TextToTextureWindows
    {
        private string _apiKeyField = "Meshy API Keys";

        private string[] artStyleKeys =
        {
            "Realistic", "2.5D Cartoon", "Cartoon Line Art", "2.5D Hand-drawn", "Japanese Anime",
            "Realistic Hand-drawn", "Oriental Comic Ink"
        };

        private string[] artStyleValues =
        {
            "realistic", "fake-3d-cartoon", "cartoon-line-art", "fake-3d-hand-drawn", "japanese-anime",
            "realistic-hand-drawn", "oriental-comic-ink"
        };

        private string[] texResolutionKeys = { "1024", "2048", "4096" };
        private string[] texResolutionValues = { "1024", "2048", "4096" };

        private string objectPrompt;
        private string stylePrompt;
        private string negativePrompt;
        private int artStyleIndex;
        private int texResolutionIndex;
        private bool enableOriginalUv;
        private bool enablePbr;
        public GameObject selectedObject;
        private string taskName = "Meshy-model";
        private float timer = 5;

        [System.Serializable]
        public class TaskInfo
        {
            public string id;
            public string name;
            public string art_style;
            public string object_prompt;
            public string style_prompt;
            public string negative_prompt;
            public string status;
            public long created_at;
            public int progress;
            public ModelUrl model_urls;
        }

        [System.Serializable]
        public class TaskObjects
        {
            public List<TaskInfo> tasks;
        }

        [System.Serializable]
        public class ModelUrl
        {
            public string glb;
            public string fbx;
            public string usdz;
        }

        [System.Serializable]
        public class TextureUrl
        {
            public string base_color;
            public string metallic;
            public string normal;
            public string roughness;
        }

        private TaskObjects tasks = null;
        public bool isRefreshing = false;
        private string buttonLabel = "Enable Auto Refreshing Task List";
        private Vector2 scrollPosition = new Vector2(0, 0);
        private DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
        private bool showGenerationSettings = true;
        private bool showTaskList = true;
        private static TextToTextureWindows instance;
        
        public static void DrawGUI()
        {
            if (instance == null)
            {
                instance = new TextToTextureWindows();
            }
            instance.OnGUI();
            instance.OnInspectorUpdate(); // 确保自动刷新功能正常工作
        }

        private void OnGUI()
        {
            showGenerationSettings = EditorGUILayout.Foldout(showGenerationSettings, "Generation Settings");

            if (showGenerationSettings)
            {
                objectPrompt = EditorGUILayout.TextField("Object Prompt", objectPrompt);
                stylePrompt = EditorGUILayout.TextField("Style Prompt", stylePrompt);
                negativePrompt = EditorGUILayout.TextField("Negative Prompt", negativePrompt);
                artStyleIndex = EditorGUILayout.Popup("Art Style", artStyleIndex, artStyleKeys);
                texResolutionIndex = EditorGUILayout.Popup("Resolution", texResolutionIndex, texResolutionKeys);
                enableOriginalUv = EditorGUILayout.Toggle("Enable Original UV", enableOriginalUv);
                enablePbr = EditorGUILayout.Toggle("Enable PBR", enablePbr);
                selectedObject =
                    EditorGUILayout.ObjectField("Selected GameObject", selectedObject, typeof(GameObject), true) as
                        GameObject;

                taskName = EditorGUILayout.TextField("Task Name", taskName);

                if (GUILayout.Button("Submit Task"))
                {
                    if (!EditorPrefs.HasKey(_apiKeyField))
                    {
                        Debug.LogError("No saved API key found!");
                        return;
                    }

                    if (string.IsNullOrEmpty(objectPrompt))
                    {
                        Debug.LogError("Object Prompt can not be empty!");
                        return;
                    }

                    if (string.IsNullOrEmpty(stylePrompt))
                    {
                        Debug.LogError("Style Prompt can not be empty!");
                        return;
                    }

                    if (string.IsNullOrEmpty(taskName))
                    {
                        Debug.LogError("Task Name can not be empty!");
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
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Download", GUILayout.Width(80));
                    GUILayout.Label("Task Name", GUILayout.Width(100));
                    GUILayout.Label("Art Style", GUILayout.Width(60));
                    GUILayout.Label("Status", GUILayout.Width(100));
                    GUILayout.Label("Progress", GUILayout.Width(80));
                    GUILayout.Label("Delete", GUILayout.Width(60));
                    GUILayout.EndHorizontal();

                    foreach (var task in tasks.tasks)
                    {
                        if (task.status == "EXPIRED") continue;

                        GUILayout.BeginHorizontal();

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
                        GUILayout.Label(task.art_style, GUILayout.Width(60));
                        GUILayout.Label(task.status, GUILayout.Width(100));
                        GUILayout.Label("", GUILayout.Width(80));
                        EditorGUI.ProgressBar(GUILayoutUtility.GetLastRect(), task.progress / 100.0f,
                            task.progress.ToString());

                        if (GUILayout.Button("Delete", GUILayout.Width(60)))
                        {
                            DeleteTask(task.id);
                        }

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndScrollView();
                }
            }
        }

        private void SubmitTaskToRemote()
        {
            Vector3 vcache = selectedObject.transform.position;
            selectedObject.transform.position = Vector3.zero;
            selectedObject.transform.position = vcache;
            string filePath = Path.Combine(Application.dataPath, "..", "Temp", "meshy_temp_model.fbx");
            string res = Utils.ExportBinaryFBX(filePath, selectedObject);
            if (res is null)
            {
                Debug.LogError("Converting unity asset to FBX failed.");
                return;
            }

            byte[] modelBinData = File.ReadAllBytes(filePath);

            WWWForm form = new WWWForm();
            form.AddField("object_prompt", objectPrompt);
            form.AddField("style_prompt", stylePrompt);
            form.AddField("negative_prompt", negativePrompt ?? "");
            form.AddField("enable_original_uv", enableOriginalUv.ToString());
            form.AddField("enable_pbr", enablePbr.ToString());
            form.AddField("art_style", artStyleValues[artStyleIndex]);
            form.AddField("resolution", texResolutionValues[texResolutionIndex]);
            form.AddField("name", taskName);
            form.AddBinaryData("model_file", modelBinData, $"{taskName}.fbx", "multipart/form-data");

            UnityWebRequest request = UnityWebRequest.Post(MeshyGlobalConfig.T2T_URL, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(_apiKeyField));
            request.downloadHandler = new DownloadHandlerBuffer();
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

            UnityWebRequest request = UnityWebRequest.Get(MeshyGlobalConfig.T2T_URL + "?sortBy=-created_at");
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
