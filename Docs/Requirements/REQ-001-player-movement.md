# REQ-001：第一個可玩場景與玩家移動（移動、奔跑、跳躍、翻滾）

| 項目 | 內容 |
| --- | --- |
| 狀態 | 實作中 |
| 建立日期 | 2026-10-18 |
| 相關需求 | 無 |

## 目標

建立 DEMO 場景 `Prototype01`：底部一塊地面，上方左右各一個簡易平台，以及一個佔位素材角色。玩家可以移動、奔跑、跳躍（可跳上平台）與翻滾。本次先讓功能可操作，數值先用預設值，驗收時統整後再調整。

## 功能說明

### 場景（數值為預設，單位為 Unity world unit）

```
Prototype01
├─ Main Camera     Orthographic，size 6，位置 (0, 0, -10)，純色深色背景，固定不跟隨
├─ Environment
│   ├─ Ground          位置 (0, -5)，scale (30, 2, 1)       → y -6..-4，頂面 y = -4
│   ├─ Platform_Left   位置 (-6.5, -1.55)，scale (5, 0.5, 1) → x -9..-4，頂面 y = -1.3
│   └─ Platform_Right  位置 ( 6.5, -1.55)，scale (5, 0.5, 1) → x  4..9，頂面 y = -1.3
├─ SpawnPoint      位置 (0, -4)
├─ Player          Player prefab 實例，放在 SpawnPoint
└─ DebugHud
```

- 平台頂面離地約 2.7，底部離地約 2.2（略高於角色身高 2，可從平台下方走過）。
- 地面與平台：SpriteRenderer（佔位方塊，不同顏色）、BoxCollider2D、Layer `Ground`。
- 平台為實心：從正下方起跳會撞頭，屬預期，需從側面或遠處起跳才能跳上去。
- 16:9 時，鏡頭要完整看到兩個平台與地面中段。

### 操作（只做鍵盤，Input System，不建立 `.inputactions` 資產；按鍵為預設，驗收時可調整）

| 動作 | 按鍵 |
| --- | --- |
| Move | `A` / `D`，另支援 `←` / `→` |
| Jump | `Space` |
| Roll | `Left Shift` / `Right Shift` |
| Sprint | 沒有專用按鍵，見下方 |

### 行為

- **Move**：水平速度直接設定，反應即時。面向由最後一次非零輸入決定，初始朝右。
- **Sprint（奔跑）**：符合任一條件即進入。
  - 按住：同方向連續按住 ≥ `sprintHoldTime`（預設 0.3 s）
  - 雙擊：放開後 `doubleTapWindow`（預設 0.3 s）內再按同方向
  - 放開或反向即結束。Roll 會中斷 Sprint，翻滾結束後需重新觸發。空中也可開始 Sprint。
- **Jump**：單段跳。放開按鍵時若仍在上升，上升速度乘以 `jumpCutMultiplier`（可變跳高）。具備 coyote time 與 jump buffer，下落時重力加成。完整按住的最高點要比平台頂面高至少 0.3 單位，讓角色能跳上平台。
- **Roll（翻滾）**：往目前面向翻滾 2 個身體寬。
  - 距離 = `rollDistanceInBodyWidths`（預設 2）× 碰撞框寬度，不寫死數字。
  - 時間：0.5 s 內完成（`rollDuration` 預設 0.3 s，上限 0.5 s），以固定速度位移。
  - 觸發條件：在地面、不在翻滾中、冷卻結束、狀態為 Idle / Walk / Sprint（站著不動也可觸發）。方向取面向，不取按住的方向。
  - 翻滾中忽略 Move 與 Jump 輸入。Visual 繞 Z 軸旋轉 360° 作為佔位表現（不做影格動畫），結束時精確還原；碰撞框大小不變。
  - 空中按 Roll 無效。翻滾中離開地面（例如滾出平台邊緣）則立即結束，進入 Fall 並還原 Visual。
- **Respawn**：y 低於 `killY` 時回到 SpawnPoint，並重置速度、狀態、面向（朝右）、Roll 冷卻、Visual 旋轉與 Sprint 偵測器。
- **Debug HUD**：Game view 顯示狀態、速度、是否著地、面向、Sprint 計時、Roll 冷卻，可由欄位開關。
- **Animator 骨架**：只建立狀態與參數，不放 clip。狀態為 Idle / Walk / Sprint / Jump / Fall / Roll，參數為 `Speed`、`VerticalSpeed`、`IsGrounded`、`IsSprinting`、`IsRolling`。以 Any State 轉換，優先序 Roll > Jump/Fall > Sprint > Walk > Idle。佔位方塊依狀態變色。

