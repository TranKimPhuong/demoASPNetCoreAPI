using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace WebApi.CommonCore.Helper
{
    public class BaseDao : IDisposable
    {
        static ILog LOGGER = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static BindingFlags BINDING_ATTR = (BindingFlags.DeclaredOnly
                                                | BindingFlags.Instance
                                                | BindingFlags.Public
                                                | BindingFlags.GetField
                                                | BindingFlags.SetField);

        protected SqlConnection _connection { get; set; }

        #region Constructors
        public BaseDao() { }
        public BaseDao(string connectionString)
        {
            this.setConnectionString(connectionString);
        }
        public BaseDao(string connectionString, string username, string password)
        {
            this.setConnectionString(connectionString, username, password);
        }

        public BaseDao(SqlConnection connection)
        {
            this._connection = connection;
        }

        protected void setConnectionString(string connectionString)
        {
            this._connection = new SqlConnection(connectionString);
        }

        protected void setConnectionString(string connectionString, string username, string password)
        {
            var securePassword = new System.Security.SecureString();
            foreach (char character in password)
            {
                securePassword.AppendChar(character);
            }
            securePassword.MakeReadOnly();
            var credentials = new SqlCredential(username, securePassword);

            this._connection = new SqlConnection(connectionString, credentials);
        }

        protected SqlConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        protected SqlTransaction GetBeginTransaction()
        {
            return this._connection.BeginTransaction();
        }

        protected SqlTransaction GetBeginTransaction(string transactionName)
        {
            return this._connection.BeginTransaction(transactionName);
        }
        protected SqlTransaction GetBeginTransaction(IsolationLevel level)
        {
            return this._connection.BeginTransaction(level);
        }
        protected SqlTransaction GetBeginTransaction(IsolationLevel level, string transactionName)
        {
            return this._connection.BeginTransaction(level, transactionName);
        }
        #endregion

        #region ADO operations        
        /// <summary>
        /// Execute and return SqlDataReader
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        protected SqlDataReader GetReader(string sqlStatement, IDictionary<string, object> paramIN = null, CommandType cmdType = CommandType.Text)
        {
            return this.GetCommand(sqlStatement, paramIN, cmdType).ExecuteReader();
        }

        public SqlDataReader ProcGetReader(string sqlStatement, IDictionary<string, object> paramIN = null)
        {
            return this.GetCommandProc(sqlStatement, paramIN).ExecuteReader();
        }



        /// <summary>
        /// Execute and return the list object T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        protected IEnumerable<T> ExecuteForListObject<T>(string sqlStatement, IDictionary<string, object> paramIN = null, CommandType cmdType = CommandType.Text) where T : class, new()
        {
            using (SqlDataReader reader = this.GetReader(sqlStatement, paramIN, cmdType))
            {
                if (reader.HasRows)
                {
                    var objectType = typeof(T);
                    while (reader.Read())
                    {
                        T item = new T();
                        for (int columnIndex = 0; columnIndex < reader.FieldCount; columnIndex++)
                        {
                            PropertyInfo objectProperty = objectType.GetProperties(BINDING_ATTR)
                                                            .Where(p => (p.CanRead == true && p.CanWrite == true
                                                                        && p.Name != null && p.Name.Equals(reader.GetName(columnIndex))))
                                                            .FirstOrDefault();
                            if (objectProperty != null)
                            {
                                object dataValue = reader.GetValue(columnIndex);
                                objectProperty.SetValue(item, DBNull.Value.Equals(dataValue) ? null : dataValue, null);
                            }
                        }
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// Execute and returns an object DataTable
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        protected DataTable ExecuteForDataTable(string sqlStatement, IDictionary<string, object> paramIN = null, CommandType cmdType = CommandType.Text)
        {
            DataTable dataTable = new DataTable();
            this.GetAdapter(sqlStatement, paramIN, cmdType).Fill(dataTable);
            return dataTable;
        }

        /// <summary>
        /// Execute and return a data set
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        protected DataSet ExecuteForDataSet(string sqlStatement, IDictionary<string, object> paramIN = null, CommandType cmdType = CommandType.Text)
        {
            DataSet dataSet = new DataSet();
            this.GetAdapter(sqlStatement, paramIN, cmdType).Fill(dataSet);
            return dataSet;
        }

        /// <summary>
        /// Execute and return object scalar
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        protected object ExecuteForScalar(string sqlStatement, IDictionary<string, object> paramIN = null, CommandType cmdType = CommandType.Text)
        {
            object result = this.GetCommand(sqlStatement, paramIN, cmdType).ExecuteScalar();
            return result;
        }
        const int SqlCommandDefaultTimeOut = 30;
        int GetSqlCommandTimeOut()
        {
            try
            {
                string configValue = ConfigHelper.GetString("SqlCommand.CommandTimeOut");
                var timeout = 0;
                if (int.TryParse(configValue, out timeout))
                {
                    return timeout > 0 ? timeout : SqlCommandDefaultTimeOut;
                }
                return SqlCommandDefaultTimeOut;
            }
            catch (Exception)
            {
                return SqlCommandDefaultTimeOut;
            }
        }
        /// <summary>
        /// Prepares SqlCommand for stored procedure with oprional paramIN list
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        protected SqlCommand GetCommand(string sqlStatement, IEnumerable<KeyValuePair<string, object>> paramIN = null, CommandType cmdType = CommandType.Text)
        {
            SqlCommand command = new SqlCommand(sqlStatement);
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            command.Connection = this._connection;
            if (paramIN != null && paramIN.Any())
            {
                foreach (var param in paramIN)
                {
                    command.Parameters.Add(this.GetSqlParameter(param.Key, param.Value));
                }
            }
            command.CommandType = cmdType;
            command.CommandTimeout = GetSqlCommandTimeOut();
            return command;
        }

        protected SqlCommand GetCommandProc(string sqlStatement, IEnumerable<KeyValuePair<string, object>> paramIN = null, IEnumerable<KeyValuePair<string, TypeCode>> paramOUT = null)
        {
            SqlCommand command = new SqlCommand(sqlStatement);
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            command.CommandTimeout = GetSqlCommandTimeOut();
            command.Connection = this._connection;
            command.CommandType = CommandType.StoredProcedure;

            //Parameter IN
            if (paramIN != null && paramIN.Any())
            {
                foreach (var param in paramIN)
                {
                    command.Parameters.Add(this.GetSqlParameter(param.Key, param.Value));
                }
            }

            //Parameter OUT
            if (paramOUT != null && paramOUT.Any())
            {
                foreach (var param in paramOUT)
                {
                    command.Parameters.Add(this.GetSqlParameterOUT(param.Key, param.Value, ParameterDirection.Output));
                }
            }

            return command;
        }


        /// <summary>
        /// Creates SqlAdapter instance for the stored procedure with optional paramets
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        private SqlDataAdapter GetAdapter(string sqlStatement, IEnumerable<KeyValuePair<string, object>> paramIN = null, CommandType cmdType = CommandType.Text)
        {
            return new SqlDataAdapter(this.GetCommand(sqlStatement, paramIN, cmdType));
        }

        /// <summary>
        /// Creates SqlAdapter instance for the command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private SqlDataAdapter GetAdapter(SqlCommand command)
        {
            return new SqlDataAdapter(command);
        }

        private SqlParameter GetSqlParameter(string name, object Value)
        {
            SqlParameter param = new SqlParameter();// DBNull.Value;
            param.ParameterName = name;
            try
            {
                if (Value != null)
                {
                    TypeCode typeCode = Type.GetTypeCode(Value.GetType());
                    switch (typeCode)
                    {
                        case TypeCode.Boolean:
                            param.SqlDbType = SqlDbType.Bit;
                            param.Value = Convert.ToBoolean(Value);
                            break;
                        case TypeCode.Byte:
                            param.SqlDbType = SqlDbType.TinyInt;
                            param.Value = Convert.ToByte(Value);
                            break;
                        case TypeCode.Char:
                            param.SqlDbType = SqlDbType.Char;
                            param.Value = Convert.ToChar(Value);
                            break;
                        case TypeCode.DateTime:
                            param.SqlDbType = SqlDbType.DateTime;
                            param.Value = Convert.ToDateTime(Value);
                            break;
                        case TypeCode.Decimal:
                            param.SqlDbType = SqlDbType.Decimal;
                            param.Value = Convert.ToDecimal(Value);
                            break;
                        case TypeCode.Double:
                            param.SqlDbType = SqlDbType.Float;
                            param.Value = Convert.ToDouble(Value);
                            break;
                        case TypeCode.Int16:
                            param.SqlDbType = SqlDbType.SmallInt;
                            param.Value = Convert.ToInt16(Value);
                            break;
                        case TypeCode.Int32:
                            param.SqlDbType = SqlDbType.Int;
                            param.Value = Convert.ToInt32(Value);
                            break;
                        case TypeCode.Int64:
                            param.SqlDbType = SqlDbType.BigInt;
                            param.Value = Convert.ToInt64(Value);
                            break;
                        case TypeCode.SByte:
                            param.SqlDbType = SqlDbType.Binary;
                            param.Value = Convert.ToSByte(Value);
                            break;
                        case TypeCode.Single:
                            param.SqlDbType = SqlDbType.Real;
                            param.Value = Convert.ToSingle(Value);
                            break;
                        case TypeCode.String:
                            //param.SqlDbType = SqlDbType.Text;
                            param.Value = Convert.ToString(Value);
                            break;
                        case TypeCode.UInt16:
                            param.SqlDbType = SqlDbType.SmallInt;
                            param.Value = Convert.ToUInt16(Value);
                            break;
                        case TypeCode.UInt32:
                            param.SqlDbType = SqlDbType.Int;
                            param.Value = Convert.ToUInt32(Value);
                            break;
                        case TypeCode.UInt64:
                            param.SqlDbType = SqlDbType.BigInt;
                            param.Value = Convert.ToUInt64(Value);
                            break;
                        case TypeCode.Object:
                            if (Value is byte[]) param.SqlDbType = SqlDbType.VarBinary;
                            else if (Value is DataTable) param.SqlDbType = SqlDbType.Structured;
                            param.Value = Value;
                            break;
                        default:
                            param.Value = DBNull.Value;
                            break;
                    }
                }
                else param.Value = DBNull.Value;
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex);
                param.Value = DBNull.Value;
            }

            return param;
        }

        private SqlParameter GetSqlParameterOUT(string name, TypeCode typeCode, ParameterDirection paramDirection)
        {
            if (string.IsNullOrEmpty(name)) return null;

            SqlParameter param = new SqlParameter();
            param.ParameterName = name;
            param.Direction = paramDirection;
            try
            {
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        param.SqlDbType = SqlDbType.Bit;
                        break;
                    case TypeCode.Byte:
                        param.SqlDbType = SqlDbType.TinyInt;
                        break;
                    case TypeCode.Char:
                        param.SqlDbType = SqlDbType.Char;
                        break;
                    case TypeCode.DateTime:
                        param.SqlDbType = SqlDbType.DateTime;
                        break;
                    case TypeCode.Decimal:
                        param.SqlDbType = SqlDbType.Decimal;
                        break;
                    case TypeCode.Double:
                        param.SqlDbType = SqlDbType.Float;
                        break;
                    case TypeCode.Int16:
                        param.SqlDbType = SqlDbType.SmallInt;
                        break;
                    case TypeCode.Int32:
                        param.SqlDbType = SqlDbType.Int;
                        break;
                    case TypeCode.Int64:
                        param.SqlDbType = SqlDbType.BigInt;
                        break;
                    case TypeCode.SByte:
                        param.SqlDbType = SqlDbType.Binary;
                        break;
                    case TypeCode.Single:
                        param.SqlDbType = SqlDbType.Real;
                        break;
                    case TypeCode.String:
                        //param.SqlDbType = SqlDbType.Text;
                        break;
                    case TypeCode.UInt16:
                        param.SqlDbType = SqlDbType.SmallInt;
                        break;
                    case TypeCode.UInt32:
                        param.SqlDbType = SqlDbType.Int;
                        break;
                    case TypeCode.UInt64:
                        param.SqlDbType = SqlDbType.BigInt;
                        break;
                }
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex);
            }

            return param;
        }

        #endregion

        #region Methods execute SQL statement text
        /// <summary>
        /// Execute SQL statement text and reutrn a object POCO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        public T QueryForObject<T>(string sqlStatement, IDictionary<string, object> paramIN = null) where T : class, new()
        {
            IEnumerable<T> ListRecords = this.ExecuteForListObject<T>(sqlStatement, paramIN, CommandType.Text);
            if (ListRecords != null) return ListRecords.FirstOrDefault<T>();
            return null;
        }

        /// <summary>
        /// Execute SQL statement text and return scalar object
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        public object QueryForScalar(string sqlStatement, IDictionary<string, object> paramIN = null)
        {
            object result = this.GetCommand(sqlStatement, paramIN, CommandType.Text).ExecuteScalar();
            return result;
        }

        /// <summary>
        /// Execute SQL statement text and return the list of object POCO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        public IEnumerable<T> QueryForListObject<T>(string sqlStatement, IDictionary<string, object> paramIN = null) where T : class, new()
        {
            return this.ExecuteForListObject<T>(sqlStatement, paramIN, CommandType.Text);
        }

        /// <summary>
        /// Execute SQL statement text and return a object DataTable
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        public DataTable QueryForDataTable(string sqlStatement, IDictionary<string, object> paramIN = null)
        {
            return this.ExecuteForDataTable(sqlStatement, paramIN, CommandType.Text);
        }

        /// <summary>
        /// Execute SQL statement text to modify and return the number of record affected
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <param name="returnIdentity"></param>
        /// <returns></returns>
        public int QueryUpdate(string sqlStatement, IDictionary<string, object> paramIN = null, bool returnIdentity = false)
        {
            int nAffected = 0;
            string sql = sqlStatement;
            if (string.IsNullOrEmpty(sql)) return nAffected;

            if (returnIdentity == true) sql = string.Format("{0}; SELECT SCOPE_IDENTITY()", sqlStatement);
            using (SqlCommand cmd = this.GetCommand(sql, paramIN, CommandType.Text))
            {
                if (returnIdentity == true)
                {
                    object objValue = cmd.ExecuteScalar();
                    if (objValue != null) nAffected = Convert.ToInt32(objValue);
                }
                else
                {
                    nAffected = cmd.ExecuteNonQuery();
                }
            }

            return nAffected;
        }
        #endregion

        #region Methods execute SQL statement procedure

        public IDictionary<string, object> ProcForDictionary(string sqlStatement, IDictionary<string, object> paramIN = null, IDictionary<string, TypeCode> paramOUT = null)
        {
            IDictionary<string, object> Result = new Dictionary<string, object>();
            using (SqlDataAdapter sqlData = new SqlDataAdapter(this.GetCommandProc(sqlStatement, paramIN, paramOUT)))
            {
                DataSet dataSet = new DataSet();
                int nCount = sqlData.Fill(dataSet);
                if (nCount > 0) Result["DATA"] = dataSet;

                IDataParameter[] aResultParams = sqlData.GetFillParameters();
                if (paramOUT != null && paramOUT.Count > 0 && aResultParams != null && aResultParams.Length > 0)
                {
                    IList<IDataParameter> ListDataOut = aResultParams.Where(p => paramOUT.Keys.Contains(p.ParameterName)).ToList();
                    if (ListDataOut != null && ListDataOut.Count > 0)
                    {
                        foreach (IDataParameter DataOut in ListDataOut)
                        {
                            Result[DataOut.ParameterName] = DataOut.Value;
                        }
                    }
                }
            }

            return Result;
        }

        /// <summary>
        /// Execute SQL statement Procedure and return a object DataTable
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        public DataTable ProcForDataTable(string sqlStatement, IDictionary<string, object> paramIN = null)
        {
            return this.ExecuteForDataTable(sqlStatement, paramIN, CommandType.StoredProcedure);
        }
        /// <summary>
        /// Execute SQL statement Procedure and return the list of object POCO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        public IEnumerable<T> ProcForListObject<T>(string sqlStatement, IDictionary<string, object> paramIN = null) where T : class, new()
        {
            return this.ExecuteForListObject<T>(sqlStatement, paramIN, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Execute SQL statement Procedure and reutrn a object POCO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        public T ProcForObject<T>(string sqlStatement, IDictionary<string, object> paramIN = null) where T : class, new()
        {
            IEnumerable<T> ListRecords = this.ExecuteForListObject<T>(sqlStatement, paramIN, CommandType.StoredProcedure);
            if (ListRecords != null) return ListRecords.FirstOrDefault<T>();
            return null;
        }

        /// <summary>
        /// Execute SQL statement Procedure and return scalar object
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="paramIN"></param>
        /// <returns></returns>
        public object ProcForScalar(string sqlStatement, IDictionary<string, object> paramIN = null)
        {
            object result = this.GetCommand(sqlStatement, paramIN, CommandType.StoredProcedure).ExecuteScalar();
            return result;
        }


        #endregion

        #region IDisposable Support
        /// <summary>
        /// Dispose DataAccessLayer instance and closes database connection
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._connection != null && this._connection.State != ConnectionState.Closed)
                {
                    this._connection.Close();
                    this._connection.Dispose();

                    this._connection = null;
                }
            }
        }
        #endregion

    }
}