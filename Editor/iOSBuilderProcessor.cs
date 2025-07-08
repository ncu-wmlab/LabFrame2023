#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System;
using UnityEngine;

public class iOSPostProcessor
{
    // 橋接頭文件相對於Unity項目的路徑
    private const string BRIDGING_HEADER_SOURCE = "Packages/LabFrame2023/Plugins/iOS/Unity_IOS_Bridging_Header.h";
    
    // Swift文件相對於Unity項目的路徑
    private const string SWIFT_FILE_SOURCE = "Packages/LabFrame2023/Plugins/iOS/iOSHelperImpl.swift";
    
    // C++橋接文件相對於Unity項目的路徑
    private const string MM_FILE_SOURCE = "Packages/LabFrame2023/Plugins/iOS/iOSHelper.mm";
    
    // 頭文件相對於Unity項目的路徑
    private const string HEADER_FILE_SOURCE = "Packages/LabFrame2023/Plugins/iOS/iOSHelper.h";
    
    // Xcode項目中的目標路徑
    private const string XCODE_DEST_FOLDER = "Libraries/LabFrame2023";
    
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // 複製文件到Xcode項目
            CopyFilesToXcode(buildPath);
            
            // 配置 Xcode 項目
            ConfigureXcodeProject(buildPath);
            
            // 配置 Info.plist
            ConfigureInfoPlist(buildPath);
            
            // 修改 AppDelegate
            UpdateAppDelegate(buildPath);
            
            // 處理 SceneDelegate
            HandleSceneDelegate(buildPath);

