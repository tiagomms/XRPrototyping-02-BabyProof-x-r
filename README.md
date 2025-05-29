# BabyProofXR - XR Object Detection and Scene Understanding Prototype

## Quick Links 🔗
## 🎥 [Watch Demo Video](https://drive.google.com/file/d/1aU9OYlAb_z5OmiX8clfsm6wi6I74qJLe/view?usp=drive_link)
## 📱 [Download APK](https://drive.google.com/file/d/1CVoA_bwxz7JGS89kzdAMkReVcJxWQTKv/view?usp=drive_link)

## Overview 🎯
This project is the second one-week solo prototype developed for the XR Bootcamp XR Prototyping course (May-July 2025). 

I wanted parents of toddlers coming to new locations (or in their own homes) to know what areas they need to be more careful and what sort of easy adjustments they can do - that don't involve buying stuff. Since toddlers have a tendency to go about anywhere dangerous, it seemed an interesting challenge. For this first prototype, I am focusing on eminent danger (objects around that are hazardous) and in danger locations.

Again, in XR with camera access + AI we can start doing this sort of analysis.

In addition to this, I can tell my partner and my baby that I am working on important stuff. 😎

## Tech Stack
- **Unity**: 6000.0.39f1
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

- [Question 3D icon](https://sketchfab.com/3d-models/question-3d-icon-ba8c685715a849fab6f289a2469d1567)
- [Exclamation Point](https://sketchfab.com/3d-models/exclamation-point-8161d30cfabe446dae1fabfb920b0f58)

## License
MIT License