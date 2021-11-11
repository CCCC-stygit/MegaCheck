using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace MegaCheck
{
    class CollisionChecks
    {
        ApplicatorCollisionDimensions applicatorCollisionDimensions = new ApplicatorCollisionDimensions();
        const double applicatorLateralExtension = 42.0d; // 20 mm????
        const double applicatorFaceToIso = 40.0d; // 40 mm????


        // Constructor to initialise applicator collision dimensions
        public CollisionChecks()
        {

        }

        //public enum ApplicatorSize { A6, A10, A15, A20, A25 };

        public class ApplicatorCollisionDimensions
        {
            public double LateralSize(string applicatorSize)
            {
                switch (applicatorSize)
                {
                    case "A6":
                        return 50.0 / 2 + applicatorLateralExtension;
                    case "A10":
                        return 100.0 / 2 + applicatorLateralExtension;
                    case "A15":
                        return 150.0 / 2 + applicatorLateralExtension;
                    case "A20":
                        return 200.0 / 2 + applicatorLateralExtension;
                        // 25 x 25 actual size 334 x 334 mm
                    case "A25":
                        return 250.0 / 2 + applicatorLateralExtension;
                    default:
                        return -1.0d;
                }
            }
            public double FaceToIsoDistance()
            {
                return applicatorFaceToIso;
            }
        }


        public MainWindow.CheckItem CheckElectronCollision(PlanSetup ps, double warnDistInCM, double failDistInCM)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Electron Collsion Check";

            // If at least 1 beam energy is an electron energy and not a set up field
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("E") && !x.IsSetupField) > 0)

            {
                if (ps.StructureSet.Structures.Count(x => x.DicomType.Equals("EXTERNAL")) > 0)
                {
                    // Get BODY and CouchExterior contours
                    Structure s_Couch = null;
                    Structure s_Body = ps.StructureSet.Structures.Where(x=>x.DicomType.Equals("EXTERNAL")).First();
                    if (ps.StructureSet.Structures.Count(x => x.Id.Contains("CouchSurface")) > 0)
                        s_Couch = ps.StructureSet.Structures.Where(x => x.Id.Contains("CouchSurface")).First();

                    //Get origin
                    Point3D origin = new Point3D(ps.Beams.First().IsocenterPosition.x, ps.Beams.First().IsocenterPosition.y, ps.Beams.First().IsocenterPosition.z);

                    //double distance = s_Couch.MeshGeometry.Positions.Select(x => (x - origin).Length).Min();// / 10.0f;
                    //Point3D FaceCenter = new Point3D();
                    Point3D TestPoint = new Point3D();



                    var minClearList = new List<Tuple<string, double>>();

                    foreach (var beam in ps.Beams.Where(x => x.EnergyModeDisplayName.Contains("E") && !x.IsSetupField))
                    {
                        double minClearTmt = 99999;
                        double tableangle = beam.ControlPoints.First().PatientSupportAngle <= 180.0f ? (Math.PI / 180.0f) * beam.ControlPoints.First().PatientSupportAngle : (Math.PI / 180.0f) * (beam.ControlPoints.First().PatientSupportAngle - 360.0f);
                        double gantryangle = beam.ControlPoints.First().GantryAngle <= 180.0f ? beam.ControlPoints.First().GantryAngle : (beam.ControlPoints.First().GantryAngle - 360.0f);
                        double collimatorangle = beam.ControlPoints.First().CollimatorAngle <= 180.0f ? beam.ControlPoints.First().CollimatorAngle : (beam.ControlPoints.First().CollimatorAngle - 360.0f);
                        // Convert to radians
                        gantryangle = gantryangle * (Math.PI / 180.0d);
                        tableangle = tableangle * (Math.PI / 180.0d);
                        collimatorangle = collimatorangle * (Math.PI / 180.0d);

                        double xPosMin = -applicatorCollisionDimensions.LateralSize(beam.Applicator.Id);
                        double xPosMax = -xPosMin;

                        double yPosMin = xPosMin;
                        double yPosMax = xPosMax;

                        double increment = 5.0d; // mm

                        double xPos = xPosMin;
                        double yPos = yPosMin;

                        double f = applicatorCollisionDimensions.FaceToIsoDistance();

                        double distance = 99999.0d;

                        bool xLimitReached = false;
                        bool yLimitReached = false;
                        do
                        {
                            if (xPos > xPosMax)
                            {
                                xPos = xPosMax;
                                xLimitReached = true;
                            }
                            do
                            {
                                if (yPos > yPosMax)
                                {
                                    yPos = yPosMax;
                                    yLimitReached = true;
                                }

                                double A = Math.Cos(gantryangle) * xPos * Math.Cos(collimatorangle)
                                    + yPos * Math.Sin(collimatorangle) - f;
                                double B = -xPos * Math.Sin(collimatorangle) + yPos * Math.Cos(collimatorangle);

                                TestPoint.X = Math.Cos(tableangle) * A + Math.Sin(tableangle) * B;
                                TestPoint.Y = Math.Sin(gantryangle) * (xPos * Math.Cos(collimatorangle) + yPos * Math.Sin(collimatorangle)) - f * Math.Cos(gantryangle);
                                TestPoint.Z = -Math.Sin(tableangle) * A + Math.Cos(tableangle) * B;

                                // Case for Col 0... need to add code for arbitrary col angle
                                /*
                                FaceCenter.X = origin.X
                                    + applicatorCollisionDimensions.FaceToIsoDistance() * Math.Cos(tableangle) * Math.Sin(gantryangle)
                                    + Math.Cos(gantryangle) * xPos;
                                FaceCenter.Y = origin.Y
                                    - applicatorCollisionDimensions.FaceToIsoDistance() * Math.Cos(gantryangle)
                                    + Math.Sin(gantryangle) * xPos;
                                FaceCenter.Z = origin.Z
                                    + applicatorCollisionDimensions.FaceToIsoDistance() * Math.Sin(tableangle) * Math.Sin(gantryangle)
                                    + yPos;
                                */
                                distance = s_Body.MeshGeometry.Positions.Select(x => (x - TestPoint).Length).Min() / 10.0f;

                                //System.Windows.MessageBox.Show(String.Format("Collimator angle: {0}", beam.ControlPoints.First().CollimatorAngle.ToString()));

                                if (distance < minClearTmt)
                                {
                                    minClearTmt = distance;
                                }
                                // If couch exists:
                                if (s_Couch != null)
                                {

                                    distance = s_Couch.MeshGeometry.Positions.Select(x => (x - TestPoint).Length).Min() / 10.0f;
                                    if (distance < minClearTmt)
                                    {
                                        minClearTmt = distance;
                                    }
                                }
                                // Increment y-position
                                yPos += increment;
                            } while (!yLimitReached);
                            // Reset y-position
                            yPos = yPosMin;
                            // Increment x-position
                            xPos += increment;
                        } while (!xLimitReached);

                        minClearList.Add(new Tuple<string, double>(beam.Id, minClearTmt));

                    }

                    bool warn = false;
                    bool fail = false;

                    string strClearance = null;
                    if (s_Couch == null)
                        strClearance += "WARNING: Couch unable to be found, not used in collision check.\n";
                    strClearance += "Minimum clearance of electron beams:";

                    foreach (var clearTuple in minClearList)
                    {
                        if (clearTuple.Item2 < failDistInCM)
                            fail = true;
                        else if (clearTuple.Item2 < warnDistInCM)
                            warn = true;
                        strClearance += String.Format("\n Field: {0}\tClearance: {1} cm",
                            clearTuple.Item1,
                            clearTuple.Item2.ToString("F1"));
                    }


                    if (fail)
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = strClearance;
                    }
                    else if (warn)
                    {
                        check.checkResult = MainWindow.Result.Warn;
                        check.checkDetail = strClearance;
                    }
                    else // Pass
                    {

                        check.checkResult = MainWindow.Result.Pass;
                        check.checkDetail = strClearance;
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Could not locate body structure";
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No electron beams present in plan";
            }

            return check;
        }

        // Add check to ensure ISOCENTRE is the same!!!
        public MainWindow.CheckItem CheckVMATCollision(PlanSetup ps, double gantryIncrement, double warnDistInCM, double failDistInCM)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "VMAT Collsion Check";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.MLCPlanType == MLCPlanType.VMAT) > 0)
            {
                // Get BODY and CouchExterior contours
                Structure s_Body = ps.StructureSet.Structures.Where(x => x.Id.Contains("BODY")).First();
                Structure s_Couch = ps.StructureSet.Structures.Where(x => x.Id.Contains("CouchSurface")).First();

                //Get origin
                Point3D origin = new Point3D(ps.Beams.First().IsocenterPosition.x, ps.Beams.First().IsocenterPosition.y, ps.Beams.First().IsocenterPosition.z);


                double distance = s_Couch.MeshGeometry.Positions.Select(x => (x - origin).Length).Min();// / 10.0f;

                Point3D FaceCenter = new Point3D();
                double gantryangle;
                double tableangle;

                double disttoface = 413.0d; ;//Assume distance from isocenter to the face of the collimator head is 35 cm. 


                var minClearList = new List<Tuple<string, double, double>>();


                double gantryMin = 9999;
                double gantryMax = -9999;

                double couchZeroGantryMin = 9999;
                double couchZeroGantryMax = -9999;

                foreach (var beam in ps.Beams.Where(x => x.MLCPlanType == MLCPlanType.VMAT))
                {

                    double minClearTmt = 99999;
                    double minGantryAngle = 9999;
                    tableangle = beam.ControlPoints.First().PatientSupportAngle <= 180.0f ? (Math.PI / 180.0f) * beam.ControlPoints.First().PatientSupportAngle : (Math.PI / 180.0f) * (beam.ControlPoints.First().PatientSupportAngle - 360.0f);
                    //string gantrystr = "Beam: " + curbeam.Id + "\nGantry Min: " + gantryMin + "\nGantry Max: " + gantryMax + "\nCouch Angle:" + tableangle;
                    //MessageBox.Show(gantrystr);
                    // If couch angle is zero, we can speed things up
                    if (tableangle == 0.0f)
                    {

                        // Determine if start of arc is the minimum or maximum gantry angle of plan
                        double gantrystart = beam.ControlPoints.First().GantryAngle <= 180.0f ? beam.ControlPoints.First().GantryAngle : (beam.ControlPoints.First().GantryAngle - 360.0f);
                        if (gantrystart < gantryMin)
                            gantryMin = gantrystart;
                        else if (gantrystart > gantryMax)
                            gantryMax = gantrystart;

                        // Determine if end of arc is the minimum or maximum gantry angle of plan
                        double gantryend = beam.ControlPoints.Last().GantryAngle <= 180.0f ? beam.ControlPoints.Last().GantryAngle : (beam.ControlPoints.Last().GantryAngle - 360.0f);
                        if (gantryend < gantryMin)
                            gantryMin = gantryend;
                        else if (gantryend > gantryMax)
                            gantryMax = gantryend;

                        //for (double i = gantryMin; i += 2; i < gantryMax)
                        double i = gantryMin;
                        bool gantryMaxReached = false;
                        while (gantryMaxReached == false)
                        {

                            // Convert gantry angle to radians
                            gantryangle = i * (Math.PI / 180.0f);
                            // Compute collision only if range has not been checked
                            if (!(gantryangle > couchZeroGantryMin && gantryangle < couchZeroGantryMax))
                            {
                                // If a setup field, find the complementary angle (i.e. detector is 180deg from source)
                                FaceCenter.X = origin.X + disttoface * Math.Sin(gantryangle);
                                FaceCenter.Y = origin.Y - disttoface * Math.Cos(gantryangle);
                                FaceCenter.Z = origin.Z;

                                distance = s_Body.MeshGeometry.Positions.Select(x => (x - FaceCenter).Length).Min() / 10.0f;

                                if (distance < minClearTmt)
                                {
                                    minClearTmt = distance;
                                    minGantryAngle = gantryangle;
                                }

                                // Only check couch collisions for the lower half of gantry rotation
                                if (s_Couch != null && gantryangle < 60 && gantryangle > 300) // Gantry angle between 90-270 (Varian scale)
                                {
                                    distance = s_Couch.MeshGeometry.Positions.Select(x => (x - FaceCenter).Length).Min() / 10.0f;

                                    if (distance < minClearTmt)
                                    {
                                        minClearTmt = distance;
                                        minGantryAngle = gantryangle;
                                    }
                                }

                                if (i >= gantryMax)
                                    gantryMaxReached = true;

                                i += gantryIncrement; // Degree increment
                                if (i > gantryMax)
                                    i = gantryMax;
                            }
                        }

                    }
                    else
                    {
                        // Determine if start of arc is the minimum or maximum gantry angle of plan
                        double gantrystart = beam.ControlPoints.First().GantryAngle <= 180.0f ? beam.ControlPoints.First().GantryAngle : (beam.ControlPoints.First().GantryAngle - 360.0f);
                        if (gantrystart < gantryMin)
                            gantryMin = gantrystart;
                        else if (gantrystart > gantryMax)
                            gantryMax = gantrystart;

                        // Determine if end of arc is the minimum or maximum gantry angle of plan
                        double gantryend = beam.ControlPoints.Last().GantryAngle <= 180.0f ? beam.ControlPoints.Last().GantryAngle : (beam.ControlPoints.Last().GantryAngle - 360.0f);
                        if (gantryend < gantryMin)
                            gantryMin = gantryend;
                        else if (gantryend > gantryMax)
                            gantryMax = gantryend;

                        //for (double i = gantryMin; i += 2; i < gantryMax)
                        double i = gantryMin;
                        bool gantryMaxReached = false;
                        while (gantryMaxReached == false)
                        {
                            gantryangle = i * (Math.PI / 180.0f);
                            // If a setup field, find the complementary angle (i.e. detector is 180deg from source)
                            FaceCenter.X = origin.X + disttoface * Math.Cos(tableangle) * Math.Sin(gantryangle);
                            FaceCenter.Y = origin.Y - disttoface * Math.Cos(gantryangle);
                            FaceCenter.Z = origin.Z + disttoface * Math.Sin(tableangle) * Math.Sin(gantryangle);

                            distance = s_Body.MeshGeometry.Positions.Select(x => (x - FaceCenter).Length).Min() / 10.0f;

                            if (distance < minClearTmt)
                            {
                                minClearTmt = distance;
                                minGantryAngle = gantryangle;
                            }

                            if (s_Couch != null)
                            {

                                distance = s_Couch.MeshGeometry.Positions.Select(x => (x - FaceCenter).Length).Min() / 10.0f;
                                if (distance < minClearTmt)
                                {
                                    minClearTmt = distance;
                                    minGantryAngle = gantryangle;
                                }
                            }
                            if (i >= gantryMax)
                                gantryMaxReached = true;

                            i += gantryIncrement; // Degree increment
                            if (i > gantryMax)
                                i = gantryMax;
                        }
                    }
                    minClearList.Add(new Tuple<string, double, double>(beam.Id, minClearTmt, minGantryAngle));
                }

                bool warn = false;
                bool fail = false;

                string strClearance = null;
                if (s_Couch == null)
                    strClearance += "WARNING: Couch unable to be found, not used in collision check.\n";
                strClearance += "Minimum clearance of VMAT arcs:";

                foreach (var clearTuple in minClearList)
                {
                    if (clearTuple.Item2 < failDistInCM)
                        fail = true;
                    else if (clearTuple.Item2 < warnDistInCM)
                        warn = true;
                    strClearance += String.Format("\n Field: {0}\tClearance: {1} cm [Gantry Angle: {2}°]",
                        clearTuple.Item1,
                        clearTuple.Item2.ToString("F1"),
                        //clearTuple.Item3);
                        clearTuple.Item3 >= 180 ? (540 - clearTuple.Item3).ToString("F1") : (180 - clearTuple.Item3).ToString("F1"));
                }


                if (fail)
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = strClearance;
                }
                else if (warn)
                {
                    check.checkResult = MainWindow.Result.Warn;
                    check.checkDetail = strClearance;
                }
                else // Pass
                {

                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = strClearance;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No VMAT beams present in plan";
            }

            return check;
        }

    }
}
