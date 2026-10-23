# REQ-002：玩家影格動畫（待機、揮砍）

| 項目 | 內容 |
| --- | --- |
| 狀態 | 確認 |
| 建立日期 | 2026-10-23 |
| 相關需求 | [REQ-001](REQ-001-player-movement.md) |

## 目標

把佔位方塊換成角色像素圖：平常播放待機動畫，按滑鼠左鍵在地面上揮砍。本次只做動畫表現，不做攻擊判定。

## 素材

| 項目 | 內容 |
| --- | --- |
| 來源 | 使用者提供的免費素材（兩個 GIF） |
| 授權 | 免費使用，不需標示出處 |
| 原始檔 | `ArtSource/Characters/Player/player_attack.gif`、`ArtSource/Characters/Player/player_idle.gif` |
| 切割腳本 | `Tools/split_gif.ps1` |

| 動作 | 原始 GIF | 影格數 | 每格延遲 | Clip 影格率 | 長度 | 循環 |
| --- | --- | --- | --- | --- | --- | --- |
| Idle | 168×168，3 倍放大 | 4 | 100 ms | 10 fps | 0.4 s | 是 |
| Attack | 201×168，3 倍放大 | 5 | 100 ms | 10 fps | 0.5 s | 否 |

### 切割方式

1. 每格依 3×3 像素區塊縮回 1 倍（原圖每個區塊顏色一致，無失真）。
2. 找出兩個動作所有影格的腳底中心（最下排非透明像素的左右邊界中點：Attack 在 1 倍畫布 x = 33、Idle 在 x = 30），以腳底中心對齊。
3. 取兩個動作所有影格非透明像素的聯集範圍，裁成同一個畫布 **68×52**，腳底中心在畫布底邊正中央。
4. 輸出到 `Assets/_Project/Art/Sprites/Characters/Player/<動作>/<名稱>_<動作>_<兩位數編號>.png`：
   - `Idle/Player_Idle_00.png` ～ `Player_Idle_03.png`
   - `Attack/Player_Attack_00.png` ～ `Player_Attack_04.png`

### 匯入設定（全部 9 張相同）

Texture Type Sprite（Single）、Pivot Bottom Center、Pixels Per Unit **41**（待機角色身高 41 px → 約 1 單位，與碰撞框同高）、Filter Point、Compression None、Mip Maps 關閉。

## 功能說明

### Player 結構

```
Player                  根物件（Rigidbody2D、BoxCollider2D、PlayerController），原點在腳底
├─ Visual               位置 (0, 0.5)、scale (1, 1)，Roll 旋轉中心（身體中心）
│   └─ Sprite           位置 (0, -0.5)，SpriteRenderer + Animator（從 Visual 移過來）
└─ GroundCheck
```

- Sprite 的 Pivot 在腳底，`Sprite` 子物件放在 Visual 下 -0.5，畫面上腳底與 Player 原點對齊；Roll 仍繞身體中心旋轉。
- 面向：`SpriteRenderer.flipX` 跟隨 `Facing`（右 = 不翻轉，左 = 翻轉）。
- 移除依狀態變色（`Placeholder Colors` 欄位），SpriteRenderer 顏色固定為白色。

### 動畫

- 兩個 clip 放 `Assets/_Project/Art/Animations/Player/`：`Idle.anim`（循環）、`Attack.anim`（不循環），影格率 10 fps。
- 沿用 `PlayerAnimator.controller`：
  - Idle、Walk、Sprint、Jump、Fall、Roll 都播 `Idle.anim`（其他動作的影格之後另開需求）。
  - 新增狀態 Attack 播 `Attack.anim`，新增參數 `IsAttacking`（bool）。
  - 轉換優先序：Roll > Attack > Jump/Fall > Sprint > Walk > Idle（Roll 與 Attack 實際上不會同時成立）。
- Roll 維持 Visual 繞 Z 軸 360° 的佔位表現。

### 揮砍（Attack）

| 動作 | 按鍵 |
| --- | --- |
| Attack | 滑鼠左鍵 |

- 輸入：`IPlayerInputSource` 的 `PlayerInputFrame` 新增 `AttackPressed`；`InputSystemPlayerInput` 綁定 `<Mouse>/leftButton`；`ScriptedPlayerInput` 可指定 Attack 按下（只在下一次讀取回報一次，與 Jump、Roll 相同）。
- 觸發條件：在地面、不在翻滾中、不在揮砍中，狀態為 Idle / Walk / Sprint。空中、翻滾中、揮砍中按下無效，也不保留到之後。
- 持續時間：`attackDuration`（預設 0.5 s = Attack clip 長度），加在 `PlayerMovementSettings`。
- 揮砍中：
  - 忽略 Move 輸入，水平速度以 `groundDeceleration` 減速到 0（帶慣性，不即停）。
  - 面向不變。
  - Jump、Roll 按下直接丟棄，不進 jump buffer，結束後也不補做。