### 可調數值（初版預設，全部集中在 `PlayerController` 的同一個 `[Serializable]` 設定區塊，Inspector 可一次調整）

| 參數 | 預設 | 參數 | 預設 |
| --- | --- | --- | --- |
| walkSpeed | 6 | jumpBufferTime | 0.1 |
| sprintSpeed | 10 | rollDistanceInBodyWidths | 2 |
| sprintHoldTime | 0.3 | rollDuration | 0.3（上限 0.5） |
| doubleTapWindow | 0.3 | rollCooldown | 0.3 |
| axisDeadzone | 0.5 | fallGravityMultiplier | 1.3 |
| jumpVelocity | 14 | Rigidbody2D Gravity Scale | 3 |
| jumpCutMultiplier | 0.5 | killY | -10 |
| coyoteTime | 0.1 | | |

## 驗收條件

> 驗收方式：單元測試通過後，提供文字總結給使用者確認。
> 每項標註驗證方式：**[測試]** EditMode 單元測試；**[MCP]** 透過 Unity MCP 在 Play Mode 量測並附數據；**[人工]** 使用者確認。

1. [MCP] 專案無編譯錯誤；進入 Play Mode 後 Console 沒有錯誤，也沒有來自專案腳本的警告。
2. [MCP] 場景階層、Layer、Component 與「功能說明」的場景結構一致。
3. [MCP] 角色出生在地面上，站穩不下沉、不抖動，`IsGrounded` 為 true。
4. [測試] `SprintIntentDetector` 六個情境：按住不足 0.3 s 不算 Sprint；按住 ≥ 0.3 s 算；窗口內雙擊算；窗口外雙擊不算；反向結束；放開結束。
5. [MCP] 按住 `D` 1.0 s：0.25 s 時仍為 Walk（速度 = `walkSpeed`），0.3～0.4 s 之間進入 Sprint，之後水平速度 = `sprintSpeed`。
6. [MCP] 雙擊（按住 0.05 s、放開 0.1 s、再按下）：第二次按下立即進入 Sprint；放開 0.5 s 後再按則維持 Walk。
7. [MCP] 完整按住 Jump 的最高點高於 0.1 s 輕點；回報兩者的最高點數值。
8. [MCP] 跳上平台：角色站在離平台邊緣約 2 單位的地面，朝平台按住方向並完整按住 Jump，落在平台頂面站穩（`IsGrounded` 為 true），可在平台上左右移動；左、右平台各驗證一次。另回報可成功跳上的起跳距離範圍（以 0.5 單位為間隔掃描，資訊項，不判定通過或失敗）。
9. [測試] `RollMotion` 單元測試：位移 = `rollDistanceInBodyWidths` × 碰撞框寬度（誤差 ±0.01）；總時間 ≤ 0.5 s；速度方向跟隨面向；結束後不再作用；中途取消會停止作用。
10. [MCP] 面向右、站著不動按 Shift：位移約 2 個身體寬（誤差 ±0.2 單位）、歷時 ≤ 0.5 s，結束後 Visual 旋轉回到 0，狀態回到 Idle。
11. [MCP] 面向左按 Shift：往左位移相同距離。
12. [MCP] 空中按 Shift 無效；翻滾中再按無效；冷卻期間按無效。
13. [MCP] 在平台邊緣翻滾，滾出邊界時立即結束並進入 Fall，Visual 旋轉已還原，之後落回地面。
14. [MCP] Respawn：把角色移到 `killY` 以下（或走出地面端點墜落）後，在 SpawnPoint 重生，狀態已重置。
15. [MCP] 附 Game view 截圖：Idle、Sprint、Roll、跳躍最高點、站在平台上，五種狀態，HUD 清楚可見。
16. [人工] 總結附上「全部可調數值」與對應實測結果的整合表（走路與 Sprint 速度、跳躍最高點、Roll 距離與時間、可跳上平台的距離範圍），由使用者在 Play Mode 親自試玩後決定要調整哪些數值。

## 範圍外

- Dodge（不建立按鍵綁定與程式碼）
- 攻擊與戰鬥、敵人、傷害系統
- 音效、手把支援、選單
- 正式美術、影格動畫、動畫 clip
- Cinemachine 與鏡頭跟隨
- 進階物理調校（Continuous 碰撞偵測、牆面處理、單向平台）、二段跳與牆跳

