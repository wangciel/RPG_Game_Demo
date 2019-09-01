COMP 313 P3 Reflection  
Jack Mills-Williams 300345107
Animal Role: Rabbit
Primary Project Responsibility: Programmer

Code Discussion:
Code that I worked on:
About half of the candle script
About half of the mainMenu script
Some of the wall-jumping code in playerController
All of the pushing/pulling code
All of the pause button code
Most of the checkpoint system code


Most Interesting Code/Code that I'm proud of :
Lines 529-551 of playerController.cs
This code is related to the pushing/pulling mechanic we added to the game, and while this code itself isn't particularly
noteworthy, I'm mentioning it here because it took a long time to figure out the right way to implement this mechanic.
I initially started trying to apply force to the objects to make them move every time the player moved but then that didn't
work so I tried some other approaches before finding that I could link the objects movement with the player's so that 
every time the player moved, the object would move with them. This was possible by attaching a RelativeJoint2D physics 
component to the object and linking that to the player, and setting the Rigidbody of the object to dynamic when the player
was interacting with it static otherwise. I'm proud of this code because it reminds me that I can problem solve solutions
with enough determination and that something good can come out of it. 


Learning Reflection:
From this course I have gained an insight into how games are developped and how the game might change and evolve from
its original concept. I have enjoyed working in a group of people who are all committed to achieving the same goal, producing
a quality game experience. After having no prior experience with the Unity Engine, I now have an understanding of how to use 
it to create content. I also have gained some experience with C# from developping our game. I feel more confident in my
ability as a programmer than I did at the beginning of the course, and look forward to developping my skills further in
future projects. 
The most important thing I will use from this project is the ability to work in a team to get things done, as most
likely in my future workplaces I will be required to work in a team with other people. Having been in a team for this
project and produced something from that is a valuable skill to have going forward and I hope I get to do something
like this again in the future.