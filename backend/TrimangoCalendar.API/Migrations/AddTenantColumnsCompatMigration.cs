using FluentMigrator;

namespace TrimangoCalendar.API.Migrations;

[Migration(2026050703)]
public class AddTenantColumnsCompatMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Tenants").Exists())
        {
            return;
        }

        if (!Schema.Table("Tenants").Column("Address").Exists())
            Alter.Table("Tenants").AddColumn("Address").AsString(500).Nullable();
        if (!Schema.Table("Tenants").Column("City").Exists())
            Alter.Table("Tenants").AddColumn("City").AsString(100).Nullable();
        if (!Schema.Table("Tenants").Column("Country").Exists())
            Alter.Table("Tenants").AddColumn("Country").AsString(100).NotNullable().WithDefaultValue("Türkiye");
        if (!Schema.Table("Tenants").Column("TaxNumber").Exists())
            Alter.Table("Tenants").AddColumn("TaxNumber").AsString(20).Nullable();
        if (!Schema.Table("Tenants").Column("TaxOffice").Exists())
            Alter.Table("Tenants").AddColumn("TaxOffice").AsString(100).Nullable();
        if (!Schema.Table("Tenants").Column("Plan").Exists())
            Alter.Table("Tenants").AddColumn("Plan").AsString(20).NotNullable().WithDefaultValue("Free");
        if (!Schema.Table("Tenants").Column("PlanStartDate").Exists())
            Alter.Table("Tenants").AddColumn("PlanStartDate").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
        if (!Schema.Table("Tenants").Column("PlanEndDate").Exists())
            Alter.Table("Tenants").AddColumn("PlanEndDate").AsDateTime2().Nullable();
        if (!Schema.Table("Tenants").Column("MaxProperties").Exists())
            Alter.Table("Tenants").AddColumn("MaxProperties").AsInt32().NotNullable().WithDefaultValue(5);
        if (!Schema.Table("Tenants").Column("UpdatedAt").Exists())
            Alter.Table("Tenants").AddColumn("UpdatedAt").AsDateTime2().Nullable();
    }

    public override void Down()
    {
    }
}
