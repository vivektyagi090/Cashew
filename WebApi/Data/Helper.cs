using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading;
namespace WebApi.Data
{
    public class Helper:IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Helper> _logger;
        private string _connectionString;
        private bool _disposed = false;
        public Helper(IConfiguration configuration, ILogger<Helper> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        }
        //public SqlConnection OpenConnection()
        //{

        //    try
        //    {

        //        if (_connection != null && _connection.State != ConnectionState.Open)
        //        {
        //            _connection.Open();
        //        }
        //        return _connection;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error opening database connection.");
        //        throw new Exception("Failed to open database connection", ex);
        //    }
        //}

        public SqlConnection OpenConnection()
        {
            // 1️⃣ Always create a NEW instance using the connection string
            var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            try
            {
                // 2️⃣ Open it immediately
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                // 3️⃣ If it fails, clean up the object to avoid memory leaks
                conn.Dispose();
                _logger.LogError(ex, "Error opening database connection.");
                throw new Exception("Failed to open database connection", ex);
            }
        }

        //public async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        //if (_connection != null && _connection.State == ConnectionState.Open)
        //        if (_connection == null || _connection.State != ConnectionState.Open)
        //        {

        //            await _connection.OpenAsync(cancellationToken);
        //            _logger.LogDebug("Database connection opened asynchronously");
        //        }
        //        return _connection;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error opening database connection asynchronously.");
        //        throw new Exception("Failed to open database connection asynchronously", ex);
        //    }
        //}


