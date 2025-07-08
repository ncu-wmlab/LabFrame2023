import Foundation
import UIKit
import Photos

@_silgen_name("UnitySendMessage")
func UnitySendMessage(_ objectName: UnsafePointer<CChar>, _ methodName: UnsafePointer<CChar>, _ message: UnsafePointer<CChar>) -> Void

@objc public class iOSHelperImplementation: NSObject {
    
    // to store launch parameters
    private static var cachedLaunchParameters: String?
    
    // MARK: - storage management of photos
    
    @objc public static func requestPhotoLibraryAccess(_ callbackGameObject: String,_ callbackMethod: String) {
        PHPhotoLibrary.requestAuthorization { status in
            let result = (status == .authorized)
            UnitySendMessage(callbackGameObject, callbackMethod, result ? "true" : "false")
        }
        
    }
    
    // MARK: - startup URL Scheme for application
    
    @objc public static func openURLScheme(_ urlScheme: String) -> Bool {
        guard let url = URL(string: urlScheme) else { return false }
        
        if #available(iOS 10.0, *) {
            var result = false
            let semaphore = DispatchSemaphore(value: 0)
            
            UIApplication.shared.open(url, options: [:]) { success in
                result = success
                semaphore.signal()
            }
            
            _ = semaphore.wait(timeout: .distantFuture)
            return result
        } else {
            return UIApplication.shared.openURL(url)
        }
    }
    
    // MARK: - show notification
    
    @objc public static func showNotification(_ message: String, duration: CGFloat) {
        // ensure the UI is updated on the main thread
        DispatchQueue.main.async {
            let alert = UIAlertController(title: nil, message: message, preferredStyle: .alert)
            
            // get the top view controller to present the alert
            let window = UIApplication.shared.windows.filter { $0.isKeyWindow }.first
            var topVC = window?.rootViewController
            while let presentedVC = topVC?.presentedViewController {
                topVC = presentedVC
            }
            
            topVC?.present(alert, animated: true)
            
            // set a timer to dismiss the alert after the specified duration
            DispatchQueue.main.asyncAfter(deadline: .now() + Double(duration)) {
                alert.dismiss(animated: true)
            }
        }
    }
    
    // MARK: - launch parameters management
    
    @objc public static func setLaunchParameters(_ parameters: String) {
        cachedLaunchParameters = parameters
    }
    
    @objc public static func getLaunchParameters() -> String {
        let result = cachedLaunchParameters ?? ""
        print("Swift: getLaunchParameters() 被呼叫")
        print("Swift: cachedLaunchParameters = '\(result)'")
        print("Swift: 回傳結果長度: \(result.count)")
        return result
    }
    
    // MARK: - parameters processing (需在AppDelegate中集成)
    
    // This method needs to be called from AppDelegate
    @objc public static func processLaunchOptions(_ options: [UIApplication.LaunchOptionsKey: Any]?) {
        // URL Scheme
        if let url = options?[UIApplication.LaunchOptionsKey.url] as? URL {
            processIncomingURL(url)
        }

        // universal link
        if let activityDictionary = options?[UIApplication.LaunchOptionsKey.userActivityDictionary] as? [AnyHashable: Any],
           let activity = activityDictionary["UIApplicationLaunchOptionsUserActivityKey"] as? NSUserActivity,
           activity.activityType == NSUserActivityTypeBrowsingWeb,
           let webpageURL = activity.webpageURL 
        {
                processIncomingURL(webpageURL)
        }
    }
    
    @objc public static func processIncomingURL(_ url: URL) 
    {
        print("=== Swift Debug Start ===")
        print("Swift: 收到 URL: \(url.absoluteString)")
        print("Swift: URL Scheme: \(url.scheme ?? "nil")")
        print("Swift: URL Host: \(url.host ?? "nil")")
        print("Swift: URL Path: \(url.path)")
        print("Swift: URL Query: \(url.query ?? "nil")")
        
        if let components = URLComponents(url: url, resolvingAgainstBaseURL: false) {
            print("Swift: ✓ URL 組件解析成功")
            var paramDict = [String: String]()
            
            if let queryItems = components.queryItems {
                print("Swift: ✓ 查詢項目數量: \(queryItems.count)")
                for (index, item) in queryItems.enumerated() {
                    print("Swift: 參數[\(index)] \(item.name) = '\(item.value ?? "nil")'")
                    paramDict[item.name] = item.value
                }
            } else {
                print("Swift: ✗ 沒有查詢項目")
            }
            
            print("Swift: 準備序列化的參數字典: \(paramDict)")
            
            if !paramDict.isEmpty {
                do {
                    let jsonData = try JSONSerialization.data(withJSONObject: paramDict)
                    if let jsonString = String(data: jsonData, encoding: .utf8) {
                        cachedLaunchParameters = jsonString
                        print("Swift: ✓ JSON 序列化成功: \(jsonString)")
                        print("Swift: ✓ 參數已快取到 cachedLaunchParameters")
                        
                        // 嘗試立即通知 Unity
                        print("Swift: 嘗試呼叫 UnitySendMessage...")
                        UnitySendMessage("TestPara", "OnURLParametersReceived", jsonString)
                        print("Swift: ✓ UnitySendMessage 呼叫完成")
                    } else {
                        print("Swift: ✗ 無法將 JSON Data 轉換為 String")
                    }
                } catch {
                    print("Swift: ✗ JSON 序列化錯誤: \(error)")
                    print("Swift: ✗ 錯誤詳情: \(error.localizedDescription)")
                }
            } else {
                print("Swift: ✗ 參數字典為空，沒有可序列化的內容")
            }
        } else {
            print("Swift: ✗ URL 組件解析失敗")
            print("Swift: ✗ 原始 URL: \(url.absoluteString)")
        }
        
        print("=== Swift Debug End ===")
    }

    // MARK: - App Groups Support

    @objc public static func getAppGroupForsendPathChecked() -> String {
        // 檢查 App Groups 容器是否可用
        guard let containerURL = FileManager.default.containerURL(forSecurityApplicationGroupIdentifier: "group.com.xrlab.labframe2023") else {
            return ""
        }
        
        // 創建用於分享的目錄
        let sendPath = containerURL.appendingPathComponent("ForSend").path
        
        // 確保目錄存在
        if !FileManager.default.fileExists(atPath: sendPath) {
            do {
                try FileManager.default.createDirectory(atPath: sendPath, withIntermediateDirectories: true, attributes: nil)
            } catch {
                print("Error creating directory: \(error)")
                return ""
            }
        }
        
        return sendPath
    }

    @objc public static func releaseAppGroupPath(_ path: String)
    {
        // 清理臨時文件
        if FileManager.default.fileExists(atPath: path) {
            do {
                let contents = try FileManager.default.contentsOfDirectory(atPath: path)
                for file in contents {
                    let filePath = (path as NSString).appendingPathComponent(file)
                    try FileManager.default.removeItem(atPath: filePath)
                }
            } catch {
                print("Error cleaning directory: \(error)")
            }
        }
    }

    @objc public static func freeString(_ str: String) {
        // 釋放字符串
        //free(str)
    }
}