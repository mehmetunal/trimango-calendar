using FluentMigrator;

namespace TrimangoCalendar.API.Migrations;

[Migration(2026050702)]
public class AddCurrencyTablesMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Currencies").Exists())
        {
            Create.Table("Currencies")
                .WithColumn("Code").AsString(3).PrimaryKey()
                .WithColumn("Symbol").AsString(5).NotNullable()
                .WithColumn("Name").AsString(50).NotNullable()
                .WithColumn("DecimalPlaces").AsInt32().NotNullable().WithDefaultValue(2)
                .WithColumn("CultureCode").AsString(10).NotNullable()
                .WithColumn("IsBaseCurrency").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
        }
        else
        {
            if (!Schema.Table("Currencies").Column("CultureCode").Exists())
            {
                Alter.Table("Currencies")
                    .AddColumn("CultureCode").AsString(10).Nullable();

                Execute.Sql("UPDATE [Currencies] SET [CultureCode] = 'tr-TR' WHERE [CultureCode] IS NULL");

                Alter.Column("CultureCode")
                    .OnTable("Currencies")
                    .AsString(10)
                    .NotNullable();
            }
        }

        if (!Schema.Table("ExchangeRates").Exists())
        {
            Create.Table("ExchangeRates")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("BaseCurrencyCode").AsString(3).NotNullable()
                .WithColumn("TargetCurrencyCode").AsString(3).NotNullable()
                .WithColumn("Rate").AsDecimal(18, 6).NotNullable()
                .WithColumn("BuyRate").AsDecimal(18, 6).NotNullable()
                .WithColumn("SellRate").AsDecimal(18, 6).NotNullable()
                .WithColumn("Date").AsDateTime2().NotNullable()
                .WithColumn("Source").AsString(50).NotNullable().WithDefaultValue("TCMB")
                .WithColumn("UpdatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.ForeignKey("FK_ExchangeRates_Currencies_BaseCurrencyCode")
                .FromTable("ExchangeRates").ForeignColumn("BaseCurrencyCode")
                .ToTable("Currencies").PrimaryColumn("Code");

            Create.ForeignKey("FK_ExchangeRates_Currencies_TargetCurrencyCode")
                .FromTable("ExchangeRates").ForeignColumn("TargetCurrencyCode")
                .ToTable("Currencies").PrimaryColumn("Code");

            Create.Index("IX_ExchangeRates_BaseCurrencyCode_TargetCurrencyCode_Date")
                .OnTable("ExchangeRates")
                .OnColumn("BaseCurrencyCode").Ascending()
                .OnColumn("TargetCurrencyCode").Ascending()
                .OnColumn("Date").Ascending()
                .WithOptions().Unique();
        }
    }

    public override void Down()
    {
        if (Schema.Table("ExchangeRates").Exists())
        {
            Delete.Table("ExchangeRates");
        }

        if (Schema.Table("Currencies").Exists())
        {
            Delete.Table("Currencies");
        }
    }
}
