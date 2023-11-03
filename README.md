# LabFrame 2023
> com.xrlab.labframe

修改自 LabFrame 2022，功能修正與結合 AIOT

## CHANGELOG
#### 0.0.11
- Fix Editor Play mode unplayable when develop on Android Platform 
- Fix AndroidHelper cannot be initilized & add context field
- Fix LabDataManager not completely cleaned up

#### 0.0.10
- Add a warning when attempt to update config in editor

#### 0.0.9
- Expose `LabConfig`-related functions (In `LabTools`)
    - `GetConfigPath<T>()` 
    - `GetConfig<T>()`
    - `WriteConfig(T)`
    - `ResetConfig<T>()`
- Update demo

#### 0.0.8
- Added support of platform-dependent SDK by Editor UI (LabFrame2023 > Choose Development Platform)

#### 0.0.7
- Fix Android always jump back to AIOT platform regardless of AIOT module is enabled
- Fix ganglion config cannot be inited
- Change behavior of `LabTools.GetConfig` parameter
- Auto update config fields in editor (during play mode)

#### 0.0.6
- Create Sample `AndroidDemo`
- Rewrite Android `OpenApk`

#### 0.0.5
#### 0.0.4
#### 0.0.3
#### 0.0.2
#### 0.0.1