/*Author: KrishanG
 * Date:04/03/2011
 * Remarks: This is a wrapper around ADS services to create user accounts then create folders on a respective drive and assign the appropriate 
 * permissions.
 */ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Configuration;
using System.Data;
using System.Data.Sql;

using ActiveDirectoryHelper;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using ActiveDs;

namespace ADSUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This program reads synergetic new user information using the proc uspGetNewADInsert and then creates the users in Active Directory - > Creates a user folder under Kazon-> Set the security permission to the folder for the user to full control. For setup information check the App.config file. Author KrishanG ( July 2012 )");
            Console.WriteLine("Connecting to Synergetic Database...");
            Console.WriteLine("DONT FORGET TO RUN THIS AGAIN SINCE THE FIRST TIME THE USER GROUPS MIGHT NOT GET ADDED.");
         //   Console.WriteLine("userIUD:" + CreateUserAccount(spath, "21aRogbertsJ", "password"));
          //ADMethodsAccountManagement ADMethods = new ADMethodsAccountManagement();
          //Console.WriteLine("Add the user to a group called students");
        //  ADMethods.AddUserToGroup("aRobertsJ", "All Students");
       //   ADMethods.AddUserToGroup("aRobertsJ", "All Year 10 Students");
            CreateUsers(ConfigurationManager.AppSettings["LDAPUser"].ToString(),
                        ConfigurationManager.AppSettings["LDAPPassword"].ToString()
                        );

            Console.WriteLine("Process complete. Please check the log file for errors");
          

        }

       private static string CreateUserAccount(string AdAdminUser,
                                               string AdminPassword,
                                               string ldapPath, 
                                               string userName, 
                                               string userPassword,
                                               string givenName,
                                               string sn,
                                               string displayName,
                                               string description,
                                               string mail,
                                               string userAccountControl,
                                               string title,
                                               string Department,
                                               string Company,
                                               string pager,
                                               string scriptPath,
                                               string homeDirectory,
                                               string HomeDrive,
                                               string UPNDomainName,
                                               string pwdLastSet
                                                )
        {
               string oGUID = string.Empty;
           try
            {
             
                string connectionPrefix = "LDAP://" + ldapPath;
                DirectoryEntry dirEntry = new DirectoryEntry(connectionPrefix);
                dirEntry.Username=AdAdminUser;
                dirEntry.Password = AdminPassword;
                                

                DirectoryEntry newUser = dirEntry.Children.Add
                    ("CN=" + displayName, "user");
               //General Tab
                newUser.Properties["givenName"].Value = givenName;
                newUser.Properties["sn"].Value = sn;
                newUser.Properties["displayName"].Value = displayName;
                newUser.Properties["distinguishedName"].Value = displayName;
                // Not sure of the following properties
               // newUser.Properties["name"].Value = displayName;
               // newUser.Properties["cn"].Value = displayName;


                newUser.Properties["description"].Value = description;
                newUser.Properties["mail"].Value = mail;
               //Account tab
                newUser.Properties["userPrincipalName"].Value = userName + UPNDomainName;
                newUser.Properties["samAccountName"].Value = userName;
                newUser.Properties["userAccountControl"].Value = userAccountControl;
                

                
                newUser.Properties["pwdLastSet"].Value = Convert.ToInt32(0);
               
               // Organization
                newUser.Properties["title"].Value =title;
                newUser.Properties["Department"].Value = Department;
                newUser.Properties["Company"].Value = Company;

               //Telephones tab
                newUser.Properties["pager"].Value =pager;
               
               //Profile tab
               if (scriptPath.Length>0)
                newUser.Properties["scriptPath"].Value = scriptPath;
               if (homeDirectory.Length>0)
                    newUser.Properties["homeDirectory"].Value = homeDirectory;
               if (HomeDrive.Length>0)
                    newUser.Properties["homeDrive"].Value = HomeDrive;

              

                newUser.CommitChanges();
                oGUID = newUser.Guid.ToString();

                newUser.Invoke("SetPassword", new object[] { userPassword });
                newUser.CommitChanges();
                dirEntry.Close();
                newUser.Close();
                newUser.Dispose();
                dirEntry.Dispose();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //DoSomethingwith --> E.Message.ToString();
                 Console.WriteLine(E.Message.ToString());
                 WriteLog(E.Message.ToString() + " User " + userName, "Failed");
            }
            return oGUID;
        }


       private static DataTable NewUserRecords()
       {
             
         //new SqlParameter("@EventID", SqlDbType.Int)
        SqlParameter[] parametros = {
            //new SqlParameter("@EventID",SqlDbType.BigInt),
	        };

        //parametros[0].Value = EventID;

           
           
          DataSet ds= DBObjects.RunProcedure(ConfigurationManager.AppSettings["ApplicationServices"].ToString(),"uspGetNewADInsert",parametros ,"tblUsers");
       


          return ds.Tables[0];
       }

       private static void CreateUsers(string AdminUser, string AdminPassword)
       {
           Boolean Isok;

           Console.WriteLine("Get the New users from the database....");
           DataTable dt = NewUserRecords();

           Console.WriteLine("Writing the records into AD now. This might take some time.");
           foreach (DataRow dr in dt.Rows)
           {
               // Add the user to AD
               string sGUID = CreateUserAccount(AdminUser, AdminPassword,
                           dr["ldapPath"].ToString(),
                           dr["LoginName"].ToString(),
                           dr["LoginPassword"].ToString(),
                           dr["FirstName"].ToString(),
                           dr["LastName"].ToString(),
                           dr["DisplayName"].ToString(),
                           dr["sdescription"].ToString(),
                           dr["email"].ToString(),
                           dr["UserAccessControl"].ToString(),
                           dr["Jobtitle"].ToString(),
                           dr["Department"].ToString(),
                           dr["Company"].ToString(),
                           dr["ID"].ToString(),
                           dr["LoginScript"].ToString(),
                           dr["FolderPath"].ToString(),
                           dr["DriveLetter"].ToString(),
                           dr["UNPDomainName"].ToString(),
                           dr["pwdLastSet"].ToString()
                           );

               // If the user creation was successful then add the user to the relavent membership groups
               if (sGUID == "")
               {
                   Console.WriteLine("user creation Error. Check the log file");
                   WriteLog("User creation failed: " + dr["ID"].ToString() + " " + dr["displayName"].ToString(), "Failed");
               }
               else
               {
                   WriteLog("Created User: " + dr["ID"].ToString() + " " + dr["displayName"].ToString(), "Success!");
               }

               Console.WriteLine("Adding user " + dr["LoginName"].ToString() + " to the following groups  {0}...", dr["MembershipGroups"].ToString());

               ADMethodsAccountManagement ADMethods = new ADMethodsAccountManagement();
               string[] gMem = dr["MembershipGroups"].ToString().Split(',');
               for (int i = 0; i < gMem.Count(); i++)
               {

                   Isok = ADMethods.AddUserToGroup(dr["LoginName"].ToString(), gMem[i]);
                   if (Isok)
                       WriteLog("Added user " + dr["displayName"].ToString() + "(" + dr["LoginName"].ToString() + ") to groups " + dr["MembershipGroups"].ToString(), "Success!");
                   else
                       WriteLog("Could not add user " + dr["displayName"].ToString() + "(" + dr["LoginName"].ToString() + ") to groups " + dr["MembershipGroups"].ToString(), "Failed");
               }


               // Now Create the user folder hope all goes well here.
               if (dr["DriveLetter"].ToString().Length > 0 && dr["FolderPathCFormat"].ToString().Length>0)
               { 
                       if (!CreateHomeFolder(dr["LoginName"].ToString(), dr["DriveLetter"].ToString(), dr["FolderPathCFormat"].ToString()))
                       {
                           Console.WriteLine("Could not create folder for user. " + dr["id"].ToString() + " " + dr["displayName"].ToString());
                           WriteLog("Could not Create Folder for user " + dr["id"].ToString() + " " + dr["displayName"].ToString(), "Failed");
                       }
                       else
                           WriteLog("Created Folder for user " + dr["id"].ToString() + " " + dr["displayName"].ToString(), "Success!");
               }
               else
                   WriteLog("No drive folder speficied for user " + dr["id"].ToString() + " " + dr["displayName"].ToString(), "No Action!");
           }
       }


 

       private static Boolean CreateHomeFolder(string LoginName, string driverLetter, string path)
       {
           // string directoryName = Path.GetDirectoryName(path);  
           string directoryName = path;
           try
           {
               if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
               {
                   DirectoryInfo di = Directory.CreateDirectory(directoryName);
                   FolderACL(LoginName, path);
                   return true;
               }
               else
               {
                   FolderACL(LoginName, path);
                   return false;
               }
               //localfolderpath = "C:\\Home\\" + homeFolder + "\\" + accountName; shareName = accountName + "$"; 
             

               //makeShare(localfolderpath, shareName); 
        


               
           }
           catch (Exception e)
           {
               Console.WriteLine("The process failed: {0}", e.ToString());
               WriteLog(e.ToString(), "Failed");
               return false;
           } 


           
       }

        public static void FolderACL(String accountName, String folderPath)     
        {          
            FileSystemRights Rights; 

            //What rights are we setting?
            Rights = FileSystemRights.FullControl;
            
            bool modified;
            InheritanceFlags none = new InheritanceFlags();

            none = InheritanceFlags.None;
            
            //set on dir itself   
            FileSystemAccessRule accessRule = new FileSystemAccessRule(accountName, Rights, none, PropagationFlags.NoPropagateInherit, AccessControlType.Allow); 
            DirectoryInfo dInfo = new DirectoryInfo(folderPath); 
            DirectorySecurity dSecurity = dInfo.GetAccessControl(); 
            dSecurity.ModifyAccessRule(AccessControlModification.Set, accessRule, out modified);

            //Always allow objects to inherit on a directory
            InheritanceFlags iFlags = new InheritanceFlags(); 
            iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;     
            
            //Add Access rule for the inheritance  
            FileSystemAccessRule accessRule2 = new FileSystemAccessRule(accountName, Rights, iFlags, PropagationFlags.InheritOnly, AccessControlType.Allow);
            dSecurity.ModifyAccessRule(AccessControlModification.Add, accessRule2, out modified);    
            dInfo.SetAccessControl(dSecurity);  
        }
        
       private static void makeShare(string filepath, string sharename)     
       {
           try
           {
               String servername = "server";
               //// assemble the string so the scope represents the remote server
               //string scope = string.Format("\\\\{0}\\root\\cimv2", servername);
               //// connect to WMI on the remote server
               //ManagementScope ms = new ManagementScope(scope);
               //// create a new instance of the Win32_Share WMI object
               //ManagementClass cls = new ManagementClass("Win32_Share");
               //// set the scope of the new instance to that created above 
               //cls.Scope = ms; 
               //// assemble the arguments to be passed to the Create method
               //object[] methodargs = { filepath, sharename, "0" };
               //// invoke the Create method to create the share
               //object result = cls.InvokeMethod("Create", methodargs);
               //MessageBox.Show(result.ToString()); 
           }
           catch (SystemException e)
           { 
               Console.WriteLine("Error attempting to create share {0}:", sharename);
               Console.WriteLine(e.Message);
           }
       } 


       private static void WriteLog(string Message,string State)
        {
            // Code to write the details to a log file.
            //new SqlParameter("@EventID", SqlDbType.Int)
            SqlParameter[] parametros = {
             new SqlParameter("@Message",SqlDbType.VarChar),
             new SqlParameter("@Status",SqlDbType.VarChar,10),
	        };

            parametros[0].Value = Message;
            parametros[1].Value = State;


            try
            {

                DataSet ds = DBObjects.RunProcedure(ConfigurationManager.AppSettings["ApplicationServices"].ToString(), "uspWriteADError", parametros, "tblUsers");
            }
            catch(Exception e)
            {
                WriteEventToWindowsLog("JPCAdUserService", e.Message.ToString());
   
            }


            


        }


       public static void WriteEventToWindowsLog(string strMyApp, string strEvent)
       {
           if (!System.Diagnostics.EventLog.SourceExists(strMyApp))
               System.Diagnostics.EventLog.CreateEventSource(strMyApp, "Application");

           EventLog MyEventLog = new EventLog();
           MyEventLog.Source = strMyApp;
           MyEventLog.WriteEntry(strEvent, EventLogEntryType.Warning);
       }
    }
}
