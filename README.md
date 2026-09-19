# 🛒 FoodMart

ASP.NET Core 10 MVC ve MongoDB ile geliştirilmiş, dinamik ürün vitrini ve yönetim paneli içeren bir gıda mağazası portföy projesidir. Uygulama ürün, kategori, kampanya, satış ve abone verilerini yönetir; çevrim içi sipariş veya ödeme altyapısı sunmaz.

## 📌 Proje Hakkında

FoodMart; Razor ViewComponent yapısı ile oluşturulmuş dinamik bir ana sayfa, ürün arama ve filtreleme ekranları, ürün detayları ve yetkilendirilmiş bir admin paneli içerir. Dashboard üzerinde ürün, satış, gelir, abone ve kategori istatistikleri sunulur. Ziyaretçiler indirim kampanyasına abone olabilir ve MailKit üzerinden kişisel indirim kodlarını e-posta ile alabilir.

## ✨ Özellikler

- Dinamik ana sayfa, kategori ve ürün vitrinleri
- Ziyaretçiye özel sepet, adet güncelleme ve güncel fiyat/stok kontrolü
- Ürün arama, kategori filtreleme ve ürün detay sayfası
- Kampanya ve çok satan ürün bölümleri
- Admin paneli ve cookie tabanlı kimlik doğrulama
- FluentValidation ile Türkçe form doğrulaması
- JPG, PNG ve WebP görsel yükleme doğrulaması
- MongoDB indeksleri ve Serilog uygulama günlükleri
- MailKit ile indirim kodu e-posta akışı
- SweetAlert2 ile güvenli silme onayları

## 🛍️ Kullanıcı Tarafı

Kullanıcılar ana sayfada aktif kategorileri, güncel ürünleri, kampanyaları ve çok satanları görüntüleyebilir. Ürünler tüm ürünler ekranından, kategori sayfasından veya arama formundan bulunabilir. Ürün detayında kategori, fiyat, indirimli fiyat, stok ve açıklama bilgileri gösterilir. Aktif olmayan ürünler storefront üzerinde yayınlanmaz.

Ana sayfadaki gezinme bağlantıları `#categories`, `#products`, `#discounts`, `#popular-products` ve `#contact` bölümlerine yönelir. Ürün sayfalarından aynı bağlantılar ana sayfaya dönerek ilgili bölüme ulaşır.

## 🛒 Sepet

Ürün kartlarından ve ürün detayından **Sepete Ekle** düğmesiyle ürün eklenebilir. Header'daki sepet bağlantısı ürün adedini ve toplam tutarı gösterir. `/Cart` ekranında adet güncellenebilir, ürün çıkarılabilir veya sepet temizlenebilir.

Sepette yalnızca ürün kimliği ve adet sunucu oturumunda saklanır. Fiyat ve indirimler her istekte MongoDB'den okunur; pasif veya stoksuz ürünler çıkarılır, azalan stoğa göre adet düzeltilir. Tüm değişiklikler anti-forgery korumalı POST işlemleridir. Sepet stok ayırmaz ve sipariş/ödeme oluşturmaz.

Oturum 24 saat hareketsizlikte sona erer. Mevcut tek sunucu kurulumunda bellek içi oturum kullanılır; uygulama yeniden başlatıldığında sepetler sıfırlanır. Sepetler ziyaretçi çereziyle birbirinden ayrılır. Her ürün için en fazla 99 adet, toplamda 50 farklı ürün desteklenir.

## 🛠️ Admin Paneli

`/Admin/Account/Login` adresindeki girişten sonra yetkili kullanıcılar şu bölümleri yönetebilir:

- Dashboard
- Kategoriler
- Ürünler
- Slider özellikleri
- İndirim kampanyaları
- Satış kayıtları
- Aboneler
- Yönetici oturumu ve çıkış işlemi

Kategori, ürün, özellik, indirim ve satış kayıtları için oluşturma, güncelleme ve silme işlemleri bulunmaktadır. Silme işlemleri yalnızca korumalı POST istekleriyle yapılır.

## 📊 Dashboard

Dashboard; toplam ürün sayısını, toplam satışları, toplam geliri, abone sayısını, son 7 günün satış ve gelir grafiğini, kategori dağılımını, çok satan ürünleri ve son satış kayıtlarını gösterir.

## 🔐 Güvenlik

- Cookie Authentication ve yönetici yetkilendirmesi
- `PasswordHasher<AdminUser>` ile parola hashleme
- Global anti-forgery koruması
- FluentValidation ve sunucu tarafı ModelState doğrulaması
- Görsel uzantısı, MIME türü, dosya imzası ve 5 MB boyut kontrolü
- SMTP, yönetici seed ve veritabanı bilgilerinin User Secrets/ortam değişkenleriyle saklanması
- Üretimde Türkçe, işlem numaralı hata sayfası
- Serilog ile başarısız giriş ve uygulama hatası günlükleri

