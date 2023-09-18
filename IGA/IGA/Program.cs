using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DEMSoft.Common;
using DEMSoft.Drawing;
using DEMSoft.EngineeringData;
using DEMSoft.Function;
using DEMSoft.IGA;
using DEMSoft.NURBS;
using DEMSoft.Plot;
using System.Diagnostics;
using System.Drawing;

namespace IGA
{
    internal static class Program
    {
        static void Main()
        {
            double r = 1;
            double a = 4;
            
            ViewerForm viewer = new ViewerForm(true);
            List<NURBSSurface> listSurface = new List<NURBSSurface>();

            NURBSSurface[] sur1 = GeometryCreator.CreateSquareWithHoleEightNURBSSurfaces(4,6,r,a);

            //int p0 = 2;
            //double[,] w = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            //double[] kv0 = { 0, 0, 0, 0.5, 1, 1, 1 };
            for (int i = 0; i < sur1.Length; i++)
            {
                listSurface.Add(sur1[i]); //patch 0-3
            }

            foreach (NURBSSurface surface in listSurface)
            {
                surface.SetDegreeOnAllDirections(2);
                surface.isColorfulFace = false;
                surface.colorControlNet = Color.Blue;
                surface.colorSurface = Color.Green;
                surface.colorKnot = Color.Red;
                surface.colorCurve = Color.Brown;
                surface.isDrawOriginal = false;
                surface.colorControlPoint = Color.Brown;
                surface.Draw(viewer);
            }
            viewer.UpdateCamera();
            viewer.Run();
        }
    }
}
