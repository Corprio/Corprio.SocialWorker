using Corprio.DataModel.Business.Sales;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Corprio.SocialWorker.Models.Meta;
using Corprio.SocialWorker.Models.Line;

namespace Corprio.SocialWorker.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) : base(option)
        {
        }
        
        public DbSet<MetaUser> MetaUsers { get; set; }
        public DbSet<MetaPage> MetaPages { get; set; }
        public DbSet<DbFriendlyBot> BotStatuses { get; set; }
        public DbSet<MetaPost> MetaPosts { get; set; }
        public DbSet<MetaMention> MetaMentions { get; set; }
        public DbSet<MetaMessageWebhook> MetaMessageWebhooks { get; set; }
        public DbSet<MetaFeedWebhook> MetaFeedWebhooks { get; set; }
        public DbSet<MetaCommentWebhook> MetaCommentWebhooks { get; set; }
        public DbSet<LineChannel> LineChannels { get; set; }

        //// https://devblogs.microsoft.com/dotnet/announcing-ef7-release-candidate-2/
        //// Note: didn't use Json column to store the bot statuses because it does not support collection of primitive types
        //public DbSet<MetaBotStatus> MetaBots { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{            
        //    modelBuilder.Entity<MetaBotStatus>().OwnsMany(
        //        bot => bot.Cart);
        //}
    }
}
