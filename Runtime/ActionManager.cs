using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    private Queue<ActionPacket> _actionQueue = new Queue<ActionPacket>();
    private readonly List<Action> _currentActions = new List<Action>();
    private readonly List<Action> _runningActions = new List<Action>();

    private System.Action<Action> _onFinish;

    public bool ExecutingActions { get; private set; }

    private bool _waitForActions = false;

    private void Start()
    {
        ExecutingActions = false;
        _onFinish += (Action action) =>
        {
            // Remove this action from current actions
            _currentActions.Remove(action);
            _runningActions.Remove(action);

            // Check if we have any more actions in the current actions
            if (_currentActions.Count == 0)
            {
                _waitForActions = false;
                ExecutingActions = false;
            }
        };
    }

    /// <summary>
    /// Schedule an action to be executed inside the action manager
    /// </summary>
    /// <param name="action"></param>
    public void ScheduleAction(Action action)
    {
        // We need to check if our action is already in our queue
        foreach (ActionPacket a in _actionQueue)
        {
            if (a.Action == action)
                return;
        }

        if (action != null)
            _actionQueue.Enqueue(new ActionPacket(action));
    }

    public void Execute()
    {
        bool currentActionsChanged = false;
        bool acceptASyncActions = false;

        List<ActionPacket> tempList = new List<ActionPacket>(_actionQueue);

        // Remove any expired actions
        foreach (ActionPacket a in _actionQueue)
        {
            if (Time.time - a.Time > 2.0f)
            {
                tempList.Remove(a);
            }
        }

        _actionQueue = new Queue<ActionPacket>(tempList);

        if (_waitForActions)
            return;

        // First we want to see if we have any interruptor actions
        foreach (ActionPacket a in _actionQueue)
        {
            if ((a.Action.Flags & Action.ActionFlags.Interruptor) == Action.ActionFlags.Interruptor)
            {
                tempList = new List<ActionPacket>(_actionQueue);
                // If we have an interruptor clear all our actions and do this one
                _currentActions.Clear();
                _currentActions.Add(a.Action);
                tempList.Remove(a);
                _actionQueue = new Queue<ActionPacket>(tempList);
                currentActionsChanged = true;
                acceptASyncActions = (a.Action.Flags & Action.ActionFlags.SyncAction) == Action.ActionFlags.SyncAction;
                _waitForActions = (a.Action.Flags & Action.ActionFlags.Interruptable) != Action.ActionFlags.Interruptable;
                break;
            }
        }

        while (_actionQueue.Count > 0)
        {
            if (_currentActions.Count > 0)
            {
                Action action = _actionQueue.Peek().Action;
                if ((action.Flags & Action.ActionFlags.SyncAction) == Action.ActionFlags.SyncAction && acceptASyncActions)
                {
                    _currentActions.Add(_actionQueue.Dequeue().Action);
                    currentActionsChanged = true;
                    _waitForActions = (action.Flags & Action.ActionFlags.Interruptable) != Action.ActionFlags.Interruptable;
                }
                else
                    break;
            }
            else
            {
                Action action = _actionQueue.Dequeue().Action;
                _currentActions.Add(action);
                currentActionsChanged = true;
                acceptASyncActions = (action.Flags & Action.ActionFlags.SyncAction) == Action.ActionFlags.SyncAction;
                _waitForActions = (action.Flags & Action.ActionFlags.Interruptable) != Action.ActionFlags.Interruptable;
            }
        }
        if (currentActionsChanged)
            ExecuteActions();
    }


    protected void ExecuteActions()
    {
        StopAllCoroutines();    // they should already be stopped unless there is an interruptor
        foreach (var action in _runningActions)
        {
            action.NodeState = DecisionTreeNodeRunningState.Interrupted;
        }
        _runningActions.Clear();
        // Execute all actions in current actions
        foreach (var action in _currentActions)
        {
            ExecutingActions = true;
            StartCoroutine(ActionWrapper(action));
        }
    }

    private IEnumerator ActionWrapper(Action action)
    {
        bool running = true;
        _runningActions.Add(action);
        action.NodeState = DecisionTreeNodeRunningState.Running;
        IEnumerator e = action.Execute();
        while (running)
        {
            if (e != null && e.MoveNext())
                yield return e.Current;
            else
                running = false;
        }
        action.NodeState = DecisionTreeNodeRunningState.Finished;
        _onFinish(action);
    }
}

public readonly struct ActionPacket
{
    public readonly Action Action { get; }
    public readonly float Time { get; }

    public ActionPacket(Action action)
    {
        this.Action = action;
        Time = UnityEngine.Time.time;
    }
}
