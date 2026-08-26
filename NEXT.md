# Kaldigimiz yer / Where we left off  (2026-08-27)

## Tamamlanan
- Sprint 0-1: kural motoru + 42 test (hepsi yesil)
- Sprint 2: Blazor tek kisilik oyun (bota karsi), tarayicida dogrulandi
- Yeniden adlandirma: RenkKapis -> fUNO
- TR/EN dil destegi (yapisal loglar, LanguageState, LanguageSwitch)
- UI tasarim sistemi (semantik token, Fredoka/Nunito, SVG kart ikonlari, erisilebilirlik)
- Sprint 3: SignalR cok oyunculu (GameRoom, RoomManager, GameHub, Multiplayer.razor)

## Bugun duzeltilenler
1. BUG FIX: `<html lang="tr">` sabitti, text-transform:uppercase Ingilizce metni bile
   Turkce harf kurallariyla buyutup "IN" -> "İN" yapiyordu. Cozum: her sayfanin
   kok elementine `lang="@Language.Code"` eklendi (LanguageState.Code: "tr"/"en"),
   boylece buyutme kurali secili dile gore uygulaniyor. Tarayicida dogrulandi:
   EN modda "PLAYERS IN ROOM" (duz I), TR modda "ODADAKİ OYUNCULAR" (noktali İ).
2. TEST: Yeni proje `tests/Funo.Web.Tests` - GameRoom, RoomManager ve Strings/
   LanguageState icin 35 test eklendi. Toplam 77 test (42 Core + 35 Web).
   Kritik test: `FullGame_WithDisconnectedHumanHost_BotsFinishTheGame` - insan
   oyuncu baglantisi koptugunda botun oyunu sonuna kadar kilitlenmeden
   bitirdigini otomatik olarak dogruluyor.
3. Cok oyunculu tam akis tarayicida dogrulandi: oda kur -> katil -> baslat ->
   iki gercek oyuncu sirayla kart oynadi -> biri "sayfa unload" ile baglantisini
   kopardi -> log'da "X baglantisi koptu, yerine bot oynuyor." goruldu ve bot
   art arda hamle yapti (Ters, Ters, Joker+4).
   NOT: Bu testte "tab kapama" (tabs_close) ile bagli kalan bir test-araci
   artefakti gozlemlendi - kapatilan sekmenin SignalR baglantisi sunucu
   tarafinda hemen dusmuyor (30+ saniye gecmesine ragmen). Gercek sayfa
   navigasyonu (location.replace) ile baglanti aninda ve dogru sekilde
   kapaniyor. Bu, urun kodunda bir hata degil; sadece otomatik tarayici
   testi yaparken "tabs_close" yerine gercek navigasyon kullanmak gerekiyor.

## Sirada
1. README'yi INGILIZCE yaz: oyun mantigi, kod yapisi, ozellikler,
   kullanilan teknolojiler, kurulum adimlari.
2. Sprint 4: kayit/giris, mac gecmisi, lider tablosu (EF Core + veritabani).
3. Odalarin bosta kalinca temizlenmesi icin RoomManager.CleanupIdleRooms'u
   periyodik cagiran bir arka plan servisi (IHostedService) eklenebilir -
   su an sadece metod var, hic cagrilmiyor.

## Onemli tuzak
`dotnet run` hot-reload YAPMAZ. Razor/CSS degistirdikten sonra sunucuyu
yeniden baslat, yoksa tarayici eski koda bagli kalir ve olmayan hatalar gorursun.
`dotnet watch run` bu derdi cozer.


## 2026-08-27 - Bug fixes from live playtesting

1. FIX: In multiplayer, all bot moves after a human's turn used to resolve
   instantly in one lump broadcast (GameRoom.AdvanceBots looped to
   completion with zero delay). Refactored: GameRoom now exposes
   `TryAdvanceOneBotTurn()` - exactly one bot's turn per call, still under
   its lock. GameHub drives it in a loop (`DriveBotsAsync`), broadcasting
   and waiting 900ms after each individual bot move, matching the pacing
   already used in single-player `GameSession`. Verified live in browser:
   bot moves now appear one at a time, ~800ms apart, each fully readable
   in the log instead of all landing at once.
2. FIX: Some `HubException`s in `GameHub` threw raw Turkish literal text
   ("Oda bulunamadi.", "Once bir odaya katilmalisin.", "Oyuncu adi bos
   olamaz.") instead of the translation keys already defined in
   `Strings.cs` (`room.notFound`, `room.joinFirst`, `room.nameEmpty`).
   An English-language player would have seen Turkish error text. Fixed
   to throw the keys; the client already translates them via
   `Language.T(...)`.
3. Updated `GameRoomTests` for the new single-step bot API: the old
   `FullGame_...` test now drives `TryAdvanceOneBotTurn()` in a loop
   itself (mirroring what GameHub does), plus two new tests
   (`TryAdvanceOneBotTurn_PlaysExactlyOneTurnAtATime`,
   `TryAdvanceOneBotTurn_ReturnsFalse_WhenItIsHumanTurn`) that pin down
   the single-step contract directly. 79 tests total now (was 77).
