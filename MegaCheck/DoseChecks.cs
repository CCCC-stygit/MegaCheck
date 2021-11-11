using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace MegaCheck
{
    class DoseChecks
    {



        // TODO: Add check if body is outside dosebox, look for body alone structure
        public MainWindow.CheckItem CheckConformityIndex(PlanSetup ps, double tol100, double tol50)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "SABR Conformity Index";


            if (ps.StructureSet != null)
            {

                if (ps.StructureSet.Structures.Count(x => x.DicomType.Equals("PTV") && x.Id.ToUpper().Contains("PTV")) != 0)
                {
                    if (ps.StructureSet.Structures.Count(x=>x.Id.ToUpper().Contains("BODY ALONE") || x.Id.ToUpper().Contains("BODYALONE")) !=0)
                    {
                        if (ps.Dose != null)
                        {

                            var ciList = new List<Tuple<string, double, double>>();

                            Structure body = ps.StructureSet.Structures.FirstOrDefault(x => x.Id.ToUpper().Contains("BODY ALONE") || x.Id.ToUpper().Contains("BODYALONE"));

                            foreach (Structure ptv in ps.StructureSet.Structures.Where(x => x.DicomType.Equals("PTV") && x.Id.ToUpper().StartsWith("PTV")))
                            {
                                DoseValue dose100 = new DoseValue(100, DoseValue.DoseUnit.Percent);
                                double v100 = ps.GetVolumeAtDose(body, dose100, VolumePresentation.AbsoluteCm3);
                                double ci100 = v100 / ptv.Volume;


                                DoseValue dose50 = new DoseValue(50, DoseValue.DoseUnit.Percent);
                                double v50 = ps.GetVolumeAtDose(body, dose50, VolumePresentation.AbsoluteCm3);
                                double ci50 = v50 / ptv.Volume;

                                ciList.Add(new Tuple<string, double, double>(ptv.Id, ci100, ci50));
                            }
                            bool fail = false;
                            string strDetail = "Conformity Index results:";

                            foreach (var ciItem in ciList)
                            {
                                strDetail += "\n  " + ciItem.Item1 +
                                    String.Format(", CI[100%]: {0:F2} (Tol: {1:F2})", ciItem.Item2, tol100) +
                                    String.Format(", CI[50%]: {0:F2} (Tol: {1:F2})", ciItem.Item3, tol50);
                                if (ciItem.Item2 > tol100)
                                    fail = true;
                                else if (ciItem.Item3 > tol50)
                                    fail = true;
                            }
                            if (fail)
                                check.checkResult = MainWindow.Result.Fail;
                            else
                                check.checkResult = MainWindow.Result.Pass;
                            check.checkDetail = strDetail;
                        }

                        else
                        {
                            check.checkResult = MainWindow.Result.Fail;
                            check.checkDetail = "Dose not calculated";
                        }
                    }
                    // Check if the plan has a dose object (i.e. dose has been computed)

                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "No Body Alone structure present in plan, could not calculate conformity index.";
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "No PTV structure present in plan";
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "No structures present in plan";
            }
            return check;
        }
    }
}