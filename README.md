# SpaceShooter

(Im sorry david I do not know how to add snapshots to the readme)
But if you're intereseted in diffing releases i recommend diffing between PerObjectCollisionSystem and OptimizedCollisionSystem and checking the polygon collider and collider system scripts.


As with all naive 
implementations of a game, all my game objects were instantiated and destroyed on demand. So my first order of business was too pool these objects to prevent massive garbage collection drops!

These screenshots are all tested on 5000 objects:

Here is the performance before pooling.
Unfortunatelly this screenshot is quite bad and doesn't intuitively show what is going on. But the dips you see are actually peeks. During those dips, the game was not running in the editor.
![cbb894ba3b3d9726847719f7fb49d22a](https://github.com/TheoSanden/SpaceShooter/assets/26997845/e7164608-de41-4500-ad5d-b74333dc89d7)


Here is the performance after pooling.
Adding the pooling actually caused the game to dip bellow 20 fps. As i dequeue and enquqe objects from the pool i set the gameobject's active state to either true or false. This causes,
for example, unitys colliders to recalculate which then causes that spike. There was just too much overhead. So while we got rid of the garbage collection spikes, we instead created a new issue.

![13972fc5005d7ee0acde4a489e585078-1](https://github.com/TheoSanden/SpaceShooter/assets/26997845/aad4964d-eff7-4d5a-a1ed-5b92de9dbe7c)

Here is the performance after pooling and after refactoring deactivation.
Now instead of deactivating the gameobject, i call a special deactivation function with in the spawned objects script. This only deactivates necessary scripts and leaves the gamepbject active, it also just turns of simulation for the colliders so that it doesn't recalculate.
![4774e9e372cdd2a6e93bdecc72cabe7b](https://github.com/TheoSanden/SpaceShooter/assets/26997845/64cf004e-7919-4f0f-89f8-cccea4beca93)


In all of the performance testing below, i could NOT leave the colliders active since they would use up so much cpu usage that the game couldn't run. Having the profiler open led to the game not even starting.
This is what it looked like with just a few hundred units (the reason for this was that i spawned all object overlapping eachother so they all fired on collision events with eachother):
![PreNewCollisionSystem](https://github.com/TheoSanden/SpaceShooter/assets/26997845/36de8cec-bb8e-43b2-ab84-48de111ca8dd)


So the next logical step was to make my own collision system that removes all of the overhead of unity's collision system and only leaves onlt the barebones collision checking.
I wanted to use polygon colliders so i created a naive solution where i just run through all points of the other collider and check if they're inside the collider I'm processing. And i did this purely in monobehaviour.

And.....
![NewCollisionSystemWithHalfAmountOfUnits](https://github.com/TheoSanden/SpaceShooter/assets/26997845/802c9ded-96d9-49f0-adc6-1a7e4e8c342f)
Womp womp. It's as bad as before.

One of the main problems is that each collider checks itself against all the other colliders which results in an algorithm speed of N^2. Increadibly unscalable.

Next i tried implementing layers, so that only colliders in a certain layer checks against colliders in another layer.
Now only the lasers check against the ships so that we can reduce the amount of collision checks!
![CollisionSystemWithLayers](https://github.com/TheoSanden/SpaceShooter/assets/26997845/8a211713-da4a-48cb-8b1d-6b43ab5541e3)


And this looks better however im pretty sure that i was only checking a few hundred objects here. And as with the previous version, the performance is based on lasers multiplied by the amount of ships. 
Here is the same system but with burst compilation enabled:
![CollisionSystemWithLayers_BurstCompilation](https://github.com/TheoSanden/SpaceShooter/assets/26997845/e1ba4f66-5882-4094-9851-0a5fd862c9bf)

Woho that actually looks pretty good. However we can definetally do better than a few hundred frames for a few hundred objects. So i started converting this System to dots and:

![CollisionSystemWithLayers_ConvertedToJob](https://github.com/TheoSanden/SpaceShooter/assets/26997845/bb298fcd-bd3b-422f-8d8f-d7e06d74d206)

BOO! The performance is now down to sixty fps. And if i remember correctly, we're talking around a hundred objects. I'll explain the reason further ahead why this version was so unperformant. 

At this point i felt done with DOTS. I had tried to get this to be more performant but the act of sorting colliders, creating jobs, allocating memory, convertin variables into job usable types (managed types?), and processing the jobs was less performant than just not doing it.

So i decided to take another approach. I decided to implement a spacial partitioning system, more specifically, a quadtree so that i could reduce the amont of calculations even more! If i implement this correctly i would increase the algorithm speed from N^2 to n Log(n) i believe.
I don't think i need to go that into detail about how it works. However, to make a long story short, a quadtree divides the screen space up into smaller chunks so that when you wanna check a collision around an object you just get the colliders in the surrounding chunks.

And this seemed promising:
![FindCollision_500](https://github.com/TheoSanden/SpaceShooter/assets/26997845/724a807d-555d-4269-8d8f-7bf57e16a80d)

However we hit the roof quite quickly with this system. In this version, i checked if two boundaries intersected, and i did it within the quadtree structure. I had moved away from polygon colliders at this point.
So i thought. Maybe i can make this more performant if i do the actual collision checking outside of the quadtree structure and covert it into dots jobs. 
This is the performance if i just find "potential collision" for a given collider.

![FindPotentialCollision_500](https://github.com/TheoSanden/SpaceShooter/assets/26997845/865cfd09-b294-4ca5-9bdd-87cf02b37834)
Seems to be performing more steadily as the number of colliders increase. So i did the same thing as I did before where i created a job for each collider and sent in the colliders it should check. Surely the multithreading is going to make this go lightning fast.

![FullyImplementedVeryUnscalable](https://github.com/TheoSanden/SpaceShooter/assets/26997845/343bbbae-0568-431c-94ea-10d39e02d1c6)
Oh. This might be one of the worst once yet. This is when one of my classmates said something that really made it click for me. He said that the way my code is structured is very object oriented where as dots, as it says in the name, should be data oriented. 

What i have basically been doing until now, is that i've created a job for each collider and let that check against other colliders. So each job has been handling a small amount of data, but a bunch of jobs have been created to encompass all collision checks, nullifying any performance gains what so ever.
In the last example this problem is even clearer. The quadtree reduces the amount of collisions that you have to check per object, but i still needed to create a job per collider, which means that each job was maybe only handling a few colliders. So the problem was not the amount of data, but the amount of jobs created to handle the data.
Up until this point I've only been using polygon colliders, which ment that i couldn't put all these calculations into one single job.The job system can't handle arrays of arrays since it doesn't know how to effectively lay that out in memory.
But now, becuase of the former switch to quadtrees i no longer used polygon colliders, but used normal 4 point box colliders instead, so i could put all of this data into one single job and voila:

![1000ObjInBuild](https://github.com/TheoSanden/SpaceShooter/assets/26997845/50f46cb2-daec-473b-95cd-21197f2bbb3f)

Around a thousand Fps in build.
This system uses a combination of some of the systems I've previously described. I no longer use Quadtrees since i found the structure to be incompatible with dots (but I'd probably find a way to make those two work in case i had more time).
It instead creates one Job for each collision layer active in the scene.Which in this case is two: laser -> planes, and planes -> player ship. This gets us around the problem of having ships collide with eachother. I don't want them to do it, so why should i check their collisions. 
I also only want the laser to be able to collide with the ships, and not the way around, because only the lasers are communicating with the ships right now. So in essence, I've made this system very specific and only calculate the collisions i care about. However this system is not very modular. If i want to add more types of colliders
or if i want to add new collisions layers it takes some time to set up and make sure that it's working.
(I used the IJobParallelFor structure to divide the calculations up more efficiently.)












