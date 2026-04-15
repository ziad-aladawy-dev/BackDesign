using System.Text.Json;
using HUP.Application.DTOs.IdentityDtos.UserDtos;
using HUP.Common.Helpers;
using HUP.Core.Entities.Identity;
using HUP.Core.Models;
using Riok.Mapperly.Abstractions;

namespace HUP.Application.Mappers.Identity
{
    [Mapper(AllowNullPropertyAssignment = false)]
    public static partial class UserMapper
    {
        // Map UserDto to User entity and vice versa
        public static partial User ToEntity(UserDto userDto);
        public static partial UserDto ToDto(User user);
        
        public static UsersListResponse ToListDto(UserSummary user, string lang){
            var dto = new UsersListResponse();
            dto.Id = user.Id;
            dto.FullName = LocalizationHelper.Get<string>(user.FullName, lang);
            dto.NationalId = user.NationalId;
            dto.RoleName = LocalizationHelper.Get<string>(user.RoleName, lang);
            return dto;
        }

        public static ProfileInfoDto ToProfileDto(User user, string lang)
        {
            var dto = new ProfileInfoDto();
            dto.ContactInfo = ToContactDto(user.ContactInfo, lang);
            dto.PersonalInfo = ToPersonalDto(user.PersonalInfo, lang);
            dto.FullName = LocalizationHelper.Get<string>(user.FullName, lang);
            dto.Email = user.Email;
            dto.NationalId = user.NationalId;
            return dto;
        }

        public static PersonalInfoDto ToPersonalDto(UserPersonalInfo personalInfo, string lang)
        {
            var dto = new PersonalInfoDto();
            dto.BirthDate = personalInfo.BirthDate ?? DateTime.MinValue;
            dto.BirthPlace = LocalizationHelper.Get(personalInfo.BirthPlace, lang);
            dto.Gender = LocalizationHelper.Get(personalInfo.Gender, lang);
            dto.Nationality = LocalizationHelper.Get(personalInfo.Nationality, lang);
            dto.Religion = LocalizationHelper.Get(personalInfo.Religion, lang);
            return dto;
        }

        public static ContactInfoDto ToContactDto(UserContact contactInfo, string lang)
        {
            var dto = new ContactInfoDto();
            dto.Address = contactInfo.Address;
            if (contactInfo.City != null)
                dto.City = LocalizationHelper.Get(contactInfo.City, lang);
            dto.PhoneNumber = contactInfo.PhoneNumber;
            dto.AltEmail = contactInfo.AltEmail;
            return dto;
        }
        public static partial UserPersonalInfo ToPersonalEntity(PersonalInfoDto personalInfo);
        public static partial UserContact ToContactEntity(ContactInfoDto contactInfo);

        public static User ToCreateEntity(CreateUserDto userDto)
        {
            var user = new User();
            user.ContactInfo = ToContactEntity(userDto.ContactInfo);
            user.PersonalInfo = ToPersonalEntity(userDto.PersonalInfo);
            user.NationalId = userDto.NationalId;
            user.Email = userDto.Email;
            user.FullName = JsonSerializer.Serialize(userDto.FullName);
            user.PasswordHash = userDto.PasswordHash;
            user.RoleId = userDto.RoleId;
            return user;
        }

        public static partial User ToUpdate(UpdateInfoDto infoDto);
        
    }
}