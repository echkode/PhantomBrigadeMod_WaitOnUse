# WaitOnUse

A library mod for [Phantom Brigade](https://braceyourselfgames.com/phantom-brigade/) to add an action to wait for an attack to finish.

It is compatible with game release version **1.3**. It works with both the Steam and Epic installs of the game. All library mods are fragile and susceptible to breakage whenever a new version is released.

This is an example mod of how to add a new action to the game. It adds a new config file for the action and implements two function interfaces that are used in the config file. See the [technical details](#technical-details) for which ones were implemented and why after the videos.

Here's a video demonstrating the action in game. When an attack action is placed in the combat timeline, a new button appears in the row of action buttons above the timeline. It is to the right of the standard wait action. When you press this button it will insert a wait that extends from the end of any run or wait action on the bottom track of the combat timeline to the end of the attack action in the top track. Once the wait is placed, the button for it is removed so that you can quickly place a run action after the wait.

<video controls src="https://github.com/echkode/PhantomBrigadeMod_WaitOnUse/assets/48565771/d971928d-981f-41df-a427-c3c004d5c08c">
  <p>Demonstrating the wait-on-use action. A run action is placed and an attack action is placed at the end of the run. The wait-on-use action button now appears and when it's clicked, a new wait action is placed that goes to the end of the attack action.</p>
</video>

In this video, a sniper moves and then waits for a target to come within range about two seconds later. That means there's a fairly large gap between the end of the run action and the start of the attack action. The wait-on-use action will span that gap and go to the end of the attack action.

<video controls src="https://github.com/echkode/PhantomBrigadeMod_WaitOnUse/assets/48565771/74253d09-71d1-42b2-bac8-490aaf28c9ef">
  <p>The wait-on-use action will span any gaps between the end of the last action on the bottom track and the start of the next action on the top track.</p>
</video>

## Technical Details

To add a new action, you first start with a ConfigOverride YAML file placed in `DataDecomposed\Combat\Actions`. For this mod, I started with the standard wait action config file and modified it slightly. The main things I added were two function interfaces, one to the `dataCore.functionsOnValidation` list and the other to the `dataCore.functionsOnCreation` list.

For the validation function, I implemented the `PhantomBrigade.Functions.ICombatActionValidationFunction` interface. My implementation scans the timeline for any attack actions within the right time interval.

For the creation function, I implemented the `PhantomBrigade.Functions.ICombatActionExecutionFunction` interface. This is where I change the duration of the wait action. The action has already been placed so I don't need to worry about its start time.
