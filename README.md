GymBoom - Fitness & Supplement E-Ticaret Platformu
GymBoom, sporcuların ve fitness tutkunlarının supplement, ekipman ve spor salonu üyeliklerini yönetebilecekleri, .NET Core teknolojileri ile geliştirilmiş kapsamlı bir e-ticaret ve yönetim platformudur.

Proje, başlangıçta monolitik bir MVC yapısıyla inşa edilmiş, ardından Decoupled API-Driven (Bağımsız API Odaklı) mimariye geçiş yapılarak modern standartlara yükseltilmiştir.


🚀 Teknolojiler ve Mimari
Backend & API (Depo Katmanı)
ASP.NET Core Web API: Tüm iş mantığı ve veritabanı işlemleri bağımsız bir API servisi olarak ayrıştırılmıştır.
Entity Framework Core: ORM (Object-Relational Mapping) olarak kullanılmıştır.
PostgreSQL: Veritabanı yönetim sistemi.
Generic Repository Pattern: Veri erişim katmanında (Data Access) kod tekrarını önlemek ve esnekliği artırmak için kullanılmıştır.
JWT (JSON Web Token): API üzerinden güvenli kimlik doğrulama, token üretimi ve rol bazlı yetkilendirme (Authentication & Authorization).
BCrypt: Kullanıcı şifrelerinin (Password Hash) güvenli bir şekilde şifrelenmesi.
ILogger: Profesyonel hata takibi ve loglama mekanizması.
Frontend (Vitrin Katmanı)
ASP.NET Core MVC: Sadece sunum (Presentation) katmanı olarak görev yapar. Tüm verileri HttpClient aracılığıyla API üzerinden çeker.
Razor Views & Bootstrap: Kullanıcı dostu ve mobil uyumlu (Responsive) arayüz tasarımı.
Cookie Authentication: API'den alınan JWT token'ını istemci tarafında güvenle saklayarak oturum yönetimini sağlar.


⚙️ Temel İşlevler ve Senaryolar
🛒 Kullanıcı (Müşteri) Senaryoları
Kayıt ve Giriş: Kullanıcılar hesap oluşturabilir, JWT tabanlı sistem üzerinden güvenle giriş yapabilir.
Mağaza ve Filtreleme: Supplement ve ekipmanlar kategorilere ayrılmıştır. Ürünler arasında arama, sıralama ve kapsamlı filtreleme yapılabilir. (Geliştirme aşamasında: Sepette ürün adedi artırma/azaltma ve sipariş öncesi stok doğrulaması).
Sepet ve Sipariş: Kullanıcılar ürünleri sepete ekleyebilir, sipariş oluşturabilir ve profil sayfasından geçmiş siparişlerinin durumunu takip edebilir.
Profil Yönetimi: Aktif spor salonu üyelik planı (GymPlan) görüntülenebilir ve mevcut hesap şifresi güncellenebilir.
🛡️ Admin (Yönetici) Senaryoları
Rol Bazlı Yetki: Sadece Admin rolüne sahip kullanıcılar yönetim paneline erişebilir.
Kategori Yönetimi: Sistemdeki ürün kategorileri (Örn: Protein Tozu, Ekipman) eklenebilir, güncellenebilir veya silinebilir. (Not: Kategori işlemleri, bağımsız API entegrasyonuyla çalışmaktadır).
Ürün Yönetimi: Yeni ürünler eklenebilir, stok durumları ve fiyatlar güncellenebilir. (Geliştirme aşamasında: Resim yükleme işleminin URL yerine dosya sistemi üzerinden yapılması).
Sipariş Takibi: Gelen tüm siparişler listelenir, detayları görüntülenir ve sipariş durumları (Hazırlanıyor, Kargolandı vb.) güncellenir.
Kullanıcı Yönetimi: Sistemdeki kayıtlı üyeler listelenir, aktif/pasif durumları değiştirilebilir (Sistem yöneticisi hariç).


🛠️ Kurulum ve Veritabanı Yapılandırması
Projeyi kendi ortamınızda çalıştırmak için aşağıdaki adımları izleyin:
1. Gereksinimler
.NET 8.0 SDK (veya üzeri)
PostgreSQL veritabanı sunucusu
2. Veritabanı Bağlantısı
GymBoom.API projesinin içindeki appsettings.json dosyasını açın ve DefaultConnection kısmına kendi PostgreSQL bilgilerinizi (Username, Password, Port) girin:

"ConnectionStrings": {

  "DefaultConnection": "Host=localhost;Port=5432;Database=GymBoomDB;Username=SİZİN_KULLANICI_ADINIZ;Password=SİZİN_ŞİFRENİZ"

}
3. Migration ve Veritabanı Oluşturma
Proje dizininde (GymBoom.API) bir terminal (Console) açın ve Entity Framework Core üzerinden veritabanını oluşturmak için aşağıdaki komutu çalıştırın:

dotnet ef database update

Bu komut, PostgreSQL üzerinde tabloları oluşturacak ve varsa başlangıç (Seed) verilerini yükleyecektir.
4. Projeyi Çalıştırma
Sistemin doğru çalışması için hem API hem de MVC projelerinin aynı anda çalışması gerekmektedir.

Önce GymBoom.API dizininde terminali açıp dotnet run komutunu çalıştırın.
Ardından GymBoom.MVC dizininde terminali açıp dotnet run komutunu çalıştırın.


🔑 Varsayılan Admin Hesabı
Sistemi test etmek ve yönetim paneline erişmek için aşağıdaki varsayılan yönetici bilgilerini kullanabilirsiniz:

Email: admin@gymboom.com (Örnektir, eğer Seed datanızda farklıysa burayı güncelleyin)
Şifre: Admin123! (Örnektir, eğer Seed datanızda farklıysa burayı güncelleyin)


🔒 Güvenlik Standartları
Bu proje geliştirilirken güvenlik standartları ön planda tutulmuştur:

Form işlemlerinde Anti-forgery kontrolleri ([ValidateAntiForgeryToken]) uygulanmıştır.
Veri transferlerinde (Giriş/Kayıt vb.) DTO (Data Transfer Object) sınıfları kullanılarak "Over-Posting" saldırılarına karşı önlem alınmıştır.
Entity validasyonları (veri doğrulama kuralları) uygulanmıştır.
Önemli sistem hataları dışarıya (Client'a) sızdırılmaz, ILogger aracılığıyla sunucu konsoluna kaydedilir (Error Handling).



Geliştirici: Enes Altaş
