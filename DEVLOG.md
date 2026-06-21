# Get It Weapon — Geliştirme Günlüğü

> Her commit'te bu dosya güncellenir. Proje geçmişine dönmek ve AI oturumlarına bağlam vermek için kullanılır.

---

## Mevcut Durum (Özet)

| Alan | Değer |
|------|-------|
| **Aşama** | 13 — Türkçe UI + forge layout düzeltmesi ✅ |
| **Son çalışan özellik** | Türkçe metinler (`GameTexts`), OTO DÖV/SAT, akıllı auto-sell popup, forge timer/buton çakışması giderildi |
| **Aktif sahne** | `Assets/Scenes/SampleScene.unity` |
| **Sonraki hedef** | i18n altyapısı, tech tree iskelet, auto-sell filtresi (tier/era) |
| **Henüz yok** | Çoklu dil (i18n), tech tree, REST remote config |

### Sistem Haritası (AI için hızlı referans)

```
EconomyManager (sahne objesi)
  ├── CurrentGold, AddGold(), TrySpendGold()
  └── Inspector: startingGold

GameTexts (static — simdilik Turkce, ileride i18n)
  ├── Buton/uyari/popup metinleri
  ├── FormatDuration, FormatOfflineDuration, GetEraDisplayName
  └── GoldAmount, AnvilInfo, OfflineWelcome vb.

GoldDisplayUI (GoldText üzerinde)
  ├── EconomyManager + TextMeshProUGUI referansı
  └── RefreshDisplay() → "X Altın"

ItemData (ScriptableObject)
  ├── itemName, icon, baseAttack, sellPrice, tier, era
  └── Create: Assets → Create → GetItWeapon → Item Data

ForgeButtonHandler (ForgeButton üzerinde)
  ├── itemDatabase.GetRandomItemForEra(CurrentEra), anvilManager, inventoryManager
  ├── ForgeAutomationManager.ProcessForgedItem() coroutine
  ├── Envanter dolu → FORGE kilitli + "Envanter dolu!"
  └── Coroutine ile forge + SaveGame

ForgeAutomationManager (SaveManager objesi, runtime)
  ├── autoForgeEnabled, autoSellEnabled (save)
  ├── ProcessForgedItem: otomatik sat / popup / envantere ekle
  ├── IsWaitingForUserDecision → auto-forge duraklat
  └── TryKeepForgedItem, SellStrictlyWorseInventoryItems

ItemComparer (static)
  ├── HasAnyStatHigher/Lower, IsStrictlyWorse, ShouldPromptUser
  └── FormatItemStats → SAL/SAV/Sev./Satış

InventoryManager (SaveManager objesi, runtime)
  ├── 8 slot, itemIndices[] (-1 = bos), selectedSlot
  ├── TryAddItem / TryRemoveSelected / SelectSlot
  ├── Export/Import + legacy lastItemIndex migration
  └── OnInventoryChanged, OnSlotSelected

InventoryPanelUI + InventorySlotUI (Canvas, runtime grid)
  ├── 4x2 slot grid, secili item detay metni
  └── GameUILayout: arka plan, header, buton renkleri

AutoForgePanelUI + ForgeItemPromptUI (Canvas, runtime)
  ├── OTO DÖV / OTO SAT toggle'ları
  └── Daha iyi item popup: TUT / YENİSİNİ SAT

GameUiBootstrap (runtime — Play'de UI kurulumu tetikler)
GameUILayout + UITheme (layout sabitleri, forge zone konumlari)

ItemDatabase (ScriptableObject)
  ├── MainItemDatabase: 12 item (Stone/Medieval/Modern/Space × sword/axe/bow)
  ├── GetItemsForEra(), GetRandomItemForEra()
  └── Create: Assets → Create → GetItWeapon → Item Database

AnvilManager (sahne objesi)
  ├── upgradeSettings → MainAnvilUpgradeSettings (SO)
  ├── TryStartUpgrade / TryCompleteUpgradeIfReady, LoadState
  ├── GetUpgradeDurationSeconds() → config level aralığı + çarpan
  ├── IsUpgradeInProgress, UpgradeEndsAtUtc (save)
  ├── GetForgeDuration(), GetUpgradeCost(), GetOfflineGoldPerSecond(), GetScaledSellPrice()
  ├── OnAnvilLevelChanged, OnUpgradeTimerChanged
  └── sellPriceMultiplierPerLevel, era gold çarpanları

AnvilUpgradeHandler (UpgradeButton üzerinde)
  ├── Upgrade geri sayım UI, offline/load sonrası tamamlama
  └── OnUpgradeClicked, yetersiz gold → "Gerekli: Xg"

SaveManager → GameSaveData: gold, inventoryItemIndices[], selectedInventorySlot,
  anvilLevel, anvilUpgradeEndsAt, lastQuitTimestamp, autoForgeEnabled, autoSellEnabled
  └── legacy lastItemIndex → envanter slot 0'a migrate

OfflineProgressManager (sahne objesi)
  ├── anvilManager.GetOfflineGoldPerSecond(), maxOfflineSeconds (28800 = 8 saat)
  ├── offlineMessageDurationSeconds (3.5) sonra mesaj kaybolur
  ├── Load sonrası offline gold + OfflineMessageText (Turkce)
  └── SaveGame() ile çift ödeme engeli

Assets/Editor/DebugSaveMenu.cs → GetItWeapon/Debug (gold, kayıt, missing script tarama)
Assets/Editor/SceneBootstrap.cs → GetItWeapon/Setup Main UI

Assets/Art/Icons/
  ├── icon_sword/axe/bow.png → Stone era
  ├── icon_medieval_*.png → Medieval era
  ├── icon_modern_*.png → Modern era
  └── icon_space_*.png → Space era
```

