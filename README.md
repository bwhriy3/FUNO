# RenkKapis

UNO benzeri kart oyunu. Yazilim Proje Yonetimi dersi donem projesi.

> Not: "UNO" Mattel'in tescilli markasidir. Bu proje ayni mekanigi kullanan
> bagimsiz bir uygulamadir ve marka ile iliskisi yoktur.

## Teknoloji

- .NET 10 / C#
- xUnit (birim ve butunlesme testleri)
- Planlanan: ASP.NET Core + SignalR (cok oyunculu), Blazor (arayuz)

## Mimari

```
src/RenkKapis.Core/         Kural motoru - UI ve ag bilmez, %100 test edilebilir
    Model/                  Card, Deck, Player, GameState, GameOptions
    Engine/                 GameEngine (kural mantigi), PlayResult
    Ai/                     SimpleBot (bot stratejisi)
src/RenkKapis.ConsoleSim/   Bot simulasyonu (Sprint 1 dogrulamasi)
tests/RenkKapis.Core.Tests/ Birim + butunlesme testleri
```

Temel tasarim karari: **kural motoru hicbir seye bagimli degildir.**
Arayuz, ag katmani ve veritabani Core'a bagimlidir; tersi degil.

## Kural kararlari

UNO'nun evden eve degisen kurallari `GameOptions` sinifinda acikca sabitlenmistir:

| Kural | Karar | Ayar |
|---|---|---|
| Baslangic el buyuklugu | 7 kart | `StartingHandSize` |
| +2 uzerine +2 birikir mi? | Evet | `StackDrawTwo` |
| +4 uzerine +4 birikir mi? | Hayir | `StackDrawFour` |
| Cekilen kart ayni turda oynanir mi? | Evet | `PlayDrawnCard` |
| "Tek!" demeyi unutmanin cezasi | 2 kart | `EnforceUnoCall`, `UnoPenaltyCards` |
| Iki kisilik oyunda Ters | Pas gibi davranir | `ReverseActsAsSkipInTwoPlayerGame` |

Basitlestirme: oyunun ilk acilan karti bir sayi karti olana kadar cekilir,
boylece oyun aksiyon karti efektiyle baslamaz.

## Calistirma

Testler:

    dotnet test

Bot simulasyonu (4 bot birbirine karsi tam oyun):

    dotnet run --project src/RenkKapis.ConsoleSim

Ayni oyunu tekrar uretmek icin tohum verilebilir:

    dotnet run --project src/RenkKapis.ConsoleSim -- 42

## Sprint durumu

- [x] Sprint 0 - Analiz, kural karar tablosu, proje iskeleti
- [x] Sprint 1 - Kural motoru + bot + testler (42 test, 550 tam oyun simulasyonu)
- [ ] Sprint 2 - Blazor arayuz, bota karsi oynanabilir oyun
- [ ] Sprint 3 - SignalR ile cok oyunculu, oda yonetimi
- [ ] Sprint 4 - Kayit/giris, mac gecmisi, lider tablosu
- [ ] Sprint 5 - Bug fix, dokumantasyon, sunum
