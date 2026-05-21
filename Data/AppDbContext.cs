using Microsoft.EntityFrameworkCore;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>(); 

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Category> Categories { get; set; }
  
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

   
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    protected override void OnModelCreating(ModelBuilder b)
    {
         // ROLE IDS
    var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var userRoleId  = Guid.Parse("22222222-2222-2222-2222-222222222222");

    // PERMISSION IDS
    var createId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    var viewId   = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    var updateId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    var deleteId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    b.Entity<RefreshToken>().HasOne(r => r.User).WithMany(x => x.RefreshTokens).HasForeignKey(r =>r.UserId);
    b.Entity<PasswordResetToken>().HasOne(p => p.User).WithMany(x => x.PasswordResetTokens).HasForeignKey(p => p.UserId);
    b.Entity<Role>().HasData(
        new Role { Id = adminRoleId, Name = "Admin" },
        new Role { Id = userRoleId, Name = "User" }
    );
   
    b.Entity<Permission>().HasData(
        new Permission { Id = createId, Code = "USER_CREATE" },
        new Permission { Id = viewId, Code = "USER_VIEW" },
        new Permission { Id = updateId, Code = "USER_UPDATE" },
        new Permission { Id = deleteId, Code = "USER_DELETE" }
    );

    b.Entity<RolePermission>().HasData(
        new RolePermission { RoleId = adminRoleId, PermissionId = createId },
        new RolePermission { RoleId = adminRoleId, PermissionId = viewId },
        new RolePermission { RoleId = adminRoleId, PermissionId = updateId },
        new RolePermission { RoleId = adminRoleId, PermissionId = deleteId },
        new RolePermission { RoleId = userRoleId, PermissionId = viewId }
    );
        b.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        b.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });

        b.Entity<EmailVerificationToken>(entity =>
    {
        entity.HasKey(x => x.Id);

        entity.HasOne(x => x.User)
              .WithMany(u => u.EmailVerificationTokens)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.Property(x => x.Token)
              .IsRequired()
              .HasMaxLength(512);

        entity.HasIndex(x => x.Token)
              .IsUnique();
    });
     b.Entity<Product>()
        .HasMany(x => x.Images)
        .WithOne(x => x.Product)
        .HasForeignKey(x => x.ProductId);
     // ===== Cart =====
        b.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);

            // 1 User chỉ có 1 Cart
            entity.HasIndex(c => c.UserId).IsUnique();
        });

        // ===== CartItem =====
        b.Entity<CartItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);

            entity.Property(ci => ci.PriceSnapshot)
                .HasColumnType("decimal(18,2)");

            // 🔗 Cart - CartItem (1 - N)
            entity.HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔗 Product - CartItem (1 - N)
            entity.HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ❗ tránh duplicate product trong 1 cart
            entity.HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();
        });
           b.Entity<Order>()
            .HasKey(x => x.Id);

        b.Entity<Order>()
            .Property(x => x.TotalAmount)
            .HasColumnType("decimal(18,2)");

        b.Entity<Order>()
            .Property(x => x.Status)
            .HasMaxLength(50);

        // ===== ORDER ITEM =====
        b.Entity<OrderItem>()
            .HasKey(x => x.Id);

        b.Entity<OrderItem>()
            .Property(x => x.PriceSnapshot)
            .HasColumnType("decimal(18,2)");

        b.Entity<OrderItem>()
            .Property(x => x.ProductNameSnapshot)
            .HasMaxLength(255);

        b.Entity<OrderItem>()
            .Property(x => x.ImageUrlSnapshot)
            .HasMaxLength(500);

        b.Entity<OrderItem>()
            .HasOne(x => x.Order)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}