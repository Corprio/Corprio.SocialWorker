﻿// <auto-generated />
using System;
using Corprio.SocialWorker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Corprio.SocialWorker.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240116095405_AddMetaBotStatusesTableToDb")]
    partial class AddMetaBotStatusesTableToDb
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.14");

            modelBuilder.Entity("Corprio.SocialWorker.Models.DbFriendlyBot", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ApplicationID")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("AttributeValueMemoryString")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("BuyerCorprioID")
                        .HasColumnType("TEXT");

                    b.Property<string>("BuyerEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("BuyerID")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("CartString")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FacebookUserID")
                        .HasColumnType("TEXT");

                    b.Property<int>("Language")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("LastUpdateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastUpdatedBy")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("OTP_Code")
                        .HasMaxLength(6)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("OTP_ExpiryTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProductMemoryString")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<int>("ThinkingOf")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VariationMemoryString")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("FacebookUserID");

                    b.ToTable("MetaBotStatuses");
                });

            modelBuilder.Entity("Corprio.SocialWorker.Models.MetaPage", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ApplicationID")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FacebookUserID")
                        .HasColumnType("TEXT");

                    b.Property<string>("InstagramID")
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("LastUpdateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastUpdatedBy")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<string>("PageId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("FacebookUserID");

                    b.ToTable("MetaPages");
                });

            modelBuilder.Entity("Corprio.SocialWorker.Models.MetaPost", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ApplicationID")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FacebookPageID")
                        .HasColumnType("TEXT");

                    b.Property<string>("KeywordForShoppingIntention")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("LastUpdateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastUpdatedBy")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("PostId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("PostedWith")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.HasKey("ID");

                    b.HasIndex("FacebookPageID");

                    b.ToTable("MetaPosts");
                });

            modelBuilder.Entity("Corprio.SocialWorker.Models.MetaUser", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ApplicationID")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<bool>("Dormant")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FacebookUserID")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("LastUpdateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastUpdatedBy")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("OrganizationID")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("MetaUsers");
                });

            modelBuilder.Entity("Corprio.SocialWorker.Models.DbFriendlyBot", b =>
                {
                    b.HasOne("Corprio.SocialWorker.Models.MetaUser", "FacebookUser")
                        .WithMany("Bots")
                        .HasForeignKey("FacebookUserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FacebookUser");
                });

            modelBuilder.Entity("Corprio.SocialWorker.Models.MetaPage", b =>
                {
                    b.HasOne("Corprio.SocialWorker.Models.MetaUser", "FacebookUser")
                        .WithMany("Pages")
                        .HasForeignKey("FacebookUserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FacebookUser");
                });

            modelBuilder.Entity("Corprio.SocialWorker.Models.MetaPost", b =>
                {
                    b.HasOne("Corprio.SocialWorker.Models.MetaPage", "FacebookPage")
                        .WithMany("Posts")
                        .HasForeignKey("FacebookPageID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FacebookPage");
                });

            modelBuilder.Entity("Corprio.SocialWorker.Models.MetaPage", b =>
                {
                    b.Navigation("Posts");
                });

            modelBuilder.Entity("Corprio.SocialWorker.Models.MetaUser", b =>
                {
                    b.Navigation("Bots");

                    b.Navigation("Pages");
                });
#pragma warning restore 612, 618
        }
    }
}