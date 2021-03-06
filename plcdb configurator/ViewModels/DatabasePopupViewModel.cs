using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using plcdb.Helpers;
using plcdb_lib.Models;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using plcdb_lib.SQL;

namespace plcdb.ViewModels
{
    public class DatabasePopupViewModel : BaseViewModel
    {
        #region Properties

        private plcdb_lib.Models.Model.DatabasesRow _currentDatabase;
        public plcdb_lib.Models.Model.DatabasesRow CurrentDatabase
        {
            get
            {
                return _currentDatabase;
            }
            set
            {
                if (_currentDatabase != value)
                {
                    _currentDatabase = value;
                    RaisePropertyChanged(() => Name);
                }
                Name = value.Name;
                try
                {
                    SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(value.ConnectionString);
                    Server = b.DataSource;
                    Catalog = b.InitialCatalog;
                    UseWindowsAuthentication = b.IntegratedSecurity;
                    Username = b.UserID;
                    Password = b.Password;
                }
                catch (Exception e)
                {
                    Server = "";
                    Catalog = "";
                    UseWindowsAuthentication = false;
                    Username = "";
                    Password = "";
                }
            }
        }

        #region Name
        public String Name
        {
            get { return CurrentDatabase.Name; }
            set
            {
                if (CurrentDatabase.Name != value)
                {
                    CurrentDatabase.Name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        #endregion

        #region Server
        public String Server
        {
            get 
            {
                return GetSqlConnection().DataSource; 
            }
            set
            {
                SqlConnectionStringBuilder b = GetSqlConnection();
                b.DataSource = value;
                CurrentDatabase.ConnectionString = b.ToString();
                UpdateAvailableDatabases();
            }
        }

        private void UpdateAvailableDatabases()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (sender, e) =>
                {
                     e.Result = SqlHelper.GetAllDatabasesInServer(CurrentDatabase.ConnectionString);
                };

            backgroundWorker.RunWorkerCompleted += (sender, e) =>
                {
                    AvailableDatabases = (List<String>)e.Result;
                };

            backgroundWorker.RunWorkerAsync(CurrentDatabase.ConnectionString);
        }

        #endregion

        #region Catalog
        public String Catalog
        {
            get
            {
                return GetSqlConnection().InitialCatalog;
            }
            set
            {
                if (value != null)
                {
                    SqlConnectionStringBuilder b = GetSqlConnection();
                    b.InitialCatalog = value;
                    CurrentDatabase.ConnectionString = b.ToString();
                }
            
            }
        }

        #endregion

        #region AvailableDatabases
        private List<String> _availableDatabases;
        public List<String> AvailableDatabases
        {
            get
            {
                return _availableDatabases;
            }
            set
            {
                if (value != _availableDatabases)
                {
                    _availableDatabases = value;
                    RaisePropertyChanged(() => AvailableDatabases);
                }
            }
        }

        #endregion

        #region UseWindowsAuthentication
        public bool UseWindowsAuthentication
        {
            get
            {
                return GetSqlConnection().IntegratedSecurity;
            }
            set
            {
                SqlConnectionStringBuilder b = GetSqlConnection();
                b.IntegratedSecurity = value;
                CurrentDatabase.ConnectionString = b.ToString();
                UpdateAvailableDatabases();
                RaisePropertyChanged(() => UseWindowsAuthentication);
            }
        }

        #endregion

        #region Username
        public String Username
        {
            get
            {
                return GetSqlConnection().UserID;
            }
            set
            {
                SqlConnectionStringBuilder b = GetSqlConnection();
                b.UserID = value;
                CurrentDatabase.ConnectionString = b.ToString();
                UpdateAvailableDatabases();
            }
        }

        #endregion

        #region Password
        public String Password
        {
            get
            {
                return GetSqlConnection().Password;
            }
            set
            {
                SqlConnectionStringBuilder b = GetSqlConnection();
                b.Password = value;
                CurrentDatabase.ConnectionString = b.ToString();
                UpdateAvailableDatabases();
            }
        }

        #endregion
        #endregion

        #region Commands

        public ICommand SaveCommand { get { return new DelegateCommand(OnSave); } }
        public ICommand CancelCommand { get { return new DelegateCommand(OnCancel); } }

        
        #endregion

        #region Ctor
        public DatabasePopupViewModel()
        {
            
        }
       
        #endregion

        #region Command Handlers

        private void OnSave()
        {
            CurrentDatabase.AcceptChanges();
        }

        private void OnCancel()
        {
            CurrentDatabase.RejectChanges();
        }
        #endregion

        private string BuildConnectionString()
        {
            SqlConnectionStringBuilder b = new SqlConnectionStringBuilder();
            b.DataSource = Server;
            b.InitialCatalog = Catalog;
            b.IntegratedSecurity = UseWindowsAuthentication;
            b.UserID = Username;
            b.Password = Password;
            return b.ToString();
        }

        private SqlConnectionStringBuilder GetSqlConnection()
        {
            try
            {
                SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(CurrentDatabase.ConnectionString);
                return b;
            }
            catch (Exception e)
            {
                return new SqlConnectionStringBuilder();
            }
        }
    }
}