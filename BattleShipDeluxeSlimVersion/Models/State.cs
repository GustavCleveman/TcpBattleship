using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipDeluxeSlimVersion.Models
{
    public enum State
    {
        WaitingForGreet = 0,
        WaitingForStart = 1,
        Playing = 2,
        GameOver = 3
    }
}
