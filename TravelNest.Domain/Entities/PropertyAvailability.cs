using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TravelNest.Domain.Entities
{
    public class PropertyAvailability : BaseEntity
    {
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
        public bool IsBlocked { get; set; } = false;
        public decimal? SpecialPrice { get; set; }

        // Foreign Keys
        public Guid PropertyId { get; set; }

        // Navigation
        public Property Property { get; set; } = null!;
    }
}
