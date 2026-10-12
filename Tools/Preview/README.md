# Unity'siz doğrulama ve önizleme aracı

Bu klasör **oyunun parçası değildir** (Unity yalnızca `Assets/` içini derler).
Burada, Unity editörü olmadan oyunun gerçek kodunu çalıştırmaya yarayan küçük
bir "sahte Unity" bulunur.

## Ne işe yarar

| Komut | Yaptığı |
|---|---|
| `./run.sh sim 40000` | Oyunu başsız (grafiksiz) simüle eder. Yapay bir oyuncu altın toplar, köylü alır, duvar diker; her kare `Awake/Start/Update/LateUpdate` gerçek script'ler üzerinde çalışır. Çökme, sonsuz döngü, null hatası ve denge sorunlarını yakalar. |
| `./run.sh shots` | Aynı simülasyonu koşturup belirli anlarda ekran görüntüsü üretir (`out/shots/*.png`). Sahnedeki tüm `SpriteRenderer`'lar yazılımda sıralanıp birleştirilir. |
| `./run.sh sheet chars` | Sprite'ları büyütülmüş bir sanat sayfası olarak basar (`king`, `chars`, `build`, `all`). Piksel sanatını hızlı gözden geçirmek için. |

## Gereksinimler

```bash
sudo apt-get install mono-mcs          # C# derleyici + çalıştırıcı
pip install pillow                     # ham RGBA -> PNG
```

## Dosyalar

- `FakeUnity.cs` — MonoBehaviour, GameObject, Transform, Sprite, Texture2D, Camera, ses,
  girdi ve bir oyun döngüsü taklidi + sprite'ları birleştiren yazılım rasterleyici.
- `SimMain.cs` — simülasyon sürücüsü, yapay oyuncu ve ekran görüntüsü programı.
- `ArtDump.cs` — sprite sayfası üretici.

## Sınırları

Bu bir Unity taklididir, Unity değildir. Fizik, gerçek render, shader, animator ve
editör davranışları yoktur; `Destroy` anında uygulanır (Unity kare sonunda yapar).
Mantık hatalarını ve görsel kompozisyonu yakalar, ama **oyunu editörde bir kez
çalıştırmanın yerini tutmaz.**
