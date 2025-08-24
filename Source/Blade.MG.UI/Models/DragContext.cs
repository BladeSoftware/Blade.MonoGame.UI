using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blade.MG.UI.Models;
public class DragContext
{
    public Point DragStart { get; set; }
    public Point CurrentPoint { get; set; }
    public Point Delta { get; set; }

    //public int InitialOffset { get; set; }
}