## 🧰 Kullanılan Teknolojiler

- .NET 10 ve ASP.NET Core MVC
- C# ve Razor
- MongoDB ve MongoDB.Driver
- Razor ViewComponents
- FluentValidation
- MailKit ve MimeKit
- Serilog.AspNetCore
- Bootstrap 5 ve Bootstrap Icons
- jQuery Validation
- Chart.js
- SweetAlert2
- JavaScript ve CSS

## 🏗️ Proje Yapısı

```text
Areas/Admin       Yönetim paneli görünümleri ve controller'ları
Controllers       Storefront ve abonelik controller'ları
Dtos              Form ve sonuç DTO'ları
Entities          MongoDB belge modelleri
Services          MongoDB, e-posta, görsel ve iş servisleri
Validators        FluentValidation kuralları
ViewComponents    Storefront ve layout bileşenleri
Views             Razor storefront görünümleri
wwwroot           CSS, JavaScript, tema ve yüklenen görseller
Verification      Entegrasyon ve istemci doğrulama araçları
```

## 🖼️ Görseller

Ekran görüntüleri proje tesliminde `docs/screenshots/` klasörüne eklenebilir. Bu depoda henüz ekran görüntüsü bulunmadığı için uydurma görsel kullanılmamıştır.

## ⚙️ Kurulum

Gereksinimler:

- .NET 10 SDK
- Yerel veya erişilebilir bir MongoDB sunucusu
- Gerçek e-posta gönderimi için SMTP hesabı

MongoDB varsayılan olarak `mongodb://localhost:27017` ve `FoodMarktDb` veritabanını kullanır. Ayarlar `appsettings.json` veya ortam değişkenleriyle değiştirilebilir.

```sh
dotnet restore
dotnet build
dotnet run
```

İlk yönetici hesabını oluşturmak için User Secrets üzerinden şu değerleri tanımlayın:

```sh
dotnet user-secrets set "AdminSeed:FullName" "Yönetici"
dotnet user-secrets set "AdminSeed:Email" "yonetici@example.com"
dotnet user-secrets set "AdminSeed:Password" "Güçlü-Bir-Parola"
```

SMTP için `EmailSettings:SenderEmail`, `EmailSettings:Username` ve `EmailSettings:Password` değerlerini User Secrets veya ortam değişkenleriyle sağlayın. Gmail kullanıyorsanız normal hesap parolası yerine uygulama parolası kullanın. Gerçek parolaları kaynak dosyalarına yazmayın.

## 🧪 Test / Doğrulama

```sh
dotnet restore
dotnet build
dotnet run --project Verification/FoodMart.Verification.csproj
node Verification/client-checks.cjs
```

Verification projesi geçici bir MongoDB veritabanı oluşturur ve kimlik doğrulama, CSRF, CRUD, görsel yükleme, storefront sayfaları, navigation anchor'ları, dashboard ve üretim hata sayfasını kontrol eder. SMTP ayarları eksik bırakıldığı için gerçek e-posta göndermez; abonelikteki rollback davranışını doğrular. `client-checks.cjs` SweetAlert2 onay, iptal ve CDN yokkenki form davranışını test eder.

## 📧 MailKit

Abonelik formu önce benzersiz indirim kodu oluşturur, ardından MailKit ile SMTP üzerinden kullanıcıya gönderir. SMTP ayarları eksik veya gönderim başarısızsa işlem hata olarak ele alınır ve oluşturulan abone kaydı temizlenir. Başarılı akışta kod 30 gün geçerlidir.

## 📁 Görsel Yükleme

Ürün, özellik ve kampanya görsellerinde JPG, PNG ve WebP desteklenir. Dosyalar en fazla 5 MB olabilir; uzantı, MIME türü ve gerçek dosya imzası kontrol edilir. Geçerli dosyalar benzersiz adlarla `wwwroot/uploads/images` altında saklanır. Değiştirilen veya artık kullanılmayan yerel görseller temizlenir.

## 🗃️ MongoDB

Başlıca koleksiyonlar `AdminUsers`, `Categories`, `Products`, `Features`, `Discounts`, `Sales` ve `Subscribers` koleksiyonlarıdır. Uygulama başlangıcında ürün, kategori, kampanya, satış, yönetici ve abone sorguları için gerekli indeksler oluşturulur.

## 👨‍💻 Developer

İsmail Baran KARASU
GitHub: <https://github.com/ismailbarankarasu>

Tema kaynaklı görsel ve tasarım varlıkları için mevcut [TemplatesJungle](https://templatesjungle.com/) atıfları korunmuştur.
