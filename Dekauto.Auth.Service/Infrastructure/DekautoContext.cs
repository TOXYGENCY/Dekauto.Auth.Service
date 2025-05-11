using System;
using System.Collections.Generic;
using Dekauto.Auth.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dekauto.Auth.Service.Infrastructure;

public partial class DekautoContext : DbContext
{
    public DekautoContext()
    {
    }

    public DekautoContext(DbContextOptions<DekautoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Oo> Oos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("groups_pkey");

            entity.ToTable("groups");

            entity.HasIndex(e => e.Name, "groups_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Oo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OO_pkey");

            entity.ToTable("OO");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.OoAddress)
                .HasMaxLength(1000)
                .HasColumnName("OO_address");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.EngName, "roles_eng_name_key").IsUnique();

            entity.HasIndex(e => e.Name, "roles_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.EngName)
                .HasMaxLength(50)
                .HasColumnName("eng_name");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("students_pkey");

            entity.ToTable("students");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AddressRegistrationApartment)
                .HasMaxLength(20)
                .HasColumnName("address_registration_apartment");
            entity.Property(e => e.AddressRegistrationCity)
                .HasMaxLength(50)
                .HasColumnName("address_registration_city");
            entity.Property(e => e.AddressRegistrationDistrict)
                .HasMaxLength(100)
                .HasColumnName("address_registration_district");
            entity.Property(e => e.AddressRegistrationHouse)
                .HasMaxLength(10)
                .HasColumnName("address_registration_house");
            entity.Property(e => e.AddressRegistrationHousing)
                .HasMaxLength(20)
                .HasColumnName("address_registration_housing");
            entity.Property(e => e.AddressRegistrationHousingType)
                .HasMaxLength(50)
                .HasColumnName("address_registration_housing_type");
            entity.Property(e => e.AddressRegistrationIndex)
                .HasMaxLength(20)
                .HasColumnName("address_registration_index");
            entity.Property(e => e.AddressRegistrationOblKrayAvtobl)
                .HasMaxLength(100)
                .HasColumnName("address_registration_obl_kray_avtobl");
            entity.Property(e => e.AddressRegistrationStreet)
                .HasMaxLength(400)
                .HasColumnName("address_registration_street");
            entity.Property(e => e.AddressRegistrationType)
                .HasMaxLength(50)
                .HasColumnName("address_registration_type");
            entity.Property(e => e.AddressResidentialApartment)
                .HasMaxLength(20)
                .HasColumnName("address_residential_apartment");
            entity.Property(e => e.AddressResidentialCity)
                .HasMaxLength(50)
                .HasColumnName("address_residential_city");
            entity.Property(e => e.AddressResidentialDistrict)
                .HasMaxLength(100)
                .HasColumnName("address_residential_district");
            entity.Property(e => e.AddressResidentialHouse)
                .HasMaxLength(10)
                .HasColumnName("address_residential_house");
            entity.Property(e => e.AddressResidentialHousing)
                .HasMaxLength(20)
                .HasColumnName("address_residential_housing");
            entity.Property(e => e.AddressResidentialHousingType)
                .HasMaxLength(50)
                .HasColumnName("address_residential_housing_type");
            entity.Property(e => e.AddressResidentialIndex)
                .HasMaxLength(20)
                .HasColumnName("address_residential_index");
            entity.Property(e => e.AddressResidentialOblKrayAvtobl)
                .HasMaxLength(100)
                .HasColumnName("address_residential_obl_kray_avtobl");
            entity.Property(e => e.AddressResidentialStreet)
                .HasMaxLength(400)
                .HasColumnName("address_residential_street");
            entity.Property(e => e.AddressResidentialType)
                .HasMaxLength(50)
                .HasColumnName("address_residential_type");
            entity.Property(e => e.BirthdayDate).HasColumnName("birthday_date");
            entity.Property(e => e.BirthdayPlace)
                .HasMaxLength(250)
                .HasColumnName("birthday_place");
            entity.Property(e => e.BonusScores)
                .HasDefaultValue((short)0)
                .HasColumnName("bonus_scores");
            entity.Property(e => e.Citizenship)
                .HasMaxLength(100)
                .HasColumnName("citizenship");
            entity.Property(e => e.Course)
                .HasMaxLength(100)
                .HasColumnName("course");
            entity.Property(e => e.CourseOfTraining)
                .HasMaxLength(300)
                .HasColumnName("course_of_training");
            entity.Property(e => e.Education)
                .HasMaxLength(100)
                .HasColumnName("education");
            entity.Property(e => e.EducationBase)
                .HasMaxLength(200)
                .HasColumnName("education_base");
            entity.Property(e => e.EducationFinishYear).HasColumnName("education_finish_year");
            entity.Property(e => e.EducationForm)
                .HasMaxLength(200)
                .HasColumnName("education_form");
            entity.Property(e => e.EducationReceived)
                .HasMaxLength(200)
                .HasColumnName("education_received");
            entity.Property(e => e.EducationReceivedDate).HasColumnName("education_received_date");
            entity.Property(e => e.EducationReceivedEndYear).HasColumnName("education_received_end_year");
            entity.Property(e => e.EducationReceivedNum)
                .HasMaxLength(100)
                .HasColumnName("education_received_num");
            entity.Property(e => e.EducationReceivedSerial)
                .HasMaxLength(100)
                .HasColumnName("education_received_serial");
            entity.Property(e => e.EducationRelationDate).HasColumnName("education_relation_date");
            entity.Property(e => e.EducationRelationForm)
                .HasMaxLength(200)
                .HasColumnName("education_relation_form");
            entity.Property(e => e.EducationRelationNum)
                .HasMaxLength(200)
                .HasColumnName("education_relation_num");
            entity.Property(e => e.EducationStartYear).HasColumnName("education_start_year");
            entity.Property(e => e.EducationTime).HasColumnName("education_time");
            entity.Property(e => e.Email)
                .HasMaxLength(300)
                .HasColumnName("email");
            entity.Property(e => e.EnrollementOrderDate).HasColumnName("enrollement_order_date");
            entity.Property(e => e.EnrollementOrderNum)
                .HasMaxLength(20)
                .HasColumnName("enrollement_order_num");
            entity.Property(e => e.Faculty)
                .HasMaxLength(100)
                .HasColumnName("faculty");
            entity.Property(e => e.Gender)
                .HasDefaultValue(true)
                .HasColumnName("gender");
            entity.Property(e => e.GiaExam1Name)
                .HasMaxLength(50)
                .HasColumnName("gia_exam_1_name");
            entity.Property(e => e.GiaExam1Note)
                .HasMaxLength(200)
                .HasColumnName("gia_exam_1_note");
            entity.Property(e => e.GiaExam1Score).HasColumnName("gia_exam_1_score");
            entity.Property(e => e.GiaExam2Name)
                .HasMaxLength(50)
                .HasColumnName("gia_exam_2_name");
            entity.Property(e => e.GiaExam2Note)
                .HasMaxLength(200)
                .HasColumnName("gia_exam_2_note");
            entity.Property(e => e.GiaExam2Score).HasColumnName("gia_exam_2_score");
            entity.Property(e => e.GiaExam3Name)
                .HasMaxLength(50)
                .HasColumnName("gia_exam_3_name");
            entity.Property(e => e.GiaExam3Note)
                .HasMaxLength(200)
                .HasColumnName("gia_exam_3_note");
            entity.Property(e => e.GiaExam3Score).HasColumnName("gia_exam_3_score");
            entity.Property(e => e.GiaExam4Name)
                .HasMaxLength(50)
                .HasColumnName("gia_exam_4_name");
            entity.Property(e => e.GiaExam4Note)
                .HasMaxLength(200)
                .HasColumnName("gia_exam_4_note");
            entity.Property(e => e.GiaExam4Score).HasColumnName("gia_exam_4_score");
            entity.Property(e => e.GradeBook)
                .HasMaxLength(100)
                .HasColumnName("grade_book");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.LivingInDormitory)
                .HasDefaultValue(false)
                .HasColumnName("living_in_dormitory");
            entity.Property(e => e.MaritalStatus)
                .HasDefaultValue(false)
                .HasColumnName("marital_status");
            entity.Property(e => e.MilitaryService)
                .HasDefaultValue(false)
                .HasColumnName("military_service");
            entity.Property(e => e.Name)
                .HasMaxLength(75)
                .HasColumnName("name");
            entity.Property(e => e.OoExitYear).HasColumnName("OO_exit_year");
            entity.Property(e => e.OoId).HasColumnName("OO_id");
            entity.Property(e => e.PassportIssuanceCode)
                .HasMaxLength(20)
                .HasColumnName("passport_issuance_code");
            entity.Property(e => e.PassportIssuanceDate).HasColumnName("passport_issuance_date");
            entity.Property(e => e.PassportIssuancePlace)
                .HasMaxLength(1000)
                .HasColumnName("passport_issuance_place");
            entity.Property(e => e.PassportNumber)
                .HasMaxLength(20)
                .HasColumnName("passport_number");
            entity.Property(e => e.PassportSerial)
                .HasMaxLength(20)
                .HasColumnName("passport_serial");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(75)
                .HasColumnName("patronymic");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("phone_number");
            entity.Property(e => e.Surname)
                .HasMaxLength(75)
                .HasColumnName("surname");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Group).WithMany(p => p.Students)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("students_group_id_fkey");

            entity.HasOne(d => d.Oo).WithMany(p => p.Students)
                .HasForeignKey(d => d.OoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("students_OO_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("students_user_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Login, "users_login_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(100)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("users_role_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
