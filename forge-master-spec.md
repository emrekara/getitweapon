# Forge Master – Idle RPG — Teknik Reverse-Engineering Spec

> Geliştirici: **Lessmore GmbH / Lessmore UG** (We Are Warriors, Eatventure yapımcısı)
> Paket: `com.hariwn.legendofcivilizations` · iOS App Store: *Forge Master – Idle RPG*
> Tür: Idle / Incremental RPG + PvP + Clan · Min iOS 13, ~198 MB
> Bu doküman dışarıdan gözlemle (store verisi, sürüm notları, oyuncu yorumları) çıkarılmış bir **klon/yeniden üretim spesifikasyonudur**. Resmi kaynak değildir.

---

## 1. Oyunun Özeti

Oyuncu bir demirci ustasıdır. Çağlar boyunca (Taş → Orta Çağ → Modern → Uzay → Quantum → Underworld) demirhanesini (anvil/örs) yükseltir, silah-zırh üretir (forge), kahramanını güçlendirir ve diğer oyunculara karşı PvP arenada / klan savaşlarında yarışır.

Çekirdek döngü idle (boşta ilerleme) mantığında: forge yükseltmeleri **gerçek zamanlı timer** ile çalışır (saatler/günler sürebilir), oyuncu çevrimdışıyken de kaynak/güç birikir. Reklam yok; gelir modeli tamamen IAP (gem + paketler).

---

## 2. Teknoloji Stack Tahmini

