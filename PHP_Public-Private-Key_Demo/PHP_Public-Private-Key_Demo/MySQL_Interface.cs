using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

/* Dan Berkowitz
 * Buildingtents.com
 * github.com/daberkow
 */

namespace PHP_PublicPrivateKey_Demo
{
	public class MySQL_Interface
	{
		public MySQL_Interface ()
		{
			//empty constructor
		}
		
		//this is for a query that shouldnt return anything, suchas a table creation
		public bool mysql_nonquery(string passed_username, string passed_password, string passed_db, string passed_command)
        {
            bool status = false;
            if (passed_command != "")
            {
                string MyConString = "SERVER=localhost;" +
                    "DATABASE=" + passed_db + ";" +
                    "UID=" + passed_username + ";" +
                    "PASSWORD=" + passed_password + ";";
                MySqlConnection connection = new MySqlConnection(MyConString);
                MySqlCommand command = new MySqlCommand(passed_command);

                try
                {
                    connection.Open();
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                    status = true;
                }
                catch
                {
                    status = false;
                }
                finally
                {
                    connection.Close();
                }
            }
            return status;
        }
		
		//query and select a cell and get that value
        public string mysql_select_cell(string passed_username, string passed_password, string passed_db, string query, string row)
        {
            string return_data = "";
            string MyConString = "SERVER=localhost;" +
                    "DATABASE=" + passed_db + ";" +
                    "UID=" + passed_username + ";" +
                    "PASSWORD=" + passed_password + ";";
            MySqlConnection connection = new MySqlConnection(MyConString);

            try
            {
                MySqlCommand command = connection.CreateCommand();
                MySqlDataReader Reader;
                command.CommandText = query;
                connection.Open();
                Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    return_data = Reader[row].ToString();
                }
                connection.Close();
                return return_data;
            }
            catch (Exception e)
            {
                try
                {
                    connection.Close();
                }
                catch { }
                return_data = "ERROR";
                return return_data;
            }
        }

	  //go through and deactive old keys     
        public bool mysql_clear_old_key(string passed_username, string passed_password, string passed_db)
        {
            bool status = false;
            string MyConString = "SERVER=localhost;" +
                    "DATABASE=" + passed_db + ";" +
                    "UID=" + passed_username + ";" +
                    "PASSWORD=" + passed_password + ";";
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = new MySqlCommand("Update `keys` SET `keys`.`active`=0");
            
            try
            {
                connection.Open();
                command.Connection = connection;
                command.ExecuteNonQuery();
                status = true;
            }
            catch
            {
                status = false;
            }
            finally
            {
                connection.Close();
            }
            return status;   
        }
		
		//get the databases
		public List<string> mysql_select_database(string passed_username, string passed_password)
        {
			List<string> temp_databases = new List<string>();
			
            string MyConString = "SERVER=localhost;" +
                    "UID=" + passed_username + ";" +
                    "PASSWORD=" + passed_password + ";";
            MySqlConnection connection = new MySqlConnection(MyConString);

            try
            {
                MySqlCommand command = connection.CreateCommand();
                MySqlDataReader Reader;
                command.CommandText = "show databases;";
                connection.Open();
                Reader = command.ExecuteReader();
                while (Reader.Read())
                {
					temp_databases.Add(Reader["Database"].ToString());
                }
                connection.Close();
                return temp_databases;
            }
            catch (Exception e)
            {
                try
                {
                    connection.Close();
                }
                catch { }
				return temp_databases;
            }
			
        }
		
		//select tables
		public List<string> mysql_select_table(string passed_username, string passed_password, string passed_db)
        {
			List<string> temp_databases = new List<string>();
            string MyConString = "SERVER=localhost;" +
                    "DATABASE=" + passed_db + ";" +
                    "UID=" + passed_username + ";" +
                    "PASSWORD=" + passed_password + ";";
            MySqlConnection connection = new MySqlConnection(MyConString);

            try
            {
                MySqlCommand command = connection.CreateCommand();
                MySqlDataReader Reader;
                command.CommandText = "show tables in " + passed_db + ";";
                connection.Open();
                Reader = command.ExecuteReader();
				
                while (Reader.Read())
                {
					temp_databases.Add(Reader["Tables_in_" + passed_db ].ToString());
                }
                connection.Close();
                return temp_databases;
            }
            catch (Exception e)
            {
                try
                {
                    connection.Close();
                }
                catch { }
				return temp_databases;
            }
        }
		
		//query and get result
		public List< List<string> > mysql_query(string passed_username, string passed_password, string passed_db, string passed_command)
		{
            bool status = false;
			List< List<string> > return_data = new List<List<string>>();
            if (passed_command != "")
            {
                string MyConString = "SERVER=localhost;" +
                    "DATABASE=" + passed_db + ";" +
                    "UID=" + passed_username + ";" +
                    "PASSWORD=" + passed_password + ";";
                MySqlConnection connection = new MySqlConnection(MyConString);
                MySqlCommand command = new MySqlCommand(passed_command);
				MySqlDataReader Reader;
                try
                {
                    connection.Open();
                    command.Connection = connection;
                    Reader = command.ExecuteReader();
					
					while (Reader.Read())
					{
						List<string> temp_list = new List<string>();
						for (int i= 0; i<Reader.FieldCount; i++)
						{
							temp_list.Add(Reader.GetValue(i).ToString());
						}
						return_data.Add(temp_list);
					}
                    status = true;
                }
                catch
                {
                    status = false;
                }
                finally
                {
                    connection.Close();
                }
            }
            return return_data;
			
		}

	}
}

