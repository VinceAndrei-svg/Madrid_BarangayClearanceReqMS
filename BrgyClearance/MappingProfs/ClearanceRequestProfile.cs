using AutoMapper;
using Proj1.DTOs;
using Proj1.Models.Entities;
using Proj1.Models.ViewModels;
using Proj1.Models.Common.Enums;

namespace Proj1.MappingProfiles;

/// <summary>
/// AutoMapper profile for clearance request mappings.
/// Defines how to convert between entities, DTOs, and view models.
/// Best Practice: Keep all clearance-related mappings in one profile.
/// </summary>
public class ClearanceRequestProfile : Profile
{
    public ClearanceRequestProfile()
    {
        // ========================================
        // ENTITY → DTO MAPPINGS
        // ========================================

        /// <summary>
        /// Maps ClearanceRequest entity to DTO.
        /// Flattens navigation properties for easy consumption.
        /// </summary>
        CreateMap<ClearanceRequest, ClearanceRequestDto>()
            // Flatten Resident info
            .ForMember(dest => dest.ResidentFirstName,
                opt => opt.MapFrom(src => src.Resident.FirstName))
            .ForMember(dest => dest.ResidentLastName,
                opt => opt.MapFrom(src => src.Resident.LastName))
            .ForMember(dest => dest.ResidentAddress,
                opt => opt.MapFrom(src => src.Resident.Address))

            // ✅ FIX: Map to ClearanceType.Name (using backward-compatible [Column("TypeName")] mapping)
            .ForMember(dest => dest.ClearanceTypeName,
                opt => opt.MapFrom(src => src.ClearanceType.Name))
            .ForMember(dest => dest.Fee,
                opt => opt.MapFrom(src => src.ClearanceType.Fee))

            // All other properties map automatically by name
            ;

        /// <summary>
        /// Maps ClearanceType entity to DTO/ViewModel.
        /// ✅ FIX: Explicitly map Name property to TypeName property
        /// Note: Entity uses Name (with [Column("TypeName")]), ViewModel uses TypeName
        /// Best Practice: Always explicitly map when property names don't match
        /// </summary>
        CreateMap<ClearanceType, ClearanceTypeViewModel>()
            .ForMember(dest => dest.TypeName, 
                opt => opt.MapFrom(src => src.Name));

        // ========================================
        // DTO → VIEW MODEL MAPPINGS
        // ========================================

        /// <summary>
        /// Maps DTO to list view model.
        /// Converts computed properties and enum to string.
        /// </summary>
        CreateMap<ClearanceRequestDto, ClearanceRequestViewModel>()
            .ForMember(dest => dest.ResidentName,
                opt => opt.MapFrom(src => src.ResidentFullName))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        /// <summary>
        /// Maps DTO to details view model.
        /// Includes UI-specific transformations.
        /// 
        /// LEARNING: ProcessedByName is a display property that doesn't exist in the DTO.
        /// We ignore it here because it will be set manually in the controller/service if needed.
        /// </summary>
        CreateMap<ClearanceRequestDto, ClearanceRequestDetailsViewModel>()
            .ForMember(dest => dest.ResidentName,
                opt => opt.MapFrom(src => src.ResidentFullName))
            .ForMember(dest => dest.StatusDisplay,
                opt => opt.MapFrom(src => GetStatusDisplayName(src.Status)))
            // ✅ FIX: Ignore ProcessedByName - it's set separately if needed
            .ForMember(dest => dest.ProcessedByName,
                opt => opt.Ignore())
            // CanBeCancelled, CanBeProcessed, etc. are computed properties in ViewModel
            // They will be calculated automatically from Status
            ;

        // ========================================
        // VIEW MODEL → DTO MAPPINGS
        // ========================================

        /// <summary>
        /// Maps create view model to DTO for service layer.
        /// </summary>
        CreateMap<CreateClearanceRequestViewModel, CreateClearanceRequestDto>();

        /// <summary>
        /// Maps process view model to DTO.
        /// 
        /// LEARNING: ProcessedByUserId is set in the controller from the logged-in user,
        /// not from the form. The ViewModel has display properties (ReferenceNumber, etc.)
        /// that don't exist in the DTO.
        /// </summary>
        CreateMap<ProcessClearanceRequestViewModel, ProcessClearanceRequestDto>()
            // ✅ FIX: Ignore ProcessedByUserId - it's set in the controller from User.FindFirstValue()
            .ForMember(dest => dest.ProcessedByUserId, opt => opt.Ignore());

        /// <summary>
        /// Maps cancel view model to DTO.
        /// </summary>
        CreateMap<CancelRequestViewModel, CancelRequestDto>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // Set in controller

        // ========================================
        // DTO → ENTITY MAPPINGS
        // ========================================

        /// <summary>
        /// Maps create DTO to new entity.
        /// 
        /// LEARNING: When creating a new entity, many properties are:
        /// 1. Auto-generated (Id, ReferenceNumber)
        /// 2. Set by the service layer (Status, RequestDate)
        /// 3. Audit fields (CreatedDate, CreatedBy)
        /// 4. Workflow fields that start as null (ProcessedDate, ReleasedDate, etc.)
        /// 
        /// We must explicitly ignore ALL of these to avoid AutoMapper errors.
        /// </summary>
        CreateMap<CreateClearanceRequestDto, ClearanceRequest>()
            // ✅ FIX: Ignore auto-generated/service-set properties
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ReferenceNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.RequestDate, opt => opt.Ignore())
            .ForMember(dest => dest.Resident, opt => opt.Ignore())
            .ForMember(dest => dest.ClearanceType, opt => opt.Ignore())
            
            // ✅ FIX: Ignore workflow properties (set later when request is processed/paid/released)
            .ForMember(dest => dest.ProcessedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.ProcessedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Remarks, opt => opt.Ignore())
            .ForMember(dest => dest.ReleasedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiryDate, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledBy, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledDate, opt => opt.Ignore())
            .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
            .ForMember(dest => dest.IsPaid, opt => opt.Ignore())
            .ForMember(dest => dest.PaidDate, opt => opt.Ignore())
            .ForMember(dest => dest.CollectedByUserId, opt => opt.Ignore())
            
            // ✅ FIX: Ignore document generation properties
            .ForMember(dest => dest.ClearanceDocumentPath, opt => opt.Ignore())
            .ForMember(dest => dest.DocumentGeneratedDate, opt => opt.Ignore())
            .ForMember(dest => dest.DocumentGeneratedByUserId, opt => opt.Ignore())
            
            // ✅ FIX: Ignore OR/payment properties
            .ForMember(dest => dest.OfficialReceiptNumber, opt => opt.Ignore())
            .ForMember(dest => dest.AmountPaid, opt => opt.Ignore())
            
            // ✅ FIX: Ignore audit/base entity properties
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedDate, opt => opt.Ignore());
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    /// <summary>
    /// Converts RequestStatus enum to user-friendly display name.
    /// Best Practice: Single source of truth for status display names.
    /// </summary>
    private static string GetStatusDisplayName(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.Submitted => "Submitted",
            RequestStatus.Pending => "Pending Review",
            RequestStatus.Approved => "Approved",
            RequestStatus.Rejected => "Rejected",
            RequestStatus.Cancelled => "Cancelled",
            RequestStatus.ForRelease => "Ready for Release",
            RequestStatus.Released => "Released",
            RequestStatus.Expired => "Expired",
            _ => status.ToString()
        };
    }
}