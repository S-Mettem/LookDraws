using System;
using System.Collections.Generic;
using System.Text;

namespace LookDraws
{
    public class Draw
    {
        public String path_to_draw { get; set; }
        public String name_of_draw { get; set; }
        public Draw(String ptd)
        {
            this.path_to_draw = ptd;
        }
    }
}
