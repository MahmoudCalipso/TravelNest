
using TravelNest.Domain.Entities;
using TravelNest.Domain.Interfaces;
using TravelNest.Infrastructure.Data;

namespace TravelNest.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    private IGenericRepository<User>? _users;
    private IGenericRepository<Property>? _properties;
    private IGenericRepository<PropertyMedia>? _propertyMedia;
    private IGenericRepository<PropertyAmenity>? _propertyAmenities;
    private IGenericRepository<PropertyAvailability>? _propertyAvailabilities;
    private IGenericRepository<Booking>? _bookings;
    private IGenericRepository<Payment>? _payments;
    private IGenericRepository<Review>? _reviews;
    private IGenericRepository<UserMedia>? _userMedia;
    private IGenericRepository<MediaLike>? _mediaLikes;
    private IGenericRepository<MediaComment>? _mediaComments;
    private IGenericRepository<Favorite>? _favorites;
    private IGenericRepository<ContactMessage>? _contactMessages;

    public IGenericRepository<User> Users
        => _users ??= new GenericRepository<User>(_context);
    public IGenericRepository<Property> Properties
        => _properties ??= new GenericRepository<Property>(_context);
    public IGenericRepository<PropertyMedia> PropertyMedia
        => _propertyMedia ??= new GenericRepository<PropertyMedia>(_context);
    public IGenericRepository<PropertyAmenity> PropertyAmenities
        => _propertyAmenities ??= new GenericRepository<PropertyAmenity>(_context);
    public IGenericRepository<PropertyAvailability> PropertyAvailabilities
        => _propertyAvailabilities ??= new GenericRepository<PropertyAvailability>(_context);
    public IGenericRepository<Booking> Bookings
        => _bookings ??= new GenericRepository<Booking>(_context);
    public IGenericRepository<Payment> Payments
        => _payments ??= new GenericRepository<Payment>(_context);
    public IGenericRepository<Review> Reviews
        => _reviews ??= new GenericRepository<Review>(_context);
    public IGenericRepository<UserMedia> UserMedia
        => _userMedia ??= new GenericRepository<UserMedia>(_context);
    public IGenericRepository<MediaLike> MediaLikes
        => _mediaLikes ??= new GenericRepository<MediaLike>(_context);
    public IGenericRepository<MediaComment> MediaComments
        => _mediaComments ??= new GenericRepository<MediaComment>(_context);
    public IGenericRepository<Favorite> Favorites
        => _favorites ??= new GenericRepository<Favorite>(_context);
    public IGenericRepository<ContactMessage> ContactMessages
        => _contactMessages ??= new GenericRepository<ContactMessage>(_context);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}
