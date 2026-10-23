# Concept_Demo

Unity 概念驗證專案。

## 環境需求

- Unity 6000.3.26f1（URP）
- Git 與 Git LFS（`.gitattributes` 會用 LFS 管理圖片、音訊、模型等二進位資源）

## 取得專案

```bash
git lfs install
git clone https://github.com/Star-Meow/Concept_Demo.git
```

Clone 之後用 Unity Hub 開啟專案資料夾即可。

## 首次設定：場景與 Prefab 合併工具

`.gitattributes` 指定場景、Prefab、`.asset` 等 Unity YAML 檔用 Unity 內建的 UnityYAMLMerge 合併，衝突時可以逐項合併，不用整檔二選一。這個合併工具的設定不會跟著 repo 同步，每份 clone 都要在專案根目錄執行一次（路徑依實際的 Unity 安裝位置調整）：

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge (UnityYAMLMerge)" && git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.3.26f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -h -p --force %O %B %A %A" && git config merge.unityyamlmerge.recursive binary
```

## Unity MCP

專案已安裝 Unity MCP 套件（`com.gamebooom.unity.mcp`），連線設定寫在 `.mcp.json`（`http://127.0.0.1:20060/`）。開啟 Unity 編輯器後，支援 MCP 的 AI 工具就能連上並操作編輯器。

## 目錄結構

| 路徑 | 說明 |
| --- | --- |
| `Assets/_Project/` | 專案自己的資源（腳本、場景、Prefab、美術、音訊等，細節見 `AGENTS.md`） |
| `Assets/Settings/`、`Assets/Scenes/` | Unity 範本產生的 URP 設定與範例場景 |
| `Packages/` | 套件清單與鎖定檔 |
| `ProjectSettings/` | Unity 專案設定 |
| `Docs/Requirements/` | 需求文件與範本 |
| `ArtSource/` | 原始美術素材（GIF、PSD 等，不匯入 Unity） |
| `Tools/` | 素材處理腳本（例如 GIF 切 PNG） |
| `.editorconfig` | 程式碼格式設定（Rider、Visual Studio 會自動讀取） |
| `AGENTS.md` | 專案規則（唯一規則來源） |
| `CLAUDE.md` | 只引用 `AGENTS.md`，供 Claude Code 自動載入 |

## 協作規則

所有規則都寫在 [AGENTS.md](AGENTS.md)。