- 開始與結束時重置 Sprint 偵測器（與 Roll 相同）；結束後若方向鍵仍按著，從 Walk 重新累積。
- 結束：時間到後回到一般狀態判斷（Idle / Walk / …）。
- 揮砍中離開地面（例如帶著速度在平台邊緣揮砍滑出去）立即結束，進入 Fall。
- Respawn 時一併取消揮砍。
- 純邏輯放 plain C# 類別 `AttackTimer`（`Begin(float duration)`、`Tick(float deltaTime)`、`Cancel()`、`IsActive`、`Progress`），不依賴 Unity 時間。
- Debug HUD 的狀態欄會顯示 `Attack`（沿用 `PlayerState`，新增 `Attack`）。

### 文件

- `AGENTS.md`「美術素材」的動作名稱清單加入 `Attack`。
- `README.md` 結構說明加入 `ArtSource/`、`Tools/`（若尚未列出）。

## 驗收條件

> 驗收方式：單元測試通過後，提供文字總結給使用者確認。
> 每項標註驗證方式：**[測試]** EditMode 單元測試；**[MCP]** 透過 Unity MCP 在 Play Mode 量測並附數據；**[人工]** 使用者確認。

1. [MCP] 專案無編譯錯誤；進入 Play Mode 後 Console 沒有錯誤，也沒有來自專案腳本的警告。
2. [MCP] 9 張影格都是 68×52，匯入設定與「匯入設定」一致；檔名與資料夾符合 AGENTS.md「美術素材」規則。
3. [MCP] Player 結構與「Player 結構」一致；站在地面時 Sprite 的 bounds 底邊與 Player 原點的 y 差 < 0.03。
4. [MCP] Animator：Idle / Walk / Sprint / Jump / Fall / Roll 的 motion 為 `Idle.anim`，Attack 為 `Attack.anim`，有參數 `IsAttacking`；`Idle.anim` 循環、`Attack.anim` 不循環，皆 10 fps。
5. [測試] `AttackTimer`：持續 `duration` 後結束；結束後 `IsActive` 為 false；`Cancel` 立即結束；`Progress` 從 0 到 1。
6. [測試] `ScriptedPlayerInput`：Attack 按下只在下一次讀取回報一次。
7. [MCP] 站著不動：Sprite 依序循環 `Player_Idle_00`～`03`，每格約 0.1 s。
8. [MCP] 站著不動揮砍：狀態為 Attack 約 0.5 s（±0.02），期間依序顯示 `Player_Attack_00`～`04`，結束後回到 Idle 並播放 Idle 影格。
9. [MCP] 走路中揮砍（持續按住方向）：揮砍中水平速度依 `groundDeceleration` 減到 0、面向不變；結束後恢復 Walk。回報減速時間與滑行距離。
10. [MCP] 揮砍中按 Jump、Roll、再按 Attack 都無效，結束後也不會補做。
11. [MCP] 空中、翻滾中按 Attack 無效。
12. [MCP] 揮砍中離開地面時立即結束並進入 Fall。
13. [MCP] 面向左時 `flipX` 為 true、面向右時為 false；翻滾後 Visual 旋轉回到 0，腳底仍對齊原點。
14. [MCP] 揮砍中掉出畫面重生（或直接呼叫 Respawn）後狀態為 Idle、`IsAttacking` 為 false。
15. [MCP] 附 Game view 截圖：Idle、揮砍中段、面向左、Roll 中，HUD 清楚可見。
16. [人工] 使用者在 Play Mode 試玩，確認滑鼠左鍵可揮砍、角色大小與動畫看起來正確。

## 範圍外

- 攻擊判定（hitbox）、傷害、敵人、連段、空中攻擊
- Walk、Sprint、Jump、Fall、Roll 的專屬影格動畫
- 備用攻擊按鍵（只用滑鼠左鍵）、手把
- 音效、特效

## 備註

**實作約定**

- 分支：`feature/REQ-002-player-animation`，從合併 REQ-001 後的 `main` 建立。
- 不新增 `.asmdef`。
- 腳本放 `Assets/_Project/Scripts/Runtime/Player/`，命名空間 `ConceptDemo.Player`；`MovementProbe` 的時間軸加入 Attack 輸入以便量測。
- 影格 PNG、原始 GIF 由 Git LFS 管理（`.gitattributes` 已設定）。

**確認時補充的決定**

- 共用畫布：AGENTS.md 規定同一角色所有影格畫布一致，所以 Idle 與 Attack 改用同一個 68×52 畫布（取代先前各自裁切的 36×41、56×52）。
- Visual 結構採「旋轉節點 + Sprite 子物件」，讓 Pivot 在腳底、Roll 仍繞身體中心旋轉。
- 揮砍開始時以減速停下（不即停）。
- 只用滑鼠左鍵，不加備用按鍵。
