# LabFrame 2023

> **套件識別碼：** `com.xrlab.labframe`  
> **版本：** 0.0.19  
> **Unity 版本：** 2020.3.33f1+

這是由中央大學無線多媒體實驗室（WMLAB）與延展實境實驗室（XRLAB）共同開發的 Unity 框架，專為實驗室研究應用而設計，具備 AIOT（人工智慧物聯網）整合、強健的資料管理，以及跨平台部署能力。

## 概述

LabFrame2023 是從 LabFrame2022 演進而來的研究框架，專門為需要在 Unity 應用程式與 AIOT 平台之間進行無縫整合的研究環境而設計。這個框架提供了標準化的資料收集、儲存和傳輸方法，同時支援包括 Android、iOS 和 Windows 在內的多個平台。

這個框架的核心理念是讓研究人員能夠專注於實驗設計和數據分析，而不需要擔心底層的技術實作細節。透過統一的介面和自動化的流程，研究團隊可以快速建立可靠的實驗環境，確保資料品質和實驗的可重現性。

## 核心功能特色

### AIOT 整合系統
框架包含完整的 AIOT（人工智慧物聯網）模組，讓 Unity 應用程式能夠與外部 AIOT 平台進行通訊。這個整合系統支援參數傳遞、資料同步，以及自動化的應用程式生命週期管理。

AIOT 整合的主要能力包括在應用程式啟動時自動接收來自 AIOT 平台的參數。這個功能特別重要，因為它允許研究人員在不修改應用程式程式碼的情況下，動態調整實驗參數。系統會根據不同平台採用適當的參數接收方式，在 Windows 上使用命令列參數，在 Android 上使用 Intent 額外資料，在 iOS 上使用 URL schemes。

當實驗結束時，系統提供無縫的返回平台功能，自動將控制權交還給啟動應用程式的 AIOT 平台。這個設計讓整個實驗流程更加流暢，減少人為操作的需求。

框架還包含可設定的伺服器 URL 和平台套件管理功能，讓研究團隊可以根據不同的實驗環境進行靈活配置。

### 資料管理系統
LabFrame2023 實作了一套複雜精密的資料管理系統，建構在 `LabDataManager` 單例模式之上。這個系統提供執行緒安全的資料收集、本地儲存，以及資料傳輸的準備工作。

資料管理系統的設計哲學是確保資料完整性，同時維持系統效能。系統採用執行緒安全的資料寫入機制，使用並行佇列實作，確保在多執行緒環境下不會發生資料競爭或損毀的情況。

系統提供彈性的檔案命名功能，支援根據資料類型分類和自訂附加標籤。這個功能讓研究人員可以輕鬆組織不同類型的實驗資料，例如將生理訊號資料和行為資料分開儲存，或者根據實驗階段添加標籤。

自動資料驗證和完整性檢查功能確保儲存的資料品質。系統會在寫入資料時進行格式驗證，並在應用程式關閉時確保所有佇列中的資料都已正確儲存。

可設定的儲存路徑功能讓系統能夠適應不同平台的檔案系統結構，同時提供優雅的關閉處理機制，保證資料保存的完整性。

### 跨平台架構
框架透過模組化設計和平台特定的輔助類別，展現出色的跨平台相容性。這個設計讓研究人員可以在不同平台上部署相同的實驗應用程式，而不需要為每個平台重新開發。

在 Android 平台上，系統提供儲存權限管理、APK 啟動功能，以及原生外掛整合。儲存權限管理確保應用程式能夠存取必要的檔案系統區域，而 APK 啟動功能讓應用程式能夠與其他 Android 應用程式進行互動。

在 iOS 平台上，系統支援相片庫存取、URL scheme 處理，以及 App Groups 支援的資料分享功能。這些功能讓 iOS 應用程式能夠與系統進行深度整合，提供更豐富的使用體驗。

在 Windows 平台上，系統處理命令列參數處理和文件資料夾整合，確保應用程式能夠正確接收啟動參數並將資料儲存在適當的位置。

## 系統架構深度分析

### 管理器模式實作
框架採用管理器模式，所有核心系統都實作 `IManager` 介面。這個設計確保所有元件都有一致的初始化和清理程序，提高了系統的可維護性和可擴展性。

