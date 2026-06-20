# Get It Weapon — Geliştirme Günlüğü

> Her commit'te bu dosya güncellenir. Proje geçmişine dönmek ve AI oturumlarına bağlam vermek için kullanılır.

---

## Mevcut Durum (Özet)

| Alan | Değer |
|------|-------|
| **Aşama** | 1 — Temel Forge Döngüsü (MVP) ✅ |
| **Son çalışan özellik** | FORGE → rastgele item üret → SELL → gold artışı |
| **Aktif sahne** | `Assets/Scenes/SampleScene.unity` |
| **Sonraki hedef** | Item ikonu, save/load, forge timer (idle) |
| **Henüz yok** | Offline kazanç, anvil yükseltme, envanter, ItemDatabase |

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
  ├── forgeableItems[] (ItemData listesi)
  ├── lastItemText referansı
  ├── OnForgeClicked() → rastgele item, LastItemText günceller
  └── ClearLastItem() → satış sonrası temizlik

SellButtonHandler (SellButton üzerinde)
  ├── forgeButtonHandler, economyManager, goldDisplayUI, lastItemText
  └── OnSellClicked() → sellPrice kadar gold, "No item yet"
```

### Klasör Yapısı (Scripts)

```
Assets/Scripts/
├── Economy/EconomyManager.cs
├── Forge/
│   ├── ForgeButtonHandler.cs
│   ├── SellButtonHandler.cs
│   └── ItemData.cs
└── UI/GoldDisplayUI.cs

Assets/ScriptableObjects/Items/
├── StoneSword_T1.asset
└── StoneAxe_T1.asset
```

### UI Hierarchy (SampleScene)

```
Canvas
├── GoldText (+ GoldDisplayUI)
├── ForgeButton (+ ForgeButtonHandler) → OnClick: OnForgeClicked
├── LastItemText
└── SellButton (+ SellButtonHandler) → OnClick: OnSellClicked
EconomyManager (+ EconomyManager)
EventSystem
```

---

## Aşama Haritası

| # | Aşama | Durum | Not |
|---|-------|-------|-----|
| 0 | Proje kurulumu, spec, Cursor kuralları | ✅ | `forge-master-spec.md`, `.cursor/rules/` |
| 1 | Temel forge döngüsü (gold + buton + UI) | ✅ | Forge → item → sell → gold |
| 2 | Item ikonu, ItemDatabase | ⏳ | Sırada |
| 3 | Anvil / forge yükseltme (timer) | ⏳ | Sırada |
| 4 | Offline / idle kazanç | ⏳ | — |
| 5 | Save / load (JSON + PlayerPrefs) | ⏳ | Sırada |
| 6+ | Era, PvP, clan vb. | ⏳ | Spec'e göre ileride |

---

## Commit Kayıtları

<!-- Yeni kayıtlar EN ÜSTE eklenir (en yeni önce). -->

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
