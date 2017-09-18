# ActiveDirectoryMgr
Service to create active directory accounts based on an available dataset provided from a MS SQL server or other database.

This little program creates active directory users by reading a data table provided from a sql server database. To get it to
work create a user account on your sql server. Then create a relavent view or a stored proc which will return a result set as
per the SQL script provided. See SQL folder. 
Then rename the App - Copy.config to app.config and enter the AD user details which will have permissions to create AD users
and groups.  Enter the SQL server connection details.  Compile and run the app.
