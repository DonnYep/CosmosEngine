﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.FSM
{
    /// <summary>
    /// 状态机池
    /// </summary>
    internal interface IFSMGroup:IRefreshable,IControllable
    {
        /// <summary>
        /// ms 轮询更新间隔
        /// </summary>
        int RefreshInterval { get;}
        List<FSMBase> FSMSet { get; }
        void AddFSM(FSMBase fsm);
        void SetRefreshInterval(int interval);
        void DestoryFSM(Predicate<FSMBase> predicate);
        void DestoryFSM(FSMBase fsm);
        void DestoryAllFSM();
        bool HasFSM(Predicate<FSMBase> predicate);
        bool HasFSM(FSMBase fsm);
        FSMBase GetFSM(Predicate<FSMBase> predicate);
        int GetFSMCount();
    }
}
