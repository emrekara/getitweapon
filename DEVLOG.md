# Get It Weapon — Geliştirme Günlüğü

> Her commit'te bu dosya güncellenir. Proje geçmişine dönmek ve AI oturumlarına bağlam vermek için kullanılır.

---

## Mevcut Durum (Özet)

| Alan | Değer |
|------|-------|
| **Aşama** | 17 — Tech araştırma timer ✅ (Aşama 18 öncesi UI + envanter düzeltmeleri) |
| **Son çalışan özellik** | Envanter kategori tekilliği (silah/kıyafet/küpe/kolye); SAT düzeltmesi; araştırma paneli KAPAT |
| **Aktif sahne** | `Assets/Scenes/SampleScene.unity` |
| **Sonraki hedef** | Aşama 18 — Hammers meta para |
| **Henüz yok** | Hammers, minigame, tech dallanma, remote config |

### Sistem Haritası (AI için hızlı referans)

```
EconomyManager (sahne objesi)
  ├── CurrentGold, AddGold(), TrySpendGold()
  └── Inspector: startingGold

GameTexts (LocalizationManager facade)
  ├── LocalizationKey enum + TR/EN tablolar
  ├── FormatDuration, GetEraDisplayName vb.
  └── GoldAmount, AnvilInfo, OfflineWelcome vb.

LocalizationManager (static)
  ├── SetLanguage(tr/en), OnLanguageChanged
  └── TurkishLocalization / EnglishLocalization tablolari

AutoSellFilter (static)
  ├── EraOrder, PassesFilter(tier/era)
  └── ForgeAutomationManager filtre entegrasyonu

TechTreeManager (SaveManager objesi, runtime)
  ├── Tek slot arastirma: gold + timer → Sv+1
  ├── techResearchNodeId, techResearchEndsAt (save)
  ├── Offline/load sonrasi TryCompleteResearchIfReady
  └── GetForgeSpeedMultiplier, GetUpgradeCostMultiplier, GetOfflineGoldMultiplier

TechTreePanelUI (Canvas, runtime)
  ├── ARAŞTIRMA butonu + dugum listesi paneli
  ├── KAPAT butonu + karartma (disari tikla kapat)
  └── GetTechNodeName: nodeId ile lokalizasyon (enum kaymasi korumasi)

SellButtonHandler (SellButton üzerinde)
  └── Runtime referans cozumleme; goldDisplayUI null hatasi giderildi

GoldDisplayUI (GoldText üzerinde)
  ├── EconomyManager + TextMeshProUGUI referansı
  └── RefreshDisplay() → "X Altın"

ItemData (ScriptableObject)
  ├── itemName, icon, baseAttack, sellPrice, tier, era, category (ItemCategory)
  └── Create: Assets → Create → GetItWeapon → Item Data

ItemCategory (enum)
  ├── Weapon, Armor, Earring, Necklace
  └── Kilic/balta/yay hepsi Weapon; envanterde kategori basina tek slot

ForgeButtonHandler (ForgeButton üzerinde)
  ├── itemDatabase.GetRandomItemForEra(CurrentEra), anvilManager, inventoryManager
  ├── ForgeAutomationManager.ProcessForgedItem() coroutine
  ├── Buton uzerinde % ilerleme + sari fill (ForgeTimerText kaldirildi)
  ├── Durum mesajlari (envanter dolu vb.) forge buton etiketinde
  └── Coroutine ile forge + SaveGame

ForgeAutomationManager (SaveManager objesi, runtime)
  ├── autoForgeEnabled, autoSellEnabled (save)
  ├── autoSellTierFilter, autoSellEraFilter (save)
  ├── ProcessForgedItem: otomatik sat / popup / envantere ekle
  ├── IsWaitingForUserDecision → auto-forge duraklat
  └── TryKeepForgedItem, SellStrictlyWorseInventoryItems

ItemComparer (static)
  ├── HasAnyStatHigher/Lower, IsStrictlyWorse, ShouldPromptUser
  └── FormatItemStats → SAL/SAV/Sev./Satış

InventoryManager (SaveManager objesi, runtime)
  ├── 8 slot, itemIndices[] (-1 = bos), selectedSlot
  ├── Kategori basina tek item: ContainsCategory, TryAddItem reddeder
  ├── ImportState: ayni kategorideki zayif duplicate'leri temizler (en gucluyu tutar)
  ├── TryAddItem / TryRemoveSelected / SelectSlot
  ├── Export/Import + legacy lastItemIndex migration
  └── OnInventoryChanged, OnSlotSelected

InventoryPanelUI + InventorySlotUI (Canvas, runtime grid)
  ├── 4x2 slot grid, secili item detay metni
  └── GameUILayout: arka plan, header, buton renkleri

AutoForgePanelUI + ForgeItemPromptUI (Canvas, runtime)
  ├── OTO DÖV / OTO SAT toggle'ları (duplicate satir/label onleme)
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
  anvilLevel, anvilUpgradeEndsAt, lastQuitTimestamp, autoForgeEnabled, autoSellEnabled,
  autoSellTierFilterEnabled, autoSellMaxTier, autoSellEraFilterEnabled, autoSellMaxEraIndex,
  languageCode, techNodeLevels[], techResearchNodeId, techResearchEndsAt
  └── legacy lastItemIndex → envanter slot 0'a migrate

OfflineProgressManager (sahne objesi)
  ├── anvilManager.GetOfflineGoldPerSecond(), maxOfflineSeconds (28800 = 8 saat)
  ├── offlineMessageDurationSeconds (3.5) sonra mesaj kaybolur
  ├── Load sonrası offline gold + OfflineMessageText (Turkce)
  └── SaveGame() ile çift ödeme engeli

Assets/Editor/DebugSaveMenu.cs → GetItWeapon/Debug (gold, kayıt, dil, missing script tarama)
Assets/Editor/SceneBootstrap.cs → GetItWeapon/Setup Main UI
Assets/Editor/TechTreeBootstrap.cs → GetItWeapon/Setup Tech Tree

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
├── Localization/ (LocalizationKey, LocalizationManager, TR/EN tablolar)
├── Economy/EconomyManager.cs
├── Forge/ (ForgeButtonHandler, SellButtonHandler, ForgeAutomationManager, AutoSellFilter,
│          ItemComparer, ItemCategory, ItemData, ItemDatabase, AnvilManager, AnvilUpgradeSettings)
├── Tech/ (TechEffectType, TechNodeData, TechTreeDatabase, TechTreeManager)
├── Idle/OfflineProgressManager.cs
├── Inventory/InventoryManager.cs
└── UI/ (GameTexts, GoldDisplayUI, AnvilUpgradeHandler, InventoryPanelUI, InventorySlotUI,
         AutoForgePanelUI, ForgeItemPromptUI, TechTreePanelUI, GameUILayout, GameUiBootstrap, UITheme)

Assets/Editor/DebugSaveMenu.cs, SceneBootstrap.cs, TechTreeBootstrap.cs
```

