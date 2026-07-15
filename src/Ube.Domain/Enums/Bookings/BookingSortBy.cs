namespace Ube.Domain.Enums.Bookings;

public enum BookingSortBy
{
    Newest =1,//latest booking first
    Oldest =2, //earliest booking first
    StartDateAsc =3, //near upcoming booking first
    StartDateDesc =4, //far upcoming booking first
}