`LabApplication` 類別作為中央協調器，自動從 Resources 資料夾發現並初始化管理器實例。這個方法提供了幾個重要優勢，包括自動相依性解析而不需要手動連接、所有子系統間一致的生命週期管理、透過新增管理器預製件實現簡易擴展性，以及不同功能區域間清楚的關注點分離。

這種架構設計的美妙之處在於它的自動化程度。當研究人員需要添加新的功能模組時，只需要建立一個實作 `IManager` 介面的新類別，並將其作為預製件放在指定的資料夾中，框架就會自動識別並整合這個新模組。

### 資料流架構
資料在系統中透過精心設計的管線流動，確保資料完整性同時維持效能。這個流程從資料生成開始，應用程式元件生成繼承自 `LabDataBase` 的資料物件。

接下來是佇列管理階段，資料進入執行緒安全的並行佇列等待處理。這個設計讓主執行緒不會因為檔案 I/O 操作而被阻塞，確保使用者介面的流暢性。

檔案組織階段由獨立的執行緒處理佇列中的資料，根據類型和自訂附加標籤進行組織。這個階段的設計考慮了研究資料的特殊需求，讓不同類型的資料能夠有序地分類儲存。

儲存管理階段將資料寫入適合平台的儲存位置，而傳輸準備階段則將處理過的資料準備好，以便可能的上傳到外部系統。

### 執行緒策略
框架實作了精密的執行緒策略，將使用者介面互動與資料處理操作分離。`LabDataManager` 使用專用的背景執行緒進行檔案 I/O 操作，防止 UI 阻塞同時確保資料一致性。

這個執行緒方法包括主執行緒保護以進行 UI 操作、所有檔案 I/O 操作的背景處理、透過並行集合實現執行緒安全通訊，以及執行緒間優雅的關閉協調。

這種設計的重要性在於它確保了使用者體驗的流暢性。在進行大量資料收集的實驗中，如果資料寫入操作阻塞了主執行緒，會導致介面卡頓，影響實驗的進行。透過背景執行緒處理，系統能夠在維持高效能的同時確保資料安全。

## 入門指南

### 安裝步驟
首先透過 Unity 的 Package Manager 匯入 LabFrame2023 套件。框架會自動在您的專案中建立必要的資料夾結構，這個過程是全自動的，不需要手動干預。

接下來透過 LabFrame2023 選單配置平台特定設定。這個步驟很重要，因為不同平台有不同的要求和限制。

### 基本實作範例
要將 LabFrame2023 整合到您的專案中，請遵循這個基本模式。首先定義您的自訂資料結構：

```csharp
// 定義您的自訂資料結構
[System.Serializable]
public class ExperimentData : LabDataBase
{
    public float reactionTime;      // 反應時間
    public Vector3 playerPosition;  // 玩家位置
    public string experimentPhase;  // 實驗階段
    
    public ExperimentData(float time, Vector3 pos, string phase)
    {
        reactionTime = time;
        playerPosition = pos;
        experimentPhase = phase;
    }
}

// 在您的遊戲邏輯中，收集並儲存資料
public class ExperimentController : MonoBehaviour
{
    private void RecordExperimentEvent()
    {
        // 建立包含實驗資料的物件
        var data = new ExperimentData(
            Time.time,                          // 當前時間
            player.transform.position,          // 玩家當前位置
            "training_phase"                    // 目前實驗階段
        );
        
        // 儲存資料，可選擇性加入附加標籤用於分類
        LabDataManager.Instance.WriteData(data, "session_01");
    }
}
```

這個範例展示了框架使用的簡潔性。研究人員只需要定義資料結構，然後在適當的時機呼叫 `WriteData` 方法即可。框架會自動處理所有複雜的背景作業。

### AIOT 整合設定
對於需要與 AIOT 平台整合的應用程式，您需要配置 AIOT 設定。首先定義您的課程參數結構：

```csharp
// 定義您的課程參數結構
[System.Serializable]
public class CourseParameters
{
    public int difficultyLevel;    // 難度等級
    public string courseType;      // 課程類型
    public float timeLimit;        // 時間限制
}

// 在應用程式啟動時取得 AIOT 參數
public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // 嘗試從 AIOT 平台取得課程參數
        var courseParams = AIOT_Manager.Instance.GetCourseParams<CourseParameters>();
        if (courseParams != null)
        {
            // 根據 AIOT 參數配置您的應用程式
            ConfigureGameDifficulty(courseParams.difficultyLevel);
            SetTimeLimit(courseParams.timeLimit);
        }
    }
}
```