        //private async Task<SqlConnection> OpenConnectionAsync(CancellationToken ct)
        //{
        //    var conn =_connection;
        //    try
        //    {
        //        await conn.OpenAsync(ct);
        //        return conn;
        //    }
        //    catch (Exception ex)
        //    {
        //        conn.Dispose(); // Clean up if opening fails
        //        _logger.LogError(ex, "Failed to open database connection asynchronously");
        //        throw;
        //    }
        //}
        private async Task<SqlConnection> OpenConnectionAsync(CancellationToken ct)
        {
            var conn = new SqlConnection(_connectionString);
            try
            {
                await conn.OpenAsync(ct);
                return conn;
            }
            catch (Exception ex)
            {
                await conn.DisposeAsync();
                _logger.LogError(ex, "Failed to open database connection asynchronously");
                throw;
            }
        }
        private void CloseConnection(SqlConnection Conn)
        {
            try
            {
                if (Conn != null && Conn.State != ConnectionState.Open)
                {
                    Conn.Close();
                    _logger.LogDebug("Database connection closed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing database connection");

            }

        }

        public DataTable FillDataTable (string commandText,CommandType commandType,SqlConnection conn,out string message, params SqlParameter[] parameters)
        {
            message= string.Empty;
            DataTable dataTable = new DataTable();
            try
            {   using var connection = OpenConnection();
                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    if (parameters?.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
               
                _logger.LogError(sqlEx, "SQL Error in FillDataTable: {CommandText}", commandText);
                throw new Exception($"Database error: {message}", sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filling DataTable.");
                message = "Failed to fill DataTable: " + ex.Message;
            }
            finally
            {
              


                // Only close if we opened it
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

               
            }
            return dataTable;
        }

        public async Task<DataTable> FillDataTableAsync(string commandText,CommandType commandType, CancellationToken cancellationToken = default, params SqlParameter[] parameters)
        {
            //Message= string.Empty;
            DataTable dataTable = new DataTable();
            try
            {   using var conn = await OpenConnectionAsync(cancellationToken);
                using (SqlCommand command = new SqlCommand(commandText, conn))
                {      // --- FIX IS HERE ---
                    command.CommandType = commandType;
                    if (parameters?.Length > 0)
                    {    // Clear parameters in case the command object is reused
                        command.Parameters.Clear();
                        command.Parameters.AddRange(parameters);
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        await Task.Run(() => {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }
                            adapter.Fill(dataTable);
                        }
                       , cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filling DataTable asynchronously.");
                throw new Exception("Failed to fill DataTable asynchronously", ex);
            }
            return dataTable;
        }



        // New method: ExecuteScalar with your pattern
        public object? ExecuteScalar(string query)
        {
            SqlCommand? cmd = null;
            var conn = OpenConnection();

            try
            {
                cmd = new SqlCommand(query, conn);
               

                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExecuteScalar: {Query}", query);
                throw new Exception($"Database error: {ex.Message}", ex);
            }
            finally
            {
                cmd?.Dispose();
                CloseConnection(conn);
            }
        }

        // Async version of ExecuteScalar
        public async Task<object?> ExecuteScalarAsync(string query,  CancellationToken cancellationToken = default, params SqlParameter[] parameters)
        {
            SqlCommand? cmd = null;
            var conn = await OpenConnectionAsync(cancellationToken);

            try
            {
                cmd = new SqlCommand(query, conn);
            
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                // cmd.Parameters.AddRange(parameters);
                return await cmd.ExecuteScalarAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExecuteScalarAsync: {Query}", query);
                throw new Exception($"Database error: {ex.Message}", ex);
            }
            finally
            {
                cmd?.Dispose();
                CloseConnection(conn);
            }
        }



        // New method: ExecuteNonQuery
        public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            SqlCommand? cmd = null;
            var conn = OpenConnection();

            try
            {
                cmd = new SqlCommand(query, conn);
               
                cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExecuteNonQuery: {Query}", query);
                throw new Exception($"Database error: {ex.Message}", ex);
            }
            finally
            {
                cmd?.Dispose();
                CloseConnection(conn);
            }
        }
        // Async version of ExecuteNonQuery
        //public async Task<int> ExecuteNonQueryAsync(string query,
        //    int? timeout = null,
        //    CancellationToken cancellationToken = default,
        //    params SqlParameter[] parameters)
        //{
        //    //SqlCommand cmd = null;
        //    var conn = await OpenConnectionAsync(cancellationToken);

        //    try
        //    {
        //        cmd = new SqlCommand(query, conn);
              
        //        cmd.Parameters.AddRange(parameters);

        //        return await cmd.ExecuteNonQueryAsync(cancellationToken);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in ExecuteNonQueryAsync: {Query}", query);
        //        throw new Exception($"Database error: {ex.Message}", ex);
        //    }
        //    finally
        //    {
        //        cmd?.Dispose();
        //        CloseConnection(conn);
        //    }
        //}


        public async Task<int> ExecuteNonQueryAsync(string query, int? timeout = null,CancellationToken cancellationToken = default, params SqlParameter[] parameters)
        {
            // Use 'using' for the connection to ensure it is ALWAYS closed and disposed
            using var conn = await OpenConnectionAsync(cancellationToken);

            try
            {
                using SqlCommand cmd = new SqlCommand(query, conn);

                if (timeout.HasValue)
                    cmd.CommandTimeout = timeout.Value;

                if (parameters != null && parameters.Length > 0)
                {
                    // Clear parameters to prevent "already contained by another SqlCommand" errors
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(parameters);
                }

                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error in ExecuteNonQueryAsync. Query: {Query}", query);
                throw; // Re-throw the original exception to keep the stack trace
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General Error in ExecuteNonQueryAsync: {Query}", query);
                throw;
            }
            // Finally block is not needed for conn/cmd if you use 'using' statements
        }

        // New method: GetDataReader (for large result sets)
        public SqlDataReader GetDataReader(string query, params SqlParameter[] parameters)
        {
            var conn = OpenConnection();
            SqlCommand? cmd = null;

            try
            {
                cmd = new SqlCommand(query, conn);
               
                cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                CloseConnection(conn);
                _logger.LogError(ex, "Error in GetDataReader: {Query}", query);
                throw new Exception($"Database error: {ex.Message}", ex);
            }
        }

        public async Task<SqlDataReader> ExecuteReaderAsync(
    string query,
    CancellationToken cancellationToken = default,
    params SqlParameter[] parameters)
        {
            var conn = await OpenConnectionAsync(cancellationToken);
            SqlCommand? cmd = null;

            try
            {
                cmd = new SqlCommand(query, conn);
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                return await cmd.ExecuteReaderAsync(
                    CommandBehavior.CloseConnection,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                CloseConnection(conn);
                _logger.LogError(ex, "Error in GetDataReaderAsync: {Query}", query);
                throw;
            }
        }


        // New method: ExecuteStoredProcedure
        public DataTable ExecuteStoredProcedure(string procedureName, params SqlParameter[] parameters)
        {
            string message;
            var conn = OpenConnection();

            try
            {
                return FillDataTable(procedureName, CommandType.StoredProcedure, conn, out message, parameters);
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        // Async version of ExecuteStoredProcedure
        public async Task<DataTable> ExecuteStoredProcedureAsync(string procedureName, CommandType commandType,
            CancellationToken cancellationToken = default, params SqlParameter[] parameters)
        {
            return await FillDataTableAsync(procedureName, CommandType.StoredProcedure, 
                cancellationToken, parameters);
        }



        // New method: ExecuteTransaction
        public async Task<bool> ExecuteTransactionAsync(Func<SqlTransaction, Task> action, CancellationToken cancellationToken = default)
        {
            // 1️⃣ 'using' ensures the connection is closed even if an error occurs
            using var conn = await OpenConnectionAsync(cancellationToken);

            // 2️⃣ Start the transaction
            using var transaction = conn.BeginTransaction();

            try
            {
                // 3️⃣ Execute the passed actions (e.g., Save User + Save Token)
                await action(transaction);

                // 4️⃣ If everything is successful, Commit
                await Task.Run(() => transaction.Commit(), cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                // 5️⃣ If anything fails, Rollback
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Transaction rollback failed.");
                }

                _logger.LogError(ex, "Transaction failed and was rolled back.");
                throw; // Re-throw so the calling service knows it failed
            }
            // No finally block needed! 'using' handles the connection disposal.
        }
        //public async Task<bool> ExecuteTransactionAsync(Func<SqlTransaction, Task> action)
        //{
        //    var conn = await OpenConnectionAsync(cancellationToken);
        //    SqlTransaction transaction = null;

        //    try
        //    {
        //        transaction = conn.BeginTransaction();
        //        await action(transaction);
        //        transaction.Commit();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        transaction?.Rollback();
        //        _logger.LogError(ex, "Transaction failed");
        //        throw;
        //    }
        //    finally
        //    {
        //        CloseConnection(conn);
        //    }
        //}



        // New method: Bulk Insert
        public async Task BulkInsertAsync(DataTable dataTable, string tableName,
            int? timeout = null, CancellationToken cancellationToken = default)
        {
            using (var conn = await OpenConnectionAsync(cancellationToken))
            {
                using (var bulkCopy = new SqlBulkCopy(conn))
                {
                    bulkCopy.DestinationTableName = tableName;
                    

                    await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
                }
            }
        }

        public async Task<DataSet> ExecuteQueryAsync(string commandText,CommandType commandType,CancellationToken cancellationToken = default,params SqlParameter[] parameters)
        {
            var dataSet = new DataSet();

            try
            {
                using var conn = await OpenConnectionAsync(cancellationToken);
                using var command = new SqlCommand(commandText, conn)
                {
                    CommandType = commandType,
                    CommandTimeout = 0
                };

                if (parameters?.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                using var adapter = new SqlDataAdapter(command);

                await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    adapter.Fill(dataSet);
                }, cancellationToken);

                return dataSet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query");
                throw;
            }
        }


        // Get database statistics
        public async Task<Dictionary<string, object>> GetDatabaseStatsAsync(CancellationToken cancellationToken = default)
        {
            var stats = new Dictionary<string, object>();

            try
            {
                var query = @"
                    SELECT 
                        DB_NAME() as DatabaseName,
                        COUNT(*) as TotalTables,
                        (SELECT COUNT(*) FROM sys.objects WHERE type = 'P') as TotalStoredProcedures,
                        (SELECT COUNT(*) FROM sys.objects WHERE type = 'U') as TotalUserTables,
                        (SELECT COUNT(*) FROM sys.dm_exec_connections) as ActiveConnections
                ";

                var dt = await FillDataTableAsync(query, CommandType.Text, cancellationToken);

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    foreach (DataColumn column in dt.Columns)
                    {
                        stats[column.ColumnName] = row[column];
                    }
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database statistics");
                return stats;
            }
        }



        // Dispose pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //CloseConnection(_connection);
                    //_connection?.Dispose();
                    //_connection = null;
                }
                _disposed = true;
            }
        }

        ~Helper() {
            Dispose(false);
        }


    }
}
