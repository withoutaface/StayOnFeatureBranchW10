using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace StayOnFeatureBranchW10
{
    class RegistryRead
    {
        private RegistryKey localMachineReg = null;

        private string _releaseId { get; set; }
        public string releaseId { get { return _releaseId; } }
        private string _productName { get; set; }
        public string productName { get { return _productName; } }
        private string _currentBuild { get; set; }
        public string currentBuild { get { return _currentBuild; } }
        private string _editionId { get; set; }
        public string editionId { get { return _editionId; } }
        private string _computerArchitecture { get; set; }
        public string computerArchitecture { get { return _computerArchitecture; } }
        private string _displayVersion { get; set; }
        public string displayVersion { get { return _displayVersion; } }

        private const string regKeyCurrentVersion = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        
        //Set a test registry entry if in debug mode
        #if DEBUG
        private const string regKeyWindowsUpdate = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdateSTAYONTEST";
        #else
        private const string regKeyWindowsUpdate = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate";
        #endif

        private enum CurrentVersion
        {
            ReleaseId, ProductName, CurrentBuild, EditionID, UBR, DisplayVersion
        }

        private enum WindowsUpdate
        {
            TargetReleaseVersion, TargetReleaseVersionInfo, DeferFeatureUpdates, BranchReadinessLevel, DeferFeatureUpdatesPeriodInDays, PauseFeatureUpdatesStartTime, DeferQualityUpdates, DeferQualityUpdatesPeriodInDays, PauseQualityUpdatesStartTime
        }

        public RegistryRead()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                localMachineReg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

                _computerArchitecture = "x64";

                _releaseId = ReadWindowsNTCurrentVersionKey(CurrentVersion.ReleaseId);

                _productName = ReadWindowsNTCurrentVersionKey(CurrentVersion.ProductName);

                _editionId = ReadWindowsNTCurrentVersionKey(CurrentVersion.EditionID);

                string build = ReadWindowsNTCurrentVersionKey(CurrentVersion.CurrentBuild);

                string ubr = ReadWindowsNTCurrentVersionKey(CurrentVersion.UBR);

                _currentBuild = build + "." + ubr;

                _displayVersion = ReadWindowsNTCurrentVersionKey(CurrentVersion.DisplayVersion);
            }
            else
            {
                _computerArchitecture = "x86";
            }
            
        }

        #region TargetRelease

        public TargetRelease GetTargetReleaseReg()
        {
            TargetRelease ret = new TargetRelease();
            if(ReadPoliciesWindowsUpdateKey(WindowsUpdate.TargetReleaseVersion) == "1")
            {
                ret.activated = true;
            }
            else
            {
                ret.activated = false;
            }

            ret.releaseId = ReadPoliciesWindowsUpdateKey(WindowsUpdate.TargetReleaseVersionInfo);

            return ret;
        }

        public bool SetTargetReleaseReg(TargetRelease target)
        {
            string activated = "0";
            if(target.activated)
            {
                activated = "1";
            }

            bool status_activated = WritePoliciesWindowsUpdateKey(WindowsUpdate.TargetReleaseVersion, activated, RegistryValueKind.DWord);

            bool status_releaseId = WritePoliciesWindowsUpdateKey(WindowsUpdate.TargetReleaseVersionInfo, target.releaseId, RegistryValueKind.String);

            if (status_activated && status_releaseId)
            {
                return true;
            }

            return false;
        }

        public bool DelTargetReleaseReg()
        {
            bool remove_activated = RemovePoliciesWindowsUpdateKey(WindowsUpdate.TargetReleaseVersion);

            bool remove_releaseId = RemovePoliciesWindowsUpdateKey(WindowsUpdate.TargetReleaseVersionInfo);

            if (remove_activated && remove_releaseId)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region FeatureUpdates

        public FeatureUpdates GetFeatureUpdatesReg()
        {
            FeatureUpdates ret = new FeatureUpdates();

            if(ReadPoliciesWindowsUpdateKey(WindowsUpdate.DeferFeatureUpdates) == "1")
            {
                ret.deferFeatureUpdates = true;
            }
            else
            {
                ret.deferFeatureUpdates = false;
            }

            ret.branchReadinessLevel = ReadPoliciesWindowsUpdateKey(WindowsUpdate.BranchReadinessLevel);

            ret.deferFeatureUpdatesPeriodInDays = ReadPoliciesWindowsUpdateKey(WindowsUpdate.DeferFeatureUpdatesPeriodInDays);

            ret.pauseFeatureUpdatesStartTime = ReadPoliciesWindowsUpdateKey(WindowsUpdate.PauseFeatureUpdatesStartTime);

            return ret;
        }

        public bool SetFeatureUpdatesReg(FeatureUpdates feature)
        {
            bool status_defer = WritePoliciesWindowsUpdateKey(WindowsUpdate.DeferFeatureUpdates, feature.deferFeatureUpdates, RegistryValueKind.DWord);

            bool status_days = WritePoliciesWindowsUpdateKey(WindowsUpdate.DeferFeatureUpdatesPeriodInDays, feature.deferFeatureUpdatesPeriodInDays, RegistryValueKind.DWord);

            bool status_pause = WritePoliciesWindowsUpdateKey(WindowsUpdate.PauseFeatureUpdatesStartTime, feature.pauseFeatureUpdatesStartTime, RegistryValueKind.String);

            bool status_readiness = WritePoliciesWindowsUpdateKey(WindowsUpdate.BranchReadinessLevel, feature.branchReadinessLevel, RegistryValueKind.DWord);

            if(status_defer && status_days && status_pause && status_readiness)
            {
                return true;
            }

            return false;
        }

        public bool DelFeatureUpdatesReg()
        {
            bool remove_defer = RemovePoliciesWindowsUpdateKey(WindowsUpdate.DeferFeatureUpdates);

            bool remove_days = RemovePoliciesWindowsUpdateKey(WindowsUpdate.DeferFeatureUpdatesPeriodInDays);

            bool remove_pause = RemovePoliciesWindowsUpdateKey(WindowsUpdate.PauseFeatureUpdatesStartTime);

            bool remove_readiness = RemovePoliciesWindowsUpdateKey(WindowsUpdate.BranchReadinessLevel);

            if(remove_defer && remove_days && remove_pause && remove_readiness)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region QualityUpdates

        public QualityUpdates GetQualityUpdatesReg()
        {
            QualityUpdates ret = new QualityUpdates();

            if(ReadPoliciesWindowsUpdateKey(WindowsUpdate.DeferQualityUpdates) == "1")
            {
                ret.deferQualityUpdates = true;
            }
            else
            {
                ret.deferQualityUpdates = false;
            }

            ret.deferQualityUpdatesPeriodInDays = ReadPoliciesWindowsUpdateKey(WindowsUpdate.DeferQualityUpdatesPeriodInDays);

            ret.pauseQualityUpdatesStartTime = ReadPoliciesWindowsUpdateKey(WindowsUpdate.PauseQualityUpdatesStartTime);

            return ret;
        }

        public bool SetQualityUpdatesReg(QualityUpdates quality)
        {
            bool status_defer = WritePoliciesWindowsUpdateKey(WindowsUpdate.DeferQualityUpdates, quality.deferQualityUpdates, RegistryValueKind.DWord);

            bool status_days = WritePoliciesWindowsUpdateKey(WindowsUpdate.DeferQualityUpdatesPeriodInDays, quality.deferQualityUpdatesPeriodInDays, RegistryValueKind.DWord);

            bool status_pause = WritePoliciesWindowsUpdateKey(WindowsUpdate.PauseQualityUpdatesStartTime, quality.pauseQualityUpdatesStartTime, RegistryValueKind.String);

            if(status_defer && status_days && status_pause)
            {
                return true;
            }

            return false;
        }

        public bool DelQualityUpdatesReg()
        {
            bool remove_defer = RemovePoliciesWindowsUpdateKey(WindowsUpdate.DeferQualityUpdates);

            bool remove_days = RemovePoliciesWindowsUpdateKey(WindowsUpdate.DeferQualityUpdatesPeriodInDays);

            bool remove_pause = RemovePoliciesWindowsUpdateKey(WindowsUpdate.PauseQualityUpdatesStartTime);

            if(remove_defer && remove_days && remove_pause)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Registry

        private string ReadWindowsNTCurrentVersionKey(CurrentVersion keyName)
        {
            if(localMachineReg != null)
            {
                try
                {
                    return localMachineReg.OpenSubKey(regKeyCurrentVersion).GetValue(keyName.ToString()).ToString();
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private string ReadPoliciesWindowsUpdateKey(WindowsUpdate keyName)
        {
            if (localMachineReg != null)
            {
                try
                {
                    return localMachineReg.OpenSubKey(regKeyWindowsUpdate).GetValue(keyName.ToString()).ToString();
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private bool WritePoliciesWindowsUpdateKey(WindowsUpdate keyName, object value, RegistryValueKind valueKind)
        {
            if (localMachineReg != null)
            {
                try
                {
                    RegistryKey key = localMachineReg.OpenSubKey(regKeyWindowsUpdate, true);
                
                    // Create the key if not available
                    if (key == null)
                    {
                        localMachineReg.CreateSubKey(regKeyWindowsUpdate);
                        key = localMachineReg.OpenSubKey(regKeyWindowsUpdate, true);
                    }

                    key.SetValue(keyName.ToString(), value, valueKind);
                    key.Close();
                }
                catch
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool RemovePoliciesWindowsUpdateKey(WindowsUpdate keyName)
        {
            if (localMachineReg != null)
            {
                try
                {
                    RegistryKey key = localMachineReg.OpenSubKey(regKeyWindowsUpdate, true);

                    key.DeleteValue(keyName.ToString());
                    key.Close();
                }
                catch
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }

    public struct TargetRelease
    {
        public bool activated;
        public string releaseId;
    }

    public struct FeatureUpdates
    {
        public bool deferFeatureUpdates;
        public string branchReadinessLevel;
        public string deferFeatureUpdatesPeriodInDays;
        public string pauseFeatureUpdatesStartTime;
    }

    public struct QualityUpdates
    {
        public bool deferQualityUpdates;
        public string deferQualityUpdatesPeriodInDays;
        public string pauseQualityUpdatesStartTime;
    }
}
