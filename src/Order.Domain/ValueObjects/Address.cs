namespace Order.Domain.ValueObjects;

public sealed class Address : IEquatable<Address>
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    private Address(string street, string city, string state, string zipCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    public static Address Create(string street, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required.", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State is required.", nameof(state));

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("Zip code is required.", nameof(zipCode));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required.", nameof(country));

        return new Address(street.Trim(), city.Trim(), state.Trim(), zipCode.Trim(), country.Trim());
    }

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        return Street == other.Street &&
               City == other.City &&
               State == other.State &&
               ZipCode == other.ZipCode &&
               Country == other.Country;
    }

    public override bool Equals(object? obj) => Equals(obj as Address);

    public override int GetHashCode() => HashCode.Combine(Street, City, State, ZipCode, Country);

    public static bool operator ==(Address? left, Address? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Address? left, Address? right) => !(left == right);

    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
