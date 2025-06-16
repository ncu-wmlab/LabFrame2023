// Unity-iPhone-Bridging-Header.h
#ifndef Unity_iPhone_Bridging_Header_h
#define Unity_iPhone_Bridging_Header_h

#ifdef __OBJC__
#import <UIKit/UIKit.h>
#endif

#ifdef __cplusplus
extern "C" {
#endif

void UnitySendMessage(const char* obj, const char* method, const char* msg);

#ifdef __cplusplus
}
#endif

#endif /* Unity_iPhone_Bridging_Header_h */