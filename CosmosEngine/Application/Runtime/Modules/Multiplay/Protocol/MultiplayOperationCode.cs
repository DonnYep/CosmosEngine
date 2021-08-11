﻿using System.Collections;
using System.Collections.Generic;
namespace CosmosEngine
{
    public enum MultiplayOperationCode : byte
    {
        SYN = 73,
        PlayerEnter = 74,
        PlayerExit = 75,
        PlayerInput = 76,
        FIN = 77
    }
}