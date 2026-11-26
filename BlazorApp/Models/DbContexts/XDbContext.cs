using Entities;
using Microsoft.EntityFrameworkCore;


namespace DbContexts;

public class XDbContext(DbContextOptions<XDbContext> options) : DbContext(options)
{

    // DbSet özellikleri
    public DbSet<Department> Departments { get; set; }
    public DbSet<Student> Students { get; set; }


    // Model oluşturma metodu
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Temel model ayarları
        base.OnModelCreating(modelBuilder);

        // Tablolar ve şema
        modelBuilder.Entity<Department>().ToTable("Department", "School");
        modelBuilder.Entity<Student>().ToTable("Student", "School");

        // Bölüm ilişkisi
        modelBuilder.Entity<Student>()
            .HasOne(student => student.Department)
            .WithMany(department => department.Students)
            .HasForeignKey(student => student.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Student_Department");

        // Öğrenci sütun ayarları
        modelBuilder.Entity<Student>()
            .Property(student => student.FirstName)
            .HasMaxLength(50);

        modelBuilder.Entity<Student>()
            .Property(student => student.LastName)
            .HasMaxLength(50);

        modelBuilder.Entity<Student>()
            .Property(student => student.Gender)
            .HasDefaultValue(0);

        // Öğrenci ad indeksi
        modelBuilder.Entity<Student>()
            .HasIndex(student => new { student.FirstName, student.LastName })
            .HasDatabaseName("IX_Student_FirstNameLastName");
    }
}