            // 配置 App Group
            ConfigureAppGroup(buildPath);
        }
    }
    
    private static void CopyFilesToXcode(string buildPath)
    {
        Debug.Log("[iOSPostProcessor] 開始檢查與複製檔案...");
        
        // 確保目標目錄存在
        string xcodeDestDir = Path.Combine(buildPath, XCODE_DEST_FOLDER);
        if (!Directory.Exists(xcodeDestDir))
        {
            Directory.CreateDirectory(xcodeDestDir);
            Debug.Log($"[iOSPostProcessor] 創建目錄: {xcodeDestDir}");
        }
        
        // 檢查Unity自動添加的檔案路徑
        string labframePluginsPath = Path.Combine(buildPath, "Libraries/com.xrlab.labframe/Plugins/iOS");
        bool labframePathExists = Directory.Exists(labframePluginsPath);
        
        // 檢查每個檔案並記錄結果
        string bridgingHeaderFileName = Path.GetFileName(BRIDGING_HEADER_SOURCE);
        string swiftFileName = Path.GetFileName(SWIFT_FILE_SOURCE);
        string mmFileName = Path.GetFileName(MM_FILE_SOURCE);
        string headerFileName = Path.GetFileName(HEADER_FILE_SOURCE);
        
        Debug.Log($"[iOSPostProcessor] 檢查四個關鍵檔案是否已存在...");
        
        // 檢查 com.xrlab.labframe/Plugins/iOS 目錄中是否已有檔案
        bool bridgingHeaderExistsInPackage = labframePathExists && File.Exists(Path.Combine(labframePluginsPath, bridgingHeaderFileName));
        bool swiftFileExistsInPackage = labframePathExists && File.Exists(Path.Combine(labframePluginsPath, swiftFileName));
        bool mmFileExistsInPackage = labframePathExists && File.Exists(Path.Combine(labframePluginsPath, mmFileName));
        bool headerFileExistsInPackage = labframePathExists && File.Exists(Path.Combine(labframePluginsPath, headerFileName));
        
        Debug.Log($"[iOSPostProcessor] 橋接頭文件存在於套件路徑: {bridgingHeaderExistsInPackage}");
        Debug.Log($"[iOSPostProcessor] Swift檔案存在於套件路徑: {swiftFileExistsInPackage}");
        Debug.Log($"[iOSPostProcessor] MM檔案存在於套件路徑: {mmFileExistsInPackage}");
        Debug.Log($"[iOSPostProcessor] 頭文件存在於套件路徑: {headerFileExistsInPackage}");
        
        // 檢查目標目錄中是否已有檔案
        bool bridgingHeaderExistsInDest = File.Exists(Path.Combine(xcodeDestDir, bridgingHeaderFileName));
        bool swiftFileExistsInDest = File.Exists(Path.Combine(xcodeDestDir, swiftFileName));
        bool mmFileExistsInDest = File.Exists(Path.Combine(xcodeDestDir, mmFileName));
        bool headerFileExistsInDest = File.Exists(Path.Combine(xcodeDestDir, headerFileName));
        
        Debug.Log($"[iOSPostProcessor] 橋接頭文件存在於目標路徑: {bridgingHeaderExistsInDest}");
        Debug.Log($"[iOSPostProcessor] Swift檔案存在於目標路徑: {swiftFileExistsInDest}");
        Debug.Log($"[iOSPostProcessor] MM檔案存在於目標路徑: {mmFileExistsInDest}");
        Debug.Log($"[iOSPostProcessor] 頭文件存在於目標路徑: {headerFileExistsInDest}");
        
        // 如果檔案已經存在於套件路徑，我們不需要複製到自訂目錄
        if (swiftFileExistsInPackage)
        {
            Debug.Log("[iOSPostProcessor] 檢測到Swift檔案已存在於Unity自動添加的路徑，將跳過複製階段");
            return; // 直接返回，不再進行後續檔案操作
        }
        
        // 只有在套件路徑不存在檔案的情況下才進行複製
        // 複製橋接頭文件
        if (!bridgingHeaderExistsInDest && !bridgingHeaderExistsInPackage)
        {
            try
            {
                File.Copy(Path.Combine(Application.dataPath, "..", BRIDGING_HEADER_SOURCE), Path.Combine(xcodeDestDir, bridgingHeaderFileName), true);
                Debug.Log($"[iOSPostProcessor] 成功複製橋接頭文件到: {xcodeDestDir}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[iOSPostProcessor] 複製橋接頭文件時發生錯誤: {ex.Message}");
            }
        }
        
        // 複製Swift文件
        if (!swiftFileExistsInDest && !swiftFileExistsInPackage)
        {
            try
            {
                File.Copy(Path.Combine(Application.dataPath, "..", SWIFT_FILE_SOURCE), Path.Combine(xcodeDestDir, swiftFileName), true);
                Debug.Log($"[iOSPostProcessor] 成功複製Swift文件到: {xcodeDestDir}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[iOSPostProcessor] 複製Swift文件時發生錯誤: {ex.Message}");
            }
        }
        
        // 複製C++橋接文件
        if (!mmFileExistsInDest && !mmFileExistsInPackage)
        {
            try
            {
                File.Copy(Path.Combine(Application.dataPath, "..", MM_FILE_SOURCE), Path.Combine(xcodeDestDir, mmFileName), true);
                Debug.Log($"[iOSPostProcessor] 成功複製C++橋接文件到: {xcodeDestDir}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[iOSPostProcessor] 複製C++橋接文件時發生錯誤: {ex.Message}");
            }
        }
        
        // 複製頭文件
        if (!headerFileExistsInDest && !headerFileExistsInPackage)
        {
            try
            {
                File.Copy(Path.Combine(Application.dataPath, "..", HEADER_FILE_SOURCE), Path.Combine(xcodeDestDir, headerFileName), true);
                Debug.Log($"[iOSPostProcessor] 成功複製頭文件到: {xcodeDestDir}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[iOSPostProcessor] 複製頭文件時發生錯誤: {ex.Message}");
            }
        }
        
        Debug.Log("[iOSPostProcessor] 文件複製階段完成");
    }
    
    private static void ConfigureXcodeProject(string buildPath)
    {
        Debug.Log("[iOSPostProcessor] 開始配置Xcode項目...");
        
        string projPath = Path.Combine(buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);
        
        // 獲取主目標 - 注意 Unity 版本差異
        string target;
        string frameworkTarget;
        
        #if UNITY_2019_3_OR_NEWER
        target = proj.GetUnityMainTargetGuid(); 
        frameworkTarget = proj.GetUnityFrameworkTargetGuid();
        #else
        target = proj.TargetGuidByName("Unity-iPhone");
        frameworkTarget = target;
        #endif
        
        // 檢查Unity自動添加的檔案路徑
        string labframePluginsPath = "Libraries/com.xrlab.labframe/Plugins/iOS";
        bool labframePathExists = Directory.Exists(Path.Combine(buildPath, labframePluginsPath));
        
        // 決定使用哪個路徑
        string usePath = labframePathExists ? labframePluginsPath : XCODE_DEST_FOLDER;
        Debug.Log($"[iOSPostProcessor] 使用路徑: {usePath} 進行Xcode項目配置");
        
        // 檔案名稱
        string bridgingHeaderFileName = Path.GetFileName(BRIDGING_HEADER_SOURCE);
        string swiftFileName = Path.GetFileName(SWIFT_FILE_SOURCE);
        string mmFileName = Path.GetFileName(MM_FILE_SOURCE);
        string headerFileName = Path.GetFileName(HEADER_FILE_SOURCE);
        
        // 完整路徑
        string bridgingHeaderPath = Path.Combine(usePath, bridgingHeaderFileName);
        string swiftFilePath = Path.Combine(usePath, swiftFileName);
        string mmFilePath = Path.Combine(usePath, mmFileName);
        string headerFilePath = Path.Combine(usePath, headerFileName);
        
        // Swift 支持設置
        proj.AddBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
        proj.AddBuildProperty(target, "SWIFT_VERSION", "5.0");
        
        // 設置橋接頭文件路徑
        proj.AddBuildProperty(target, "SWIFT_OBJC_BRIDGING_HEADER", bridgingHeaderPath);
        
        // 添加頭文件搜索路徑
        proj.AddBuildProperty(target, "HEADER_SEARCH_PATHS", $"\"$(PROJECT_DIR)/{usePath}\"");
        
        // 確保框架目標也有正確設置
        if (frameworkTarget != target)
        {
            proj.AddBuildProperty(frameworkTarget, "SWIFT_VERSION", "5.0");
            proj.AddBuildProperty(frameworkTarget, "HEADER_SEARCH_PATHS", $"\"$(PROJECT_DIR)/{usePath}\"");
        }
        
        // 寫回文件
        proj.WriteToFile(projPath);
        
        Debug.Log($"[iOSPostProcessor] Xcode項目配置已更新: Swift版本=5.0, 橋接頭文件={bridgingHeaderPath}");
    }
    
    private static void ConfigureInfoPlist(string buildPath)
    {
        string plistPath = Path.Combine(buildPath, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        
        // 添加照片庫權限說明
        PlistElementDict rootDict = plist.root;
        if (!rootDict.values.ContainsKey("NSPhotoLibraryUsageDescription"))
        {
            rootDict.SetString("NSPhotoLibraryUsageDescription", 
                              "此應用需要訪問您的照片庫以保存和讀取實驗數據");
        }

        // 添加 URL Scheme 支援 - 檢查是否已存在
        string customURLScheme = "xrlab-" + PlayerSettings.productName.ToLower();
        bool schemeExists = false;
        
        // 檢查是否已有 URL Types
        PlistElementArray urlTypes;
        if (rootDict.values.ContainsKey("CFBundleURLTypes"))
        {
            urlTypes = rootDict.values["CFBundleURLTypes"].AsArray();
            
            // 檢查是否已存在相同的URL Scheme
            foreach (var urlTypeElement in urlTypes.values)
            {
                PlistElementDict urlTypeDict = urlTypeElement.AsDict();
                if (urlTypeDict.values.ContainsKey("CFBundleURLSchemes"))
                {
                    PlistElementArray schemes = urlTypeDict.values["CFBundleURLSchemes"].AsArray();
                    foreach (var schemeElement in schemes.values)
                    {
                        if (schemeElement.AsString() == customURLScheme)
                        {
                            schemeExists = true;
                            break;
                        }
                    }
                }
                if (schemeExists) break;
            }
        }
        else
        {
            urlTypes = rootDict.CreateArray("CFBundleURLTypes");
        }
        
        // 只有在URL Scheme不存在時才添加
        if (!schemeExists)
        {
            // 創建新的 URL Type
            PlistElementDict urlDict = urlTypes.AddDict();
            urlDict.SetString("CFBundleURLName", PlayerSettings.applicationIdentifier);
            
            PlistElementArray urlSchemes = urlDict.CreateArray("CFBundleURLSchemes");
            // 添加自定義 URL Scheme，使用類似 "aiot-{bundleId}" 的格式
            urlSchemes.AddString(customURLScheme);
            
            Debug.Log($"[iOSPostProcessor] 已添加URL Scheme: {customURLScheme}");
        }
        
        // 寫回文件
        plist.WriteToFile(plistPath);
        
        Debug.Log("[iOSPostProcessor] Info.plist 已更新，確保照片庫權限和URL Scheme已添加");
    }
    
    private static void UpdateAppDelegate(string buildPath)
    {
        string appDelegatePath = Path.Combine(buildPath, "Classes/UnityAppController.mm");
        if (!File.Exists(appDelegatePath))
        {
            Debug.LogError($"[iOSPostProcessor] UnityAppController.mm not found");
            return;
        }

        string content = File.ReadAllText(appDelegatePath);
        bool modified = false;

        // 1. 確保 import 存在
        if (!content.Contains("#import \"iOSHelper.h\""))
        {
            int firstImportPos = content.IndexOf("#import ");
            if (firstImportPos >= 0)
            {
                content = content.Insert(firstImportPos, "#import \"iOSHelper.h\"\n");
                modified = true;
                Debug.Log("[iOSPostProcessor] 已添加 iOSHelper.h import");
            }
        }

        // 2. 添加啟動處理
        if (!content.Contains("_iOS_ProcessLaunchOptions"))
        {
            string searchPattern = "- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions";
            int methodPos = content.IndexOf(searchPattern);
            if (methodPos >= 0)
            {
                int bracePos = content.IndexOf('{', methodPos);
                if (bracePos > 0)
                {
                    string insertCode = "\n    // 處理 AIOT 啟動參數\n" +
                                    "    _iOS_ProcessLaunchOptions((__bridge void*)launchOptions);\n";
                    content = content.Insert(bracePos + 1, insertCode);
                    modified = true;
                    Debug.Log("[iOSPostProcessor] 已添加啟動參數處理");
                }
            }
        }

        // 3. 修改 openURL 方法 - 使用精確的文本搜索和替換
        string openURLPattern = "AppController_SendNotificationWithArg(kUnityOnOpenURL, notifData);\n    return YES;";
        if (content.Contains(openURLPattern) && !content.Contains("_iOS_ProcessURL"))
        {
            string replacement = "AppController_SendNotificationWithArg(kUnityOnOpenURL, notifData);\n    \n    // LabFrame2023 URL 處理\n    _iOS_ProcessURL([url.absoluteString UTF8String]);\n    \n    return YES;";
            content = content.Replace(openURLPattern, replacement);
            modified = true;
            Debug.Log("[iOSPostProcessor] 已修改 openURL 方法");
        }

        // 4. 修改 continueUserActivity 方法 - 如果還沒修改
        string continuePattern = "if (url)\n        UnitySetAbsoluteURL(url.absoluteString.UTF8String);\n    return YES;";
        if (content.Contains(continuePattern) && !content.Contains("_iOS_ProcessURL.*continueUserActivity"))
        {
            string replacement = "if (url)\n        UnitySetAbsoluteURL(url.absoluteString.UTF8String);\n    \n    // LabFrame2023 處理\n    if ([userActivity.activityType isEqualToString:NSUserActivityTypeBrowsingWeb]) {\n        _iOS_ProcessURL([userActivity.webpageURL.absoluteString UTF8String]);\n    }\n    \n    return YES;";
            content = content.Replace(continuePattern, replacement);
            modified = true;
            Debug.Log("[iOSPostProcessor] 已修改 continueUserActivity 方法");
        }

        if (modified)
        {
            File.WriteAllText(appDelegatePath, content);
            Debug.Log("[iOSPostProcessor] UnityAppController.mm 更新完成");
        }
        else
        {
            Debug.Log("[iOSPostProcessor] 無需修改或已包含必要代碼");
        }
    }
        
    // 新增方法 - 處理 SceneDelegate
    private static void HandleSceneDelegate(string buildPath)
    {
        // 檢查是否有 SceneDelegate 文件
        string sceneDelegatePath = Path.Combine(buildPath, "Classes/UI/UnitySceneDelegate.mm");
        if (!File.Exists(sceneDelegatePath))
        {
            // 搜索整個項目
            string[] potentialPaths = Directory.GetFiles(buildPath, "UnitySceneDelegate.mm", SearchOption.AllDirectories);
            if (potentialPaths.Length > 0)
            {
                sceneDelegatePath = potentialPaths[0];
                Debug.Log($"[iOSPostProcessor] 找到 SceneDelegate 在: {sceneDelegatePath}");
            }
            else 
            {
                Debug.Log("[iOSPostProcessor] 找不到 SceneDelegate 文件，iOS 13+ 可能會有問題");
                return;
            }
        }
        
        string content = File.ReadAllText(sceneDelegatePath);
        bool modified = false;
        
        // 檢查是否已經導入了我們的頭文件
        if (!content.Contains("#import \"iOSHelper.h\""))
        {
            int insertPos = content.IndexOf("#import ");
            if (insertPos >= 0)
            {
                content = content.Insert(insertPos, "#import \"iOSHelper.h\"\n");
                modified = true;
                Debug.Log("[iOSPostProcessor] 已添加iOSHelper.h導入到SceneDelegate");
            }
        }
        
        // 使用正則表達式檢查方法是否已存在
        bool hasOpenURLContextsMethod = Regex.IsMatch(content, @"- \s*\(\s*void\s*\)\s*scene\s*:\s*\(\s*UIScene\s*\*\s*\)\s*\w+\s+openURLContexts\s*:");
        bool hasContinueUserActivityMethod = Regex.IsMatch(content, @"- \s*\(\s*void\s*\)\s*scene\s*:\s*\(\s*UIScene\s*\*\s*\)\s*\w+\s+continueUserActivity\s*:");
        
        Debug.Log($"[iOSPostProcessor] SceneDelegate現有方法檢查: openURLContexts方法={hasOpenURLContextsMethod}, continueUserActivity方法={hasContinueUserActivityMethod}");
        
        // 查找 @implementation UnitySceneDelegate 區域
        int implementationPos = content.IndexOf("@implementation");
        if (implementationPos >= 0)
        {
            string className = "";
            // 嘗試提取類名
            Regex classNameRegex = new Regex(@"@implementation\s+(\w+)");
            Match classNameMatch = classNameRegex.Match(content, implementationPos);
            if (classNameMatch.Success && classNameMatch.Groups.Count > 1)
            {
                className = classNameMatch.Groups[1].Value;
                Debug.Log($"[iOSPostProcessor] 找到 SceneDelegate 類名: {className}");
            }
            
            // 找到實現結束的位置
            int endPos = content.IndexOf("@end", implementationPos);
            if (endPos > implementationPos)
            {
                StringBuilder methodsToAdd = new StringBuilder();
                
                // 只在需要時添加方法
                if (!hasOpenURLContextsMethod)
                {
                    methodsToAdd.Append("\n// iOS 13+ 處理 URL Scheme\n");
                    methodsToAdd.Append("- (void)scene:(UIScene *)scene openURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts {\n");
                    methodsToAdd.Append("    for (UIOpenURLContext *context in URLContexts) {\n");
                    methodsToAdd.Append("        _iOS_ProcessURL([context.URL.absoluteString UTF8String]);\n");
                    methodsToAdd.Append("    }\n");
                    methodsToAdd.Append("}\n\n");
                    Debug.Log("[iOSPostProcessor] 將添加openURLContexts方法");
                }
                
                if (!hasContinueUserActivityMethod)
                {
                    methodsToAdd.Append("\n// iOS 13+ 處理 Universal Links\n");
                    methodsToAdd.Append("- (void)scene:(UIScene *)scene continueUserActivity:(NSUserActivity *)userActivity {\n");
                    methodsToAdd.Append("    if ([userActivity.activityType isEqualToString:NSUserActivityTypeBrowsingWeb]) {\n");
                    methodsToAdd.Append("        _iOS_ProcessURL([userActivity.webpageURL.absoluteString UTF8String]);\n");
                    methodsToAdd.Append("    }\n");
                    methodsToAdd.Append("}\n\n");
                    Debug.Log("[iOSPostProcessor] 將添加continueUserActivity方法");
                }
                
                if (methodsToAdd.Length > 0)
                {
                    content = content.Insert(endPos, methodsToAdd.ToString());
                    modified = true;
                }
            }
            else
            {
                Debug.LogError("[iOSPostProcessor] 找不到 SceneDelegate 實現的結束處");
            }
        }
        else
        {
            Debug.LogError("[iOSPostProcessor] 找不到 SceneDelegate 實現區域");
        }
        
        // 如果有修改，寫回文件
        if (modified)
        {
            File.WriteAllText(sceneDelegatePath, content);
            Debug.Log("[iOSPostProcessor] SceneDelegate 已更新");
        }
        else
        {
            Debug.Log("[iOSPostProcessor] SceneDelegate 不需要更新，所有必要的方法已存在");
        }
    }
    
    private static void ConfigureAppGroup(string buildPath)
    {
        string projPath = Path.Combine(buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);
        
        // 獲取主目標
        // string target = proj.GetUnityMainTargetGuid();
        string targetName = "Unity-iPhone";
        string entitlementsFile = "UnityAppGroups.entitlements";

        string frameworkTarget = proj.GetUnityFrameworkTargetGuid();

        // string entitlementsFile = proj.GetBuildPropertyForAnyConfig(target, "CODE_SIGN_ENTITLEMENTS");
        if (string.IsNullOrEmpty(entitlementsFile))
        {
            entitlementsFile = "UnityAppGroups.entitlements";
        }

        ProjectCapabilityManager capabilityManager = new ProjectCapabilityManager(projPath, entitlementsFile, targetName);

        // 添加 App Groups 能力和群組 ID
        string[] groups = new string[] { "group.com.xrlab.labframe2023" };
        capabilityManager.AddAppGroups(groups);
        
        // 寫回文件
        capabilityManager.WriteToFile();

        // if(frameworkTarget != target)
        // {
        //     string frameworkEntitlementsFile = proj.GetBuildPropertyForAnyConfig(frameworkTarget, "CODE_SIGN_ENTITLEMENTS");
        //     if (string.IsNullOrEmpty(frameworkEntitlementsFile))
        //     {
        //         frameworkEntitlementsFile = "UnityFrameworkAppGroups.entitlements";
        //     }
            
        //     ProjectCapabilityManager frameworkCapabilityManager = new ProjectCapabilityManager(projPath, frameworkEntitlementsFile, frameworkTarget);
        //     frameworkCapabilityManager.AddAppGroups(groups);
        //     frameworkCapabilityManager.WriteToFile();
        // }
        
        Debug.Log("[iOSPostProcessor] App Groups 已添加到 Xcode 項目");
    }
}
#endif