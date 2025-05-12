#ifndef iOSHelper_h
#define iOSHelper_h

#ifdef __OBJC__
#import <stdbool.h>
#import <Foundation/Foundation.h>
#endif

#ifdef __cplusplus
extern "C" {
#endif

bool _iOS_RequestStoragePermission();
bool _iOS_OpenApplication(const char* urlScheme);
void _iOS_ShowNotification(const char* message, float duration);
const char* _iOS_GetLaunchParameters();
void _iOS_FreeString(const char* str);
void _iOS_ProcessLaunchOptions(void* options);
bool _iOS_ProcessURL(const char* urlString);
const char* _GetAppGroupForsendPathChecked(void);
void _ReleaseAppGroupPath(const char* path);

#ifdef __cplusplus
}
#endif

#endif