using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyLamDep.Models.ViewModels
{
    // Enum định nghĩa phương thức thanh toán
    public enum PaymentMethod
    {
        [Display(Name = "Tiền mặt")]
        TienMat,

        [Display(Name = "Thẻ")]
        The,

        [Display(Name = "Chuyển khoản")]
        ChuyenKhoan
    }

    // Enum định nghĩa trạng thái thanh toán
    public enum PaymentStatus
    {
        [Display(Name = "Hoàn tất")]
        HoanTat,

        [Display(Name = "Đang xử lý")]
        DangXuLy,

        [Display(Name = "Thất bại")]
        ThatBai
    }

    public class PaymentViewModel
    {
        public int PaymentID { get; set; }

        [Required(ErrorMessage = "Số tiền thanh toán là bắt buộc.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0.")]
        [Display(Name = "Số tiền thanh toán")]
        public decimal Amount { get; set; }

        [Display(Name = "Ngày thanh toán")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod Method { get; set; }

        [Required(ErrorMessage = "Trạng thái thanh toán là bắt buộc.")]
        [Display(Name = "Trạng thái thanh toán")]
        public PaymentStatus Status { get; set; }

        // Liên kết đến hóa đơn
        public int InvoiceID { get; set; }
    }
}
