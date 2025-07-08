#import "iOSHelper.h"
#import <UIKit/UIKit.h>
#import <Photos/Photos.h>
#import <UnityFramework/UnityFramework-Swift.h>

// @interface iOSHelperImplementation : NSObject
// + (void)requestPhotoLibraryAccess:(NSString *)callbackGameObject :(NSString *)callbackMethod;
// + (BOOL)openURLScheme:(NSString *)urlScheme;
// + (void)showNotification:(NSString *)message duration:(CGFloat)duration;
// + (NSString *)getLaunchParameters;
// + (void)setLaunchParameters:(NSString *)parameters;
// + (void)processLaunchOptions:(NSDictionary *)options;
// + (void)processIncomingURL:(NSURL *)url;
// + (void)releaseAppGroupPath:(NSString *)path;
// @end

// c language interface
bool _iOS_RequestStoragePermission() {
    // 使用一個預定義的回調對象和方法
    [iOSHelperImplementation requestPhotoLibraryAccess:@"PhotoPermissionCallback" :@"OnPermissionResult"];
    return true; // 這里只返回是否成功發起請求，實際結果通過回調傳遞
}

bool _iOS_OpenApplication(const char* urlScheme) 
{
    NSString *scheme = [NSString stringWithUTF8String:urlScheme];
    return [iOSHelperImplementation openURLScheme:scheme];
}

void _iOS_ShowNotification(const char* message, float duration) 
{
    NSString *messageStr = [NSString stringWithUTF8String:message];
    [iOSHelperImplementation showNotification:messageStr duration:duration];
}

const char* _iOS_GetLaunchParameters() 
{
    NSLog(@"[iOSHelper.mm] _iOS_GetLaunchParameters 被調用");
    NSString *params = [iOSHelperImplementation getLaunchParameters];
    NSLog(@"[iOSHelper.mm] Swift 返回的參數: %@", params);

    if (params == nil || [params length] == 0) {
        NSLog(@"[iOSHelper.mm] 參數為空，返回空字符串");
        return strdup(""); // 返回空字符串而不是 NULL
    }
    // 注意：此處由於返回的字串需由Unity管理，需使用特殊記憶體處理
    // 將Swift字串轉換為C字串並確保記憶體不會被釋放
    const char* cString = [params UTF8String];
    char* result = (char*)malloc(strlen(cString) + 1);
    strcpy(result, cString);
    NSLog(@"[iOSHelper.mm] 返回的 C 字串: %s", result);
    return result;
}

void _iOS_FreeString(const char* str) 
{
    if (str != NULL) {
        // Free memory allocated by malloc
        free((void*)str);
    }
}

void _iOS_ProcessLaunchOptions(void* options) 
{
    // Convert void* to NSDictionary*
    NSDictionary* launchOptions = (__bridge NSDictionary*)options;
    [iOSHelperImplementation processLaunchOptions:launchOptions];
}

bool _iOS_ProcessURL(const char* urlString) 
{
    if (urlString == NULL) return false;
    
    NSString* nsUrlString = [NSString stringWithUTF8String:urlString];
    NSURL* url = [NSURL URLWithString:nsUrlString];
    if (url == nil) return false;
    
    [iOSHelperImplementation processIncomingURL:url];
    return true;
}

const char* _GetAppGroupForsendPathChecked(void) 
{
    NSString *groupID = @"group.com.xrlab.labframe2023";
    NSURL *container = [[NSFileManager defaultManager]
        containerURLForSecurityApplicationGroupIdentifier:groupID];
    if (!container) {
        NSLog(@"[AppGroup] Failed to get container URL for group ID: %@", groupID);
        return NULL;  // entitlement not set or invalid group ID
    }
    NSURL *forsend = [container URLByAppendingPathComponent:@"forsend"];
    NSError *err = nil;
    BOOL ok = [[NSFileManager defaultManager]
        createDirectoryAtURL:forsend
        withIntermediateDirectories:YES
        attributes:nil
        error:&err];
    if (!ok || err) {
        NSLog(@"[AppGroup] Failed to create directory: %@, Error: %@", [forsend path], err);
        return NULL;  // cannot create directory
    }

    const char* path = [[forsend path] UTF8String];
    char* result = (char*)malloc(strlen(path) + 1);
    strcpy(result, path);

    NSLog(@"[AppGroup] Forsend path: %@", [forsend path]);
    
    return result;
}

void _ReleaseAppGroupPath(const char* path) {
    if (path != NULL) {
        NSString *nsPath = [NSString stringWithUTF8String:path];
        // 清理目錄
        NSError *error = nil;
        NSFileManager *fileManager = [NSFileManager defaultManager];
        NSArray *contents = [fileManager contentsOfDirectoryAtPath:nsPath error:&error];
        
        if (error) {
            NSLog(@"[AppGroup] Error getting directory contents: %@", error);
            return;
        }
        
        for (NSString *file in contents) {
            NSString *fullPath = [nsPath stringByAppendingPathComponent:file];
            BOOL success = [fileManager removeItemAtPath:fullPath error:&error];
            if (!success) {
                NSLog(@"[AppGroup] Error removing file %@: %@", fullPath, error);
            }
        }
        
        NSLog(@"[AppGroup] Cleaned directory: %@", nsPath);
    }
}
