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
using System.Reflection;

namespace StayOnFeatureBranchW10
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region objects

        private JsonConfig json;
        private RegistryRead reg;

        //Read and write registry objects
        private TargetRelease rel;
        private TargetRelease newtargetrel;

        private FeatureUpdates fet;
        private FeatureUpdates newfetupdate;

        private QualityUpdates qal;
        private QualityUpdates newqalupdate;

        //Update information from json file
        private Windows10FeatureUpdateInfo info;
        private Windows10FeatureUpdateInfo selectedUpdate;
        private List<Windows10FeatureUpdateInfo> featureupdates;

        #endregion

        private void WindowLoaded(object sender, EventArgs e)
        {
            //Lock ui elements
            LockUnlockUiElements();
            btnUnlockLock.IsEnabled = false;

            //Hide json config button
            #if DEBUG
            #else
            btnSaveJsonConfig.IsEnabled = false;
            btnSaveJsonConfig.Visibility = Visibility.Hidden;
            //Hide button to update release information from web
            btnUpdateReleaseInformation.IsEnabled = false;
            btnUpdateReleaseInformation.Visibility = Visibility.Hidden;
            #endif

            //Show version and build number
            string debugbuild = "";
            #if DEBUG
            debugbuild = " debug";
            #endif
            string toolversion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            lblToolVersion.Content = "v" + toolversion + debugbuild;

            reg = new RegistryRead();

            json = new JsonConfig();

            //Load feature update list
            try
            {
                json.LoadFeatureUpdateConfigFromJson();
            }
            catch (Exception jsonerror)
            {
                MessageBox.Show("Exception during load of featureupdates.json: " + jsonerror.Message);
                return;
            }
            
            featureupdates = json.GetFeatureUpdates();

            //Load system information
            lblProduct.Content = "Product: " + reg.productName;

            lblEdition.Content = "Edition: " + reg.editionId;

            if (reg.displayVersion == null)
            {
                lblRelease.Content = "Release: " + reg.releaseId;
            }
            else
            {
                lblRelease.Content = "Release: " + reg.displayVersion;
            }

            lblArch.Content = "OS architecture: " + reg.computerArchitecture;

            lblBuild.Content = "OS build: " + reg.currentBuild;

            //If supported OS
            if (CheckOSSupported())
            {
                //Load registry values and update ui elements
                LoadRegistryValues();

                InitCheckBoxUI();

                //Windows10UpdateInformation
                info = json.GetCurrentFeatureUpdate(reg.releaseId);

                //Try display version if releaseId does not match
                if (info.version == null)
                {
                    info = json.GetCurrentFeatureUpdate(reg.displayVersion);
                }

                if (info.version != null)
                {
                    lblFeatureUpdateVersion.Content = "Version: " + info.version;

                    lblFeatureUpdateBuild.Content = "Build: " + info.build;

                    lblFeatureUpdateName.Content = "Name: " + info.name;

                    lblFeatureUpdateRelease.Content = "Release date: " + info.releasedate;

                    lblFeatureUpdateEndOfSupport.Content = "End of support: " + info.endofsupport;

                    LoadRecommendedUpdate();

                    UpdateFeatureUpdateConfig();

                    UpdateQualityUpdateConfig();

                    CheckEndOfSupport();

                    CheckUpdateEqualRegistry(); 
                    
                }
                else
                {
                    //Lock unlock button if version info is not available
                    btnUnlockLock.IsEnabled = false;
                    return;
                }

                //Check if user is administrator
                if (json.UserIsAdmin())
                {
                    lblUserIsAdmin.Content = "Running with administrator rights.";
                    btnUnlockLock.IsEnabled = true;
                    lblUserIsAdmin.Foreground = Brushes.Green;
                }
                else
                {
                    lblUserIsAdmin.Content = "Please run the app as administrator.";
                    btnUnlockLock.IsEnabled = false;
                    lblUserIsAdmin.Foreground = Brushes.Orange;
                }
            } 

        }

        #region Load

        private void LoadRecommendedUpdate()
        {
            //Recommended feature update information
            Windows10FeatureUpdateInfo recommendation = json.GetRecommendedFeatureUpdate();

            if (recommendation.rank >= info.rank)
            {
                lblRecommended.Content = "Recommended version: " + recommendation.version;
            }

            //Select recommended feature update
            if (rel.releaseId != null)
            {
                selectedUpdate = recommendation;

                foreach (Windows10FeatureUpdateInfo update in featureupdates)
                {
                    if (update.version == rel.releaseId)
                    {
                        selectedUpdate = update;
                        lstFeatureUpdates.SelectedItem = update.version + " / " + update.name;
                    }
                }
            }
            else
            {
                selectedUpdate = recommendation;
                lstFeatureUpdates.SelectedItem = recommendation.version + " / " + recommendation.name;
            }


            lblSelectedFeatureUpdate.Content = "Selected update: " + selectedUpdate.version;
        }

        private void LoadRegistryValues()
        {
            //TargetRelease
            rel = reg.GetTargetReleaseReg();

            if (rel.activated)
            {
                lblTargetReleaseActive.Content = "TargetRelease activated: Yes";
            }
            else
            {
                lblTargetReleaseActive.Content = "TargetRelease activated: No";
            }

            lblTargetReleaseId.Content = "TargetRelease Version: " + rel.releaseId;

            //FeatureUpdates
            fet = reg.GetFeatureUpdatesReg();

            if (fet.deferFeatureUpdates)
            {
                lblDeferFeatureUpdates.Content = "Defer feature updates: Yes";
            }
            else
            {
                lblDeferFeatureUpdates.Content = "Defer feature updates: No";
            }

            lblDeferFeatureUpdatesInDays.Content = "Defer feature updates in days: " + fet.deferFeatureUpdatesPeriodInDays;

            lblBranchReadiness.Content = "Branch readiness level: " + fet.branchReadinessLevel;

            lblPauseFeatureUpdatesStartTime.Content = "Pause feature updates start time: " + fet.pauseFeatureUpdatesStartTime;

            //QualityUpdates
            qal = reg.GetQualityUpdatesReg();

            if (qal.deferQualityUpdates)
            {
                lblDeferQualityUpdates.Content = "Defer quality updates: Yes";
            }
            else
            {
                lblDeferQualityUpdates.Content = "Defer quality updates: No";
            }

            lblDeferQualityUpdatesPeriodInDays.Content = "Defer quality updates in days: " + qal.deferQualityUpdatesPeriodInDays;

            lblPauseQualityUpdatesStartTime.Content = "Pause quality updates start time: " + qal.pauseQualityUpdatesStartTime;
        }

        #endregion

        #region Check

        private bool CheckOSSupported()
        {
            //Check if version is supported
            if (reg.productName.IndexOf("10") != -1 && reg.editionId.ToLower().IndexOf("prof") != -1 && reg.computerArchitecture == "x64")
            {
                lblSupported.Content = "System is supported.";
                lblSupported.Foreground = Brushes.Green;
                btnUnlockLock.IsEnabled = true;
                return true;
            }
            else
            {
                lblSupported.Content = "System is not supported.\nOnly Professional and x64 allowed.";
                lblSupported.Foreground = Brushes.Red;
                btnUnlockLock.IsEnabled = false;
                return false;
            }
        }

        private void CheckUpdateEqualRegistry()
        {
            //Check if update is equal to registry value
            if (rel.releaseId != null)
            {
                if (info.version == rel.releaseId)
                {
                    lblUpdateMatchReg.Content = "Current update version stored in registry.";
                    lblUpdateMatchReg.Foreground = Brushes.Green;
                    lblTargetReleaseId.Foreground = Brushes.Green;
                    lblNotification.Content = "";
                }
                else
                {
                    lblUpdateMatchReg.Content = "Different update version found in registry.";
                    lblUpdateMatchReg.Foreground = Brushes.Orange;
                    lblTargetReleaseId.Foreground = Brushes.Orange;
                    lblNotification.Content = "Please restart and run\n'Windows Update' to install\nthe new feature update.";
                }
            }
            else
            {
                lblUpdateMatchReg.Content = "No update version stored in registry.";
                lblUpdateMatchReg.Foreground = Brushes.Black;
                lblTargetReleaseId.Foreground = Brushes.Black;
                lblNotification.Content = "";
            }
        }

        private void CheckEndOfSupport()
        {
            //Check if end of support is reached
            if (json.GetFeatureUpdateEndofSupport(info))
            {
                lblEndOfSupport.Content = "Current update reached end of support.";
                lblEndOfSupport.Foreground = Brushes.Red;
                lblFeatureUpdateEndOfSupport.Foreground = Brushes.Red;
            }
            else
            {
                lblEndOfSupport.Content = "Current update is still supported.";
                lblEndOfSupport.Foreground = Brushes.Green;
                lblFeatureUpdateEndOfSupport.Foreground = Brushes.Black;
            }
        }

        #endregion

        #region Listbox

        private void lstFeatureUpdates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string version_name = lstFeatureUpdates.SelectedItem.ToString();

            string version = version_name.Split('/')[0].Trim();

            foreach (Windows10FeatureUpdateInfo update in featureupdates)
            {
                if (update.version == version)
                {
                    LoadFeatureUpdateInfo(update);

                    selectedUpdate = update;

                    lblSelectedFeatureUpdate.Content = "Selected update: " + selectedUpdate.version;

                    UpdateNewTargetRelease();
                }
            }
        }

        private void LoadFeatureUpdateInfo(Windows10FeatureUpdateInfo update)
        {
            lblNewFeatureUpdateVersion.Content = "Version: " + update.version;

            lblNewFeatureUpdateBuild.Content = "Build: " + update.build;

            lblNewFeatureUpdateName.Content = "Name: " + update.name;

            lblNewFeatureUpdateRelease.Content = "Release date: " + update.releasedate;

            lblNewFeatureUpdateEndOfSupport.Content = "End of support: " + update.endofsupport;

            if (json.GetFeatureUpdateEndofSupport(update))
            {
                lblNewFeatureUpdateEndOfSupport.Foreground = Brushes.Red;
            }
            else
            {
                lblNewFeatureUpdateEndOfSupport.Foreground = Brushes.Black;
            }

        }

        #endregion

        #region Update

        private void UpdateNewTargetRelease()
        {
            if (chkLockTargetRelease.IsChecked == true)
            {
                newtargetrel = new TargetRelease { releaseId = selectedUpdate.version, activated = true };
            }
            else
            {
                newtargetrel = new TargetRelease { releaseId = null, activated = false };
            }

            if (newtargetrel.releaseId != rel.releaseId && newtargetrel.releaseId != null)
            {
                lblNewTargetReleaseActive.Content = "TargetRelease activated: Yes";
                lblNewTargetReleaseId.Content = "TargetRelease Version: " + newtargetrel.releaseId;
                lblNewTargetReleaseActive.Foreground = Brushes.DarkBlue;
                lblNewTargetReleaseId.Foreground = Brushes.DarkBlue;
            }
            else if(!newtargetrel.activated && rel.activated)
            {
                lblNewTargetReleaseActive.Content = "TargetRelease activated: No";
                lblNewTargetReleaseId.Content = "TargetRelease Version: ";
                lblNewTargetReleaseActive.Foreground = Brushes.DarkBlue;
                lblNewTargetReleaseId.Foreground = Brushes.DarkBlue;
            }
            else
            {
                lblNewTargetReleaseActive.Content = "TargetRelease activated: ";
                lblNewTargetReleaseId.Content = "TargetRelease Version: ";
                lblNewTargetReleaseActive.Foreground = Brushes.Gray;
                lblNewTargetReleaseId.Foreground = Brushes.Gray;
            }

        }

        private void UpdateFeatureUpdateConfig()
        {
            if (chkDeferFeatureUpdates.IsChecked == true)
            {
                string deferindays = "256";
                if (cmbDeferFeatureInDays.SelectedItem == null)
                {
                    cmbDeferFeatureInDays.SelectedItem = deferindays;
                }
                else
                {
                    deferindays = cmbDeferFeatureInDays.SelectedItem.ToString();
                }
                
                newfetupdate = new FeatureUpdates { deferFeatureUpdates = true, branchReadinessLevel = "16", pauseFeatureUpdatesStartTime = "", deferFeatureUpdatesPeriodInDays = deferindays };
            }
            else
            {
                newfetupdate = new FeatureUpdates { deferFeatureUpdates = false, branchReadinessLevel = null, deferFeatureUpdatesPeriodInDays = null, pauseFeatureUpdatesStartTime = null };
            }

            if (newfetupdate.deferFeatureUpdates && !fet.deferFeatureUpdates)
            {
                lblNewDeferFeatureUpdates.Content = "Defer feature updates: Yes";
                lblNewDeferFeatureUpdates.Foreground = Brushes.DarkBlue;
            }
            else if(!newfetupdate.deferFeatureUpdates && fet.deferFeatureUpdates)
            {
                lblNewDeferFeatureUpdates.Content = "Defer feature updates: No";
                lblNewDeferFeatureUpdates.Foreground = Brushes.DarkBlue;
            }
            else
            {
                lblNewDeferFeatureUpdates.Content = "Defer feature updates: ";
                lblNewDeferFeatureUpdates.Foreground = Brushes.Gray;
            }

            if (newfetupdate.deferFeatureUpdatesPeriodInDays != fet.deferFeatureUpdatesPeriodInDays)
            {
                lblNewDeferFeatureUpdatesInDays.Content = "Defer feature updates in days: " + newfetupdate.deferFeatureUpdatesPeriodInDays;
                lblNewDeferFeatureUpdatesInDays.Foreground = Brushes.DarkBlue;
            }
            else
            {
                lblNewDeferFeatureUpdatesInDays.Content = "Defer feature updates in days: ";
                lblNewDeferFeatureUpdatesInDays.Foreground = Brushes.Gray;
            }

            if (newfetupdate.branchReadinessLevel != fet.branchReadinessLevel)
            {
                lblNewBranchReadiness.Content = "Branch readiness level: " + newfetupdate.branchReadinessLevel;
                lblNewBranchReadiness.Foreground = Brushes.DarkBlue;
            }
            else
            {
                lblNewBranchReadiness.Content = "Branch readiness level: ";
                lblNewBranchReadiness.Foreground = Brushes.Gray;
            }

            if (newfetupdate.pauseFeatureUpdatesStartTime != fet.pauseFeatureUpdatesStartTime)
            {
                lblNewPauseFeatureUpdatesStartTime.Content = "Pause feature updates start time: " + newfetupdate.pauseFeatureUpdatesStartTime;
                lblNewPauseFeatureUpdatesStartTime.Foreground = Brushes.DarkBlue;
            }
            else
            {
                lblNewPauseFeatureUpdatesStartTime.Content = "Pause feature updates start time: ";
                lblNewPauseFeatureUpdatesStartTime.Foreground = Brushes.Gray;
            }

        }

        private void UpdateQualityUpdateConfig()
        {
            if (chkDeferQualityUpdates.IsChecked == true)
            {
                string deferindays = "30";
                if (cmbDeferQualityInDays.SelectedItem == null)
                {
                    cmbDeferQualityInDays.SelectedItem = deferindays;
                }
                else
                {
                    deferindays = cmbDeferQualityInDays.SelectedItem.ToString();
                }

                newqalupdate = new QualityUpdates { deferQualityUpdates = true, deferQualityUpdatesPeriodInDays = deferindays, pauseQualityUpdatesStartTime = "" };
            }
            else
            {
                newqalupdate = new QualityUpdates { deferQualityUpdates = false, deferQualityUpdatesPeriodInDays = null, pauseQualityUpdatesStartTime = null };
            }

            if (newqalupdate.deferQualityUpdates && !qal.deferQualityUpdates)
            {
                lblNewDeferQualityUpdates.Content = "Defer quality updates: Yes";
                lblNewDeferQualityUpdates.Foreground = Brushes.DarkBlue;
            }
            else if (!newqalupdate.deferQualityUpdates && qal.deferQualityUpdates)
            {
                lblNewDeferQualityUpdates.Content = "Defer quality updates: No";
                lblNewDeferQualityUpdates.Foreground = Brushes.DarkBlue;
            }
            else
            {
                lblNewDeferQualityUpdates.Content = "Defer quality updates: ";
                lblNewDeferQualityUpdates.Foreground = Brushes.Gray;
            }

            if (newqalupdate.deferQualityUpdatesPeriodInDays != qal.deferQualityUpdatesPeriodInDays)
            {
                lblNewDeferQualityUpdatesPeriodInDays.Content = "Defer quality updates in days: " + newqalupdate.deferQualityUpdatesPeriodInDays;
                lblNewDeferQualityUpdatesPeriodInDays.Foreground = Brushes.DarkBlue;
            }
            else
            {
                lblNewDeferQualityUpdatesPeriodInDays.Content = "Defer quality updates in days: ";
                lblNewDeferQualityUpdatesPeriodInDays.Foreground = Brushes.Gray;
            }

            if (newqalupdate.pauseQualityUpdatesStartTime != qal.pauseQualityUpdatesStartTime)
            {
                lblNewPauseQualityUpdatesStartTime.Content = "Pause quality updates start time: " + newqalupdate.pauseQualityUpdatesStartTime;
                lblNewPauseQualityUpdatesStartTime.Foreground = Brushes.DarkBlue;
            }
            else
            {
                lblNewPauseQualityUpdatesStartTime.Content = "Pause quality updates start time: ";
                lblNewPauseQualityUpdatesStartTime.Foreground = Brushes.Gray;
            }

        }

        #endregion

        #region Checkbox

        private void InitCheckBoxUI()
        {
            foreach (Windows10FeatureUpdateInfo update in featureupdates)
            {
                lstFeatureUpdates.Items.Add(update.version + " / " + update.name);
            }

            //Load combo box
            for (int i = 1; i < 366; i++)
            {
                cmbDeferFeatureInDays.Items.Add(i.ToString());
            }

            //Select combo box value based on registry
            if (fet.deferFeatureUpdatesPeriodInDays != null)
            {
                cmbDeferFeatureInDays.SelectedItem = fet.deferFeatureUpdatesPeriodInDays;
            }

            //Select check box values based on registry
            chkDeferFeatureUpdates.IsChecked = fet.deferFeatureUpdates;

            //Load combo box quality
            for (int i = 1; i < 31; i++)
            {
                cmbDeferQualityInDays.Items.Add(i.ToString());
            }

            //Select combo box value based on registry
            if (qal.deferQualityUpdatesPeriodInDays != null)
            {
                cmbDeferQualityInDays.SelectedItem = qal.deferQualityUpdatesPeriodInDays;
            }

            //Select check box if defer quality updates is set
            chkDeferQualityUpdates.IsChecked = qal.deferQualityUpdates;

            //Select check box lock target release
            chkLockTargetRelease.IsChecked = rel.activated;
        }

        private void chkLockTargetRelease_Click(object sender, RoutedEventArgs e)
        {
            UpdateNewTargetRelease();
        }

        private void chkDeferFeatureUpdates_Click(object sender, RoutedEventArgs e)
        {
            UpdateFeatureUpdateConfig();
        }

        private void chkDeferQualityUpdates_Click(object sender, RoutedEventArgs e)
        {
            UpdateQualityUpdateConfig();
        }

        private void LockUnlockUiElements(bool unlock = false)
        {
            btnSaveNewRegistry.IsEnabled = unlock;
            chkLockTargetRelease.IsEnabled = unlock;
            chkDeferFeatureUpdates.IsEnabled = unlock;
            chkDeferQualityUpdates.IsEnabled = unlock;
            cmbDeferFeatureInDays.IsEnabled = unlock;
            cmbDeferQualityInDays.IsEnabled = unlock;
        }

        #endregion

        #region Buttons

        private void btnSaveJsonConfig_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(json.SaveFeatureUpdateConfigToJson());
        }

        private void btnSaveNewRegistry_Click(object sender, RoutedEventArgs e)
        {
            string report = "--- TargetRelease ---\n";

            //Update registry target release
            if (selectedUpdate.version == null)
            {
                report += "Please select a update first";
                chkLockTargetRelease.IsChecked = false;
            }
            else if (chkLockTargetRelease.IsChecked == true && newtargetrel.activated)
            {
                if (selectedUpdate.rank >= info.rank)
                {
                    if(rel.releaseId == newtargetrel.releaseId)
                    {
                        report += "The selected update is already stored in registry";
                    }
                    else
                    {
                        bool ret = reg.SetTargetReleaseReg(newtargetrel);
                        if(ret)
                        {
                            report += "Successful updated registry";
                        }
                        else
                        {
                            report += "Failed to update registry";
                        }
                        
                    }
                    
                }
                else
                {
                    report += "The selected update is older than the current installed update";
                }

            }
            else if(chkLockTargetRelease.IsChecked == false && rel.activated)
            {
                //Remove target release from registry
                bool ret = reg.DelTargetReleaseReg();
                if(ret)
                {
                    report += "Successful removed registry entries";
                }
                else
                {
                    report += "Failed to remove registry entries";
                }
                
            }
            else
            {
                report += "No update of target release";
            }

            //Update registry feature updates
            report += "\n\n--- DeferFeatureUpdates ---\n";
            if(chkDeferFeatureUpdates.IsChecked == true && newfetupdate.deferFeatureUpdates)
            {
                if(newfetupdate.deferFeatureUpdatesPeriodInDays == fet.deferFeatureUpdatesPeriodInDays)
                {
                    report += "The selected value for defer feature updates is already stored";
                }
                else
                {
                    bool ret_fet = reg.SetFeatureUpdatesReg(newfetupdate);
                    if(ret_fet)
                    {
                        report += "Successful updated registry";
                    }
                    else
                    {
                        report += "Failed to update registry";
                    }
                    
                }
            }
            else if(chkDeferFeatureUpdates.IsChecked == false && fet.deferFeatureUpdates)
            {
                //Remove from registry
                bool rem_fet = reg.DelFeatureUpdatesReg();
                if(rem_fet)
                {
                    report += "Successful removed registry entries";
                }
                else
                {
                    report += "Failed to remove registry entries";
                }
                
            }
            else
            {
                report += "No update of feature update registry";
            }

            //Update registry quality updates
            report += "\n\n--- DeferQualityUpdates ---\n";
            if(chkDeferQualityUpdates.IsChecked == true && newqalupdate.deferQualityUpdates)
            {
                if(newqalupdate.deferQualityUpdatesPeriodInDays == qal.deferQualityUpdatesPeriodInDays)
                {
                    report += "The selected value for defer quality updates is already stored";
                }
                else
                {
                    bool ret_qal = reg.SetQualityUpdatesReg(newqalupdate);
                    if(ret_qal)
                    {
                        report += "Successful updated registry";
                    }
                    else
                    {
                        report += "Failed to update registry";
                    }
                    
                }
            }
            else if(chkDeferQualityUpdates.IsChecked == false && qal.deferQualityUpdates)
            {
                //Remove from registry
                bool rem_qal = reg.DelQualityUpdatesReg();
                if(rem_qal)
                {
                    report += "Successful removed registry entries";
                }
                else
                {
                    report += "Failed to remove registry entries";
                }
                
            }
            else
            {
                report += "No update of quality update registry";
            }

            MessageBox.Show(report, "Status registry updates");

            //Reload registry values
            LoadRegistryValues();

            //Update new registry values ui
            UpdateNewTargetRelease();
            UpdateFeatureUpdateConfig();
            UpdateQualityUpdateConfig();

            //Check registry
            CheckUpdateEqualRegistry();
        }

        private void btnUnlockLock_Click(object sender, RoutedEventArgs e)
        {
            if (btnSaveNewRegistry.IsEnabled == false)
            {
                LockUnlockUiElements(true);
                btnUnlockLock.Content = "Lock settings";
            }
            else
            {
                LockUnlockUiElements();
                btnUnlockLock.Content = "Unlock settings";
            }
            
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            string about = "GitHub: https://github.com/withoutaface/StayOnFeatureBranchW10 ";
            
            about += "\n\n";

            about += "MIT License\n\n";

            about += "Copyright 2022 withoutaface\n\n";

            about += "Permission is hereby granted, free of charge, to any person obtaining a copy";
            about += "of this software and associated documentation files(the \"Software\"), to deal ";
            about += "in the Software without restriction, including without limitation the rights";
            about += "to use, copy, modify, merge, publish, distribute, sublicense, and/ or sell";
            about += " copies of the Software, and to permit persons to whom the Software is";
            about += " furnished to do so, subject to the following conditions:\n\n";

            about += "The above copyright notice and this permission notice shall be included in all";
            about += "copies or substantial portions of the Software.\n\n";

            about += "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR";
            about += "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, ";
            about += "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE";
            about += "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER";
            about += "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, ";
            about += "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE";
            about += "SOFTWARE.";

            MessageBox.Show(about, "About");
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            string help = "StayOnFeatureBranchW10 is build for Windows 10 Professional and only supports this edition. The Home version is not supported because registry settings will not affect 'Windows Update' service.";

            help += "\n\nSelect your desired target release and settings to defer feature and qualitiy updates. Use the check boxes to activate the settings. Confirm your choice by clicking the 'Save registry values' button.";

            help += "\n\nFor changes to take effect you have to restart the computer and maybe run 'Windows Update' to install a Windows 10 release. Please note that you can not downgrade to a lower release.";

            help += "\n\nTo remove all registry values done by the tool you can uncheck all three check boxes and confirm by clicking the 'Save registry values' button.";

            help += "\n\nIf anything fails, you can navigate to 'SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate' using regedit. Please report bugs on GitHub: \n\n https://github.com/withoutaface/StayOnFeatureBranchW10/issues";

            MessageBox.Show(help, "Help");
        }

        #endregion

        #region Combo

        private void cmbDeferFeatureInDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFeatureUpdateConfig();
        }

        private void cmbDeferQualityInDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateQualityUpdateConfig();
        }

        #endregion

        private void btnUpdateReleaseInformation_Click(object sender, RoutedEventArgs e)
        {
            //TODO Function is work in progress
            WebConfig webConfig = new WebConfig();

            string content = webConfig.ReadWikiContent();

            MessageBox.Show(content);
        }
    }
}
