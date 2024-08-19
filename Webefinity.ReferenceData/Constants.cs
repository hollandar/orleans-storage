namespace Webefinity.ReferenceData;

public static class Constants
{
    public static Dictionary<IsoCountryNumber, Country> countries = new Dictionary<IsoCountryNumber, Country>
        {
            { IsoCountryNumber.None, new Country { CountryName = "(none)", Iso3166_2 = "", DialingCode = "", CurrencyCode = "" } },
            { IsoCountryNumber.UnitedStates, new Country { CountryName = "United States", Iso3166_2 = "US", DialingCode = "+1", CurrencyCode = "USD" } },
            { IsoCountryNumber.Canada, new Country { CountryName = "Canada", Iso3166_2 = "CA", DialingCode = "+1", CurrencyCode = "CAD" } },
            { IsoCountryNumber.UnitedKingdom, new Country { CountryName = "United Kingdom", Iso3166_2 = "GB", DialingCode = "+44", CurrencyCode = "GBP" } },
            { IsoCountryNumber.Australia, new Country { CountryName = "Australia", Iso3166_2 = "AU", DialingCode = "+61", CurrencyCode = "AUD" } },
            { IsoCountryNumber.Germany, new Country { CountryName = "Germany", Iso3166_2 = "DE", DialingCode = "+49", CurrencyCode = "EUR" } },
            { IsoCountryNumber.France, new Country { CountryName = "France", Iso3166_2 = "FR", DialingCode = "+33", CurrencyCode = "EUR" } },
            { IsoCountryNumber.Japan, new Country { CountryName = "Japan", Iso3166_2 = "JP", DialingCode = "+81", CurrencyCode = "JPY" } },
            { IsoCountryNumber.India, new Country { CountryName = "India", Iso3166_2 = "IN", DialingCode = "+91", CurrencyCode = "INR" } },
            { IsoCountryNumber.China, new Country { CountryName = "China", Iso3166_2 = "CN", DialingCode = "+86", CurrencyCode = "CNY" } },
            { IsoCountryNumber.Brazil, new Country { CountryName = "Brazil", Iso3166_2 = "BR", DialingCode = "+55", CurrencyCode = "BRL" } },
            { IsoCountryNumber.Russia, new Country { CountryName = "Russia", Iso3166_2 = "RU", DialingCode = "+7", CurrencyCode = "RUB" } },
            { IsoCountryNumber.SouthAfrica, new Country { CountryName = "South Africa", Iso3166_2 = "ZA", DialingCode = "+27", CurrencyCode = "ZAR" } },
            { IsoCountryNumber.Mexico, new Country { CountryName = "Mexico", Iso3166_2 = "MX", DialingCode = "+52", CurrencyCode = "MXN" } },
            { IsoCountryNumber.Italy, new Country { CountryName = "Italy", Iso3166_2 = "IT", DialingCode = "+39", CurrencyCode = "EUR" } },
            { IsoCountryNumber.Spain, new Country { CountryName = "Spain", Iso3166_2 = "ES", DialingCode = "+34", CurrencyCode = "EUR" } },
            { IsoCountryNumber.SouthKorea, new Country { CountryName = "South Korea", Iso3166_2 = "KR", DialingCode = "+82", CurrencyCode = "KRW" } },
            { IsoCountryNumber.Argentina, new Country { CountryName = "Argentina", Iso3166_2 = "AR", DialingCode = "+54", CurrencyCode = "ARS" } },
            { IsoCountryNumber.SaudiArabia, new Country { CountryName = "Saudi Arabia", Iso3166_2 = "SA", DialingCode = "+966", CurrencyCode = "SAR" } },
            { IsoCountryNumber.Turkey, new Country { CountryName = "Turkey", Iso3166_2 = "TR", DialingCode = "+90", CurrencyCode = "TRY" } },
            { IsoCountryNumber.Nigeria, new Country { CountryName = "Nigeria", Iso3166_2 = "NG", DialingCode = "+234", CurrencyCode = "NGN" } },
            { IsoCountryNumber.NewZealand, new Country { CountryName = "New Zealand", Iso3166_2 = "NZ", DialingCode = "+64", CurrencyCode = "NZD" } },
            { IsoCountryNumber.PapuaNewGuinea, new Country { CountryName = "Papua New Guinea", Iso3166_2 = "PG", DialingCode = "+675", CurrencyCode = "PGK" } },
            { IsoCountryNumber.Indonesia, new Country { CountryName = "Indonesia", Iso3166_2 = "ID", DialingCode = "+62", CurrencyCode = "IDR" } },
            { IsoCountryNumber.Fiji, new Country { CountryName = "Fiji", Iso3166_2 = "FJ", DialingCode = "+679", CurrencyCode = "FJD" } },
            { IsoCountryNumber.SolomonIslands, new Country { CountryName = "Solomon Islands", Iso3166_2 = "SB", DialingCode = "+677", CurrencyCode = "SBD" } },
            { IsoCountryNumber.Vanuatu, new Country { CountryName = "Vanuatu", Iso3166_2 = "VU", DialingCode = "+678", CurrencyCode = "VUV" } }
        };
}
