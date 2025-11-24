namespace ArchonBot.Models
{
    [Table("BOT_PARAM_DETAIL")]
    public class BOT_PARAM_DETAIL : BaseModel
    {
        public override long Id => BPD_SEQ ?? 0;

        /// <summary>索引(BPD_SEQ)</summary>
        [Key]
        [Column("BPD_SEQ")]
        [Display(Name = "索引")]
        public long? BPD_SEQ { get; set; }

        /// <summary>主檔索引(BPM_SEQ)</summary>
        [Key]
        [Column("BPD_BPM_SEQ")]
        [Display(Name = "主檔索引")]
        public required long BPD_BPM_SEQ { get; set; }

        /// <summary>機器人ID</summary>
        [Column("BPD_BOT_ID")]
        [Display(Name = "機器人ID")]
        public required string BPD_BOT_ID { get; set; }

        /// <summary>模組ID</summary>
        [Column("BPD_MODULE_ID")]
        [Display(Name = "模組ID")]
        public required string BPD_MODULE_ID { get; set; }

        /// <summary>參數類別</summary>
        [Column("BPD_PARAM_TYPE")]
        [Display(Name = "參數類別")]
        public required string BPD_PARAM_TYPE { get; set; }

        /// <summary>伺服器ID</summary>
        [Column("BPD_GUILD_ID")]
        [Display(Name = "伺服器ID")]
        public required long BPD_GUILD_ID { get; set; }

        /// <summary>參數ID</summary>
        [Column("BPD_PARAM_ID")]
        [Display(Name = "參數ID")]
        public required string BPD_PARAM_ID { get; set; }

        /// <summary>參數值</summary>
        [Column("BPD_PARAM_VALUE")]
        [Display(Name = "參數值")]
        public required string BPD_PARAM_VALUE { get; set; }

        /// <summary>參數名稱</summary>
        [Column("BPD_PARAM_NAME")]
        [Display(Name = "參數名稱")]
        public string? BPD_PARAM_NAME { get; set; }

        /// <summary>參數屬性</summary>
        [Column("BPD_PARAM_ATTR")]
        [Display(Name = "參數屬性")]
        public string? BPD_PARAM_ATTR { get; set; }

        /// <summary>參數說明</summary>
        [Column("BPD_PARAM_DESC")]
        [Display(Name = "參數說明")]
        public string? BPD_PARAM_DESC { get; set; }
    }
}
