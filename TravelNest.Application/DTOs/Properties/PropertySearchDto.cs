using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TravelNest.Application.DTOs.Common;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Properties
{
    public class PropertySearchDto : PagedRequest
    {
        public PropertyType? Type { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinGuests { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? CheckIn { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? CheckOut { get; set; }
        public double? MinRating { get; set; }
    }
}
