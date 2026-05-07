using FluentMigrator;

namespace TrimangoCalendar.API.Migrations;

[Migration(2026050701)]
public class InitialBaselineMigration : Migration
{
    public override void Up()
    {
        Create.Table("Tenants")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Name").AsString(200).NotNullable()
            .WithColumn("Subdomain").AsString(100).NotNullable()
            .WithColumn("Email").AsString(256).NotNullable()
            .WithColumn("Phone").AsString(30).Nullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("IX_Tenants_Subdomain").OnTable("Tenants").OnColumn("Subdomain").Unique();

        Create.Table("AspNetRoles")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Name").AsString(256).Nullable()
            .WithColumn("NormalizedName").AsString(256).Nullable()
            .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable()
            .WithColumn("Description").AsString(500).Nullable();

        Create.Table("AspNetUsers")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("UserName").AsString(256).Nullable()
            .WithColumn("NormalizedUserName").AsString(256).Nullable()
            .WithColumn("Email").AsString(256).Nullable()
            .WithColumn("NormalizedEmail").AsString(256).Nullable()
            .WithColumn("EmailConfirmed").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("PasswordHash").AsString(int.MaxValue).Nullable()
            .WithColumn("SecurityStamp").AsString(int.MaxValue).Nullable()
            .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable()
            .WithColumn("PhoneNumber").AsString(int.MaxValue).Nullable()
            .WithColumn("PhoneNumberConfirmed").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("TwoFactorEnabled").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("LockoutEnd").AsDateTimeOffset().Nullable()
            .WithColumn("LockoutEnabled").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("AccessFailedCount").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("TenantId").AsGuid().Nullable()
            .WithColumn("FirstName").AsString(100).Nullable()
            .WithColumn("LastName").AsString(100).Nullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("UpdatedAt").AsDateTime2().Nullable();

        Create.ForeignKey("FK_AspNetUsers_Tenants_TenantId")
            .FromTable("AspNetUsers").ForeignColumn("TenantId")
            .ToTable("Tenants").PrimaryColumn("Id");

        Create.Table("AspNetUserClaims")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsGuid().NotNullable()
            .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
            .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable();
        Create.ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId")
            .FromTable("AspNetUserClaims").ForeignColumn("UserId")
            .ToTable("AspNetUsers").PrimaryColumn("Id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);

        Create.Table("AspNetUserLogins")
            .WithColumn("LoginProvider").AsString(128).NotNullable()
            .WithColumn("ProviderKey").AsString(128).NotNullable()
            .WithColumn("ProviderDisplayName").AsString(int.MaxValue).Nullable()
            .WithColumn("UserId").AsGuid().NotNullable();
        Create.PrimaryKey("PK_AspNetUserLogins").OnTable("AspNetUserLogins").Columns("LoginProvider", "ProviderKey");
        Create.ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId")
            .FromTable("AspNetUserLogins").ForeignColumn("UserId")
            .ToTable("AspNetUsers").PrimaryColumn("Id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);

        Create.Table("AspNetUserTokens")
            .WithColumn("UserId").AsGuid().NotNullable()
            .WithColumn("LoginProvider").AsString(128).NotNullable()
            .WithColumn("Name").AsString(128).NotNullable()
            .WithColumn("Value").AsString(int.MaxValue).Nullable();
        Create.PrimaryKey("PK_AspNetUserTokens").OnTable("AspNetUserTokens").Columns("UserId", "LoginProvider", "Name");
        Create.ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId")
            .FromTable("AspNetUserTokens").ForeignColumn("UserId")
            .ToTable("AspNetUsers").PrimaryColumn("Id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);

        Create.Table("AspNetRoleClaims")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("RoleId").AsGuid().NotNullable()
            .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
            .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable();
        Create.ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId")
            .FromTable("AspNetRoleClaims").ForeignColumn("RoleId")
            .ToTable("AspNetRoles").PrimaryColumn("Id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);

        Create.Table("AspNetUserRoles")
            .WithColumn("UserId").AsGuid().NotNullable()
            .WithColumn("RoleId").AsGuid().NotNullable();
        Create.PrimaryKey("PK_AspNetUserRoles").OnTable("AspNetUserRoles").Columns("UserId", "RoleId");
        Create.ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId")
            .FromTable("AspNetUserRoles").ForeignColumn("UserId")
            .ToTable("AspNetUsers").PrimaryColumn("Id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);
        Create.ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId")
            .FromTable("AspNetUserRoles").ForeignColumn("RoleId")
            .ToTable("AspNetRoles").PrimaryColumn("Id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);

        Create.Index("RoleNameIndex").OnTable("AspNetRoles").OnColumn("NormalizedName").Unique();
        Create.Index("UserNameIndex").OnTable("AspNetUsers").OnColumn("NormalizedUserName").Unique();
        Create.Index("EmailIndex").OnTable("AspNetUsers").OnColumn("NormalizedEmail");
        Create.Index("IX_AspNetUsers_TenantId").OnTable("AspNetUsers").OnColumn("TenantId");

        Create.Table("Properties")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("TenantId").AsGuid().NotNullable()
            .WithColumn("Name").AsString(300).NotNullable()
            .WithColumn("Slug").AsString(300).NotNullable()
            .WithColumn("City").AsString(100).Nullable()
            .WithColumn("Country").AsString(100).Nullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
        Create.ForeignKey("FK_Properties_Tenants_TenantId")
            .FromTable("Properties").ForeignColumn("TenantId")
            .ToTable("Tenants").PrimaryColumn("Id");
        Create.Index("IX_Properties_TenantId_Slug").OnTable("Properties").OnColumn("TenantId").Ascending().OnColumn("Slug").Ascending();
        Create.UniqueConstraint("UQ_Properties_TenantId_Slug").OnTable("Properties").Columns("TenantId", "Slug");

        Create.Table("Units")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("PropertyId").AsGuid().NotNullable()
            .WithColumn("Name").AsString(200).NotNullable()
            .WithColumn("BasePrice").AsDecimal(18, 2).NotNullable()
            .WithColumn("CurrencyCode").AsString(3).NotNullable()
            .WithColumn("MaxAdults").AsInt32().NotNullable().WithDefaultValue(2)
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
        Create.ForeignKey("FK_Units_Properties_PropertyId")
            .FromTable("Units").ForeignColumn("PropertyId")
            .ToTable("Properties").PrimaryColumn("Id");

        Create.Table("Guests")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("TenantId").AsGuid().NotNullable()
            .WithColumn("FirstName").AsString(100).NotNullable()
            .WithColumn("LastName").AsString(100).NotNullable()
            .WithColumn("Email").AsString(256).Nullable()
            .WithColumn("Phone").AsString(30).Nullable()
            .WithColumn("CreatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
        Create.ForeignKey("FK_Guests_Tenants_TenantId")
            .FromTable("Guests").ForeignColumn("TenantId")
            .ToTable("Tenants").PrimaryColumn("Id");

        Create.Table("Reservations")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("TenantId").AsGuid().NotNullable()
            .WithColumn("PropertyId").AsGuid().NotNullable()
            .WithColumn("UnitId").AsGuid().NotNullable()
            .WithColumn("GuestId").AsGuid().NotNullable()
            .WithColumn("CheckIn").AsDateTime2().NotNullable()
            .WithColumn("CheckOut").AsDateTime2().NotNullable()
            .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("CurrencyCode").AsString(3).NotNullable()
            .WithColumn("Status").AsString(30).NotNullable()
            .WithColumn("CreatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
        Create.ForeignKey("FK_Reservations_Tenants_TenantId")
            .FromTable("Reservations").ForeignColumn("TenantId")
            .ToTable("Tenants").PrimaryColumn("Id");
        Create.ForeignKey("FK_Reservations_Properties_PropertyId")
            .FromTable("Reservations").ForeignColumn("PropertyId")
            .ToTable("Properties").PrimaryColumn("Id");
        Create.ForeignKey("FK_Reservations_Units_UnitId")
            .FromTable("Reservations").ForeignColumn("UnitId")
            .ToTable("Units").PrimaryColumn("Id");
        Create.ForeignKey("FK_Reservations_Guests_GuestId")
            .FromTable("Reservations").ForeignColumn("GuestId")
            .ToTable("Guests").PrimaryColumn("Id");
    }

    public override void Down()
    {
        Delete.Table("Reservations");
        Delete.Table("Guests");
        Delete.Table("Units");
        Delete.Table("Properties");
        Delete.Table("AspNetUserRoles");
        Delete.Table("AspNetRoleClaims");
        Delete.Table("AspNetUserTokens");
        Delete.Table("AspNetUserLogins");
        Delete.Table("AspNetUserClaims");
        Delete.Table("AspNetUsers");
        Delete.Table("AspNetRoles");
        Delete.Table("Tenants");
    }
}