### Klasör Yapısı (Scripts)

```
Assets/Scripts/
├── Core/ (GameSaveData, SaveManager)
├── Economy/EconomyManager.cs
├── Forge/ (ForgeButtonHandler, SellButtonHandler, ForgeAutomationManager, ItemComparer,
│          ItemData, ItemDatabase, AnvilManager, AnvilUpgradeSettings)
├── Idle/OfflineProgressManager.cs
├── Inventory/InventoryManager.cs
└── UI/ (GameTexts, GoldDisplayUI, AnvilUpgradeHandler, InventoryPanelUI, InventorySlotUI,
         AutoForgePanelUI, ForgeItemPromptUI, GameUILayout, GameUiBootstrap, UITheme)

Assets/Editor/DebugSaveMenu.cs, SceneBootstrap.cs
```

### UI Hierarchy (SampleScene)

```
Canvas (+ GameUiBootstrap, GameUILayout, InventoryPanelUI — runtime)
├── Background (koyu tema)
├── HeaderPanel
│   ├── GoldText
│   └── AnvilInfoText
├── ForgeTimerText (raycastTarget kapali, butonun ustunde ayri satir)
├── ForgeButton (+ ForgeButtonHandler)
├── AutoForgePanel (runtime: OTO DÖV / OTO SAT)
├── InventoryPanel (4x8 grid, runtime)
├── SelectedItemText
├── UpgradeButton (+ AnvilUpgradeHandler)
├── SellButton (+ SellButtonHandler)
├── OfflineMessageText
└── ForgeItemPrompt (runtime popup)
(ItemIcon, LastItemText — gizli/legacy)
EconomyManager | SaveManager (+ InventoryManager, ForgeAutomationManager) | AnvilManager | OfflineProgressManager
EventSystem
```

### Forge Zone Layout (UITheme sabitleri)

```
Header (112px)
ForgeTimerText  → top 120, h 32
[12px gap]
ForgeButton     → top 164, h 80
[12px gap]
AutoForgePanel  → top 256, h 96
Envanter        → topInset 364
```

---

## Aşama Haritası

| # | Aşama | Durum | Not |
|---|-------|-------|-----|
| 0 | Proje kurulumu, spec, Cursor kuralları | ✅ | `forge-master-spec.md`, `.cursor/rules/` |
| 1 | Temel forge döngüsü (gold + buton + UI) | ✅ | Forge → item → sell → gold |
| 2 | Item ikonu | ✅ | Placeholder PNG + UI Image |
| 2b | ItemDatabase | ✅ | MainItemDatabase, 3 stone item |
| 2c | StoneBow + icon_bow | ✅ | 3. item forge havuzunda |
| 3 | Forge timer (üretim süresi) | ✅ | AnvilManager'dan süre |
| 4 | Anvil yükseltme / çağ | ✅ | Upgrade UI + save |
| 5 | Save / load (JSON + PlayerPrefs) | ✅ | gold + lastItemIndex + anvilLevel + lastQuitTimestamp |
| 6 | Offline / idle kazanç | ✅ | 8 saat cap, welcome UI |
| 6b | Offline gold/s anvil ölçekleme | ✅ | seviye + çağ çarpanı |
| 7a | Forge UX + offline mesaj kaybolma | ✅ | forge'da item gizle, 3.5s mesaj |
| 7b | Satış fiyatı anvil çarpanı | ✅ | GetScaledSellPrice + UI event |
| 7c | Era item'ları | ✅ | 4 çağ, 12 item + çağ ikonları |
| 8 | Anvil upgrade timer | ✅ | Config SO, level aralığı, save/load, offline |
| 9 | Satılmamış item koruması | ✅ | Tek slot; envantere gecildi |
| 10 | Envanter MVP + UI tema | ✅ | 8 slot, save/load, koyu tema, header |
| 11 | Auto-forge / Auto-sell toggle | ✅ | OTO DÖV / OTO SAT, save alanları |
| 12 | Akıllı auto-sell + popup | ✅ | Stat karsilastirma, TUT/YENİSİNİ SAT |
| 13 | Türkçe UI + forge layout | ✅ | GameTexts, item isimleri, timer/buton çakışması giderildi |

