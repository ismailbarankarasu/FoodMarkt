# FoodMarkt

ASP.NET Core 10 MVC, MongoDB, Razor ViewComponents ve MailKit kullanan tek projeli mağaza ve yönetim paneli.

## Çalıştırma

.NET 10 SDK ve MongoDB gerekir. Varsayılan veritabanı `mongodb://localhost:27017` üzerindeki `FoodMarktDb` veritabanıdır.

```sh
dotnet restore
dotnet build
dotnet run
```

Yönetici girişi: `/Admin/Account/Login`. Giriş sonrası yönetim paneli açılır. Çıkış, başlıktaki POST formuyla yapılır.

İlk yönetici için geliştirme ortamında Visual Studio **Manage User Secrets** üzerinden `AdminSeed:FullName`, `AdminSeed:Email`, `AdminSeed:Password` değerlerini tanımlayın. Uygulama yalnızca `AdminUsers` boşsa hesap oluşturur; parola `PasswordHasher<AdminUser>` ile hashlenir. Seed mevcut hesabın parolasını değiştirmez. İlk kurulumdan sonra seed parolasını kaldırabilirsiniz.

E-posta için `EmailSettings:SenderEmail`, `EmailSettings:Username`, `EmailSettings:Password` değerlerini User Secrets üzerinden tanımlayın. Gmail kullanılıyorsa parola alanı uygulama parolasıdır. Gerçek parolaları appsettings veya kaynak dosyalara yazmayın. Üretimde aynı ayarları ortam değişkenleriyle (`AdminSeed__Email` gibi) veya sunucunun gizli yapılandırma sistemiyle sağlayın. Üretim kimlik doğrulama çerezleri HTTPS gerektirir.

## Formlar ve görseller

Mevcut create/update DTO'ları FluentValidation ile doğrulanır. Asenkron MVC filtresi doğrulama hatalarını ModelState'e aktarır; formlar Türkçe hata özetlerini gösterir. Ürün, satış ve kampanya seçimlerinde ilgili kaydın varlığı da kontrol edilir. Satış toplamı sunucuda hesaplanır.

Ürün, slider ve kampanya formları URL veya dosya kabul eder. JPG, PNG ve WebP için uzantı, MIME türü, dosya imzası ve 5 MB sınırı kontrol edilir. Dosyalar benzersiz adlarla `wwwroot/uploads/images` altında saklanır. Düzenlemede görsel belirtilmezse mevcut görsel korunur. Değiştirme/silme sonrasında başka kayıtta kullanılmayan yerel görseller temizlenir. URL ile bağlanan harici dosyalar silinmez. Yükleme dizinine uygulama hesabının yazma yetkisi olmalı; bu dizini dağıtımlar arasında koruyun ve MongoDB ile birlikte yedekleyin.

Silme işlemleri anti-forgery korumalı POST formlarıdır. SweetAlert2 onay ve başarı mesajları Türkçedir. SweetAlert2 mevcut CDN yaklaşımıyla yüklenir; CDN erişilemezse silme işlemi onaysız çalışmaz.

## İndeksler ve günlükler

İndeksler başlangıçta bir kez, sabit adlarla oluşturulur. Ürün listeleme/kategori filtreleri, slider sıralaması, kampanya filtreleri, satış ve abone tarih sıralamaları kapsanır. Yönetici/abone e-postası ve abone indirim kodu benzersizdir. Kategoriler için mevcut `_id` indeksi yeterlidir; mevcut metin aramasına yararı olmayan bir `Name` indeksi eklenmez.

Mevcut verilerde aynı e-posta veya indirim kodu varsa benzersiz indeks oluşturma başarısız olur ve uygulama başlamaz. Kayıtları inceleyip çakışmaları giderin; başlangıç kodu kullanıcı verisini otomatik silmez veya indeksleri düşürmez. Veritabanı hesabı indeks oluşturabilmelidir.

Serilog konsola beklenmeyen hataları, başarısız girişleri, e-posta hatalarını ve başarılı yönetici işlemlerini yazar. Üretimde konsol çıktısını barındırma ortamınızla saklayın. Üretim hata sayfası veritabanına bağımlı değildir; kullanıcıya Türkçe mesaj ve işlem numarası gösterir.

## Doğrulama

```sh
dotnet run --project Verification/FoodMart.Verification.csproj
node Verification/client-checks.cjs
```

Doğrulama aracı yerel MongoDB'de rastgele isimli `FoodMart_Verification_*` veritabanı ve rastgele parolalı bir yönetici oluşturur, HTTP üzerinden uygulamayı `localhost:5189` adresinde test eder ve sonunda geçici veritabanını siler. Bu port boş olmalıdır. Gerçek uygulama veritabanını kullanmaz. Gerçek e-posta göndermez; e-posta hatası ve kayıt geri alma akışını kontrol eder. İlk geçerli SMTP gönderimini kendi yapılandırmanızla ayrıca doğrulayın.

Kontroller giriş/çıkış, yetkilendirme, CSRF, CRUD, Türkçe doğrulama, yükleme, görsel koruma/temizleme, indeksler, mağaza sayfaları, dashboard ve üretim hata sayfasını kapsar. JavaScript kontrolü onay/iptal davranışını doğrular; görsel tarayıcı testi yerine geçmez.

Uygulama yaklaşımı için: [FluentValidation ASP.NET entegrasyonu](https://docs.fluentvalidation.net/en/latest/aspnet.html), [Serilog ASP.NET Core](https://github.com/serilog/serilog-aspnetcore), [SweetAlert2](https://sweetalert2.github.io/).
