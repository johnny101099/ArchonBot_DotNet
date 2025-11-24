namespace ArchonBot.Models
{
    [Table("LOTTERY_EVENT_MASTER")]
    public class LOTTERY_EVENT_MASTER : BaseModel
    {
		public override long Id => LEM_SEQ ?? 0;

        /// <summary>索引(LEM_SEQ)</summary>
        [Key]
        [Column("LEM_SEQ")]
        [Display(Name = "索引")]
        public long? LEM_SEQ { get; set; }

		/// <summary>擁有者ID(LEM_OWNER_ID)</summary>
		[Column("LEM_OWNER_ID")]
		[Display(Name = "擁有者ID")]
		public required ulong LEM_OWNER_ID { get; set; }

		/// <summary>活動名稱(LEM_NAME)</summary>
		[Column("LEM_NAME")]
		[Display(Name = "活動名稱")]
		public required string LEM_NAME { get; set; }

		/// <summary>獎品數量(LEM_PRIZE_AMOUNT)</summary>
		[Column("LEM_PRIZE_AMOUNT")]
		[Display(Name = "獎品數量")]
		public required int LEM_PRIZE_AMOUNT { get; set; }

		/// <summary>活動說明(LEM_DESC)</summary>
		[Column("LEM_DESC")]
		[Display(Name = "活動說明")]
		public string? LEM_DESC { get; set; }

		/// <summary>參加方式(LEM_JOIN_MODE)</summary>
		[Column("LEM_JOIN_MODE")]
		[Display(Name = "參加方式")]
		public required string LEM_JOIN_MODE { get; set; }

		/// <summary>活動狀態(LEM_STATUS)</summary>
		[Column("LEM_STATUS")]
		[Display(Name = "活動狀態")]
		public required string LEM_STATUS { get; set; }

        /// <summary>允許重複中獎(LEM_ALLOW_DUPLICATE)</summary>
        [Column("LEM_ALLOW_DUPLICATE")]
		[Display(Name = "允許重複中獎")]
		public required bool LEM_ALLOW_DUPLICATE { get; set; }

		/// <summary>伺服器ID(LEM_GUILD_ID)</summary>
		[Column("LEM_GUILD_ID")]
		[Display(Name = "伺服器ID")]
		public ulong? LEM_GUILD_ID { get; set; }

		/// <summary>頻道ID(LEM_CHANNEL_ID)</summary>
		[Column("LEM_CHANNEL_ID")]
		[Display(Name = "頻道ID")]
		public ulong? LEM_CHANNEL_ID { get; set; }

		/// <summary>訊息ID(LEM_MESSAGE_ID)</summary>
		[Column("LEM_MESSAGE_ID")]
		[Display(Name = "訊息ID")]
		public ulong? LEM_MESSAGE_ID { get; set; }

		/// <summary>抽獎批次(LEM_DRAW_BATCH)</summary>
		[Column("LEM_DRAW_BATCH")]
		[Display(Name = "抽獎批次")]
		public required int LEM_DRAW_BATCH { get; set; }

		/// <summary>建立時間(LEM_CREATE_TIME)</summary>
		[Column("LEM_CREATE_TIME")]
		[Display(Name = "建立時間")]
		public required DateTime LEM_CREATE_TIME { get; set; }

        /// <summary>預計抽獎時間(LEM_DRAW_TIME)</summary>
        [Column("LEM_DRAW_TIME")]
		[Display(Name = "預計抽獎時間")]
		public DateTime? LEM_DRAW_TIME { get; set; }

		/// <summary>開始時間(LEM_START_TIME)</summary>
		[Column("LEM_START_TIME")]
		[Display(Name = "開始時間")]
		public DateTime? LEM_START_TIME { get; set; }

		/// <summary>結束時間(LEM_END_TIME)</summary>
		[Column("LEM_END_TIME")]
		[Display(Name = "結束時間")]
		public DateTime? LEM_END_TIME { get; set; }

		/// <summary>關閉時間(LEM_CLOSE_TIME)</summary>
		[Column("LEM_CLOSE_TIME")]
		[Display(Name = "關閉時間")]
		public DateTime? LEM_CLOSE_TIME { get; set; }
    }
}
