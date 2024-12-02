﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Webefinity.Module.Messaging.Data;

#nullable disable

namespace Webefinity.Module.Messaging.Data.Migrations
{
    [DbContext(typeof(MessagingDbContext))]
    partial class MessagingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Webefinity.Module.Messaging.Data.Address", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("MessageId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MessageId");

                    b.ToTable("Addresses", (string)null);
                });

            modelBuilder.Entity("Webefinity.Module.Messaging.Data.Attachment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AttachmentId")
                        .HasColumnType("uuid");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<long>("Length")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AttachmentId");

                    b.ToTable("Attachments", (string)null);
                });

            modelBuilder.Entity("Webefinity.Module.Messaging.Data.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Error")
                        .HasColumnType("text");

                    b.Property<int>("Format")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("PurgeAfter")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SenderId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("Sent")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Subject")
                        .HasColumnType("text");

                    b.Property<int>("Target")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PurgeAfter");

                    b.HasIndex("SenderId", "Status");

                    b.ToTable("Messages", (string)null);
                });

            modelBuilder.Entity("Webefinity.Module.Messaging.Data.Address", b =>
                {
                    b.HasOne("Webefinity.Module.Messaging.Data.Message", "Message")
                        .WithMany("Addresses")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Message");
                });

            modelBuilder.Entity("Webefinity.Module.Messaging.Data.Attachment", b =>
                {
                    b.HasOne("Webefinity.Module.Messaging.Data.Message", "Message")
                        .WithMany("Attachments")
                        .HasForeignKey("AttachmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Message");
                });

            modelBuilder.Entity("Webefinity.Module.Messaging.Data.Message", b =>
                {
                    b.Navigation("Addresses");

                    b.Navigation("Attachments");
                });
#pragma warning restore 612, 618
        }
    }
}