using System;
using Microsoft.EntityFrameworkCore;
using GymBoom.Models;

namespace GymBoom.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<GymPlan> GymPlans => Set<GymPlan>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Hassasiyet (Precision) Ayarları
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<GymPlan>()
            .Property(g => g.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        // Seed Data Metodu
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var fixedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 1. Kategoriler
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = 1,
                Name = "Fitness Ekipmanları",
                Description = "Ağırlıklar, barlar ve kuvvet antrenman araçları.",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new Category
            {
                Id = 2,
                Name = "Sporcu Gıdaları",
                Description = "Protein tozları, kreatin, amino asit ve takviye edici gıdalar.",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new Category
            {
                Id = 3,
                Name = "Antrenman Aksesuarları",
                Description = "Kemer, eldiven, mat ve taşıma ürünleri.",
                IsActive = true,
                CreatedDate = fixedDate
            }
        );

        // 2. Ürünler (Ekipmanlar + Aksesuarlar + Sporcu Gıdaları)
        modelBuilder.Entity<Product>().HasData(
            // Ekipmanlar
            new Product
            {
                Id = 1,
                CategoryId = 1,
                Name = "20 kg Ayarlanabilir Döküm Dambıl Seti",
                Description = "Vidalı döküm dambıl plakaları ve taşıma çantasıyla ev ve salon kullanımına uygun.",
                Price = 1450.00m,
                Stock = 25,
                ImageUrl = "/images/products/dumbell-set.jpg",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new Product
            {
                Id = 2,
                CategoryId = 1,
                Name = "Olimpik Barbell (220 cm - 20 kg)",
                Description = "Rulmanlı, yüksek taşıma kapasiteli profesyonel çelik halter barı.",
                Price = 3200.00m,
                Stock = 12,
                ImageUrl = "/images/products/barbell.jpg",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new Product
            {
                Id = 3,
                CategoryId = 1,
                Name = "Lateks Direnç Lastiği Seti (5 Seviye)",
                Description = "Isınma, mobilite ve kuvvet antrenmanları için 5 farklı direnç seviyesi.",
                Price = 350.00m,
                Stock = 50,
                ImageUrl = "/images/products/resistance-bands.jpg",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new Product
            {
                Id = 4,
                CategoryId = 1,
                Name = "Kaymaz Taban Egzersiz & Yoga Matı (10 mm)",
                Description = "Diz ve eklemleri koruyan yüksek yoğunluklu kaydırmaz köpük mat.",
                Price = 420.00m,
                Stock = 30,
                ImageUrl = "/images/products/exercise-mat.jpg",
                IsActive = true,
                CreatedDate = fixedDate
            },

            // Aksesuarlar
            new Product
            {
                Id = 5,
                CategoryId = 3,
                Name = "Deri Powerlifting Ağırlık Kemeri",
                Description = "Ağır squat ve deadlift çalışmalarında tam bel desteği sağlayan çelik tokalı deri kemer.",
                Price = 850.00m,
                Stock = 20,
                ImageUrl = "/images/products/lifting-belt.jpg",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new Product
            {
                Id = 6,
                CategoryId = 3,
                Name = "Bilek Destekli Fitness Eldiveni",
                Description = "Avuç içi silikon kaydırmaz pedli ve ayarlanabilir bilek sarımlı eldiven.",
                Price = 280.00m,
                Stock = 45,
                ImageUrl = "/images/products/gloves.jpg",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new Product
            {
                Id = 7,
                CategoryId = 3,
                Name = "Paslanmaz Çelik Shaker (750 ml)",
                Description = "Koku tutmayan, sızdırmaz emniyet kapaklı ve top mikserli çelik shaker.",
                Price = 320.00m,
                Stock = 60,
                ImageUrl = "/images/products/steel-shaker.jpg",
                IsActive = true,
                CreatedDate = fixedDate
            },

            // Sporcu Gıdaları (Supplement)
            new Product
            {
                Id = 8,
                CategoryId = 2,
                Name = "Whey Protein Tozu (Çikolata - 2000 g)",
                Description = "%78 protein oranı, BCAA destekli, kolay çözünen izole & konsantre whey formülü.",
                Price = 1850.00m,
                Stock = 35,
                ImageUrl = "/images/products/whey-protein.jpg",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new Product
            {
                Id = 9,
                CategoryId = 2,
                Name = "Mikronize Kreatin Monohidrat (300 g)",
                Description = "%100 saf mikronize kreatin monohidrat. Kas gücü, hacim ve patlayıcı kuvvet artışı sağlar.",
                Price = 620.00m,
                Stock = 40,
                ImageUrl = "/images/products/creatine.jpg",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new Product
            {
                Id = 10,
                CategoryId = 2,
                Name = "BCAA 4:1:1 Toz Form (Yeşil Elma - 500 g)",
                Description = "Antrenman esnasında ve sonrasında kas yıkımını önlemeye ve toparlanmaya yardımcı esansiyel amino asitler.",
                Price = 740.00m,
                Stock = 28,
                ImageUrl = "/images/products/bcaa.jpg",
                IsActive = true,
                CreatedDate = fixedDate
            }
        );

        // 3. Spor Salonu Üyelik Paketleri (GymPlans)
        modelBuilder.Entity<GymPlan>().HasData(
            new GymPlan
            {
                Id = 1,
                Title = "1 Aylık Standart Üyelik",
                Description = "Kısa dönemli antrenman ve salonu denemek isteyenler için ideal başlangıç paketi.",
                Price = 4000.00m,
                DurationInMonths = 1,
                Features = "Sınırsız Fitness Alanı Kullanımı, Soyunma Odası & Duş, Ücretsiz Dolap",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new GymPlan
            {
                Id = 2,
                Title = "3 Aylık Gelişim Paketi",
                Description = "Düzenli spora başlamak ve gözle görülür sonuçlar almak isteyenler için.",
                Price = 11000.00m,
                DurationInMonths = 3,
                Features = "Fitness & Kardiyo Alanı, 1 Seans Ücretsiz Ölçüm & Program, Sauna Erişimi",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new GymPlan
            {
                Id = 3,
                Title = "6 Aylık Pro Paket",
                Description = "Yüksek motivasyon ve avantajlı fiyat sunan orta-uzun dönem üyelik paketi.",
                Price = 19000.00m,
                DurationInMonths = 6,
                Features = "Tüm Fitness Alanları, Aylık Düzenli Vücut Analizi, Sauna & Buhar Odası, Grup Dersleri İndirimi",
                IsActive = true,
                CreatedDate = fixedDate
            },
            new GymPlan
            {
                Id = 4,
                Title = "12 Aylık VIP Yıllık Üyelik",
                Description = "En ekonomik aylık maliyet, tam kapsamlı erişim ve dondurma hakkı sunan premium paket.",
                Price = 36000.00m,
                DurationInMonths = 12,
                Features = "VIP Alan Erişimi, Sınırsız Sauna & Buhar Odası, 30 Gün Üyelik Dondurma Hakkı, 2 Seans Birebir PT Desteği",
                IsActive = true,
                CreatedDate = fixedDate
            }
        );
    }
}