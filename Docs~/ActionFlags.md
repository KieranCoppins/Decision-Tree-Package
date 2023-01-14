# Action Flags
Action flags determine how an action should be executed.

There are three different flag types:
- AsyncAction - Async actions can run at the same time as other Async actions in the action manager. If an async action is being performed, and then a new async action is queued, it will run the async action rather than weight for the current action to finish.
- Interruptor - If an interruptor action is queued and the current action playing is interruptable, it will stop the current action and start the interruptor.
- Interruptable - An interruptable action can be interrupted by and interruptor.

By default actions are created as interruptable.