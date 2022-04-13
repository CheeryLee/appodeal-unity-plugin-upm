﻿using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using AppodealStack.UnityEditor.Utils;
using AppodealStack.UnityEditor.InternalResources;

#pragma warning disable 618

// ReSharper Disable CheckNamespace
namespace AppodealStack.UnityEditor.PostProcess
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class IosPostprocessUtils : MonoBehaviour
    {
        private const string Suffix = ".framework";
        private const string MinVersionToEnableBitcode = "10.0";

        [PostProcessBuildAttribute(41)]
        public static void UpdateInfoPlist(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget.ToString() != "iOS") return;

            var path = Path.Combine(buildPath, "Info.plist");

            AddAdMobApplicationIdentifier(path);
            AddNsUserTrackingUsageDescription(path);
            AddNsLocationWhenInUseUsageDescription(path);
            AddNsCalendarsUsageDescription(path);
            AddSkAdNetworkIds(buildTarget, buildPath);
            IosPostProcessServices.AddFacebookKeys(path);
        }

        private static void AddSkAdNetworkIds(BuildTarget buildTarget, string buildPath)
        {
            
            if (string.IsNullOrEmpty(PlayerSettings.iOS.targetOSVersionString)) return;

            if (!AppodealSettings.Instance.IosSkAdNetworkItems || (AppodealSettings.Instance.IosSkAdNetworkItemsList?.Count ?? 0) <= 0)  return;

            if (buildTarget != BuildTarget.iOS) return;

            var plistPath = buildPath + "/Info.plist";
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementArray array = null;
            if (plist.root.values.ContainsKey(AppodealUnityUtils.KeySkAdNetworkItems))
            {
                try
                {
                    PlistElement element;
                    plist.root.values.TryGetValue(AppodealUnityUtils.KeySkAdNetworkItems, out element);
                    if (element != null) array = element.AsArray();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    array = null;
                }
            }
            else
            {
                array = plist.root.CreateArray(AppodealUnityUtils.KeySkAdNetworkItems);
            }

            if (array != null)
            {
                foreach (var id in AppodealSettings.Instance.IosSkAdNetworkItemsList)
                {
                    if (ContainsSkAdNetworkIdentifier(array, id)) continue;
                    var added = array.AddDict();
                    added.SetString(AppodealUnityUtils.KeySkAdNetworkID, id);
                }
            }

            File.WriteAllText(plistPath, plist.WriteToString());
        }

        private static void AddKeyToPlist(string path, string key, string value)
        {
            var plist = new PlistDocument();
            plist.ReadFromFile(path);
            plist.root.SetString(key, value);
            File.WriteAllText(path, plist.WriteToString());
        }

        private static bool CheckContainsKey(string path, string key)
        {
            string contentString;
            using (var reader = new StreamReader(path))
            {
                contentString = reader.ReadToEnd();
                reader.Close();
            }

            return contentString.Contains(key);
        }

        private static void AddAdMobApplicationIdentifier(string path)
        {
            if (!File.Exists(Path.Combine(AppodealEditorConstants.PluginPath,
                AppodealEditorConstants.DependenciesPath, "GoogleAdMobDependencies.xml")))
            {
                Debug.LogWarning(
                    "Missing Admob config (Assets/Appodeal/Editor/Dependencies/AdNetworkDependencies/GoogleAdMobDependencies.xml).\nAdmob App Id won't be added.");
                return;
            }

            if (!CheckIosAttribute())
            {
                Debug.LogError(
                    "Google Admob Config is invalid. Ensure that Appodeal Unity plugin is imported correctly.");
                return;
            }

            if (string.IsNullOrEmpty(AppodealSettings.Instance.AdMobIosAppId))
            {
                Debug.LogError(
                    "Admob App ID is not set via 'Appodeal/Appodeal Settings' tool.\nThe app may crash on startup!");
                return;
            }

            if (!AppodealSettings.Instance.AdMobIosAppId.StartsWith("ca-app-pub-"))
            {
                Debug.LogError(
                        "Incorrect value. The app may crash on startup." +
                        "\nPlease enter a valid AdMob App ID via 'Appodeal/Appodeal Settings' tool." +
                        "\nAlternatively, change the value manually in Info.plist file.");
            }

            if (!CheckContainsKey(path, "GADApplicationIdentifier"))
            {
                AddKeyToPlist(path, "GADApplicationIdentifier", AppodealSettings.Instance.AdMobIosAppId);
            }
        }

        private static void AddNsUserTrackingUsageDescription(string path)
        {
            if (!AppodealSettings.Instance.NsUserTrackingUsageDescription) return;
            if (!CheckContainsKey(path, "NSUserTrackingUsageDescription"))
            {
                AddKeyToPlist(path, "NSUserTrackingUsageDescription",
                    "$(PRODUCT_NAME)" + " " +
                    "needs your advertising identifier to provide personalized advertising experience tailored to you.");
            }
        }

        private static void AddNsLocationWhenInUseUsageDescription(string path)
        {
            if (!AppodealSettings.Instance.NsLocationWhenInUseUsageDescription) return;
            if (!CheckContainsKey(path, "NSLocationWhenInUseUsageDescription"))
            {
                AddKeyToPlist(path, "NSLocationWhenInUseUsageDescription",
                    "$(PRODUCT_NAME)" + " " +
                    "needs your location for analytics and advertising purposes.");
            }
        }

        private static void AddNsCalendarsUsageDescription(string path)
        {
            if (!AppodealSettings.Instance.NsCalendarsUsageDescription) return;
            if (!CheckContainsKey(path, "NSCalendarsUsageDescription"))
            {
                AddKeyToPlist(path, "NSCalendarsUsageDescription",
                    "$(PRODUCT_NAME)" + " " +
                    "needs your calendar to provide personalized advertising experience tailored to you.");
            }
        }

        private static void ReplaceInFile(string filePath, string searchText, string replaceText)
        {
            string contentString;
            using (var reader = new StreamReader(filePath))
            {
                contentString = reader.ReadToEnd();
                reader.Close();
            }

            contentString = Regex.Replace(contentString, searchText, replaceText);

            using (var writer = new StreamWriter(filePath))
            {
                writer.Write(contentString);
                writer.Close();
            }
        }

        private static readonly string[] FrameworkList =
        {
            "AdSupport",
            "AudioToolbox",
            "AVFoundation",
            "CFNetwork",
            "CoreFoundation",
            "CoreGraphics",
            "CoreImage",
            "CoreLocation",
            "CoreMedia",
            "CoreMotion",
            "CoreTelephony",
            "CoreText",
            "EventKitUI",
            "EventKit",
            "GLKit",
            "ImageIO",
            "JavaScriptCore",
            "MediaPlayer",
            "MessageUI",
            "MobileCoreServices",
            "QuartzCore",
            "SafariServices",
            "Security",
            "Social",
            "StoreKit",
            "SystemConfiguration",
            "Twitter",
            "UIKit",
            "VideoToolbox",
            "WatchConnectivity",
            "WebKit",
            
        };

        private static readonly string[] WeakFrameworkList =
        {
            "AppTrackingTransparency"
        };

        private static readonly string[] PlatformLibs =
        {
            "libc++.dylib",
            "libz.dylib",
            "libsqlite3.dylib",
            "libxml2.2.dylib"
        };

        public static void PrepareProject(string buildPath)
        {
            Debug.Log("preparing your xcode project for appodeal");
            var projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project = new PBXProject();

            project.ReadFromString(File.ReadAllText(projectPath));

            string firebasePlistPath = Path.Combine(buildPath, "GoogleService-Info.plist");
            if (IosPostProcessServices.AddFirebasePlistFile(buildPath) && File.Exists(firebasePlistPath))
                project.AddFile(firebasePlistPath, "GoogleService-Info.plist", PBXSourceTree.Sdk);

            var target = project.GetUnityMainTargetGuid();
            var unityFrameworkTarget = project.GetUnityFrameworkTargetGuid();

            AddProjectFrameworks(FrameworkList, project, target, false);
            AddProjectFrameworks(WeakFrameworkList, project, target, true);
            AddProjectLibs(PlatformLibs, project, target);
            project.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");

            var xcodeVersion = AppodealUnityUtils.getXcodeVersion();
            if (xcodeVersion == null ||
                AppodealUnityUtils.compareVersions(xcodeVersion, MinVersionToEnableBitcode) >= 0)
            {
                project.SetBuildProperty(target, "ENABLE_BITCODE", "YES");
            }
            else
            {
                project.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            }

            project.AddBuildProperty(target, "LIBRARY_SEARCH_PATHS", "$(SRCROOT)/Libraries");
            project.AddBuildProperty(target, "LIBRARY_SEARCH_PATHS", "$(TOOLCHAIN_DIR)/usr/lib/swift/$(PLATFORM_NAME)");
            project.AddBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            project.AddBuildProperty(unityFrameworkTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            project.AddBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
            project.SetBuildProperty(target, "SWIFT_VERSION", "4.0");

            File.WriteAllText(projectPath, project.WriteToString());
        }

        private static void AddProjectFrameworks(IEnumerable<string> frameworks, PBXProject project, string target,
            bool weak)
        {
            foreach (var framework in frameworks)
            {
                if (!project.ContainsFramework(target, framework))
                {
                    project.AddFrameworkToProject(target, framework + Suffix, weak);
                }
            }
        }

        private static void AddProjectLibs(IEnumerable<string> libs, PBXProject project, string target)
        {
            foreach (var lib in libs)
            {
                var libGuid = project.AddFile("usr/lib/" + lib, "Libraries/" + lib, PBXSourceTree.Sdk);
                project.AddFileToBuild(target, libGuid);
            }
        }

        private static void CopyAndReplaceDirectory(string srcPath, string dstPath)
        {
            if (Directory.Exists(dstPath))
            {
                Directory.Delete(dstPath);
            }

            if (File.Exists(dstPath))
            {
                File.Delete(dstPath);
            }

            Directory.CreateDirectory(dstPath);

            foreach (var file in Directory.GetFiles(srcPath))
            {
                if (!file.Contains(".meta"))
                {
                    File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
                }
            }

            foreach (var dir in Directory.GetDirectories(srcPath))
            {
                CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
            }
        }

        private static bool CheckIosAttribute()
        {
            var adMobConfigPath = Path.Combine(AppodealEditorConstants.PluginPath,
                AppodealEditorConstants.DependenciesPath, "GoogleAdMobDependencies.xml");

            XDocument config;
            try
            {
                config = XDocument.Load(adMobConfigPath);
            }
            catch (IOException exception)
            {
                Debug.LogError(exception.Message);
                return false;
            }

            var elementConfigDependencies = config.Element("dependencies");
            if (elementConfigDependencies == null)
            {
                return false;
            }

            if (!elementConfigDependencies.HasElements)
            {
                return false;
            }

            var elementIosPods = elementConfigDependencies.Element("iosPods");
            if (elementIosPods == null)
            {
                return false;
            }

            if (!elementIosPods.HasElements)
            {
                return false;
            }

            var elementIosPod = elementIosPods.Element("iosPod");
            if (elementIosPod == null)
            {
                return false;
            }

            if (!elementIosPod.HasAttributes)
            {
                return false;
            }

            var attributeElementIosPod = elementIosPod.Attribute("name");

            if (attributeElementIosPod == null)
            {
                return false;
            }

            return attributeElementIosPod.Value.Equals("APDGoogleAdMobAdapter");
        }

        private static bool ContainsSkAdNetworkIdentifier(PlistElementArray skAdNetworkItemsArray, string id)
        {
            foreach (var elem in skAdNetworkItemsArray.values)
            {
                try
                {
                    PlistElement value;
                    var identifierExists = elem.AsDict().values
                        .TryGetValue(AppodealUnityUtils.KeySkAdNetworkID, out value);

                    if (identifierExists && value.AsString().Equals(id))
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

            return false;
        }
    }
}
