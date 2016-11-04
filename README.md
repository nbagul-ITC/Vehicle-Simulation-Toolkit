## Vehicle Simulation Toolkit

This toolkit provides building blocks for the development of simulated driving scenarios. Two components included are:

1. Unity Vehicle Simulation Environment: Construct 3D environments with dynamic objects such as vehicles and pedestrians.
2. Driving Scenario Vehicle Visualization Tool: Visualize driving scenarios in a 3D world from simulated or recorded data.

## Unity Vehicle Simulation Environment

![alt text](https://cloud.githubusercontent.com/assets/3961167/19617456/c3bfd938-97e5-11e6-9c77-93d2f8955c61.gif)

Creating realistic vehicle simulation environments fundamentally requires many different dynamic objects that interact with each other. Common dynamic objects include environmental signals, adjacent vehicles, and pedestrians. The Unity Vehicle Simulation Environment provides the scripts needed to create realistic driving scenarios using Unity3D. It includes dynamic objects, such as cars and pedestrians, and road generation scripts.

Note: This package includes scripts which may be dependent on additional external libraries. The Unity Standard Assets are included in the project but additional assets may be required. The dependent scripts are kept in the same folder and commented out.

## Driving Scenario Vehicle Visualization Tool

This tool provides a means to visualize driving scenarios with from simulated or recorded data. It also captures images/videos from the scene at from any position desired. The tool also provides opportunities to explore computer vision algorithms by providing depth images and pixel-level labeled data sets.

1. Provide visual feedback about a driving scenario
2. Support research for creating large data sets of driving scenarios
3. Assess performance of vision algorithms

## Driving Scenario Vehicle Visualization API

![alt text](https://cloud.githubusercontent.com/assets/3961167/20024665/4e33e310-a2a5-11e6-90a8-c8a717990af4.gif)

This section provides details for the vehicle visualization API and explains how to provide data to the system in the appropriate format. All files are in JSON format and must contain the appropriate fields for the system. Files are placed in separate folders depending on the vehicle described. Check out the "Scenario Example" folder for a template.

### Vehicles

Vehicles use high quality detailed 3D models. Each vehicle in the scene must be described with a unique JSON file located in the folder. The format is the following

{
	"time": Array.of(float),
	"x": Array.of(float),
	"y": Array.of(float),
	"z": Array.of(float),
	"phi": Array.of(float),
	"psi": Array.of(float),
	"theta": Array.of(float)
}

### Environment

Environment can contain a variety of features: Roads, signs, buildings. Roads can be made by using lanes coordinates or a custom road builder. 

{
	"x": Array.of(float),
	"y": Array.of(float),
	"z": Array.of(float),
	"LaneWidth": float
}

## Interacting with the Scene

C - Change the position of the camera. There are three positions available - first person, third person, and third person orbit.
T - Toggle the target of the camera to another vehicle in the scene.
R - Reset the scene.
S - Stop the scene.
P - Play the scene if stopped.
Q - Quit.

Run the executable from the command prompt with a string containing the location of your scenario.

VehicleVisualizationTool.exe "...\Scenario Library\Scenario Example"