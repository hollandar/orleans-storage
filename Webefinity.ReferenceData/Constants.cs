namespace Webefinity.ReferenceData;

public static class Constants
{
    public static Dictionary<IsoCountryNumber, Country> countries = new Dictionary<IsoCountryNumber, Country>
        {
            { IsoCountryNumber.None, new Country ("(none)","","","") },
            { IsoCountryNumber.UnitedStates, new Country ("United States","US","+1", "USD" ) },
            { IsoCountryNumber.Canada, new Country ("Canada","CA","+1", "CAD" ) },
            { IsoCountryNumber.UnitedKingdom, new Country("United Kingdom","GB","+44", "GBP" ) },
            { IsoCountryNumber.Australia, new Country ("Australia","AU","+61", "AUD" ) },
            { IsoCountryNumber.Germany, new Country ("Germany","DE","+49", "EUR" ) },
            { IsoCountryNumber.France, new Country ("France","FR","+33", "EUR" ) },
            { IsoCountryNumber.Japan, new Country ("Japan","JP","+81", "JPY" ) },
            { IsoCountryNumber.India, new Country ("India","IN","+91", "INR" ) },
            { IsoCountryNumber.China, new Country ("China","CN","+86", "CNY" ) },
            { IsoCountryNumber.Brazil, new Country ("Brazil","BR","+55", "BRL" ) },
            { IsoCountryNumber.Russia, new Country ("Russia","RU","+7", "RUB" ) },
            { IsoCountryNumber.SouthAfrica, new Country ("South Africa","ZA","+27", "ZAR" ) },
            { IsoCountryNumber.Mexico, new Country ("Mexico","MX","+52", "MXN" ) },
            { IsoCountryNumber.Italy, new Country ("Italy","IT","+39", "EUR" ) },
            { IsoCountryNumber.Spain, new Country ("Spain","ES","+34", "EUR" ) },
            { IsoCountryNumber.SouthKorea, new Country ("South Korea","KR","+82", "KRW" ) },
            { IsoCountryNumber.Argentina, new Country ("Argentina","AR","+54", "ARS" ) },
            { IsoCountryNumber.SaudiArabia, new Country ("Saudi Arabia","SA","+966", "SAR" ) },
            { IsoCountryNumber.Turkey, new Country ("Turkey","TR","+90", "TRY" ) },
            { IsoCountryNumber.Nigeria, new Country ("Nigeria","NG","+234", "NGN" ) },
            { IsoCountryNumber.NewZealand, new Country ("New Zealand","NZ","+64", "NZD" ) },
            { IsoCountryNumber.PapuaNewGuinea, new Country ("Papua New Guinea","PG","+675", "PGK" ) },
            { IsoCountryNumber.Indonesia, new Country ("Indonesia","ID","+62", "IDR" ) },
            { IsoCountryNumber.Fiji, new Country ("Fiji","FJ","+679", "FJD" ) },
            { IsoCountryNumber.SolomonIslands, new Country ("Solomon Islands","SB","+677", "SBD" ) },
            { IsoCountryNumber.Vanuatu, new Country ("Vanuatu","VU","+678", "VUV" ) }
        };
}
