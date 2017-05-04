using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tribot
{
    /// <summary>
    /// FsmSystem interface used by FsmRunner
    /// </summary>
    public interface IFsmSystem
    {
        void UpdateState();
        bool IsActive { get; }
    }

    /// <summary>
    /// A Finite State Machine that supports both monobehaviour states and normal states
    /// Working with the state manager requires to set up different state objects that derive from FsmState
    /// These FsmStates have to be added to the state machine and can then transition to new states by itself when required using a delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FsmSystem<T> : IFsmSystem where T : struct
    {
        private Dictionary<T, FsmState<T>> _states;

        private FsmState<T> _activeState
        {
            get { return _stack.Count == 0 ? null : _stack[_stack.Count - 1]; }
            set
            {
                if (_stack.Count == 0)
                    throw new Exception("No states on stack.");
                else
                {
                    _stack[_stack.Count - 1] = value;
                }
            }
        }
        private List<FsmState<T>> _stack;

        /// <summary>
        /// Use the regular initialize if the update component is not required
        /// </summary>
        public FsmSystem()
        {
            _states = new Dictionary<T, FsmState<T>>();
            _stack = new List<FsmState<T>>();
        }

        public bool IsActive
        {
            get { return (_activeState != null); }
        }

        /// <summary>
        /// Use this initialize if an update component is required
        /// </summary>
        /// <param name="component"></param>
        /// <param name="startState"></param>
        /// <returns></returns>
        public static FsmSystem<T> Initialize(MonoBehaviour component)
        {
            var runner = component.GetComponent<FsmRunner>();
            if (!runner) runner = component.gameObject.AddComponent<FsmRunner>();

            return runner.Initialize<T>(component);
        }

        /// <summary>
        /// Adds a state to the state map and prepares the added state for usage
        /// </summary>
        /// <param name="state"></param>
        /// <param name="type"></param>
        public void AddState(FsmState<T> state, T type)
        {
            if (_states.ContainsKey(type))
                _states[type] = state;
            else
                _states.Add(type, state);
            state.TransitionEvent = Transition;
            state.PushEvent = Push;
            state.PopEvent = Pop;
            state.InitState();
        }

        /// <summary>
        /// Transitions the current activated state to a new state
        /// </summary>
        /// <param name="state"></param>
        public void Transition(T state)
        {
            if (!_states.ContainsKey(state))
                throw new Exception("State with key " + state.ToString() + " not found");

            if (_activeState != null)
            {
                _activeState.ExitState();
                _activeState = _states[state];
                _activeState.EnterState();
            }
            else
            {
                Push(state);
            }
        }

        /// <summary>
        /// Pushes a new state on top of the stack
        /// </summary>
        /// <param name="state"></param>
        public void Push(T state)
        {
            if (!_states.ContainsKey(state))
                throw new Exception("State with key " + state.ToString() + " not found");

            if (_activeState != null)
            {
                _activeState.ExitState();
            }

            _stack.Add(_states[state]);
            _activeState.EnterState();
        }

        /// <summary>
        /// Pops the last state of the stack and activates the next one
        /// </summary>
        public void Pop()
        {
            if (_activeState == null)
                return;

            _activeState.ExitState();
            _stack.Remove(_activeState);

            if (_activeState != null)
                _activeState.EnterState();
        }

        /// <summary>
        /// Updates the active state
        /// </summary>
        public void UpdateState()
        {
            if (IsActive && _activeState.IsActive)
                _activeState.UpdateState();
        }
    }

    /// <summary>
    /// Get created when using the static initialize of FSM, this class runs the update method for the FsmSystem
    /// </summary>
    public class FsmRunner : MonoBehaviour
    {
        private IFsmSystem _fsm;

        public FsmSystem<T> Initialize<T>(MonoBehaviour component) where T : struct
        {
            var fsm = new FsmSystem<T>();
            _fsm = fsm;

            return fsm;
        }

        /// <summary>
        /// Updates the active state machine
        /// </summary>
        void Update()
        {
            if (_fsm.IsActive)
                _fsm.UpdateState();
        }
    }

    public interface FsmState<T> where T : struct
    {
        FsmAction<T> TransitionEvent { set; }
        FsmAction<T> PushEvent { set; }
        Action PopEvent { set; }
        bool IsActive { get; set; }

        /// <summary>
        /// Gets executed when a state is added to the FsmSystem state map
        /// </summary>
        /// <param name="InitEvent"></param>
        void InitState(Action InitEvent = null);

        /// <summary>
        /// Gets executed when the state is activated
        /// </summary>
        /// <param name="EnterEvent"></param>
        void EnterState(Action EnterEvent = null);

        /// <summary>
        /// Updates every frame when active
        /// </summary>
        void UpdateState();

        /// <summary>
        /// Gets triggered by transition, should disable the state/object
        /// </summary>
        /// <param name="ExitEvent"></param>
        void ExitState(Action ExitEvent = null);
    }

    public delegate void FsmAction<T>(T state) where T : struct;
}
