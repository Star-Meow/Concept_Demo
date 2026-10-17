# AGENTS.md

本專案（Concept_Demo）唯一的規則來源。所有代理與協作者以本文件為準；新增或修改規則時，直接更新本文件。

`CLAUDE.md` 只能有一行 `@AGENTS.md` 引用本文件，不得寫入其他規則；其他工具的設定檔也一樣，只能引用本文件。

## 專案概況

- 引擎：Unity 6000.3.26f1（URP）
- 遠端倉庫：https://github.com/Star-Meow/Concept_Demo（分支 `main`）
- Unity MCP：`funplay-concept-demo`（設定於 `.mcp.json`，套件 `com.gamebooom.unity.mcp`）

## 進度管理

- 專案進度一律以 git 保存：每完成一個可運作的段落就提交一次 commit。
- Commit 訊息使用繁體中文，第一行簡述做了什麼。
- 提交前確認 Unity 沒有編譯錯誤。

## 版本控制範圍

應納入 git：

- `Assets/`（含所有 `.meta` 檔，`.meta` 必須和對應的資源一起提交）
- `Packages/manifest.json`、`Packages/packages-lock.json`
- `ProjectSettings/`
- 根目錄設定檔：`.gitignore`、`.gitattributes`、`.vsconfig`、`.mcp.json`
- 文件：`AGENTS.md`、`CLAUDE.md`、`README.md`，以及之後新增的文件（統一放在 `Docs/`）。專案的環境、設定或結構有變動時，同步更新 `README.md`。

不納入 git（由 `.gitignore` 過濾）：

- Unity 產生的目錄：`Library/`、`Temp/`、`Logs/`、`UserSettings/`、`obj/`、`Build/`
- IDE 自動產生的檔案：`*.csproj`、`*.sln`、`*.slnx`、`.vs/`、`.idea/`
- 作業系統雜檔、個人或本機設定、建置輸出

## 一般原則

- 沒有特別規定的事項，比照一般 Unity 專案的慣例處理。
- 發現問題或跟慣例有衝突時，先向使用者提出，不要自行決定。
- 金鑰、密碼、keystore、`.env` 一律不提交。Unity Cloud 專案 ID 和組織 ID 依一般慣例跟著 `ProjectSettings/` 提交，不另外排除。

## Unity 操作

- 操作 Unity 編輯器時優先使用 Unity MCP 工具；只有 MCP 做不到的步驟才改用其他方式，並先說明原因。
- Unity 開著時，不直接以文字方式修改 `.unity`、`.prefab`、`.asset`，改透過 MCP 或 Editor API 修改。
- 修改後用 MCP 讀回結果，並檢查 Console 與編譯錯誤。
