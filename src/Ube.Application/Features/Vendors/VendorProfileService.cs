using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;

namespace Ube.Application.Features.Vendors;

public class VendorProfileService : IVendorProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IVendorProfileRepository _vendorProfileRepository;

    public VendorProfileService(
        IUserRepository userRepository,
        IVendorProfileRepository vendorProfileRepository)
    {
        _userRepository = userRepository;
        _vendorProfileRepository = vendorProfileRepository;
    }
    // Method to get vendor profile
    public async Task<VendorProfileDto> GetVendorProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("User not found");

        var vendorProfile = await _vendorProfileRepository.GetVendorIdAsync(userId);

        if (vendorProfile == null)
            throw new NotFoundException("Vendor profile not found");

        return new VendorProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            BusinessName = vendorProfile.BusinessName,
            BusinessType = vendorProfile.BusinessType,
            Bio = vendorProfile.Bio,
            BusinessDescription = vendorProfile.BusinessDescription
        };
    }
    // Method to update vendor profile
    public async Task<VendorProfileDto> UpdateVendorProfileAsync(Guid userId,UpdateVendorProfileDto dto)
    {
        // 1. Get user
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("User not found");

        // 2. Get vendor profile
        var vendorProfile = await _vendorProfileRepository.GetVendorIdAsync(userId);

        if (vendorProfile == null)
            throw new NotFoundException("Vendor profile not found");

        // 3. Update User fields
        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        // 4. Update Vendor fields
        vendorProfile.BusinessName = dto.BusinessName.Trim();
        vendorProfile.BusinessDescription = dto.BusinessDescription?.Trim() ?? string.Empty;
        vendorProfile.Bio = dto.Bio?.Trim() ?? string.Empty;
        vendorProfile.UpdatedAt = DateTime.UtcNow;

        // 5. Save changes
        await _userRepository.UpdateAsync(user);
        await _vendorProfileRepository.UpdateAsync(vendorProfile);

        // 6. Return updated data
        return new VendorProfileDto
        {
            // User
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,

            // Vendor
            BusinessName = vendorProfile.BusinessName,
            BusinessType = vendorProfile.BusinessType,
            Bio = vendorProfile.Bio,
            BusinessDescription = vendorProfile.BusinessDescription
        };
    }
    //method to update profile image url in user entity
    public async Task UpdateProfileImageAsync(Guid userId, string imageUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("User not found");

        user.ProfileImageUrl = imageUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }
}