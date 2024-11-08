﻿using Microsoft.EntityFrameworkCore;
using Timer = TimerService.DataModels.Timer;

namespace TimerService.Data
{
    public class TimerDbContext : DbContext
    {
        public TimerDbContext(DbContextOptions<TimerDbContext> options) : base(options) { }
        public DbSet<Timer> Timers { get; set; }
    }
}