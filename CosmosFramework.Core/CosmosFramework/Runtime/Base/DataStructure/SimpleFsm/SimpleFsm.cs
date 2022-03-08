using System;
using System.Collections.Generic;
namespace Cosmos
{
    /// <summary>
    /// 简易的抽象有限状态机；
    /// 区别于FSM模块，此状态机是精简只含核心逻辑的状态机抽象；
    /// </summary>
    public class SimpleFsm<T> where T : class
    {
        SimpleFsmState<T> currentState;
        Dictionary<Type, SimpleFsmState<T>> typeStateDict
            = new Dictionary<Type, SimpleFsmState<T>>();
        public string FsmName { get; set; }
        public Type CurrentStateType { get { return currentState.GetType(); } }
        /// <summary>
        /// 拥有者；
        /// </summary>
        public T Handle { get; private set; }
        public int StateCount { get { return typeStateDict.Count; } }
        public void SetHandle(T handle)
        {
            Handle = handle;
        }
        public void SetDefaultState(Type stateType)
        {
            currentState = GetState(stateType);
            currentState.OnEnter(this);
        }
        public bool AddState(SimpleFsmState<T> state)
        {
            var type = state.GetType();
            if (typeStateDict.TryAdd(type, state))
            {
                state.OnInit(this);
                return true;
            }
            return false;
        }
        public void AddStates(params SimpleFsmState<T>[] states)
        {
            var length = states.Length;
            for (int i = 0; i < length; i++)
            {
                AddState(states[i]);
            }
        }
        public bool RemoveState(Type stateType)
        {
            if (typeStateDict.Remove(stateType, out var state))
            {
                state.OnDestroy(this);
                return true;
            }
            return false;
        }
        public bool HasState(Type stateType)
        {
            return typeStateDict.ContainsKey(stateType);
        }
        public SimpleFsmState<T> GetState(Type type)
        {
            return typeStateDict[type];
        }
        public void Refresh()
        {
            currentState?.OnUpdate(this);
        }
        public void ChangeState(Type stateType)
        {
            var state = GetState(stateType);
            if (state != null)
            {
                currentState?.OnExit(this);
                currentState = state;
                currentState.OnEnter(this);
            }
        }
    }
}