這個設計讓研究人員可以從外部平台動態控制實驗參數，而不需要重新編譯應用程式。這對於需要進行多種變數測試的研究特別有用。

## 配置設定

### AIOT 配置
在您專案的 Config 資料夾中建立並配置 `AIOT_Config.json`：

```json
{
    "Enabled": true,
    "ServerUrl": "https://your-aiot-server.com/",
    "AIOTPlatformPackageName": "com.yourcompany.aiotplatform"
}
```

這個配置檔讓您可以控制 AIOT 功能是否啟用，設定伺服器位址，以及指定 AIOT 平台的套件名稱。`Enabled` 欄位特別有用，因為它讓您可以在開發過程中暫時關閉 AIOT 功能。

### 資料管理配置
配置 `LabDataConfig.json` 用於資料管理設定：

```json
{
    "IsTest": false,
    "GameID": "YourExperimentName",
    "BucketID": "research_data_2024",
    "LocalSavePath": "",
    "LocalSaveDataTimeLayout": "yyyyMMddHHmmss"
}
```

這個配置檔控制資料管理系統的行為。`IsTest` 欄位讓您可以區分測試資料和正式實驗資料。`GameID` 用於識別不同的實驗項目，而 `BucketID` 可以用於進一步的資料分類。

## 平台特定考量

### Android 開發
在為 Android 平台建置時，請確保您配置以下項目。儲存權限管理是自動處理的，框架會自動請求必要的儲存權限，但您需要在 Android manifest 中宣告這些權限。

套件命名配置對於無縫整合很重要，請確保 AIOT 平台套件名稱正確設定。建置設定方面，請使用提供的 Android 輔助類別來存取原生功能。

Android 平台的特殊考量包括檔案系統權限的處理。現代 Android 版本對檔案存取有嚴格的限制，框架會自動處理這些複雜性，但開發者需要理解這些限制對資料儲存位置的影響。

### iOS 開發
iOS 開發需要額外的設定以獲得最佳功能。App Groups 配置需要在您的 Apple Developer 帳戶中設定，用於資料分享。這個功能讓不同的應用程式可以安全地分享資料。

URL Schemes 設定是為了應用程式間通訊而建立自訂 URL schemes。框架會自動處理相片庫權限，但您需要在 Info.plist 中提供適當的使用說明。

iOS 平台的沙盒環境對檔案存取有特殊要求，框架透過 App Groups 功能來克服這些限制，讓資料可以在不同應用程式間安全地傳遞。

### Windows 開發
Windows 部署專注於命令列整合和文件資料夾存取。框架會自動處理啟動參數，預設將資料儲存在使用者的文件資料夾中，並透過本地 JSON 檔案管理設定。

Windows 平台的優勢在於相對寬鬆的檔案系統存取權限，讓研究人員可以更靈活地選擇資料儲存位置。

## 進階使用方法

### 自訂資料寫入器
對於特殊的資料儲存需求，您可以擴展資料寫入系統：

```csharp
// 具有複雜需求的自訂資料結構
[System.Serializable]
public class BiometricData : LabDataBase
{
    public float heartRate;           // 心率
    public float skinConductance;     // 皮膚電導
    public Vector3 eyeGazeDirection;  // 眼球注視方向
    
    // 如果需要，可以覆寫序列化方法
    public string GetCustomFormat()
    {
        return $"{Timestamp},{heartRate},{skinConductance},{eyeGazeDirection}";
    }
}
```

這個範例展示了如何為特殊的生理訊號資料建立自訂格式。在生理心理學研究中，不同的資料類型可能需要特殊的格式化方式。

### 事件驅動資料收集
透過框架的事件系統實作響應式資料收集：

```csharp
public class DataCollectionManager : MonoBehaviour
{
    private void Start()
    {
        // 訂閱資料寫入事件
        LabDataManager.Instance.WriteDataAction += OnDataWritten;
    }
    
    private void OnDataWritten(LabDataBase data)
    {
        Debug.Log($"資料已寫入：{data.GetType().Name} 於 {data.Timestamp}");
        
        // 實作自訂的資料處理邏輯
        UpdateDataVisualization();
    }
}
```

