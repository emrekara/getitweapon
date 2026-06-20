# Get It Weapon — Geliştirme Günlüğü

> Her commit'te bu dosya güncellenir. Proje geçmişine dönmek ve AI oturumlarına bağlam vermek için kullanılır.

---

## Mevcut Durum (Özet)

| Alan | Değer |
|------|-------|
| **Aşama** | 4 — Anvil yükseltme ✅ |
| **Son çalışan özellik** | Anvil upgrade (gold harcar, seviye/çağ), forge süresi AnvilManager'dan |
| **Aktif sahne** | `Assets/Scenes/SampleScene.unity` |
| **Sonraki hedef** | Offline kazanç, ItemDatabase, forge UX iyileştirmeleri |
| **Henüz yok** | Offline kazanç, envanter, ItemDatabase, tier/era ölçekleme |

### Sistem Haritası (AI için hızlı referans)

```
EconomyManager (sahne objesi)
  ├── CurrentGold, AddGold(), TrySpendGold()
  └── Inspector: startingGold

GoldDisplayUI (GoldText üzerinde)
  ├── EconomyManager + TextMeshProUGUI referansı
  └── RefreshDisplay() → "Gold: X"

ItemData (ScriptableObject)
  ├── itemName, icon, baseAttack, sellPrice, tier
  └── Create: Assets → Create → GetItWeapon → Item Data

ForgeButtonHandler (ForgeButton üzerinde)
  ├── anvilManager.GetForgeDuration(), forgeTimerText
  └── Coroutine ile forge + SaveGame

AnvilManager (sahne objesi)
  ├── GetForgeDuration(), GetUpgradeCost(), TryUpgrade(), CurrentEra
  └── baseForgeDuration, baseUpgradeCost Inspector'dan

AnvilUpgradeHandler (UpgradeButton üzerinde)
  └── OnUpgradeClicked, yetersiz gold → "Need XXg"

SaveManager → GameSaveData: gold, lastItemIndex, anvilLevel

Assets/Editor/DebugSaveMenu.cs → GetItWeapon/Debug menüsü
```

### Klasör Yapısı (Scripts)

```
Assets/Scripts/
├── Core/ (GameSaveData, SaveManager)
├── Economy/EconomyManager.cs
├── Forge/ (ForgeButtonHandler, SellButtonHandler, ItemData, AnvilManager)
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
├── ForgeButton (+ ForgeButtonHandler)
├── LastItemText
└── SellButton (+ SellButtonHandler)
EconomyManager | SaveManager | AnvilManager
EventSystem
```

---

## Aşama Haritası

| # | Aşama | Durum | Not |
|---|-------|-------|-----|
| 0 | Proje kurulumu, spec, Cursor kuralları | ✅ | `forge-master-spec.md`, `.cursor/rules/` |
| 1 | Temel forge döngüsü (gold + buton + UI) | ✅ | Forge → item → sell → gold |
| 2 | Item ikonu | ✅ | Placeholder PNG + UI Image |
| 2b | ItemDatabase | ⏳ | İleride |
| 3 | Forge timer (üretim süresi) | ✅ | AnvilManager'dan süre |
| 4 | Anvil yükseltme / çağ | ✅ | Upgrade UI + save |
| 5 | Save / load (JSON + PlayerPrefs) | ✅ | gold + lastItemIndex + anvilLevel |
| 6 | Offline / idle kazanç | ⏳ | Sırada |
| 7+ | Era, PvP, clan vb. | ⏳ | Spec'e göre ileride |

---

## Commit Kayıtları

<!-- Yeni kayıtlar EN ÜSTE eklenir (en yeni önce). -->

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
