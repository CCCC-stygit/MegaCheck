using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


using System.Windows;
using System.Windows.Controls;

//gitcheck

namespace MegaCheck
{

    public class BeamChecks
    {
        public MainWindow.CheckItem CheckPhotonDoseAlgorithm(PlanSetup ps, String expectedValue)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Photon Dose Algorithm";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && !x.Technique.Id.Equals("SRS ARC") && !x.IsSetupField) > 0)
            {
                if (ps.GetCalculationModel(CalculationType.PhotonVolumeDose) == expectedValue)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "Photon dose algorithm: " + expectedValue;
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Photon dose algorithm: " + ps.GetCalculationModel(CalculationType.PhotonVolumeDose)
                        + "; Expected photon dose algorithm: " + expectedValue;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No photon beams present in plan";
            }

            return check;
        }

        public MainWindow.CheckItem CheckPhotonDoseGridSize(PlanSetup ps, double expectedValueInCM, double expectedStereotacticValueinCM, double stereotacticDoseThresh)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Photon Dose Grid Size";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && !x.Technique.Id.Equals("SRS ARC") && !x.IsSetupField) > 0)
            {
                string gridSize;
                if (ps.GetCalculationOption(ps.GetCalculationModel(CalculationType.PhotonVolumeDose), "CalculationGridSizeInCM", out gridSize))
                {
                    ps.DoseValuePresentation = DoseValuePresentation.Absolute;
                    int fractions = ps.NumberOfFractions.Value;
                    double dosePerFraction = ps.TotalDose.Dose / fractions;

                    double expectedDose = expectedValueInCM;
                    if ((dosePerFraction >= stereotacticDoseThresh) && (fractions > 1))
                        expectedDose = expectedStereotacticValueinCM;

                    if (Math.Abs(Convert.ToDouble(gridSize) - expectedDose) < 0.0001)
                    {
                        check.checkResult = MainWindow.Result.Pass;
                        check.checkDetail = "Photon dose grid size: " + expectedDose + " cm";
                    }
                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "Photon dose grid size: " + gridSize
                            + " cm; Expected photon dose grid size: " + expectedDose + " cm";
                    }
                }
                else
                {

                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No photon beams present in plan";
            }

