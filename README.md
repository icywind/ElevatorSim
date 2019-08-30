# ElevatorSim
## Requirements
Create a “Unity Elevator Simulator” for n elevator(s) that work between m floor(s).

choose any value of n and m to simulate as long as you choose some n >= 1 and m >= 2

Optional: allow us to select n and m before starting the simulation

Elevators can be requested from any floor.

Floors can be requested from any elevator.

Each elevator has its own elevator shaft.

Unity Project 2018.4 or newer

Project compiles and runs in editor on the PC, Mac, and Linux Standalone platform.

No additional external project dependencies. (we mostly use macbooks w/ Unity)


## To load and run in Unity 2018.

1. Open MainScene
2. Configure n and m as you wish in the Inspector for GameController (See controller.png).  I did not put boundary check on the input due to time constraints.
3. Request elevator from the left hand side button groups from any floor.  Note that you can not go Up any more on the top floor and Down at the bottom floor.
4. When stopped, an elevator will have green coating light, at this time you may open the elevator panel to control that particular elevator (see ElevatorPanel.png) 
5. Use "ESC" key on the keyboard to close the elevator panel.


## Capabilities:
- Theoretically this app supports any n >= 1 and m >= 2, although limited display is provided at the moment (static building and no scrolling).
- At still state, all elevators will move to the requesting floor
- At a stop, elevator allows operation on the internal panel for selection of floors
- Time pauses when elevator panel is opened. So there will be no movement at this state.
- Time resumes when elevator panel is closed.
- Floor stops insertion scenario #1 (e.g., elevator is going to floor 4 from 0; floor 2 make a request when elevator is at floor 1; then the elevator will stop at floor 2 first, then floor 4)
- Floor stops insertion scenario #2 (e.g., same as above, except when during the stop at floor 2, someone pressed floor 3 inside the elevator. then the elevator will go to floor 3 first then floor 4).
- At correct timing, if one elevator reaches the requesting floor, the other elevators will be notified to cancel.  However, if there is not other request pending on such other elevator instance, then that such elevator will continue movement to the requesting floor.
Floor button light (yellow) corresponds to requesting state.  When an elevator arrives for service, light goes out on that button.
