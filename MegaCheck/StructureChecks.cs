using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace MegaCheck
{
    class StructureChecks
    {

        public MainWindow.CheckItem CheckCouchOverrides(PlanSetup ps, string CouchInteriorName, double CouchInteriorHu, string CouchOuterName, double CouchOuterHu)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Density overrides";

            StructureSet structureSet = ps.StructureSet;

            bool couchInteriorExists = false;
            bool couchInteriorHuCorrect = false;

            bool couchOuterExists = false;
            bool couchOuterHuCorrect = false;


            // If structures exist
            if (structureSet != null)
            {
                foreach (var couch in structureSet.Structures.Where(x => x.Id.StartsWith(CouchInteriorName)))
                {
                    couchInteriorExists = true;

                    double hu = double.MaxValue;
                    couch.GetAssignedHU(out hu);
                    if (hu == CouchInteriorHu)
                        couchInteriorHuCorrect = true;
                }
                foreach (var couch in structureSet.Structures.Where(x => x.Id.StartsWith(CouchOuterName)))
                {
                    couchOuterExists = true;

                    double hu = double.MaxValue;
                    couch.GetAssignedHU(out hu);
                    if (hu == CouchOuterHu)
                        couchOuterHuCorrect = true;
                }

                if (couchInteriorExists && couchOuterExists)
                {
                    if (couchInteriorHuCorrect && couchOuterHuCorrect)
                    {
                        check.checkResult = MainWindow.Result.Pass;
                        check.checkDetail = "Couch structures present with correct overrides.";
                    }
                    else if (!couchInteriorHuCorrect || !couchOuterHuCorrect)
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "Couch structures present with incorrect override(s).";
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Couch structures not present or non-standard in name.";
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "No structures present in plan.";
            }
            return check;
        }

        public MainWindow.CheckItem CheckStructureOverrides(PlanSetup ps, string CouchInteriorName, string CouchOuterName)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Density overrides";

            StructureSet structureSet = ps.StructureSet;

            // If structures exist
            if (structureSet != null)
            {

                check.checkResult = MainWindow.Result.Info;
                double hu;
                var overrideValuePair = new List<KeyValuePair<string, double>>();

                foreach (var structure in structureSet.Structures.Where(x => x.GetAssignedHU(out hu) != false))
                {
                    if (!structure.Id.StartsWith(CouchInteriorName) && !structure.Id.StartsWith(CouchOuterName))
                    {
                        structure.GetAssignedHU(out hu);
                        overrideValuePair.Add(new KeyValuePair<string, double>(structure.Id, hu));
                    }
                }

                if (overrideValuePair.Count == 0)
                {
                    check.checkDetail = "No non-couch structures with density override.";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Info;
                    string strOverride = null;
                    if (overrideValuePair.Count == 1)
                        strOverride = String.Format("1 non-couch structure with density override:", overrideValuePair.Count());
                    else
                        strOverride = String.Format("{0} non-couch structures with density overrides:", overrideValuePair.Count());
                    foreach (var valpair in overrideValuePair)
                    {
                        strOverride += String.Format("\n• {0} [{1} HU]", valpair.Key, valpair.Value);
                    }
                    check.checkDetail = strOverride;
                }

            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "No structures present in plan.";
            }
            return check;
        }

        public MainWindow.CheckItem CheckUnusedStructures(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Unused Structures";

            StructureSet structureSet = ps.StructureSet;

            // If structures exist
            if (structureSet != null)
            {
                var emptyStructureList = new List<string>();

                foreach (Structure s in ps.StructureSet.Structures)
                {
                    if (s.IsEmpty)
                        emptyStructureList.Add(s.Id);
                }

                if (emptyStructureList.Count == 0)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "No unused structures";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Warn;
                    string emptyStr = null;
                    if (emptyStructureList.Count == 1)
                        emptyStr = "Empty structure:";
                    else
                        emptyStr = "Empty structures:";
                    foreach (var str in emptyStructureList)
                    {
                        emptyStr += String.Format("\n• {0}", str);
                    }
                    check.checkDetail = emptyStr;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "No structures present in plan.";
            }
            return check;
        }

        private bool IsStructureOutside(Structure s, int SlicesInImage, VVector DoseMin, VVector DoseMax)
        {
            bool isOutside = false;

            // Iterate through each slice of the current image set
            for (int z = 0; z < SlicesInImage; z++)
            {
                if (isOutside)
                    break;
                // Get the coordinates of all contours on the current image plane
                VVector[][] contoursOnPlane = s.GetContoursOnImagePlane(z);
                //if (contoursOnPlane.Length > 0)
                //   isEmpty = false;
                if (contoursOnPlane.GetLength(0) > 0)
                {
                    // Iterate through each contour in the current plane
                    foreach (VVector[] vecs in contoursOnPlane)
                    {
                        if (isOutside)
                        {
                            break;
                        }
                        // Iterate through the coordinates of the contour
                        foreach (VVector vec in vecs)
                        {
                            // Are z-coordinates greater or less than calc volume
                            if ((vec.z < DoseMin.z) || (vec.z > DoseMax.z))
                            {
                                isOutside = true;
                                //System.Windows.MessageBox.Show(String.Format("Structure {0} found outside at:\n{1}\n{2}\n{3}", s.Id, vec.x, vec.y, vec.z));
                                break;
                            }
                            // Are y-coordinates greater or less than calc volume
                            else if ((vec.y < DoseMin.y) || (vec.y > DoseMax.y))
                            {
                                isOutside = true;
                                //System.Windows.MessageBox.Show(String.Format("Structure {0} found outside at:\n{1}\n{2}\n{3}", s.Id, vec.x, vec.y, vec.z));
                                break;
                            }

                            // Are x-coordinates greater or less than calc volume
                            else if ((vec.x < DoseMin.x) || (vec.x > DoseMax.x))
                            {
                                isOutside = true;
                                //System.Windows.MessageBox.Show(String.Format("Structure {0} found outside at:\n{1}\n{2}\n{3}", s.Id, vec.x, vec.y, vec.z));
                                break;
                            }
                        }
                    }
                }
            }

            return isOutside;
        }

        public MainWindow.CheckItem CheckBodyStructuresOutsideDoseGrid(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Body Structures Within Dose Grid";

            if (ps.StructureSet != null)
            {
                // Get the StructureSet object for the current plan
                StructureSet ss = ps.StructureSet;

                if (ss.Structures.Where(x => x.DicomType == "EXTERNAL").Count() > 0)
                {

                    // Check if the plan has a dose object (i.e. dose has been computed)
                    if (ps.Dose != null)
                    {
                        // Get Dose object for current plan
                        Dose dose = ps.Dose;

                        if (ss.Image != null)
                        {
                            // Get number of slices on primary image
                            int slicesInImage = ss.Image.ZSize;

                            // Get dose grid dimensions, note that origin refers to the centre of
                            // the min(X,Y,Z) pixel so a 0.5 voxel offset needs to be applied

                            // Offset

                            double offset = 0.5;

                            // Get the origin of the dose grid in x,y,z
                            double doseMinX = dose.Origin.x - (offset * dose.XRes);
                            double doseMinY = dose.Origin.y - (offset * dose.YRes);
                            double doseMinZ = dose.Origin.z - (offset * dose.ZRes);
                            VVector doseMin = new VVector(doseMinX, doseMinY, doseMinZ);

                            //System.Windows.MessageBox.Show(String.Format("Dose origin:\nx: {0}\ny: {1}\nz: {1}", dose.Origin.x, dose.Origin.y, dose.Origin.z));

                            // Get the extent of the dose grid in x,y,z
                            double doseMaxX = doseMinX + dose.XSize * dose.XRes + (offset * dose.XRes);
                            double doseMaxY = doseMinY + dose.YSize * dose.YRes + (offset * dose.YRes);
                            double doseMaxZ = doseMinZ + dose.ZSize * dose.ZRes + (offset * dose.ZRes);
                            VVector doseMax = new VVector(doseMaxX, doseMaxY, doseMaxZ);

                            // Define a string to hold the list of structures outside the dosegrid
                            List<string> structOutside = new List<string>();

                            // Iterate through each structure in the current structure set
                            foreach (Structure structure in ss.Structures.Where(x => x.DicomType.Equals("EXTERNAL")))
                            {
                                if (!structure.IsEmpty)
                                {
                                    if (IsStructureOutside(structure, slicesInImage, doseMin, doseMax))
                                        structOutside.Add(structure.Id);
                                }
                            }
                            // If structres found outside the dose box
                            if (structOutside.Count > 0)
                            {
                                check.checkResult = MainWindow.Result.Fail;

                                string strDetail = "Body structures found outside dose box:";
                                foreach (var structure in structOutside)
                                    strDetail += "\n  " + structure;
                                check.checkDetail = strDetail;
                            }
                            // If no structures found outside the dose box
                            else
                            {
                                check.checkResult = MainWindow.Result.Pass;
                                check.checkDetail = "All body structures within dose box.";
                            }
                        }
                        // If a primary image was not found
                        else
                        {
                            check.checkResult = MainWindow.Result.Fail;
                            check.checkDetail = "Check cannot be performed without image associated with plan.";
                        }
                    }
                    // If a dose object was not found (i.e. dose has not been calculated)
                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "Check cannot be performed without calculated dose.";
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Body structures not present in plan.";
                }
            }
            // If no StructureSet object is found (i.e. no structure set associated with plan)
            else
            {
                check.checkResult = MainWindow.Result.Warn;
                check.checkDetail = "No structures present in plan.";
            }

            return check;
        }

        public MainWindow.CheckItem CheckStructuresOutsideDoseGrid(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Structures Outside Dose Grid";

            if (ps.StructureSet != null)
            {
                // Get the StructureSet object for the current plan
                StructureSet ss = ps.StructureSet;

                // Check if the plan has a dose object (i.e. dose has been computed)
                if (ps.Dose != null)
                {
                    // Get Dose object for current plan
                    Dose dose = ps.Dose;

                    if (ss.Image != null)
                    {
                        // Get number of slices on primary image
                        int slicesInImage = ss.Image.ZSize;

                        // Get dose grid dimensions, note that origin refers to the centre of
                        // the min(X,Y,Z) pixel so a 0.5 voxel offset needs to be applied

                        // Offset

                        double offset = 0.5;

                        // Get the origin of the dose grid in x,y,z
                        double doseMinX = dose.Origin.x - (offset * dose.XRes);
                        double doseMinY = dose.Origin.y - (offset * dose.YRes);
                        double doseMinZ = dose.Origin.z - (offset * dose.ZRes);
                        VVector doseMin = new VVector(doseMinX, doseMinY, doseMinZ);

                        //System.Windows.MessageBox.Show(String.Format("Dose origin:\nx: {0}\ny: {1}\nz: {1}", dose.Origin.x, dose.Origin.y, dose.Origin.z));

                        // Get the extent of the dose grid in x,y,z
                        double doseMaxX = doseMinX + dose.XSize * dose.XRes + (offset * dose.XRes);
                        double doseMaxY = doseMinY + dose.YSize * dose.YRes + (offset * dose.YRes);
                        double doseMaxZ = doseMinZ + dose.ZSize * dose.ZRes + (offset * dose.ZRes);
                        VVector doseMax = new VVector(doseMaxX, doseMaxY, doseMaxZ);

                        // Define a string to hold the list of structures outside the dosegrid
                        List<string> structOutside = new List<string>();

                        // Iterate through each structure in the current structure set
                        foreach (Structure structure in ss.Structures.Where(x => !x.Id.ToUpper().StartsWith("COUCH") && !x.Id.ToUpper().Contains("TOL") && !x.DicomType.Equals("EXTERNAL")))
                        {
                            if (!structure.IsEmpty)
                            {
                                if (IsStructureOutside(structure, slicesInImage, doseMin, doseMax))
                                    structOutside.Add(structure.Id);
                            }
                        }
                        // If structres found outside the dose box
                        if (structOutside.Count > 0)
                        {
                            check.checkResult = MainWindow.Result.Fail;
                            string strDetail = "Structures found outside dose box (excluding couch/tol. structures):";
                            foreach (var structure in structOutside)
                                strDetail += "\n  " + structure;
                            check.checkDetail = strDetail;
                        }
                        // If no structures found outside the dose box
                        else
                        {
                            check.checkResult = MainWindow.Result.Pass;
                            check.checkDetail = "No non-couch/tol. structures found outside dose box.";
                        }
                    }
                    // If a primary image was not found
                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "Check cannot be performed without image associated with plan.";
                    }
                }
                // If a dose object was not found (i.e. dose has not been calculated)
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Check cannot be performed without calculated dose.";
                }
            }
            // If no StructureSet object is found (i.e. no structure set associated with plan)
            else
            {
                check.checkResult = MainWindow.Result.Warn;
                check.checkDetail = "No structures present in plan.";
            }

            return check;
        }

        public MainWindow.CheckItem CheckBolusAssigned(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Bolus linked to each field";

            // If at least 1 beam energy is a non-SRS arc
            if (ps.Beams.Count(x => !x.Technique.Id.Equals("SRS ARC") && !x.IsSetupField) > 0)
            {
                if (ps.StructureSet.Structures.Count(x => x.DicomType.Equals("BOLUS")) > 0)
                {
                    var failBolusList = new List<Tuple<string, string>>();

                    foreach (Structure bolus in ps.StructureSet.Structures.Where(x => x.DicomType.Equals("BOLUS")))
                    {
                        foreach (Beam beam in ps.Beams.Where(x => !x.Technique.Id.Equals("SRS ARC") && !x.IsSetupField))
                        {
                            // If 'bolus' is not assigned to this beam
                            if (beam.Boluses.Count(b => b.Id.Equals(bolus.Id)) == 0)
                            {

                                failBolusList.Add(new Tuple<string, string>(beam.Id, bolus.Id));
                            }

                        }
                    }
                    if (failBolusList.Count() == 0)
                    {
                        check.checkResult = MainWindow.Result.Pass;
                        check.checkDetail = "Bolus structures assigned to all treatment fields.";
                    }
                    else
                    {
                        check.checkResult = MainWindow.Result.Warn;
                        string strDetail = "Bolus structures not assigned to all treatment fields:";
                        foreach (var failBolusItem in failBolusList)
                        {
                            strDetail += String.Format("\n  Bolus Structure: {0}, Field: {1}", failBolusItem.Item2, failBolusItem.Item1);
                        }
                        check.checkDetail = strDetail;
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.NA;
                    check.checkDetail = "No bolus structures beams present in plan";
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No photon beams present in plan";
            }

            return check;
        }
    }
}
