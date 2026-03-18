using AutoMapper;
using TravelNest.Application.DTOs.Bookings;
using TravelNest.Application.DTOs.Media;
using TravelNest.Application.DTOs.Messages;
using TravelNest.Application.DTOs.Properties;
using TravelNest.Application.DTOs.Reviews;
using TravelNest.Application.DTOs.Users;
using TravelNest.Domain.Entities;

namespace TravelNest.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.TotalProperties, o => o.MapFrom(s => s.Properties.Count))
            .ForMember(d => d.TotalBookings, o => o.MapFrom(s => s.Bookings.Count))
            .ForMember(d => d.TotalMediaPosts, o => o.MapFrom(s => s.MediaPosts.Count));
        CreateMap<UpdateProfileDto, User>()
            .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

        // Property
        CreateMap<CreatePropertyDto, Property>();
        CreateMap<UpdatePropertyDto, Property>()
            .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Property, PropertyDto>()
            .ForMember(d => d.ProviderName, o => o.MapFrom(s => $"{s.Provider.FirstName} {s.Provider.LastName}"))
            .ForMember(d => d.ProviderPhone, o => o.MapFrom(s => s.Provider.PhoneNumber))
            .ForMember(d => d.ProviderProfilePic, o => o.MapFrom(s => s.Provider.ProfilePictureUrl))
            .ForMember(d => d.Amenities, o => o.MapFrom(s => s.Amenities.Select(a => a.Name).ToList()));
        CreateMap<PropertyMedia, PropertyMediaDto>();

        // Booking
        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.PropertyName, o => o.MapFrom(s => s.Property.Name))
            .ForMember(d => d.PropertyType, o => o.MapFrom(s => s.Property.Type.ToString()))
            .ForMember(d => d.PropertyCoverImage, o => o.MapFrom(s =>
                s.Property.Media.FirstOrDefault(m => m.IsCover) != null
                    ? s.Property.Media.First(m => m.IsCover).Url
                    : s.Property.Media.FirstOrDefault()!.Url))
            .ForMember(d => d.PropertyCity, o => o.MapFrom(s => s.Property.City))
            .ForMember(d => d.PropertyCountry, o => o.MapFrom(s => s.Property.Country))
            .ForMember(d => d.TravellerName, o => o.MapFrom(s => $"{s.Traveller.FirstName} {s.Traveller.LastName}"))
            .ForMember(d => d.TravellerEmail, o => o.MapFrom(s => s.Traveller.Email))
            .ForMember(d => d.TravellerPhone, o => o.MapFrom(s => s.Traveller.PhoneNumber))
            .ForMember(d => d.ProviderId, o => o.MapFrom(s => s.Property.ProviderId))
            .ForMember(d => d.ProviderName, o => o.MapFrom(s => $"{s.Property.Provider.FirstName} {s.Property.Provider.LastName}"));

        // Payment
        CreateMap<Payment, PaymentDto>();

        // Review
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.TravellerName, o => o.MapFrom(s => $"{s.Traveller.FirstName} {s.Traveller.LastName}"))
            .ForMember(d => d.TravellerProfilePic, o => o.MapFrom(s => s.Traveller.ProfilePictureUrl));

        // UserMedia
        CreateMap<UserMedia, UserMediaDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"))
            .ForMember(d => d.UserProfilePic, o => o.MapFrom(s => s.User.ProfilePictureUrl))
            .ForMember(d => d.PropertyName, o => o.MapFrom(s => s.Property != null ? s.Property.Name : null));
        CreateMap<MediaComment, MediaCommentDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"))
            .ForMember(d => d.UserProfilePic, o => o.MapFrom(s => s.User.ProfilePictureUrl));

        // Messages
        CreateMap<ContactMessage, MessageDto>()
            .ForMember(d => d.SenderName, o => o.MapFrom(s => $"{s.Sender.FirstName} {s.Sender.LastName}"))
            .ForMember(d => d.SenderProfilePic, o => o.MapFrom(s => s.Sender.ProfilePictureUrl))
            .ForMember(d => d.ReceiverName, o => o.MapFrom(s => $"{s.Receiver.FirstName} {s.Receiver.LastName}"))
            .ForMember(d => d.BookingReference, o => o.MapFrom(s => s.Booking != null ? s.Booking.BookingReference : null));
    }
}
