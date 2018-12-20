using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace DataAccess
{
    public abstract class Abstract : IDisposable
    {
        private string _cnnString;
        private string _providerName;
        private DbProviderFactory _factory;
        private DbConnection _connection;

        //public Abstract()
        //{
        //    Log("Instanced");
        //}

        /// <summary>
        /// Implementacion por defecto
        /// Lee la cadena de conn del app.config
        /// Obtiene un factory
        /// construye la conn y la abre
        /// </summary>
        /// <param name="connectionStringName"></param>
        public void Init(string connectionStringName, bool open = false)
        {
            Log("Instanced");

            var _cnAppCnfg = ConfigurationManager.ConnectionStrings[connectionStringName];
            _cnnString = _cnAppCnfg.ConnectionString;
            _providerName = _cnAppCnfg.ProviderName;
            _factory = DbProviderFactories.GetFactory(_providerName);
            _connection = BuildConnection(_cnnString);
            if (null == _connection) throw new Exception("Could not build connection");
            if (open)
            {
                _connection.Open();
                Log("Connection init and opened");
            }
        }

        /// <summary>
        /// Implementacion por defecto
        /// Lee la cadena de conn del app.config
        /// Obtiene un factory
        /// construye la conn y la abre
        /// </summary>
        /// <param name="connectionStringName"></param>
        public void Init(string connectionString, string providerName, bool open = false)
        {
            _cnnString = connectionString;
            _providerName = providerName;
            _factory = DbProviderFactories.GetFactory(_providerName);
            _connection = BuildConnection(_cnnString);
            if (null == _connection) throw new Exception("Could not build connection");
            if (open)
            {
                _connection.Open();
                Log("Connection init and opened");
            }
        }

        /// <summary>
        /// Para implementacion derivada
        /// </summary>
        public abstract void Init();

        public DbConnection BuildConnection()
        {
            var _connection = _factory.CreateConnection();
            _connection.ConnectionString = _cnnString;
            return _connection;
        }

        public DbConnection BuildConnection(string _cnString)
        {
            var _connection = _factory.CreateConnection();
            _connection.ConnectionString = _cnString;
            return _connection;
        }

        public abstract DbConnection BuildConnection(DbProviderFactory _factory);

        public IDbCommand GetCommand()
        {
            Log("Creating command");
            return _connection.CreateCommand();
        }

        public abstract DbCommand GetCommand(DbConnection _connection);


        private static void Log(string msg)
        {
            Debug.WriteLine($"DAL.Abstract: {msg}");
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
            Log("Disposed");
            return;
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are.
        ~Abstract()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_factory != null)
                {
                    _factory = null;
                }
                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }
    }
}
