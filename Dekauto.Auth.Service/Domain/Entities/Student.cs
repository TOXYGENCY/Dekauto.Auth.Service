using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dekauto.Auth.Service.Domain.Entities;

public partial class Student
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string? Patronymic { get; set; }

    public bool? Gender { get; set; }

    public DateOnly? BirthdayDate { get; set; }

    public string? BirthdayPlace { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public DateOnly? EnrollementOrderDate { get; set; }

    public string? EnrollementOrderNum { get; set; }

    public string? Faculty { get; set; }

    public string? CourseOfTraining { get; set; }

    public string? Course { get; set; }

    public string? PassportSerial { get; set; }

    public string? PassportNumber { get; set; }

    public string? PassportIssuancePlace { get; set; }

    public DateOnly? PassportIssuanceDate { get; set; }

    public string? PassportIssuanceCode { get; set; }

    public string? AddressRegistrationIndex { get; set; }

    public string? AddressRegistrationOblKrayAvtobl { get; set; }

    public string? AddressRegistrationDistrict { get; set; }

    public string? AddressRegistrationStreet { get; set; }

    public string? AddressRegistrationCity { get; set; }

    public string? AddressRegistrationHouse { get; set; }

    public string? AddressRegistrationHousing { get; set; }

    public string? AddressRegistrationApartment { get; set; }

    public string? AddressResidentialIndex { get; set; }

    public string? AddressResidentialOblKrayAvtobl { get; set; }

    public string? AddressResidentialDistrict { get; set; }

    public string? AddressResidentialStreet { get; set; }

    public string? AddressResidentialCity { get; set; }

    public string? AddressResidentialHouse { get; set; }

    public string? AddressResidentialHousing { get; set; }

    public string? AddressResidentialApartment { get; set; }

    public bool? LivingInDormitory { get; set; }

    public Guid? GroupId { get; set; }

    public string? GiaExam1Name { get; set; }

    public short? GiaExam1Score { get; set; }

    public string? GiaExam1Note { get; set; }

    public string? GiaExam2Name { get; set; }

    public short? GiaExam2Score { get; set; }

    public string? GiaExam2Note { get; set; }

    public string? GiaExam3Name { get; set; }

    public short? GiaExam3Score { get; set; }

    public string? GiaExam3Note { get; set; }

    public string? GiaExam4Name { get; set; }

    public short? GiaExam4Score { get; set; }

    public string? GiaExam4Note { get; set; }

    public bool? MilitaryService { get; set; }

    public bool? MaritalStatus { get; set; }

    public Guid? OoId { get; set; }

    public short? OoExitYear { get; set; }

    public string? EducationReceived { get; set; }

    public string? EducationForm { get; set; }

    public string? EducationBase { get; set; }

    public short? EducationStartYear { get; set; }

    public short? EducationFinishYear { get; set; }

    public short? EducationTime { get; set; }

    public string? EducationRelationForm { get; set; }

    public string? EducationRelationNum { get; set; }

    public DateOnly? EducationRelationDate { get; set; }

    public string? EducationReceivedSerial { get; set; }

    public string? EducationReceivedNum { get; set; }

    public DateOnly? EducationReceivedDate { get; set; }

    public short? EducationReceivedEndYear { get; set; }

    public string? GradeBook { get; set; }

    public short? BonusScores { get; set; }

    public string? Citizenship { get; set; }

    public string? AddressRegistrationType { get; set; }

    public string? AddressResidentialType { get; set; }

    public string? AddressRegistrationHousingType { get; set; }

    public string? AddressResidentialHousingType { get; set; }

    public string? Education { get; set; }

    [JsonIgnore]
    public virtual Group? Group { get; set; }

    [JsonIgnore]
    public virtual Oo? Oo { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}
