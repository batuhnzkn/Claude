# Kingdom 2D — *Kingdom: New Lands* tarzı 2D krallık savunma oyunu

Unity ile yapılmış, yandan görünümlü (side-scroll) bir krallık kurma & gece savunma oyunu.
Atlı hükümdarı yönetir, altın atarak köylü toplar, onlara alet verip okçu/inşaatçı/çiftçi/şövalye
yapar, duvarlar diker ve geceleri deliklerden çıkan **Greed** sürülerine karşı tacını korursun.

> **Projede tek bir görsel veya ses dosyası yok.** Tüm pixel-art sprite'lar, animasyon kareleri,
> manzara, paralaks tepeler ve ses efektleri oyun açılırken **kodla üretilir**. Bu yüzden proje
> çok hafiftir ve hazır asset indirmeye gerek duymaz.

---

## 1. Nasıl çalıştırılır

Üç yoldan biri yeterli (hepsinde sahne hazırlamaya gerek yok):

**A) Bu klasörü doğrudan açmak (en hızlı)**
1. Unity Hub → `Add` → bu klasörü seç → aç.
   (Hub farklı bir sürüm önerirse kabul edin; proje Unity 2021.3 / 2022 LTS / Unity 6 ile uyumludur.)
2. Play tuşuna bas. Oyun kendini kurar.

**B) Yeni bir Unity projesine kopyalamak**
1. Unity Hub'da yeni bir **2D (Built-in Render Pipeline)** proje aç.
2. Bu depodaki `Assets/Kingdom2D` klasörünü projenin `Assets` klasörüne kopyala.
3. Play tuşuna bas — açık olan boş sahne yeterlidir.