### UI Hierarchy (SampleScene)

```
Canvas (+ GameUiBootstrap, GameUILayout, InventoryPanelUI — runtime)
├── Background (koyu tema)
├── HeaderPanel
│   ├── GoldText
│   └── AnvilInfoText
├── ForgeButton (+ ForgeButtonHandler, % ilerleme fill)
├── AutoForgePanel (runtime: OTO DÖV / OTO SAT / tier-çağ filtre ‹ ›)
├── TechTreeToggle + TechTreePanel (runtime)
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
ForgeButton     → top 120, h 80  (% + fill)
[12px gap]
AutoForgePanel  → top 212, h 200
Envanter        → topInset 424
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
| 14 | Auto-sell filtresi (tier/era) | ✅ | Filtre toggle + cycle, save alanları |
| 15 | i18n iskelet (TR/EN) | ✅ | LocalizationManager, tablolar, dil kaydı |
| 16 | Tech tree iskelet | ✅ | 3 düğüm, Anvil/Offline entegrasyonu, panel UI |
| 17 | Tech araştırma timer | ✅ | Gold + süre, tek slot, save/load, offline tamamlama |

---

## Commit Kayıtları

<!-- Yeni kayıtlar EN ÜSTE eklenir (en yeni önce). -->

### [2026-06-21] Kategori tekilliği, SAT/araştırma UI ve OTO panel düzeltmeleri

**Aşama:** 17 sonrası polish — Aşama 18 öncesi

**Ne yapıldı:**
- **Envanter kategori tekilliği:** `ItemCategory` (Silah, Kıyafet, Küpe, Kolye); kılıç/balta/yay aynı kategori → envanterde tek silah.
- **Forge otomasyonu:** Aynı kategoride zayıf item satılır; güçlü item değiştirme popup'ı kategori slotuna göre çalışır.
- **SAT düzeltmesi:** `SellButtonHandler` runtime `GoldDisplayUI` bulur; NullReference giderildi.
- **Araştırma paneli:** Yeşil **KAPAT** butonu + karartma; dışarı tıklayınca kapanır.
- **Tech node isimleri:** `{0} sa {1} dk` hatası — SO enum kayması düzeltildi; `nodeId` ile isim çözümleme.
- **OTO panel:** Üst üste toggle satırı yüzünden silik "KAPALI" metni — duplicate satır/label temizlendi.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Forge/ItemCategory.cs` (yeni)
- `Assets/Scripts/Forge/ItemData.cs`, `ForgeAutomationManager.cs`, `ForgeButtonHandler.cs`, `SellButtonHandler.cs`
- `Assets/Scripts/Inventory/InventoryManager.cs`
- `Assets/Scripts/UI/TechTreePanelUI.cs`, `AutoForgePanelUI.cs`, `GameTexts.cs`
- `Assets/Scripts/Localization/` (kategori anahtarları, PanelClose)
- `Assets/ScriptableObjects/Items/*.asset` (category: Weapon)
- `Assets/ScriptableObjects/TechTree/*.asset` (displayNameKey düzeltmesi)
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- Play'de otomatik; eski duplicate OTO satırları ilk açılışta temizlenir.
- İsteğe bağlı: `GetItWeapon → Setup Tech Tree` (SO anahtarlarını yeniler).

