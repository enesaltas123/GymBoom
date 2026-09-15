using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymBoom.Migrations
{
    /// <inheritdoc />
    public partial class SeedSupplementsAndGymPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "GymPlans",
                columns: new[] { "Id", "CreatedDate", "Description", "DurationInMonths", "Features", "IsActive", "Price", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kısa dönemli antrenman ve salonu denemek isteyenler için ideal başlangıç paketi.", 1, "Sınırsız Fitness Alanı Kullanımı, Soyunma Odası & Duş, Ücretsiz Dolap", true, 4000.00m, "1 Aylık Standart Üyelik" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Düzenli spora başlamak ve gözle görülür sonuçlar almak isteyenler için.", 3, "Fitness & Kardiyo Alanı, 1 Seans Ücretsiz Ölçüm & Program, Sauna Erişimi", true, 11000.00m, "3 Aylık Gelişim Paketi" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Yüksek motivasyon ve avantajlı fiyat sunan orta-uzun dönem üyelik paketi.", 6, "Tüm Fitness Alanları, Aylık Düzenli Vücut Analizi, Sauna & Buhar Odası, Grup Dersleri İndirimi", true, 19000.00m, "6 Aylık Pro Paket" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "En ekonomik aylık maliyet, tam kapsamlı erişim ve dondurma hakkı sunan premium paket.", 12, "VIP Alan Erişimi, Sınırsız Sauna & Buhar Odası, 30 Gün Üyelik Dondurma Hakkı, 2 Seans Birebir PT Desteği", true, 36000.00m, "12 Aylık VIP Yıllık Üyelik" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedDate", "Description", "ImageUrl", "IsActive", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 8, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%78 protein oranı, BCAA destekli, kolay çözünen izole & konsantre whey formülü.", "/images/products/whey-protein.jpg", true, "Whey Protein Tozu (Çikolata - 2000 g)", 1850.00m, 35 },
                    { 9, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "%100 saf mikronize kreatin monohidrat. Kas gücü, hacim ve patlayıcı kuvvet artışı sağlar.", "/images/products/creatine.jpg", true, "Mikronize Kreatin Monohidrat (300 g)", 620.00m, 40 },
                    { 10, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Antrenman esnasında ve sonrasında kas yıkımını önlemeye ve toparlanmaya yardımcı esansiyel amino asitler.", "/images/products/bcaa.jpg", true, "BCAA 4:1:1 Toz Form (Yeşil Elma - 500 g)", 740.00m, 28 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GymPlans",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "GymPlans",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "GymPlans",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "GymPlans",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
