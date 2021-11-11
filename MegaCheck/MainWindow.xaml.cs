using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;
using Microsoft.Win32;

namespace MegaCheck
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        Xceed.Wpf.Toolkit.BusyIndicator busyIndicator;

        Patient pat = null;
        Course course = null;
        PlanSetup ps = null;
        ScriptContext context = null;



        VMS.TPS.Common.Model.API.Application app = VMS.TPS.Common.Model.API.Application.CreateApplication();


        string[] str_checkTypes = { "Physics Check", "RT Check", "Senior RT Check" };
        string[] str_modalities = { "Photon", "Electron", "Conical SRS" };
        string[] str_technique = { "3DCRT", "IMRT (Conventional)", "VMAT (Conventional)", "IMRT (SABR)", "IMRT (SABR)" };
        string[] str_sites = { "Breast", "Non-Breast" };

        public enum ContextResult { Fail, Pass, None };

        public enum Result { Fail, Pass, Warn, Info, NA, Section, SelectableNone, SelectableFail, SelectablePass };

        BitmapImage bitmapPass = new BitmapImage();
        BitmapImage bitmapWarn = new BitmapImage();
        BitmapImage bitmapFail = new BitmapImage();
        BitmapImage bitmapInfo = new BitmapImage();

        List<CheckItem> checkList = new List<CheckItem>();

        public class CheckItem
        {
            public Result checkResult { get; set; }
            public string checkName { get; set; }
            public string checkDetail { get; set; }
            public string checkComment { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();


            /*
            busyIndicator = new Xceed.Wpf.Toolkit.BusyIndicator();
            busyIndicator.IsBusy = true;
            busyIndicator.BusyContent = "MegaCheck is connecting to Eclipse...";
            */

            //app = VMS.TPS.Common.Model.API.Application.CreateApplication("Therapist", "Therapist");



            if (Environment.GetCommandLineArgs().Length > 4)
            {
                string args = "";


                if (Environment.GetCommandLineArgs()[1] == "-p")
                    for (int i = 2; i < Environment.GetCommandLineArgs().Length; i++)
                    {
                        if (i != 2) args += " ";
                        args += Environment.GetCommandLineArgs()[i];

                    }
                //MessageBox.Show(args);
                string[] stringSeparators = new string[] { " %%%% " };
                var argsArray = args.Split(stringSeparators, 4, StringSplitOptions.None);

                string patId = null;
                string courseId = null;
                string planId = null;
                string userId = null;

                if (argsArray.Length == 4)
                {
                    patId = argsArray[0];
                    courseId = argsArray[1];
                    planId = argsArray[2];
                    userId = argsArray[3];


                    try
                    {
                        app.ClosePatient();
                        pat = app.OpenPatientById(patId);
                    }
                    catch
                    {
                        MessageBox.Show("Could not find patient ID");
                    }
                    if (pat != null)
                    {
                        course = pat.Courses.First(x => x.Id == courseId);

                        if (course != null)
                        {
                            ps = course.PlanSetups.Single(o => o.Id == planId);


                            //MessageBox.Show(ps.Id);
                            txt_name.Text = pat.LastName.ToUpper() + ", " + pat.FirstName;
                            txt_mrn.Text = pat.Id;
                            txt_course.Text = course.Id;
                            txt_plan.Text = ps.Id;
                            txt_user.Text = app.CurrentUser.Name;
                        }
                    }

                }

                //MessageBox.Show(String.Format("Patient ID:\t{0}\nCourse ID:\t{1}\nPlan ID:\t{2}\nUser ID:{3}", patId, courseId, planId, userId));

                //foreach (var str in Environment.GetCommandLineArgs())
                //{
                //    if (i == 0)
                //        args += str;
                //    else
                //        args += " " + str;
                //    i++;
                //}

                //MessageBox.Show("Arguments:_" + args);
            }


            PopulateGui();

            TestGui();


        }
        /*
        protected override void OnPreviewKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
        }
        */
        private void TestGui()
        {
            //throw new NotImplementedException();


            CheckItem sectionPrescription = new CheckItem();
            sectionPrescription.checkName = "Prescription";
            sectionPrescription.checkResult = Result.Section;
            checkList.Add(sectionPrescription);

            CheckItem selectableTest1 = new CheckItem();
            selectableTest1.checkName = "Prescription has been entered";
            selectableTest1.checkResult = Result.SelectableNone;
            checkList.Add(selectableTest1);

            CheckItem selectableTest2 = new CheckItem();
            selectableTest2.checkName = "Eclipse prescription matches Aria";
            selectableTest2.checkResult = Result.SelectablePass;
            checkList.Add(selectableTest2);

            CheckItem selectableTest3 = new CheckItem();
            selectableTest3.checkName = "Selectable check";
            selectableTest3.checkResult = Result.SelectableFail;
            checkList.Add(selectableTest3);

            CheckItem sectionDose = new CheckItem();
            sectionDose.checkName = "Dose Parameters";
            sectionDose.checkResult = Result.Section;
            checkList.Add(sectionDose);
            /*

            CheckItem selectableTest1 = new CheckItem();
            selectableTest1.checkName = "Qualitative check";
            selectableTest1.checkResult = Result.SelectableNone;
            checkList.Add(selectableTest1);

            CheckItem selectableTest2 = new CheckItem();
            selectableTest2.checkName = "Selectable check";
            selectableTest2.checkResult = Result.SelectablePass;
            checkList.Add(selectableTest2);

            CheckItem selectableTest3 = new CheckItem();
            selectableTest3.checkName = "Selectable check";
            selectableTest3.checkResult = Result.SelectableFail;
            checkList.Add(selectableTest3);
            */

            var beamChecks = new BeamChecks();
            CheckItem photonDoseAlgorithm = beamChecks.CheckPhotonDoseAlgorithm(ps, "AAA_15606_2020");
            checkList.Add(photonDoseAlgorithm);
            CheckItem imrtOptimizerAlgorithm = beamChecks.CheckImrtOptimizerAlgorithm(ps, "PO_15606");
            checkList.Add(imrtOptimizerAlgorithm);
            CheckItem vmatOptimizerAlgorithm = beamChecks.CheckVmatOptimizerAlgorithm(ps, "PO_15606");
            checkList.Add(vmatOptimizerAlgorithm);
            CheckItem heteogeneityCorr = beamChecks.CheckHeteogeneityCorr(ps);
            checkList.Add(heteogeneityCorr);
            CheckItem coneDoseAlgorithm = beamChecks.CheckConeDoseAlgorithm(ps, "CDC_15606");
            checkList.Add(coneDoseAlgorithm);
            CheckItem electronDoseAlgorithm = beamChecks.CheckElectronDoseAlgorithm(ps, "EMC_11031");
            checkList.Add(electronDoseAlgorithm);

            CheckItem photonDoseGridSize = beamChecks.CheckPhotonDoseGridSize(ps, 0.2 /*cm*/, 0.1 /*cm*/, 800.0 /*cGy*/);
            checkList.Add(photonDoseGridSize);
            CheckItem electronDoseGridSize = beamChecks.CheckElectronDoseGridSize(ps, 0.2 /*cm*/);
            checkList.Add(electronDoseGridSize);
            CheckItem coneDoseGridSize = beamChecks.CheckSrsDoseGridSize(ps, "Fine");
            checkList.Add(coneDoseGridSize);

            CheckItem imrtFieldWidth = beamChecks.CheckImrtFieldWidth(ps);
            checkList.Add(imrtFieldWidth);

            CheckItem srsMuPerDeg = beamChecks.CheckSrsMuPerDeg(ps);
            checkList.Add(srsMuPerDeg);
            CheckItem srsJawSize = beamChecks.CheckSrsJawSize(ps, 40.0d /*mm*/);
            checkList.Add(srsJawSize);

            CheckItem edwMu = beamChecks.CheckEdwMu(ps);
            checkList.Add(edwMu);


            CheckItem sectionStructures = new CheckItem();
            sectionStructures.checkName = "Structures and Image Dataset";
            sectionStructures.checkResult = Result.Section;
            checkList.Add(sectionStructures);

            var structureChecks = new StructureChecks();

            CheckItem unusedStructures = structureChecks.CheckUnusedStructures(ps);
            checkList.Add(unusedStructures);

            CheckItem structsOutsideDoseBox = structureChecks.CheckStructuresOutsideDoseGrid(ps);
            checkList.Add(structsOutsideDoseBox);
            CheckItem bodyStructsOutsideDoseBox = structureChecks.CheckBodyStructuresOutsideDoseGrid(ps);
            checkList.Add(bodyStructsOutsideDoseBox);

            CheckItem bolusAssigned = structureChecks.CheckBolusAssigned(ps);
            checkList.Add(bolusAssigned);

            var imageChecks = new ImageChecks();
            CheckItem imageClipping = imageChecks.CheckImageClipping(ps);
            checkList.Add(imageClipping);

            CheckItem structureOverrides = structureChecks.CheckStructureOverrides(ps, "CouchInterior", "CouchSurface");
            checkList.Add(structureOverrides);
            CheckItem structureCouch = structureChecks.CheckCouchOverrides(ps, "CouchInterior", -1000.0, "CouchSurface", -300.0);
            checkList.Add(structureCouch);

            
            CheckItem sectionReferencePoint = new CheckItem();
            sectionReferencePoint.checkName = "Reference Point";
            sectionReferencePoint.checkResult = Result.Section;
            checkList.Add(sectionReferencePoint);
            var pointChecks = new PointChecks();
            CheckItem checkRefPointLocation = pointChecks.CheckRefPointLocation(ps);
            checkList.Add(checkRefPointLocation);
            CheckItem checkRefPointDose = pointChecks.CheckRefPointDose(ps);
            checkList.Add(checkRefPointDose);



            CheckItem sectionFieldProps = new CheckItem();
            sectionFieldProps.checkName = "Field Properties";
            sectionFieldProps.checkResult = Result.Section;
            checkList.Add(sectionFieldProps);


            CheckItem beamsNamed = beamChecks.CheckBeamsNamed(ps);
            checkList.Add(beamsNamed);

            CheckItem checkDoseRate = beamChecks.CheckDoseRate(ps);
            checkList.Add(checkDoseRate);

            CheckItem checkBeamEnergies = beamChecks.CheckBeamEnergies(ps);
            checkList.Add(checkBeamEnergies);

            CheckItem checkMinPhotonJawSizes = beamChecks.CheckMinPhotonJawSize(ps);
            checkList.Add(checkMinPhotonJawSizes);

            CheckItem tolTables = beamChecks.CheckTolTables(ps);
            checkList.Add(tolTables);

            CheckItem checkMachine = beamChecks.CheckMachine(ps);
            checkList.Add(checkMachine);

            CheckItem checkBeamIsos = beamChecks.CheckBeamIsocenters(ps);
            checkList.Add(checkBeamIsos);




            CheckItem sectionPlanEval = new CheckItem();
            sectionPlanEval.checkName = "Plan Evaluation";
            sectionPlanEval.checkResult = Result.Section;
            checkList.Add(sectionPlanEval);

            DoseChecks doseChecks = new DoseChecks();
            CheckItem checkCI = doseChecks.CheckConformityIndex(ps, 1.15, 9);
            checkList.Add(checkCI);

            CheckItem sectionCollision = new CheckItem();
            sectionCollision.checkName = "Collision Checks";
            sectionCollision.checkResult = Result.Section;
            checkList.Add(sectionCollision);
            
            var collisionChecks = new CollisionChecks();
            //CheckItem vmatCollision = collisionChecks.CheckVMATCollision(ps, 5, 5, 2);
            CheckItem electronCollision = collisionChecks.CheckElectronCollision(ps, 3, 1);
            checkList.Add(electronCollision);
            
            /*
            var collisionChecks = new CollisionChecks();

            
            
            CheckItem vmatCollision = collisionChecks.CheckVMATCollision(ps, 5, 5, 2);
            checkList.Add(vmatCollision);
            */

            /*
            var imageChecks = new ImageChecks();
            CheckItem imageClipping = imageChecks.CheckImageClipping(ps);
            checkList.Add(imageClipping);

            CheckItem sectionDoseTemp = new CheckItem();
            sectionDoseTemp.checkName = "Dose Parameters - Temp";
            sectionDoseTemp.checkResult = Result.Section;
            checkList.Add(sectionDoseTemp);

            CheckItem checkDoseBody = new CheckItem();
            checkDoseBody.checkName = "Body structure within Dose Box";
            checkDoseBody.checkResult = Result.Pass;
            checkList.Add(checkDoseBody);

            CheckItem checkDoseStructures = new CheckItem();
            checkDoseStructures.checkName = "Non-couch/tol structures found outside Dose Box";
            checkDoseStructures.checkDetail = "Structures outside: L Kidney, R Kidney";
            checkDoseStructures.checkResult = Result.Fail;
            checkList.Add(checkDoseStructures);

            CheckItem checkDoseRes = new CheckItem();
            checkDoseRes.checkName = "Dose grid resolution incorrect";
            checkDoseRes.checkDetail = "Resolution: 0.3 cm; Expected: 0.2 cm";
            checkDoseRes.checkResult = Result.Fail;
            checkList.Add(checkDoseRes);

            CheckItem checkDoseHetero = new CheckItem();
            checkDoseHetero.checkName = "Dose algorithms correct";
            checkDoseHetero.checkDetail = "Photon algorithm: AAA__13623; PO Algorithm: PO__13623";
            checkDoseHetero.checkResult = Result.Pass;
            checkList.Add(checkDoseHetero);

            CheckItem checkDoseAlg = new CheckItem();
            checkDoseAlg.checkName = "Heterogeneity correction on";
            checkDoseAlg.checkResult = Result.Pass;
            checkList.Add(checkDoseAlg);

            CheckItem sectionImg = new CheckItem();
            sectionImg.checkName = "Image Dataset and Voluming";
            sectionImg.checkResult = Result.Section;
            checkList.Add(sectionImg);

            CheckItem checkImgUnusedStruct = new CheckItem();
            checkImgUnusedStruct.checkName = "Unused structures found";
            checkImgUnusedStruct.checkDetail = "Unused structures: Small hands";
            checkImgUnusedStruct.checkResult = Result.Fail;
            checkList.Add(checkImgUnusedStruct);

            CheckItem checkImgClipping = new CheckItem();
            checkImgClipping.checkName = "Image voxels are clipping, check for overrides";
            checkImgClipping.checkResult = Result.Info;
            checkList.Add(checkImgClipping);

            CheckItem checkImgCouch = new CheckItem();
            checkImgCouch.checkName = "Couch structures have correct overrides";
            checkImgCouch.checkResult = Result.Pass;
            checkList.Add(checkImgCouch);

            CheckItem sectionDyn = new CheckItem();
            sectionDyn.checkName = "Dynamic Plan Properties";
            sectionDyn.checkResult = Result.Section;
            checkList.Add(sectionDyn);

            CheckItem checkDynImrtWidth = new CheckItem();
            checkDynImrtWidth.checkName = "IMRT field width <13.9cm";
            checkDynImrtWidth.checkDetail = "No IMRT fields present in plan";
            checkDynImrtWidth.checkResult = Result.NA;
            checkList.Add(checkDynImrtWidth);

            CheckItem checkDynImrtCol = new CheckItem();
            checkDynImrtCol.checkName = "IMRT collimator angle 0° for 1 or less beams";
            checkDynImrtCol.checkDetail = "No IMRT fields present in plan";
            checkDynImrtCol.checkResult = Result.NA;
            checkList.Add(checkDynImrtCol);


            CheckItem checkDynVmatWidth = new CheckItem();
            checkDynVmatWidth.checkName = "VMAT field widths <14.9cm";
            checkDynVmatWidth.checkResult = Result.Pass;
            checkList.Add(checkDynVmatWidth);

            CheckItem checkDynVmatCol = new CheckItem();
            checkDynVmatCol.checkName = "VMAT collimator angle >±5° off 0 for all beams";
            checkDynVmatCol.checkResult = Result.Pass;
            checkList.Add(checkDynVmatCol);

            CheckItem checkDynNorm = new CheckItem();
            checkDynNorm.checkName = "Plan normalisation within ±5%";
            checkDynNorm.checkDetail = "Plan normalisation +2%.";
            checkDynNorm.checkResult = Result.Pass;
            checkList.Add(checkDynNorm);

            CheckItem sectionRef = new CheckItem();
            sectionRef.checkName = "Reference Point and Plan Evaluation";
            sectionRef.checkResult = Result.Section;
            checkList.Add(sectionRef);

            CheckItem checkRefLoc = new CheckItem();
            checkRefLoc.checkName = "TD point location within CTV or PTV as per ICRU";
            checkRefLoc.checkResult = Result.Pass;
            checkList.Add(checkRefLoc);

            CheckItem checkRefDose = new CheckItem();
            checkRefDose.checkName = "TD point dose matches Eclipse prescription";
            checkRefDose.checkResult = Result.Pass;
            checkList.Add(checkRefDose);

            CheckItem checkRefEdw = new CheckItem();
            checkRefEdw.checkName = "EDW Field MU valid";
            checkRefEdw.checkDetail = "No EDW fields in plan";
            checkRefEdw.checkResult = Result.NA;
            checkList.Add(checkRefEdw);

            CheckItem sectionField = new CheckItem();
            sectionField.checkName = "Field Properties";
            sectionField.checkResult = Result.Section;
            checkList.Add(sectionField);

            CheckItem checkFieldNames = new CheckItem();
            checkFieldNames.checkName = "All treatment fields named";
            checkFieldNames.checkResult = Result.Pass;
            checkList.Add(checkFieldNames);

            CheckItem checkFieldDoserate = new CheckItem();
            checkFieldDoserate.checkName = "Dose rate correct for all beams";
            checkFieldDoserate.checkResult = Result.Pass;
            checkList.Add(checkFieldDoserate);

            CheckItem checkFieldEnergy = new CheckItem();
            checkFieldEnergy.checkName = "Beams all the same energy";
            checkFieldEnergy.checkResult = Result.Pass;
            checkList.Add(checkFieldEnergy);

            CheckItem checkFieldTolTable = new CheckItem();
            checkFieldTolTable.checkName = "Tolerance table consistent for all beams";
            checkFieldTolTable.checkDetail = "Tolerance table: '4 Indexed'";
            checkFieldTolTable.checkResult = Result.Pass;
            checkList.Add(checkFieldTolTable);

            CheckItem checkFieldMachine = new CheckItem();
            checkFieldMachine.checkName = "Machine for all beams";
            checkFieldMachine.checkDetail = "Machine: 'G2-2012'";
            checkFieldMachine.checkResult = Result.Pass;
            checkList.Add(checkFieldMachine);

            CheckItem b = new CheckItem();
            b.checkName = "FailItem";
            b.checkComment = "FailComment2";
            b.checkDetail = "Blah blah blah";
            b.checkResult = Result.Fail;


            CheckItem c = new CheckItem();
            c.checkName = "InfoItem3";
            c.checkComment = null;
            c.checkResult = Result.Info;

            CheckItem d = new CheckItem();
            d.checkName = "N/AItem3";
            d.checkComment = "NA/Comment3";
            d.checkResult = Result.NA;

            CheckItem e = new CheckItem();
            e.checkName = "N/AItem3";
            e.checkComment = "NA/Comment3";
            e.checkResult = Result.Warn;
            */
            UpdateList(checkList);
        }

        public void UpdateList(List<CheckItem> checkList)
        {
            if (stk_main != null)
                stk_main.Children.Clear();

            foreach (CheckItem check in checkList)
            {
                if (check.checkResult == Result.Section)
                {

                    Border border = new Border();
                    border.BorderBrush = new SolidColorBrush(Colors.White);
                    border.BorderThickness = new Thickness(1);

                    TextBlock txtSection = new TextBlock();
                    txtSection.Text = check.checkName;
                    txtSection.FontSize = 16;
                    txtSection.Margin = new Thickness(0, 5, 0, 5);
                    txtSection.FontWeight = FontWeights.Bold;

                    border.Child = txtSection;
                    stk_main.Children.Add(border);
                }
                else
                {
                    bool displayCheck = false;
                    bool selectable = false;
                    string strCheckResult = null;
                    System.Windows.Controls.Image imgResult = new System.Windows.Controls.Image();
                    imgResult.Width = 24;
                    imgResult.Height = 24;
                    SolidColorBrush bgColor = new SolidColorBrush(Colors.White);
                    switch (check.checkResult)
                    {
                        case Result.Pass:
                            if (chk_pass.IsChecked == true)
                                displayCheck = true;
                            strCheckResult = "Pass";
                            imgResult.Source = new BitmapImage(new Uri("pack://application:,,,/Images/sign-check.png"));
                            bgColor = new SolidColorBrush(Color.FromRgb(230, 255, 230));
                            break;
                        case Result.SelectablePass:
                            if (chk_pass.IsChecked == true)
                                displayCheck = true;
                            strCheckResult = "Pass";
                            imgResult.Source = new BitmapImage(new Uri("pack://application:,,,/Images/sign-check.png"));
                            bgColor = new SolidColorBrush(Color.FromRgb(230, 255, 230));
                            selectable = true;
                            break;
                        case Result.Warn:
                            if (chk_warn.IsChecked == true)
                                displayCheck = true;
                            strCheckResult = "Warn";
                            imgResult.Source = new BitmapImage(new Uri("pack://application:,,,/Images/sign-warning.png"));
                            bgColor = new SolidColorBrush(Color.FromRgb(255, 255, 230));
                            break;
                        case Result.Fail:
                            if (chk_fail.IsChecked == true)
                                displayCheck = true;
                            strCheckResult = "Fail";
                            imgResult.Source = new BitmapImage(new Uri("pack://application:,,,/Images/sign-error.png"));
                            bgColor = new SolidColorBrush(Color.FromRgb(255, 220, 220));
                            break;
                        case Result.SelectableFail:
                            if (chk_fail.IsChecked == true)
                                displayCheck = true;
                            strCheckResult = "Fail";
                            imgResult.Source = new BitmapImage(new Uri("pack://application:,,,/Images/sign-error.png"));
                            bgColor = new SolidColorBrush(Color.FromRgb(255, 220, 220));
                            selectable = true;
                            break;
                        case Result.Info:
                            if (chk_info.IsChecked == true)
                                displayCheck = true;
                            strCheckResult = "Info";
                            bgColor = new SolidColorBrush(Color.FromRgb(220, 255, 255));
                            imgResult.Source = new BitmapImage(new Uri("pack://application:,,,/Images/sign-info.png"));
                            break;
                        case Result.NA:
                            if (chk_na.IsChecked == true)
                                displayCheck = true;
                            strCheckResult = "N/A";
                            bgColor = new SolidColorBrush(Color.FromRgb(230, 230, 230));
                            break;
                        case Result.SelectableNone:
                            displayCheck = true;
                            strCheckResult = "N/A";
                            bgColor = new SolidColorBrush(Color.FromRgb(255, 209, 164));
                            selectable = true;
                            break;
                        default:
                            break;
                    }
                    if (displayCheck == true)
                    {

                        imgResult.ToolTip = strCheckResult;

                        Border border = new Border();
                        if (selectable)
                            border.BorderBrush = new SolidColorBrush(Colors.Gray);
                        else
                            border.BorderBrush = new SolidColorBrush(Colors.White);
                        border.BorderThickness = new Thickness(1);
                        border.MouseLeftButtonUp += (sender, e) => Check_MouseDoubleClick(check);

                        border.PreviewKeyUp += (sender, e) => Check_MouseDoubleClick(check); //Check_KeyPress(check, e);

                        if (selectable)
                        {
                            ContextMenu menu = new ContextMenu();
                            MenuItem menuPass = new MenuItem();
                            menuPass.Header = "_Pass";
                            MenuItem menuFail = new MenuItem();
                            menuFail.Header = "_Fail";
                            MenuItem menuNone = new MenuItem();
                            menuNone.Header = "_None";



                            MenuItem menuTitle = new MenuItem();
                            menuTitle.Header = check.checkName;
                            menuTitle.Background = new SolidColorBrush(Color.FromRgb(0, 122, 204));
                            menuTitle.FontWeight = FontWeights.Bold;
                            menuTitle.IsEnabled = false;
                            menuTitle.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                            
                            menu.Items.Add(menuTitle);
                            menu.Items.Add(menuPass);
                            menu.Items.Add(menuFail);
                            menu.Items.Add(menuNone);


                            menuPass.Click += (sender, e) => Check_SetContextResult(check, ContextResult.Pass);
                            menuFail.Click += (sender, e) => Check_SetContextResult(check, ContextResult.Fail);
                            menuNone.Click += (sender, e) => Check_SetContextResult(check, ContextResult.None);
                            //Assign event handlers
                            //
                            /*
                            mnuCopy.Click += new EventHandler(mnuCopy_Click(check));
                            mnuCut.Click += new EventHandler(mnuCut_Click);
                            mnuPaste.Click += new EventHandler(mnuPaste_Click);
                            //Add to main context menu
                            mnu.Items.AddRange(new ToolStripItem[] { mnuCopy, mnuCut, mnuPaste });
                            //Assign to datagridview
                            
                                */
                            border.ContextMenu = menu;
                        }

                        StackPanel vertStack = new StackPanel();
                        vertStack.Orientation = Orientation.Vertical;
                        vertStack.Margin = new Thickness(5, 5, 5, 5);
                        //vertStack.Background = bgColor;


                        border.Background = bgColor;


                        border.Child = vertStack;

                        StackPanel horzStack = new StackPanel();
                        horzStack.Orientation = Orientation.Horizontal;
                        horzStack.VerticalAlignment = VerticalAlignment.Center;

                        horzStack.Children.Add(imgResult);


                        TextBlock txtDescription = new TextBlock();
                        txtDescription.Text = check.checkName;
                        txtDescription.FontSize = 12;
                        txtDescription.Margin = new Thickness(5, 0, 0, 0);
                        txtDescription.FontWeight = FontWeights.Bold;
                        txtDescription.VerticalAlignment = VerticalAlignment.Center;
                        horzStack.Children.Add(txtDescription);



                        vertStack.Children.Add(horzStack);
                        if (check.checkDetail != null && check.checkDetail != "")
                        {

                            TextBlock txtDetail = new TextBlock();
                            txtDetail.Text = check.checkDetail;
                            txtDetail.Margin = new Thickness(30, 0, 0, 0);
                            vertStack.Children.Add(txtDetail);

                        }


                        if (check.checkComment != null && check.checkComment != "")
                        {
                            StackPanel stackComments = new StackPanel();
                            stackComments.Orientation = Orientation.Horizontal;

                            TextBlock txtCommentTitle = new TextBlock();
                            txtCommentTitle.FontWeight = FontWeights.Bold;
                            txtCommentTitle.Margin = new Thickness(40, 0, 0, 0);
                            txtCommentTitle.Text = "Comments: ";
                            stackComments.Children.Add(txtCommentTitle);

                            TextBlock txtComments = new TextBlock();
                            txtComments.Text = check.checkComment;
                            stackComments.Children.Add(txtComments);

                            vertStack.Children.Add(stackComments);
                        }
                        stk_main.Children.Add(border);
                    }
                }
            }
        }

        private void Check_KeyPress(CheckItem check, KeyEventArgs e)
        {
            MessageBox.Show(e.Key.ToString());
            //Main
            if (e.Key == Key.P)
            {
                check.checkResult = Result.SelectablePass;
                UpdateList(checkList);
            }
            else if (e.Key == Key.F)
            {
                check.checkResult = Result.SelectableFail;
                UpdateList(checkList);
            }
        }

        private void Border_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void PopulateGui()
        {

            BitmapImage bitmapPass = new BitmapImage();
            bitmapPass.BeginInit();
            bitmapPass.UriSource = new Uri("pack://application:,,,/Images/sign-check.png");
            bitmapPass.EndInit();

            foreach (var str in str_checkTypes)
                cmb_checkType.Items.Add(str);
            cmb_checkType.SelectedIndex = 2;
            foreach (var str in str_modalities)
                cmb_modality.Items.Add(str);
            cmb_modality.SelectedIndex = 1;
            foreach (var str in str_technique)
                cmb_technique.Items.Add(str);

            cmb_technique.SelectedIndex = 2;
            foreach (var str in str_sites)
                cmb_site.Items.Add(str);

            cmb_site.SelectedIndex = 1;

        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            UpdateList(checkList);
        }

        private void Check_SetContextResult(CheckItem check, ContextResult result)
        {
            //MessageBox.Show("Check: " + check.checkName + " was double clicked!");

            switch (result)
            {
                case ContextResult.Pass:
                    check.checkResult = Result.SelectablePass;
                    break;
                case ContextResult.Fail:
                    check.checkResult = Result.SelectableFail;
                    break;
                case ContextResult.None:
                    check.checkResult = Result.SelectableNone;
                    break;
            }
            var scrollPos = scroll_main.VerticalOffset;
            UpdateList(checkList);
            scroll_main.ScrollToVerticalOffset(scrollPos);

        }

        private void Check_MouseDoubleClick(CheckItem check)
        {
            //MessageBox.Show("Check: " + check.checkName + " was double clicked!");

            CommentWindow commentWin = new CommentWindow(check);
            commentWin.ShowDialog();
            if (commentWin.commentChanged)
            {
                var scrollPos = scroll_main.VerticalOffset;
                check.checkComment = commentWin.comment;
                UpdateList(checkList);
                scroll_main.ScrollToVerticalOffset(scrollPos);
            }

        }



        private void btn_savePdf_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dlg = new PrintDialog();
            dlg.PrintVisual(stk_main, "testPrint");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            app.ClosePatient();
            app.Dispose();
        }

        private void btn_collision_Click(object sender, RoutedEventArgs e)
        {
            btn_collision.Visibility = Visibility.Collapsed;
            prog_collision.Visibility = Visibility.Visible;


            //Dispatcher.Invoke(new Action(() => performCollisionChecks()), System.Windows.Threading.DispatcherPriority.Background);
            performCollisionChecks();



        }
        private void performCollisionChecks()
        {
            //Dispatcher.Invoke(new Action(() => btn_collision.Visibility = Visibility.Collapsed), System.Windows.Threading.DispatcherPriority.Send);
            //Dispatcher.Invoke(new Action(() => prog_collision.Visibility = Visibility.Visible), System.Windows.Threading.DispatcherPriority.Send);

            //btn_collision.Visibility = Visibility.Collapsed;
            //prog_collision.Visibility = Visibility.Visible;

            CheckItem sectionCollision = new CheckItem();
            sectionCollision.checkName = "Collision Checks";
            sectionCollision.checkResult = Result.Section;
            checkList.Add(sectionCollision);


            var collisionChecks = new CollisionChecks();
            CheckItem vmatCollision = collisionChecks.CheckVMATCollision(ps, 5, 5, 2);

            checkList.Add(vmatCollision);

            prog_collision.Visibility = Visibility.Collapsed;
            btn_collision.Visibility = Visibility.Visible;
            //btn_collision.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Warning MegaCheck is in development. Most checks do not actually work, and are just a shell for future functionality.\n\nDO NOT USE MEGACHECK FOR ANY CLINICAL WORK.", "TESTING ONLY", MessageBoxButton.OK, MessageBoxImage.Stop);

        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            /*
            btn_collision.Visibility = Visibility.Collapsed;
            prog_collision.Visibility = Visibility.Visible;
            //Dispatcher.Invoke(new Action(() => MessageBox.Show("Warning MegaCheck is in development. Most checks do not actually work, and are just a shell for future functionality.\n\nDO NOT USE MEGACHECK FOR ANY CLINICAL WORK.", "TESTING ONLY", MessageBoxButton.OK, MessageBoxImage.Stop)), System.Windows.Threading.DispatcherPriority.Background);
            Dispatcher.Invoke(new Action(() => performCollisionChecks()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            MessageBox.Show("Warning MegaCheck is in development. Most checks do not actually work, and are just a shell for future functionality.\n\nDO NOT USE MEGACHECK FOR ANY CLINICAL WORK.", "TESTING ONLY", MessageBoxButton.OK, MessageBoxImage.Stop);
            //Dispatcher.Invoke(new Action(() => MessageBox.Show("Warning MegaCheck is in development. Most checks do not actually work, and are just a shell for future functionality.\n\nDO NOT USE MEGACHECK FOR ANY CLINICAL WORK.", "TESTING ONLY", MessageBoxButton.OK, MessageBoxImage.Stop)), System.Windows.Threading.DispatcherPriority.Background);
            */
        }
    }
}