## 備註

**實作約定**

- 分支：`feature/REQ-001-player-movement`，從乾淨的 `main` 建立。
- 腳本放 `Assets/_Project/Scripts/Runtime/Player/` 與 `Scripts/Runtime/DebugTools/`，命名空間 `ConceptDemo.Player`、`ConceptDemo.DebugTools`。不要使用名為 `Debug` 的命名空間，會遮蔽 `UnityEngine.Debug`。
- 純邏輯寫成不依賴 Unity 輸入與時間的 plain C# 類別以便單元測試：`SprintIntentDetector`（`Tick(float time, int directionSign)`、`IsSprinting`）、`RollMotion`（`Begin`、`Tick(float deltaTime)` 回傳水平速度、`Cancel`、`IsActive`）。測試放 `Assets/_Project/Tests/EditMode/`。
- 不新增 `.asmdef`，使用既有三個。PlayMode 測試需要新的 asmdef，本次不做。
- Player 結構：根物件放 `Rigidbody2D`（Dynamic、Freeze Rotation Z）、`BoxCollider2D`（size (1, 2)、offset (0, 1)）與邏輯腳本；`Visual` 子物件放 SpriteRenderer、Animator；`GroundCheck` 子物件位於腳底。原點在腳底中央。
- 零摩擦物理材質：`Assets/_Project/Data/PlayerNoFriction.physicsMaterial2D`（friction 0、bounciness 0），指定給 Player 的 `BoxCollider2D`，避免貼著平台側面時被卡住。
- 佔位 Sprite：`Art/Sprites/WhiteSquare.png`；Animator Controller：`Art/Animations/Player/PlayerAnimator.controller`；場景 `Scenes/Prototype01.unity`；Prefab `Prefabs/Player.prefab`。
- 使用 Unity 6 API `Rigidbody2D.linearVelocity`。輸入在 `Update` 鎖存、在 `FixedUpdate` 消耗。著地偵測用 OverlapBox（寬度為碰撞框的 90%，Layer `Ground`）。
- Prefab 資產與 Prefab Mode 內不輸出缺少參考的警告（SpawnPoint 是場景物件），場景實例與 Play Mode 才警告。
- 不加入 Build Settings。

**確認時補充的決定**

- Layer `Ground` 新增於 User Layer 6。
- Roll 冷卻從翻滾結束時起算（含提前中斷）。
- 驗收 8 的「離平台邊緣 2 單位」指腳底中心到平台邊緣的水平距離。以預設數值估算，走路速度下身體前緣約 0.25 s 碰到平台側面，此時腳底高約 2.58，低於平台頂面 2.7，會先擦到側面再靠零摩擦滑上去，轉角可能卡住；照做並回報實測結果。
- 按住方向 ≥ `sprintHoldTime` 會進入 Sprint，所以驗收 8 在空中可能是 Sprint 速度，量測時一併記錄。
- 起跳後短暫忽略著地判定（上升中不算著地），避免起跳當下 OverlapBox 仍碰到地面而誤判。
- 同一個物理步同時按 Jump 與 Roll 時，Jump 優先。
- 翻滾中按下的 Jump 直接丟棄，不進入 jump buffer。
- 翻滾開始時重置 Sprint 偵測器；翻滾結束後若方向鍵仍按著，按住計時從翻滾結束重新累積。
- Visual 位於身體中心 (0, 1)，翻滾時繞身體中心旋轉。
- Rigidbody2D 使用 Interpolate，減少畫面抖動。
- 驗收 6「放開 0.5 s 後再按維持 Walk」只判定再次按下後 0.25 s 內的狀態（持續按住 0.3 s 後會因「按住」條件進入 Sprint）。
- 專案範本內既有的 `Assets/Settings/InputSystem_Actions.inputactions` 不使用也不刪除；輸入以程式內建立的 `InputAction` 處理。
- Debug HUD 以 OnGUI 實作。
- Play Mode 量測使用提交到 `Scripts/Editor/` 的量測工具，以 Input System 送入鍵盤事件並逐幀記錄角色狀態。Editor 未聚焦時可能降頻，量測結果會附上取樣間隔。

**已知事項（只回報，不修）**

- 預設數值估算：跳躍最高點約 3.3，平台頂面 2.7；Roll 速度約 6.7 u/s，略快於走路（6），但慢於 Sprint（10），所以 Sprint 中翻滾會減速。
- 若仍出現貼著平台側面卡住的情況，回報即可。
