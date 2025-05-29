# InteractiveATC

InteractiveATC is a Unity-based project designed to simulate air traffic control scenarios. It uses state-of-the-art voice transcription from OpenAI and a Text-to-Speech Services, to create the responses.

## Video

[![Video Title or Alt Text](https://img.youtube.com/vi/NWjVJQr67Yk/0.jpg)](https://www.youtube.com/watch?v=NWjVJQr67Yk)

## Features

- Realistic air traffic control simulation
- Interactive user interface for ATC communications
- 3D visualization of airspace and aircraft

## Requirements

- Unity Editor 6000.0.42f1 (probably also works with newer versions)
- Windows or macOS
- Quest 2 (probalby also works on Quest 3)

## Getting Started

### Getting the code

Ensure you have [Git LFS](https://git-lfs.github.com/) installed:

```
git lfs install
```

Clone this repo or download it as a zip file.

```
git clone https://github.com/yourusername/InteractiveATC.git
```

All of the project files can be found in `Assets/Models` and `Assets/Scripts`. This folder includes all scripts and assets to run the experience. This project depends on [Voice SDK v59](https://developer.oculus.com/downloads/package/meta-voice-sdk/).

### Configuration

Add a OpenAI API Key to `Assets/Scripts/Secrets.cs`

Wit.ai is already Configured, but here is how to do it.

Using _InteractiveATC_ reqiures a [Wit.ai](https://wit.ai) account.

1. Then find the `Server Access Token` in your wit.ai app setup under `Managment > Settings` from the left navigation panel. Go to Unity Editor, in the toolbar find `Meta > Voice SDK > Get Started`, select `Custom App`, and paste in your `Server Access Token`, click `Create` and choose a location to store the new app configuration, and wait until the Wit Configurations tab in the Voice Hub is fully populated.

2. Now go to the `Assets/Scenes/Loader` scene, find the `Management` game object under Hierarchy. Click on the `Management` game object under Inspector and find the `App Voice Experience (Script)` and `TTS Wit (Script)`.

   - Expand `App Dictation Experience (Script) > Wit Runtime Configuration`, select the wit.ai app configuration you just created.
   - Expand `TTS Wit (Script) > Request Settings`, select the same wit.ai app configuration.

For more information on setting up an App, check out the [Wit.ai Quickstart](https://wit.ai/docs/quickstart).

> **Note:** Wit.ai will need to train its model before it's ready to use. On Wit.ai, the current status of the training is indicated by the dot next to the app name.

### Run the game

To run _InteractiveATC_ on the Quest headset, go to `File > Build Settings`, Choose `Android` Platform, click `Switch Platform`, making sure the headset is connected, then click `Build And Run`. Please consult [Set Up Development Environment and Headset](https://developer.oculus.com/documentation/unity/unity-env-device-setup/) for more details.
