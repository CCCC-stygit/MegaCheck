using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace MegaCheck
{
    class PointChecks
    {
        public MainWindow.CheckItem CheckRefPointLocation(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Reference Point Location";
            if (ps.PrimaryReferencePoint != null)
            {
                ReferencePoint refPoint = ps.PrimaryReferencePoint;

                if (ps.Beams.Where(x => !x.IsSetupField).Count() > 0)
                {
                    // If all treatment fields have a field reference point equal to the primary reference point
                    if (ps.Beams.Where(x => !x.IsSetupField && x.FieldReferencePoints.Count(frp => frp.ReferencePoint == refPoint) > 0).Count() ==
                        ps.Beams.Where(x => !x.IsSetupField).Count())
                    {
                        FieldReferencePoint fieldRefPoint = ps.Beams.First(beam => !beam.IsSetupField).FieldReferencePoints.First(frp => frp.ReferencePoint == refPoint);
                        VVector refLocation = fieldRefPoint.RefPointLocation;
                        bool isInsidePTV = false;
                        bool isInsideCTV = false;

                        foreach (Structure structure in ps.StructureSet.Structures.Where(s => s.DicomType.Equals("CTV")))
                        {
                            if (structure.IsPointInsideSegment(refLocation))
                                isInsideCTV = true;
                        }
                        foreach (Structure structure in ps.StructureSet.Structures.Where(s => s.DicomType.Equals("PTV")))
                        {
                            if (structure.IsPointInsideSegment(refLocation))
                                isInsidePTV = true;
                        }

                        if (isInsideCTV & isInsidePTV)
                        {
                            check.checkResult = MainWindow.Result.Pass;
                            check.checkDetail = "Reference point within PTV and CTV.";
                        }
                        else if (isInsideCTV)
                        {
                            check.checkResult = MainWindow.Result.Warn;
                            check.checkDetail = "Reference point within CTV, but not PTV.";
                        }
                        else if (isInsidePTV)
                        {
                            check.checkResult = MainWindow.Result.Warn;
                            check.checkDetail = "Reference point within PTV, but not CTV.";
                        }
                        else
                        {
                            check.checkResult = MainWindow.Result.Fail;
                            check.checkDetail = "Reference point not within CTV or PTV.";
                        }

                    }
                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "Not all fields reference points equal to the primary reference point.";
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Plan must have at least one treatment beam to check reference point.";
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "Plan contains no primary reference point.";
            }

            return check;
        }

        public MainWindow.CheckItem CheckRefPointDose(PlanSetup ps)
        {
            MainWindow.CheckItem check = new MainWindow.CheckItem();
            check.checkName = "Reference Point Dose Matches Prescription";
            if (ps.PrimaryReferencePoint != null)
            {
                ReferencePoint refPoint = ps.PrimaryReferencePoint;
                if (ps.Beams.Where(x => !x.IsSetupField).Count() > 0)
                {
                    // If all treatment fields have a field reference point equal to the primary reference point
                    if (ps.Beams.Where(x => !x.IsSetupField && x.FieldReferencePoints.Count(frp => frp.ReferencePoint == refPoint) > 0).Count() ==
                        ps.Beams.Where(x => !x.IsSetupField).Count())
                    {
                        if (ps.Dose != null)
                        {
                            ps.DoseValuePresentation = DoseValuePresentation.Absolute;

                            FieldReferencePoint fieldRefPoint = ps.Beams.First(beam => !beam.IsSetupField).FieldReferencePoints.First(frp => frp.ReferencePoint == refPoint);
                            VVector refPointLocation = fieldRefPoint.RefPointLocation;

                            if (refPointLocation.x.ToString() != "NaN" && refPointLocation.y.ToString() != "NaN" && refPointLocation.x.ToString() != "NaN")
                            {

                                DoseValue doseRx = ps.TotalDose;
                                DoseValue doseRef = ps.Dose.GetDoseToPoint(refPointLocation);

                                if ((Math.Abs(doseRx.Dose - doseRef.Dose) < 0.05))
                                {
                                    check.checkResult = MainWindow.Result.Pass;
                                    check.checkDetail = String.Format("Reference point dose matches Eclipse prescription.");
                                }
                                else
                                {
                                    check.checkResult = MainWindow.Result.Fail;
                                    check.checkDetail = String.Format("Reference point dose ({0} {1}) does not match prescription dose ({2} {3}).",
                                        doseRef.Dose.ToString("F1"), doseRef.UnitAsString,
                                        doseRx.Dose.ToString("F1"), doseRx.UnitAsString);
                                }
                            }
                            else
                            {
                                check.checkResult = MainWindow.Result.Fail;
                                check.checkDetail = "Primary reference point does not have a location.";
                            }

                        }
                        else
                        {
                            check.checkResult = MainWindow.Result.Fail;
                            check.checkDetail = "Dose must be calculated to check reference point dose against prescription.";
                        }

                    }
                    else
                    {
                        check.checkResult = MainWindow.Result.Fail;
                        check.checkDetail = "Not all fields reference points equal to the primary reference point.";
                    }
                }
                else
                {
                    check.checkResult = MainWindow.Result.Fail;
                    check.checkDetail = "Plan must have at least one treatment beam to check reference point.";
                }
            }
            else
            {
                check.checkResult = MainWindow.Result.Fail;
                check.checkDetail = "Plan contains no primary reference point.";
            }

            return check;
        }
    }
}
