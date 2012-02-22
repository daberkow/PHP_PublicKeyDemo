using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

/* Dan Berkowitz
 * Buildingtents.com
 * github.com/daberkow
 */

namespace PHP_PublicPrivateKey_Demo
{
	class MainClass
	{
		//Default options
		public static string my_username = "root";
		public static string my_password = "";
		public static string my_database = "SSL_TEST";
		public static string my_db_table = "keys";
		public static string my_db_data  = "data";
		
		public static void Main (string[] args)
		{
			//scan for databases and try to make, if they cant either user said no or cant get into database
			if (!scan_for_db())
			{
				Console.Clear();
				Console.Write("Cant write table in database" + Environment.NewLine);
			}else{
				Console.Clear();
				Console.Write ("Table available!" + Environment.NewLine);
			}// stopped here need to actually make the tables
			
			//Options that will never go away
			while (true)
			{
				//Enviroment new line is used so this works on linux and windows
				Console.Write(Environment.NewLine + "Please select a option from the menu below: " + Environment.NewLine + Environment.NewLine);
				Console.Write("1. Create New Key" + Environment.NewLine);
				Console.Write("2. Show Keys Table" + Environment.NewLine);
				Console.Write("3. Show Data Table Encrypted" + Environment.NewLine);
				Console.Write("4. Show Data Table Unencrypted" + Environment.NewLine + Environment.NewLine);
				Console.Write("Default Database Options" + Environment.NewLine);
				Console.Write("\t User:     " + my_username + Environment.NewLine);
				Console.Write("\t Password: " + my_password + Environment.NewLine);
				Console.Write("\t Database: " + my_database + Environment.NewLine);
				Console.Write("" + Environment.NewLine);
				Console.Write("Option: ");

				switch(Console.ReadKey().KeyChar)
				{
				case '1':
					option1_create_key(); 	
					break;
				case '2':
					option2_get_key_table();
					break;
				case '3':
					option3_show_encrypted();
					break;
				case '4':
					option4_show_unencrypted();
					break;
				}
			}
		}
		
		//setup up database, make sure we can hit it or make the tables, 90% sure this will work
		private static bool scan_for_db()
		{
			MySQL_Interface MySQL = new MySQL_Interface(); //this is my layer over mysql connector
			List<string> databases = MySQL.mysql_select_database(my_username,my_password);
			bool found_keys = false;// looking for different tables
			bool found_data = false;
			foreach(string db in databases)
			{
				if (db == my_database)
				{
					//found database
					List<string> tables = MySQL.mysql_select_table(my_username,my_password,my_database);
					foreach (string table in tables)
					{
						if (table == "keys")
						{
							found_keys = true;
						}else{
							if (table == "data")
							{
								found_data = true;
							}
						}
					}
				}
			}
			if (found_data && found_keys) // if both tables are there just return true
			{
				return true;
			}
			
			//Tables Need made
			Console.Write("Database was found? Can I make a 'keys' and 'data' table in " + my_database + "?[y/n] "); //asking is always nice
			if (Console.ReadKey().KeyChar == 'y')
			{
				//make tables
				if (!found_keys)
				{
					string command_text = "CREATE TABLE  `" + my_database + "`.`" + my_db_table + "` ( ";
						command_text += @"	`index` INT( 10 ) NOT NULL AUTO_INCREMENT ,
											 `modulus` VARCHAR( 256 ) NOT NULL ,
											 `exponent` INT( 6 ) NOT NULL ,
											 `active` TINYINT( 1 ) NOT NULL ,
											 `date_added` INT( 10 ) NOT NULL ,
											 `revoked` INT( 2 ) NOT NULL ,
											PRIMARY KEY (  `index` )
											) ENGINE = INNODB DEFAULT CHARSET = latin1;";
					MySQL.mysql_nonquery(my_username, my_password, my_database, command_text);
					
					command_text = @"SELECT COUNT(`index`) AS items FROM `" + my_db_table + "`";
					
					List<List<string>> return_stuff = MySQL.mysql_query(my_username,my_password,my_database,command_text);
					
					if (return_stuff.Count > 0)
					{
						found_keys = true;
					}
				}
				
				if (!found_data)
				{
					string command_text = "CREATE TABLE  `" + my_database + "`.`" + my_db_data + "` ( ";
						command_text += @"`index` INT NOT NULL AUTO_INCREMENT ,
											`data` TEXT NOT NULL ,
											`key_used` INT NOT NULL ,
											PRIMARY KEY (  `index` )
											) ENGINE = MYISAM ;";
					MySQL.mysql_nonquery(my_username, my_password, my_database, command_text);
					
					command_text = @"SELECT COUNT(`index`) AS items FROM `" + my_db_data + "`";
					
					List<List<string>> return_stuff = MySQL.mysql_query(my_username,my_password,my_database,command_text);
					
					if (return_stuff.Count > 0)
					{
						found_data = true;
					}
				}
				
