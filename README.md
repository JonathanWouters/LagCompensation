# Lagcompensated hitbox system for Unity3D


## How to install
- Open the package manager (Window/Package Manager)
- Click on the + icon in the topleft
- Select Add package from git url
- Insert https://github.com/JonathanWouters/LagCompensation.git
- Click Add


## Getting started


### Hitbox World
For the system to work a HitboxWorld monobehaviour should be present in the scene.
This component should be updated every tick by the network loop.

HitboxWorld can be updated by saving the current world snapshot.

```    
public void NetworkUpdate(int currentFrame) 
{
	HitboxWorld.SaveFrame(currentFrame);
}
```

### Hitbox Body
Every entity that wants to make use of lagcompensated hitboxes needs a HitBoxbody component.
A hitbox body has a `radius` that should encompas all hitboxes assosiated with this entity.

A hitbox body has 2 lists of hitbox components one for boxes and one for spheres.
All hitboxes/spheres of the entity have to be filled in here.

### Hitbox Component & HitSphere Component
Hitbox Component and HitSphere Component are the actual hitboxes that will be used for lagcompensation.
They should be added to a hitbox body.

### Raycasts
To do a lagcompensated raycast use the Raycast method on Hitboxworld.
It takes in a frame and a ray.
The frame should be a frame in the past where the raycast should look at. 
This frame can not be more then 60 frames in the past. (This can be changed in WorldConst.cs if needed)

```
public void RaycastExample(int frame) 
{
	if(HitboxWorld.Raycast(frame, new Ray(Vector3.zero, Vector3.forward), out HitInfo info)) 
	{
		Debug.Log($"Hit Object {info.HitObject.name} HitPoint{info.Point} Distance{info.Distance}");
	}
	else
	{
		Debug.Log("Nothing was hit");
	}
}
```

### Profiling
For extra info while using the unity profiler add HITBOXES_PROFILING to the scripting define symbols.

