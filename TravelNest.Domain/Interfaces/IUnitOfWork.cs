using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Entities;

namespace TravelNest.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Property> Properties { get; }
        IGenericRepository<PropertyMedia> PropertyMedia { get; }
        IGenericRepository<PropertyAmenity> PropertyAmenities { get; }
        IGenericRepository<PropertyAvailability> PropertyAvailabilities { get; }
        IGenericRepository<Booking> Bookings { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<UserMedia> UserMedia { get; }
        IGenericRepository<MediaLike> MediaLikes { get; }
        IGenericRepository<MediaComment> MediaComments { get; }
        IGenericRepository<Favorite> Favorites { get; }
        IGenericRepository<ContactMessage> ContactMessages { get; }
        Task<int> SaveChangesAsync();
    }
}
