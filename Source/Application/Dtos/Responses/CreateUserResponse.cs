﻿using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Dtos;

namespace ApplicationCore.Dtos.Responses
{
    public record CreateUserResponse : AuditableEntityDto<int>
    {
        public CreateUserResponse(User user)
        {
            Id = user.Id;
            Username = user.Username;
            Role = user.Role;
            InsertedAt = user.Inserted_At;
            InsertedBy = user.Inserted_By;
        }

        public string Username { get; set; }
        public string Role { get; set; }
    }
}
