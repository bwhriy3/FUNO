# Kaldigimiz yer / Where we left off  (2026-08-28)

## Tamamlanan
- Sprint 0-3: kural motoru, Blazor tek/cok kisilik oyun, TR/EN dil destegi,
  SignalR cok oyunculu, tasarim sistemi.
- Lider tablosu ve mac gecmisi: SQLite + EF Core (Data/ klasoru). Oyuncular
  isimle takip edilir - hesap/sifre sistemi YOK (bilerek kaldirildi, asagida).
- Bosta kalan odalari temizleyen arka plan servisi (RoomCleanupService,
  5 dakikada bir kontrol, 30 dakika bosta kalan oda siliniyor).
- Toplam 79 test (42 Core + 37 Web), hepsi yesil.

## Onemli karar: hesap/giris sistemi kaldirildi
Ilk basta kullanici adi + sifre ile gercek bir giris sistemi kuruldu (cerez
tabanli, PBKDF2 hash, SQLite). Test sirasinda gereksiz karmasiklik oldugu
goruldu ve kullanicinin acik istegiyle TAMAMEN kaldirildi. Su an:
- Oyuncular hem tek kisilik hem cok oyunculu modda sadece isim yazip oynuyor
  (Multiplayer oda sistemiyle ayni mantik).
- Lider tablosu oyuncu ISMINE gore gruplaniyor (hesap degil). Ayni ismi iki
  farkli kisi kullanirsa istatistikleri karisir - bilinen ve kabul edilen
  bir sinirlama, tekrar hesap sistemine donmeden duzeltilemez.
- "My matches" (kisisel mac gecmisi) sayfasi da bu yuzden kaldirildi - hesap
  olmadan "ben kimim" sorusu guvenilir cevaplanamiyor.

## Bugun duzeltilenler
1. BUG: `text-transform: uppercase` + sabit `<html lang="tr">` kombinasyonu,
   Ingilizce metni bile Turkce harf kurallariyla buyutup yanlis sonuc
   veriyordu ("IN" -> "İN", "WINS" -> "WİNS"). Ayni hata Leaderboard
   sayfasinda da cikti; oraya da `lang="en"` eklendi.
2. BUG: SQLite'in EF Core saglayicisi `DateTimeOffset` uzerinde ORDER BY
   destekmiyor. `Match.PlayedAtUtc` alani `DateTime` (UTC) olarak degistirildi.
3. Hesap sistemi denemesi sirasinda ayni route'a hem Blazor sayfasi hem
   minimal API POST endpoint'i baglaninca "AmbiguousMatchException" olustu.
   Kod artik kaldirildigi icin gecerli degil, sadece not olarak kalsin.

## Bugun ayrica yapilanlar
- "TEK!"/"Call UNO!" butonu artik kirmizi + "!" rozetli + belirgin nabiz
  animasyonlu; oncesinde diger butonlarla (Start Game, Leaderboard) ayni
  altin rengi kullandigi icin gozden kaciyordu.
- Lider tablosuna sayfalama eklendi (sayfa basi 10 oyuncu, `?page=N`).
  25+ sahte oyuncuyla tarayicida test edildi: sayfa gecisleri, sinir disi
  sayfa numaralari (0, negatif, cok buyuk) hepsi dogru clamp ediliyor.

## Sirada (istenirse)
- README ekran goruntuleri guncel arayuzu yansitiyor mu kontrol edilmeli
  (auth kaldirildiktan sonra ust kosede artik sadece Leaderboard butonu var,
  ekran goruntuleri hala eski hali gosteriyor olabilir).
- Lider tablosuna filtre (ör. sadece cok oyunculu maclar) eklenebilir.

## Onemli tuzak
`dotnet run` hot-reload YAPMAZ. Razor/CSS degistirdikten sonra sunucuyu
yeniden baslat, yoksa tarayici eski koda bagli kalir ve olmayan hatalar gorursun.
`dotnet watch run` bu derdi cozer.
