using System;

namespace Escon.SisctNET.Fortes.ContextDataBase
{
    public class SqlContextFortesDb : IDisposable
    {
        protected System.Data.SqlClient.SqlConnection _SqlConnection;
        protected System.Data.SqlClient.SqlCommand _SqlCommand;
        protected System.Data.SqlClient.SqlDataReader _SqlDataReader;
        protected string _ConnectionString;

        public SqlContextFortesDb()
        {

        }

        public void Dispose()
        {
            if (_SqlDataReader != null)
            {
                if (!_SqlDataReader.IsClosed)
                {
                    _SqlDataReader.Close();
                }
                _SqlDataReader = null;
            }

            if (_SqlCommand != null)
            {
                _SqlCommand.Dispose();
                _SqlCommand = null;
            }

            if (_SqlConnection != null)
            {
                _SqlConnection.Dispose();
                _SqlConnection = null;
            }
        }
    }
}
