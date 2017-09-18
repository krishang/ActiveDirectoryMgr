
//*********************************************************************
// Programme = DBObjects
// Description = Create a connection with database
// Created By = Ibuy Spy portal class
// Start Date = 13-03-2004
// Last updated 03-04-2007
//Revised by Krishan 
//WARNING-DO NOT HANDLE ERRORS INSIDE THESE METHOD CALLS.
//ERRORS SHOULD BE PROPOGATED UPWARDS AND HANDLED IN THE CALLING CODE.
//*********************************************************************
using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration ;
using System.Data.SqlTypes;



	/// <summary>
	/// Summary description for DBObjects.
	/// </summary>
	public class DBObjects
	{
//		private SqlConnection myConnection;
//		private SqlCommand myCommand;
//		private SqlDataReader myDatareader;
		
		

		/// <summary>
		/// Runs a stored procedure, can only be called by those classes deriving
		/// from this base. It returns an integer indicating the return value of the
		/// stored procedure, and also returns the value of the RowsAffected aspect
		/// of the stored procedure that is returned by the ExecuteNonQuery method.
		/// </summary>
		/// <param name="storedProcName">Name of the stored procedure</param>
		/// <param name="parameters">Array of IDataParameter objects</param>
		/// <param name="rowsAffected">Number of rows affected by the stored procedure.</param>
		/// <returns>An integer indicating return value of the stored procedure</returns>
		public static int RunProcedure(string ConnectionString, string storedProcName, IDataParameter[] parameters, out int rowsAffected )
		{
				SqlConnection myConnection;
				int result;
				myConnection=new SqlConnection(ConnectionString);
				myConnection.Open();
				SqlCommand command = BuildIntCommand(myConnection, storedProcName, parameters );
				rowsAffected = command.ExecuteNonQuery();
				result = (int)command.Parameters["ReturnValue"].Value;
				myConnection.Close();
				return result;
		}
		

		/// <summary>
		/// Will run a stored procedure, can only be called by those classes deriving
		/// from this base. It returns a SqlDataReader containing the result of the stored
		/// procedure.
		/// </summary>
		/// <param name="storedProcName">Name of the stored procedure</param>
		/// <param name="parameters">Array of parameters to be passed to the procedure</param>
		/// <returns>A newly instantiated SqlDataReader object</returns>


		/// <summary>
		/// Creates a DataSet by running the stored procedure and placing the results
		/// of the query/proc into the given tablename.
		/// </summary>
		/// <param name="storedProcName"></param>
		/// <param name="parameters"></param>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public static DataSet RunProcedure(string ConnectionString, string storedProcName, IDataParameter[] parameters, string tableName )
		{
			SqlConnection myConnection;
			

			DataSet dataSet = new DataSet();
			myConnection=new SqlConnection(ConnectionString);
			SqlDataAdapter sqlDA = new SqlDataAdapter();
			sqlDA.SelectCommand = BuildQueryCommand(myConnection, storedProcName, parameters );
			sqlDA.Fill( dataSet, tableName );
			myConnection.Close();
            //if (myConnection.State != ConnectionState.Open )
            //{int X = 1;}
			return dataSet;
		}



		/// <summary>
		/// Builds a SqlCommand designed to return a SqlDataReader, and not
		/// an actual integer value.
		/// </summary>
		/// <param name="storedProcName">Name of the stored procedure</param>
		/// <param name="parameters">Array of IDataParameter objects</param>
		/// <returns></returns>
		private static SqlCommand BuildQueryCommand(SqlConnection CNN, string storedProcName, IDataParameter[] parameters)
		{
			
			
			
			
			SqlCommand command = new SqlCommand( storedProcName, CNN);
			command.CommandType = CommandType.StoredProcedure;

			foreach (SqlParameter parameter in parameters)
			{
				command.Parameters.Add( parameter );
			}

			return command;

		}

		/// <summary>
		/// Private routine allowed only by this base class, it automates the task
		/// of building a SqlCommand object designed to obtain a return value from
		/// the stored procedure.
		/// </summary>
		/// <param name="storedProcName">Name of the stored procedure in the DB, eg. sp_DoTask</param>
		/// <param name="parameters">Array of IDataParameter objects containing parameters to the stored proc</param>
		/// <returns>Newly instantiated SqlCommand instance</returns>
		public static SqlCommand BuildIntCommand(SqlConnection CNN, string storedProcName, IDataParameter[] parameters)
		{
			
			SqlCommand command = BuildQueryCommand(CNN, storedProcName, parameters );			

			command.Parameters.Add( new SqlParameter ( "ReturnValue",
				SqlDbType.Int,
				4, /* Size */
				ParameterDirection.ReturnValue,
				false, /* is nullable */
				0, /* byte precision */
				0, /* byte scale */
				string.Empty,
				DataRowVersion.Default,
				null ));
			return command;
		}

		// Excecute the SQL
		/// <summary>
		/// Executes a sql statement using the given connection string.
		/// The underlying connection is  closed once the execution is done.
		/// 
		/// </summary>
		public static bool ExecuteSql (string ConnectionString, string sql)
		{
			SqlConnection myConnection;
			SqlCommand myCommand;
			myConnection=new SqlConnection(ConnectionString);
			myConnection.Open();
             
			myCommand = myConnection.CreateCommand ();
			myCommand.CommandText = sql;
            
			myCommand.ExecuteNonQuery();
			myConnection.Close();
			return true;
		}



		/// <summary>
		/// Executes a sql statement when a Connection object is passed.
		/// Dont forget to close the connection in the calling code!
		/// 
		/// </summary>
		public static bool ExecuteSql(SqlConnection CNN,string sql)
		{
			SqlCommand myCommand=CNN.CreateCommand();
			myCommand.CommandText=sql;
			myCommand.ExecuteNonQuery();
			return true;
		}


        /// <summary>
        /// Executes a sql statement when a Connection object is passed.
        /// Dont forget to close the connection in the calling code!
        /// 
        /// </summary>
        public static bool ExecuteSql(SqlConnection CNN,SqlTransaction TR, string sql)
        {
            SqlCommand myCommand = CNN.CreateCommand();
            myCommand.CommandText = sql;
            myCommand.Transaction = TR;
            myCommand.ExecuteNonQuery();
            return true;
        }
		

		/// <summary>
		/// Executes a sql statement when a Connection object is passed and returns a DataReader
		/// Dont forget to close the connection & the datareader in the calling code!
		/// 
		/// </summary>
		public static SqlDataReader GetDataReader(SqlConnection CNN,string sql)
		{
			SqlCommand myCommand=CNN.CreateCommand();
			myCommand.CommandText=sql;
			SqlDataReader DR=myCommand.ExecuteReader();
			return DR;
		}
		public static SqlDataReader GetDataReader(SqlConnection CNN,SqlTransaction tran, string sql)
		{
			SqlCommand myCommand=CNN.CreateCommand();
			myCommand.Transaction=tran;
			myCommand.CommandText=sql;
			SqlDataReader DR=myCommand.ExecuteReader();
			return DR;
		}

		/// <summary>
		/// This method updates the base table(in the database) with the data in the source table.
		/// Note that the schema of the base table and the source table must be the same.
		///
		/// </summary>
		public static int UpdateTable(string ConnectionString,string TableName,DataTable SourceData)
		{
			SqlConnection CNN=null;
			SqlDataAdapter DA=null;
			DataRow dRow=null;
			string SelectCommand ="Select * from " +TableName;
			try
			{
				CNN=new SqlConnection(ConnectionString);
				DA=new SqlDataAdapter(SelectCommand,CNN);
				DA.MissingSchemaAction=MissingSchemaAction.AddWithKey;
				SqlCommandBuilder sqlCmd=new SqlCommandBuilder(DA);
				sqlCmd.QuotePrefix="[";
				sqlCmd.QuoteSuffix="]";

				CNN.Open();
				
				DataSet ds=new DataSet();
				DA.Fill(ds,"tblTemp");
				
				ds.Tables[0].BeginLoadData();
				for(int i=0;i<SourceData.Rows.Count;i++)
				{
					dRow=SourceData.Rows[i];
					ds.Tables[0].LoadDataRow(dRow.ItemArray,false);
				}
				ds.Tables[0].EndLoadData();
				DA.Update(ds,"tblTemp");
			
				return 0;
			}
			catch(Exception e)
			{
                return -1;
			}
			finally
			{
				if (CNN!=null)
				{
					if (CNN.State!=ConnectionState.Closed)
						CNN.Close();
				}
			}
			
		}

		/// <summary>
		/// This method updates the base table(in the database) with the data in the source table.
		/// Note that the schema of the base table and the source table must be the same.
		/// This version supports transactions.
		///
		/// </summary>
		/// 
		public static int UpdateTable(SqlConnection CNN,SqlTransaction TR,string TableName,DataTable SourceData)
		{
			
			SqlDataAdapter DA=null;
			DataRow dRow=null;
			SqlCommand CMD=null;
			string SelectCommand ="Select * from " +TableName;
			try
			{
				CMD=CNN.CreateCommand();
				CMD.CommandText=SelectCommand;
				CMD.Transaction=TR;

				DA=new SqlDataAdapter(CMD);

				DA.MissingSchemaAction=MissingSchemaAction.AddWithKey;
				SqlCommandBuilder sqlCmd=new SqlCommandBuilder(DA);
				sqlCmd.QuotePrefix="[";
				sqlCmd.QuoteSuffix="]";

				DataSet ds=new DataSet();
				DA.Fill(ds,"tblTemp");

				sqlCmd.GetUpdateCommand().Transaction =TR;
				sqlCmd.GetDeleteCommand().Transaction =TR;
				sqlCmd.GetInsertCommand().Transaction =TR;
				
				ds.Tables[0].BeginLoadData();
				for(int i=0;i<SourceData.Rows.Count;i++)
				{
					dRow=SourceData.Rows[i];
					ds.Tables[0].LoadDataRow(dRow.ItemArray,false);
				}
				ds.Tables[0].EndLoadData();
				DA.Update(ds,"tblTemp");
			
				return 0;
			}
			
		    catch 
	        {
                return -1;
            }
        }
			
		
		
		
		/// <summary>
		/// returns true if the schemas of the 2 tables are the same
		///Note-The columns must be in the same order in both tables for this to return true! 
		///Function checks column name only !! assumes DataType is the same
		///
		/// </summary>
        public static bool CompareDataTableSchema(DataTable table1, DataTable table2)
        {
            bool bIsSame = true;
            int iColCount = 0;
            try
            {
                if (table1.Columns.Count == table2.Columns.Count)
                {
                    iColCount = table1.Columns.Count;
                    for (int i = 0; i < iColCount; i++)
                    {
                        if (table1.Columns[i].ColumnName.ToUpper() != table2.Columns[i].ColumnName.ToUpper())
                        {
                            bIsSame = false;
                            break;
                        }
                    }
                }
                else
                    bIsSame = false;
                return bIsSame;
            }
            catch
            {
                return false;
            }
        }
			
		
		/// <summary>
		/// returns an open Connection object.Dont forget to close it once
		/// you are done with it!
		/// 
		/// </summary>
		public static SqlConnection GetSQLConnection(string ConnectionString)
		{
			SqlConnection myConnection;
			myConnection=new SqlConnection(ConnectionString);
			myConnection.Open();
			return myConnection;
		}

		
		/// <summary>
		/// returns a datatable collection which can contain one or more tables;
		/// pass the sql statements seperated with a ";".This method call is resource
		/// heavy abd should be used sparingly.Use the much lighter GetDataTable where possible.
		/// 
		/// </summary>
		public static DataSet  GetDataTables(string ConnectionString, string sSql)
		{
			SqlConnection myConnection;
			SqlCommand myCommand;
			myConnection=new SqlConnection(ConnectionString);;	//Open DB
			myConnection.Open();
			myCommand= myConnection.CreateCommand(); //Create command
			myCommand.CommandText=sSql;	//Set command text

			SqlDataAdapter dAdapter=new SqlDataAdapter();	//Create the data adapter
			dAdapter.SelectCommand=myCommand;

			DataSet DSet=new DataSet();		//Create the data set
			dAdapter.Fill(DSet);			//Fill data set with the data
			myConnection.Close();			//Close the connection
			return DSet; 
		}


		/// <summary>
		/// returns a single DataTable object when the sql statement & connection string
		/// is passed respectively.Much lighter and faster than the GetDataTables method.
		/// 
		/// </summary>
		public static  System.Data.DataTable  GetDataTable(string Sql,string ConnectionString) 
		{
			//This function returns a datatable when executed
			//Used in conjunction with the datagrid control
			//Needs to have the connection open otherwise will throw an error
			//this function is significantly faster and uses less overhead than   GetDataTables
			//method
			System.Data.DataTable dTable=null;
			SqlDataReader DR=null;
			
			System.Data.DataRow dRow=null;
			
			SqlConnection CNN=null;
			
				CNN=DBObjects.GetSQLConnection(ConnectionString );
				DR=DBObjects.GetDataReader(CNN,Sql);
				dTable=new System.Data.DataTable("Table");

				for (int i=0;i<DR.FieldCount ;i++)
				{
					dTable.Columns.Add(DR.GetName(i),DR.GetFieldType(i));
				}
			
				while(DR.Read())
				{
					dRow=dTable.NewRow();
					for (int i=0;i<DR.FieldCount;i++)
					{
					
						dRow[i]=DR[i];
					}
					dTable.Rows.Add(dRow);					
				}
				DR.Close(); //need to close the DataReader
				return dTable;
			
			
		}

		

	}

