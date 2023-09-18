using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DEMSoft.Drawing;
using DEMSoft.NURBS;
using DEMSoft.Plot;
using DEMSoft.IGA;
using System.Drawing;
using DEMSoft.EngineeringData;
using DEMSoft.Common;
using DEMSoft.Function;
using System.Drawing.Printing;
using System.CodeDom.Compiler;
using System.Diagnostics;

namespace SampleTesting
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            double r = 1;
            double a = 4;

            ViewerForm viewer = new ViewerForm(true);
            List<NURBSSurface> listSurfaces = new List<NURBSSurface>();            // Upper part

            NURBSSurface[] sur1 = GeometryCreator.CreateSquareWithHoleFourNURBSSurfaces(4, 6, r, a);
            NURBSSurface sur2 = GeometryCreator.CreateRectangleNURBSSurface(0, 4, 2, 8);


            // danh sach cac surface
            List<NURBSSurface> listSurface = new List<NURBSSurface>();

            // Create half Rect surface with Circle Hole
            int p0 = 2;
            double[,] w = { { 1, 1, 1 }, { 1, 1, 1 }, {1, 1, 1 }, { 1, 1, 1 } };
            double[] kv0 = { 0, 0, 0, 0.5, 1, 1, 1 };
            double[] kv1 = { 0, 0, 0, 1, 1, 1 };

            ControlPoint[,] cps0 = new ControlPoint[4, 3];

            cps0[0, 0] = new ControlPoint(-4, 0);
            cps0[0, 1] = new ControlPoint(-6, 0);
            cps0[0, 2] = new ControlPoint(-8, 0);

            cps0[1, 0] = new ControlPoint(-4, 4 * Math.Sqrt(2) - 4);
            cps0[1, 1] = new ControlPoint(-6, 3);
            cps0[1, 2] = new ControlPoint(-8, 8);

            cps0[2, 0] = new ControlPoint(4 - 4 * Math.Sqrt(2), 4);
            cps0[2, 1] = new ControlPoint(-3, 6);
            cps0[2, 2] = new ControlPoint(-8, 8);

            cps0[3, 0] = new ControlPoint(0, 4);
            cps0[3, 1] = new ControlPoint(0, 6);
            cps0[3, 2] = new ControlPoint(0, 8);

            NURBSSurface sur3 = new NURBSSurface(p0, kv0, p0, kv1, cps0, w);

            NURBSSurface[] sur4 = new NURBSSurface[sur1.Length];

            for (int i = 0; i < sur1.Length; i++)
            {
                ActionOnGeometry act1 = new ActionOnGeometry(sur1[i]);
                act1.AddAction(ActionType.ReflectionXZ, 0);
                sur4[i] = (NURBSSurface)act1.Do();
            }

            ActionOnGeometry act2 = new ActionOnGeometry(sur2);
            act2.AddAction(ActionType.ReflectionXZ, 0);
            NURBSSurface sur5 = (NURBSSurface)act2.Do();

            ActionOnGeometry act3 = new ActionOnGeometry(sur3);
            act3.AddAction(ActionType.ReflectionXZ, 0);
            NURBSSurface sur6 = (NURBSSurface)act3.Do();

            // quan ly listsurface
            for (int i = 0; i < sur1.Length; i++)
            {
                listSurface.Add(sur1[i]); //patch 0-3
            }
            listSurface.Add(sur2); //patch 4
            listSurface.Add(sur3); //patch 5
            for (int i = 0; i < sur4.Length; i++)
            {
                listSurface.Add(sur4[i]); //patch 6-9
            }
            listSurface.Add(sur5); //patch 10
            listSurface.Add(sur6); //patch 11


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

            ////NURBSCurve curve0 = (NURBSCurve)listSurfaces[1].GetCurve(1);
            ////curve0.Draw(viewer);
            //s[0].Draw(viewer);
            //s[1].Draw(viewer);
            //viewer.UpdateCamera();
            //viewer.Run();


            //// --------------------Khai bao vat lieu--------------------

            Material steel = new Material("steel");
            steel.AddProperty(new IsotropicElasticity(
                PairOfIsotropicElasticity.YoungModulusAndPoissonRatio, 1000, 0.3));
            steel.TypeMaterialStructure = TypeMaterialStructure.Elasticity;


            ModelStructureStatic model = new ModelStructureStatic(Dimension.Plane);
            // dinh nghia trang thai bai toan
            model.StressState = Structure2DState.PlaneStress;
            model.AddMaterial(steel);
            model.AddComputeResult(Result.SIGMAXX, Result.SIGMAYY, Result.SIGMAEQV);

            for (int i = 0; i < listSurface.Count(); i++)
            {
                model.AddPatch(listSurface[i]);
                model.AttachMaterialToPatch(0, i);
                model.SetThicknessPlate(5, i);
            }

            model.SetAutomaticInterfaceAllPatch(true, false);

            //// them dai luong can tinh 
            //model.AddComputeResult(Result.SIGMAXX, Result.SIGMAYY, Result.SIGMAEQV);

            ////apply constraint
            //// clamped edge, patch 6-9
            ConstraintValueEdge2D cX5 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(6), 1, 0, new NullFunctionRToR(), 0);
            ConstraintValueEdge2D cY5 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(6), 1, 1, new NullFunctionRToR(), 0);
            ConstraintValueEdge2D cX6 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(7), 1, 0, new NullFunctionRToR(), 0);
            ConstraintValueEdge2D cY6 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(7), 1, 1, new NullFunctionRToR(), 0);
            ConstraintValueEdge2D cX7 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(8), 1, 0, new NullFunctionRToR(), 0);
            ConstraintValueEdge2D cY7 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(8), 1, 1, new NullFunctionRToR(), 0);
            ConstraintValueEdge2D cX8 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(9), 1, 0, new NullFunctionRToR(), 0);
            ConstraintValueEdge2D cY8 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(9), 1, 1, new NullFunctionRToR(), 0);


            //// displacement edges, patch 6-9
            double du = 0.1;
            ConstraintValueEdge2D cY0 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(0), 1, 1, new ConstantFunctionRToR(1.0), du);
            ConstraintValueEdge2D cY1 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(1), 1, 1, new ConstantFunctionRToR(1.0), du);
            ConstraintValueEdge2D cY2 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(2), 1, 1, new ConstantFunctionRToR(1.0), du);
            ConstraintValueEdge2D cY3 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(3), 1, 1, new ConstantFunctionRToR(1.0), du);
            ConstraintValueEdge2D cX0 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(0), 1, 0, new ConstantFunctionRToR(1.0), 0);
            ConstraintValueEdge2D cX1 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(1), 1, 0, new ConstantFunctionRToR(1.0), 0);
            ConstraintValueEdge2D cX2 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(2), 1, 0, new ConstantFunctionRToR(1.0), 0);
            ConstraintValueEdge2D cX3 = new ConstraintValueEdge2D(
                (PatchStructure2D)model.GetPatch(3), 1, 0, new ConstantFunctionRToR(1.0), 0);
            model.AddConstraint(cX5, cX6, cX7, cX8, cY5, cY6, cY7, cY8,
                cY0, cY1, cY2, cY3, cX0, cX1, cX2, cX3);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            model.IsSaveStepByStep = false;
            //model.NumberOfStepStorageNonlinear = 10;
            AbstractModel.IsParallelProcesing = true;

            model.InitializePatch();
            model.PreProcessing();
            model.Solve();
            model.PostProcessing();

            sw.Stop();
            double t = sw.Elapsed.TotalMilliseconds;
            model.Extrapolation(Result.SIGMAXX, Result.SIGMAYY, Result.SIGMAEQV);

            viewer = new ViewerForm(true);
            model.DrawResult(Result.SIGMAEQV, viewer, 20, false, 20);
            //model.DrawResult(Result.USUM, viewer, 5, false, 1);

            viewer.UpdateCamera();
            viewer.Run();
        }
    }
}
