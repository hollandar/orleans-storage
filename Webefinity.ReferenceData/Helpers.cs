namespace Webefinity.ReferenceData;

public static class Helpers
{
    public static IEnumerable<IsoCountryNumber> Countries = Constants.countries.Keys;
    public static string GetCountryName(IsoCountryNumber countryNumber) => Constants.countries[countryNumber].CountryName;
    public static string GetCountryISO3166(IsoCountryNumber countryNumber) => Constants.countries[countryNumber].Iso3166_2;
    public static string GetCountryDialingCode(IsoCountryNumber countryNumber) => Constants.countries[countryNumber].DialingCode;
    public static string GetCountryCurrencyCode(IsoCountryNumber countryNumber) => Constants.countries[countryNumber].CurrencyCode;
    public static IEnumerable<KeyValuePair<IsoCountryNumber, string>> GetOrderedCountryNames() => Constants.countries.OrderBy(r => r.Value.CountryName).Select(x => new KeyValuePair<IsoCountryNumber, string>(x.Key, x.Value.CountryName));
    public static string ToDisplay(this IsoCountryNumber countryNumber) => GetCountryName(countryNumber);
}
