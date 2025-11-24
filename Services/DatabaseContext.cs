using DbUp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Data;
using System.Security.Claims;
using System.Xml.Linq;

namespace ArchonBot.Services
{
    public class DatabaseContext
    {
        private readonly IDbConnection _dbConn;
        private readonly ILogger _logger;
        private IDbTransaction? _trans;
        public int GlobalTimeOut { get; set; } = 30;
        public DatabaseContext(IDbConnection dbConn, ILogger<DatabaseContext> logger)
        {
            _dbConn = dbConn;
            _logger = logger;
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            Initialize();
        }

        /// <summary>取得資料庫連線</summary>
        public IDbConnection GetConnection()
        {
            EnsureConnectionOpen();
            return _dbConn;
        }

        /// <summary>確保連線處於開啟狀態</summary>
        private void EnsureConnectionOpen()
        {
            if (_dbConn.State == System.Data.ConnectionState.Closed ||
                _dbConn.State == System.Data.ConnectionState.Broken)
            {
                _dbConn.Open();
            }
        }

        /// <summary>初始化進行資料庫更新</summary>
        /// <exception cref="Exception">更新失敗時拋出異常</exception>
        private void Initialize()
        {
            EnsureConnectionOpen();
            var connectionString = _dbConn.ConnectionString;
            var upgrader = DeployChanges.To
                .SqliteDatabase(connectionString)
                .WithScriptsFromFileSystem("SQL")
                .LogToConsole()
                .Build();
            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                _logger.LogCritical("[DbUp] Migration Failed!");
                _logger.LogError($"錯誤訊息：{result.Error}");
                throw new Exception("DB Migration Failed", result.Error);
            }
            _logger.LogInformation("[DbUp] Database updated successfully!");
        }

