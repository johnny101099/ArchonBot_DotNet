namespace ArchonBot.Models
{
    [Table("LOTTERY_EVENT_PARTICIPANT")]
    public class LOTTERY_EVENT_PARTICIPANT : BaseModel
    {
        public override long Id => LEP_SEQ ?? 0;

        /// <summary>索引(LEP_SEQ)</summary>
        [Key]
        [Column("LEP_SEQ")]
        [Display(Name = "索引")]
        public long? LEP_SEQ { get; set; }

        /// <summary>活動主檔索引(LEP_LEM_SEQ)</summary>
        [Column("LEP_LEM_SEQ")]
        [Display(Name = "活動主檔索引")]
        public required long LEP_LEM_SEQ { get; set; }

        /// <summary>使用者ID(LEP_USER_ID)</summary>
        [Column("LEP_USER_ID")]
        [Display(Name = "使用者ID")]
        public required ulong LEP_USER_ID { get; set; }

        /// <summary>使用者名稱(LEP_USER_NAME)</summary>
        [Column("LEP_USER_NAME")]
        [Display(Name = "使用者名稱")]
        public required string LEP_USER_NAME { get; set; }

        /// <summary>加入時間(LEP_JOIN_TIME)</summary>
        [Column("LEP_JOIN_TIME")]
        [Display(Name = "加入時間")]
        public required DateTime LEP_JOIN_TIME { get; set; }
    }
}
