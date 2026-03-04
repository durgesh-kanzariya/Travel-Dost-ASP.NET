using Microsoft.EntityFrameworkCore;
using travel_dost_asp.net.Models;

namespace travel_dost_asp.net.Data
{
    public class TravelDostContext : DbContext
    {
        public TravelDostContext(DbContextOptions<TravelDostContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<TripDestination> TripDestinations { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<CustomChecklist> CustomChecklists { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<CountryGuide> CountryGuides { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<UserProfile>().ToTable("user_profiles").HasKey(up => up.UserId);
            modelBuilder.Entity<Role>().ToTable("roles");
            modelBuilder.Entity<UserRole>().ToTable("user_roles").HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<Trip>().ToTable("trips");
            modelBuilder.Entity<Destination>().ToTable("destinations");
            modelBuilder.Entity<TripDestination>().ToTable("trip_destinations").HasKey(td => new { td.TripId, td.DestinationId });
            modelBuilder.Entity<Expense>().ToTable("expenses");
            modelBuilder.Entity<CustomChecklist>().ToTable("custom_checklists");
            modelBuilder.Entity<ChecklistItem>().ToTable("checklist_items");
            modelBuilder.Entity<CountryGuide>().ToTable("country_guides");

            // Mapping property names to snake_case column names
            modelBuilder.Entity<User>(entity => {
                entity.Property(e => e.FirstName).HasColumnName("first_name");
                entity.Property(e => e.LastName).HasColumnName("last_name");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<UserProfile>(entity => {
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.NativeLanguage).HasColumnName("native_language");
                entity.Property(e => e.DefaultCurrency).HasColumnName("default_currency");
            });

            modelBuilder.Entity<Role>(entity => {
                entity.Property(e => e.RoleName).HasColumnName("role_name");
            });

            modelBuilder.Entity<UserRole>(entity => {
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.AssignedAt).HasColumnName("assigned_at");
            });

            modelBuilder.Entity<Trip>(entity => {
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.StartDate).HasColumnName("start_date");
                entity.Property(e => e.EndDate).HasColumnName("end_date");
            });

            modelBuilder.Entity<Destination>(entity => {
                entity.Property(e => e.LocationName).HasColumnName("location_name");
                entity.Property(e => e.CountryName).HasColumnName("country_name");
                entity.Property(e => e.LocationType).HasColumnName("location_type");
            });

            modelBuilder.Entity<TripDestination>(entity => {
                entity.Property(e => e.TripId).HasColumnName("trip_id");
                entity.Property(e => e.DestinationId).HasColumnName("destination_id");
                entity.Property(e => e.VisitOrder).HasColumnName("visit_order");
            });

            modelBuilder.Entity<Expense>(entity => {
                entity.Property(e => e.TripId).HasColumnName("trip_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ExpenseDate).HasColumnName("expense_date");
            });

            modelBuilder.Entity<CustomChecklist>(entity => {
                entity.Property(e => e.TripId).HasColumnName("trip_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<ChecklistItem>(entity => {
                entity.Property(e => e.ChecklistId).HasColumnName("checklist_id");
                entity.Property(e => e.ItemName).HasColumnName("item_name");
                entity.Property(e => e.IsChecked).HasColumnName("is_checked");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<CountryGuide>(entity => {
                entity.Property(e => e.CountryName).HasColumnName("country_name");
                entity.Property(e => e.PoliceNumber).HasColumnName("police_number");
                entity.Property(e => e.AmbulanceNumber).HasColumnName("ambulance_number");
                entity.Property(e => e.FireNumber).HasColumnName("fire_number");
                entity.Property(e => e.EmbassyNumber).HasColumnName("embassy_number");
                entity.Property(e => e.LocalRules).HasColumnName("local_rules");
            });
        }
    }
}
