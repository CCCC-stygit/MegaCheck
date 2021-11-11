using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace MegaCheck
{
    class MLCChecks
    {

        private double GetMLCLeafWidth(int MLCNumber)
        {
            // MLCNumber + 1 is equals to Varian MLC leaf number

            // MLC Leaf 11-49 return 5 mm
            if (MLCNumber + 1 > 10 && MLCNumber + 1 < 51)
                return 5.0d;
            // If an outer leaf (1-10 or 51-60) return 10 mm
            else
                return 10.0d;
        }

        private VRect<double> GetAppertureOutline(ControlPointCollection controlPoints)
        {
            VRect<double> apperture = new VRect<double>(-999, -999,999,999);

            foreach (ControlPoint cp in controlPoints)
            {
                // Do magic stuff here
            }

            return apperture;
        }

        public MainWindow.CheckItem CheckCIAO(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Complete Irradiated Apperture Outline Check";

            if (ps.Beams != null)
            {

                // If at least 1 beam energy is a photon energy and not an SRS Arc
                if (ps.Beams.Count(x => x.EnergyModeDisplayName.Contains("X") && !x.Technique.Id.Equals("SRS ARC") && !x.IsSetupField) > 0)
                {
                    foreach (Beam beam in ps.Beams.Where(x => x.EnergyModeDisplayName.Contains("X") && !x.IsSetupField))
                    {
                        
                        if (beam.MLCPlanType == MLCPlanType.Static)
                        {
                            if (beam.MLC != null)
                            {
                                ControlPoint cp = beam.ControlPoints.First();
                            }

                        }
                        else if (beam.MLCPlanType == MLCPlanType.ArcDynamic || beam.MLCPlanType == MLCPlanType.DoseDynamic || beam.MLCPlanType == MLCPlanType.VMAT)
                        {
                            if (beam.MLC !=null)
                            {

                            }
                            else
                            {
                                MessageBox.Show("Error, a dynamic field has no MLC positions. This treatment could harm the patient if allowed to proceed.", "No MLC positions found for field " + beam.Id, MessageBoxButton.OK, MessageBoxImage.Stop);
                            }
                        }
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.NA;
                    check.checkDetail = "No photon beams present in plan";
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "No beams present in plan";
            }

            return check;
        }
    }
}