            return check;
        }

        public MainWindow.CheckItem CheckSrsDoseGridSize(PlanSetup ps, string ExpectedValue)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Conical SRS Dose Grid Size";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.Technique.Id.Equals("SRS ARC") && !x.IsSetupField) > 0)
            {

                string strGridSize;
                if (ps.GetCalculationOption(ps.GetCalculationModel(CalculationType.PhotonSRSDose), "Grid", out strGridSize))
                {
                    if (strGridSize == ExpectedValue)
                    {
                        check.checkResult = MainWindow.Result.Pass;
                        check.checkDetail = "Conical SRS dose grid set to " + ExpectedValue;
                    }
                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = String.Format("Conical SRS dose grid set to: {0}; expected setting: {1}", strGridSize, ExpectedValue);
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Could not determine conical SRS dose grid size.";
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No conical SRS beams present in plan";
            }

            return check;
        }

        public MainWindow.CheckItem CheckElectronDoseGridSize(PlanSetup ps, double expectedValueInCM)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Electron Dose Grid Size";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("E")) > 0)
            {
                KeyValuePair<string, string> gridSize = ps.ElectronCalculationOptions.Where(x => x.Key == "CalculationGridSizeInCM").FirstOrDefault();
                if (gridSize.Key == "CalculationGridSizeInCM")
                {
                    if (Math.Abs(Convert.ToDouble(gridSize.Value) - expectedValueInCM) < 0.0001)
                    {
                        check.checkResult = MainWindow.Result.Pass;
                        check.checkDetail = "Electron dose grid size: " + gridSize.Value + " cm";
                    }
                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "Electron dose grid size: " + gridSize.Value
                            + " cm; Expected electron dose grid size: " + expectedValueInCM + " cm";
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Could not get electron dose grid size";
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No electron beams present in plan";
            }

            return check;
        }

        public MainWindow.CheckItem CheckHeteogeneityCorr(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Heterogeneity Correction";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && !x.Technique.Id.Equals("SRS ARC") && !x.IsSetupField) > 0)
            {

                string hetCorr;
                if (ps.GetCalculationOption(ps.GetCalculationModel(CalculationType.PhotonVolumeDose), "HeterogeneityCorrection", out hetCorr))
                {
                    if (hetCorr == "ON")
                    {
                        check.checkResult = MainWindow.Result.Pass;
                        check.checkDetail = "Heterogeneity correction on";
                    }
                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "Heterogeneity correction off";
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Could not determine state of heterogeneity correction.";
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No photon beams present in plan";
            }

            return check;
        }

        public MainWindow.CheckItem CheckDoseRate(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Treatment Field Dose Rates";

            var failBeamList = new List<Tuple<string, int, int>>();

            // If at least 1 treatment beam
            if (ps.Beams.Count(x => !x.IsSetupField) > 0)
            {
                foreach (var beam in ps.Beams.Where(x => !x.IsSetupField))
                {
                    // If a FFF beam
                    if (beam.EnergyModeDisplayName.Contains("FFF"))
                    {
                        if (beam.DoseRate != 1400)
                        {
                            failBeamList.Add(new Tuple<string, int, int>(beam.Id, beam.DoseRate, 1400));
                        }
                    }
                    // If any other beam
                    else
                    {
                        if (beam.DoseRate != 600)
                        {
                            failBeamList.Add(new Tuple<string, int, int>(beam.Id, beam.DoseRate, 600));
                        }
                    }
                }
                if (failBeamList.Count == 0)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "All treatment field dose rates correct";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    string failStr = null;
                    if (failBeamList.Count == 1)
                        failStr = "Treatment field with incorrect dose rate:";
                    else
                        failStr = "Treatment fields with incorrect dose rate:";
                    
                    foreach (var failBeam in failBeamList)
                    {
                        failStr += String.Format("\n {0}, Dose Rate: {1} MU; Expected {2} MU],", failBeam.Item1, failBeam.Item2, failBeam.Item3);
                    }
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No treatment beams present in plan";
            }

            return check;
        }
        
        public MainWindow.CheckItem CheckImrtOptimizerAlgorithm(PlanSetup ps, String expectedValue)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "IMRT Optimiser Algorithm";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && x.MLCPlanType.Equals(MLCPlanType.DoseDynamic) && !x.IsSetupField) > 0)
            {
                if (ps.GetCalculationModel(CalculationType.PhotonIMRTOptimization) == expectedValue)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "IMRT optimiser algorithm: " + expectedValue;
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "IMRT optimiser algorithm: " + ps.GetCalculationModel(CalculationType.PhotonIMRTOptimization)
                        + "; Expected IMRT optimiser algorithm: " + expectedValue;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No IMRT beams present in plan";
            }

            return check;
        }

        public MainWindow.CheckItem CheckVmatOptimizerAlgorithm(PlanSetup ps, String expectedValue)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "VMAT Optimiser Algorithm";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && x.MLCPlanType.Equals(MLCPlanType.VMAT) && !x.IsSetupField) > 0)
            {
                if (ps.GetCalculationModel(CalculationType.PhotonVMATOptimization) == expectedValue)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "VMAT optimiser  algorithm: " + expectedValue;
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "VMAT optimiser algorithm: " + ps.GetCalculationModel(CalculationType.PhotonVMATOptimization)
                        + "; Expected VMAT optimiser algorithm: " + expectedValue;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No VMAT beams present in plan";
            }

            return check;
        }

        public MainWindow.CheckItem CheckConeDoseAlgorithm(PlanSetup ps, String expectedValue)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Conical SRS Dose Algorithm";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.Technique.Id.Equals("SRS ARC") && !x.IsSetupField) > 0)
            {
                if (ps.GetCalculationModel(CalculationType.PhotonSRSDose) == expectedValue)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "Conical SRS dose algorithm: " + expectedValue;
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Conical SRS dose algorithm: " + ps.GetCalculationModel(CalculationType.PhotonSRSDose)
                        + "; Expected Conical SRS dose algorithm: " + expectedValue;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No conical SRS beams present in plan";
            }

            return check;
        }

        public MainWindow.CheckItem CheckElectronDoseAlgorithm(PlanSetup ps, String expectedValue)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Electron Dose Algorithm";
            // If at least 1 beam energy con
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("E")) > 0)
            {
                if (ps.ElectronCalculationModel == expectedValue)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "Electron dose algorithm: " + expectedValue;
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Electron dose algorithm: " + ps.ElectronCalculationModel
                        + "; Expected electron dose algorithm: " + expectedValue;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No electron beams present in plan";
            }

            return check;
        }

        public MainWindow.CheckItem CheckTolTables(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Tolerance table consistent for all beams";

            if (ps.Beams != null)
            {
                List<string> tolList = new List<string>();
                foreach (var beam in ps.Beams)
                    tolList.Add(beam.ToleranceTableLabel);
                if (tolList.Distinct<string>().Count() == 1)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "All beam tolerance tables set to: " + tolList.First();
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    string detailString = "Not all beam tolerance tables consistent. Current tolerance tables:";
                    int i = 0;
                    foreach (string str in tolList)
                    {
                        if (i++ > 0)
                            detailString += ",";
                        detailString += str;
                    }
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "No beams present in plan.";
            }

            return check;
        }


        public MainWindow.CheckItem CheckMachine(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Machine Consistent For All Beams";

            if (ps.Beams != null)
            {
                var machList = new List<KeyValuePair<string, string>>();
                foreach (var beam in ps.Beams)
                    machList.Add(new KeyValuePair<string, string>(beam.Id, beam.TreatmentUnit.Id));
                if (machList.Select(m => m.Value).Distinct().Count() == 1)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "Machine set to " + machList.First().Value + " for all beams.";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    string detailString = "Machine not consistent for all beams. Current machines:";
                    
                    foreach (var mach in machList)
                    {
                        detailString += String.Format("\n {0}:\t{1}", mach.Key, mach.Value);
                    }
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "No beams present in plan.";
            }

            return check;
        }

        public MainWindow.CheckItem CheckBeamEnergies(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Beams All The Same Energy";

            if (ps.Beams != null)
            {
                var energyList = new List<KeyValuePair<string, string>>();
                foreach (var beam in ps.Beams)
                    energyList.Add(new KeyValuePair<string, string>(beam.Id, beam.EnergyModeDisplayName));
                if (energyList.Select(m => m.Value).Distinct().Count() == 1)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "Energy is " + energyList.First().Value + " for all beams.";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Warn;
                    string detailString = "Energy not the same for all beams. Current energies:";
                    
                    foreach (var energy in energyList)
                    {
                        detailString += String.Format("\n {0}:\t{1}", energy.Key, energy.Value);
                    }
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "No beams present in plan.";
            }

            return check;
        }

        public MainWindow.CheckItem CheckBeamIsocenters(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Isocenter Consistent For All Beams";

            if (ps.Beams != null)
            {
                var isoList = new List<KeyValuePair<string, VVector>>();
                foreach (var beam in ps.Beams)
                    isoList.Add(new KeyValuePair<string, VVector>(beam.Id, beam.IsocenterPosition));
                if (isoList.Select(m => m.Value).Distinct().Count() == 1)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "Isocenter is consistent for all beams.";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    string strDetail = "Isocenter not consistent for all beams:";
                    var distinctIsoList = isoList.Select(iso => iso.Value).Distinct();
                    int i = 1;
                    foreach (var isoVect in distinctIsoList)
                    {
                        strDetail += String.Format("\n  Isocenter {0} ({1}, {2}, {3}):", i++,
                            isoVect.x.ToString("F4"),
                            isoVect.y.ToString("F4"),
                            isoVect.z.ToString("F4"));
                        foreach (var iso in isoList.Where(iso => iso.Value.Equals(isoVect)))
                        {
                            strDetail += String.Format("\n    {0}", iso.Key);
                        }
                    }
                    check.checkDetail = strDetail;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "No beams present in plan.";
            }

            return check;
        }


        public MainWindow.CheckItem CheckBeamsNamed(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "All Treatment Fields Named";

            if (ps.Beams != null)
            {
                List<string> unnamedList = new List<string>();
                foreach (var beam in ps.Beams.Where(x => x.Name == "" && !x.IsSetupField))
                {
                    unnamedList.Add(beam.Id);
                }
                if (unnamedList.Count() == 0)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "All fields are named";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    string detailString = "Field IDs without field names:";
                    foreach (string str in unnamedList)
                    {
                        detailString += "\n " + str;
                    }
                    check.checkDetail = detailString;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "No beams present in plan.";
            }

            return check;
        }


        public MainWindow.CheckItem CheckSrsMuPerDeg(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Conical SRS MUs per Degree";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && x.Technique.Id.Equals("SRS ARC") && !x.IsSetupField) > 0)
            {
                var failBeamList = new List<KeyValuePair<string, double>>();

                foreach (var srsBeam in ps.Beams.Where(x => x.Technique.Id.Equals("SRS ARC")))
                {
                    if (srsBeam.Meterset.Value / srsBeam.ArcLength > 20.0d)
                    {
                        failBeamList.Add(new KeyValuePair<string, double>(srsBeam.Id, srsBeam.Meterset.Value / srsBeam.ArcLength));
                    }
                }
                if (failBeamList.Count == 0)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "All conical SRS beams < 20 MU/deg";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    string failStr = null;
                    if (failBeamList.Count == 1)
                        failStr = "Conical SRS beam with MU/deg > 20:";
                    else
                        failStr = "Conical SRS beams with MU/deg > 20:";
                    int i = 0;
                    foreach (var failBeam in failBeamList)
                    {
                        if (i++ > 0)
                            failStr += ",";
                        failStr += String.Format(" {0} [{1} MU/deg]", failBeam.Key, failBeam.Value.ToString("N1"));
                    }
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No conical SRS beams present in plan";
            }

            return check;
        }


        public MainWindow.CheckItem CheckSrsJawSize(PlanSetup ps, double ExpectedJawSize)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Conical SRS Jaw Size Correct";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && x.Technique.Id.Equals("SRS ARC") && !x.IsSetupField) > 0)
            {
                var failBeamList = new List<KeyValuePair<string, string>>();

                foreach (var srsBeam in ps.Beams.Where(x => x.Technique.Id.Equals("SRS ARC")))
                {
                    if (!(Math.Abs(srsBeam.ControlPoints.First().JawPositions.X1 + ExpectedJawSize/2) < 0.001 &&
                        Math.Abs(srsBeam.ControlPoints.First().JawPositions.X2 - ExpectedJawSize / 2) < 0.001 &&
                        Math.Abs(srsBeam.ControlPoints.First().JawPositions.Y1 + ExpectedJawSize / 2) < 0.001 &&
                        Math.Abs(srsBeam.ControlPoints.First().JawPositions.Y2 - ExpectedJawSize / 2) < 0.001))
                    {
                        double xJaw = (srsBeam.ControlPoints.First().JawPositions.X2 - srsBeam.ControlPoints.First().JawPositions.X1) / 10.0;
                        double yJaw = (srsBeam.ControlPoints.First().JawPositions.Y2 - srsBeam.ControlPoints.First().JawPositions.Y1) / 10.0;
                        failBeamList.Add(new KeyValuePair<string, string>(srsBeam.Id, String.Format("{0} x {1} cm", xJaw.ToString("F1"), yJaw.ToString("F1"))));
                    }
                }
                if (failBeamList.Count == 0)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = String.Format("All conical SRS beams have jaw size ({0} x {1} cm", (ExpectedJawSize/10.0).ToString("F1"), (ExpectedJawSize/10.0).ToString("F1"));
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    string failStr = String.Format("Conical SRS beam without correct jaw size ({0} x {1} cm):", (ExpectedJawSize / 10.0).ToString("F1"), (ExpectedJawSize / 10.0).ToString("F1"));
                    foreach (var failBeam in failBeamList)
                    {
                        failStr += String.Format("\n  {0} [{1}]", failBeam.Key, failBeam.Value);
                    }
                    check.checkDetail = failStr;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No conical SRS beams present in plan";
            }

            return check;
        }

        public MainWindow.CheckItem CheckEdwMu(PlanSetup ps)
        {

            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "EDW fields ≥ 21 MU";

            int wedgeTally = 0;
            var wedgeFailList = new List<KeyValuePair<string, double>>();


            foreach (Beam beam in ps.Beams.Where(b => !b.IsSetupField))
            {
                if (beam.Wedges != null)
                {
                    foreach (Wedge wedge in beam.Wedges)
                    {
                        wedgeTally++;
                        if (beam.Meterset.Value < 21.0d)
                            wedgeFailList.Add(new KeyValuePair<string, double>(beam.Id, beam.Meterset.Value));
                    }
                }
            }

            if (wedgeTally != 0)
            {
                if (wedgeFailList.Count == 0)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "All EDW fields ≥ 21 MU";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    string strDetail;
                    if (wedgeFailList.Count == 1)
                        strDetail = "EDW field with < 21 MU:";
                    else
                        strDetail = "EDW fields with < 21 MU:";
                    foreach (var wedgeFailItem in wedgeFailList)
                    {
                        strDetail += String.Format("\n  Field: {0}, MU: {1}", wedgeFailItem.Key, wedgeFailItem.Value.ToString("F0"));
                    }
                    check.checkDetail = strDetail;
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No EDW fields present in plan.";
            }

            return check;
        }

        // Not working correctly
        public MainWindow.CheckItem CheckImrtColAngle(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "All IMRT Collimator Angles 0°";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && x.MLCPlanType.Equals(MLCPlanType.DoseDynamic) && !x.IsSetupField) > 0)
            {

                var failBeamList = new List<KeyValuePair<string, double>>();

                foreach (var imrtBeam in ps.Beams.Where(x => x.MLCPlanType.Equals(MLCPlanType.DoseDynamic)))
                {
                    ControlPoint maxCPWidth = null;
                    foreach (var cp in imrtBeam.ControlPoints.Where(x => x.JawPositions.X2 - x.JawPositions.X1 > 139))
                    {
                        if (maxCPWidth == null)
                            maxCPWidth = cp;
                        else if (cp.JawPositions.X2 - cp.JawPositions.X1 > maxCPWidth.JawPositions.X2 - maxCPWidth.JawPositions.X1)
                            maxCPWidth = cp;
                    }

                    if (maxCPWidth != null)
                    {
                        double fieldWidth = maxCPWidth.JawPositions.X2 - maxCPWidth.JawPositions.X1;
                        failBeamList.Add(new KeyValuePair<string, double>(imrtBeam.Id, fieldWidth));
                    }
                }

                if (failBeamList.Count == 0)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "All IMRT field widths < 13.9 cm";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Warn;
                    string failStr = null;
                    if (failBeamList.Count == 1)
                        failStr = "IMRT field with width > 13.9 cm:";
                    else
                        failStr = "IMRT fields with width > 13.9 cm:";
                    int i = 0;
                    foreach (var failBeam in failBeamList)
                    {
                        if (i++ > 0)
                            failStr += ",";
                        failStr += String.Format(" {0} [{1} cm]", failBeam.Key, failBeam.Value.ToString("N1"));
                    }
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No IMRT beams present in plan";
            }

            return check;
        }

        // Not working correctly
        public MainWindow.CheckItem CheckImrtFieldWidth(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "All IMRT Field Widths < 13.9 cm";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && x.MLCPlanType.Equals(MLCPlanType.DoseDynamic) && !x.IsSetupField) > 0)
            {

                var failBeamList = new List<KeyValuePair<string, double>>();

                foreach (var imrtBeam in ps.Beams.Where(x => x.MLCPlanType.Equals(MLCPlanType.DoseDynamic)))
                {
                    ControlPoint maxCPWidth = null;
                    foreach (var cp in imrtBeam.ControlPoints.Where(x => x.JawPositions.X2 - x.JawPositions.X1 > 139))
                    {
                        if (maxCPWidth == null)
                            maxCPWidth = cp;
                        else if (cp.JawPositions.X2 - cp.JawPositions.X1 > maxCPWidth.JawPositions.X2 - maxCPWidth.JawPositions.X1)
                            maxCPWidth = cp;
                    }

                    if (maxCPWidth != null)
                    {
                        double fieldWidth = maxCPWidth.JawPositions.X2 - maxCPWidth.JawPositions.X1;
                        failBeamList.Add(new KeyValuePair<string, double>(imrtBeam.Id, fieldWidth));
                    }
                }

                if (failBeamList.Count == 0)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "All IMRT field widths < 13.9 cm";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Warn;
                    string failStr = null;
                    if (failBeamList.Count == 1)
                        failStr = "IMRT field with width > 13.9 cm:";
                    else
                        failStr = "IMRT fields with width > 13.9 cm:";
                    int i = 0;
                    foreach (var failBeam in failBeamList)
                    {
                        if (i++ > 0)
                            failStr += ",";
                        failStr += String.Format(" {0} [{1} cm]", failBeam.Key, failBeam.Value.ToString("N1"));
                    }
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.NA;
                check.checkDetail = "No IMRT beams present in plan";
            }

            return check;
        }


        public MainWindow.CheckItem CheckMinPhotonJawSize(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "All photon beam jaw sizes ≥ 3.0 cm";

            // If at least 1 beam energy is a photon energy and not an SRS Arc
            if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && !x.IsSetupField && !x.Technique.Id.Equals("SRS ARC")) > 0)
            {

                var failBeamList = new List<KeyValuePair<string, string>>();

                foreach (var pBeam in ps.Beams.Where(x => x.EnergyModeDisplayName.Contains("X") && !x.IsSetupField && !x.Technique.Id.Equals("SRS ARC")))
                {
                    if ((pBeam.ControlPoints.First().JawPositions.X2 - pBeam.ControlPoints.First().JawPositions.X1 < 30.0) ||
                        (pBeam.ControlPoints.First().JawPositions.Y2 - pBeam.ControlPoints.First().JawPositions.Y1 < 30.0))
                    {
                        double xJaw = (pBeam.ControlPoints.First().JawPositions.X2 - pBeam.ControlPoints.First().JawPositions.X1) / 10.0;
                        double yJaw = (pBeam.ControlPoints.First().JawPositions.Y2 - pBeam.ControlPoints.First().JawPositions.Y1) / 10.0;
                        failBeamList.Add(new KeyValuePair<string, string>(pBeam.Id, String.Format("{0} x {1} cm", xJaw.ToString("F1"), yJaw.ToString("F1"))));
                    }
                }

                if (failBeamList.Count == 0)
                {
                    check.checkResult = MainWindow.Result.Pass;
                    check.checkDetail = "All photon field jaw sizes ≥ 3.0 cm";
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    string failStr = null;
                    if (failBeamList.Count == 1)
                        failStr = "Photon field with jaw size < 3.0 cm:";
                    else
                        failStr = "Photon fields with jaw size < 3.0 cm:";
                    int i = 0;
                    foreach (var failBeam in failBeamList)
                    {
                        if (i++ > 0)
                            failStr += ",";
                        failStr += String.Format(" {0} [{1}]", failBeam.Key, failBeam.Value);
                    }
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
