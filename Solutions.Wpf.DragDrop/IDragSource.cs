﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solutions.Wpf.DragDrop
{
    public interface IDragSource
    {
        void StartDrag(DragInfo dragInfo);
    }
}
