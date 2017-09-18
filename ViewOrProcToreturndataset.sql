Create table #tmpAdRecord
(
 ID Int,
 LoginName varchar(100),
 LastName varchar(50),
 FirstName varchar(50),
 DisplayName varchar(150),
 sDescription varchar(155),
 Jobtitle varchar(50),
 Department varchar(50),
 Company varchar(50),
 Email varchar(255),
 DriveLetter varchar(255),
 FolderPath varchar(255),
 FolderPathCFormat varchar(255),
 LoginScript varchar(50),
 MembershipGroups varchar(1000),
 LoginPassword  varchar(20),
 YearLevel int,
 title varchar(10),
 BaseYear varchar(5)
)


select '544' UserAccessControl, 
	case when Department='Student' then 
		'OU='+baseYear+',OU=Students,DC=YourDC,DC=LOCAL' 
	when Department='Department Name' then
	'OU=YourOU1,OU=YourOU2,DC=YourDC,DC=LOCAL'
	else 
		'OU=testOU,DC=YourDC,DC=LOCAL' 
	end 
	as
	ldapPath


, *,'@YourMaildomain.com' as UNPDomainName,1 as pwdLastSet from  #tmpAdRecord 

drop table #tmpAdRecord
