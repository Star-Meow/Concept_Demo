# AGENTS.md

本專案（Concept_Demo）唯一的規則來源。所有代理與協作者以本文件為準；新增或修改規則時，直接更新本文件。

`CLAUDE.md` 只能有一行 `@AGENTS.md` 引用本文件，不得寫入其他規則；其他工具的設定檔也一樣，只能引用本文件。

## 專案概況

- 引擎：Unity 6000.3.26f1（URP）
- 遠端倉庫：https://github.com/Star-Meow/Concept_Demo（分支 `main`）
- Unity MCP：`funplay-concept-demo`（設定於 `.mcp.json`，套件 `com.gamebooom.unity.mcp`）

## 進度管理

- 專案進度一律以 git 保存：每完成一個可運作的段落就提交一次 commit。
- Commit 訊息使用繁體中文，第一行簡述做了什麼；對應到需求時，在第一行開頭標上編號，例如 `REQ-001 新增玩家移動`。
- 提交前確認 Unity 沒有編譯錯誤。

## 分支策略

- `main` 隨時保持可開啟、可執行、沒有編譯錯誤。
- 每個需求開一個分支 `feature/REQ-<編號>-<英文短名>`，完成並通過驗收條件後合併回 `main`（`git merge --no-ff`），再刪除分支。
- 文件、設定之類的小修改可以直接提交到 `main`。
- 合併前先把 `main` 的最新變更合併進分支，在分支上解決衝突。
- 場景、Prefab、`.asset` 的衝突交給 UnityYAMLMerge 處理（設定方式見 `README.md`）；處理不了的，先向使用者提出。

## 需求文件

- 需求文件放在 `Docs/Requirements/`，檔名為 `REQ-<三位數編號>-<英文短名>.md`，用 `_TEMPLATE.md` 開始寫。
- 狀態變成「確認」才開始實作；實作完成、驗收條件全部打勾後，狀態改為「完成」，並同步更新 `Docs/Requirements/README.md` 的需求清單。
- 實作時發現需求不清楚或互相矛盾，先向使用者提出，不要自行假設。

## 版本控制範圍

應納入 git：

- `Assets/`（含所有 `.meta` 檔，`.meta` 必須和對應的資源一起提交）
- `Packages/manifest.json`、`Packages/packages-lock.json`
- `ProjectSettings/`
- 根目錄設定檔：`.gitignore`、`.gitattributes`、`.editorconfig`、`.vsconfig`、`.mcp.json`
- 文件：`AGENTS.md`、`CLAUDE.md`、`README.md`，以及之後新增的文件（統一放在 `Docs/`）。專案的環境、設定或結構有變動時，同步更新 `README.md`。

不納入 git（由 `.gitignore` 過濾）：

- Unity 產生的目錄：`Library/`、`Temp/`、`Logs/`、`UserSettings/`、`obj/`、`Build/`
- IDE 自動產生的檔案：`*.csproj`、`*.sln`、`*.slnx`、`.vs/`、`.idea/`
- 作業系統雜檔、個人或本機設定、建置輸出

## 一般原則

- 沒有特別規定的事項，比照一般 Unity 專案的慣例處理。
- 發現問題或跟慣例有衝突時，先向使用者提出，不要自行決定。
- 提出建議時只說明內容，等使用者同意後才能動作，不得先行修改檔案或執行。
- 金鑰、密碼、keystore、`.env` 一律不提交。Unity Cloud 專案 ID 和組織 ID 依一般慣例跟著 `ProjectSettings/` 提交，不另外排除。

## Assets 資料夾結構

專案自己的資源一律放在 `Assets/_Project/`，跟第三方套件與 Unity 範本內容分開：

| 路徑 | 內容 |
| --- | --- |
| `Scripts/Runtime/` | 遊戲執行時的程式碼（組件 `ConceptDemo.Runtime`） |
| `Scripts/Editor/` | 只在編輯器使用的工具程式碼（組件 `ConceptDemo.Editor`） |
| `Tests/EditMode/` | Edit Mode 測試（組件 `ConceptDemo.Tests.EditMode`） |
| `Scenes/` | 遊戲場景 |
| `Prefabs/` | Prefab |
| `Art/Sprites/`、`Art/Animations/`、`Art/Materials/` | 圖片、動畫、材質 |
| `Audio/` | 音樂與音效 |
| `UI/` | UI 用的 Prefab 與資源 |
| `Data/` | ScriptableObject 等設定資料 |

- 空資料夾放一個 `.gitkeep`，資料夾才會被 git 保存；資料夾裡有其他檔案後就刪掉 `.gitkeep`。
- 第三方資源放在 `Assets/` 下各自的資料夾，不放進 `_Project/`。
- 新場景要加到 Build Settings 時，先向使用者確認。

## 程式規範

- 命名空間以 `ConceptDemo` 開頭，依資料夾往下分，例如 `ConceptDemo.Player`。
- 命名：類別、方法、屬性、常數用 PascalCase；介面加 `I` 前綴；私有欄位用 `_camelCase`；Inspector 要顯示的欄位用 `[SerializeField] private`，不要開 `public` 欄位。
- 一個檔案一個主要類別，檔名跟類別名稱相同。
- 格式依 `.editorconfig`：4 個空白縮排、大括號換行（Allman）、UTF-8、LF。
- 新的組件定義檔（`.asmdef`）要經過使用者同意再新增；需要引用其他組件時，加在既有的 `.asmdef` 裡。
- 能用測試驗證的邏輯，就在 `Tests/EditMode/` 補上測試。

## Unity 操作

- 操作 Unity 編輯器時優先使用 Unity MCP 工具；只有 MCP 做不到的步驟才改用其他方式，並先說明原因。
- Unity 開著時，不直接以文字方式修改 `.unity`、`.prefab`、`.asset`，改透過 MCP 或 Editor API 修改。
- 修改後用 MCP 讀回結果，並檢查 Console 與編譯錯誤。
