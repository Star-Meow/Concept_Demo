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
- 狀態變成「確認」才開始實作。
- 驗收流程：
  1. 實作完成後，先執行單元測試（`Tests/EditMode/`，必要時加上 Play Mode 測試），全部通過才能進入下一步。測試沒通過時回報失敗內容，不得交付驗收。
  2. 提供一份文字總結給使用者確認，內容包含：
     - 這次實作了什麼、改了哪些檔案
     - 單元測試結果（執行了哪些測試、通過與失敗數量）
     - 每一項驗收條件的驗證方式與結果，無法用測試驗證的項目要說明怎麼確認
     - 尚未完成、未驗證或已知的問題
  3. 使用者確認總結後才算通過驗收。
- 通過驗收後，狀態才能改為「完成」，並同步更新 `Docs/Requirements/README.md` 的需求清單。
- 實作時發現需求不清楚或互相矛盾，先向使用者提出，不要自行假設。

## GitHub 協作

- 在 issue、PR 的描述或 comment 裡提到檔案時，直接附上可點擊的連結，例如 `[AGENTS.md](https://github.com/Star-Meow/Concept_Demo/blob/main/AGENTS.md)`，不要只寫路徑。
- 引用程式碼時，使用固定在 commit 的 permalink 加上行號，例如 `https://github.com/Star-Meow/Concept_Demo/blob/<sha>/Assets/_Project/Scripts/Runtime/Foo.cs#L60-L68`。
  - `<sha>` 用完整的 commit SHA，不要用 `main` 等分支名稱，避免之後檔案變動導致行號對不上。
  - permalink 單獨放一行，GitHub 才會展開成程式碼片段，審閱者不用自己翻 repo。這只對同一個 repo 的程式碼有效。
- 被指出格式問題時，直接原地編輯那則 comment，不另外發新的 comment 修正。
- issue 裡的驗收 checkbox 不要自己打勾，留給使用者（審閱者）勾選。
- `README.md` 只寫專案本身的內容（環境、取得與設定、結構）；協作與格式規範一律寫在本文件。

## 版本控制範圍

應納入 git：

- `Assets/`（含所有 `.meta` 檔，`.meta` 必須和對應的資源一起提交）
- `Packages/manifest.json`、`Packages/packages-lock.json`
- `ProjectSettings/`
- 根目錄設定檔：`.gitignore`、`.gitattributes`、`.editorconfig`、`.vsconfig`、`.mcp.json`
- 原始美術素材 `ArtSource/` 與素材處理腳本 `Tools/`（見「美術素材」）
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

## 美術素材

原始素材與匯入 Unity 的成品分開放：

| 路徑 | 內容 |
| --- | --- |
| `ArtSource/<分類>/<名稱>/` | 原始素材（GIF、PSD、Aseprite 等），不放進 `Assets/`，避免 Unity 匯入。例：`ArtSource/Characters/Player/player_idle.gif` |
| `Tools/` | 處理素材的腳本（例如把 GIF 切成 PNG），不放進 `Assets/` |
| `Assets/_Project/Art/Sprites/<分類>/<名稱>/<動作>/` | 切好、要匯入 Unity 的 PNG 影格。例：`Art/Sprites/Characters/Player/Idle/` |
| `Assets/_Project/Art/Animations/<名稱>/` | 動畫 clip（`<動作>.anim`）與 Animator Controller。例：`Art/Animations/Player/Idle.anim` |

- 資料夾名稱用 PascalCase；影格檔名為 `<名稱>_<動作>_<兩位數影格編號>.png`，編號從 `00` 開始，例如 `Player_Idle_00.png`。動作名稱對應 Animator 狀態（Idle、Walk、Sprint、Jump、Fall、Roll）。
- 每個動作的影格放在各自的資料夾，不要混放；同一角色所有影格的畫布大小一致，角色在畫布中的位置固定，播放時才不會跳動。
- Sprite 匯入設定：Texture Type 為 Sprite（Single）、Pivot 為 Bottom Center（對齊角色腳底的原點）。同一角色所有影格的 Pixels Per Unit 一致，換算後角色身高約 1 單位；像素風素材用 Point filter、不壓縮。
- 切割時記下原始影格時間（GIF 每格的延遲），換算成動畫 clip 的影格率，寫在該需求文件裡。
- `.gif`、`.png`、`.psd` 由 `.gitattributes` 交給 Git LFS 管理，提交前確認 `git lfs` 可用。
- 原始素材的來源與授權寫在需求文件；不確定能否使用的素材先向使用者確認。

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
