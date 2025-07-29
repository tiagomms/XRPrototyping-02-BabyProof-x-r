# BabyProofXR - XR Object Detection and Scene Understanding Prototype

## Quick Links 🔗
## 🎥 [Watch Demo Video and Download APK](https://drive.google.com/drive/folders/1OLXIlN2O2HMEfddcInAJTlfAvXzgD9T4?usp=sharing)

![BabyProofxr short video walkthrough](./Images/02-BabyProofxr-TiagoMSilva.gif)

## Overview 🎯
This project is the second one-week solo prototype developed for the XR Bootcamp XR Prototyping course (May-July 2025). 

I wanted parents of toddlers coming to new locations (or in their own homes) to know what areas they need to be more careful and what sort of easy adjustments they can do - that don't involve buying stuff. Since toddlers have a tendency to go about anywhere dangerous, it seemed an interesting challenge. For this first prototype, I am focusing on eminent danger (objects around that are hazardous) and in danger locations.

Again, in XR with camera access + AI we can start doing this sort of analysis.

In addition to this, I can tell my partner and my baby that I am working on important stuff. 😎

## Tech Stack
- **Unity**: 6000.0.51f1
- **Meta XR SDK**: 
  - All-in-one SDK v76
  - Camera Access API
  - Scene Understanding and MRUK
- **AI/ML**: 
  - Unity Sentis
  - YoloV8 for object detection

## Core Features
- Real-time object detection using Unity Sentis and YoloV8
- Integration with Meta's Scene Understanding (MRUK)
- Camera access and environment raycasting
- Object filtering based on dangerous labels
- Scene anchor detection and labeling
- Camera access does not work on Oculus Link, so I had to create a way of avoiding it and still test Unity Sentis. Built a simulation that runs a set of images that on Unity Sentis and change every x seconds. It was a child of the camera, so I could move the images to the areas where scene understanding considered ok.
- Palm Hand UI for easier management
- Optional Use of Speech-to-text to start/stop experience (Activate / Stop)

## Project Setup 
You need a [create an account in Groq](https://console.groq.com/home) and create a API Key to have this experience working. After that:

1. Create an APIKeyConfig asset: Right-click in Project window → Create → Config → API Key Config. Move file ideally to the Secrets folder so that is not published on github
2. Add your API keys to the config asset 
3. Assign the config asset to the APIKeyLoader component in the scene inspector

**Note:** You may still test the experience without the AI part, by never using the Microphone button (that triggers the AI). You can create a APIKeyConfig with a fake API Key, and it will work as the Object Detection runs locally.

## Project Structure
Assets/

├── Scenes/

│ └── MultiObjectDetection/

│ └── SentisInference/

│ └── Scripts/

│ ├── BabyProofxrFilter.cs

│ ├── BabyProofxrInferenceRunManager.cs

│ └── BabyProofxrInferenceUiManager.cs

└── Scripts/

└── BoundingZones/

├── BoundingZoneChecker.cs

├── BoundingZoneManager.cs

└── LabelOffsetConfig.cs


## Known Limitations
1. **AI Model and Environment Ray Manager**:
   - Current implementation of AI model for object detection and Meta's environment ray manager are still in early stages
   - Some performance and accuracy limitations exist

2. **Scene Understanding**:
   - Meta's scene understanding creates block-based representations of structures
   - Limited ability to recognize complex structures like shelves
   - Objects on shelves may not be properly detected

3. **Detection Accuracy**:
   - False positives in object detection
   - Need for better filtering of non-relevant objects

## Development Notes
### What Worked Well
- Successful integration of Camera Access with Scene Understanding
- Basic object detection implementation
- Label-based filtering system
- Understanding of MRUK anchor system

### Areas for Improvement
1. **Code Quality**:
   - Need to better adhere to SOLID principles
   - More robust error handling
   - Better separation of concerns

2. **Development Process**:
   - Avoid last-minute major changes
   - Better planning for integration points
   - More thorough testing of component interactions

3. **Feature Enhancements**:
   - Reduce false positives in object detection
   - Show areas where toddlers can navigate
   - Handle shelves with scene understanding
   - Implement object tracking to provide feedback on safe locations
   - Integrate voice SDK for:
     - Triggering the experience
     - Providing contextual cues
   - Create custom AI model for:
     - Home appliance detection
     - Fruit detection
   - Better object categorization
   - Improved scene understanding
   - More accurate danger zone detection

## Lessons Learned
1. **Development Process**:
   - Importance of proper planning for major integrations
   - Value of following SOLID principles
   - Need for thorough testing of component interactions

2. **Technical Insights**:
   - Understanding of object detection basics
   - Experience with Unity Sentis implementation
   - Deep dive into Meta Scene Understanding SDK
   - Integration challenges between different systems
   - Importance of proper scene understanding

## Contributing
This is a prototype project. While contributions are welcome, please note that this is primarily a learning exercise and may not be actively maintained.

## Contributions/Assets Used

This project utilizes the following third-party assets:

- 3D Models
   - [Question 3D icon](https://sketchfab.com/3d-models/question-3d-icon-ba8c685715a849fab6f289a2469d1567)
   - [Exclamation Point](https://sketchfab.com/3d-models/exclamation-point-8161d30cfabe446dae1fabfb920b0f58)

- Icons
   - [Baby safety icons created by Iconjam - Flaticon](https://www.flaticon.com/free-icons/baby-safety)

- Sounds
   - [FreeSound.org's user IronCross32 audio samples for activating/deactiving things](https://freesound.org/people/ironcross32/)

- Packages
   - [Naughty Attributes (free on Unity's Asset Store)](https://assetstore.unity.com/packages/tools/utilities/naughtyattributes-129996)
   - [Groq-Unity Integration by Lucas Martinic](https://github.com/lucas-martinic/Groq-Unity)
   

## Personal Motivation and Notes
I wanted to make it an AI enabled “feature”, not an app per se. 
So I used the Microphone to call our AI assistant and say:

- On: Babyproof Start, Activate, Turn on, enable, launch, begin
- Off: Stop, turn off, disable, cancel, shutdown, close (do not use deactivate)

I didn’t implement a LLM on this project due to lack of time - right now, it is just a speech to text with keyword matching.

This AI assistant bit was not included in the video because I thought it was not the main feature, just a nice to have (also 20 seconds is very short, and I wanted to end the video with Emilia somehow).

I created a Debug version - where you see all the buttons - it is cheaper for everyone (since you are not using my local Whisper Transcriber at all times).

Feel free to use any version,
Thank you!
Tiago & Baby Emilia


## License
MIT License