using Microsoft.EntityFrameworkCore;
using MqttSubscriber.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttSubscriber.repository
{
    internal class Repository : DbContext
    {
        public DbSet<MessageMqtt> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseSqlite("Data Source=C:\\work_space\\c#\\2.mqtt_dispatcher\\MqttLibrary\\MqttLibrary\\Mqtt.db;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageMqtt>().ToTable("Messages");

        }


        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);

        //    // connect to mysql with connection string from app settings
        //    var connectionString = "server=localhost; port=3307; database=repository; user=root; password=flaminio; Persist Security Info=False; Connect Timeout=300"; //Configuration.GetConnectionString("ConnectionStringDatabase");
        //    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        //    // var connectionString = "server=localhost; port=3307; database=repository; user=root; password=flaminio; Persist Security Info=False; Connect Timeout=300";
        //}


    }
}
