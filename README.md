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

## Unity MCP

專案已安裝 Unity MCP 套件（`com.gamebooom.unity.mcp`），連線設定寫在 `.mcp.json`（`http://127.0.0.1:20060/`）。開啟 Unity 編輯器後，支援 MCP 的 AI 工具就能連上並操作編輯器。

## 目錄結構

| 路徑 | 說明 |
| --- | --- |
| `Assets/` | 專案資源（場景、設定、腳本等） |
| `Packages/` | 套件清單與鎖定檔 |
| `ProjectSettings/` | Unity 專案設定 |
| `AGENTS.md` | 專案規則（唯一規則來源） |
| `CLAUDE.md` | 只引用 `AGENTS.md`，供 Claude Code 自動載入 |

## 協作規則

所有規則都寫在 [AGENTS.md](AGENTS.md)。
