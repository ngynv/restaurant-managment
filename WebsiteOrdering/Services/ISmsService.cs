﻿namespace WebsiteOrdering.Services
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);

    }
}
