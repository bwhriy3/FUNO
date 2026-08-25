# Kaldigimiz yer / Where we left off  (2026-08-25)

## Tamamlanan
- Sprint 0-1: kural motoru + 42 test (hepsi yesil)
- Sprint 2: Blazor tek kisilik oyun (bota karsi), tarayicida dogrulandi
- Yeniden adlandirma: RenkKapis -> fUNO (klasor, namespace, proje adlari)
- TR/EN dil destegi: Localization/Strings.cs, LanguageState, LanguageSwitch
  - Loglar yapisal (LogEntry: anahtar + parametre), ceviri ekranda yapiliyor
  - Core artik metin uretmiyor, sadece anahtar doner (EngineMessages)
- UI tasarim sistemi: semantik token'lar, Fredoka/Nunito, SVG kart ikonlari,
  prefers-reduced-motion, 44px dokunma hedefleri, 375/768/1024 kirilim noktalari
- Sprint 3 altyapisi: GameRoom, RoomManager, GameHub (SignalR), Multiplayer.razor
  - Dogrulandi: oda kurma (kod 9QEGZ), ikinci oyuncunun katilmasi,
    ayni odada iki farkli dil (Bahriye TR / Ahmet EN) ayni anda calisiyor

## Yarin ilk isler
1. BUG: `text-transform: uppercase` Turkce yerelde "IN" -> "İN" yapiyor
   ("PLAYERS İN ROOM"). Sebep: App.razor'da <html lang="tr"> sabit.
   Cozum: lang niteligini secilen dile gore ayarla ya da .pile-label'da
   text-transform yerine metni oldugu gibi birak.
2. Cok oyunculu oyunu bastan sona test et: Oyunu Baslat -> kart oynama ->
   kazanma. Simdiye kadar sadece lobiye kadar test edildi.
3. Baglanti kopma senaryosu: bir sekmeyi kapat, yerine botun oynadigini dogrula.
4. README'yi INGILIZCE yaz: oyun mantigi, kod yapisi, ozellikler,
   kullanilan teknolojiler, kurulum adimlari. (Su anki README Turkce ve eski.)
5. GameRoom icin birim test yaz (su an sadece Core test ediliyor).

## Onemli tuzak
`dotnet run` hot-reload YAPMAZ. Razor/CSS degistirdikten sonra sunucuyu
yeniden baslat, yoksa tarayici eski koda bagli kalir ve olmayan hatalar gorursun.
`dotnet watch run` bu derdi cozer.
