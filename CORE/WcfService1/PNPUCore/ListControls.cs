using PNPUCore.Controle;
using System.Collections.Generic;

namespace PNPUCore.Control
{
    class ListControls
    {
        internal static List<IControle> listOfMockControl = new List<IControle>() { new ControleMock(), new ControleMock()};
    }
}