				if(found_keys && found_data)
				{
					//Foudn everything good to go
					return true;
				}else{
					return false;
				}
			}
			return false; //this shouldnt be hit, compiler complained
		}
		
		//this will add new keys, then deactive the others
		private static void option1_create_key()
		{
			MySQL_Interface MySQL = new MySQL_Interface();

            string Errors = "";
            //System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
            System.Console.Write("Creating Keys..." + Environment.NewLine);
            System.Security.Cryptography.RSACryptoServiceProvider provider = new System.Security.Cryptography.RSACryptoServiceProvider(); //make keys in c#
            System.Console.Write("Keys Created." + Environment.NewLine);

            System.Console.WriteLine("Connecting to database and deactivating past keys...");
            if (MySQL.mysql_clear_old_key(my_username,my_password,my_database))
            {
                Console.WriteLine("Old Keys Deactivated");
            }
            else
            {
                Errors += "Error disabling old keys" + "|" + "Run command 'database-maintance'" + "|";
                Console.WriteLine("Error disabling old keys");
                Console.WriteLine("Manually deactivte old keys or empty table, im not a english major spelling is for computers, and awake people");
            }

            System.Console.WriteLine("Inserting new key, and activating...");
            int time = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds); // get unix time stamp for easy use
            string mod = BitConverter.ToString(provider.ExportParameters(false).Modulus).Replace("-", string.Empty).ToLower(); //public key parts of ssl
            string expon = BitConverter.ToString(provider.ExportParameters(false).Exponent).Replace("-", string.Empty);
			
			//Public key parts get put in database to be used in web end, the rest of the key is saved to the hard drive so that just a local app can decrypt
			
            string mysql_command = "INSERT INTO `" + my_database + "`.`" + my_db_table +  "` (`index` ,`modulus` ,`exponent`,`active`,`date_added`) VALUES (NULL , '" + mod + "', '" + expon + "',  '1',  '" + time + "')";
            if (MySQL.mysql_nonquery(my_username, my_password, my_database, mysql_command))
            {
                //Save local key pairs
                string index_of = MySQL.mysql_select_cell(my_username, my_password, my_database, "SELECT * FROM `" + my_database + "`.`" + my_db_table +  "` WHERE `modulus`='" + mod + "' AND `date_added`='" + time + "'", "index");
                System.IO.DirectoryInfo Working_dir = new System.IO.DirectoryInfo(System.Environment.CurrentDirectory);
				string working_dir = Working_dir.ToString();
				local_store_key(working_dir, index_of, provider);
            }
            else
            {
                Console.Write("Error putting key in database" + Environment.NewLine);
            }
				
		}
		
		//show the key table for funzies
		private static void option2_get_key_table()
		{
			MySQL_Interface MySQL = new MySQL_Interface();
			
			string command_text = @"SELECT * FROM `" + my_database + "`.`" + my_db_table + "`";
					
			List<List<string>> return_stuff = MySQL.mysql_query(my_username,my_password,my_database,command_text);
			
			if (return_stuff.Count > 0)
			{
				Console.Write(Environment.NewLine +  "||IND||   Modulus          || Exponent || Active || Revoked ||" + Environment.NewLine);
				for (int i = 0; i < return_stuff.Count; i++)
				{
					Console.Write("|| " + return_stuff[i][0] + " || "  + return_stuff[i][1].Substring(0, 34) + " ||     "  + return_stuff[i][2] + " || "  + return_stuff[i][3] + " || " + return_stuff[i][4] + " || " + Environment.NewLine);
				}
			}
		}
		
		//show all the strings encrypted, for fun
		private static void option3_show_encrypted()
		{
			MySQL_Interface MySQL = new MySQL_Interface();
			
			string command_text = @"SELECT * FROM `" + my_database + "`.`" + my_db_data + "`";
					
			List<List<string>> return_stuff = MySQL.mysql_query(my_username,my_password,my_database,command_text);
			
			if (return_stuff.Count > 0)
			{
				Console.Write(Environment.NewLine +  "||IND||   data          || key used " + Environment.NewLine);
				for (int i = 0; i < return_stuff.Count; i++)
				{
					Console.Write("|| " + return_stuff[i][0] + " || "  + return_stuff[i][1] + " ||     "  + return_stuff[i][2] + Environment.NewLine);
				}
			}
		}
		
		//show all the string unencrypted
		private static void option4_show_unencrypted()
		{
			MySQL_Interface MySQL = new MySQL_Interface();
			
			string command_text = @"SELECT * FROM `" + my_database + "`.`" + my_db_data + "`";
					
			List<List<string>> return_stuff = MySQL.mysql_query(my_username,my_password,my_database,command_text);
			
			if (return_stuff.Count > 0)
			{
				Console.Write(Environment.NewLine +  "||IND||   data          || key used " + Environment.NewLine);
				for (int i = 0; i < return_stuff.Count; i++)
				{
					Console.Write("|| " + return_stuff[i][0] + " || ");
					Console.Write(decrypt ((new System.IO.DirectoryInfo(System.Environment.CurrentDirectory)).ToString(), int.Parse(return_stuff[i][2]), return_stuff[i][1]));
					Console.Write (" ||     "  + return_stuff[i][2] + Environment.NewLine);
				}
			}
		}
		
		//this will write the keys to the hard drive
		private static string local_store_key(string passed_data_store, string keyID, RSACryptoServiceProvider passed_provider)
        {
            System.IO.DirectoryInfo Working_dir = new System.IO.DirectoryInfo(passed_data_store);
            System.IO.DirectoryInfo Keys_Dir = new System.IO.DirectoryInfo(System.IO.Path.Combine(Working_dir.ToString(), "keys"));
            if (Working_dir.Exists)
            {
                //Directory is good for storage
                if (!Keys_Dir.Exists)
                {
                    try
                    {
                        Keys_Dir.Create();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        return e.ToString();
                    }
                }
            }
            else
            {
                try
                {
                    Working_dir.Create();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return e.ToString();
                }
                //No keys directory
                try
                {
                    Keys_Dir.Create();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return e.ToString();
                }
            }
            Working_dir = null;
            //Keys Dir should work now
            System.IO.FileInfo Key_file = new System.IO.FileInfo(Keys_Dir.ToString() + System.IO.Path.DirectorySeparatorChar + keyID + ".key");
            using (System.IO.StreamWriter sw = Key_file.CreateText())
            {
                sw.Write(BitConverter.ToString(passed_provider.ExportCspBlob(true)));
                Console.WriteLine("Key Stored in 'keys'");
                //Console.WriteLine(BitConverter.ToString(passed_provider.ExportCspBlob(true)));
                //Console.WriteLine();
                //Console.WriteLine(BitConverter.ToString(passed_provider.ExportCspBlob(true)).Replace("-", string.Empty));
                
            }
            return "yes";
        }
		
		//this actually decrypts
		public  static string decrypt(string passed_data_store, int key_used, string passed_data)
        {
            string return_data = "error";
            System.IO.DirectoryInfo Keys_Dir = new System.IO.DirectoryInfo(System.IO.Path.Combine(passed_data_store, "keys"));
            string blob = "";
            System.Security.Cryptography.RSACryptoServiceProvider provider = new System.Security.Cryptography.RSACryptoServiceProvider();
            
            foreach(System.IO.FileInfo File in Keys_Dir.GetFiles())
            {
                if (File.Name.Split('.')[0] == key_used.ToString())
                {
                    
                    using (System.IO.StreamReader reader = File.OpenText())
                    {
                        blob = reader.ReadToEnd();

                        String[] str_arr = blob.Split('-');
                        byte[] encrypted_array = new byte[str_arr.Length];
                        for(int i = 0; i < str_arr.Length; i++) encrypted_array[i]=Convert.ToByte(str_arr[i], 16);
                        provider.ImportCspBlob(encrypted_array);

                        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

                        byte[] science = StringToByteArray(passed_data);
                        byte[] decrtpyerd_array = provider.Decrypt(science, false);
                        return Encoding.UTF8.GetString(decrtpyerd_array);
                    }
                }
            }
            Console.WriteLine("Can not find decryption key");
            return return_data;
        }
		
		//converting script
		private static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
	}
}
