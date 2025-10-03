using System;

namespace TradeSphere3.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }  // nullable

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}