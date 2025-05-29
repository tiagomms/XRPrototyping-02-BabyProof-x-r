# BabyProofXR - XR Object Detection and Scene Understanding Prototype

## Overview
BabyProofXR is a prototype that combines object detection with scene understanding to identify potentially dangerous objects to avoid toddlers getting hurt. The project integrates Unity Sentis for object detection with Meta's Scene Understanding (MRUK) to create a more contextual awareness of the environment.

## Tech Stack
- **Unity**: 6000.0.39f1
- **Meta XR SDK**: 
  - All-in-one SDK v76
  - Camera Access API
  - Scene Understanding and MRUK
- **AI/ML**: 
  - Unity Sentis
  - YoloV8 for object detection

## Key Features
- Real-time object detection using Unity Sentis and YoloV8
- Integration with Meta's Scene Understanding (MRUK)
- Camera access and environment raycasting
- Object filtering based on dangerous labels
- Scene anchor detection and labeling

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

## License
MIT License