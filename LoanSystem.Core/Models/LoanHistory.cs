using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanSystem.Core.Models
{
    public class LoanHistory
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public double InterestRate { get; set; }
        public int TermYears { get; set; }
        public int TermMonths { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPayment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