**C) Hazır sahne üretmek (ayarları Inspector'dan kurcalamak için)**
- Üst menüden **Kingdom 2D → Oyun Sahnesi Olustur**. `Assets/Kingdom2D/Scenes/Kingdom.unity`
  oluşur, içindeki `Kingdom2D` nesnesinin `GameBootstrap` bileşeninden tüm denge değerlerini
  görebilir ve oyun çalışırken bile değiştirebilirsin.

> Neden sahne hazırlamak gerekmiyor? `GameBootstrap`, `[RuntimeInitializeOnLoadMethod]` ile
> oyun başlarken kamerayı, dünyayı, kralı, arayüzü ve sesleri kendisi kurar.

---

## 2. Kontroller

| Tuş | İşlev |
|---|---|
| `A` / `D` veya `←` / `→` | Atı sür |
| `Space` (basılı tut) veya `S` / `↓` | Altın at / ödeme yap |
| `R` | Oyun bitince yeniden başla |
| `Esc` | Oyundan çık (derlenmiş sürümde) |
| Gamepad | Sol çubuk = hareket, A/RT = altın atma |

Ödeme noktasının (tezgah, duvar, çadır, üs) yanına gidince üstünde **altın ikonu + kalan maliyet**
görünür. Yaklaş, `Space`'i basılı tut, altınlar akar. Yakında ödeme noktası yoksa altın yere düşer;
köylüler onu alıp üsse taşır.

---

## 3. Oyunun döngüsü

**Gündüz (yaklaşık 65 sn):** ekonomi kurarsın.
- **Serseri kampları** (±30, ±66): 1 altın = 1 yeni köylü.
- **Alet tezgahları** (üssün yanında): boş bir köylü gelip aleti alır ve rolü değişir.
  - 🏹 **Yay (2)** → Okçu: gündüz tavşan/geyik avlayıp altın getirir, gece duvar üstünden ok atar.
  - 🔨 **Çekiç (2)** → İnşaatçı: ödenmiş duvar/tarla/üs yükseltmelerini inşa eder, hasarı tamir eder.
  - 🌾 **Tırpan (3)** → Çiftçi: tarlada çalışır, hasadı altın olarak üsse taşır.
  - ⚔️ **Kılıç (5)** → Şövalye: duvarın önünde durup greed'lerle göğüs göğüse çarpışır. *(Üs 2. kademe şart)*
- **Tarlalar** (±15.5): 4 altın ile açılır, inşaatçı kurar, çiftçi çalıştırır.
- **Duvar mevzileri** (±20, ±38, ±58, ±80): 4 / 9 / 18 altın ile kazık → ahşap palisad → taş sur.
  Her kademe daha çok okçu mevzisi ve çok daha fazla dayanıklılık verir.
- **Üs yükseltmesi** (merkez): 14 ve 34 altın. Her kademe kesenin kapasitesini +8 büyütür.
- Haritaya dağılmış **altın yığınlarını** toplamayı ihmal etme — toplamazsan geceleyin greed'ler alır.

**Gece (yaklaşık 38 sn):** haritanın iki ucundaki deliklerden Greed'ler çıkar.
- Önce duvara saldırırlar; duvar yıkılırsa içeri girerler.
- Yerdeki altınları ve yalnız köylüleri kapıp deliğe kaçarlar (kaçırılan köylü kaybedilir).
- Krala ulaşırlarsa **önce altınlarını**, altın kalmadıysa **tacını** alırlar → oyun biter.
- Şafakta güneşte yanarlar, kaçamayanlar ölür.

**Greed türleri:** Hayalet (hızlı, zayıf) → Yürüyücü → Devasa (duvar kırıcı, 17 can) → Uçan
(duvarları aşar, doğrudan altına/krala gider). Her 5. gün **Kanlı Ay**: sürü neredeyse iki katı.

**Amaç:** mümkün olduğunca çok gün dayanmak. En iyi günün kaydedilir (`PlayerPrefs`).

---

## 4. Taktik ipuçları

1. İlk gün: 2 köylü topla → 2 yay al. Okçu yoksa gece kimse seni korumaz.
2. Üçüncü gün öncesi 1 **inşaatçı** al; inşaatçı olmayan krallıkta ödenen duvar asla yapılmaz
   (ekranda "INSAATCI GEREK" uyarısı çıkar).
3. Tarla + çiftçi, avcılıktan daha istikrarlı gelirdir. 2 tarla × 2 çiftçi iyi bir temeldir.
4. Duvarı yükseltmek yeni duvar dikmekten çoğu zaman daha iyidir: taş sur 600 cana ve 3 okçu
   mevzisine sahiptir.
5. Gece dışarıda kalma. Altın taşıyorken yakalanırsan sadece altını kaybedersin; kesen boşken
   yakalanırsan taç gider.
6. Kanlı Ay gecelerinde dış duvarı feda edip içeride yoğunlaş.

---

## 5. Ayarları değiştirme

Tüm denge değerleri `GameConfig` içinde tek yerde toplanmıştır
(`Assets/Kingdom2D/Scripts/Core/GameConfig.cs`): gün/gece süreleri, hızlar, maliyetler, dalga
büyüme eğrisi, duvar canları, kamera... Hazır sahneyi oluşturduysan bunları `GameBootstrap`
bileşeninin Inspector'ından oyun çalışırken de değiştirebilirsin.

---

## 6. Proje yapısı

```
Assets/Kingdom2D/
├─ Scripts/
│  ├─ Art/          Kodla üretilen tüm grafikler
│  │  ├─ Pal.cs         Renk paleti + karakter haritası renk tablosu
│  │  ├─ Pix.cs         Piksel tuvali (dikdörtgen, daire, üçgen, çizgi, karakter haritası)
│  │  ├─ PixelFont.cs   3x5 piksel font (UI paketine ve font dosyasına ihtiyaç yok)
│  │  ├─ Art.cs         Karakterler, greed'ler, altın, ok, hayvanlar (piksel haritaları)
│  │  ├─ ArtWorld.cs    Duvarlar, üs, tarla, tezgahlar, delikler, ağaçlar, gökyüzü
│  │  └─ ArtExtra.cs    Arka plan orman şeridi + degrade gökyüzü dokusu
│  ├─ Core/
│  │  ├─ GameBootstrap.cs  Açılış: her şeyi koddan kurar, yeniden başlatmayı yönetir
│  │  ├─ GameManager.cs    Kayıtlar, gün olayları, sorgular, oyun sonu
│  │  ├─ GameConfig.cs     Tüm denge değerleri
│  │  ├─ DayNight.cs       Gün döngüsü, evreler, kanlı ay
│  │  ├─ WorldMap.cs       Harita üretimi (üs, duvarlar, kamplar, delikler, doğa, hazine)
│  │  ├─ Background.cs     Gökyüzü degradesi, yıldızlar, güneş/ay, bulutlar, paralaks, karartma
│  │  ├─ CameraRig.cs      Takip eden kamera + sarsıntı
│  │  ├─ Hud.cs            Arayüz (sprite + piksel font ile)
│  │  ├─ Sfx.cs            Matematiksel olarak üretilen ses efektleri
│  │  ├─ Fx.cs             Parçacık ve yükselen yazı efektleri
│  │  ├─ GameInput.cs      Eski Input Manager + yeni Input System uyumlu girdi
│  │  └─ Util.cs / Layer.cs / Enums.cs
│  ├─ Actors/        Kral, köylü, serseri, greed, delik, ok, hayvan, altın, ödeme noktası
│  ├─ Roles/         Köylü rol davranışları (peasant / archer / builder / farmer / knight)
│  └─ Structures/    Duvar, üs, tarla, alet tezgahı + ortak Structure temeli
└─ Editor/
   └─ KingdomMenu.cs  "Kingdom 2D" menüsü (sahne oluştur / aç, rekoru sıfırla)
```

**Teknik notlar**
- Fizik motoru kullanılmaz: dünya tek boyutlu bir hattır, konumlar elle hesaplanır
  (deterministik, hafif ve Collider ayarı derdi yok).
- Sprite'lar `Texture2D` + `Sprite.Create` ile üretilip önbelleğe alınır (toplam ~170 doku).
- Sesler `AudioClip.Create` ile sinüs/kare/testere dalgalarından sentezlenir.
- Arayüz Unity UI (uGUI/TextMeshPro) kullanmaz; kameraya bağlı sprite'lar ve 3x5 piksel font ile
  çizilir, böylece paket bağımlılığı yoktur.
- Kod hem eski hem yeni girdi sistemiyle derlenir (`ENABLE_INPUT_SYSTEM` kontrolü).

---

## 7. Geliştirme fikirleri

- Greed deliklerinin yıkılabilmesi (şövalyelerle) ve ada/bölge geçişi (*New Lands* gibi).
- At yükseltmeleri (daha hızlı at, daha büyük kese), köpek/binek yardımcıları.
- Kayıt/yükleme, farklı mevsimler, müzik.
- Mobil için dokunmatik kontrol (sol/sağ bölge + altın at düğmesi).
