namespace ArchonBot.Models
{
    [Table("LOTTERY_EVENT_WINNER")]
    public class LOTTERY_EVENT_WINNER : BaseModel
    {
        public override long Id => LEW_SEQ ?? 0;

        /// <summary>索引(LEW_SEQ)</summary>
        [Key]
        [Column("LEW_SEQ")]
        [Display(Name = "索引")]
        public long? LEW_SEQ { get; set; }

        /// <summary>活動索引(LEW_LEM_SEQ)</summary>
        [Column("LEW_LEM_SEQ")]
        [Display(Name = "活動索引")]
        public required long LEW_LEM_SEQ { get; set; }

        /// <summary>參與者索引(LEW_LEP_SEQ)</summary>
        [Column("LEW_LEP_SEQ")]
        [Display(Name = "參與者索引")]
        public required long LEW_LEP_SEQ { get; set; }

        /// <summary>使用者ID(LEW_USER_ID)</summary>
        [Column("LEW_USER_ID")]
        [Display(Name = "使用者ID")]
        public required ulong LEW_USER_ID { get; set; }

        /// <summary>使用者名稱(LEW_USER_NAME)</summary>
        [Column("LEW_USER_NAME")]
        [Display(Name = "使用者名稱")]
        public required string LEW_USER_NAME { get; set; }

        /// <summary>批次(LEW_BATCH)</summary>
        [Column("LEW_BATCH")]
        [Display(Name = "批次")]
        public required int LEW_BATCH { get; set; }

        /// <summary>是否已領獎(LEW_HAS_CLAIM)</summary>
        [Column("LEW_HAS_CLAIM")]
        [Display(Name = "是否已領獎")]
        public required bool LEW_HAS_CLAIM { get; set; }

        /// <summary>中獎時間(LEW_WINNER_TIME)</summary>
        [Column("LEW_WINNER_TIME")]
        [Display(Name = "中獎時間")]
        public required DateTime LEW_WINNER_TIME { get; set; }
    }
}