**Test kriteri:**
- Envanterde balta varken yay dövülür → tek silah kalır (OTO SAT veya popup).
- SAT → gold artar, hata yok.
- ARAŞTIRMA paneli → KAPAT veya dışarı tıkla → kapanır; düğüm adları doğru (Hızlı Dövme vb.).
- OTO DÖV AÇIK iken arkada silik KAPALI görünmez.

**AI bağlam notları:**
- Sıradaki: **Aşama 18 — Hammers**.
- Yeni item eklerken Inspector'da `Category` seçilmeli.
- `LocalizationKey` enum'a ekleme yapılınca Tech SO'ları `Setup Tech Tree` ile güncellenmeli.

---

### [2026-06-21] Forge UI sadeleştirme, OTO SAT düzeltmesi, envanter tekilliği

**Aşama:** 17 sonrası — Aşama 18 öncesi polish

**Ne yapıldı:**
- **Forge UI:** `ForgeTimerText` kaldırıldı; ilerleme yalnızca butonda `DÖV %N` + sarı fill.
- **OTO SAT:** Eşit statlı item boş slot varken satılmıyordu — düzeltildi; geri bildirim kuyruğu forge sırasında timer'ı ezmiyor.
- **Filtre UI:** Tier/çağ `‹ ›` okları; sınırda pasif; etiket `Sev.1–4 · ≤Sev.2 (2/4)` formatı.
- **Gold header:** Runtime `GoldText` + sarı outline; `GoldDisplayUI.Configure()`.
- **Envanter tekilliği:** Aynı item tipinden (SO) yalnızca bir adet; duplicate forge OTO SAT ile satılır.
- **i18n:** `ItemAlreadyInInventory`, `ForgingPercent`, `FormatForgeCountdown` anahtarları.
- **Aşama 14–17** (bu commit'e dahil önceki oturumlar): auto-sell filtresi, i18n iskelet, tech tree, araştırma timer.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Inventory/InventoryManager.cs`
- `Assets/Scripts/Forge/ForgeButtonHandler.cs`, `ForgeAutomationManager.cs`
- `Assets/Scripts/Localization/` (TR/EN anahtarlar)
- `Assets/Scripts/UI/GameUILayout.cs`, `UITheme.cs`, `GameTexts.cs`, `GoldDisplayUI.cs`, `AutoForgePanelUI.cs`
- `Assets/Scripts/Tech/`, `Assets/Scripts/Forge/AutoSellFilter.cs`
- `Assets/Scripts/Core/GameSaveData.cs`, `SaveManager.cs`
- `Assets/Editor/TechTreeBootstrap.cs`, `DebugSaveMenu.cs`
- `Assets/Scenes/SampleScene.unity`
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- Play'de UI runtime kurulur; `ForgeTimerText` gizli kalır.
- İsteğe bağlı: `GetItWeapon → Setup Tech Tree`, `Setup Main UI`.

**Test kriteri:**
- OTO DÖV: butonda % artar, üstte "Dövülüyor" yazısı yok.
- Envanterde Taş Balta varken tekrar Taş Balta dövülür → OTO SAT ile satılır, ikinci slot oluşmaz.
- OTO SAT kapalı + duplicate → butonda "Zaten envanterde: …" mesajı.
- Eski kayıtta duplicate slot varsa yüklemede temizlenir.

**AI bağlam notları:**
- Sıradaki: **Aşama 18 — Hammers** meta para.
- Item isimleri hâlâ SO string (i18n değil).
- `ForgeTimerText` sahne objesi duruyor ama gizli; ileride tamamen silinebilir.

---

### [2026-06-21] Tech araştırma timer — gold + süre, tek slot

**Aşama:** 17 — Tech araştırma timer

**Ne yapıldı:**
- Anlık MAKS yükseltme kaldırıldı: her seviye **gold + araştırma süresi** gerektirir.
- **Tek araştırma slotu:** bir düğüm araştırılırken diğerleri kilitli.
- `TechNodeData`: `baseResearchDurationSeconds`, `durationScalePerLevel`, `GetResearchDurationSeconds()`.
- `TechTreeManager`: `TryStartResearch`, `TryCompleteResearchIfReady`, offline/load tamamlama.
- Save: `techResearchNodeId`, `techResearchEndsAt`.
- UI: buton `ARAŞTIR (Xg, Ys)`; devam ederken `ARAŞTIRILIYOR …`; panel üstte (`SetAsLastSibling`).

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Tech/TechNodeData.cs`, `TechTreeManager.cs`, `TechTreeDatabase.cs`
- `Assets/Scripts/UI/TechTreePanelUI.cs`, `GameTexts.cs`
- `Assets/Scripts/Localization/` (yeni anahtarlar)
- `Assets/Scripts/Core/GameSaveData.cs`, `SaveManager.cs`
- `Assets/Editor/TechTreeBootstrap.cs`
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- İsteğe bağlı: `GetItWeapon → Setup Tech Tree` (SO'lara süre alanları yazar).
- Eski kayıt uyumlu; yeni save alanları varsayılan boş.

**Test kriteri:**
- ARAŞTIR → gold düşer, geri sayım başlar, süre bitince Sv+1.
- Araştırma sürerken başka düğüm tıklanamaz.
- Stop → süre geç → Play → araştırma tamamlanmış olur.
- Tek seferde MAKS'a çıkılamaz.

**AI bağlam notları:**
- Varsayılan süreler test için kısa (15–20 sn); production'da remote config.
- Sıradaki kurgu sırası: Hammers → minigame puanı → mobile UI pass.

---

### [2026-06-21] Auto-sell filtresi, i18n iskeleti ve tech tree

**Aşama:** 14–16 — Filtre + i18n + tech tree iskeleti

**Ne yapıldı:**
- **Auto-sell filtresi:** Tier ve çağ bazlı filtre; OTO SAT açıkken yalnızca filtreye uyan zayıf item'lar otomatik satılır.
- `AutoSellFilter` + `ForgeAutomationManager`: filtre geçmeyen item envantere alınır.
- `AutoForgePanelUI`: 4 satır (OTO DÖV, OTO SAT, tier filtresi, çağ filtresi); › ile değer döngüsü.
- **i18n iskelet:** `LocalizationKey`, `LocalizationManager`, TR/EN tablolar; `GameTexts` facade.
- Dil kaydı (`languageCode`); Debug menü: Dil Türkçe / English.
- **Tech tree iskelet:** `TechNodeData`, `TechTreeDatabase`, `TechTreeManager` (3 düğüm: forge hızı, upgrade indirimi, offline gold).
- `TechTreePanelUI`: ARAŞTIRMA butonu + düğüm listesi; `AnvilManager` ve `OfflineProgressManager` tech çarpanları.
- Editor: `GetItWeapon → Setup Tech Tree` (SO asset oluşturma).
- Runtime fallback: database yoksa 3 düğüm otomatik oluşur.

**Değişen / eklenen dosyalar:**
- `Assets/Scripts/Localization/` (4 yeni)
- `Assets/Scripts/Forge/AutoSellFilter.cs` (yeni)
- `Assets/Scripts/Tech/` (4 yeni)
- `Assets/Scripts/UI/TechTreePanelUI.cs` (yeni)
- `Assets/Scripts/Forge/ForgeAutomationManager.cs`
- `Assets/Scripts/UI/AutoForgePanelUI.cs`, `GameTexts.cs`, `UITheme.cs`, `GameUILayout.cs`
- `Assets/Scripts/Core/GameSaveData.cs`, `SaveManager.cs`
- `Assets/Scripts/Forge/AnvilManager.cs`, `ItemComparer.cs`
- `Assets/Scripts/Idle/OfflineProgressManager.cs`
- `Assets/Scripts/UI/AnvilUpgradeHandler.cs`
- `Assets/Editor/TechTreeBootstrap.cs` (yeni)
- `Assets/Editor/DebugSaveMenu.cs`, `SceneBootstrap.cs`
- `DEVLOG.md`

**Unity editöründe yapılanlar:**
- İsteğe bağlı: `GetItWeapon → Setup Tech Tree` (kalıcı SO asset'ler).
- İsteğe bağlı: `GetItWeapon → Setup Main UI` (panel yeniden kurulumu).
- Test: Game view 9:16 veya 1080×1920.

**Test kriteri:**
- OTO SAT + tier filtresi (≤T1): yalnızca tier 1 zayıf item otomatik satılır; T2+ envantere gider.
- Çağ filtresi: Stone dışı item filtre dışı kalır, otomatik satılmaz.
- Debug → Dil: English → butonlar FORGE/SELL/UPGRADE olur; kayıt sonrası korunur.
- ARAŞTIRMA paneli açılır; gold harcayarak düğüm yükseltilebilir; forge süresi kısalır.
- Filtre ve tech seviyeleri save/load korunur.

**AI bağlam notları:**
- Item isimleri henüz lokalize değil (SO string); sadece UI metinleri i18n.
- Tech tree runtime fallback geçici; `Setup Tech Tree` ile SO'ya geçilebilir.
- Sıradaki: dil seçim UI, tech tree görsel ağaç, remote config.

---

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