這個事件驅動的方法讓您可以在資料寫入時執行額外的邏輯，例如即時更新資料視覺化或觸發其他系統反應。

## API 參考

### 核心類別

**LabDataManager**：中央資料管理單例
這個類別是整個資料管理系統的核心。`WriteData(LabDataBase data, string appendix = "")` 方法用於儲存資料，可選擇性加入分類標籤。`LabDataInit(string userID, string motionIdOverride = "")` 方法初始化資料收集系統，必須在開始收集資料前呼叫。`IsInited` 屬性指示初始化狀態，讓您可以檢查系統是否準備好接收資料。

**AIOT_Manager**：AIOT 平台整合管理器
這個類別處理與 AIOT 平台的所有互動。`GetCourseParams<T>()` 方法從 AIOT 平台取得型別化的課程參數，這是一個泛型方法，可以自動將 JSON 參數轉換為您定義的型別。`ManagerInit()` 方法初始化 AIOT 連線。

**LabTools**：配置和記錄的工具函數
這個工具類別提供多種實用功能。`GetConfig<T>(bool onlyUseTemplate = false)` 載入配置物件，`WriteConfig<T>(T config, bool updateInEditor = false)` 儲存配置變更，`Log(string message)` 提供具有切換功能的框架記錄。

## 疑難排解

### 常見問題和解決方案

**資料未儲存**：這通常是因為沒有在嘗試寫入資料前呼叫 `LabDataManager.LabDataInit()`。請確認您已經用有效的使用者 ID 呼叫了初始化方法。初始化是資料收集的先決條件，如果跳過這個步驟，系統會拒絕接受資料。

**AIOT 參數未接收**：請檢查 AIOT_Config 是否正確配置，以及啟動應用程式是否以預期格式傳遞參數。常見的問題包括 JSON 格式錯誤或參數名稱不匹配。

**平台特定當機**：確保平台特定權限正確配置，並且建置中包含了必要的原生外掛。在 Android 上特別要注意儲存權限，在 iOS 上要確保 App Groups 配置正確。

**效能問題**：透過 `LabDataManager.Instance.DataCount` 監控資料佇列大小，如果必要請調整資料收集頻率。如果佇列大小持續增長，可能表示資料生成速度超過了寫入速度。

記住，大多數問題都可以透過檢查初始化狀態和配置設定來解決。框架設計時考慮了除錯的便利性，提供了豐富的記錄訊息來幫助識別問題。

## 貢獻

這個框架由中央大學無線多媒體實驗室（WMLAB）與延展實境實驗室（XRLAB）開發和維護。如需研究合作或技術支援，請透過官方管道聯絡開發團隊。

我們歡迎研究社群的回饋和建議，特別是來自實際使用經驗的改進提議。這個框架的發展方向很大程度上受到使用者需求的影響。

## 版本歷史

**0.0.19**（當前版本）
這個版本增強了資料管理器，加入附加標籤支援以提供更好的檔案組織功能。改善了管理器處置系統並修復了相關錯誤。將 JSON 序列化更新為 Newtonsoft.Json 以提供更好的相容性。

**0.0.18**
完全重寫了 LabDataManager，加入附加標籤系統用於更好的檔案分類。修復了影響應用程式生命週期的重要管理器處置錯誤。

**0.0.16**
遷移到 Newtonsoft.Json 以改善序列化效能，增強了資料完整性和型別安全性。

**0.0.15**
為 LabDataManager 新增 `IsInited` 欄位以提供更好的狀態追蹤，移除了已棄用的 `GameMode` 欄位以獲得更清潔的配置。

每個版本的更新都反映了我們對使用者回饋的重視，以及對程式碼品質和穩定性的持續改進。

## 授權

這個框架是為研究目的而開發的。請參考官方文件以了解授權條款和使用限制。框架的設計考慮了學術研究的特殊需求，包括資料透明度和可重現性的要求。

---

如需詳細的技術文件和進階實作範例，請參考此套件隨附的範例，或聯絡開發團隊。我們鼓勵研究人員分享他們的使用經驗，以幫助改進這個框架的功能和易用性。
