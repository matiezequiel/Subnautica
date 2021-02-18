using System;

namespace TGC.Group.Model
{
    class GameState
    {
        public Action Update { get; set; }
        public Action Render { get; set; }
    }
}