        /// <summary>透過索引值<paramref name="id"/>直接取得對應的<typeparamref name="T"/>資料</summary>
        /// <typeparam name="T">目標資料</typeparam>
        /// <param name="id">索引值</param>
        /// <returns>索引值<paramref name="id"/>對應的<typeparamref name="T"/>資料，若查無資料則回傳<see langword="null"/>。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public T? Get<T>(long? id) where T : BaseModel
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            var type = typeof(T);
            var table = type.GetTableName();
            var key = type.GetKeyName();
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"{nameof(T)}缺少Table屬性或是未定義Key欄位", nameof(T));
            }
            // -- 防護: 避免有人亂寫屬性造成 SQL 注入 --
            if (!Regex.IsMatch(table, @"^[A-Za-z0-9_]+$") ||
                !Regex.IsMatch(key, @"^[A-Za-z0-9_]+$"))
            {
                throw new ArgumentException("Table 或 Key 名稱不合法");
            }
            var sql = $@"SELECT * FROM {table} WHERE {key} = @ID";
            return QueryFirstOrDefault<T>(sql, new { ID = id });
        }

        /// <summary>儲存資料</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public long Save<T>(long? id, Dictionary<string, object?> data) where T : BaseModel
        {
            //  如果ID不存在則執行新增
            if (id == null) 
            {
                return Insert<T>(data);
            }
            //  如果傳入ID，但該ID不存在資料則拋出例外
            if (Get<T>(id.Value) == null)
            {
                throw new Exception($"{typeof(T).Name} ID={id} 的資料不存在，無法進行更新。");
            }
            //  傳入ID且存在的資料則執行更新
            return Update<T>(id.Value, data);
        }

        public long Insert<T>(Dictionary<string, object?> data) where T : BaseModel
        {
            var type = typeof(T);
            var table = type.GetTableName();
            var columns = data.Keys.ToList();
            var param = new DynamicParameters(data);

            var colList = string.Join(", ", columns.Select(c => $"\"{c}\""));
            var paramList = string.Join(", ", columns.Select(c => $"@{c}"));

            var sql = $"INSERT INTO \"{table}\" ({colList}) VALUES ({paramList});SELECT last_insert_rowid();";

            return ExecuteScalar<long>(sql, param);
        }

        public long Update<T>(long id, Dictionary<string, object?> data) where T : BaseModel
        {
            var type = typeof(T);
            var table = type.GetTableName();
            var key = type.GetKeyName();

            if (data.ContainsKey(key))
            {
                throw new Exception($"Update<{typeof(T).Name}> 的 data 中不可包含主鍵欄位 {key}");
            }
            if (data.ContainsKey("ID"))
            {
                throw new Exception($"Update<{typeof(T).Name}>的 data 中不可包含ID，請透過參數id傳入更新目標的索引值。");
            }

            var param = new DynamicParameters(data);
            param.Add("ID", id);

            var setClause = string.Join(", ", data.Keys.Select(k => $"\"{k}\" = @{k}"));

            var sql = $"UPDATE \"{table}\" SET {setClause} WHERE \"{key}\" = @ID";

            return Execute(sql, param);
        }


        public bool Exists(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            sql = "SELECT COUNT(*) FROM (" + sql + ") T";
            long total = _dbConn.ExecuteScalar<long>(sql, param: param, transaction: _trans, commandTimeout: commandTimeout, commandType: commandType);
            return total > 0;
        }

        public IEnumerable<T> Query<T>(string sql, object? param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _dbConn.Query<T>(sql, param: param, transaction: _trans, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }
        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _dbConn.QueryAsync<T>(sql, param: param, transaction: _trans, commandTimeout: commandTimeout, commandType: commandType);
        }
        public T? QueryFirstOrDefault<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _dbConn.QueryFirstOrDefault<T>(sql, param: param, _trans, commandTimeout: commandTimeout, commandType: commandType);
        }
        public Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _dbConn.QueryFirstOrDefaultAsync<T>(sql, param: param, _trans, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>執行參數化的SQL語句</summary>
        /// <param name="sql">SQL語句</param>
        /// <param name="param">傳入參數</param>
        /// <param name="commandTimeout">超時限制(秒數)</param>
        /// <param name="commandType">SQL類型</param>
        /// <returns>影響的資料行數</returns>
        public long Execute(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _dbConn.Execute(sql, param: param, transaction: _trans, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>執行回傳單一值的參數化SQL語句</summary>
        /// <param name="sql">SQL語句</param>
        /// <param name="param">傳入參數</param>
        /// <param name="commandTimeout">超時限制(秒數)</param>
        /// <param name="commandType">SQL類型</param>
        /// <returns>第一筆回傳結果的首欄資料</returns>
        public object? ExecuteScalar(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _dbConn.ExecuteScalar(sql, param: param, transaction: _trans, commandTimeout: commandTimeout, commandType: commandType);
        }
        public T? ExecuteScalar<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _dbConn.ExecuteScalar<T>(sql, param: param, transaction: _trans, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>開啟一個交易並執行多個SQL</summary>
        /// <param name="action">執行內容</param>
        /// <returns>執行成功與否，若發生錯誤會回傳錯誤訊息</returns>
        public (bool Success, string Message) RunTx(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);
            var result = (Success: false, Message: "");
            EnsureConnectionOpen();
            //  開啟交易
            _trans = _dbConn.BeginTransaction();
            bool rollback = true;   //  預設需要回滾
            try
            {
                //  執行傳入的動作並提交交易
                action();
                _trans.Commit();
                //  若執行成功則不需要回滾
                rollback = false;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                throw;
            }
            finally
            {
                if (rollback)
                {
                    _trans.Rollback();
                }
                _trans.Dispose();
                _trans = null;
            }
            return result;
        }

        /// <summary>開啟一個交易並執行多個SQL</summary>
        /// <param name="action">執行內容</param>
        /// <returns>執行成功與否，若發生錯誤會回傳錯誤訊息</returns>
        public async Task<(bool Success, string Message)> RunTxAsync(Func<Task> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            var result = (Success: false, Message: "");
            EnsureConnectionOpen();
            //  開啟交易
            _trans = _dbConn.BeginTransaction();
            bool rollback = true;   //  預設需要回滾
            try
            {
                //  執行傳入的動作並提交交易
                await action();
                _trans.Commit();
                //  若執行成功則不需要回滾
                rollback = false;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                throw;
            }
            finally
            {
                if (rollback)
                {
                    _trans.Rollback();
                }
                _trans.Dispose();
                _trans = null;
            }
            return result;
        }
    }
}
