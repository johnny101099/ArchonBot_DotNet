namespace ArchonBot.Models
{
    [Table("BOT_PARAM_MASTER")]
    public class BOT_PARAM_MASTER : BaseModel
    {
        public override long Id => BPM_SEQ ?? 0;

        /// <summary>索引(BPM_SEQ)</summary>
        [Key]
        [Column("BPM_SEQ")]
        [Display(Name = "索引")]
        public long? BPM_SEQ { get; set; }

        /// <summary>機器人ID</summary>
        [Column("BPM_BOT_ID")]
        [Display(Name = "機器人ID")]
        public required string BPM_BOT_ID { get; set; }

        /// <summary>模組ID</summary>
        [Column("BPM_MODULE_ID")]
        [Display(Name = "模組ID")]
        public required string BPM_MODULE_ID { get; set; }

        /// <summary>參數類別</summary>
        [Column("BPM_PARAM_TYPE")]
        [Display(Name = "參數類別")]
        public required string BPM_PARAM_TYPE { get; set; }

        /// <summary>參數類別說明</summary>
        [Column("BPM_PARAM_DESC")]
        [Display(Name = "參數類別說明")]
        public string? BPM_PARAM_DESC { get; set; }
    }
}
