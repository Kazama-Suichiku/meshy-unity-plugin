# Meshy for Unity

## User Documentation

### Installation

After the installation, we can use Meshy-for-Unity to generate models. Before we continue, please make sure that you have already registered a Meshy account and obtained an API key. If you have not, you can follow the [instruction](https://docs.meshy.ai/api-authentication).

Once you have your API key, we need to configure the API key. In Unity, select "Meshy"->"API Key" to open the window.

<p align="center"> <img src = "https://github.com/user-attachments/assets/9f78b2b0-6e88-463d-9a22-18f2ac3a1b20"> </p>

Enter the API key obtained from the Meshy website, and click "Save Key" to save the API key to local.

<p align="center"> <img src = "https://github.com/user-attachments/assets/68eb2e74-f9f8-4919-8ea1-f222ab691ac4"> </p>

Now we are ready to generate!

### Text to Texture

After registering a Meshy account and obtaining an API key, we can use Meshy-for-Unity to generate textures. In this tutorial, we will use a T-Rex  model as an example. After installing Meshy-for-Unity, we create a new scene in Unity and add the T-Rex model.

<p align="center"> <img src = "https://github.com/user-attachments/assets/5729f066-0a9f-478b-9835-14aac0c6b179"> </p>

Now we can start generating textures for our T-Rex model. In the scene, right-click the T-Rex game object and select "Meshy"->"Text To Texture". Or you can select "Meshy"->"Text To Texture" in the toolbar and select the game object by yourself.

<p align="center"> <img src = "https://github.com/user-attachments/assets/9d1b561e-2f73-4635-a224-4a98b3944cd5"> </p>

In the popup window, you can enter custom prompts to generate textures, and click "Submit Task" to submit the task.

For example:

* Object Prompt: `a T-Rex`
* Prompt: `a giant T-Rex, 4k, hdr, highest quality`
* Art Style: `realistic`
* Resolution: `4096`
* Task Name: `T-Rex`

<p align="center"> <img src = "https://github.com/user-attachments/assets/39517e00-8735-4340-92f1-4b3e9bfa8c40"> </p>

After submitting the task, you can click the "Enable Auto Refreshing Task List" button to get the task list, which records the status of submitted tasks. When a task is completed, you can click the "Download" button to download the model with generated textures. Please note that completed tasks will expire after a few days, and only unexpired tasks will be displayed in the task list.

<p align="center"> <img src = "https://github.com/user-attachments/assets/5a3a55b0-13b5-4625-94cb-b60217400dd6"> </p>

After clicking the "Download" button, the model will be automatically downloaded to "Assets"->"Meshy" directory.

Add the textured model to the scene, and you can see the final model with textures!

<p align="center"> <img src = "https://github.com/user-attachments/assets/0556360f-9cf6-4b5a-98e2-b045625ce50b"> </p>

### Text to Model

Besides texturing existing models, we can also generate models with textures from scratch. Select "Meshy"->"Text to Model" to open the window, in which you can enter prompts, task name and select art styles. Click "Submit Task" to submit the task.

For example:

* Prompt: `a legendary battle axe ,fantasy, #Medieval Game Assets#`
* Task Name: `axe`
* Art Style: `realistic`

<p align="center"> <img src = "https://github.com/user-attachments/assets/f5861ed9-9b72-4e01-b821-38486249cbe7"> </p>

After submitting the task, you can click the "Enable Auto Refreshing Task List" button to get the task list, which records the status of submitted tasks. 

<p align="center"> <img src = "https://github.com/user-attachments/assets/f333467f-73c3-4957-8fd8-6f32a934d553"> </p>

When a task is completed, you can click the "Download" button to download the model. After clicking the "Download" button, the model will be automatically downloaded to "Assets"->"Meshy" directory just like Text to Texture. 

<p align="center"> <img src = "https://github.com/user-attachments/assets/2f281984-e039-42eb-9c52-267da58d314a"> </p>

Add the textured model to the scene, and you can see the final model!

<p align="center"> <img src = "https://github.com/user-attachments/assets/a1dcb840-7cb9-4e4c-a765-8a2bbedbb648"> </p>

If you want the model to be more detailed, you can click the "Refine" button on the right side of the task list to generate a model with higher quality. It will submit a new task with the mode "refine" which you can see if you have enabled auto refreshing.

<p align="center"> <img src = "https://github.com/user-attachments/assets/0b1bdc91-5314-44eb-b329-50a370a8aaec"> </p>

Download the refined model in the same way and add it into the scene, you can see that there is a significant improvement in the quality of the model after it is refined. 

<p align="center"> <img src = "https://github.com/user-attachments/assets/dea4f507-453f-4b6a-849f-b3f2992b0699"> </p>

## Developer Documentation

### Code Design

UI Components:

* ``: Entry point for obtaining the API key.
* `Meshy.TextToModel.MainWindow`: Entry point for the Text to Model feature.
* `Meshy.TextToTexture.MainWindow`: Entry point for the Text to Texture feature.

Core Functions:

* `SubmitTaskToRemote()`: Submit a task to remote.
* `AcquireResultsFromRemote()`: Download results of a task from remote.
* `RefreshTaskList()`: Refresh the status of the task list.
* `RefineModel()`: Refine a preview task in Text to Model feature.
* `DeleteTask()`: Delete a task from the task list.
* `OnInspectorUpdate()`: Timer for auto refreshing.
