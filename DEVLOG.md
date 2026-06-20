# Get It Weapon — Geliştirme Günlüğü

> Her commit'te bu dosya güncellenir. Proje geçmişine dönmek ve AI oturumlarına bağlam vermek için kullanılır.

---

## Mevcut Durum (Özet)

| Alan | Değer |
|------|-------|
| **Aşama** | 7c — Era item'ları (Stone + Medieval) ✅ |
| **Son çalışan özellik** | Çağ bazlı forge havuzu, anvil satış çarpanı, upgrade'de sell UI yenileme |
| **Aktif sahne** | `Assets/Scenes/SampleScene.unity` |
| **Sonraki hedef** | Modern + Space item'ları (Lv.10/15), anvil timer |
| **Henüz yok** | Modern/Space SO'lar, anvil timer, envanter |

### Sistem Haritası (AI için hızlı referans)

```
EconomyManager (sahne objesi)
  ├── CurrentGold, AddGold(), TrySpendGold()
  └── Inspector: startingGold

GoldDisplayUI (GoldText üzerinde)
  ├── EconomyManager + TextMeshProUGUI referansı
  └── RefreshDisplay() → "Gold: X"

ItemData (ScriptableObject)
  ├── itemName, icon, baseAttack, sellPrice, tier, era
  └── Create: Assets → Create → GetItWeapon → Item Data

ForgeButtonHandler (ForgeButton üzerinde)
  ├── itemDatabase.GetRandomItemForEra(CurrentEra), anvilManager
  ├── Forge başlayınca HideItemDisplay(), IsForging
  ├── OnAnvilLevelChanged → RefreshLastItemDisplay (sell fiyat UI)
  └── Coroutine ile forge + SaveGame

ItemDatabase (ScriptableObject)
  ├── MainItemDatabase: 3 Stone + 3 Medieval item
  ├── GetItemsForEra(), GetRandomItemForEra()
  └── Create: Assets → Create → GetItWeapon → Item Database

AnvilManager (sahne objesi)
  ├── GetForgeDuration(), GetUpgradeCost(), GetOfflineGoldPerSecond(), GetScaledSellPrice()
  ├── OnAnvilLevelChanged event
  └── sellPriceMultiplierPerLevel, era gold çarpanları

SellButtonHandler → anvilManager.GetScaledSellPrice()

AnvilUpgradeHandler (UpgradeButton üzerinde)
  └── OnUpgradeClicked, yetersiz gold → "Need XXg"

SaveManager → GameSaveData: gold, lastItemIndex, anvilLevel, lastQuitTimestamp

OfflineProgressManager (sahne objesi)
  ├── anvilManager.GetOfflineGoldPerSecond(), maxOfflineSeconds (28800 = 8 saat)
  ├── offlineMessageDurationSeconds (3.5) sonra mesaj kaybolur
  ├── Load sonrası offline gold + OfflineMessageText
  └── SaveGame() ile çift ödeme engeli

Assets/Editor/DebugSaveMenu.cs → GetItWeapon/Debug (gold, kayıt, missing script tarama)
```

### Klasör Yapısı (Scripts)

```
Assets/Scripts/
├── Core/ (GameSaveData, SaveManager)
├── Economy/EconomyManager.cs
├── Forge/ (ForgeButtonHandler, SellButtonHandler, ItemData, ItemDatabase, AnvilManager)
├── Idle/OfflineProgressManager.cs
└── UI/ (GoldDisplayUI, AnvilUpgradeHandler)

Assets/Editor/DebugSaveMenu.cs
```

### UI Hierarchy (SampleScene)

```
Canvas
├── AnvilInfoText
├── UpgradeButton (+ AnvilUpgradeHandler)
├── ForgeTimerText
├── ItemIcon
├── GoldText (+ GoldDisplayUI)
├── OfflineMessageText
├── ForgeButton (+ ForgeButtonHandler)
├── LastItemText
└── SellButton (+ SellButtonHandler)
EconomyManager | SaveManager | AnvilManager | OfflineProgressManager
EventSystem
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
| 7c | Era item'ları | 🔄 | Stone + Medieval; Modern/Space sırada |

---

## Commit Kayıtları

<!-- Yeni kayıtlar EN ÜSTE eklenir (en yeni önce). -->

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
