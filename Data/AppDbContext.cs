using BackendAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public override int SaveChanges()
    {
        HandleSoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void HandleSoftDelete()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ISoftDelete && e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            ((ISoftDelete)entry.Entity).IsDeleted = true;
        }
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<ViolationRecord> ViolationRecords => Set<ViolationRecord>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RoomTransferRequest> RoomTransferRequests => Set<RoomTransferRequest>();
    public DbSet<SemesterPeriods> SemesterPeriods => Set<SemesterPeriods>();
    public DbSet<RenewalPackages> RenewalPackages => Set<RenewalPackages>();
    public DbSet<RenewalRequest> RenewalRequests => Set<RenewalRequest>();
    public DbSet<ElectricWaterReading> ElectricWaterReadings => Set<ElectricWaterReading>();
    public DbSet<Relative> Relatives => Set<Relative>();
    public DbSet<StudentRequest> StudentRequests => Set<StudentRequest>();
    public DbSet<Facility> Facilities => Set<Facility>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ================= SOFT DELETE =================
        modelBuilder.Entity<Student>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Room>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<Facility>().HasQueryFilter(f => !f.IsDeleted);
        modelBuilder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);

        // ================= USER - STUDENT =================
        modelBuilder.Entity<Student>()
            .HasOne(s => s.User)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ================= STUDENT - RELATIVE =================
        modelBuilder.Entity<Relative>()
            .HasOne(r => r.Student)
            .WithMany(s => s.Relatives)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // ================= BUILDING - ROOM =================
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Building)
            .WithMany(b => b.Rooms)
            .HasForeignKey(r => r.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================= REGISTRATION =================
        modelBuilder.Entity<Registration>()
            .HasOne(r => r.Student)
            .WithMany(s => s.Registrations)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Registration>()
            .HasOne(r => r.Room)
            .WithMany()
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================= CONTRACT =================
        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Student)
            .WithMany(s => s.Contracts)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict); // ⚠️ FIX

        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Room)
            .WithMany()
            .HasForeignKey(c => c.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================= INVOICE =================
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Student)
            .WithMany(s => s.Invoices)
            .HasForeignKey(i => i.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Room)
            .WithMany()
            .HasForeignKey(i => i.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================= VIOLATION =================
        modelBuilder.Entity<ViolationRecord>()
            .HasOne(v => v.Student)
            .WithMany(s => s.ViolationRecords)
            .HasForeignKey(v => v.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // ================= ROOM TRANSFER =================
        modelBuilder.Entity<RoomTransferRequest>()
            .HasOne(r => r.Student)
            .WithMany(s => s.RoomTransferRequests)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoomTransferRequest>()
            .HasOne(r => r.FromRoom)
            .WithMany(r => r.TransferRequestsFrom)
            .HasForeignKey(r => r.FromRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoomTransferRequest>()
            .HasOne(r => r.ToRoom)
            .WithMany(r => r.TransferRequestsTo)
            .HasForeignKey(r => r.ToRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================= ELECTRIC WATER =================
        modelBuilder.Entity<ElectricWaterReading>()
            .HasOne(e => e.Room)
            .WithMany(r => r.ElectricWaterReadings)
            .HasForeignKey(e => e.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================= RENEWAL REQUEST (🔥 FIX CHÍNH) =================
        modelBuilder.Entity<RenewalRequest>()
            .HasOne(r => r.Student)
            .WithMany()
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict); // ⚠️ FIX

        modelBuilder.Entity<RenewalRequest>()
            .HasOne(r => r.Contract)
            .WithMany()
            .HasForeignKey(r => r.ContractId)
            .OnDelete(DeleteBehavior.Restrict); // ⚠️ FIX

        modelBuilder.Entity<RenewalRequest>()
            .HasOne(r => r.RenewalPackage)
            .WithMany()
            .HasForeignKey(r => r.RenewalPackageId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================= STUDENT REQUEST =================
        modelBuilder.Entity<StudentRequest>()
            .HasOne(r => r.Student)
            .WithMany()
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================= NOTIFICATION =================
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ================= UNIQUE =================
        modelBuilder.Entity<User>().HasIndex(u => u.CitizenId).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(s => s.CitizenId).IsUnique();
        modelBuilder.Entity<Room>().HasIndex(r => r.RoomCode).IsUnique();
        modelBuilder.Entity<Contract>().HasIndex(c => c.ContractCode).IsUnique();
        modelBuilder.Entity<Registration>().HasIndex(r => r.RegistrationCode).IsUnique();

        // ================= DECIMAL =================
        modelBuilder.Entity<Contract>().Property(c => c.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(i => i.RoomFee).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(i => i.ElectricFee).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(i => i.WaterFee).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(i => i.TotalAmount).HasPrecision(18, 2);

        modelBuilder.Entity<ElectricWaterReading>().Property(e => e.OldElectric).HasPrecision(18, 2);
        modelBuilder.Entity<ElectricWaterReading>().Property(e => e.NewElectric).HasPrecision(18, 2);
        modelBuilder.Entity<ElectricWaterReading>().Property(e => e.OldWater).HasPrecision(18, 2);
        modelBuilder.Entity<ElectricWaterReading>().Property(e => e.NewWater).HasPrecision(18, 2);

        // ================= ROOM TRANSFER REQUEST =================
        modelBuilder.Entity<RoomTransferRequest>()
            .HasOne(r => r.Semester)
            .WithMany()
            .HasForeignKey(r => r.SemesterId);
    }
}