---

## Commit Kayıtları

<!-- Yeni kayıtlar EN ÜSTE eklenir (en yeni önce). -->

### [2026-06-21] Türkçe UI, auto-forge/sell ve forge layout düzeltmesi

**Aşama:** 11–13 — Otomasyon + Türkçe UI + layout

**Ne yapıldı:**
- `GameTexts`: tüm oyuncu metinleri merkezi Türkçe sabitler (ileride i18n için hazır).
- 12 item asset ismi Türkçeleştirildi (Taş Kılıç, Orta Çağ Baltası, Uzay Yayı vb.).
- `ForgeAutomationManager`: OTO DÖV / OTO SAT; akıllı satış (zayıf item otomatik sat).
- `ItemComparer` + `ForgeItemPromptUI`: daha iyi item veya envanter dolu+eşit stat → TUT / YENİSİNİ SAT popup.
- `AutoForgePanelUI`, `GameUiBootstrap`, `GameUILayout`, `UITheme`: runtime UI kurulumu ve tema.
- Envanter MVP (8 slot), save/load migration, koyu tema UI (aşama 10 tamamlama).
- Forge zone layout: timer metni butonla çakışmıyor; `raycastTarget` kapalı → DÖV butonu tam tıklanabilir.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/UI/GameTexts.cs` (yeni)
- `Assets/Scripts/Forge/ForgeAutomationManager.cs`, `ItemComparer.cs` (yeni)
- `Assets/Scripts/UI/AutoForgePanelUI.cs`, `ForgeItemPromptUI.cs`, `GameUILayout.cs`, `GameUiBootstrap.cs`, `UITheme.cs` (yeni)
- `Assets/Scripts/Inventory/InventoryManager.cs` (yeni)
- `Assets/Scripts/UI/InventoryPanelUI.cs`, `InventorySlotUI.cs` (yeni)
- `Assets/Scripts/Core/GameSaveData.cs`, `SaveManager.cs`
- `Assets/Scripts/Forge/ForgeButtonHandler.cs`, `SellButtonHandler.cs`, `ItemData.cs`
- `Assets/Scripts/UI/AnvilUpgradeHandler.cs`, `GoldDisplayUI.cs`
- `Assets/Scripts/Idle/OfflineProgressManager.cs`
- `Assets/Editor/SceneBootstrap.cs` (yeni)
- `Assets/ScriptableObjects/Items/*.asset` (12 item Türkçe isim)
- `Assets/Scenes/SampleScene.unity`
- `.cursor/rules/3_sprint-mode.mdc` (yeni)
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- Ekstra Inspector adımı gerekmez; Play'de UI otomatik kurulur.
- Test: Game view 9:16 veya 1080×1920 aspect önerilir.

**Test kriteri:**
- Tüm UI metinleri Türkçe (DÖV, SAT, YÜKSELT, OTO DÖV/SAT, popup, offline mesaj).
- Dövme sırasında "Dövülüyor..." metni butonu kapatmaz; DÖV tam tıklanır.
- OTO SAT açık: zayıf item otomatik satılır; daha iyi item → popup.
- Envanter 8 slot, save/load korunur.

**AI bağlam notları:**
- `GameTexts` → ileride localization key'lerine dönüştürülecek.
- Auto-sell filtresi (tier/era bazlı) henüz yok (spec §3.1).
- Sıradaki: i18n iskelet veya tech tree (spec §3.4).

---

### [2026-06-21] Envanter MVP ve UI tema güncellemesi

**Aşama:** 10 — Envanter MVP + UI tema

**Ne yapıldı:**
- 8 slot envanter: forge item'ı slota ekler, envanter dolana kadar tekrar forge.
- Slot tıklama → seçim; SELL seçili slotu satar.
- Save/load: `inventoryItemIndices[]`, `selectedInventorySlot`; eski `lastItemIndex` otomatik migrate.
- UI tema: koyu arka plan, header (gold + anvil), renkli butonlar (forge/sell/upgrade).
- Runtime UI kurulumu: `GameUILayout`, `InventoryPanelUI` Play'de otomatik oluşur.
- Editor menü: `GetItWeapon → Setup Main UI`.
- Sprint modu kuralı: `.cursor/rules/3_sprint-mode.mdc`.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Inventory/InventoryManager.cs` (yeni)
- `Assets/Scripts/UI/InventoryPanelUI.cs`, `InventorySlotUI.cs`, `GameUILayout.cs`, `UITheme.cs` (yeni)
- `Assets/Scripts/Forge/ForgeButtonHandler.cs`, `SellButtonHandler.cs`
- `Assets/Scripts/Core/GameSaveData.cs`, `SaveManager.cs`
- `Assets/Scripts/UI/GoldDisplayUI.cs`
- `Assets/Editor/SceneBootstrap.cs` (yeni)
- `.cursor/rules/3_sprint-mode.mdc` (yeni)
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- Ekstra Inspector adımı gerekmez; Play'de otomatik kurulur.
- İsteğe bağlı: `GetItWeapon → Setup Main UI` + Ctrl+S.

**Test kriteri:**
- FORGE × 3 → 3 slot dolu, tek tek seçilebilir.
- 8 forge → "Inventory full!", FORGE kilitli.
- SELL → seçili slot boşalır, gold artar, forge tekrar açılır.
- Stop → Play → envanter korunur.

**AI bağlam notları:**
- ItemIcon/LastItemText legacy; gizlendi, silinebilir.
- Sıradaki: auto-forge + auto-sell filtresi (spec §3.1).

---

### [2026-06-21] Satılmamış item koruması — forge öncesi SELL zorunluluğu

**Aşama:** 9 — Satılmamış item koruması

**Ne yapıldı:**
- Elde satılmamış item varken yeni forge başlatılamaz; item kaybı engellendi.
- `HasUnsoldItem` property; forge butonu otomatik kilitlenir/açılır.
- Engellenmiş forge denemesinde `ForgeTimerText`: "Sell item first!" (2 sn).
- Kayıt yüklemesinde (`RestoreLastItem`) buton durumu senkronize edilir.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Forge/ForgeButtonHandler.cs`
- `DEVLOG.md`

**Unity editöründe yapılanlar:** Yok; mevcut sahne referansları yeterli.

**Test kriteri:**
- Forge → item gelir → FORGE pasif / tıklanınca uyarı.
- SELL → FORGE tekrar aktif.
- Kayıt yükle → item varsa FORGE kilitli.

**AI bağlam notları:**
- Tam envanter henüz yok; tek slot koruma katmanı.
- Sıradaki: çoklu item envanteri veya auto-sell filtresi (spec §3.1).

---

### [2026-06-21] TextMesh Pro font asset Unity 6 uyumu

**Aşama:** 8 — Bakım (asset senkron)

**Ne yapıldı:**
- `LiberationSans SDF.asset` Unity 6 material/font serializedVersion güncellemesi (editör otomatik).

**Değişen / eklenen dosyalar:**
- `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset`
- `DEVLOG.md`

**Unity editöründe yapılanlar:** Yok (otomatik asset refresh).

**Test kriteri:** UI metinleri (Gold, Forge timer vb.) normal render edilir.

**AI bağlam notları:** Oyun mantığı değişmedi; yalnızca TMP import uyumu.

---

### [2026-06-21] Anvil upgrade timer ve level bazlı config

**Aşama:** 8 — Anvil upgrade timer

**Ne yapıldı:**
- Anvil upgrade artık anında değil; config'e göre timer veya anında tamamlanır.
- `AnvilUpgradeSettings` SO: `useUpgradeTimer`, baz süre, level aralığı + `durationMultiplier`.
- Varsayılan: timer kapalı (anında); açıkken Lv.1–4 anında, Lv.5+ çarpanlı süre.
- `GameSaveData.anvilUpgradeEndsAt` — offline upgrade tamamlama.
- `AnvilUpgradeHandler` geri sayım UI + load sonrası otomatik complete.
- `MainAnvilUpgradeSettings` asset + sahne bağlantısı.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Forge/AnvilManager.cs`, `AnvilUpgradeSettings.cs`
- `Assets/Scripts/UI/AnvilUpgradeHandler.cs`
- `Assets/Scripts/Core/GameSaveData.cs`, `SaveManager.cs`
- `Assets/ScriptableObjects/AnvilUpgrade/MainAnvilUpgradeSettings.asset`
- `Assets/Scenes/SampleScene.unity`
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- AnvilManager → Upgrade Settings: MainAnvilUpgradeSettings.
- Timer test: SO'da Use Upgrade Timer işaretle.

**Test kriteri:**
- Timer kapalı → UPGRADE anında Lv+1.
- Timer açık, Lv.1 → anında; Lv.5+ → geri sayım, offline bitince tamamlanır.
- Kayıt JSON'da `anvilUpgradeEndsAt` görünür (timer aktifken).

**AI bağlam notları:**
- Süreler SO'dan; ileride REST/remote config bu SO'yu runtime doldurabilir (spec §8.5).
- Tech tree çarpan override ayrı katman olacak.
- Sıradaki: envanter veya forge öncesi sat zorunluluğu.

---

### [2026-06-21] Modern/Space era item'ları ve çağ bazlı ikonlar

**Aşama:** 7c — Era item'ları tamamlandı

**Ne yapıldı:**
- Modern Sword/Axe/Bow item SO'ları eklendi (tier 3, Lv.10–14 havuzu).
- Space Sword/Axe/Bow item SO'ları eklendi (tier 4, Lv.15+ havuzu).
- `MainItemDatabase` 12 item'a genişletildi.
- Medieval/Modern/Space için 9 era placeholder ikonu (`Assets/Art/Icons/`).
- Medieval item'lar era ikonlarına bağlandı; Stone genel ikonları kullanmaya devam ediyor.
- `MedievalAxe_T1` GUID düzeltmesi (31→32 karakter; broken PPtr hatası giderildi).

**Değişen / eklenen dosyalar:**
- `Assets/ScriptableObjects/Items/Modern*_T1.asset` (3 yeni)
- `Assets/ScriptableObjects/Items/Space*_T1.asset` (3 yeni)
- `Assets/ScriptableObjects/Items/Medieval*_T1.asset` (ikon + MedievalAxe GUID)
- `Assets/ScriptableObjects/ItemDatabase/MainItemDatabase.asset`
- `Assets/Art/Icons/icon_medieval_*.png`, `icon_modern_*.png`, `icon_space_*.png` (9+9 meta)
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- Ekstra Inspector adımı gerekmez; `MainItemDatabase` ve item SO'lar hazır.
- Project → `Assets/Art/Icons` altında 12 PNG görünmeli (Refresh gerekirse).

**Test kriteri:**
- Lv.1–4 → Stone item + taş ikon; Lv.5–9 → Medieval + altın tonlu ikon.
- Lv.10–14 → Modern item; Lv.15+ → Space item + mor/cyan ikon.
- Debug missing script taraması temiz; `MainItemDatabase` 12 slot dolu.

**AI bağlam notları:**
- Kod değişmedi; `AnvilManager.GetEraForLevel()` zaten 4 çağı destekliyordu.
- İkonlar placeholder; gerçek art aynı dosya adlarıyla değiştirilebilir.
- Sıradaki mantıklı adım: anvil upgrade timer (spec §3.2).

---

### [2026-06-20] Anvil satış çarpanı ve era bazlı forge havuzu

**Aşama:** 7b + 7c (Stone/Medieval)

**Ne yapıldı:**
- `AnvilManager.GetScaledSellPrice()`: satış fiyatı anvil seviye + çağ çarpanı.
- `OnAnvilLevelChanged` event: upgrade sonrası item satış fiyatı UI anında güncellenir.
- `ItemData.era` alanı; `ItemDatabase.GetRandomItemForEra()`.
- Forge yalnızca `AnvilManager.CurrentEra` havuzundan item üretir.
- Medieval Sword/Axe/Bow item SO'ları eklendi (tier 2, daha yüksek baz fiyat).
- Medieval asset GUID düzeltmesi (31→32 karakter; missing script uyarısı giderildi).
- `DebugSaveMenu`: missing script tarama (sahne + asset).

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Forge/AnvilManager.cs`, `ForgeButtonHandler.cs`, `SellButtonHandler.cs`
- `Assets/Scripts/Forge/ItemData.cs`, `ItemDatabase.cs`
- `Assets/ScriptableObjects/Items/Medieval*_T1.asset` (3 yeni)
- `Assets/ScriptableObjects/ItemDatabase/MainItemDatabase.asset`
- `Assets/ScriptableObjects/Items/Stone*_T1.asset` (era alanı)
- `Assets/Editor/DebugSaveMenu.cs`
- `Assets/Scenes/SampleScene.unity`
- `DEVLOG.md`

**Test kriteri:**
- Lv.1–4 → Stone item; Lv.5+ → Medieval item.
- Upgrade → Sell: Xg anında artar; SELL doğru gold verir.
- Debug missing script taraması temiz.

**AI bağlam notları:**
- Global anvil çarpanı geçici; asıl fark era item baz fiyatından gelir.
- Sıradaki: Modern (Lv.10) + Space (Lv.15) item SO'ları.
- Anvil timer henüz yok.

---

### [2026-06-20] ItemDatabase, StoneBow, forge UX ve offline mesaj kaybolma

**Aşama:** 7a — ItemDatabase + forge UX

**Ne yapıldı:**
- `ItemDatabase` ScriptableObject + `MainItemDatabase` (kılıç, balta, yay).
- `ForgeButtonHandler` item listesini `ItemDatabase` üzerinden okur.
- `StoneBow_T1` item + `icon_bow.png` placeholder ikon.
- Forge başlayınca eski item/ikon gizlenir; `IsForging` ile satış engellenir.
- Offline welcome mesajı 3.5 sn sonra otomatik kaybolur.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Forge/ItemDatabase.cs` (yeni)
- `Assets/ScriptableObjects/ItemDatabase/MainItemDatabase.asset` (yeni)
- `Assets/ScriptableObjects/Items/StoneBow_T1.asset` (yeni)
- `Assets/Art/Icons/icon_bow.png` (yeni)
- `Assets/Scripts/Forge/ForgeButtonHandler.cs`, `SellButtonHandler.cs`
- `Assets/Scripts/Idle/OfflineProgressManager.cs`
- `Assets/Scenes/SampleScene.unity`
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- ForgeButton → Item Database: MainItemDatabase.
- OfflineProgressManager → Offline Message Duration Seconds: 3.5.

**Test kriteri:**
- Forge 3 item'dan rastgele üretir (kılıç/balta/yay).
- Forge sırasında ikon/isim kaybolur, SELL çalışmaz.
- Offline mesaj birkaç saniye sonra silinir.

**AI bağlam notları:**
- Satış fiyatı hâlâ ItemData sabit değeri; sıradaki: anvil çarpanı (7b), sonra era item'ları (7c).
- `Assets/NewItem.asset` yanlışlıkla oluşturulmuş test dosyası — commit'e dahil edilmedi.

---

### [2026-06-20] Offline gold/s anvil seviye ve çağ ölçeklemesi

**Aşama:** 6b — Offline gold/s anvil bağlantısı

**Ne yapıldı:**
- `AnvilManager.GetOfflineGoldPerSecond()`: seviye bonusu + çağ çarpanı (Stone→Space).
- Inspector alanları: `baseOfflineGoldPerSecond` (1), `goldPerSecondPerLevel` (0.5).
- `OfflineProgressManager` sabit gold/s kaldırıldı; `AnvilManager` referansı ile hesaplar.
- Sahne referansları bağlandı.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Forge/AnvilManager.cs`
- `Assets/Scripts/Idle/OfflineProgressManager.cs`
- `Assets/Scenes/SampleScene.unity`
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- OfflineProgressManager → Anvil Manager referansı.
- AnvilManager → Base Offline Gold Per Second: 1, Gold Per Second Per Level: 0.5.

**Test kriteri:**
- Anvil yükseldikçe offline kazanç artar (ör. 16s offline, yüksek seviyede +1000 gold).
- Seviye 1'de ~1 gold/s.

**AI bağlam notları:**
- Formül: `(base + (level-1)×0.5) × eraMultiplier`.
- Era çarpanları: Stone 1×, Medieval 1.5×, Modern 2×, Space 2.5×.
- Sıradaki: ItemDatabase, forge UX.

---

### [2026-06-20] Offline idle kazanç sistemi

**Aşama:** 6 — Offline kazanç

**Ne yapıldı:**
- `GameSaveData.lastQuitTimestamp`: Unix saniye, quit/pause/forge/sell'de güncellenir.
- `OfflineProgressManager`: açılışta geçen süre × gold/s hesabı, max 8 saat.
- `OfflineMessageText` UI: "Welcome back! +X gold (Ym offline)" mesajı.
- Offline gold uygulanınca `SaveGame()` — çift ödeme engeli.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Idle/OfflineProgressManager.cs` (yeni)
- `Assets/Scripts/Core/GameSaveData.cs`, `SaveManager.cs`
- `Assets/Scenes/SampleScene.unity`
- `DEVLOG.md`

**Test kriteri:**
- Stop → 10+ sn bekle → Play → gold artar, mesaj görünür.
- Hemen tekrar Play → offline mesajı yok, gold artmaz.

**AI bağlam notları:**
- goldPerSecond şimdilik sabit 1; ileride anvil seviyesine bağlanabilir.
- Zaman damgası cihaz saati; spec'e göre ileride sunucu zamanı.
- Sıradaki: ItemDatabase veya gold/s anvil ölçeklemesi.

---

### [2026-06-20] Anvil upgrade sistemi, forge süresi kaynağı ve debug gold menüsü

**Aşama:** 4 — Anvil yükseltme

**Ne yapıldı:**
- `AnvilManager`: seviye, çağ, forge süresi, upgrade maliyeti.
- `AnvilUpgradeHandler`: UPGRADE butonu, yetersiz gold uyarısı, kod ile OnClick.
- Forge süresi artık yalnızca `AnvilManager.baseForgeDuration`'dan okunur.
- `GameSaveData.anvilLevel` kayda eklendi.
- `Assets/Editor/DebugSaveMenu.cs`: gold ekle (+100/+1000/input), kayıt sil.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Forge/AnvilManager.cs` (yeni)
- `Assets/Scripts/UI/AnvilUpgradeHandler.cs` (yeni)
- `Assets/Editor/DebugSaveMenu.cs` (yeni)
- `Assets/Scripts/Forge/ForgeButtonHandler.cs`
- `Assets/Scripts/Core/GameSaveData.cs`, `SaveManager.cs`
- `Assets/Scenes/SampleScene.unity`
- `DEVLOG.md`

**Test kriteri:**
- UPGRADE → gold azalır, Anvil Lv artar, maliyet ekranda doğru.
- Forge süresi AnvilManager Inspector'dan ayarlanır.
- Debug menü ile gold eklenebilir.

**AI bağlam notları:**
- ForgeButton'daki eski `forgeDurationSeconds` kaldırıldı.
- Upgrade buton text raycastTarget kod ile kapatılır.
- Sıradaki: offline kazanç veya ItemDatabase.

---

### [2026-06-20] Forge timer: geri sayım ve buton kilidi

**Aşama:** 3 — Forge timer

**Ne yapıldı:**
- `ForgeButtonHandler`: Coroutine ile `forgeDurationSeconds` (varsayılan 3 sn) bekleme.
- Forge süresince buton `interactable = false`.
- `ForgeTimerText` UI ile `Forging... Xs` geri sayımı.
- Sahneye `ForgeTimerText` eklendi, referanslar bağlandı.

**Test kriteri:**
- FORGE → 3-2-1 sayar → item gelir → buton tekrar aktif.

**AI bağlam notları:**
- Forge sırasında önceki item ekranda kalabilir (bilinen UX durumu).
- Sıradaki: AnvilManager (seviye, çağ, süre/gold maliyeti).

---

### [2026-06-20] Save/load: gold ve son item kalıcılığı (PlayerPrefs + JSON)

**Aşama:** 5 — Save/load

**Ne yapıldı:**
- `GameSaveData`: gold + lastItemIndex.
- `SaveManager`: Load on Start, Save on quit/pause/forge/sell.
- `EconomyManager.SetGold()` kayıt yüklemesi için.
- `ForgeButtonHandler`: GetLastItemIndex, RestoreLastItem.
- Sahneye SaveManager objesi ve referans bağlantıları.

**Test kriteri:**
- Play → forge/sell → stop → play → gold ve item durumu korunur.

**AI bağlam notları:**
- PlayerPrefs cihaz bazlı; ileride sunucu otoriter kayıt gelecek.
- Sıradaki: forge timer (üretim süresi).

---

### [2026-06-20] Forge item ikonları ve UI Image entegrasyonu

**Aşama:** 2 — Item ikonu

**Ne yapıldı:**
- `Assets/Art/Icons/` altına placeholder kılıç ve balta PNG'leri eklendi.
- `ItemData.Icon` alanına sprite'lar bağlandı (StoneSword_T1, StoneAxe_T1).
- Sahneye `ItemIcon` UI Image eklendi; forge'da ikon gösterilir, SELL'de kaybolur.
- `ForgeButtonHandler` güncellendi: `Image itemIcon` referansı, `ClearDisplay()`.
- `StoneAxe_T1` item adı düzeltildi: "Stone Axe".

**Değişen / eklenen dosyalar:**
- `Assets/Art/Icons/icon_sword.png`, `icon_axe.png` (yeni)
- `Assets/Scripts/Forge/ForgeButtonHandler.cs`
- `Assets/Scripts/Forge/SellButtonHandler.cs`
- `Assets/Scenes/SampleScene.unity`
- `Assets/ScriptableObjects/Items/StoneSword_T1.asset`
- `Assets/ScriptableObjects/Items/StoneAxe_T1.asset`
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- ItemIcon Image oluşturuldu (128x128, orta-üst konum).
- ForgeButton → Item Icon referansı bağlandı.

**Test kriteri:**
- FORGE → kılıç/balta ikonu + isim görünür.
- SELL → ikon kaybolur, gold artar.

**AI bağlam notları:**
- İkonlar placeholder; gerçek art `Assets/Art/Icons/` içine PNG koyup ItemData'ya bağla.
- Sıradaki özellik: save/load (gold kalıcılığı).
- SellButtonHandler'da `lastItemText` referansı artık kullanılmıyor (temizlik yapılabilir).

---

### [2026-06-20] Temel forge-sell oyun döngüsü ve ScriptableObject item sistemi

**Aşama:** 1 — Temel Forge Döngüsü (MVP)

**Ne yapıldı:**
- **ItemData** ScriptableObject: silah tanımları Inspector'dan düzenlenebilir (isim, saldırı, satış fiyatı, tier).
- İlk iki item kartı: `StoneSword_T1`, `StoneAxe_T1`.
- **ForgeButtonHandler** güncellendi: rastgele item üretir, `LastItemText`'e yazar.
- **SellButtonHandler** eklendi: son item'ı satar, gold artırır, ekranı sıfırlar.
- UI: Canvas (1080x1920), GoldText, ForgeButton, LastItemText, SellButton.
- TextMesh Pro import edildi.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Economy/EconomyManager.cs`
- `Assets/Scripts/UI/GoldDisplayUI.cs`
- `Assets/Scripts/Forge/ForgeButtonHandler.cs`
- `Assets/Scripts/Forge/SellButtonHandler.cs` (yeni)
- `Assets/Scripts/Forge/ItemData.cs` (yeni)
- `Assets/ScriptableObjects/Items/StoneSword_T1.asset` (yeni)
- `Assets/ScriptableObjects/Items/StoneAxe_T1.asset` (yeni)
- `Assets/Scenes/SampleScene.unity`
- `Assets/TextMesh Pro/`
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- Canvas mobil dikey (1080x1920) ayarlandı.
- EconomyManager, GoldText, ForgeButton, LastItemText, SellButton sahneye eklendi.
- Forgeable Items listesine iki ItemData asset bağlandı.
- Forge OnClick → `OnForgeClicked`, Sell OnClick → `OnSellClicked`.

**Test kriteri:**
- Play → FORGE → silah ismi görünür, gold değişmez.
- SELL → gold artar (sellPrice kadar), yazı "No item yet" olur.

**AI bağlam notları:**
- Kullanıcı Unity'de yeni; adım adım + onay ile ilerleniyor (`1_general.mdc`).
- Yeni silah: Item Data asset oluştur + Forgeable Items Size artır (ItemDatabase henüz yok).
- Item icon alanı boş; sprite ekleme sırada.
- Save/load yok; Play'den çıkınca gold sıfırlanır (startingGold).
- Forge timer yok; anında üretim.
- `EconomyManager` singleton değil, sahne bazlı.

---

### [2026-06-20] — Kayıt başlangıcı (commit öncesi mevcut çalışma)

**Aşama:** 1 — Temel Forge Döngüsü (ilk iterasyon)

**Ne yapıldı:**
- Unity 6.3 LTS projesi, spec, Cursor kuralları.
- EconomyManager, GoldDisplayUI, ForgeButtonHandler (eski: doğrudan gold).
- TextMesh Pro eklendi.

**Not:** Bu kayıt ilk geliştirme oturumunu belgeler; üstteki commit ile birleştirilmiş durum geçerli.

---
