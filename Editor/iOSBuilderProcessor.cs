#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
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
        // 確保目標目錄存在
        string xcodeDestDir = Path.Combine(buildPath, XCODE_DEST_FOLDER);
        if (!Directory.Exists(xcodeDestDir))
        {
            Directory.CreateDirectory(xcodeDestDir);
        }
        
        // 複製橋接頭文件
        string bridgingHeaderDest = Path.Combine(xcodeDestDir, Path.GetFileName(BRIDGING_HEADER_SOURCE));
        File.Copy(Path.Combine(Application.dataPath, "..", BRIDGING_HEADER_SOURCE), bridgingHeaderDest, true);
        
        // 複製Swift文件
        string swiftFileDest = Path.Combine(xcodeDestDir, Path.GetFileName(SWIFT_FILE_SOURCE));
        File.Copy(Path.Combine(Application.dataPath, "..", SWIFT_FILE_SOURCE), swiftFileDest, true);
        
        // 複製C++橋接文件
        string mmFileDest = Path.Combine(xcodeDestDir, Path.GetFileName(MM_FILE_SOURCE));
        File.Copy(Path.Combine(Application.dataPath, "..", MM_FILE_SOURCE), mmFileDest, true);
        
        // 複製頭文件
        string headerFileDest = Path.Combine(xcodeDestDir, Path.GetFileName(HEADER_FILE_SOURCE));
        File.Copy(Path.Combine(Application.dataPath, "..", HEADER_FILE_SOURCE), headerFileDest, true);
        
        Debug.Log($"[iOSPostProcessor] 文件已複製到Xcode項目: {xcodeDestDir}");
    }
    
    private static void ConfigureXcodeProject(string buildPath)
    {
        string projPath = Path.Combine(buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);
        
        // 獲取主目標 - 注意 Unity 版本差異
        string target;
        string frameworkTarget;
        
        #if UNITY_2019_3_OR_NEWER
        target = proj.GetUnityMainTargetGuid(); // Unity 2019.3 及以上版本
        frameworkTarget = proj.GetUnityFrameworkTargetGuid(); // 框架目標
        #else
        target = proj.TargetGuidByName("Unity-iPhone"); // 較舊版本
        frameworkTarget = target;
        #endif
        
        // 向Xcode項目添加文件引用
        string bridgingHeaderFileName = Path.GetFileName(BRIDGING_HEADER_SOURCE);
        string swiftFileName = Path.GetFileName(SWIFT_FILE_SOURCE);
        string mmFileName = Path.GetFileName(MM_FILE_SOURCE);
        string headerFileName = Path.GetFileName(HEADER_FILE_SOURCE);
        
        string bridgingHeaderPath = Path.Combine(XCODE_DEST_FOLDER, bridgingHeaderFileName);
        string swiftFilePath = Path.Combine(XCODE_DEST_FOLDER, swiftFileName);
        string mmFilePath = Path.Combine(XCODE_DEST_FOLDER, mmFileName);
        string headerFilePath = Path.Combine(XCODE_DEST_FOLDER, headerFileName);
        
        // 添加文件到項目
        string bridgingFileGuid = proj.AddFile(bridgingHeaderPath, bridgingHeaderPath);
        string swiftFileGuid = proj.AddFile(swiftFilePath, swiftFilePath, PBXSourceTree.Source);
        string mmFileGuid = proj.AddFile(mmFilePath, mmFilePath, PBXSourceTree.Source);
        string headerFileGuid = proj.AddFile(headerFilePath, headerFilePath);
        
        // 將Swift文件添加到編譯源
        proj.AddFileToBuild(target, swiftFileGuid);
        proj.AddFileToBuild(target, mmFileGuid);

        proj.AddFileToBuild(frameworkTarget, swiftFileGuid);
        
        // Swift 支持設置
        proj.AddBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
        proj.AddBuildProperty(target, "SWIFT_VERSION", "5.0");
        
        // 設置橋接頭文件路徑 - 這是Xcode中相對於項目根目錄的路徑
        string bridgingHeaderRelativePath = bridgingHeaderPath;
        proj.AddBuildProperty(target, "SWIFT_OBJC_BRIDGING_HEADER", bridgingHeaderRelativePath);
        
        // 添加頭文件搜索路徑
        proj.AddBuildProperty(target, "HEADER_SEARCH_PATHS", $"\"$(PROJECT_DIR)/{XCODE_DEST_FOLDER}\"");
        
        // 可能需要設置框架目標
        if (frameworkTarget != target)
        {
            proj.AddBuildProperty(frameworkTarget, "SWIFT_VERSION", "5.0");
            // 確保框架目標也能找到頭文件
            proj.AddBuildProperty(frameworkTarget, "HEADER_SEARCH_PATHS", $"\"$(PROJECT_DIR)/{XCODE_DEST_FOLDER}\"");
        }
        
        // 寫回文件
        proj.WriteToFile(projPath);
        
        Debug.Log($"[iOSPostProcessor] Xcode項目配置已更新: Swift版本=5.0, 橋接頭文件={bridgingHeaderRelativePath}, 已添加頭文件搜索路徑");
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
        string customURLScheme = "xrlab-" + PlayerSettings.productName;
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
            Debug.LogError($"[iOSPostProcessor] 找不到AppDelegate文件: {appDelegatePath}");
            return;
        }
        
        string content = File.ReadAllText(appDelegatePath);
        bool modified = false;
        
        // 在頂部導入我們的頭文件 - 只有在不存在時才添加
        if (!content.Contains("#import \"iOSHelper.h\""))
        {
            int insertPos = content.IndexOf("#import ");
            if (insertPos >= 0)
            {
                content = content.Insert(insertPos, "#import \"iOSHelper.h\"\n");
                modified = true;
            }
        }
        
        // 使用更寬鬆的正則表達式匹配 didFinishLaunchingWithOptions 方法
        Regex didFinishLaunchingRegex = new Regex(@"- \(BOOL\)application:.*didFinishLaunchingWithOptions:.*\{");
        Match didFinishMatch = didFinishLaunchingRegex.Match(content);
        
        // 檢查是否已經插入了啟動處理代碼
        bool hasLaunchCodeInserted = content.Contains("// 處理 AIOT 啟動參數") || 
                                    content.Contains("_iOS_ProcessLaunchOptions");
        
        if (didFinishMatch.Success && !hasLaunchCodeInserted)
        {
            // 找到方法的開括號位置
            int bracePos = content.IndexOf('{', didFinishMatch.Index);
            if (bracePos > 0)
            {
                // 插入我們的代碼在開括號後 - 使用C接口而非直接調用Swift
                string insertCode = "\n    // 處理 AIOT 啟動參數\n" +
                                   "    _iOS_ProcessLaunchOptions((__bridge void*)launchOptions);\n";
                
                content = content.Insert(bracePos + 1, insertCode);
                modified = true;
            }
        }
        
        // 添加 openURL 方法（如果不存在）
        if (!content.Contains("application:openURL:options:") && 
            !content.Contains("_iOS_ProcessURL"))
        {
            // 在文件末尾添加方法 - 使用C接口
            string openURLMethod = "\n// 處理通過 URL Scheme 啟動應用\n" +
                                  "- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options {\n" +
                                  "    return _iOS_ProcessURL([url.absoluteString UTF8String]);\n" +
                                  "}\n";
            
            content += openURLMethod;
            modified = true;
        }
        
        // 添加 continueUserActivity 方法（如果不存在）
        if (!content.Contains("application:continueUserActivity:restorationHandler:") && 
            !content.Contains("_iOS_ProcessURL"))
        {
            // 在文件末尾添加方法 - 使用C接口
            string continueUserActivityMethod = "\n// 處理 Universal Links\n" +
                                             "- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity restorationHandler:(void (^)(NSArray<id<UIUserActivityRestoring>> * _Nullable))restorationHandler {\n" +
                                             "    if ([userActivity.activityType isEqualToString:NSUserActivityTypeBrowsingWeb]) {\n" +
                                             "        return _iOS_ProcessURL([userActivity.webpageURL.absoluteString UTF8String]);\n" +
                                             "    }\n" +
                                             "    return NO;\n" +
                                             "}\n";
            
            content += continueUserActivityMethod;
            modified = true;
        }
        
        // 如果有修改，寫回文件
        if (modified)
        {
            File.WriteAllText(appDelegatePath, content);
            Debug.Log("[iOSPostProcessor] AppDelegate 已更新，添加了使用C接口的URL處理方法");
        }
    }
    
    // 新增方法 - 處理 SceneDelegate
    private static void HandleSceneDelegate(string buildPath)
    {
        // 檢查是否有 SceneDelegate 文件
        string sceneDelegatePath = Path.Combine(buildPath, "Classes/UI/UnitySceneDelegate.mm");
        if (!File.Exists(sceneDelegatePath))
        {
            // 或許在其他位置？嘗試在項目中搜索
            string[] potentialPaths = Directory.GetFiles(buildPath, "UnitySceneDelegate.mm", SearchOption.AllDirectories);
            if (potentialPaths.Length > 0)
            {
                sceneDelegatePath = potentialPaths[0];
            }
            else 
            {
                // 找不到 SceneDelegate，可能需要創建一個
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
            }
        }
        
        // 添加 scene:openURLContexts: 方法（iOS 13+）
        if (!content.Contains("scene:openURLContexts:"))
        {
            // 找到合適位置，如 @implementation 開始後
            int implementationPos = content.IndexOf("@implementation");
            if (implementationPos >= 0)
            {
                // 找到實現結束的位置
                int endPos = content.IndexOf("@end", implementationPos);
                if (endPos > implementationPos)
                {
                    string openURLContextsMethod = "\n// iOS 13+ 處理 URL Scheme\n" +
                                                 "- (void)scene:(UIScene *)scene openURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts {\n" +
                                                 "    for (UIOpenURLContext *context in URLContexts) {\n" +
                                                 "        _iOS_ProcessURL([context.URL.absoluteString UTF8String]);\n" +
                                                 "    }\n" +
                                                 "}\n\n";
                    
                    content = content.Insert(endPos, openURLContextsMethod);
                    modified = true;
                }
            }
        }
        
        // 處理 Universal Links (scene:continueUserActivity:)
        if (!content.Contains("scene:continueUserActivity:"))
        {
            // 找到合適位置
            int implementationPos = content.IndexOf("@implementation");
            if (implementationPos >= 0)
            {
                // 找到實現結束的位置
                int endPos = content.IndexOf("@end", implementationPos);
                if (endPos > implementationPos)
                {
                    string continueUserActivityMethod = "\n// iOS 13+ 處理 Universal Links\n" +
                                                     "- (void)scene:(UIScene *)scene continueUserActivity:(NSUserActivity *)userActivity {\n" +
                                                     "    if ([userActivity.activityType isEqualToString:NSUserActivityTypeBrowsingWeb]) {\n" +
                                                     "        _iOS_ProcessURL([userActivity.webpageURL.absoluteString UTF8String]);\n" +
                                                     "    }\n" +
                                                     "}\n\n";
                    
                    content = content.Insert(endPos, continueUserActivityMethod);
                    modified = true;
                }
            }
        }
        
        // 如果有修改，寫回文件
        if (modified)
        {
            File.WriteAllText(sceneDelegatePath, content);
            Debug.Log("[iOSPostProcessor] SceneDelegate 已更新，添加了iOS 13+的URL處理方法");
        }
    }
    
    private static void ConfigureAppGroup(string buildPath)
    {
        string projPath = Path.Combine(buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);
        
        // 獲取主目標
        string target = proj.GetUnityMainTargetGuid();

        string frameworkTarget = proj.GetUnityFrameworkTargetGuid();

        string entitlementsFile = proj.GetBuildPropertyForAnyConfig(target, "CODE_SIGN_ENTITLEMENTS");
        if (string.IsNullOrEmpty(entitlementsFile))
        {
            entitlementsFile = "UnityAppGroups.entitlements";
        }

        ProjectCapabilityManager capabilityManager = new ProjectCapabilityManager(projPath, entitlementsFile, target);
    
        // 添加 App Groups 能力和群組 ID
        string[] groups = new string[] { "group.com.xrlab.labframe2023" };
        capabilityManager.AddAppGroups(groups);
        
        // 寫回文件
        capabilityManager.WriteToFile();

        if(frameworkTarget != target)
        {
            string frameworkEntitlementsFile = proj.GetBuildPropertyForAnyConfig(frameworkTarget, "CODE_SIGN_ENTITLEMENTS");
            if (string.IsNullOrEmpty(frameworkEntitlementsFile))
            {
                frameworkEntitlementsFile = "UnityFrameworkAppGroups.entitlements";
            }
            
            ProjectCapabilityManager frameworkCapabilityManager = new ProjectCapabilityManager(projPath, frameworkEntitlementsFile, frameworkTarget);
            frameworkCapabilityManager.AddAppGroups(groups);
            frameworkCapabilityManager.WriteToFile();
        }
        
        Debug.Log("[iOSPostProcessor] App Groups 已添加到 Xcode 項目");
    }
}
#endif