using Campus_Management.DTOs.Colleges;
using Campus_Management.Models;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services.Interfaces;

namespace Campus_Management.Services;

public class CollegeService : ICollegeService
{
    private readonly ICollegeRepository _collegeRepository;

    public CollegeService(ICollegeRepository collegeRepository)
    {
        _collegeRepository = collegeRepository;
    }

    public async Task<IEnumerable<CollegeResponse>> GetAllAsync()
    {
        var colleges = await _collegeRepository.GetAllAsync();

        return colleges.Select(MapToResponse);
    }

    public async Task<CollegeResponse?> GetByIdAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);

        return college == null ? null : MapToResponse(college);
    }

    public async Task<CollegeResponse> CreateAsync(CreateCollegeRequest request)
    {
        var college = new College
        {
            CollegeName = request.CollegeName,
            City = request.City,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email
        };

        var createdCollege = await _collegeRepository.AddAsync(college);

        return MapToResponse(createdCollege);
    }

    public async Task<bool> UpdateAsync(
        int collegeId,
        UpdateCollegeRequest request)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);

        if (college == null)
        {
            return false;
        }

        college.CollegeName = request.CollegeName;
        college.City = request.City;
        college.Address = request.Address;
        college.PhoneNumber = request.PhoneNumber;
        college.Email = request.Email;
        college.Status = (Models.Enums.CollegeStatus)request.Status;

        await _collegeRepository.UpdateAsync(college);

        return true;
    }

    public async Task<bool> DeleteAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);

        if (college == null)
        {
            return false;
        }

        await _collegeRepository.DeleteAsync(college);

        return true;
    }

    private static CollegeResponse MapToResponse(College college)
    {
        return new CollegeResponse
        {
            CollegeId = college.CollegeId,
            CollegeName = college.CollegeName,
            City = college.City,
            Address = college.Address,
            PhoneNumber = college.PhoneNumber,
            Email = college.Email,
            Status = college.Status
        };
    }
}