using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymBoom.Migrations
{
    /// <inheritdoc />
    public partial class SeedSporMalzemeleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "OrderItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "GymPlans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ağırlıklar, barlar ve kuvvet antrenman araçları.", true, "Fitness Ekipmanları" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Protein tozları, kreatin, amino asit ve takviye edici gıdalar.", true, "Sporcu Gıdaları" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kemer, eldiven, mat ve taşıma ürünleri.", true, "Antrenman Aksesuarları" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedDate", "Description", "ImageUrl", "IsActive", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vidalı döküm dambıl plakaları ve taşıma çantasıyla ev ve salon kullanımına uygun.", "/images/products/dumbell-set.jpg", true, "20 kg Ayarlanabilir Döküm Dambıl Seti", 1450.00m, 25 },
                    { 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rulmanlı, yüksek taşıma kapasiteli profesyonel çelik halter barı.", "/images/products/barbell.jpg", true, "Olimpik Barbell (220 cm - 20 kg)", 3200.00m, 12 },
                    { 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Isınma, mobilite ve kuvvet antrenmanları için 5 farklı direnç seviyesi.", "/images/products/resistance-bands.jpg", true, "Lateks Direnç Lastiği Seti (5 Seviye)", 350.00m, 50 },
                    { 4, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Diz ve eklemleri koruyan yüksek yoğunluklu kaydırmaz köpük mat.", "/images/products/exercise-mat.jpg", true, "Kaymaz Taban Egzersiz & Yoga Matı (10 mm)", 420.00m, 30 },
                    { 5, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ağır squat ve deadlift çalışmalarında tam bel desteği sağlayan çelik tokalı deri kemer.", "/images/products/lifting-belt.jpg", true, "Deri Powerlifting Ağırlık Kemeri", 850.00m, 20 },
                    { 6, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Avuç içi silikon kaydırmaz pedli ve ayarlanabilir bilek sarımlı eldiven.", "/images/products/gloves.jpg", true, "Bilek Destekli Fitness Eldiveni", 280.00m, 45 },
                    { 7, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Koku tutmayan, sızdırmaz emniyet kapaklı ve top mikserli çelik shaker.", "/images/products/steel-shaker.jpg", true, "Paslanmaz Çelik Shaker (750 ml)", 320.00m, 60 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "OrderItems",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "GymPlans",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