| Katman | Seçim | Gerekçe |
|--------|-------|---------|
| Game engine | **Unity (C#)** | Lessmore'un diğer oyunları (We Are Warriors, Eatventure) Unity. 2D, cross-platform, idle UI ağırlıklı. 198 MB boyut Unity ile tutarlı. |
| UI | Unity UGUI / Canvas | Bol popup, liste, envanter grid'i |
| Backend | Authoritative game server (muhtemelen Node.js / Go) + managed DB | PvP, clan, global chat, leaderboard, anti-cheat gerektiriyor |
| Realtime | WebSocket | Global chat + clan chat realtime |
| Auth | Anonim cihaz ID + opsiyonel Google/Apple login (cross-device) | Yorumlar: "Google hesabı şart değil ama başka cihaza taşımak için var" |
| Veri | Sunucu-otoriter state + client cache | Whale/refund tartışmaları sunucu tarafı ekonomi kontrolünü gösteriyor |
| Analytics/Ads SDK | 3rd-party advertising + analytics (App Privacy'de "Third-Party Advertising" beyanı var, reklam göstermese de attribution/IAP analitiği için) | Store privacy etiketi |

> **Kritik mimari kararı:** Ekonomi ve ilerleme *sunucu otoriter* olmalı. Idle ilerleme client'ta hesaplanabilir ama doğrulaması sunucuda yapılmalı (offline süresi sunucu zaman damgasıyla).

---

## 3. Çekirdek Sistemler

### 3.1 Forge (Üretim) Sistemi
- Oyuncu örste ekipman üretir: her item için **attack / defense / özel substat** değerleri.
- Substat örnekleri: `%Health` (1.8.1'de eklendi), kalkan delme ("ignore shield") gibi özel efektler.
- Item tier'ları (T1–T5+) ve çağ bazlı malzemeler. Item kalitesi renk kodlu (yorum: "kırmızı = uzay seviyesi").
- **Auto Forge + Filter System** (1.5.1): otomatik üretim ve istenmeyen item'ları otomatik satma/filtreleme.
- Üretilen item satılarak para (gold/coin) elde edilir → sell price bir denge parametresi (2.0.0'da nerf edildi).

**Veri modeli (item):**
```
Item {
  id, era, tier, slot (weapon/armor/...),
  baseAttack, baseDefense,
  substats: [{ type: "%Health"|"%Damage"|..., value }],
  rarity, sellPrice, powerScore
}
```

### 3.2 Anvil / Çağ İlerlemesi (Idle Timer)
- Anvil yükseltmesi yeni çağı açar. Yükseltme **uzun timer** (oyuncu yorumu: "3 gün bekliyorum").
- Hızlandırma: gem harcama veya **Clock Winder** item'ı (1.7.0'da shop'a eklendi).
- Çağlar: Stone → Medieval → Modern → Space → Quantum → **Underworld** (1.8.0 yeni çağ).

```
Anvil { level, currentEra, upgradeEndsAt (server timestamp), upgradeCostGold, speedUpCostGems }
```

### 3.3 Hero (Kahraman) & Stats
- Kahramanın attack/defense/health statları ekipman + tech + mount + skin ile artar.
- Health Regen statı (1.7.0'da buff). PvP'de skill kullanımı.

### 3.4 Tech Tree (Araştırma)
- Forge hızı, maliyet düşürme (cost reduction), savaş gücü artıran düğümler.
- Yeni node örnekleri (1.7.0): "ekstra egg drop şansı", "ekstra mount summon şansı".
- **Denge uyarısı:** 2.0.0 güncellemesinde tech yüzdeleri (forge progression + cost reduction) yarıya indirildi → büyük oyuncu tepkisi. Tech değerleri sunucudan config ile çekilmeli (remote config), client'a gömülmemeli.

### 3.5 Pets / Eggs & Mounts
- **Egg** açarak pet, **mount** summon etme (gacha tarzı, çoklu summon 2.0.0'da geldi).
- Mount: `%Damage` ve `%Health` artışı (1.7.0).
- Bunlar collection + power kaynağı; gacha/RNG ekonomisi.

### 3.6 Skills
- Kahraman skill'leri (toplu/yüksek miktarda summon edilebiliyor — 2.0.0).
- Muhtemelen RNG ile çekilen, PvP'de kullanılan aktif/pasif yetenekler.

### 3.7 Ascension (Prestige)
- **Ascension** (2.0.0): klasik idle "prestige" mekaniği. İlerlemeyi sıfırlayıp kalıcı çarpan/bonus kazanma. Geç oyun derinliği için.

### 3.8 Skins & Stepping Stones
- Kozmetik + **stat veren** skin'ler. "Stepping Stones" eventi (1.8.3): stat-boost skin'leri açma; set tamamlama bonus veriyor.
- Aylık tema skinleri (March/April skins). Skin'ler autorun ödüllerinde ve minigame'de görünmeli.

---

## 4. Sosyal / Kompetitif Sistemler

### 4.1 PvP Arena
- Asenkron eşleşme: oyuncular gear/power karşılaştırır, kahraman skill'leriyle savaşır, ödül kazanır.
- Kısa oturum dövüş (auto-resolve battle muhtemel). Bağlantı kararsızlığında arena bug'ı (1.8.4 fix).
- PvP profilinde mount/skin görünür.

### 4.2 Clans (Klanlar)
- Klan kur/katıl, klan chat, koordinasyon, klan lider tablosu.

### 4.3 Clan Wars (1.5.1)
- Çok günlü etkinlik (Day 1–6+). Günlük sonuç ekranları, puanlama.
- "Forge Divine Equipment" gibi klan-savaşı aksiyonları (1.8.1).
- **Anti-abuse:** 50k puan sonrası savaştan çıkıp başka klana geçme ("clan hopping") engellendi (1.8.3) → katılım kilitleme mantığı gerek.

### 4.4 Global Chat + Clan Chat (Realtime)
- WebSocket. Moderasyon: chat-mute sistemi (mute edilen oyuncu için şeffaflık 1.8.1).

### 4.5 Leaderboards
- Global power leaderboard + clan leaderboard.

---

## 5. Ekonomi & Para Birimleri

| Birim | Kaynak | Harcama |
|-------|--------|---------|
| Gold/Coin | Item satışı, idle | Anvil/forge yükseltme |
| Hammers | İlerleme/günlük | Forge işlemleri (oyuncu: "daha çok hammer istiyoruz") |
| Gems (premium) | IAP, az miktarda free drop | Timer hızlandırma, gacha summon, paketler |
| Clock Winders | Shop (gem) | Upgrade timer kısaltma |

**IAP fiyatları (gözlemlenen):**
- Starter Offer $1.99 / $6.99
- Gem Pack: Tiny–Large ($1.99–$24.99)
- Daily Deal: Tiny $1.99 → Large $49.99
- Battle Progress Pass $9.99

**Tasarım notu (oyuncu geri bildiriminden çıkan riskler):**
1. Timer'lar çok uzun → erken oyuncu kaybı riski. QoL: opsiyonel reklam ile hızlandırma talebi yüksek (şu an reklam yok).
2. Ekonomi nerf'i (2.0.0) whale'lerde refund/öfke yarattı → **geriye dönük denge değişimlerinde adil tazminat ve net iletişim** kritik.
3. Gem'ler F2P için pahalı algılanıyor.

---

## 6. Mini Oyun
- Şans bazlı bir minigame eklendi (oyuncu: "eğlenceli ama tamamen luck-based"). Forge sırasında oyalanma içeriği. Skin'ler minigame'de görünmeli.

---

## 7. Live-Ops / Sürüm Kadansı
- ~2–4 haftada bir güncelleme. Aylık tema skin'leri, sezonsal eventler (Stepping Stones), yeni çağ/feature drop'ları.
- Bug-fix odaklı ara sürümler + büyük feature minor bump (1.5 Clan Wars, 1.8 Underworld, 2.0 Ascension).

---

## 8. Klon İçin Önerilen Mimari (Yeniden Üretim Planı)

### 8.1 Client (Unity)
```
/Core        → GameState, EconomyManager, TimeManager (server-synced)
/Forge       → ItemGenerator, ForgeController, AutoForgeFilter
/Hero        → StatAggregator (equipment+tech+mount+skin)
/Idle        → OfflineProgressCalculator (server timestamp diff)
/Gacha       → Egg/Mount/Skill summon (RNG seed server'dan)
/PvP         → BattleResolver (deterministik), MatchmakingClient
/Social      → ChatClient (WS), ClanService, Leaderboard
/Meta        → AscensionManager, TechTree, EventManager
/IAP         → StoreController (Unity IAP), ReceiptValidator(server)
/Net         → ApiClient, WebSocketClient, RemoteConfig
```

### 8.2 Backend (authoritative)
```
- Auth service (anon + Apple/Google link)
- Player state service (Postgres/Redis)
- Economy service (config-driven, remote-tunable)
- Matchmaking + PvP resolution (deterministic, server-validated)
- Clan service + Clan War scheduler (cron/state machine)
- Chat service (WebSocket + moderation/mute)
- Leaderboard (Redis sorted sets)
- IAP receipt validation (Apple/Google)
- RemoteConfig (tech %, sell price, drop rates — canlı ayarlanabilir)
- Anti-cheat: tüm power/ekonomi mutasyonları sunucuda doğrulanır
```

### 8.3 Idle/Offline Hesabı (kritik)
```
gain = ratePerSecond * (now_server - lastClaim_server)
```
Client tahmini gösterir, **claim anında sunucu** kesin değeri verir. Cihaz saatiyle asla güvenme.

### 8.4 Deterministik PvP
Savaş sonucu istemcide gösterilse de sunucuda aynı seed + statlarla yeniden hesaplanabilir olmalı (replay/doğrulama). RNG seed sunucudan.

### 8.5 Remote Config zorunlu
2.0.0 nerf olayının dersi: tech %, sell price, drop rate, timer süreleri **hiçbir zaman build'e gömülmez**; remote config'ten gelir, A/B ve canlı denge için.

---

## 9. Veri Şeması (özet)

```
Player { id, era, anvilLevel, gold, hammers, gems,
         heroStats, equippedItems[], inventory[],
         techTree{}, mounts[], pets[], skills[], skins[],
         ascensionLevel, ascensionBonuses,
         clanId, lastClaimTs }

Clan { id, name, members[], war{ day, score, state }, leaderboardRank }

ForgeJob { playerId, itemTemplate, startTs, endTs, speedUpApplied }

PvPMatch { id, attackerId, defenderId, seed, resultLog, rewards }

ConfigBlob (remote) { techValues, sellPriceMultiplier, dropRates,
                      upgradeTimers, gachaWeights }
```

---

## 10. Bilinen Zayıf Noktalar (klonda kaçınılacaklar)
1. Gün sıfırlamasında crash / uzun yükleme → reset işini sunucu tarafı batch + kademeli yapmak.
2. Aşırı uzun timer'lar, az hammer → erken funnel kaybı. Opsiyonel reklamlı hızlandırma değerlendirilmeli.
3. Geriye dönük ekonomi nerf'inin tazminatsız uygulanması → topluluk güveni krizi. Net changelog + adil refund.
4. Tamamen şansa dayalı minigame eleştirisi → skill unsuru ekle.
5. Bağlantı kopmasında PvP/arena bug → sunucu otoriter resolution + reconnect.

---

*Bu spec yalnızca kamuya açık store/changelog/oyuncu verisinden çıkarımdır; iç kaynak kod veya resmi tasarım dokümanı değildir.*
